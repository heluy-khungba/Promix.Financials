using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Infrastructure;
using Promix.Financials.UI.Security;
using Promix.Financials.UI.ViewModels.Accounts;
using System;
using Windows.Globalization;
using Windows.Storage;
using Promix.Financials.Infrastructure.Persistence;
using Promix.Financials.Infrastructure.Persistence.Seeding;

namespace Promix.Financials.UI;

public partial class App : Microsoft.UI.Xaml.Application
{
    private Window? _window;
    private readonly IHost _host;
    public IServiceProvider Services => _host.Services;
    public Window? CurrentWindow => _window;

    public App()
    {
        InitializeComponent();

        // ✅ UnhandledException في Constructor — المكان الصحيح
        UnhandledException += (sender, e) =>
        {
            var msg = $"[CRASH] {e.Exception?.GetType()?.FullName}\n" +
                      $"Message: {e.Exception?.Message}\n" +
                      $"Inner: {e.Exception?.InnerException?.Message}\n" +
                      $"Stack: {e.Exception?.StackTrace}";

            System.Diagnostics.Debug.WriteLine(msg);

            try
            {
                System.IO.File.WriteAllText(
                    @"C:\Users\mdien\Desktop\crash_log.txt", msg);
            }
            catch { /* تجاهل إذا فشل الكتابة */ }

            e.Handled = true;
        };

        var savedLang = ApplicationData.Current.LocalSettings.Values["AppLanguage"] as string;
        if (!string.IsNullOrWhiteSpace(savedLang))
            ApplicationLanguages.PrimaryLanguageOverride = savedLang;

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                var cs = "Server=.\\MSSQLSERVER2025;Database=PromixFinancials;Trusted_Connection=True;TrustServerCertificate=True;";

                services.AddInfrastructure(cs);

                services.AddSingleton<ISessionStore, LocalSettingsSessionStore>();

                services.AddTransient<ChartOfAccountsViewModel>();
                services.AddTransient<NewAccountDialogViewModel>();
            })
            .Build();
    }

    protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<PromixDbContext>();
                var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
                await SeedData.EnsureSeedAsync(db, hasher);
            }

            var bootstrapper = Services.GetRequiredService<IUserContextBootstrapper>();
            await bootstrapper.InitializeAsync();

            _window = new MainWindow();

            var lang = ApplicationLanguages.PrimaryLanguageOverride;
            if (_window.Content is FrameworkElement fe)
            {
                fe.FlowDirection = (!string.IsNullOrWhiteSpace(lang) && lang.StartsWith("ar"))
                    ? FlowDirection.RightToLeft
                    : FlowDirection.LeftToRight;
            }

            _window.Activate();

            if (_window is MainWindow mainWindow)
                mainWindow.RefreshAfterLogin();
        }
        catch (Exception ex)
        {
            // ✅ عرض خطأ واضح عند فشل التشغيل
            _window = new Window();
            _window.Activate();

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "خطأ في التشغيل",
                Content = $"فشل تشغيل التطبيق:\n\n{ex.Message}\n\nتحقق من اتصال قاعدة البيانات.",
                CloseButtonText = "إغلاق",
            };

            _window.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    dialog.XamlRoot = _window.Content?.XamlRoot;
                    await dialog.ShowAsync();
                }
                finally
                {
                    _window.Close();
                }
            });
        }
    }
}