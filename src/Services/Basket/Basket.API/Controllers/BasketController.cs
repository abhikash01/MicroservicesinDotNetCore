using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace Basket.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

		public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService, IMapper mapper, IPublishEndpoint publishEndpoint)
		{
			_repository = repository ?? throw new ArgumentNullException(nameof(repository));
			_discountGrpcService = discountGrpcService ?? throw new ArgumentNullException(nameof(discountGrpcService));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		[HttpGet("{userName}", Name ="GetBasket")]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>>GetBasket(string userName)
        {
             var basket = await _repository.GetBasket(userName);
             return Ok(basket ?? new ShoppingCart(userName));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody]ShoppingCart basket)
        {

            // To Do : Communicate with Discount GRPC
            // and calculate latest prices of Products into shopping Cart
            foreach(var item in basket.Items)
            {
                #pragma warning disable CS8604 // Possible null reference argument.
                var coupon = await _discountGrpcService.GetDiscount(item.ProductName);
                #pragma warning restore CS8604 // Possible null reference argument.
                item.Price -= coupon.Amount;
            }
            return Ok(await  _repository.UpdateBasket(basket));
        }

        [HttpDelete("{userName}" , Name="DeleteBasket") ]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteBasket(string userName)
        {
            await _repository.DeleteBasket(userName);
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Accepted)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] BasketCheckout basketCheckout)
        {
			// Get exisiting Basket with Total Price
			// Create basketCheckout event -- Set TotalPrice on basketCheckout eventMessage
			// Send checkout event to RabbitMq
			// Remove the Basket

			// Get exisiting Basket with Total Price
			var basket = await _repository.GetBasket(basketCheckout.UserName);
            if (basket == null)
                return BadRequest();

            // Create basketCheckout event -- Set TotalPrice on basketCheckout eventMessage


            // Send checkout event to RabbitMq
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = basket.TotalPrice;
            await _publishEndpoint.Publish(eventMessage);
			// Remove the Basket
			await _repository.DeleteBasket(basketCheckout.UserName);
            return Accepted();
		}


    }
}
