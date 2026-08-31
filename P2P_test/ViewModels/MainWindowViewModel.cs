using CommunityToolkit.Mvvm.ComponentModel;

namespace P2P_test.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string _greeting = "Здарова зайбал";
}