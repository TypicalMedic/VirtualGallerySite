﻿using KKKKPPP.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KKKKPPP.Data.Interfaces
{
    public interface IЭкспонат
    {
        IEnumerable<Экспонат> Showpieces { get; }
    }
}
