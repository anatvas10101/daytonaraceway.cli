namespace DaytonaRaceway.Agent;

public interface IAgentSettings
{
    public string SourceIp { get; }

    public int Port { get; }

    public TimeSpan Timeout => TimeSpan.FromSeconds(30);
}
