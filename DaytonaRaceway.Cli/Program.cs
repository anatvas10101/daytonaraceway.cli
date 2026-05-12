using DaytonaRaceway.Cli.Commands;
using Spectre.Console.Cli;

// Create a cancellation token source to handle Ctrl+C
var cancellationTokenSource = new CancellationTokenSource();
  
// Wire up Console.CancelKeyPress to trigger cancellation
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // Prevent immediate process termination
    cancellationTokenSource.Cancel();
    Console.WriteLine("Cancellation requested...");
};

var app = new CommandApp();
app.Configure(config =>
{
    config.SetApplicationName("daytona");

    config.AddCommand<QualificationResultsCommand>("qualification")
        .WithDescription("Shows qualification results of an event (per stage or aggregated result).")
        .WithAlias("qualy");

    config.AddCommand<GroupsDistributionCommand>("groups")
        .WithDescription("Splits participants into <COUNT> groups based on qualification results.");

    config.AddCommand<PointsScaleCommand>("points")
        .WithDescription("Shows a points scale of a selected type for a given number of participants.");
});

return await app.RunAsync(args, cancellationTokenSource.Token);
