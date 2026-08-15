using Application.Interfaces;
using DataAccess.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Application.Services;

public class TelegramService : ITelegramService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TelegramBotClient _botClient;
    private readonly List<long> _chatIds;

    public TelegramService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;

        var token = configuration["Telegram:BotToken"]
            ?? throw new InvalidOperationException("Telegram:BotToken not configured in appsettings.json");

        _chatIds = configuration.GetSection("Telegram:ChatIds")
            .Get<List<string>>()
            ?.Select(long.Parse)
            .ToList()
            ?? throw new InvalidOperationException("Telegram:ChatIds not configured in appsettings.json");

        _botClient = new TelegramBotClient(token);
    }

    public async Task StartPollingAsync(CancellationToken cancellationToken = default)
    {
        var offset = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var updates = await _botClient.GetUpdates(offset, timeout: 30, cancellationToken: cancellationToken);

                foreach (var update in updates)
                {
                    offset = update.Id + 1;
                    await HandleUpdateAsync(update, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                await Task.Delay(5000, cancellationToken);
            }
        }
    }

    private async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        long chatId;
        string? text = null;

        if (update.Type == UpdateType.Message && update.Message != null)
        {
            chatId = update.Message.Chat.Id;
            text = update.Message.Text;
        }
        else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
        {
            chatId = update.CallbackQuery.Message!.Chat.Id;
            text = update.CallbackQuery.Data;

            await _botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
        }
        else
        {
            return;
        }

        if (text == "/start")
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: "👋 <b>Assalomu alaykum!</b>\n\nMen Fine Sheet botiman. Har kuni jarimalar va bonuslar haqida hisobot yuboraman.\n\nQuyidagi tugmalarni bosing:",
                parseMode: ParseMode.Html,
                replyMarkup: GetMainKeyboard(),
                cancellationToken: cancellationToken);
        }
        else if (text == "get_today_sheet" || text == "/get_todays_sheet")
        {
            await SendReportToChatAsync(chatId, cancellationToken);
        }
        else if (text == "get_backup")
        {
            await HandleBackupRequestAsync(chatId, cancellationToken);
        }
    }

    public async Task SendReportToChatAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var message = await BuildReportAsync(cancellationToken);

        await _botClient.SendMessage(
            chatId: chatId,
            text: message,
            parseMode: ParseMode.Html,
            replyMarkup: GetMainKeyboard(),
            cancellationToken: cancellationToken);
    }

    public async Task SendDailyReportAsync(CancellationToken cancellationToken = default)
    {
        var message = await BuildReportAsync(cancellationToken);

        foreach (var chatId in _chatIds)
        {
            await _botClient.SendMessage(
                chatId: chatId,
                text: message,
                parseMode: ParseMode.Html,
                replyMarkup: GetMainKeyboard(),
                cancellationToken: cancellationToken);
        }
    }

    private static InlineKeyboardMarkup GetMainKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("📊 Bugungi hisobot", "get_today_sheet"),
                InlineKeyboardButton.WithCallbackData("💾 Bek'ap", "get_backup"),
            }
        });
    }

    private async Task HandleBackupRequestAsync(long chatId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();

        var info = backupService.Save(null);

        var data = backupService.ExportAll();
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var fileName = $"backup_{DateTime.UtcNow:yyyy-MM-dd_HH-mm}.json";

        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);
        var inputFile = InputFile.FromStream(stream, fileName);

        await _botClient.SendDocument(
            chatId: chatId,
            document: inputFile,
            caption: $"💾 <b>Bek'ap saqlandi</b>\n📅 {DateTime.UtcNow:dd.MM.yyyy HH:mm}\n👥 {info.EmployeeCount} xodim | 🔴 {info.FineCount} jarima | 🟢 {info.BonusCount} bonus",
            parseMode: ParseMode.Html,
            replyMarkup: GetMainKeyboard(),
            cancellationToken: cancellationToken);
    }

    public async Task SendBackupFileAsync(string json, string fileName, CancellationToken cancellationToken = default)
    {
        const long backupChatId = 5601098661;

        var bytes = Encoding.UTF8.GetBytes(json);
        using var stream = new MemoryStream(bytes);

        var inputFile = InputFile.FromStream(stream, fileName);

        await _botClient.SendDocument(
            chatId: backupChatId,
            document: inputFile,
            caption: $"💾 <b>Bek'ap saqlandi</b>\n📅 {DateTime.UtcNow:dd.MM.yyyy HH:mm}\n📄 {fileName}",
            parseMode: ParseMode.Html,
            cancellationToken: cancellationToken);
    }

    private async Task<string> BuildReportAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateTime.UtcNow.Date;

        var employees = await context.Employee
            .Include(e => e.Fines)
            .Include(e => e.Bonuses)
            .ToListAsync(cancellationToken);

        var todayFines = employees
            .SelectMany(e => e.Fines.Where(f => f.Date.Date == today)
                .Select(f => new { Employee = e.FullName, f.Reason, f.Amount }))
            .ToList();

        var todayBonuses = employees
            .SelectMany(e => e.Bonuses.Where(b => b.Date.Date == today)
                .Select(b => new { Employee = e.FullName, b.Reason, b.Amount }))
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine($"📊 <b>Kunlik hisobot — {today:dd.MM.yyyy}</b>");
        sb.AppendLine();

        // Jarimalar (Штрафы)
        if (todayFines.Count > 0)
        {
            sb.AppendLine("🔴 <b>Jarimalar:</b>");
            decimal totalFines = 0;
            foreach (var fine in todayFines)
            {
                sb.AppendLine($"  • {fine.Employee} — {fine.Amount:N0} so'm ({fine.Reason})");
                totalFines += fine.Amount;
            }
            sb.AppendLine($"  <b>Jami jarimalar: {totalFines:N0} so'm</b>");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("🔴 <b>Jarimalar:</b> bugun yo'q");
            sb.AppendLine();
        }

        // Bonuslar (Бонусы)
        if (todayBonuses.Count > 0)
        {
            sb.AppendLine("🟢 <b>Bonuslar:</b>");
            decimal totalBonuses = 0;
            foreach (var bonus in todayBonuses)
            {
                sb.AppendLine($"  • {bonus.Employee} — {bonus.Amount:N0} so'm ({bonus.Reason})");
                totalBonuses += bonus.Amount;
            }
            sb.AppendLine($"  <b>Jami bonuslar: {totalBonuses:N0} so'm</b>");
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine("🟢 <b>Bonuslar:</b> bugun yo'q");
            sb.AppendLine();
        }

        // Umumiy statistika
        var totalEmployees = employees.Count;
        var allFines = employees.Sum(e => e.Fines.Sum(f => f.Amount));
        var allBonuses = employees.Sum(e => e.Bonuses.Sum(b => b.Amount));

        sb.AppendLine("📈 <b>Umumiy statistika:</b>");
        sb.AppendLine($"  Xodimlar soni: {totalEmployees}");
        sb.AppendLine($"  Jami jarimalar: {allFines:N0} so'm");
        sb.AppendLine($"  Jami bonuslar: {allBonuses:N0} so'm");
        sb.AppendLine();

        // Har bir xodim bo'yicha jarimalar
        var employeesWithFines = employees
            .Where(e => e.Fines.Any())
            .OrderByDescending(e => e.Fines.Sum(f => f.Amount))
            .ToList();

        if (employeesWithFines.Count > 0)
        {
            sb.AppendLine("🔴 <b>Xodimlar bo'yicha jarimalar:</b>");
            foreach (var emp in employeesWithFines)
            {
                var empTotal = emp.Fines.Sum(f => f.Amount);
                var fineCount = emp.Fines.Count;
                sb.AppendLine($"  • {emp.FullName} — {empTotal:N0} so'm ({fineCount} ta jarima)");
            }
        }

        return sb.ToString();
    }
}
