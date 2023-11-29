
using BulkyBook.DataAccess.Data;
using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyBookWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _CategoryRepository;
        public CategoryController(ICategoryRepository CategoryRepository) 
        { 
            _CategoryRepository = CategoryRepository;
        }

        public IActionResult Index()
        {
            List<Category> CategoryList=_CategoryRepository.GetAll().ToList();
            return View(CategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "the display order does not march the name");
            }

            if (ModelState.IsValid)
            {
                _CategoryRepository.Add(obj);
                _CategoryRepository.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View(obj);
           
        }

        public IActionResult Edit(int? id)
        {
            if(id == null || id==0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _CategoryRepository.Get(x=>x.Id==id);
            if(categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            

            if (ModelState.IsValid)
            {
                _CategoryRepository.Update(obj);
                _CategoryRepository.Save();
                TempData["success"] = "Category updated successfully";

                return RedirectToAction("Index");
            }
            return View(obj);

        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? categoryFromDb = _CategoryRepository.Get(x => x.Id == id);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {

            Category? obj = _CategoryRepository.Get(x => x.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _CategoryRepository.Remove(obj);
            _CategoryRepository.Save();
                TempData["success"] = "Category Deleted successfully";

            return RedirectToAction("Index");

        }
    }
}
