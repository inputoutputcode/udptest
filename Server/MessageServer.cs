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
        public bool messageSent = false;
        public int messageCount = 0;

        public MessageServer()
        {
            // Receive a message and write it to the console.
            var udpStateServer = GetServerUdpState();

            udpStateServer.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpStateServer);

            Log("Server is listening...");
        }

        private UdpState GetServerUdpState()
        {
            var endPointServer = new IPEndPoint(IPAddress.Any, listenPort);
            var udpClientServer = new UdpClient(endPointServer);

            UdpState udpStateServer = new UdpState
            {
                endPoint = endPointServer,
                udpClient = udpClientServer
            };

            return udpStateServer;
        }

        public async void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                var udpClient = ((UdpState)(result.AsyncState)).udpClient;
                var endPoint = ((UdpState)(result.AsyncState)).endPoint;

                byte[] receiveBytes = udpClient.EndReceive(result, ref endPoint);

                UdpState udpStateServer = new UdpState
                {
                    endPoint = endPoint,
                    udpClient = udpClient
                };
                udpStateServer.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpStateServer);

                string receiveString = Encoding.UTF8.GetString(receiveBytes);

                Log($"Message received: {receiveString}", endPoint);

                SendMessage(udpClient, endPoint, receiveString);
            }
            catch (ArgumentNullException ex)
            {
                Log("asyncResult is null");
            }
            catch (ArgumentException ex)
            {
                Log("asyncResult was not return by BeginReceive");
            }
            catch (SocketException ex)
            {
                Log("An error occurred when attempting to access the underlying Socket.");
            }
            catch (ObjectDisposedException ex)
            {
                Log("The underlying Socket has been closed.");
            }
            catch (InvalidOperationException ex)
            {
                Log("BeginReceive was previously called for the asynchronous read.");
            }
            catch (Exception ex)
            {
                Log("ReceiveCallback unknown exception. Messsage: " + ex.Message );
            }
        }

        private void SendMessage(UdpClient udpClient, IPEndPoint endPoint, string receiveString)
        {
            string messageRespond = $"Hello client, I receveived: {receiveString}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(messageRespond);

            // TODO: use End for broadcast
            udpClient.Send(responseBytes, messageRespond.Length, endPoint);

            Log(messageRespond, endPoint);

            messageCount++;
            messageReceived = true;
        }
        public void Log(string message, IPEndPoint endPoint = null)
        {
            if (endPoint == null)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else if (endPoint.Port == 1002)
                Console.BackgroundColor = ConsoleColor.Gray;
            else
                Console.BackgroundColor = ConsoleColor.DarkGray;

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(message);

            Console.ResetColor();
        }
    }
}
