using System;
using P2P_test.Views;

namespace P2P_test.ViewModels;

public class TestViewModel : ViewModelBase
{
    public TestViewModel(IServiceProvider serviceProvider, MainWindow mainWindow)
    {
        _serviceProvider = serviceProvider;
        _mainWindow = mainWindow;
    }
}