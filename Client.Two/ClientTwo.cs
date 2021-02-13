using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace Client.Two
{
    public class ClientTwo
    {
        public int messageCount = 0;
        public UdpState udpStateServer;

        public ClientTwo()
        {
            var serverAddress = IPAddress.Loopback;
            int serverPort = 1000;

            Thread.Sleep(1000);

            var udpClient = new UdpClient(1002);
            udpClient.Connect(serverAddress, serverPort);
            
            SendMessage(udpClient);

            var remoteIpEndPoint = new IPEndPoint(serverAddress, serverPort);
            udpStateServer = new UdpState
            {
                endPoint = remoteIpEndPoint,
                udpClient = udpClient
            };

            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpStateServer);
        }

        public void SendMessage(UdpClient udpClient)
        {
            Thread.Sleep(10000);

            string messageText = "Hello Server! #" + messageCount;
            byte[] sendBytes = Encoding.UTF8.GetBytes(messageText);
            udpClient.Send(sendBytes, sendBytes.Length);
            messageCount++;

            Log("Message sent: " + messageText);
        }

        public void ReceiveCallback(IAsyncResult result)
        {
            UdpClient client = ((UdpState)(result.AsyncState)).udpClient;
            IPEndPoint endPoint = ((UdpState)(result.AsyncState)).endPoint;
            string receivedValue = string.Empty;

            try
            {
                byte[] receivedBytes = client.EndReceive(result, ref endPoint);
                client.BeginReceive(new AsyncCallback(ReceiveCallback), udpStateServer);
                receivedValue = Encoding.UTF8.GetString(receivedBytes);

                Log("Message you received: " + receivedValue.ToString());
                Log("Message was sent from " + endPoint.Address.ToString() + " on port  " + endPoint.Port.ToString());

                SendMessage(client);
            }
            catch (Exception ex)
            { 
                
            }
        }

        public void Log(string message)
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);

            Console.ResetColor();
        }
    }
}
