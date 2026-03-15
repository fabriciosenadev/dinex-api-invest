namespace DinExApi.Core;

public sealed class BootstrapAdminSettings
{
    public bool Enabled { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
