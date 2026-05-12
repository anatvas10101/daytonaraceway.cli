using System.ComponentModel;
using DaytonaRaceway.Adapter;
using Spectre.Console;
using Spectre.Console.Cli;
using AnsiConsoleOutput = DaytonaRaceway.Cli.Output.Console.AnsiConsoleOutput;

namespace DaytonaRaceway.Cli.Commands;

public sealed class PointsScaleCommand : Command<PointsScaleCommand.Settings>
{
    public enum ScaleType { Stage, Final, Championship }

    public sealed class Settings : CommandSettings
    {
        [CommandOption("-c|--count <COUNT>", isRequired: true)]
        [Description("Number of participants for which the points scale is calculated. Must be at least 3.")]
        public int Count { get; init; }

        [CommandOption("-t|--type <TYPE>", isRequired: true)]
        [Description("Points scale. Possible values are: Stage, Final, or Championship.")]
        public ScaleType? Type { get; init; }

        public override ValidationResult Validate()
            => Count switch
            {
                < 6 when Type == ScaleType.Championship
                    => ValidationResult.Error("'--count' must be greater than 5 for championship points to be awarded."),
                < 3 => ValidationResult.Error("'--count' must be greater than 2 for points to be awarded."),
                _ => ValidationResult.Success()
            };
    }

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var pointsScale = settings.Type switch
        {
            ScaleType.Stage => PointsDistribution.CreateStageScale(settings.Count),
            ScaleType.Final => PointsDistribution.CreateFinalScale(settings.Count),
            ScaleType.Championship => PointsDistribution.CreateChampionshipScale(settings.Count),
            _ => throw new NotSupportedException($"{settings.Type} is not supported."),
        };
        
        AnsiConsoleOutput.PrintPointsScale(settings.Type!.Value, pointsScale);
        return 0;
    }
}
