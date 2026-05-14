using KostinShop.Data;
using KostinShop.Views;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Windows;

namespace KostinShop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandledException;

        if (!TryInitializeDatabase(out var dbError))
        {
            MessageBox.Show(
                $"Не удалось подключиться к базе данных.\n\n{dbError}\n\n" +
                "Проверьте строку подключения в файле appsettings.json.",
                "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        new LoginWindow().Show();
    }

    private static void OnUnhandledException(
        object sender,
        System.Windows.Threading.DispatcherUnhandledExceptionEventArgs args)
    {
        var ex = args.Exception;
        var sb = new StringBuilder();
        sb.AppendLine("Непредвиденная ошибка:");
        sb.AppendLine(ex.Message);

        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            sb.AppendLine($"\n[Причина] {inner.Message}");

        sb.AppendLine("\n=== StackTrace ===");
        sb.AppendLine(ex.StackTrace);

        MessageBox.Show(sb.ToString(), $"Ошибка: {ex.GetType().Name}",
            MessageBoxButton.OK, MessageBoxImage.Error);

        args.Handled = true;
    }

    private static bool TryInitializeDatabase(out string error)
    {
        error = string.Empty;
        try
        {
            var connStr = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build()
                .GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "Строка подключения 'DefaultConnection' не найдена в appsettings.json");

            DbContextFactory.Initialize(connStr);
            DbSeeder.CheckConnectionAndSeed();
            return true;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.InnerException?.Message
                     ?? ex.InnerException?.Message;
            error = inner is null ? ex.Message : $"{ex.Message}\n\nПодробности: {inner}";
            return false;
        }
    }
}
