using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.Domain.Entities;

namespace VillaHotelBooking.App.Common.Utility
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusCheckedIn = "CheckedIn";
        public const string StatusCompleted = "Completed";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public static int VillaRoomAvailable_Count(int villaId, List<VillaNumber> villaNumberList,
            DateOnly checkInDate, int nights, List<Booking> bookings)
        {
            List<int> bookingInDate = new();
            int finalAvailableRoomsForAllNights = int.MaxValue;
            var roomsInVilla = villaNumberList.Where(u => u.VillaId == villaId).Count();

            for(int i = 0; i < nights; i++)
            {
                var vallasbooked = bookings.Where(u => u.CheckInDate <= checkInDate.AddDays(i) 
                && u.CheckOutDate > checkInDate.AddDays(i) && u.VillaId == villaId);

                foreach(var booking in vallasbooked)
                {
                    if (!bookingInDate.Contains(booking.Id))
                    {
                        bookingInDate.Add(booking.Id);
                    }
                }

                var totalAvailableRooms = roomsInVilla - bookingInDate.Count;
                if(totalAvailableRooms == 0)
                {
                    return 0;
                }
                else if (totalAvailableRooms < finalAvailableRoomsForAllNights)
                {
                    finalAvailableRoomsForAllNights = totalAvailableRooms;
                }
            }
            return finalAvailableRoomsForAllNights;
        }
    }
}
