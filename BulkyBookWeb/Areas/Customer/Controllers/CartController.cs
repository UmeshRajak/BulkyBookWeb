using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Drawing;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [BindProperty]
        public ShoppingCartVm ShoppingCartVm { get; set; }
       
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            ShoppingCartVm = new()
            { 
             ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "productModel"),
                orderHeaderModel = new OrderHeaderModel()
            };

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                cart.Price = getPriceBasedOnQuantity(cart);
                ShoppingCartVm.orderHeaderModel.OrderTotal += cart.Price * cart.Count;
            }

            return View(ShoppingCartVm);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;


            ShoppingCartVm = new()
            {
                ShoppingCartList =
                _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
                includeproperties: "productModel"),
                orderHeaderModel = new OrderHeaderModel()
            };
            
            ShoppingCartVm.orderHeaderModel.applicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);
            ShoppingCartVm.orderHeaderModel.Name = ShoppingCartVm.orderHeaderModel.applicationUser.Name;
            ShoppingCartVm.orderHeaderModel.PhoneNumber = ShoppingCartVm.orderHeaderModel.applicationUser.PhoneNumber;
            ShoppingCartVm.orderHeaderModel.StreetAddress = ShoppingCartVm.orderHeaderModel.applicationUser.StreetAddress;
            ShoppingCartVm.orderHeaderModel.City = ShoppingCartVm.orderHeaderModel.applicationUser.City;
            ShoppingCartVm.orderHeaderModel.State = ShoppingCartVm.orderHeaderModel.applicationUser.State;
            ShoppingCartVm.orderHeaderModel.PostalAddress = ShoppingCartVm.orderHeaderModel.applicationUser.PostalAddress;

                      


            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
                cart.Price = getPriceBasedOnQuantity(cart);
                ShoppingCartVm.orderHeaderModel.OrderTotal += cart.Price * cart.Count;
            }
           

            return View(ShoppingCartVm);
        }
        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPost()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVm.ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
            includeproperties: "productModel");

			ShoppingCartVm.orderHeaderModel.OrderDate = System.DateTime.Now;
            ShoppingCartVm.orderHeaderModel.ApplicationUserId = userId;

	        ApplicationUser	applicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);


            foreach (var cart in ShoppingCartVm.ShoppingCartList)
			{
				cart.Price = getPriceBasedOnQuantity(cart);
				ShoppingCartVm.orderHeaderModel.OrderTotal += cart.Price * cart.Count;
			}

			if (applicationUser.CompanyId.GetValueOrDefault()==0)
            {
            //it is a regular coustomer account and we need to capture payment
                ShoppingCartVm.orderHeaderModel.PaymentStatus = SD.PaymentStatusPending;
                ShoppingCartVm.orderHeaderModel.OrderStatus = SD.StatusPending;
            }
            else
            {
				ShoppingCartVm.orderHeaderModel.PaymentStatus = SD.PaymentStatusDelayedPayment;
				ShoppingCartVm.orderHeaderModel.OrderStatus = SD.StatusApproved;
			}
            _unitOfWork.orderHeaderRepository.Add(ShoppingCartVm.orderHeaderModel);
            _unitOfWork.Save();

            foreach(var cart in ShoppingCartVm.ShoppingCartList)
            {
                OrderDetailModel orderDetailModel = new OrderDetailModel();
                orderDetailModel.ProductId= cart.ProductId;
                orderDetailModel.OrderHeaderId = ShoppingCartVm.orderHeaderModel.OrderHeaderId;
                orderDetailModel.Price=cart.Price;
                orderDetailModel.Count = cart.Count;
                //OrderDetailModel orderDetailModel = new()
                //{
                //    ProductId = cart.ProductId,
                //    OrderHeaderId = ShoppingCartVm.orderHeaderModel.OrderHeaderId,
                //    Price = cart.Price,
                //    Count = cart.Count

                //};
                _unitOfWork.orderDetailRepository.Add(orderDetailModel);
                _unitOfWork.Save();
            }
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                //it is a regular coustomer account and we need to capture payment             
                //stripe logic//from stripsession.net..copy/past

                var domain = "https://localhost:7159/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/OrderConfirmation?Id={ShoppingCartVm.orderHeaderModel.OrderHeaderId}",
                    CancelUrl = domain + "Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVm.ShoppingCartList)
                {
                    var sessionlineitem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.productModel.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionlineitem);
                }
                var service = new SessionService();
                Session session=service.Create(options);
                _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(ShoppingCartVm.orderHeaderModel.OrderHeaderId,session.Id,session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);

            }

			return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVm.orderHeaderModel.OrderHeaderId});
		}

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeaderModel orderHeaderModel = _unitOfWork.orderHeaderRepository.Get(u=>u.OrderHeaderId==id, includeproperties: "applicationUser");
            if (orderHeaderModel.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeaderModel.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.orderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();
            }
            List<ShoppingCartModel>shoppingCartModels=_unitOfWork.shoppingCartRepository.
                GetAll(u=>u.ApplicationUserId==orderHeaderModel.ApplicationUserId).ToList();

            _unitOfWork.shoppingCartRepository.RemoveRange(shoppingCartModels);
            _unitOfWork.Save();

            return View(id);
        }
		public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.shoppingCartRepository.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(u => u.Id == cartId, tracked: true);
            if (cartFromDb.Count - 1 <= 0)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.shoppingCartRepository
                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count()-1);

                //Remove that from cart
                _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                   cartFromDb.Count -= 1;
                _unitOfWork.shoppingCartRepository.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(u => u.Id == cartId, tracked:true);

            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.shoppingCartRepository
               .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();
           

            return RedirectToAction(nameof(Index));
        }

        private double getPriceBasedOnQuantity(ShoppingCartModel shoppingCartModel)
        {
            if (shoppingCartModel.Count <= 50)
            {
                return shoppingCartModel.productModel.Price;
            }
            else
            {
                if (shoppingCartModel.Count <= 100)
                {
                    return shoppingCartModel.productModel.Price50;
                }
                else
                {
                    return shoppingCartModel.productModel.Price100;
                }
            }
        }

    }
}
