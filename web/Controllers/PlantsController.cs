using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using Microsoft.AspNetCore.Authorization;	
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace web.Controllers
{
    // Require login for everything	
    [Authorize]
    public class PlantsController : Controller
    {
        private readonly PileaContext _context;

        // Object for fetching user info.	
        private readonly UserManager<User> _usermanager;

        public PlantsController(PileaContext context, UserManager<User> userManager)
        {
            _context = context;
            _usermanager = userManager;
        }

        // GET: Plants
        public async Task<IActionResult> Index()
        {
            var pileaContext = _context.Plants.Include(p => p.Category).Include(p => p.Location);
            return View(await pileaContext.ToListAsync());
        }

        // GET: Plants/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _context.Plants
                .Include(p => p.Category)
                .Include(p => p.Location)
                .FirstOrDefaultAsync(m => m.PlantID == id);
            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }

        // GET: Plants/Create
        public IActionResult Create()
        {
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID");
            ViewData["LocationID"] = new SelectList(_context.Locations, "LocationID", "LocationID");
            return View();
        }

        // POST: Plants/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlantID,Name,Description,Note,image,DaysBetweenWatering,LastWateredDate,NextWateredDate,CategoryID,LocationID")] Plant plant)
        {

            // Get ApplicationUser object (plant owner)	
            var currentUser = await _usermanager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                plant.User = currentUser;
                Console.WriteLine("to je image "+Request.Form.Files[0]);
                plant.image = UploadImage(Request);
                
                _context.Add(plant);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", plant.CategoryID);
            ViewData["LocationID"] = new SelectList(_context.Locations, "LocationID", "LocationID", plant.LocationID);
            return View(plant);
        }

        // GET: Plants/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _context.Plants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", plant.CategoryID);
            ViewData["LocationID"] = new SelectList(_context.Locations, "LocationID", "LocationID", plant.LocationID);
            return View(plant);
        }

        // POST: Plants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlantID,Name,Description,Note,image,DaysBetweenWatering,LastWateredDate,NextWateredDate,CategoryID,LocationID")] Plant plant)
        {
            if (id != plant.PlantID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlantExists(plant.PlantID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryID"] = new SelectList(_context.Categories, "CategoryID", "CategoryID", plant.CategoryID);
            ViewData["LocationID"] = new SelectList(_context.Locations, "LocationID", "LocationID", plant.LocationID);
            return View(plant);
        }

        // GET: Plants/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plant = await _context.Plants
                .Include(p => p.Category)
                .Include(p => p.Location)
                .FirstOrDefaultAsync(m => m.PlantID == id);
            if (plant == null)
            {
                return NotFound();
            }

            return View(plant);
        }

        // POST: Plants/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plant = await _context.Plants.FindAsync(id);
            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlantExists(int id)
        {
            return _context.Plants.Any(e => e.PlantID == id);
        }



        [HttpPost]
        public byte[] UploadImage(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            foreach(var file in request.Form.Files)
            {
                Plant img = new Plant();
                var ImageTitle = file.FileName;

                MemoryStream ms = new MemoryStream();
                file.CopyTo(ms);
                img.image = ms.ToArray();

                ms.Close();
                ms.Dispose();
                Console.Write("IM WRIGINT IMAGE "+file);
                ViewBag.Message = "Image(s) stored in database!";
                return img.image;
            }
            return null;
            
            
            //return View("Index");
        }

    }
}
