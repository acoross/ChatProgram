﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acoross.BaseNetworkLib
{
    public interface IGlobal
    {
        void AddClientSocket(ISocket sock);
    }
}
