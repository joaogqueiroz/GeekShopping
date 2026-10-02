namespace GeekShopping.Tests
{
    // A missing or misspelled property between an entity and its VO makes AutoMapper silently drop data.
    // AssertConfigurationIsValid fails on any destination member that nothing maps to.
    public class MappingConfigTests
    {
        [Fact]
        public void CartApi_MappingIsValid() =>
            GeekShopping.CartAPI.Config.MappingConfig.RegisterMaps().AssertConfigurationIsValid();

        [Fact]
        public void ProductApi_MappingIsValid() =>
            GeekShopping.ProductAPI.Config.MappingConfig.RegisterMaps().AssertConfigurationIsValid();

        [Fact]
        public void CouponApi_MappingIsValid() =>
            GeekShopping.CouponAPI.Config.MappingConfig.RegisterMaps().AssertConfigurationIsValid();
    }
}
