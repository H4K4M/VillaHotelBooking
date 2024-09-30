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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IVillaRepository Villas { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Villas = new VillaRepository(_context);
            // VillaNumbers = new VillaNumberRepository(_context);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
