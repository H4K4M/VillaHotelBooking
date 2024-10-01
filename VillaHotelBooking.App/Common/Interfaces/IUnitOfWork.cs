﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillaHotelBooking.App.Common.Interfaces
{
    public interface IUnitOfWork
    {
        IVillaRepository Villas { get; }
        IVillaNumberRepository VillaNumbers { get; }
        void Save();
    }
}
