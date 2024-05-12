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
	public class CompanyController : Controller
    {
        private readonly IUnitOfWork _iunitOfWork;
       
        public CompanyController(IUnitOfWork iunitOfWork)
        {
            _iunitOfWork = iunitOfWork;
            
        }
        public IActionResult Index()
        {
            List<CompanyModel> objCompanyList = _iunitOfWork.companyRepository.GetAll().ToList();
            return View(objCompanyList);
        }

        public IActionResult Upsert(int? id)
        {           
          
            if(id==null || id ==0)
            {
                //create
               
                return View(new CompanyModel());
            }
            else
            {
                //update
                CompanyModel cm = _iunitOfWork.companyRepository.Get(u=>u.id == id);
                return View(cm);
            }
           
        }
        [HttpPost]
        public IActionResult Upsert(CompanyModel cm)
        {

            if (ModelState.IsValid)
            {
                
               
                if(cm.id==0)
                {
                    _iunitOfWork.companyRepository.Add(cm);
                }
                else
                {
                    _iunitOfWork.companyRepository.Update(cm);
                }

                _iunitOfWork.Save();
                TempData["Success"] = "Company Created Successfully";
                return RedirectToAction("Index", "Company");
            }
            else
            {
               
                return View(cm);
            }
            
        }

      

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<CompanyModel> objCompanyList = _iunitOfWork.companyRepository.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }
        #endregion

        #region API CALLS
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var DelObj = _iunitOfWork.companyRepository.Get(u => u.id == id);
            if(DelObj == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            

            _iunitOfWork.companyRepository.Remove(DelObj);
            _iunitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}





