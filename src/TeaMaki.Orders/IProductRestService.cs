using Refit;
using System.Threading.Tasks;
using TeaMaki.Orders.Services;

namespace TeaMaki.Orders
{
    public interface IProductRestService
    {
        [Get("/product/?productId={ProductId}")]
        public Task<ProductToGet> GetAsync(string ProductId);
    }
}