using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public struct UdpState
    {
        public UdpClient udpClient;
        public IPEndPoint endPoint;
    }
}
