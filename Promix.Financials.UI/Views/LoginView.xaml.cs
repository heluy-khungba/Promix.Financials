using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Promix.Financials.Application.Abstractions;   // ✅ المسار الصحيح
using Promix.Financials.Application.Features.Auth;
using System;

namespace Promix.Financials.UI.Views;

public sealed partial class LoginView : Page
{
    private readonly IAuthService _auth;
    private readonly IUserContextBootstrapper _bootstrapper;
    private readonly ILogger<LoginView> _logger;
    private readonly ISessionStore _sessionStore;
    public LoginView()
    {
        InitializeComponent();

        var services = ((App)Microsoft.UI.Xaml.Application.Current).Services;

        _auth = services.GetRequiredService<IAuthService>();
        _bootstrapper = services.GetRequiredService<IUserContextBootstrapper>();
        _logger = services.GetRequiredService<ILogger<LoginView>>();
        _sessionStore = services.GetRequiredService<ISessionStore>(); // ✅ أضف هذا

        Loaded += LoginView_Loaded;
    }

    private async void LoginView_Loaded(object sender, RoutedEventArgs e)
    {
        var last = await _sessionStore.LoadLastUsernameAsync();
        if (!string.IsNullOrWhiteSpace(last))
            UsernameBox.Text = last;
    }

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;

        if (sender is not Button button)
            return;

        button.IsEnabled = false;
        button.Content = "Signing in...";

        try
        {
            var command = new LoginCommand(
                 UsernameBox.Text?.Trim() ?? string.Empty,
                 PasswordBox.Password ?? string.Empty,
                 RememberMeCheckBox.IsChecked == true
                 );

            var result = await _auth.LoginAsync(command);

            if (!result.Succeeded)
            {
                ErrorText.Text = result.ErrorCode ?? "Login failed";
                return;
            }

            // ✅ تحديث IUserContext من SessionStore بعد Login
            await _bootstrapper.InitializeAsync();

            // ✅ Gate في MainWindow يقرر CompanySelection/Dashboard
            var app = (App)Microsoft.UI.Xaml.Application.Current;
            if (app.CurrentWindow is MainWindow mainWindow)
            {
                mainWindow.RefreshAfterLogin();
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login.");
            ErrorText.Text = "Unexpected error occurred.";
        }
        finally
        {
            button.IsEnabled = true;
            button.Content = "Sign In";
        }
    }
}