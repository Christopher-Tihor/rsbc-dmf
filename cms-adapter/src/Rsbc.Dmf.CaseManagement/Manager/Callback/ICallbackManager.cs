﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rsbc.Dmf.CaseManagement
{
    public interface ICallbackManager
    {
        Task<IEnumerable<Callback>> GetDriverCallbacks(Guid driverId);
    }
}