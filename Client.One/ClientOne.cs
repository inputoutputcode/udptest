using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Client.One
{
    public class ClientOne
    {
        public ClientOne()
        {
            var serverAddress = IPAddress.Loopback;
            int serverPort = 1000;

            Thread.Sleep(1000);

            UdpClient udpClient = new UdpClient(1001);
            udpClient.Connect(serverAddress, serverPort);
            string messageText = "Hello Server";
            byte[] sendBytes = Encoding.UTF8.GetBytes(messageText);
            udpClient.Send(sendBytes, sendBytes.Length);
            Console.WriteLine("Message sent: " + messageText);

            var remoteIpEndPoint = new IPEndPoint(serverAddress, serverPort);

            // TODO: BeginReceive
            byte[] receiveBytes = udpClient.Receive(ref remoteIpEndPoint);
            string returnData = Encoding.UTF8.GetString(receiveBytes);

            Console.WriteLine("Message you received: " + returnData.ToString());
            Console.WriteLine("Message was sent from " + remoteIpEndPoint.Address.ToString() + " on port  " + remoteIpEndPoint.Port.ToString());

            udpClient.Close();
        }
    }
}
