using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2P_test.Models.UDP;

public class STUNConnector
{
        string[] STUNServers = ["stun.l.google.com:19302", "stun1.l.google.com:19302", "stun2.l.google.com:19302", "stun3.l.google.com:19302"];
        public UdpClient Client { get; private set; } = new();
        public bool Connected { get; private set; }
        public IPEndPoint ServerEndPoint { get; private set; }
        public STUNConnector()
        {
            StartAsync();
        }
        private async Task StartAsync()
        {
            foreach (string Server in STUNServers)
            {
                try
                {
                    var hostnamePort = Server.Split(':');

                    byte[] data = { 0x00, 0x01, 0x00, 0x00, 0x21, 0x12, 0xA4, 0x42, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xF2, 0xF1, 0xFF, 0xF1, 0x1F, 0x12, 0x4F };

                    IPAddress? iPAddress = await GetIPByHostnameAsync(hostnamePort[0]);
                    
                    if (iPAddress == null)
                    {
                        continue;
                    }

                    int port = 3478;

                    try
                    {
                         port = int.Parse(hostnamePort[1]);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);

                    using var cts1 = new CancellationTokenSource(500);

                    var sendTask = Client.SendAsync(data, data.Length, iPEndPoint);
                    await sendTask.WaitAsync(cts1.Token);
                    

                    var ReceiveEndPoint = (IPEndPoint)Client.Client.RemoteEndPoint;

                    using var cts2 = new CancellationTokenSource(500);

                    var ReceiveTask = Client.ReceiveAsync();
                    await ReceiveTask.WaitAsync(cts2.Token);

                    var RTResult = ReceiveTask.Result;

                    var STUNData = RTResult.Buffer;

                    if (STUNData[0] == 1 && STUNData[1] == 1)
                    {
                        Connected = true;
                        ServerEndPoint = iPEndPoint;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine(Server);
                    Client = new();
                    continue;
                }
            }
        }
        public string? GetNATHole()
        {
            while (!Connected)
            {
                Thread.Sleep(100);
            }
            string result = "";

            byte[] data = { 0x00, 0x01, 0x00, 0x00, 0x21, 0x12, 0xA4, 0x42, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xF2, 0xF1, 0xFF, 0xF1, 0x1F, 0x12, 0x4F};

            Client.Send(data, ServerEndPoint);

            var ReceiveEndPoint = (IPEndPoint)Client.Client.RemoteEndPoint;

            try
            {
                var STUNData = Client.Receive(ref ReceiveEndPoint);

                byte[] decryptedSTUN = XORDecryptIPv4(STUNData);

                StringBuilder sb = new();

                for(int i = 2; i<6; i++)
                {
                    sb.Append(decryptedSTUN[i]);
                    if (i == 5)
                    {
                        sb.Append(':');
                        break;
                    }
                    sb.Append('.');
                }

                int port = decryptedSTUN[0] * 256 + decryptedSTUN[1];

                sb.Append(port);

                result = sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result;
        }

        private async Task<IPAddress?>? GetIPByHostnameAsync(string hostname)
        {
            try
            {
                using var cts = new CancellationTokenSource(1000);

                var addrList = await Dns.GetHostAddressesAsync(hostname, cts.Token);
                
                return addrList[0];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private byte[] XORDecryptIPv4(byte[] data)
        {
            byte[] decryptKey = { 0x21, 0x12, 0x21, 0x12, 0xA4, 0x42 };
            byte[] decryptedData = new byte[6];

            for(int i = 0; i<6; i++)
            {
                decryptedData[i] = (byte)(data[26 + i] ^ decryptKey[i]);
            }

            return decryptedData;
        }
    }