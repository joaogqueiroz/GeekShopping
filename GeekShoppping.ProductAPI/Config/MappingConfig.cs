using AutoMapper;
using GeekShoppping.ProductAPI.Data.ValeuObjects;
using GeekShoppping.ProductAPI.Model;

namespace GeekShoppping.ProductAPI.Config
{
    public class MappingConfig
    {
        public static MapperConfiguration RegisterMaps()
        {
            var mappingConfig =  new MapperConfiguration(config =>
            {
                config.CreateMap<ProductVO, Product>();
                config.CreateMap<Product, ProductVO>();
            });
            return mappingConfig;
        }
    }
}