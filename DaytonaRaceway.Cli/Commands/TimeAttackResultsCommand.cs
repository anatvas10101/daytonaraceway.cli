using System.ComponentModel;
using DaytonaRaceway.Adapter;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Cli.Output.Html;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DaytonaRaceway.Cli.Commands;

public class TimeAttackResultsCommand : AsyncCommand<TimeAttackResultsCommand.Settings>
{
    public sealed class Settings : RaceCommandSettings, IAgentSettings
    {
        [CommandOption("-l|--default-lap <SECONDS>")]
        [Description("Default lap time (in seconds) to replace a missing result.")]
        [DefaultValue(120)]
        public int DefaultLapTime { get; init; } = 120;

        [CommandOption("-k|--kart-results")]
        [Description("A flag determining whether to include kart results to a results table.")]
        public bool IncludeKartResults { get; init; }

        [CommandOption("-n|--average-from-top-n")]
        [Description("Numner of top results per kart to calculate average kart time.")]
        [DefaultValue(6)]
        public int N { get; init; } = 6;

        [CommandOption("--sourceip <IP>")]
        [Description("IP address of the API host.")]
        [DefaultValue(ApiAgent.DefaultIpHost)]
        public string SourceIp { get; init; } = ApiAgent.DefaultIpHost;

        [CommandOption("--port <PORT>", IsHidden = true)]
        [Description("Port of the API host.")]
        [DefaultValue(ApiAgent.DefaultPort)]
        public int Port { get; init; } = ApiAgent.DefaultPort;
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        using var resultsAdapter = new TimeAttackResultsAdapter(settings);
        var (results, kartResults) =
            await resultsAdapter.GetResults(settings.RaceId, settings.DefaultLapTime * 1000, settings.N, cancellationToken);

        var outputFile = await HtmlOutput.WriteTimeAttackResults(results, kartResults, settings, cancellationToken);
        AnsiConsole.MarkupLine($"Results: [link={outputFile.AbsoluteUri}]{outputFile.AbsoluteUri}[/]");

        return 0;
    }
}
