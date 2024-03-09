
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
        
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
    
        public UserController(IUnitOfWork unitOfWork, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
           
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        
       

        public IActionResult Index()
        {
           return View();
        }

        public IActionResult RoleManagment(string UserId)
        {
            //string RoleId=_db.UserRoles.FirstOrDefault(u=>u.UserId== UserId).RoleId;
            RoleManagmentVM RoleVM = new RoleManagmentVM
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u=>u.Id==UserId,IncludeProperities: "company"),
                

                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name

                }),
                CompanyList =_unitOfWork.Company.GetAll(). Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                })
            };
            RoleVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.
                Get(x=>x.Id==UserId)).
                GetAwaiter().GetResult().FirstOrDefault();
            return View(RoleVM);
        }

        [HttpPost]
        public IActionResult RoleManagment(RoleManagmentVM roleManagmentVM)
        {
            string oldRole =
              _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.
                Get(x => x.Id == roleManagmentVM.ApplicationUser.Id)).
                GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u=>u.Id==roleManagmentVM.ApplicationUser.Id);


            if (!(roleManagmentVM.ApplicationUser.Role == oldRole))
            {
                // role was updated
                if (roleManagmentVM.ApplicationUser.Role==SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                }
                if (oldRole==SD.Role_Company)
                {
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();
                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole==SD.Role_Company && applicationUser.CompanyId != roleManagmentVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId = roleManagmentVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

          return RedirectToAction("index");
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(IncludeProperities:"company").ToList();
            
            foreach(var user in objUserList)
            {
               
                user.Role =  _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

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
          var objFromDb= _unitOfWork.ApplicationUser.Get(u=>u.Id==id);
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
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Opertaion Successful" });

        }
        #endregion
    }
}
