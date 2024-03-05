
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using BulkyBook.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.ObjectModel;

namespace BulkyBookWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
   [Authorize(Roles = SD.Role_Admin)]

    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
    
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }

        
       

        public IActionResult Index()
        {
            List<Company> CompanyList = _unitOfWork.Company.GetAll().ToList();

            return View(CompanyList);
        }

        public IActionResult UpSert(int? id)
        {


            if(id == null|| id == 0)
            {
                //create
                return View(new Company());

            }
            else
            {
                // Update Fuctionality
               Company companyobj= _unitOfWork.Company.Get(u => u.Id == id);
                return View(companyobj);
            }
        }

        [HttpPost]
        public IActionResult UpSert(Company Companyobj)
        {


            if (ModelState.IsValid)
            {
                
            
                if(Companyobj.Id==0)
                {
                    _unitOfWork.Company.Add(Companyobj);


                }
                else
                {
                    _unitOfWork.Company.Update(Companyobj);
                }
                _unitOfWork.Save();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            else
            {

                return View(Companyobj);

            }

        }

        #region Edit_Hashed
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? CompanyFromDb = _unitOfWork.Company.Get(x => x.Id == id);
        //    if (CompanyFromDb == null)
        //    {

        //        return NotFound();
        //    }
        //    return View(CompanyFromDb);
        //}

        //[HttpPost]
        //public IActionResult Edit(Company obj)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Company.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Company updated successfully";

        //        return RedirectToAction("Index");
        //    }
        //    return View(obj);

        //} 
        #endregion

        #region Delete_Hashed
        //public IActionResult Delete(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Company? CompanyFromDb = _unitOfWork.Company.Get(x => x.Id == id);
        //    if (CompanyFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(CompanyFromDb);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{

        //    Company? obj = _unitOfWork.Company.Get(x => x.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Company.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Company Deleted successfully";

        //    return RedirectToAction("Index");

        //} 
        #endregion
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> CompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new {data= CompanyList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var CompanyToBeDelete = _unitOfWork.Company.Get(u => u.Id == id);
            if(CompanyToBeDelete== null)
            {
                return Json(new { success = false, message = "Error While Deleting!" });
            }


            _unitOfWork.Company.Remove(CompanyToBeDelete);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion
    }
}
