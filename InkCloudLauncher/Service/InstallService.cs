using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InkCloudLauncher.Models;
using MinecraftLaunch.Base.Models.Network;
using MinecraftLaunch.Components.Installer;

namespace InkCloudLauncher.Service;

public sealed class InstallService {
    public const string None = "不安装";
    
    public static readonly InstallService Instance = new();
    
    public async Task<string> InstallAsync(string? configuredFolder, GameInstallOptions options, CancellationToken token) {
        var folder = MinecraftService.Instance.GetMinecraftFolder(configuredFolder);
        Directory.CreateDirectory(folder);

        var versionEntries = await VanillaInstaller.EnumerableMinecraftAsync(token);
        var vanilla = versionEntries.FirstOrDefault(x => x.Id == options.MinecraftVersion);
        if (vanilla is null)
        {
            throw new InvalidOperationException($"没有找到 Minecraft {options.MinecraftVersion}。");
        }

        await VanillaInstaller.Create(folder, vanilla).InstallAsync(token);

        var javaPath = await JavaService.FindJavaPathAsync(token);
        var selectedId = options.MinecraftVersion;
        var customId = BuildCustomId(options);

        if (!IsNone(options.FabricVersion))
        {
            var fabrics = await FabricInstaller.EnumerableFabricAsync(options.MinecraftVersion, token);
            var fabric = fabrics.FirstOrDefault(x => Matches(x.DisplayVersion, options.FabricVersion));
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
            var forge = forges.FirstOrDefault(x => Matches(x.DisplayVersion, options.ForgeVersion));
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

    //TODO: Java 实际暂不可用
    public async Task<string> InstallJavaAsync(string? configuredFolder, CancellationToken token)
    {
        var folder = Path.Combine(MinecraftService.Instance.GetMinecraftFolder(configuredFolder), "runtime");
        Directory.CreateDirectory(folder);
        await JavaInstaller.Create(folder).InstallAsync(token);
        return folder;
    }

    public static async Task<IReadOnlyList<MinecraftVersionItem>> GetVersionsAsync(CancellationToken token) {
            var entries = await VanillaInstaller.EnumerableMinecraftAsync(token);
            return entries
                .Select(x => new MinecraftVersionItem(x.Id, x.Type))
                .ToArray();
    }
    
    public static async Task<IReadOnlyList<string>> GetForgeVersionsAsync(string mcVersion, CancellationToken token)
    {
        var entries = await ForgeInstaller.EnumerableForgeAsync(mcVersion, false, token);
        return [
            ..entries
                .Select(x => x.DisplayVersion)
                .Prepend(None)
        ];
    }

    public static async Task<IReadOnlyList<string>> GetFabricVersionsAsync(string mcVersion, CancellationToken token)
    {
        var entries = await FabricInstaller.EnumerableFabricAsync(mcVersion, token);
        return [
            ..entries
                .Select(x => x.DisplayVersion)
                .Prepend(None)
        ];
    }

    public static async Task<IReadOnlyList<string>> GetOptifineVersionsAsync(string mcVersion, CancellationToken token)
    {
        var entries = await OptifineInstaller.EnumerableOptifineAsync(mcVersion, token);
        return entries
            .Select(GetOptifineDisplay)
            .Prepend(None)
            .ToArray();
    }
    
    private static bool IsNone(string? value)
        {
            return string.IsNullOrWhiteSpace(value) || value == None;
        }

    private static string CleanId(string value) {
        foreach (var ch in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(ch, '-');
        }

        return value.Replace(' ', '-');
    }

    private static string BuildCustomId(GameInstallOptions options) {
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
    
    private static bool Matches(string left, string right)
    {
        return string.Equals(left, right, StringComparison.OrdinalIgnoreCase) ||
               left.Contains(right, StringComparison.OrdinalIgnoreCase) ||
               right.Contains(left, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetOptifineDisplay(OptifineInstallEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.DisplayVersion))
        {
            return entry.DisplayVersion;
        }

        return string.IsNullOrWhiteSpace(entry.Type) ? entry.Patch : $"{entry.Type}_{entry.Patch}";
    }
}