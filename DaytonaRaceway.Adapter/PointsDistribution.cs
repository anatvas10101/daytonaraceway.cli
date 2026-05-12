namespace DaytonaRaceway.Adapter;

public static class PointsDistribution
{
    public static IReadOnlyDictionary<int, int> CreateStageScale(int numberOfParticipants)
        => Enumerable.Range(1, numberOfParticipants)
            .ToDictionary(
                i => i,
                i => i switch
                {
                    1 => numberOfParticipants + 2,
                    2 => numberOfParticipants,
                    _ => numberOfParticipants - i + 1
                });

    public static IReadOnlyDictionary<int, int> CreateFinalScale(int numberOfParticipants)
        => Enumerable.Range(1, numberOfParticipants)
            .ToDictionary(
                i => i,
                i => i switch
                {
                    1 => numberOfParticipants + 2,
                    2 => numberOfParticipants,
                    _ => numberOfParticipants - i + 1
                });

    public static IReadOnlyDictionary<int, int> CreateChampionshipScale(int numberOfParticipants)
    {
        var maxPoints = numberOfParticipants switch
        {
            < 21 => 1000 + numberOfParticipants,
            < 31 => 1021,
            >= 31 => 1022
        };

        return Enumerable.Range(1, numberOfParticipants)
            .ToDictionary(
                i => i,
                i => (int)Math.Round(maxPoints - 1000.0 * (i - 1) / (numberOfParticipants - 1)));
    }
}
