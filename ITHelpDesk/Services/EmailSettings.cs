namespace ITHelpDesk.Services;

public class EmailSettings
{
    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? From { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host) &&
        Port is > 0 &&
        !string.IsNullOrWhiteSpace(UserName) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(From);
}

