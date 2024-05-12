using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBookWeb.Area.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _iunitOfWork;
        public CategoryController(IUnitOfWork iunitOfWork)
        {
            _iunitOfWork = iunitOfWork;
        }
        public IActionResult Index()
        {
            List<CategoryModel> objCategoryList = _iunitOfWork.categoryRepository.GetAll().ToList();
            return View(objCategoryList);
        }
        
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(CategoryModel cm)
        {
            if (ModelState.IsValid)
            {
                _iunitOfWork.categoryRepository.Add(cm);
                _iunitOfWork.Save();
                TempData["Success"] = "Category Created Successfully";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        public IActionResult Edit(int? id)// 
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            CategoryModel cm = _iunitOfWork.categoryRepository.Get(u=>u.CategoryId == id); // select command
            //CategoryModel? cm1 = _billingContext.Categories.FirstOrDefault(u => u.Categoryid == id);       // first or default or dot contains use when id is not primary so we can finf by name also
            //CategoryModel? cm2 = _billingContext.Categories.Where(u=>u.Categoryid==id).FirstOrDefault();

            if (cm==null)
                {
                    return NotFound();
                }
            
            return View(cm);
        }

        [HttpPost]
        public IActionResult Edit(CategoryModel cm)
        {
            if (ModelState.IsValid)
            {
                _iunitOfWork.categoryRepository.Update(cm);
                _iunitOfWork.Save();
                TempData["Success"] = "Category Updated Successfully";
                return RedirectToAction("Index", "Category");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            CategoryModel cm = _iunitOfWork.categoryRepository.Get(u => u.CategoryId == id);
            //CategoryModel? cm1 = _billingContext.Categories.FirstOrDefault(u => u.Categoryid == id);       // first or default or dot contains use when id is not primary so we can finf by name also
            //CategoryModel? cm2 = _billingContext.Categories.Where(u=>u.Categoryid==id).FirstOrDefault();

            if (cm == null)
            {
                return NotFound();
            }

            return View(cm);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            CategoryModel cm = _iunitOfWork.categoryRepository.Get(u => u.CategoryId == id);
            if (cm == null)
            {
                return NotFound();
            }

            _iunitOfWork.categoryRepository.Remove(cm);
            _iunitOfWork.Save();
                TempData["Success"] = "Category Deleted Successfully";
                return RedirectToAction("Index", "Category");
                        
        }
    }
}
