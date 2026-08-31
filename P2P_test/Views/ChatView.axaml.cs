using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using P2P_test.ViewModels;

namespace P2P_test.Views;

public partial class ChatView : UserControl
{
    private ChatViewModel vm;
    public ChatView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, EventArgs e)
    {
        vm = DataContext as ChatViewModel;
    }

    private void OnEnterPressed(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            vm.SendMessageCommand.Execute(null);
        }
    }

    private void CopyAddress(object? sender, RoutedEventArgs e)
    {
        var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
        if (clipboard != null)
        {
            clipboard.SetTextAsync(ClientAddressTextBlock.Text).Wait();
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        vm.StartConnectionCommand.Execute(null);
    }
}
public class MessageGridConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isMe = value is bool b && b;
        return isMe ? 1 : 0;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
public class MessageAligmentConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isMe = value is bool b && b;
        return isMe ? HorizontalAlignment.Right : HorizontalAlignment.Left;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}
public class MessageBackgroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isMe = value is bool b && b;
        var resultColor = isMe ? new SolidColorBrush(Colors.CornflowerBlue) : new SolidColorBrush(Colors.DarkGray);
        return resultColor;
    }
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}