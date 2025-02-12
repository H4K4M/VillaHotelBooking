﻿using VillaHotelBooking.Domain.Entities;
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

        public IVillaNumberRepository VillaNumbers { get; private set; }
        public IAmenityRepository Amenities { get; private set; }
        public IBookingRepository Bookings { get; private set; }
        public IApplicationUserRepository ApplicationUsers { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Villas = new VillaRepository(_context);
            VillaNumbers = new VillaNumberRepository(_context);
            Amenities = new AmenityRepository(_context);
            Bookings = new BookingRepository(_context);
            ApplicationUsers = new ApplicationUserRepository(_context);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
