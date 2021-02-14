using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public ConcurrentBag<UdpState> clientList = new ConcurrentBag<UdpState>();
        private readonly Timer broadCastTimer;
        
        public MessageServer()
        {
            // Receive a message and write it to the console.
            var endPointServer = new IPEndPoint(IPAddress.Any, listenPort);
            var udpClientServer = new UdpClient(endPointServer);

            UdpState udpStateServer = new UdpState
            {
                endPoint = endPointServer,
                udpClient = udpClientServer
            };

            udpStateServer.udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpStateServer);

            Log("Server is listening...");

            broadCastTimer = new Timer(BroadCastPlayerList, udpClientServer, 10000, 10000);
        }

        public void BroadCastPlayerList(object localUdpClient)
        {
            var udpClientServer = (UdpClient)localUdpClient;

            var list = clientList.ToImmutableList<UdpState>();
            string message = $"Server status: {clientList.Count} Players(s)";

            foreach (UdpState udpStateClient in list)
            {
                SendMessage(udpClientServer, udpStateClient.endPoint, message);
            }
        }

        private void AddClient(UdpState client)
        {
            var list = clientList.ToImmutableList<UdpState>();

            if (!list.Exists(c => c.endPoint.Port == client.endPoint.Port))
                clientList.Add(client);
        }

        public async void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                var udpClient = ((UdpState)(result.AsyncState)).udpClient;
                var endPoint = ((UdpState)(result.AsyncState)).endPoint;

                byte[] receiveBytes = udpClient.EndReceive(result, ref endPoint);

                UdpState udpStateClient = new UdpState
                {
                    endPoint = endPoint,
                    udpClient = udpClient
                };
                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), udpStateClient);

                AddClient(udpStateClient);

                string receiveString = Encoding.UTF8.GetString(receiveBytes);

                Log($"Message from {endPoint.Address}:{endPoint.Port}: " + receiveString, endPoint);

                string messageRespond = $"Hello client, I received: {receiveString}";
                SendMessage(udpClient, endPoint, messageRespond);
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

        private void SendMessage(UdpClient udpClient, IPEndPoint endPoint, string messageRespond)
        {
            messageRespond = $"Message sent: {messageRespond}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(messageRespond);
            udpClient.Send(responseBytes, messageRespond.Length, endPoint);
            Log(messageRespond, endPoint);

            messageCount++;
            messageReceived = true;
        }

        public void Log(string message, IPEndPoint endPoint = null, bool isError = false)
        {
            if (endPoint == null)
                Console.BackgroundColor = ConsoleColor.DarkGreen;
            else if (isError)
                Console.BackgroundColor = ConsoleColor.Red;
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
