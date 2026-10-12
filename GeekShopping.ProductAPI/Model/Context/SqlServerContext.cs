using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Model.Context
{
    public class SqlServerContext : DbContext
    {
        public SqlServerContext()
        {

        }
        public SqlServerContext(DbContextOptions<SqlServerContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 3,
                Name = "Camiseta No Internet",
                Price = new decimal(69.9),
                Description = "Black T-shirt with a 'No Internet' print in the style of a classic dinosaur park logo. For anyone who has stared at an offline browser tab waiting for the connection to come back.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/2_no_internet.jpg?raw=true",
                CategoryName = "T-shirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 4,
                Name = "Capacete Darth Vader Star Wars Black Series",
                Price = new decimal(999.99),
                Description = "Full-size Darth Vader helmet from the Star Wars Black Series collection, made in three pieces that fit together like the one on screen. A display piece for collectors, and wearable too.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/3_vader.jpg?raw=true",
                CategoryName = "Action Figure"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 5,
                Name = "Star Wars The Black Series Hasbro - Stormtrooper Imperial",
                Price = new decimal(189.99),
                Description = "Imperial Stormtrooper action figure from the Star Wars Black Series line by Hasbro, with detailed armor, several points of articulation and a blaster accessory.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/4_storm_tropper.jpg?raw=true",
                CategoryName = "Action Figure"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 6,
                Name = "Camiseta Gamer",
                Price = new decimal(69.99),
                Description = "Black T-shirt with a '100% Gamer' print surrounded by controller, headset and console icons. A comfortable everyday shirt for long gaming sessions.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/5_100_gamer.jpg?raw=true",
                CategoryName = "T-shirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 7,
                Name = "Camiseta SpaceX",
                Price = new decimal(49.99),
                Description = "Raglan T-shirt with the SpaceX logo across the chest, white body and dark sleeves. For fans of rockets, launches and landings.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/6_spacex.jpg?raw=true",
                CategoryName = "T-shirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 8,
                Name = "Camiseta Feminina Coffee Benefits",
                Price = new decimal(69.9),
                Description = "Women's T-shirt in a light sand color with a vintage 'Benefits of Drinking Coffee' illustration. For people whose day only starts after the first cup.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/7_coffee.jpg?raw=true",
                CategoryName = "T-shirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 9,
                Name = "Moletom Com Capuz Cobra Kai",
                Price = new decimal(159.9),
                Description = "Cobra Kai hooded sweatshirt with the dojo's snake logo on the front, front pocket and adjustable hood. Strike first, strike hard, no mercy.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/8_moletom_cobra_kay.jpg?raw=true",
                CategoryName = "Sweatshirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 10,
                Name = "Livro Star Talk – Neil DeGrasse Tyson",
                Price = new decimal(49.9),
                Description = "StarTalk, by Neil deGrasse Tyson: the book version of the science show, with answers about space travel, science fiction and the universe, explained with humor.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/9_neil.jpg?raw=true",
                CategoryName = "Book"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 11,
                Name = "Star Wars Mission Fleet Han Solo Nave Milennium Falcon",
                Price = new decimal(359.99),
                Description = "Star Wars Mission Fleet set with a Han Solo figure and a Millennium Falcon he fits inside, plus projectile launchers. Sized for play, for ages 4 and up.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/10_milennium_falcon.jpg?raw=true",
                CategoryName = "Action Figure"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 12,
                Name = "Camiseta Elon Musk Spacex Marte Occupy Mars",
                Price = new decimal(59.99),
                Description = "T-shirt with an 'Occupy Mars' print over the red planet, inspired by SpaceX and the plan to take people there.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/11_mars.jpg?raw=true",
                CategoryName = "T-shirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 13,
                Name = "Camiseta GNU Linux Programador Masculina",
                Price = new decimal(59.99),
                Description = "Men's T-shirt for programmers with a GNU/Linux print. For people who live in the terminal and choose their distro with care.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/12_gnu_linux.jpg?raw=true",
                CategoryName = "T-shirt"
            });
            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = 14,
                Name = "Camiseta Goku Fases",
                Price = new decimal(59.99),
                Description = "T-shirt showing Goku through his transformations in Dragon Ball, from his base form to Super Saiyan.",
                ImageURL = "https://github.com/leandrocgsi/erudio-microservices-dotnet6/blob/main/ShoppingImages/13_dragon_ball.jpg?raw=true",
                CategoryName = "T-shirt"
            });
        }
    }
}