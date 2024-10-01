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
    public class AmenityRepository : Repository<Amenity>, IAmenityRepository
    {
        private readonly ApplicationDbContext _context;
        public AmenityRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public void Update(Amenity entity)
        {
            _context.Update(entity);
        }
    }
}
