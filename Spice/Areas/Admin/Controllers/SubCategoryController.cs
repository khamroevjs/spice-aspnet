using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private readonly ApplicationDbContext db;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoryController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task<IActionResult> Index()
        {
            var subCategory = await db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subCategory);
        }

        public async Task<IActionResult> GetSubCategory(int? id)
        {
            var subCategories = new List<SubCategory>();

            subCategories = await (from subCategory in db.SubCategory
                                   where subCategory.CategoryId == id
                                   select subCategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        #region Create

        // GET - Create
        public async Task<IActionResult> Create()
        {
            var model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = new SubCategory(),
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        // POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = db.SubCategory.Include(s => s.Category).Where(s =>
                    s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                /* Alternative Solution:
                 * var temp = from item in db.SubCategory.Include(s => s.Category)
                 *            where item.Name == model.SubCategory.Name && item.Category.Id == model.SubCategory.CategoryId
                 *            select item;
                 */

                if (doesSubCategoryExists.Any())
                {
                    //Error
                    StatusMessage =
                        $"Error: SubCategory exists under {doesSubCategoryExists.First().Category.Name} category. Please use another name.";
                }
                else
                {
                    await db.SubCategory.AddAsync(model.SubCategory);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }

            var modelVM = new SubCategoryAndCategoryViewModel
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(modelVM);
        }

        #endregion

        #region Edit

        // GET - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            var model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = await db.SubCategory.FindAsync(id),
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = db.SubCategory.Include(s => s.Category).Where(s =>
                    s.Name == model.SubCategory.Name && s.Category.Id == model.SubCategory.CategoryId);

                if (doesSubCategoryExists.Any())
                {
                    // Error
                    StatusMessage =
                        $"Error: SubCategory exists under {doesSubCategoryExists.First().Category.Name} category. Please use another name.";
                }
                else
                {
                    var subCategoryFromDb = await db.SubCategory.FindAsync(model.SubCategory.Id);
                    subCategoryFromDb.Name = model.SubCategory.Name;

                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }

            var modelVM = new SubCategoryAndCategoryViewModel
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(modelVM);

        }
        #endregion

        #region Details

        // GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            var model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = await db.SubCategory.FindAsync(id),
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        #endregion

        #region Delete

        // GET - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var model = new SubCategoryAndCategoryViewModel
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = await db.SubCategory.FindAsync(id),
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).Distinct().ToListAsync()
            };

            return View(model);
        }

        // POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var subCategory = await db.SubCategory.FindAsync(id);
            if (subCategory == null)
            {
                return NotFound();
            }
            db.SubCategory.Remove(subCategory);
            await db.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        #endregion
    }
}
