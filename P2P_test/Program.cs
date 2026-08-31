using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using P2P_test.Models.Models;
using P2P_test.Models.UDP;
using P2P_test.ViewModels;
using P2P_test.Views;

namespace P2P_test;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder().ConfigureServices((c, s) =>
        {
            
            s.AddSingleton<MainWindow>();
            s.AddSingleton<MainWindowViewModel>();
            
            s.AddSingleton<SettingsViewModel>();
            s.AddSingleton<ChatViewModel>();
                
            s.AddSingleton<Settings>();
            s.AddSingleton<Engine>();

        }).Build();
        BuildAvaloniaApp(host.Services)
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp(IServiceProvider hostServices)
        => AppBuilder.Configure(() => new App(hostServices))
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}