
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
  //  [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        
       

        public IActionResult Index()
        {
            List<Product> ProductList = _unitOfWork.Product.GetAll(IncludeProperities: "category").ToList();

            return View(ProductList);
        }

        public IActionResult UpSert(int? id)
        {


            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                product = new Product()
            };
            if(id == null|| id == 0)
            {
                //create
                return View(productVM);

            }
            else
            {
                // Update Fuctionality
               productVM.product= _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult UpSert(ProductVM ProductVM,IFormFile? file)
        {


            if (ModelState.IsValid)
            {
                string wwwRootpath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    //File Name
                    string FileName=Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    //Location where we have to Save
                    string pathProduct=Path.Combine(wwwRootpath, @"Images\Product");

                    if(!string.IsNullOrEmpty(ProductVM.product.ImageUrl))
                    {
                        //delete old image
                        var oldIamge =
                            Path.Combine(wwwRootpath, ProductVM.product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldIamge))
                        {
                            System.IO.File.Delete(oldIamge);
                        }

                    }
                    //save
                    using(var stream = new FileStream(Path.Combine(pathProduct,FileName),FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                    ProductVM.product.ImageUrl = @"\Images\Product\" + FileName;
                }
                if(ProductVM.product.Id==0)
                {
                    _unitOfWork.Product.Add(ProductVM.product);


                }
                else
                {
                    _unitOfWork.Product.Update(ProductVM.product);
                }
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            else
            {
                ProductVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(ProductVM);

            }

        }

        #region Edit_Hashed
        //public IActionResult Edit(int? id)
        //{
        //    if (id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    Product? ProductFromDb = _unitOfWork.Product.Get(x => x.Id == id);
        //    if (ProductFromDb == null)
        //    {

        //        return NotFound();
        //    }
        //    return View(ProductFromDb);
        //}

        //[HttpPost]
        //public IActionResult Edit(Product obj)
        //{


        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Product.Update(obj);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product updated successfully";

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
        //    Product? ProductFromDb = _unitOfWork.Product.Get(x => x.Id == id);
        //    if (ProductFromDb == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(ProductFromDb);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePost(int? id)
        //{

        //    Product? obj = _unitOfWork.Product.Get(x => x.Id == id);
        //    if (obj == null)
        //    {
        //        return NotFound();
        //    }
        //    _unitOfWork.Product.Remove(obj);
        //    _unitOfWork.Save();
        //    TempData["success"] = "Product Deleted successfully";

        //    return RedirectToAction("Index");

        //} 
        #endregion
        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> ProductList = _unitOfWork.Product.GetAll(IncludeProperities: "category").ToList();
            return Json(new {data= ProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var ProductToBeDelete = _unitOfWork.Product.Get(u => u.Id == id);
            if(ProductToBeDelete== null)
            {
                return Json(new { success = false, message = "Error While Deleting!" });
            }

            // delete image 
            var oldIamge =
                            Path.Combine(_webHostEnvironment.WebRootPath, 
                            ProductToBeDelete.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldIamge))
            {
                System.IO.File.Delete(oldIamge);
            }
            _unitOfWork.Product.Remove(ProductToBeDelete); 
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });

        }
        #endregion
    }
}
