using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
	{

		private readonly IUnitOfWork _unitOfWork;
        
        public OrderController(IUnitOfWork unitOfWork)
        {
				_unitOfWork=unitOfWork;
        }
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderHeaderId)
        {
            OrderVM = new()
            {
                orderHeaderModel = _unitOfWork.orderHeaderRepository.Get(u => u.OrderHeaderId == orderHeaderId, includeproperties : "applicationUser"),

                orderDetailModels =_unitOfWork.orderDetailRepository.GetAll(u=>u.OrderHeaderId== orderHeaderId, includeproperties : "productModel")


            };

            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin +","+SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.orderHeaderModel.OrderHeaderId);
            orderHeaderFromDb.Name = OrderVM.orderHeaderModel.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.orderHeaderModel.PhoneNumber;
            orderHeaderFromDb.StreetAddress= OrderVM.orderHeaderModel.StreetAddress;
            orderHeaderFromDb.City= OrderVM.orderHeaderModel.City;
            orderHeaderFromDb.State = OrderVM.orderHeaderModel.State;
            orderHeaderFromDb.PostalAddress = OrderVM.orderHeaderModel.PostalAddress;

            if(!string.IsNullOrEmpty(OrderVM.orderHeaderModel.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeaderModel.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderHeaderModel.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.orderHeaderModel.TrackingNumber;
            }
            _unitOfWork.orderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Deatils Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderHeaderId = orderHeaderFromDb.OrderHeaderId });
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.orderHeaderRepository.UpdateStatus(OrderVM.orderHeaderModel.OrderHeaderId, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Deatils Updated Successfully";
            return RedirectToAction(nameof(Details), new { orderHeaderId = OrderVM.orderHeaderModel.OrderHeaderId });
        }

        public IActionResult ShipOrder()
        {

            var orderHeader = _unitOfWork.orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.orderHeaderModel.OrderHeaderId);
            orderHeader.TrackingNumber = OrderVM.orderHeaderModel.TrackingNumber;
            orderHeader.Carrier = OrderVM.orderHeaderModel.TrackingNumber;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if(orderHeader.OrderStatus==SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate=DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.orderHeaderRepository.Update(orderHeader);
            _unitOfWork.Save();
            TempData["Success"] = "Order shipped Successfully";
            return RedirectToAction(nameof(Details), new { orderHeaderId = OrderVM.orderHeaderModel.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.orderHeaderModel.OrderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeader.OrderHeaderId, SD.StatusCancelled, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancel Successfully";
            return RedirectToAction(nameof(Details), new { orderHeaderId = OrderVM.orderHeaderModel.OrderHeaderId });
        }

        [ActionName("Details")]
        [HttpPost]
       public IActionResult Details_PAY_NOW()
        {
           OrderVM.orderHeaderModel = _unitOfWork.orderHeaderRepository.Get(u => u.OrderHeaderId == OrderVM.orderHeaderModel.OrderHeaderId, includeproperties: "applicationUser");

            OrderVM.orderDetailModels = _unitOfWork.orderDetailRepository.GetAll(u => u.OrderHeaderId == OrderVM.orderHeaderModel.OrderHeaderId, includeproperties: "productModel");
            
            //it is a regular coustomer account and we need to capture payment             
            //stripe logic//from stripsession.net..copy/past

            var domain = "https://localhost:7159/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVM.orderHeaderModel.OrderHeaderId}",
                CancelUrl = domain + $"admin/order/details?orderHeaderId={OrderVM.orderHeaderModel.OrderHeaderId}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVM.orderDetailModels)
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
            Session session = service.Create(options);
            _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(OrderVM.orderHeaderModel.OrderHeaderId, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeaderModel orderHeaderModel = _unitOfWork.orderHeaderRepository.Get(u => u.OrderHeaderId == orderHeaderId);
            if (orderHeaderModel.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeaderModel.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeaderId, orderHeaderModel.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        #region API CALLS
        [HttpGet]
               
        public IActionResult GetAll(string status)
        {            
            IEnumerable<OrderHeaderModel> objOrderHeader;
           
            if(User.IsInRole(SD.Role_Admin)||User.IsInRole(SD.Role_Employee))
            {
                objOrderHeader = _unitOfWork.orderHeaderRepository.GetAll(includeproperties: "applicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeader = _unitOfWork.orderHeaderRepository.GetAll(u => u.ApplicationUserId == userId, includeproperties: "applicationUser").ToList();
            }

            switch (status)
            {
                case "pending":
                    objOrderHeader = objOrderHeader.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeader = objOrderHeader.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                    
                default:

                    break;
            }
            return Json(new { data = objOrderHeader });
        }
        #endregion
    }





}
