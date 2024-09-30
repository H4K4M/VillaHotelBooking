using VillaHotelBooking.Domain.Entities;
using VillaHotelBooking.Infa.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.App.Common.Interfaces;

namespace VillaHotelBooking.Infa.Repository
{
    public class VillaRepository : Repository<Villa>, IVillaRepository
    {
        private readonly ApplicationDbContext _context;
        public VillaRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        void IVillaRepository.Update(Villa villa)
        {
            _context.Update(villa);
        }
    }
}
