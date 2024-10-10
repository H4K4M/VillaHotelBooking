using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Stripe;
using Stripe.Checkout;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using System.Globalization;
using System.Security.Claims;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.Domain.Entities;

namespace VillaHotelBooking.Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;  // acess root folder
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, string checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Parse the date back into a DateOnly object
            DateOnly parsedCheckInDate = DateOnly.ParseExact(checkInDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            ApplicationUser user = _unitOfWork.ApplicationUsers.Get(u => u.Id == UserId);

            Booking booking = new Booking
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villas.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = parsedCheckInDate,
                Nights = nights,
                CheckOutDate = parsedCheckInDate.AddDays(nights),
                UserId = UserId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name
            };
            booking.TotalCost = booking.Villa.Price * nights;
            return View(booking);
        }

        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking booking)
        {
            var villa = _unitOfWork.Villas.Get(u => u.Id == booking.VillaId);
            booking.TotalCost = villa.Price * booking.Nights;

            booking.Status = SD.StatusPending;
            booking.BookingDate = DateTime.Now;

            var villaNumberList = _unitOfWork.VillaNumbers.GetAll().ToList();
            var bookedVillas = _unitOfWork.Bookings.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();


            int roomAvailable = SD.VillaRoomAvailable_Count(villa.Id, villaNumberList, booking.CheckInDate, booking.Nights, bookedVillas);

            if(roomAvailable == 0)
            {
                TempData["Error"] = "No Room Available for this Villa";
                return RedirectToAction(nameof(FinalizeBooking), new 
                { 
                    villaId = booking.VillaId, 
                    checkInDate = booking.CheckInDate.ToString("dd/MM/yyyy"), 
                    nights = booking.Nights 
                });
            }

            _unitOfWork.Bookings.Add(booking);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                
                Mode = "payment",
                SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={booking.Id}",
                CancelUrl = domain + $"booking/FinalizeBooking?villaId={booking.VillaId}&checkInDate={booking.CheckInDate.ToString("dd/MM/yyyy")}&nights={booking.Nights}",
                LineItems = new List<SessionLineItemOptions>()
            };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(booking.TotalCost * 100),
                    Currency = "THB",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name
                        //Images = new List<string> { domain + villa.ImageUrl }
                    }
                },
                Quantity = 1,
            });

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Bookings.UpdateStripePaymentID(booking.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Bookings.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.Status == SD.StatusPending)
            {
                // Need to check that if payment is successful
                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Bookings.UpdateStatus(bookingFromDb.Id, SD.StatusApproved, 0);
                    _unitOfWork.Bookings.UpdateStripePaymentID(bookingFromDb.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }

            return View(bookingId);
        }
        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            Booking bookingFromDb = _unitOfWork.Bookings.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

            if(bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId);
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumbers.GetAll(u => u.VillaId == bookingFromDb.VillaId 
                                            && availableVillaNumbers.Any(x => x == u.Villa_Number)).ToList();

            }

            return View(bookingFromDb);
        }
        [HttpPost]
        [Authorize]
        public IActionResult GenerateInvoice(int id)
        {
            string basePath = _webHostEnvironment.WebRootPath;

            WordDocument document = new WordDocument();

            // Loading the template document
            string dataPath = basePath + @"/exports/BookingDetails.docx";
            using FileStream fileStream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            document.Open(fileStream, FormatType.Automatic);

            // Update the template document
            Booking bookingFromDb = _unitOfWork.Bookings.Get(u => u.Id == id, includeProperties: "User,Villa");

            TextSelection textSelection = document.Find("xx_customer_name", false, true);
            WTextRange textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Name;

            textSelection = document.Find("XX_BOOKING_NUMBER", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING ID - " + bookingFromDb.Id;

            textSelection = document.Find("XX_BOOKING_DATE", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = "BOOKING DATE - " + bookingFromDb.BookingDate.ToShortDateString();

            textSelection = document.Find("xx_customer_phone", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Phone;

            textSelection = document.Find("xx_customer_email", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.Email;

            textSelection = document.Find("xx_payment_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.PaymentDate.ToShortDateString();

            textSelection = document.Find("xx_checkin_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckInDate.ToShortDateString();

            textSelection = document.Find("xx_checkout_date", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.CheckOutDate.ToShortDateString();

            textSelection = document.Find("xx_booking_total", false, true);
            textRange = textSelection.GetAsOneRange();
            textRange.Text = bookingFromDb.TotalCost.ToString("c", new System.Globalization.CultureInfo("th-TH"));


            using DocIORenderer renderer = new();

            MemoryStream stream = new();
            document.Save(stream, FormatType.Docx);
            stream.Position = 0;

            return File(stream, "application/docx", "BookingDetails.docx");
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(Booking booking)
        {
            _unitOfWork.Bookings.UpdateStatus(booking.Id, SD.StatusCheckedIn, booking.VillaNumber);
            _unitOfWork.Save();

            TempData["Success"] = "Check In Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(Booking booking)
        {
            _unitOfWork.Bookings.UpdateStatus(booking.Id, SD.StatusCompleted, booking.VillaNumber);
            _unitOfWork.Save();

            TempData["Success"] = "Check Out Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(Booking booking)
        {
            _unitOfWork.Bookings.UpdateStatus(booking.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();

            TempData["Success"] = "Cancelled Successfully";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = booking.Id });
        }

        private List<int> AssignAvailableVillaNumberByVilla(int villaId)
        {
            List<int> availableVillaNumbers = new();

            var villanumbers = _unitOfWork.VillaNumbers.GetAll(u => u.VillaId == villaId);

            var checkedInVilla = _unitOfWork.Bookings.GetAll(u => u.VillaId == villaId && u.Status == SD.StatusCheckedIn)
                .Select(u => u.VillaNumber);

            foreach (var villaNumber in villanumbers)
            {
                if(!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }

        #region API Calls

        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> objBookings;
            if(User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Bookings.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objBookings = _unitOfWork.Bookings.GetAll(u => u.UserId == UserId, includeProperties: "User,Villa");
            }
            if(!string.IsNullOrEmpty(status) && status != "All")
            {
                objBookings = objBookings.Where(u => u.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = objBookings });
        }

        #endregion
    }
}
