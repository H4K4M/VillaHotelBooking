using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.Domain.Entities;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public VillaNumberController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villasNumbers = _unitOfWork.VillaNumbers.GetAll(includeProperties: "Villa");  // Include is used to load the related Villa object
            return View(villasNumbers);
        }


        // GET: Villa/Create
        public IActionResult Create()
        {
            // Dropdown list of Villas for Creating VillaNumber
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };


            // ViewBag.VillaList = list;
            //ViewData["VillaList"] = list;
            return View(villaNumberVM);
        }

        // POST: Villa/Create
        [HttpPost]
        public IActionResult Create(VillaNumberVM villanumberVM)
        {
            //VillaNumber? VillaNumber = _context.VillaNumbers.FirstOrDefault(u => u.Villa_Number == villanumber.Villa_Number);
            bool RoomNumberExists = _unitOfWork.VillaNumbers.Any(u => u.Villa_Number == villanumberVM.VillaNumber.Villa_Number);
            if (ModelState.IsValid && !RoomNumberExists)
            {
                _unitOfWork.VillaNumbers.Add(villanumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Villa Number already exists.";
            villanumberVM.VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(villanumberVM);
        }

        // GET: Villa/Edit?villaId=
        public IActionResult Edit(int villanumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumbers.Get(u => u.Villa_Number == villanumberId)
            };

            if (villaNumberVM.VillaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        // POST: Villa/Edit
        [HttpPost]
        public IActionResult Edit(VillaNumberVM villanumberVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.VillaNumbers.Update(villanumberVM.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The Villa Number can not be updated.";
            villanumberVM.VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
            return View(villanumberVM);
        }

        // GET: Villa/Delete?villaId=
        public IActionResult Delete(int villanumberId)
        {
            VillaNumberVM villaNumberVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumbers.Get(u => u.Villa_Number == villanumberId)
            };

            if (villaNumberVM.VillaNumber is null)
            {
                return RedirectToAction("Error", "Home");
            }
            return View(villaNumberVM);
        }

        // POST: Villa/Delete?villaId=
        [HttpPost]
        public IActionResult Delete(VillaNumberVM villanumberVM)
        {
            VillaNumber? villaNumberFromDb = _unitOfWork.VillaNumbers.Get(v => v.Villa_Number == villanumberVM.VillaNumber.Villa_Number);
            if (villaNumberFromDb is not null)
            {
                _unitOfWork.VillaNumbers.Remove(villaNumberFromDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa number has been deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "The villa number could not be deleted.";
            return View();
        }
    }
}
