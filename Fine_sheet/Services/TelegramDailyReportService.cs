using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Fine_sheet.Services;

public class TelegramDailyReportService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TelegramDailyReportService> _logger;

    public TelegramDailyReportService(
        IServiceProvider serviceProvider,
        ILogger<TelegramDailyReportService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Telegram Bot Service started.");

        var telegramService = _serviceProvider.GetRequiredService<ITelegramService>();

        // Запускаем polling (слушание кнопок) в фоне
        var pollingTask = Task.Run(() => telegramService.StartPollingAsync(stoppingToken), stoppingToken);

        // Запускаем ежедневные отчёты
        var dailyTask = Task.Run(() => RunDailyReportsAsync(telegramService, stoppingToken), stoppingToken);

        await Task.WhenAll(pollingTask, dailyTask);
    }

    private async Task RunDailyReportsAsync(ITelegramService telegramService, CancellationToken stoppingToken)
    {
        // Отправляем отчёт при запуске
        await SendReportAsync(telegramService, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            // Следующая отправка в 18:00 UTC+5 (13:00 UTC)
            var nextRun = now.Date.AddHours(13);
            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("Keyingi hisobot: {NextRun} ({Delay} qoldi)", nextRun, delay);

            await Task.Delay(delay, stoppingToken);
            await SendReportAsync(telegramService, stoppingToken);
        }
    }

    private async Task SendReportAsync(ITelegramService telegramService, CancellationToken stoppingToken)
    {
        try
        {
            await telegramService.SendDailyReportAsync(stoppingToken);
            _logger.LogInformation("Kunlik Telegram hisobot muvaffaqiyatli yuborildi.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Telegram hisobotni yuborishda xatolik.");
        }
    }
}
