using BulkyBook.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.CookiePolicy;
using BulkyBook.DataAccess.Data;
using BulkyBook.Utility;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        public DbInitializer(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
           ApplicationDbContext db )
        {
            _userManager= userManager;
            _roleManager= roleManager;
            _db= db;
        }
        public void Initialize()
        {
            // migration if they are not applied
            try {
                if (_db.Database.GetPendingMigrations().Count()>0)
                {
                    _db.Database.Migrate();
                }
            }
            catch(Exception ex)
            {

            }
            //create roles if they are not crated
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();

                //if roles are not created, then will create admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "ahmed.elghobary01@gmail.com",
                    Email = "ahmed.elghobary01@gmail.com",
                    Name = "Ahmed Elghobary",
                    PhoneNumber = "01021075761",
                    StreetAddress = "Alfatah",
                    State = "Gamssa",
                    PostalCode = "7731168",
                    City = "mansoura"

                }, "Admin*123").GetAwaiter().GetResult();
                ApplicationUser user = _db.ApplicationUsers.
                    FirstOrDefault(u => u.Email == "ahmed.elghobary01@gmail.com");

                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }
            return;
           
        }

    }
}
