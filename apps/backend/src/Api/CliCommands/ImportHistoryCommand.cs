using Kcow.Application.Interfaces;

namespace Kcow.Api.CliCommands;

/// <summary>
/// CLI command to show recent import history from the audit log.
/// Usage: dotnet run import history [--count N]
/// </summary>
public static class ImportHistoryCommand
{
    private const string CommandName = "import";
    private const string SubCommandName = "history";

    public static bool IsImportHistoryCommand(string[] args)
    {
        return args.Length >= 2 &&
               args[0].Equals(CommandName, StringComparison.OrdinalIgnoreCase) &&
               args[1].Equals(SubCommandName, StringComparison.OrdinalIgnoreCase);
    }

    public static async Task<int> ExecuteAsync(
        string[] args, IImportAuditLogRepository repository, TextWriter? output = null)
    {
        output ??= Console.Out;

        var count = 10;
        for (int i = 2; i < args.Length; i++)
        {
            if ((args[i] == "--count" || args[i] == "-n") && i + 1 < args.Length)
            {
                if (int.TryParse(args[++i], out var n) && n > 0) count = n;
            }
            else if (args[i] is "--help" or "-h" or "/?")
            {
                output.WriteLine("Usage: dotnet run import history [--count N]");
                output.WriteLine("  -n, --count N   Number of recent imports to show (default: 10)");
                return 0;
            }
        }

        IEnumerable<Kcow.Domain.Entities.ImportAuditLog> logs;
        try
        {
            logs = await repository.GetRecentAsync(count);
        }
        catch (Exception ex)
        {
            output.WriteLine($"Error: Could not read import history: {ex.Message}");
            return 1;
        }
        var logList = logs.ToList();

        output.WriteLine();
        output.WriteLine("=== IMPORT HISTORY ===");
        output.WriteLine();

        if (logList.Count == 0)
        {
            output.WriteLine("No import runs found.");
            return 0;
        }

        output.WriteLine($"{"ID",-5} {"Date",-12} {"Status",-22} {"Created",-10} {"Failed",-8}");
        output.WriteLine(new string('-', 60));

        foreach (var log in logList)
        {
            var date = log.StartedAt.ToString("yyyy-MM-dd");
            var created = log.TotalCreated.ToString();
            var failed = log.TotalFailed > 0 ? log.TotalFailed.ToString() : "-";
            output.WriteLine($"{log.Id,-5} {date,-12} {log.Status,-22} {created,-10} {failed,-8}");
        }

        output.WriteLine();
        return 0;
    }
}
