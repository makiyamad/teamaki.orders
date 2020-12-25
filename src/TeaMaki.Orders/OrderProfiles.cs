using AutoMapper;
using TeaMaki.Orders.Controllers;

namespace TeaMaki.Orders
{
    public class OrderProfiles : Profile
    {
        public OrderProfiles()
        {
            CreateMap<OrderToPut, Order>();
            CreateMap<OrderItemToPut, OrderItem>();
            CreateMap<Order, OrderToGet>();
            CreateMap<OrderItem, OrderItemToGet>();
        }
    }
}
