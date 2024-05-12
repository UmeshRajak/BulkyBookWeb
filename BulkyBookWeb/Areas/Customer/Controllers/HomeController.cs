using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyBookWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _iunitofWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork iunitOfWork)
        {
            _logger = logger;
            _iunitofWork = iunitOfWork;
        }

        public IActionResult Index()
        {
           
            IEnumerable<ProductModel> productList = _iunitofWork.productRepository.GetAll(includeproperties: "categoryModel").ToList();
            return View(productList);
        }
        
        public IActionResult Details(int pid)
        {
            ShoppingCartModel shoppingCartModel = new()
            {
                productModel = _iunitofWork.productRepository.Get(u => u.ProductId == pid, includeproperties: "categoryModel"),
                Count = 1,
                ProductId = pid
            };
            //ShoppingCartModel shoppingCartModel = new ShoppingCartModel();
            //shoppingCartModel.productModel = _iunitofWork.productRepository.Get(u => u.ProductId == pid, includeproperties: "categoryModel");
            //shoppingCartModel.Count = 1;
            //shoppingCartModel.productModel.ProductId = pid;
            return View(shoppingCartModel);
        }
        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCartModel shoppingCartModel)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCartModel.ApplicationUserId = UserId;

            ShoppingCartModel cartFromDb = _iunitofWork.shoppingCartRepository.Get(u=>u.ApplicationUserId == UserId &&
            u.ProductId == shoppingCartModel.ProductId);    // this line check ki ye ApplicationUserid && ProductID phle se he ya nahi
            if (cartFromDb != null)
            {
               //shoppoing cart exist than update
                cartFromDb.Count += shoppingCartModel.Count;
                _iunitofWork.shoppingCartRepository.Update(cartFromDb);
                _iunitofWork.Save();
                TempData["Success"] = "Updated";
            }
            else
            {
                //New shopping cart Add
                _iunitofWork.shoppingCartRepository.Add(shoppingCartModel);
                _iunitofWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, 
                        _iunitofWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == UserId).Count());
                TempData["Success"] = "Inserted Success";
            }
            
           
          
            return RedirectToAction (nameof(Index));
        }
        //public IActionResult Details(int pid)
        //{
        //    ProductModel productModel = _iunitofWork.productRepository.Get(u => u.ProductId == pid, includeproperties: "categoryModel");
        //}          return View(ProductModel);

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


