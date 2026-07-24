using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MinecraftLaunch.Base.Models.Game;
using MinecraftLaunch.Extensions;
using MinecraftLaunch.Utilities;

namespace InkCloudLauncher.Service;

public sealed class JavaService {
    public static async Task<JavaEntry> FindJavaAsync(string? configuredJava, MinecraftEntry minecraft, CancellationToken token)
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

        return javas.Count <= 0 
            ? throw new InvalidOperationException("没有找到 Java，请在设置页配置 Java 路径。") 
            : minecraft.GetAppropriateJava(javas);
    }

    public static async Task<string?> FindJavaPathAsync(CancellationToken token)
    {
        await foreach (var java in JavaUtil.EnumerableJavaAsync(token))
        {
            return java.JavaPath;
        }

        return null;
    }
}