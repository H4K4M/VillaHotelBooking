using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.App.Common.Interfaces;
using VillaHotelBooking.Domain.Entities;
using VillaHotelBooking.Infa.Data;

namespace VillaHotelBooking.Infa.Repository
{
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        private readonly ApplicationDbContext _context;
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(Booking entity)
        {
            _context.Bookings.Update(entity);
        }
    }
}
