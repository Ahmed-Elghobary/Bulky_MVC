
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            

            IEnumerable<Product> ProductList = _unitOfWork.Product.GetAll(IncludeProperities: "category,ProductImages");
            return View(ProductList);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart Cart = new ShoppingCart()
            {
                Product = _unitOfWork.Product.Get(x => x.Id == productId, IncludeProperities: "category,ProductImages"),
                Count = 1,
                ProductId = productId

            };

            return View(Cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

          
           shoppingCart.AppUserId = userId;

            ShoppingCart CartFromDb = _unitOfWork.ShoppingCart.Get(x => x.AppUserId == userId &&
            x.ProductId == shoppingCart.ProductId);

            if(CartFromDb != null)
            {
                // exist
                CartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart.Update(CartFromDb);
                _unitOfWork.Save();

            }
            else
            {
                //add car record
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                _unitOfWork.Save();

                HttpContext.Session.SetInt32(SD.SessionCart,
                    _unitOfWork.ShoppingCart.GetAll(x => x.AppUserId == userId).Count());

            }
            TempData["success"] = "Cart Updated successfully";


            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
