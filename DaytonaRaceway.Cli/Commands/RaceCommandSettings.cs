using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
namespace DaytonaRaceway.Cli.Commands;

public abstract class RaceCommandSettings : CommandSettings
{
    [CommandArgument(0, "<RACE_UUID>")]
    [Description("Race UUID.")]
    public Guid RaceId { get; init; }

    public override ValidationResult Validate()
    {
        if (RaceId == Guid.Empty)
            return ValidationResult.Error("Race id is required and must be a valid non-empty GUID.");

        return ValidationResult.Success();
    }
}
