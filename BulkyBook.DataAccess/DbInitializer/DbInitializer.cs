using BulkyBook.DataAccess.Data;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{

    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        public readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(UserManager<IdentityUser> userManager,
                                RoleManager<IdentityRole> roleManager,
                               ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
           _db=db;
        }

       
        public void Initialize()
        {
            //Migration if they are not Apply
            try
            {
                if(_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception x)
            {

                
            }
           

            //Create Roles if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "umeshrjk2@gmail.com",
                    Email = "umeshrjk2@gmail.com",
                    Name = "umesh",
                    PhoneNumber = "9898989898",
                    StreetAddress = "12,a",
                    State = "IL",
                    PostalAddress = "232323",
                    City = "Chicago",
                }, "Admin@123").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "umeshrjk2@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            
            //if Roles are not created, then we will create admin user as well            
        }
    }
}
