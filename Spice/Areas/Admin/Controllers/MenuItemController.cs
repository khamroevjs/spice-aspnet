using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Models.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IWebHostEnvironment hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM{ get; set; }

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
            var menuItem = await db.MenuItem.Include(x=>x.Category).Include(x=>x.SubCategory).ToListAsync();
            return View(menuItem);
        }

        // GET - Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        [HttpPost]
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
                // files has been uploaded
            }
            else
            {
                // no file uploaded, so use default
            }
        }
    }
}
