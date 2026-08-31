using System;
using Microsoft.Extensions.DependencyInjection;
using P2P_test.Views;

namespace P2P_test.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    
    public SettingsViewModel(IServiceProvider serviceProvider, MainWindow mainWindow)
    {
        _serviceProvider = serviceProvider;
        _mainWindow = mainWindow;
    }
}