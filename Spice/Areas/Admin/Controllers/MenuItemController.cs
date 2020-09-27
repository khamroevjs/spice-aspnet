using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        #region Constructor and Index
        public MenuItemController(ApplicationDbContext db, IWebHostEnvironment hostingEnvironment)
        {
            this.db = db;
            this.hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel
            {
                Category = this.db.Category,
                MenuItem = new MenuItem()
            };
        }

        public async Task<IActionResult> Index()
        {
            var menuItem = await db.MenuItem.Include(x => x.Category).Include(x => x.SubCategory).ToListAsync();
            return View(menuItem);
        }

        #endregion

        #region Create

        // GET - Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
                return View(MenuItemVM);

            await db.MenuItem.AddAsync(MenuItemVM.MenuItem);
            await db.SaveChangesAsync();

            string webRootPath = hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Any())
            {
                //files has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                await using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    await files[0].CopyToAsync(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                //no file was uploaded, so use default
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";
            }

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Edit

        //GET - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();


            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());

            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            //Work on the image saving section

            string webRootPath = hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            var menuItemFromDb = await db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            if (files.Count > 0)
            {
                //New Image has been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);

                //Delete the original file
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                //we will upload the new file
                await using (var filesStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension_new), FileMode.Create))
                {
                    await files[0].CopyToAsync(filesStream);
                }
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
            }

            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spiciness = MenuItemVM.MenuItem.Spiciness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Details

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();


            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        #endregion

        #region Delete

        // GET - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();


            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            string webRootPath = hostingEnvironment.WebRootPath;
            var menuItem = await db.MenuItem.FindAsync(id);

            if (menuItem != null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                db.MenuItem.Remove(menuItem);
                await db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        #endregion
    }
}
