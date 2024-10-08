namespace VillaHotelBooking.Web.ViewModels
{
    public class RadialBarChartVM
    {
        public decimal TotalCount { get; set; }
        public decimal CountInCurrentMonth { get; set; }
        public bool hasRatioIncrease { get; set; }
        public int[] Series { get; set; }
    }
}
