using Avalonia.Controls;
using P2P_test.ViewModels;

namespace P2P_test.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        DataContext = mainWindowViewModel;
        InitializeComponent();
        mainWindowViewModel.SetWindow(this);
    }
}