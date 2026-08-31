using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using P2P_test.Models.Models;

namespace P2P_test.Models.UDP;

public class Engine
{

        public delegate void ChatMessageHandler(Message msg);
        public event ChatMessageHandler OnChatMessage;
        
        public delegate void ClientAdressRecieved(string address);
        public event ClientAdressRecieved OnClientAdressRecieved;
        
        public delegate void SuccessfulConnection(bool success);
        
        public event SuccessfulConnection OnSuccessfulConnection;
    
        private string PeerAddress = "";
        
        private string ClientAddress = "";

        private IPAddress PeerIP;

        private IPEndPoint PeerEndPoint;

        private bool Connected = false;
        
        private bool StopMsg = false;

        private UdpClient client;

        private List<Message> messages = new();

        private STUNConnector connector = new STUNConnector();

        private long KPLPacketNum = 0;

        private Thread ListenerThread;

        private Thread KPLNATThread;

        public Engine()
        {
            var StartThread = new Thread(Start);
            StartThread.Start();
        }

        private void Start()
        {
            ClientAddress = connector.GetNATHole();
            
            OnClientAdressRecieved.Invoke(ClientAddress);

            client = connector.Client;

            KPLNATThread = new Thread(KPLpackets);
            KPLNATThread.Start();

            RequestPeerIP();

            ListenerThread = new Thread(MessageListenerAsync);
            ListenerThread.Start();

            TryConnect();

            KPLNATThread = new Thread(KPLpackets);
            KPLNATThread.Start();
        }
        
        public void KPLpackets()
        {
            if (!Connected)
            {
                while (!Connected&&!StopMsg)
                {
                    SendMessage(MessageType.KeepAlive,"KPL");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                while (!StopMsg)
                {
                    SendMessage(MessageType.KeepAlive,"KPL");
                    Thread.Sleep(1000);
                }
            }
        }
        private void RequestPeerIP()
        {
            while (PeerAddress=="" && !StopMsg)
            {
                Thread.Sleep(500);
            }
            PeerIP = IPAddress.Parse(PeerAddress.Split(':')[0]);
            PeerEndPoint = new IPEndPoint(PeerIP, int.Parse(PeerAddress.Split(':')[1]));
        }

        private void TryConnect()
        {
            while (!Connected && !StopMsg)
            {
                SendMessage(MessageType.Connection,"ConnectionSuccess");
                Thread.Sleep(100);
            }
        }

        private void MessageListenerAsync()
        {
            while (!StopMsg)
            {
                var receiveTask = client.ReceiveAsync();
                receiveTask.Wait();

                var result = receiveTask.Result.Buffer;
                string receivedMessage = Encoding.UTF8.GetString(result);

                try
                {
                    Message receivedMessageObj = JsonSerializer.Deserialize<Message>(result, new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });

                    if (!Connected)
                    {
                        if (receivedMessageObj != null)
                        {
                            if (receivedMessageObj.Type == MessageType.Connection && receivedMessageObj.Text.Equals("ConnectionSuccess"))
                            {
                                Connected = true;
                                OnSuccessfulConnection.Invoke(true);
                            }
                        }
                    }

                    OnChatMessage?.Invoke(receivedMessageObj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void ApplyPeerAddress(string peerAddress)
        {
            PeerAddress = peerAddress;
        }
        
        public void SendMessage(MessageType type, string text)
        {
            uint messageID = GlobalVars.GetNewMessageID();
            //Console.WriteLine($"Отослали {type} {text} на {PeerAddress}");
            byte[] sendBuffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<Message>(new Message(type, text, messageID), new JsonSerializerOptions{TypeInfoResolver = new DefaultJsonTypeInfoResolver()}));
            client.SendAsync(sendBuffer, sendBuffer.Length, PeerEndPoint);
        }
        
        public string GetClientAddress()
        {
            return ClientAddress;
        }
        
        public void Stop()
        {
            SendMessage(MessageType.TextMessage, "Клиент отключился");
            Connected = false;  
            StopMsg = true;
            client.Close();
        }
        
    }