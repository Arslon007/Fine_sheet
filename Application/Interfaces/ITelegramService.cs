namespace Application.Interfaces;

public interface ITelegramService
{
    Task SendDailyReportAsync(CancellationToken cancellationToken = default);
    Task SendReportToChatAsync(long chatId, CancellationToken cancellationToken = default);
    Task StartPollingAsync(CancellationToken cancellationToken = default);
    Task SendBackupFileAsync(string json, string fileName, CancellationToken cancellationToken = default);
}
