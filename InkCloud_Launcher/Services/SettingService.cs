using InkCloud_Launcher.Model;
using System;
using System.IO;
using System.Text.Json;

namespace InkCloud_Launcher.Services;

public sealed class SettingService {
    private const string SAVE_PATH = "settings.json";

    public DataModel? Data { get; private set; }
    public static SettingService? Current { get; private set; }

    public static void InitService() {
        if (Current is not null)
            throw new InvalidOperationException();

        Current = new();
    }

    public void Init() {
        if (!File.Exists(SAVE_PATH)) {
            Save();

            return;
        }

        var json = File.ReadAllText(SAVE_PATH);
        Data = JsonSerializer.Deserialize<DataModel>(json);
    }

    public void Save() {
        File.WriteAllText(SAVE_PATH,
            JsonSerializer.Serialize(Data ??= new DataModel()));
    }
}