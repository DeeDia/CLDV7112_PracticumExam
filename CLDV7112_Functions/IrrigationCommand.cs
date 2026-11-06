namespace CLDV7112_Functions;

public class IrrigationCommand
{
    public string DeviceId { get; set; } = string.Empty;
    public string Command { get; set; } = "StartIrrigation";
    public int DurationMinutes { get; set; } = 30;
    public string Reason { get; set; } = string.Empty;
    public DateTime IssuedUtc { get; set; } = DateTime.UtcNow;
}