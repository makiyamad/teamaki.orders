using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TeaMaki.Orders.Services
{
    public class ProductToGet
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Tax { get; set; }
    }
}