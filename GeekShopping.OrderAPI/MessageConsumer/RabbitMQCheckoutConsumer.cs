using System.Text;
using System.Text.Json;
using GeekShopping.OrderAPI.Messages;
using GeekShopping.OrderAPI.Model;
using GeekShopping.OrderAPI.RabbitMQSender;
using GeekShopping.OrderAPI.Repository;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace GeekShopping.OrderAPI.MessageConsumer
{
    public class RabbitMQCheckoutConsumer : BackgroundService
    {
        private readonly OrderRepository _repository;
        private IConnection _connection;
        private IModel _channel;
        private IRabbitMQMessageSender _rabbitMQMessageSender;

        public RabbitMQCheckoutConsumer(OrderRepository repository,
        IRabbitMQMessageSender rabbitMQMessageSender)
        {
            _repository = repository;
            _rabbitMQMessageSender = rabbitMQMessageSender;
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "checkoutqueue", false, false, false, arguments: null);
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (chanel, evt) => 
            {
                var content = Encoding.UTF8.GetString(evt.Body.ToArray());
                CheckoutHeaderVO vo = JsonSerializer.Deserialize<CheckoutHeaderVO>(content);
                ProcessOrder(vo).GetAwaiter().GetResult();
                _channel.BasicAck(evt.DeliveryTag,false);
            };
            _channel.BasicConsume("checkoutqueue", false, consumer);
            return Task.CompletedTask;
        }

        private async Task ProcessOrder(CheckoutHeaderVO vo)
        {
            OrderHeader order = new()
            {
                UserId = vo.UserId,
                CouponCode = vo.CouponCode,
                PurchaseAmount = vo.PurchaseAmount,
                DiscountTotal = vo.DiscountTotal,
                FirstName = vo.FirstName,
                LastName = vo.LastName,
                DateTime = vo.DateTime,
                OrderTime = DateTime.Now,
                Phone = vo.Phone,
                Email = vo.Email,
                CardNumber = vo.CardNumber,
                CVV = vo.CVV,
                ExpiryMonthYear = vo.ExpiryMonthYear,
                PaymentStatus = false,
                OrderDetails = new List<OrderDetail>(),
            };

            foreach (var orderDetail in vo.CartDetails)
            {
                OrderDetail detail = new()
                {
                    ProductId = orderDetail.ProductId,
                    ProductName = orderDetail.Product.Name,
                    Price = orderDetail.Product.Price,
                    Count = orderDetail.Count,

                };
                order.OrderTotal += detail.Count;
                order.OrderDetails.Add(detail);
            }
            await _repository.AddOrder(order);

            PaymentVO payment = new()
            {
                Name = order.FirstName + " " + order.LastName,
                CardNumber = order.CardNumber,
                CVV = order.CVV,
                ExpiryMonthYear = order.ExpiryMonthYear,
                OrderId = order.Id,
                PurchaseAmount = order.PurchaseAmount,
                Email = order.Email,
                MessageCreated = DateTime.Now

            };

            try
            {
                _rabbitMQMessageSender.SendMessage(payment, "orderpaymentprocessqueue");
            }
            catch (System.Exception)
            {
                //log
                throw;
            }
        }
    }
}