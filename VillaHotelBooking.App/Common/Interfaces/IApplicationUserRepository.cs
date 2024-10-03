﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaHotelBooking.Domain.Entities;

namespace VillaHotelBooking.App.Common.Interfaces
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
    }
}
