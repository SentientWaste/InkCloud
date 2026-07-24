namespace InkCloudLauncher.Models;

public record MinecraftVersionItem(string Id, string Type) {
    public string DisplayType => Type switch {
        "release" => "正式版",
        "snapshot" => "预览版",
        "old_alpha" or "old_beta" => "远古版",
        _ => "其他"
    };
}