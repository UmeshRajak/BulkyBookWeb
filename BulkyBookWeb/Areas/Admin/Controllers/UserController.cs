
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BulkyBookWeb.Area.Admin.Controllers
{
    [Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]
	public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityUser> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityUser>roleManager, IUnitOfWork unitOfWork)
        {
            _userManager= userManager;
            _roleManager = roleManager;
            _unitOfWork= unitOfWork;
            
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult RoleManagment(string userId)
        {
           
            RollManagmentVM RoleVm = new RollManagmentVM()
            {
                applicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId, includeproperties:"CompanyModel"),

                RoleList = _roleManager.Roles.Select(i => new SelectListItem {

                    Text = i.NormalizedUserName,
                    Value = i.NormalizedUserName
                }),
                 CompanyList = _unitOfWork.companyRepository.GetAll().Select(i => new SelectListItem
                 {
                     Text = i.Name,
                     Value = i.id.ToString()
                 }),
            };
            RoleVm.applicationUser.Role = _userManager.GetRolesAsync
                (_unitOfWork.applicationUserRepository.Get(u => u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();
            
            return View(RoleVm);
        }

        [HttpPost]
        public IActionResult RoleManagment(RollManagmentVM rollManagmentVM)
        {
                string oldRole = _userManager.GetRolesAsync(_unitOfWork.applicationUserRepository.Get
                                        (u => u.Id == rollManagmentVM.applicationUser.Id))
                                        .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.Get
                                                        (u => u.Id == rollManagmentVM.applicationUser.Id);
            if (!(rollManagmentVM.applicationUser.Role == oldRole))
            {
                {
                    applicationUser.CompanyId = rollManagmentVM.applicationUser.CompanyId;
                }
                if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.applicationUserRepository.Update(applicationUser);
                _unitOfWork.Save();
                 _userManager.RemoveFromRoleAsync(applicationUser, oldRole.ToString()).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, rollManagmentVM.applicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if (oldRole == SD.Role_Company && applicationUser.CompanyId != rollManagmentVM.applicationUser.CompanyId)
                {
                    applicationUser.CompanyId = rollManagmentVM.applicationUser.CompanyId;
                    _unitOfWork.applicationUserRepository.Update(applicationUser);
                    _unitOfWork.Save();
                }
                
            }
            return RedirectToAction("Index");
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.applicationUserRepository.GetAll(includeproperties:"ComapanyModel").ToList();
          
            foreach (var user in objUserList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                if(user.companyModel==null)
                {
                    user.companyModel = new CompanyModel()
                    {
                        Name = "",
                        
                    };
                }
            }
            return Json(new { data = objUserList });
        }
        #endregion

        #region API CALLS
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var objFromDb = _unitOfWork.applicationUserRepository.Get(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while locking/Unloacking" });
            }
            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
           _unitOfWork.applicationUserRepository.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Operation Successful" });
        }
        #endregion
    }
}





