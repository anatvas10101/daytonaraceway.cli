using System.ComponentModel;
using DaytonaRaceway.Adapter;
using DaytonaRaceway.Adapter.Models;
using DaytonaRaceway.Agent;
using DaytonaRaceway.Cli.Output;
using DaytonaRaceway.Cli.Output.Csv;
using DaytonaRaceway.Cli.Output.Html;
using Spectre.Console;
using Spectre.Console.Cli;
using AnsiConsoleOutput = DaytonaRaceway.Cli.Output.Console.AnsiConsoleOutput;

namespace DaytonaRaceway.Cli.Commands;

public sealed class RaceResultsCommand : AsyncCommand<RaceResultsCommand.Settings>
{
    public sealed class Settings : RaceCommandSettings, IAgentSettings
    {
        [CommandOption("-s|--stage <STAGE_ID>")]
        [Description("Stage ID. If omitted, results are aggregated across all stages of a command type.")]
        public int? Stage { get; init; }

        [CommandOption("--finals-order <ORDER>")]
        [Description("Ordering of heats in final stage. Asc => A,B,C...; Desc => ...C,B,A. Used for points distribution for final stage.")]
        [DefaultValue(FinalStageHeatsOrdering.Asc)]
        public FinalStageHeatsOrdering FinalHeatsOrdering { get; init; } = FinalStageHeatsOrdering.Asc;

        [CommandOption("-f|--format <FORMAT>")]
        [Description("Output format for the results. Possible values are: Console, Html, Csv.")]
        [DefaultValue(OutputFormat.Console)]
        public OutputFormat Format { get; init; } = OutputFormat.Console;

        [CommandOption("--sourceip <IP>")]
        [Description("IP address of the API host.")]
        [DefaultValue(ApiAgent.DefaultIpHost)]
        public string SourceIp { get; init; } = ApiAgent.DefaultIpHost;

        [CommandOption("--port <PORT>", IsHidden = true)]
        [Description("Port of the API host.")]
        [DefaultValue(ApiAgent.DefaultPort)]
        public int Port { get; init; } = ApiAgent.DefaultPort;
    }

    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        Settings settings,
        CancellationToken cancellationToken)
    {
        var handlingTask = settings.Stage.HasValue
            ? GetStageResults(settings.RaceId, settings.Stage.Value, settings, cancellationToken)
            : GetTotalResults(settings.RaceId, settings, cancellationToken);

        await handlingTask;

        return 0;
    }

    private static async Task GetStageResults(
        Guid raceId,
        int stage,
        Settings settings,
        CancellationToken cancellationToken)
    {
        using var resultsAdapter = new RaceResultsAdapter(settings);
        var (stageName, results) =
            await resultsAdapter.GetRaceStageResults(raceId, stage, settings.FinalHeatsOrdering, cancellationToken);
        var stageForOutput = stageName ?? stage.ToString();

        var outputFile = settings.Format switch
        {
            OutputFormat.Csv =>
                await CsvOutput.WriteRaceStageResults(raceId, stageForOutput, results, cancellationToken),
            OutputFormat.Html =>
                await HtmlOutput.WriteRaceStageResults(raceId, stageForOutput, results, cancellationToken),
            _ => null,
        };

        if (outputFile != null)
        {
            AnsiConsole.MarkupLine($"Results: [link={outputFile.AbsoluteUri}]{outputFile.AbsoluteUri}[/]");
        }
        else
        {
            AnsiConsoleOutput.PrintRaceStageResults(raceId, stageForOutput, results);
        }
    }

    private static async Task GetTotalResults(
        Guid raceId,
        Settings settings,
        CancellationToken cancellationToken)
    {
        using var resultsAdapter = new TotalResultsAdapter(settings);
        var results = await resultsAdapter.GetResults(raceId, settings.FinalHeatsOrdering, cancellationToken);
        var orderedResults = results
            .OrderByDescending(result =>
                result.ResultsPerStage.Sum(v => v.Value.TotalPoints) + (result.QualificationExtraPoints ?? 0))
            .ThenBy(result => result.ResultsPerStage.Sum(v => v.Value.PenaltyPoints))
            .ThenBy(result => result.ResultsPerStage.Last().Value.Position)
            .ToArray();

        var outputFile = await HtmlOutput.WriteTotalResults(raceId, orderedResults, cancellationToken);
        AnsiConsole.MarkupLine($"Results: [link={outputFile.AbsoluteUri}]{outputFile.AbsoluteUri}[/]");
    }
}
