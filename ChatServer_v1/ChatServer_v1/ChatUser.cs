using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Acoross.BaseNetworkLib;

namespace ChatServer_v1
{
    public class ChatUser : IOwner
    {
        public string name;
        public ISocket UserSocket;
    }
}
