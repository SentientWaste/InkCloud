using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InkCloudLauncher.Models;
using MinecraftLaunch;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Launch;

namespace InkCloudLauncher.Service;

public sealed class MinecraftService {
    public static readonly MinecraftService Instance = new();

    public static string GetDefaultMinecraftFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, ".minecraft");
    }

    public void InitializeMinecraftLaunch() {
        InitializeHelper.Initialize(settings => {
            settings.UserAgent = "InkCloudLauncher/1.0";
            settings.MaxThread = 32;
            settings.MaxRetryCount = 3;
            settings.IsEnableMirror = true;
            settings.IsEnableFragment = true;
        });
    }
    
    public string GetMinecraftFolder(string? configuredFolder)
    {
        return string.IsNullOrWhiteSpace(configuredFolder)
            ? GetDefaultMinecraftFolder()
            : configuredFolder.Trim();
    }

    public IReadOnlyList<string> GetInstalledVersions(string? configuredFolder)
    {
        var folder = GetMinecraftFolder(configuredFolder);
        if (!Directory.Exists(folder))
        {
            return [];
        }

        return new MinecraftParser(folder)
            .GetMinecrafts()
            .Select(x => x.Id)
            .OrderByDescending(x => x)
            .ToArray();
    }
    
    public async Task LaunchAsync(string? configuredFolder, LaunchOption options, CancellationToken token)
    {
        var folder = GetMinecraftFolder(configuredFolder);
        var parser = new MinecraftParser(folder);
        var versions = parser.GetMinecrafts();
        if (versions.Count == 0)
        {
            throw new InvalidOperationException("没有找到可启动的游戏版本，请先下载游戏。");
        }

        var minecraft = string.IsNullOrWhiteSpace(options.VersionId)
            ? versions.OrderByDescending(x => x.ReleaseTime).First()
            : parser.GetMinecraft(options.VersionId);

        var java = await JavaService.FindJavaAsync(options.JavaPath, minecraft, token);
        var account = new OfflineAuthenticator().Authenticate(
            string.IsNullOrWhiteSpace(options.PlayerName) ? "InkCloud" : options.PlayerName,
            Guid.NewGuid());

        var config = new LaunchConfig
        {
            Account = account,
            JavaPath = java,
            LauncherName = "InkCloudLauncher",
            MaxMemorySize = options.MaxMemoryMb,
            MinMemorySize = Math.Min(512, options.MaxMemoryMb),
            Width = options.Width,
            Height = options.Height,
            IsFullscreen = options.Fullscreen,
            JvmArguments = SplitArgs(options.JvmArgs)
        };

        var runner = new MinecraftRunner(config, parser);
        var process = await runner.RunAsync(minecraft, token);
        process.Start();
    }

    private static string[] SplitArgs(string? args)
    {
        return string.IsNullOrWhiteSpace(args)
            ? []
            : args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}