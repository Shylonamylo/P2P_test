using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using P2P_test.Models;
using P2P_test.Models.Models;
using P2P_test.Models.UDP;
using P2P_test.Views;

namespace P2P_test.ViewModels;

public partial class ChatViewModel : ViewModelBase
{
    [ObservableProperty] private string _messageText;
    [ObservableProperty] private ObservableCollection<ChatMessage> _chatMessages = new();
    [ObservableProperty] private string _clientAddress;
    [ObservableProperty] private string _peerAddress = "";
    [ObservableProperty] private ChatMessage _selectedMessage;
    
    private Engine _engine;

    [RelayCommand]
    private void StartConnection()
    {
        if(PeerAddress.Split(':').Length==2 && IPAddress.TryParse(PeerAddress.Split(':')[0], out _) && PeerAddress.Split(':')[1].Length > 2 && PeerAddress.Split(':')[1].Length < 6)
        {
            _engine.ApplyPeerAddress(PeerAddress);
        }
        else
        {
            MessageBoxManager
                .GetMessageBoxStandard("Ошибка", "Введенный адрес клиента не валидный", ButtonEnum.Ok, Icon.Error)
                .ShowWindowAsync();
        }
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (MessageText != "")
        {
            _engine.SendMessage(MessageType.TextMessage, _messageText);
            var chatMessage = new ChatMessage(_messageText, true); 
            ChatMessages.Add(chatMessage);
            MessageText = "";
            SelectedMessage = ChatMessages[^1];
        }
    }

    private void DisplayMessage(Message message)
    {
        if (message.Type == MessageType.TextMessage)
        {
            var chatMessage = new ChatMessage(message.Text, false); 
            ChatMessages.Add(chatMessage);
            
            SelectedMessage = ChatMessages[^1];
        }
    }
    
    public ChatViewModel(IServiceProvider serviceProvider, MainWindow mainWindow)
    {
        _serviceProvider = serviceProvider;
        _mainWindow = mainWindow;
        _engine = _serviceProvider.GetRequiredService<Engine>();
        _engine.OnChatMessage += DisplayMessage;
        _engine.OnClientAddressReceived += (string address) =>
        {
            ClientAddress = address;
        };
        _engine.OnSuccessfulConnection += DisplaySuccessConnectionWindow;
    }

    public void DisplaySuccessConnectionWindow(bool success)
    {
        DisplayMessage(new Message(MessageType.TextMessage, "Вы успешно подключились", GlobalVars.GetNewMessageID()));
    }
    
}