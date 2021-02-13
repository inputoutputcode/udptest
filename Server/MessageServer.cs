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

        public UdpClient udpClientServer;
        public IPEndPoint endPointServer;

        public MessageServer()
        {
            // Receive a message and write it to the console.
            endPointServer = new IPEndPoint(IPAddress.Any, listenPort);
            udpClientServer = new UdpClient(endPointServer);

            ReceiveMessages();
        }

        public async void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                var udpClient = ((UdpState)(result.AsyncState)).udpClient;
                var endPoint = ((UdpState)(result.AsyncState)).endPoint;

                byte[] receiveBytes = udpClient.EndReceive(result, ref endPoint);
                string receiveString = Encoding.UTF8.GetString(receiveBytes);

                Console.WriteLine($"Message received: {receiveString}");


                string messageRespond = $"Hello client, server received your message: {receiveString}";
                byte[] responseBytes = Encoding.UTF8.GetBytes(messageRespond);

                // TODO: use End for broadcast
                await udpClient.SendAsync(responseBytes, messageRespond.Length, endPoint);
                udpClient.EndSend += SendCallback;

                messageReceived = true;
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("asyncResult is null");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("asyncResult was not return by BeginReceive");
            }
            catch (SocketException ex)
            {
                Console.WriteLine("An error occurred when attempting to access the underlying Socket.");
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine("The underlying Socket has been closed.");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("BeginReceive was previously called for the asynchronous read.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReceiveCallback unknown exception. Messsage: " + ex.Message );
            }
        }

        public void ReceiveMessages()
        {
            UdpState udpState = new UdpState
            {
                endPoint = endPointServer,
                udpClient = udpClientServer
            };

            Console.WriteLine("listening for messages");
            udpClientServer.BeginReceive(new AsyncCallback(ReceiveCallback), udpState);

            Console.WriteLine("Server is listening...");
        }

        public void SendCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)ar.AsyncState;

            Console.WriteLine($"number of bytes sent: {u.EndSend(ar)}");
            messageSent = true;
        }
    }
}
