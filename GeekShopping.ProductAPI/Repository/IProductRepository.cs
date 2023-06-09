using GeekShopping.ProductAPI.Data.ValeuObjects;

namespace GeekShopping.ProductAPI.Repository
{
    public interface IProductRepository
    {
         Task<IEnumerable<ProductVO>> FindAll();
         Task<ProductVO> FindById(long Id);
         Task<ProductVO> Create(ProductVO vo);
         Task<ProductVO> Update(ProductVO vo);
        Task<bool> Delete(long Id);

    }
}