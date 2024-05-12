using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBookWeb.Area.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _iunitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork iunitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _iunitOfWork = iunitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<ProductModel> objProductList = _iunitOfWork.productRepository.GetAll(includeproperties: "categoryModel").ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM pvm = new ProductVM();

            pvm.categoryList = _iunitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.CategoryName,
                Value = u.CategoryId.ToString()
            });
            
          
            if(id==null || id ==0)
            {
                //create
                pvm.productModel = new ProductModel();
                return View(pvm);
            }
            else
            {
                //update
                pvm.productModel = _iunitOfWork.productRepository.Get(u => u.ProductId == id);
                return View(pvm);
            }
           
        }
        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.productModel.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath,productVM.productModel.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.productModel.ImageUrl = @"\images\product\" + fileName;
                }
               
                if(productVM.productModel.ProductId==0)
                {
                    _iunitOfWork.productRepository.Add(productVM.productModel);
                }
                else
                {
                    _iunitOfWork.productRepository.Update(productVM.productModel);
                }

                _iunitOfWork.Save();
                TempData["Success"] = "Product Created Successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.categoryList = _iunitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.CategoryName,
                    Value = u.CategoryId.ToString()
                });
                return View(productVM);
            }
            
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ProductModel> objProductList = _iunitOfWork.productRepository.GetAll(includeproperties: "categoryModel").ToList();
            return Json(new { data = objProductList });
        }
        #endregion

        #region API CALLS
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var DelObj = _iunitOfWork.productRepository.Get(u => u.ProductId == id);
            if(DelObj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, DelObj.ImageUrl.TrimStart('\\'));

            if(System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _iunitOfWork.productRepository.Remove(DelObj);
            _iunitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}




//public IActionResult Create()
//{
//    IEnumerable<SelectListItem> categoryItems = _unitOfWork.Cate
//        .GetAll().Select(u => new SelectListItem
//        {
//            Text = u.CategoryName,
//            Value = u.Categoryid.ToString()
//        }) ;
//    ViewBag.CatList = categoryItems;
//    //ViewData["CatList"]= categoryItems;
//    return View();
//}

//public IActionResult Edit(int? id)// 
//{
//    if (id == null || id == 0)
//    {
//        return NotFound();
//    }
//    ProductModel pm = _unitOfWork.Prod.Get(u => u.ProductId == id); // select command
//                                                                    //CategoryModel? cm1 = _billingContext.Categories.FirstOrDefault(u => u.Categoryid == id);       // first or default or dot contains use when id is not primary so we can finf by name also
//                                                                    //CategoryModel? cm2 = _billingContext.Categories.Where(u=>u.Categoryid==id).FirstOrDefault();

//    if (pm == null)
//    {
//        return NotFound();
//    }

//    return View(pm);
//}

//[HttpPost]
//public IActionResult Edit(ProductModel pm)
//{
//    if (ModelState.IsValid)
//    {
//        _unitOfWork.Prod.Update(pm);
//        _unitOfWork.Save();
//        TempData["Success"] = "Product Updated Successfully";
//        return RedirectToAction("Index", "Product");
//    }
//    return View();
//}

//public IActionResult Delete(int? id)
//{
//    if (id == null || id == 0)
//    {
//        return NotFound();
//    }
//    ProductModel pm = _unitOfWork.productRepository.Get(u => u.ProductId == id);


//    if (pm == null)
//    {
//        return NotFound();
//    }

//    return View(pm);
//}

//[HttpPost, ActionName("Delete")]
//public IActionResult DeletePost(int? id)
//{
//    ProductModel pm = _unitOfWork.productRepository.Get(u => u.ProductId == id);
//    if (pm == null)
//    {
//        return NotFound();
//    }

//    _unitOfWork.productRepository.Remove(pm);
//    _unitOfWork.Save();
//    TempData["Success"] = "Product Deleted Successfully";
//    return RedirectToAction("Index", "Product");

//}
