using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using P2P_test.Models.Models;
using P2P_test.Models.Navigation;
using P2P_test.Models.UDP;
using P2P_test.Views;

namespace P2P_test.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private MainWindow _currentWindow;
    
    IServiceProvider _serviceProvider;
    private Settings _settings;
    
    [ObservableProperty] 
    private List<NavItem> _navItems = new();
    
    [ObservableProperty] 
    private List<NavItem> _downNavItems = new();
    
    [ObservableProperty]
    private NavItem _selectedNavItem;
    
    [ObservableProperty]
    private NavItem _selectedDownNavItem;
    
    [ObservableProperty]
    private bool _isPaneOpen = true;
    
    [ObservableProperty]
    private ViewModelBase _currentPage;

    [RelayCommand]
    private void OpenSetting()
    {
        SelectedDownNavItem = null;
        SelectedNavItem = null;
        CurrentPage = (ViewModelBase)_serviceProvider.GetService(typeof(ViewModelBase));
    }

    [RelayCommand]
    private void TogglePane() => IsPaneOpen = !IsPaneOpen;

    partial void OnSelectedNavItemChanged(NavItem value)
    {
        if (value is not null)
        {
            CurrentPage = (ViewModelBase)_serviceProvider.GetRequiredService(value.ViewModelType); //(ViewModelBase)Activator.CreateInstance(value.ViewModelType, _serviceProvider, _currentWindow);
            SelectedDownNavItem = null;
        }
    }
    partial void OnSelectedDownNavItemChanged(NavItem value)
    {
        if (value is not null)
        {
            if (value.ViewModelType == typeof(SettingsViewModel))
            {
                CurrentPage = (ViewModelBase)Activator.CreateInstance(value.ViewModelType, _serviceProvider, _currentWindow);
                SelectedNavItem = null;
            }
            else
            {
                CurrentPage = (ViewModelBase)_serviceProvider.GetRequiredService(value.ViewModelType); //(ViewModelBase)Activator.CreateInstance(_serviceProvider, _currentWindow);
                SelectedNavItem = null;
            }
        }
    }

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _settings = serviceProvider.GetRequiredService<Settings>();
        _settings.LoadSettings();
        
        NavItems.Add(new NavItem(1, "Чат", typeof(ChatViewModel)));
        DownNavItems.Add(new NavItem(4, "Настройки", typeof(SettingsViewModel)));
    }

    private void StopEngine(object? sender, EventArgs e)
    {
        _serviceProvider.GetRequiredService<Engine>().Stop();
    }

    public void SetWindow(MainWindow window)
    {
        _currentWindow = window;
        _currentWindow.Closed += StopEngine;
    }
}