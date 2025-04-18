using System.Text.Json.Serialization;

namespace InkCloud_Launcher.Model;

public class DataModel {
    [JsonPropertyName("themeType")] public int ThemeType { get; set; }
}