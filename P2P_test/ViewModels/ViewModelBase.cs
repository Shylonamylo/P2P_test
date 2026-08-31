using System;
using CommunityToolkit.Mvvm.ComponentModel;
using P2P_test.Models.Models;
using P2P_test.Views;

namespace P2P_test.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected IServiceProvider _serviceProvider;
    
    protected Settings _settings;
    
    protected MainWindow _mainWindow;
}