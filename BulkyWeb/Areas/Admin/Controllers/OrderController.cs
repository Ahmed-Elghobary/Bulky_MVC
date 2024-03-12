﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM  OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
             OrderVM = new ()
            {
                OrderHeader=_unitOfWork.OrderHeader.Get(u=>u.Id==orderId,IncludeProperities: "applicationUser"),
                OrderDetails=_unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId==orderId,IncludeProperities:"Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]

        public IActionResult UpdateOrderDetail( )
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name=OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber=OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress=OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City=OrderVM.OrderHeader.City;
            orderHeaderFromDb.State=OrderVM.OrderHeader.State;
            orderHeaderFromDb.PostalCode=OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier)){
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.Carrier = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated successfully";

            return RedirectToAction(nameof(Details), new { orderId =orderHeaderFromDb.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order Details Updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {

            var orderHeader= _unitOfWork.OrderHeader.Get(u=>u.Id==OrderVM.OrderHeader.Id);
            orderHeader.TrackingNumber=OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier=OrderVM.OrderHeader.Carrier;
            orderHeader.ShippingDate=DateTime.Now;
            orderHeader.OrderStatus=SD.StatusShipped;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime( DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();
            TempData["success"] = "Order  Shipped successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        { 
            var orderHeader= _unitOfWork.OrderHeader.Get(u=>u.Id==OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus==SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent=orderHeader.PaymentIntendId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);

            }
            _unitOfWork.Save();
            TempData["success"] = "Order  Cancelled successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });

        }
        [ActionName("Details")]
        [HttpPost]
        public IActionResult Details_PAY_NOW()
        {

            OrderVM.OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id,
                IncludeProperities: "applicationUser");

            OrderVM.OrderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == OrderVM.OrderHeader.Id,
                IncludeProperities: "Product");

            // stripe logic
            var Domain = "https://localhost:7203/";
            var options = new Stripe.Checkout.SessionCreateOptions
            {
                SuccessUrl = Domain + $"admin/order/OrderConformation?orderHeaderId={OrderVM.OrderHeader.Id}",
                CancelUrl = Domain + $"admin/order/details/orderId={OrderVM.OrderHeader.Id}",
                LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),

                Mode = "payment",
            };
            foreach (var item in OrderVM.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),// 20.50>>2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new Stripe.Checkout.SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
           

        }
        public IActionResult OrderConformation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(x => x.Id == orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this oeder by company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId,orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            

            return View(orderHeaderId);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if(User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)){
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(IncludeProperities: "applicationUser").ToList();
            }
            else
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders=_unitOfWork.OrderHeader.GetAll(u=>u.AppUserid == userId, IncludeProperities: "applicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u=>u.PaymentStatus==SD.PaymentStatusDelayedPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.StatusApproved);
                    break;
                default:
                   
                    break;
            }


            return Json(new { data = objOrderHeaders });
        }

        
        #endregion
    }
}