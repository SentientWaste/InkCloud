using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Authenticator;
using MinecraftLaunch.Components.Installer;
using MinecraftLaunch.Components.Parser;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Launch;
using MinecraftLaunch.Utilities;

namespace InkCloudLauncher.Service;

public sealed class MinecraftService
{
    public const string None = "不安装";

    private static readonly object InitLock = new();
    private static bool _initialized;

    public MinecraftService()
    {
        EnsureMinecraftLaunch();
    }

    public static string GetDefaultMinecraftFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, ".minecraft");
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

    public async Task<IReadOnlyList<MinecraftVersionItem>> GetOnlineVersionsAsync(CancellationToken token)
    {
        var entries = await VanillaInstaller.EnumerableMinecraftAsync(token);
        return entries
            .Select(x => new MinecraftVersionItem(x.Id, x.Type, GetVersionTypeName(x.Type)))
            .ToArray();
    }

    public async Task<IReadOnlyList<string>> GetForgeVersionsAsync(string mcVersion, CancellationToken token)
    {
        var entries = await ForgeInstaller.EnumerableForgeAsync(mcVersion, false, token);
        return entries
            .Select(GetForgeDisplay)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Prepend(None)
            .ToArray();
    }

    public async Task<IReadOnlyList<string>> GetFabricVersionsAsync(string mcVersion, CancellationToken token)
    {
        var entries = await FabricInstaller.EnumerableFabricAsync(mcVersion, token);
        return entries
            .Select(GetFabricDisplay)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Prepend(None)
            .ToArray();
    }

    public async Task<IReadOnlyList<string>> GetOptifineVersionsAsync(string mcVersion, CancellationToken token)
    {
        var entries = await OptifineInstaller.EnumerableOptifineAsync(mcVersion, token);
        return entries
            .Select(GetOptifineDisplay)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Prepend(None)
            .ToArray();
    }

    public async Task<string> InstallAsync(string? configuredFolder, GameInstallOptions options, CancellationToken token)
    {
        var folder = GetMinecraftFolder(configuredFolder);
        Directory.CreateDirectory(folder);

        var versionEntries = await VanillaInstaller.EnumerableMinecraftAsync(token);
        var vanilla = versionEntries.FirstOrDefault(x => x.Id == options.MinecraftVersion);
        if (vanilla is null)
        {
            throw new InvalidOperationException($"没有找到 Minecraft {options.MinecraftVersion}。");
        }

        await VanillaInstaller.Create(folder, vanilla).InstallAsync(token);

        var javaPath = await FindJavaPathAsync(token);
        var selectedId = options.MinecraftVersion;
        var customId = BuildCustomId(options);

        if (!IsNone(options.FabricVersion))
        {
            var fabrics = await FabricInstaller.EnumerableFabricAsync(options.MinecraftVersion, token);
            var fabric = fabrics.FirstOrDefault(x => Matches(GetFabricDisplay(x), options.FabricVersion));
            if (fabric is null)
            {
                throw new InvalidOperationException($"没有找到 Fabric {options.FabricVersion}。");
            }

            selectedId = customId;
            await FabricInstaller.Create(folder, fabric, customId).InstallAsync(token);
            return selectedId;
        }

        if (!IsNone(options.ForgeVersion))
        {
            if (javaPath is null)
            {
                throw new InvalidOperationException("安装 Forge 需要 Java，请先在设置页配置 Java。");
            }

            var forges = await ForgeInstaller.EnumerableForgeAsync(options.MinecraftVersion, false, token);
            var forge = forges.FirstOrDefault(x => Matches(GetForgeDisplay(x), options.ForgeVersion));
            if (forge is null)
            {
                throw new InvalidOperationException($"没有找到 Forge {options.ForgeVersion}。");
            }

            selectedId = customId;
            await ForgeInstaller.Create(folder, javaPath, forge, customId).InstallAsync(token);
        }

        if (!IsNone(options.OptifineVersion))
        {
            if (javaPath is null)
            {
                throw new InvalidOperationException("安装 OptiFine 需要 Java，请先在设置页配置 Java。");
            }

            var optifines = await OptifineInstaller.EnumerableOptifineAsync(options.MinecraftVersion, token);
            var optifine = optifines.FirstOrDefault(x => Matches(GetOptifineDisplay(x), options.OptifineVersion));
            if (optifine is null)
            {
                throw new InvalidOperationException($"没有找到 OptiFine {options.OptifineVersion}。");
            }

            selectedId = customId;
            await OptifineInstaller.Create(folder, javaPath, optifine, customId).InstallAsync(token);
        }

        return selectedId;
    }

    public async Task LaunchAsync(string? configuredFolder, LaunchOptions options, CancellationToken token)
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

        var java = await FindJavaAsync(options.JavaPath, minecraft, token);
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

    public async Task<string> InstallJavaAsync(string? configuredFolder, CancellationToken token)
    {
        var folder = Path.Combine(GetMinecraftFolder(configuredFolder), "runtime");
        Directory.CreateDirectory(folder);
        await JavaInstaller.Create(folder).InstallAsync(token);
        return folder;
    }

    private static void EnsureMinecraftLaunch()
    {
        if (_initialized)
        {
            return;
        }

        lock (InitLock)
        {
            if (_initialized)
            {
                return;
            }

            InitializeHelper.Initialize(settings =>
            {
                settings.UserAgent = "InkCloudLauncher/1.0";
                settings.MaxThread = 32;
                settings.MaxRetryCount = 3;
                settings.IsEnableMirror = true;
                settings.IsEnableFragment = true;
            });

            _initialized = true;
        }
    }

    private static async Task<JavaEntry> FindJavaAsync(string? configuredJava, MinecraftEntry minecraft, CancellationToken token)
    {
        if (!string.IsNullOrWhiteSpace(configuredJava) && File.Exists(configuredJava))
        {
            return await JavaUtil.GetJavaInfoAsync(configuredJava, token);
        }

        var javas = new List<JavaEntry>();
        await foreach (var java in JavaUtil.EnumerableJavaAsync(token))
        {
            javas.Add(java);
        }

        if (javas.Count == 0)
        {
            throw new InvalidOperationException("没有找到 Java，请在设置页配置 Java 路径。");
        }

        return minecraft.GetAppropriateJava(javas);
    }

    private static async Task<string?> FindJavaPathAsync(CancellationToken token)
    {
        await foreach (var java in JavaUtil.EnumerableJavaAsync(token))
        {
            return java.JavaPath;
        }

        return null;
    }

    private static string GetForgeDisplay(ForgeInstallEntry entry)
    {
        return string.IsNullOrWhiteSpace(entry.DisplayVersion) ? entry.ForgeVersion : entry.DisplayVersion;
    }

    private static string GetFabricDisplay(FabricInstallEntry entry)
    {
        return string.IsNullOrWhiteSpace(entry.DisplayVersion) ? entry.BuildVersion : entry.DisplayVersion;
    }

    private static string GetOptifineDisplay(OptifineInstallEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.DisplayVersion))
        {
            return entry.DisplayVersion;
        }

        return string.IsNullOrWhiteSpace(entry.Type) ? entry.Patch : $"{entry.Type}_{entry.Patch}";
    }

    private static string BuildCustomId(GameInstallOptions options)
    {
        var parts = new List<string> { options.MinecraftVersion };
        if (!IsNone(options.ForgeVersion))
        {
            parts.Add($"forge-{CleanId(options.ForgeVersion)}");
        }

        if (!IsNone(options.FabricVersion))
        {
            parts.Add($"fabric-{CleanId(options.FabricVersion)}");
        }

        if (!IsNone(options.OptifineVersion))
        {
            parts.Add($"optifine-{CleanId(options.OptifineVersion)}");
        }

        return string.Join("-", parts);
    }

    private static string CleanId(string value)
    {
        foreach (var ch in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(ch, '-');
        }

        return value.Replace(' ', '-');
    }

    private static bool Matches(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase) ||
               left.Contains(right, StringComparison.OrdinalIgnoreCase) ||
               right.Contains(left, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> SplitArgs(string? args)
    {
        return string.IsNullOrWhiteSpace(args)
            ? []
            : args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static bool IsNone(string? value)
    {
        return string.IsNullOrWhiteSpace(value) || value == None;
    }

    private static string GetVersionTypeName(string? type)
    {
        return type switch
        {
            "release" => "正式版",
            "snapshot" => "预览版",
            "old_alpha" => "远古版",
            "old_beta" => "远古版",
            _ => "其他"
        };
    }
}

public sealed record GameInstallOptions(string MinecraftVersion, string ForgeVersion, string FabricVersion, string OptifineVersion);

public sealed record MinecraftVersionItem(string Id, string Type, string DisplayType);

public sealed record LaunchOptions(
    string? VersionId,
    string? JavaPath,
    string? PlayerName,
    int MaxMemoryMb,
    int Width,
    int Height,
    bool Fullscreen,
    string? JvmArgs);
