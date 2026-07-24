namespace InkCloudLauncher.Models;

public record LaunchOption(
    string? VersionId,
    string? JavaPath,
    string? PlayerName,
    int MaxMemoryMb,
    int Width,
    int Height,
    bool Fullscreen,
    string? JvmArgs);