using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Promix.Financials.Application.Abstractions;
using Promix.Financials.Application.Features.Accounts.Services;
using Promix.Financials.Infrastructure;
using Promix.Financials.Infrastructure.Persistence;
using Promix.Financials.Infrastructure.Persistence.Seeding;
using Promix.Financials.Infrastructure.Security;
using Promix.Financials.UI.Security;
using Promix.Financials.UI.ViewModels.Accounts;
using Promix.Financials.UI.ViewModels.Currencies;
using System;
using Windows.Globalization;
using Windows.Storage;

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
                var crashPath = System.IO.Path.Combine(
    ApplicationData.Current.LocalFolder.Path, "crash_log.txt");
                System.IO.File.WriteAllText(crashPath, msg);
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
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                var cs = config.GetConnectionString("Promix")
                    ?? throw new InvalidOperationException("Missing ConnectionStrings:Promix in appsettings.json");
                services.AddInfrastructure(cs);
                services.AddTransient<CompanyCurrenciesViewModel>();
                services.AddSingleton<ISessionStore, LocalSettingsSessionStore>();
                services.AddTransient<ChartOfAccountsViewModel>();
                services.AddTransient<NewAccountDialogViewModel>();
                
                services.AddTransient<EditAccountDialogViewModel>();
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