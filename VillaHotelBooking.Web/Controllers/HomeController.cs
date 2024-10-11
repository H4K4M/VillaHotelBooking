using Microsoft.AspNetCore.Mvc;
using Syncfusion.DocIO;
using Syncfusion.Presentation;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.Metadata;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.App.Common.Utility;
using VillaHotelBooking.Web.Models;
using VillaHotelBooking.Web.ViewModels;

namespace VillaHotelBooking.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            HomeVM homeVM = new()
            {
                VillaList = _unitOfWork.Villas.GetAll(includeProperties: "VillaAmenity"),
                Nights = 1,
                CheckInDate = DateOnly.FromDateTime(DateTime.Now),
            };
            return View(homeVM);
        }

        // POST: HomeController/Index  POST as PartialView
        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villas.GetAll(includeProperties: "VillaAmenity").ToList();
            var villaNumberList = _unitOfWork.VillaNumbers.GetAll().ToList();
            var bookedVillas = _unitOfWork.Bookings.GetAll(u => u.Status == SD.StatusApproved || u.Status == SD.StatusCheckedIn).ToList();


            foreach (var villa in villaList)
            {
                int roomAvailable = SD.VillaRoomAvailable_Count(villa.Id, villaNumberList, checkInDate, nights, bookedVillas);

                villa.IsVailable = roomAvailable > 0 ? true : false;
            }
            HomeVM homeVM = new()
            {
                VillaList = villaList,
                Nights = nights,
                CheckInDate = checkInDate,
            };
            return PartialView("_VillaList", homeVM);
        }

        [HttpPost]
        public IActionResult GeneratePPTExport(int id)
        {
            var villa = _unitOfWork.Villas.Get(u => u.Id == id, includeProperties: "VillaAmenity");
            if(villa is null)
            {
                return RedirectToAction(nameof(Error));
            }

            string basePath = _webHostEnvironment.WebRootPath;
            // Loading the template document
            string filePath = basePath + @"/exports/ExportVillaDetails.pptx";
            
            using IPresentation presentation = Presentation.Open(filePath);
            // Accessing the first slide
            ISlide slide = presentation.Slides[0];

            // Replace the text content
            IShape? shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaName") as IShape;
            if(shape is not null)
            {
                shape.TextBody.Text = villa.Name;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaDescription") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = villa.Description;
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtOccupancy") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaSize") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("Villa Size: {0} sqft", villa.Sqft);
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtPricePerNight") as IShape;
            if (shape is not null)
            {
                shape.TextBody.Text = string.Format("THB {0}/Night",villa.Price.ToString("c", new CultureInfo("th-TH")));
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "txtVillaAmenitiesHeading") as IShape;
            if(shape is not null)
            {
                List<string> amenityList = villa.VillaAmenity.Select(u => u.Name).ToList();

                shape.TextBody.Text = "";

                foreach(var item in amenityList)
                {
                    IParagraph paragraph = shape.TextBody.AddParagraph();
                    ITextPart textPart = paragraph.AddTextPart(item);

                    paragraph.ListFormat.Type = ListType.Bulleted;
                    paragraph.ListFormat.BulletCharacter = '\u2022';
                    textPart.Font.FontName = "system-ui";
                    textPart.Font.FontSize = 18;
                    textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);

                }
            }

            shape = slide.Shapes.FirstOrDefault(u => u.ShapeName == "imgVilla") as IShape;
            if (shape is not null)
            {
                byte[] imgData;
                string imgUrl;
                try
                {
                    imgUrl = string.Format("{0}{1}", basePath, villa.ImageUrl);
                    imgData = System.IO.File.ReadAllBytes(imgUrl);

                }
                catch (Exception)
                {
                    imgUrl = string.Format("{0}{1}", basePath, "/images/placeholder.png");
                    imgData = System.IO.File.ReadAllBytes(imgUrl);
                }
                slide.Shapes.Remove(shape);

                using MemoryStream imgStream = new(imgData);
                IPicture picture = slide.Pictures.AddPicture(imgStream, 60, 120, 300, 200);
            }

            MemoryStream stream = new();
            presentation.Save(stream);
            stream.Position = 0;

            return File(stream, "application/pptx", "Villa.pptx");


        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
