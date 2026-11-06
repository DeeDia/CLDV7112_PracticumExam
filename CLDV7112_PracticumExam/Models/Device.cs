namespace CLDV7112_PracticumExam.Models;

public class Device
{
    public string DeviceId { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Status { get; set; } = "Idle";
    public DateTime LastUpdated { get; set; }
}