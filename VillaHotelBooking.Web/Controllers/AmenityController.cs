using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.Domain.Entities;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
{
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villasNumbers = _unitOfWork.Amenities.GetAll(includeProperties: "Villa");  // Include is used to load the related Villa object
            return View(villasNumbers);
        }


        // GET: Villa/Create
        public IActionResult Create()
        {
            // Dropdown list of Villas for Creating Amenity
            AmenityVM AmenityVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };


            // ViewBag.VillaList = list;
            //ViewData["VillaList"] = list;
            return View(AmenityVM);
        }

        // POST: Villa/Create
        [HttpPost]
        public IActionResult Create(AmenityVM AmenityVM)
        {
            
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenities.Add(AmenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity already exists.";
            AmenityVM.VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(AmenityVM);
        }

        // GET: Villa/Edit?villaId=
        public IActionResult Edit(int AmenityId)
        {
            AmenityVM AmenityVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenities.Get(u => u.Id == AmenityId)
            };

            if (AmenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(AmenityVM);
        }

        // POST: Villa/Edit
        [HttpPost]
        public IActionResult Edit(AmenityVM AmenityVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.Amenities.Update(AmenityVM.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity can not be updated.";
            AmenityVM.VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(AmenityVM);
        }

        // GET: Villa/Delete?villaId=
        public IActionResult Delete(int AmenityId)
        {
            AmenityVM AmenityVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenities.Get(u => u.Id == AmenityId)
            };

            if (AmenityVM.Amenity is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(AmenityVM);
        }

        // POST: Villa/Delete?villaId=
        [HttpPost]
        public IActionResult Delete(AmenityVM AmenityVM)
        {
            Amenity? AmenityFromDb = _unitOfWork.Amenities.Get(v => v.Id == AmenityVM.Amenity.Id);
            if (AmenityFromDb is not null)
            {
                _unitOfWork.Amenities.Remove(AmenityFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The amenity could not be deleted.";
            return View();
        }
    }
}
