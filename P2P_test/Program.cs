using Avalonia;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            s.AddTransient<MainWindow>();
            s.AddTransient<MainWindowViewModel>();

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