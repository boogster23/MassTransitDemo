namespace MassTransitDemo.ApiService.Configurations;

public class DebounceOptions
{
    public TimeSpan DefaultInterval { get; set; } = TimeSpan.FromMilliseconds(500);
}