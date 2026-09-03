using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using P2P_test.Models.Models;

namespace P2P_test.Models.UDP;

public class Engine
{

        public delegate void ChatMessageHandler(Message msg);
        public event ChatMessageHandler OnChatMessage;
        
        public delegate void ClientAddressReceived(string address);
        public event ClientAddressReceived OnClientAddressReceived;
        
        public delegate void SuccessfulConnection(bool success);
        
        public event SuccessfulConnection OnSuccessfulConnection;
    
        private string _peerAddress = "";
        
        private string? _clientAddress = "";

        private IPAddress _peerIp;

        private IPEndPoint _peerEndPoint;

        private bool _connected;
        
        private bool _stopMsg;

        private UdpClient _client;

        private readonly List<Message> _messagesBuffer = new();

        private readonly HashSet<uint> _messagesHistory = new();

        private readonly STUNConnector _connector = new STUNConnector();

        public long KplPacketNum = 0;

        private Thread _listenerThread;

        private Thread _kplnatThread;
        private Thread _messagesBufferWorker;

        private List<(MessageType, uint, int, List<Message>)> _fragmentsBuffer = new();

        public Engine()
        {
            var startThread = new Thread(Start);
            startThread.Start();
        }

        private void Start()
        {
            _clientAddress = _connector.GetNATHole();

            if (_clientAddress != null) OnClientAddressReceived.Invoke(_clientAddress);

            _client = _connector.Client;

            _kplnatThread = new Thread(KPLpackets);
            _kplnatThread.Start();

            RequestPeerIP();

            _listenerThread = new Thread(MessageListenerAsync);
            _listenerThread.Start();

            TryConnect();

            _kplnatThread = new Thread(KPLpackets);
            _kplnatThread.Start();

            _messagesBufferWorker = new Thread(CheckMessageLifeTime);
            _messagesBufferWorker.Start();
        }

        private void CheckMessageLifeTime()
        {
            while (!_stopMsg)
            {
                foreach (var message in _messagesBuffer.ToList())
                {
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > message.SendTime+500)
                    {
                        _messagesBuffer.Remove(message);
                        SendMessage(MessageType.TextMessage, message.Text);
                    }
                }
                Thread.Sleep(500);
            }
        }
        public void KPLpackets()
        {
            if (!_connected)
            {
                while (!_connected&&!_stopMsg)
                {
                    SendMessage(MessageType.KeepAlive,"KPL");
                    Thread.Sleep(1000);
                }
            }
            else
            {
                while (!_stopMsg)
                {
                    SendMessage(MessageType.KeepAlive,"KPL");
                    Thread.Sleep(1000);
                }
            }
        }
        private void RequestPeerIP()
        {
            while (_peerAddress=="" && !_stopMsg)
            {
                Thread.Sleep(500);
            }
            _peerIp = IPAddress.Parse(_peerAddress.Split(':')[0]);
            _peerEndPoint = new IPEndPoint(_peerIp, int.Parse(_peerAddress.Split(':')[1]));
        }

        private void TryConnect()
        {
            while (!_connected && !_stopMsg)
            {
                SendMessage(MessageType.Connection,"ConnectionSuccess");
                Thread.Sleep(100);
            }
        }

        private void MessageListenerAsync()
        {
            while (!_stopMsg)
            {
                var receiveTask = _client.ReceiveAsync();
                receiveTask.Wait();

                var result = receiveTask.Result.Buffer;

                try
                {
                    Message? receivedMessageObj = JsonSerializer.Deserialize<Message>(result, new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
    
                    if (receivedMessageObj != null)
                    {
                        if (!_connected)
                        {
                            if (receivedMessageObj is { Type: MessageType.Connection, Text: "ConnectionSuccess" })
                            {
                                _connected = true;
                                OnSuccessfulConnection.Invoke(true);
                                continue;
                            }
                        }

                        if (receivedMessageObj.Type == MessageType.FragmentInit)
                        {
                            if(_fragmentsBuffer.Exists(frag => frag.Item2 == receivedMessageObj.PackageId)) return;
                            var fragmentsCount = int.Parse(receivedMessageObj.Text.Split(',')[1]);
                            Logger.Log($"Получен {receivedMessageObj.Type} - {receivedMessageObj.PackageId} - {fragmentsCount}");
                            MessageType.TryParse<MessageType>(receivedMessageObj.Text.Split(',')[0], out var framentationResultMessageType);
                            _fragmentsBuffer.Add((framentationResultMessageType, receivedMessageObj.PackageId, fragmentsCount, new List<Message>()));
                            
                            continue;
                        }
                        
                        if (receivedMessageObj.Type == MessageType.Fragment)
                        {
                            var fragmentInitPackageId = int.Parse(receivedMessageObj.Text.Split(',')[1]);
                            var cortageOfFragmentsThisInitPackage = _fragmentsBuffer
                                .Where(frag => frag.Item2 == fragmentInitPackageId).GetEnumerator().Current;
                            var listOfFragmentsThisInitPackage = cortageOfFragmentsThisInitPackage.Item4;
                            if(listOfFragmentsThisInitPackage.Exists(m => m.PackageId==receivedMessageObj.PackageId)) return;
                            listOfFragmentsThisInitPackage.Add(receivedMessageObj);
                            Logger.Log($"Получен {receivedMessageObj.Type} - {receivedMessageObj.PackageId}, инициализирован пакетом {fragmentInitPackageId}");
                            
                            if (listOfFragmentsThisInitPackage.Count == cortageOfFragmentsThisInitPackage.Item3)
                            {
                                var orderedListOfFragmentsThisInitPackage = listOfFragmentsThisInitPackage.OrderBy(message => message.PackageId).ToList();
                                Logger.Log("Все фрагменты получены, инициализирована сборка сообщения");
                                StartBuildFragmentedMessage(cortageOfFragmentsThisInitPackage.Item1, orderedListOfFragmentsThisInitPackage);
                            }
                            
                            continue;
                        }

                        if (receivedMessageObj.Type == MessageType.KeepAlive)
                        {
                            Logger.Log($"Получен {receivedMessageObj.Type} - {receivedMessageObj.PackageId}");
                            continue;
                        }

                        if (receivedMessageObj.Type == MessageType.Acknowledge)
                        {
                            uint.TryParse(receivedMessageObj.Text, out uint packageId);
                            Logger.Log($"Получен {receivedMessageObj.Type} - {receivedMessageObj.PackageId} на {packageId}");
                            if (!_messagesHistory.Add(packageId))
                            {
                                continue;
                            }
                            Message? messageWithThisId = _messagesBuffer.Find(m=>m.PackageId == packageId);
                            if (messageWithThisId != null)
                            {
                                _messagesBuffer.Remove(messageWithThisId);
                                Logger.Log($"Сообщение {messageWithThisId.PackageId} удалено из буфера ожидания");
                            }
                            
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                    
                    SendMessage(MessageType.Acknowledge, receivedMessageObj.PackageId.ToString());
                    
                    Logger.Log($"Получено {receivedMessageObj.Type} - {receivedMessageObj.PackageId} - '{receivedMessageObj.Text}'");

                    if (_messagesHistory.Add(receivedMessageObj.PackageId)) OnChatMessage?.Invoke(receivedMessageObj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void StartBuildFragmentedMessage(MessageType resultingMessageType, List<Message> fragments)
        {
            var fragmentStringBuilder = new StringBuilder();

            foreach (var fragment in fragments)
            {
                fragmentStringBuilder.Append(fragment.Text);
            }
            
            Message resultingMessage = new Message(resultingMessageType, fragmentStringBuilder.ToString(), GlobalVars.GetNewMessageID());
            
            Logger.Log($"Фрагментированное особщение успешно собрано, ID - {resultingMessage.PackageId}, количество фрагментов {fragments.Count}");
            
            if (_messagesHistory.Add(resultingMessage.PackageId)) OnChatMessage?.Invoke(resultingMessage);
        }

        public void ApplyPeerAddress(string peerAddress)
        {
            _peerAddress = peerAddress;
        }
        
        public void SendMessage(MessageType type, string text)
        {
            uint messageId = GlobalVars.GetNewMessageID();
            
            var message = new Message(type, text, messageId);

            SendMessage(message);
        }
        
        public void SendMessage(Message message)
        {
            var serializedMessage = JsonSerializer.Serialize(message,
                new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
            
            byte[] sendBuffer = Encoding.UTF8.GetBytes(serializedMessage);

            if (sendBuffer.Length > 1200)
            {
                Logger.Log($"Инициализирована фрагментация сообщения, размер исходного сообщения {sendBuffer.Length}");
                FragmentationSendMessage(message.Type, message.Text);
                
                return;
            }
            
            _client.SendAsync(sendBuffer, sendBuffer.Length, _peerEndPoint);
            
            Logger.Log($"Отправлено {message.Type} - {message.PackageId} - '{message.Text}'");
            
            if(message.Type is MessageType.KeepAlive or MessageType.Acknowledge or MessageType.Connection) return;
            
            _messagesBuffer.Add(message);
            
            Logger.Log($"{message.Type} - {message.PackageId} - '{message.Text}' отправлено в буфер ожидания");
        }

        private void FragmentationSendMessage(MessageType type, string text)
        {
            var fragments = text.Chunk(100).Select(ch => new string(ch)).ToArray();
            
            var initMessage = new Message(MessageType.FragmentInit, $"{type}, {fragments.Length}", GlobalVars.GetNewMessageID());
            
            SendMessage(initMessage);
            
            var initMessageId = initMessage.PackageId;
            foreach (var fragment in fragments)
            {
                var fragmentMessage = new Message(MessageType.Fragment, $"{fragment}, {initMessageId}", GlobalVars.GetNewMessageID());
                SendMessage(fragmentMessage);
            }
            
        }

        public string? GetClientAddress()
        {
            return _clientAddress;
        }
        
        public void Stop()
        {
            SendMessage(MessageType.TextMessage, "Клиент отключился");
            _connected = false;  
            _stopMsg = true;
            _client.Close();
            Logger.Log($"Клиент отключился");
        }
        
    }