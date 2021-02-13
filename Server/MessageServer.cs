using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Server
{
    internal class MessageServer
    {
        public int listenPort = 1000;
        public bool messageReceived = false;

        public UdpClient udpClient;
        public IPEndPoint localEndPoint;

        public MessageServer()
        {
            // Receive a message and write it to the console.
            localEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
            udpClient = new UdpClient(localEndPoint);

            ReceiveMessages();
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            var udpClient = ((UdpState)(result.AsyncState)).udpClient;
            var endPoint = ((UdpState)(result.AsyncState)).endPoint;

            byte[] receiveBytes = udpClient.EndReceive(result, ref endPoint);
            string receiveString = Encoding.UTF8.GetString(receiveBytes);

            Console.WriteLine($"Message received: {receiveString}");
            messageReceived = true;
        }

        public void ReceiveMessages()
        {
            UdpState udpState = new UdpState
            {
                endPoint = localEndPoint,
                udpClient = udpClient
            };

            Console.WriteLine("listening for messages");
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

            Console.WriteLine("Server is listening...");
        }
    }
}
