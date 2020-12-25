using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TeaMaki.Persistence;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using TeaMaki.Orders.Services;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace TeaMaki.Orders.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ILogger<OrderController> _logger;
        private readonly IRepository<Order> _repository;
        private readonly IMapper _mapper;
        private readonly IProductRestService _productRestService;

        public OrderController(ILogger<OrderController> logger,
        IRepository<Order> repository, IMapper mapper, IProductRestService productRestService)
        {
            _logger = logger;
            _repository = repository;
            _mapper = mapper;
            _productRestService = productRestService;
        }

        [HttpPut]
        public IActionResult AddOrUpdate(OrderToPut orderToPut)
        {
            if (!ModelState.IsValid) return BadRequest();

            var order = _mapper.Map<Order>(orderToPut);

            foreach(var item in order.OrderItems)
            {
                var product = _productRestService.GetAsync(item.ProductId).Result;
                item.UnitPrice = product.Price;
                item.UnitTax = product.Tax;
            }

            try
            {
                _repository.InsertOrUpdate(order);
            }
            catch (RepositoryException<Order> exception)
            {
                _logger.Log(Microsoft.Extensions.Logging.LogLevel.Error, exception, exception.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return NoContent();
        }

        [HttpGet]
        public IActionResult Get([FromQuery] Guid orderId)
        {
            var order = _repository.Get(orderId.ToString());

            if (order == null) return NotFound();

            var orderToGet = _mapper.Map<OrderToGet>(order);

            return Ok(orderToGet);
        }
    }

    public class Order
    {
        public string Id { get; protected set; }
        public OrderItem[] OrderItems { get; protected set; }
        public string CustomerId { get; protected set; }

        public Order(string orderId, string customerId)
        {
            CustomerId = customerId;
            Id = orderId ?? string.Empty;
        }
    }

    public class OrderItem
    {
        public string Id { get; protected set; }
        public int Quantity { get; protected set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitTax { get; set; }
        public string ProductId { get; protected set; }

        public OrderItem(string orderItemId, string productId, int quantity)
        {
            ProductId = productId;
            Id = orderItemId ?? Guid.NewGuid().ToString();
            Quantity = quantity;
        }
    }

    public class OrderToGet
    {
        public Guid OrderId { get; set; }
        public OrderItemToGet[] OrderItems { get; set; }
        public Guid CustomerId { get; set; }
    }

    public class OrderItemToGet
    {
        public Guid OrderItemId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal UnitTax { get; set; }
    }

    public class OrderToPut
    {
        public string OrderId { get; set; }
        public OrderItemToPut[] OrderItems { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
    }

    public class OrderItemToPut
    {
        public string OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string ProductId { get; set; }
    }
}
