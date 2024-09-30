using VillaHotelBooking.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using VillaHotelBooking.App.Common.Interfaces;

namespace VillaHotelBooking.Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public VillaController(IUnitOfWork unitOfWork, IWebHostEnvironment hostingEnvironment)
        {
            _unitOfWork = unitOfWork;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            var villas = _unitOfWork.Villas.GetAll();
            return View(villas);
        }


        // GET: Villa/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Villa/Create
        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("Description", "Name and Description cannot be the same");
            }
            if (ModelState.IsValid)
            {
                if (villa.Image != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_hostingEnvironment.WebRootPath, @"images\VillaImage");

                    using var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create);
                    villa.Image.CopyTo(fileStream);

                    villa.ImageUrl = @"images\VillaImage\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }
                _unitOfWork.Villas.Add(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(villa);
        }

        // GET: Villa/Edit?villaId=
        public IActionResult Edit(int villaId)
        {
            Villa? villa = _unitOfWork.Villas.Get(v => v.Id == villaId);
            //Villa? villa = _context.Villas.Find(villaId);
            //var villaList = _context.Villas.Where(u => u.Price > 50 && u.Occupancy > 0);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        // POST: Villa/Edit
        [HttpPost]
        public IActionResult Edit(Villa villa)
        {
            if (villa.Name == villa.Description)
            {
                ModelState.AddModelError("Description", "Name and Description cannot be the same");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.Villas.Update(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa can not be updated.";
            return View(villa);
        }

        // GET: Villa/Delete?villaId=
        public IActionResult Delete(int villaId)
        {
            Villa? villa = _unitOfWork.Villas.Get(v => v.Id == villaId);
            if (villa is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villa);
        }

        // POST: Villa/Delete?villaId=
        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            Villa? villaFromDb = _unitOfWork.Villas.Get(v => v.Id == villa.Id);
            if (villaFromDb is not null)
            {
                _unitOfWork.Villas.Remove(villaFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa could not be deleted.";
            return View(villaFromDb);
        }
    }
}
