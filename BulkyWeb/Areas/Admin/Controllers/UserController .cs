
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
   [Authorize(Roles = SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
    
        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        
       

        public IActionResult Index()
        {
           return View();
        }

        public IActionResult RoleManagment(string UserId)
        {
            string RoleId=_db.UserRoles.FirstOrDefault(u=>u.UserId== UserId).RoleId;
            RoleManagmentVM RoleVM = new RoleManagmentVM
            {
                ApplicationUser = _db.ApplicationUsers.Include(u => u.company)
                .FirstOrDefault(u => u.Id == UserId),

                RoleList = _db.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name

                }),
                CompanyList = _db.Companies.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            RoleVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            string RoleId = _db.UserRoles.FirstOrDefault(u => u.UserId == roleManagmentVM.ApplicationUser.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(u => u.Id == RoleId).Name;

            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {
                // role was updated
                ApplicationUser applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == roleManagmentVM.ApplicationUser.Id);
                if (roleManagmentVM.ApplicationUser.Role==SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole==SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _db.SaveChanges();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }

          return RedirectToAction("index");
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u=>u.company).ToList();
            var UserRoles = _db.UserRoles.ToList();
            var Roles=_db.Roles.ToList();
            foreach(var user in objUserList)
            {
                var RoleId = UserRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = Roles.FirstOrDefault(u => u.Id == RoleId).Name;
                if (user.company == null)
                {
                    user.company=new Company()
                    {
                        Name=""
                    };
                }
            }
            return Json(new {data= objUserList });
        }

        [HttpPost]
        public IActionResult LockUnLock([FromBody]string id)
        {
          var objFromDb= _db.ApplicationUsers.FirstOrDefault(u=>u.Id==id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/UnLocking" });

            }
            if (objFromDb.LockoutEnd!=null&& objFromDb.LockoutEnd>DateTime.Now)
            {
                // user is currently locked and wee need UnLocked them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();



            return Json(new { success = true, message = "Opertaion Successful" });

        }
        #endregion
    }
}
