using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InkCloudLauncher.Service;

namespace InkCloudLauncher.ViewModel;

public partial class MainWindowViewModel : ObservableObject
{
    private const double DragThreshold = 10;
    private const double HintTriggerRatio = 0.64;
    private const double HintCenterBand = 260;

    private bool _isDraggingHandle;
    private Point _handleStart;
    private int _navigationRun;
    private readonly MinecraftService _minecraft = new();
    private readonly ObservableCollection<MinecraftVersionItem> _allOnlineGameVersions = [];
    private CancellationTokenSource? _minecraftTaskToken;

    [ObservableProperty]
    private bool isNavigationOpen;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isNavigationLayerVisible;

    [ObservableProperty]
    private bool isReturnButtonVisible;

    [ObservableProperty]
    private bool isDragHandleVisible = true;

    [ObservableProperty]
    private bool isPlazaPageVisible = true;

    [ObservableProperty]
    private bool isSettingPageVisible;

    [ObservableProperty]
    private bool isDownloadPageVisible;

    [ObservableProperty]
    private double plazaPageOpacity = 1;

    [ObservableProperty]
    private Thickness plazaPageMargin;

    [ObservableProperty]
    private double settingPageOpacity;

    [ObservableProperty]
    private Thickness settingPageMargin = new(34, 0, -34, 0);

    [ObservableProperty]
    private double downloadPageOpacity;

    [ObservableProperty]
    private Thickness downloadPageMargin = new(34, 0, -34, 0);

    [ObservableProperty]
    private string selectedSettingSection = "Download";

    [ObservableProperty]
    private string selectedDownloadSection = "Game";

    [ObservableProperty]
    private string selectedGameVersion = string.Empty;

    [ObservableProperty]
    private string minecraftFolder = MinecraftService.GetDefaultMinecraftFolder();

    [ObservableProperty]
    private string javaPath = string.Empty;

    [ObservableProperty]
    private string playerName = "InkCloud";

    [ObservableProperty]
    private string selectedForgeVersion = MinecraftService.None;

    [ObservableProperty]
    private string selectedFabricVersion = MinecraftService.None;

    [ObservableProperty]
    private string selectedOptifineVersion = MinecraftService.None;

    [ObservableProperty]
    private string selectedLaunchVersion = string.Empty;

    [ObservableProperty]
    private string taskStatus = "就绪";

    [ObservableProperty]
    private string gameVersionSearchText = string.Empty;

    [ObservableProperty]
    private string selectedGameVersionFilter = "正式版";

    [ObservableProperty]
    private string pressedSettingSection = string.Empty;

    [ObservableProperty]
    private double settingContentOpacity = 1;

    [ObservableProperty]
    private Thickness settingContentMargin;

    [ObservableProperty]
    private double downloadContentOpacity = 1;

    [ObservableProperty]
    private Thickness downloadContentMargin;

    public bool IsDownloadSettingsSelected => SelectedSettingSection == "Download";
    public bool IsLaunchSettingsSelected => SelectedSettingSection == "Launch";
    public bool IsJavaSettingsSelected => SelectedSettingSection == "Java";
    public bool IsWindowSettingsSelected => SelectedSettingSection == "Window";

    public bool IsDownloadSettingsHighlighted => IsDownloadSettingsSelected || PressedSettingSection == "Download";
    public bool IsLaunchSettingsHighlighted => IsLaunchSettingsSelected || PressedSettingSection == "Launch";
    public bool IsJavaSettingsHighlighted => IsJavaSettingsSelected || PressedSettingSection == "Java";
    public bool IsWindowSettingsHighlighted => IsWindowSettingsSelected || PressedSettingSection == "Window";

    public bool IsGameDownloadSelected => SelectedDownloadSection == "Game";
    public bool IsExtraDownloadSelected => SelectedDownloadSection == "Extra";
    public bool IsGameVersionListVisible => IsGameDownloadSelected && string.IsNullOrEmpty(SelectedGameVersion);
    public bool IsGameVersionConfigVisible => IsGameDownloadSelected && !string.IsNullOrEmpty(SelectedGameVersion);

    public ObservableCollection<MinecraftVersionItem> OnlineGameVersions { get; } = [];
    public ObservableCollection<string> InstalledVersions { get; } = [];
    public ObservableCollection<string> ForgeVersions { get; } = [MinecraftService.None];
    public ObservableCollection<string> FabricVersions { get; } = [MinecraftService.None];
    public ObservableCollection<string> OptifineVersions { get; } = [MinecraftService.None];
    public bool IsReleaseVersionFilterSelected => SelectedGameVersionFilter == "正式版";
    public bool IsSnapshotVersionFilterSelected => SelectedGameVersionFilter == "预览版";
    public bool IsOldVersionFilterSelected => SelectedGameVersionFilter == "远古版";
    public bool IsAllVersionFilterSelected => SelectedGameVersionFilter == "全部";

    [ObservableProperty]
    private double homeOpacity = 1;

    [ObservableProperty]
    private Thickness homeMargin;

    [ObservableProperty]
    private double plazaOpacity;

    [ObservableProperty]
    private Thickness plazaMargin = new(0, 420, 0, -420);

    [ObservableProperty]
    private double micaOpacity = .78;

    [ObservableProperty]
    private double returnButtonOpacity;

    [ObservableProperty]
    private double dragHandleOpacity;

    [ObservableProperty]
    private Thickness dragHandleMargin;

    [RelayCommand]
    public async Task OpenNavigationAsync()
    {
        if (IsNavigationOpen && IsNavigationLayerVisible)
        {
            return;
        }

        var run = ++_navigationRun;
        IsBusy = true;
        IsNavigationOpen = true;
        IsNavigationLayerVisible = true;

        DragHandleOpacity = 0;
        MicaOpacity = .95;
        HomeOpacity = 0;
        HomeMargin = new Thickness(0, -58, 0, 58);
        PlazaMargin = new Thickness(0);
        PlazaOpacity = 1;

        await Task.Delay(180);
        if (run != _navigationRun)
        {
            return;
        }

        IsDragHandleVisible = false;
        IsReturnButtonVisible = true;
        ReturnButtonOpacity = 1;

        await Task.Delay(420);
        if (run != _navigationRun)
        {
            return;
        }

        IsBusy = false;
    }

    [RelayCommand]
    public async Task CloseNavigationAsync()
    {
        if (!IsNavigationOpen && !IsNavigationLayerVisible)
        {
            return;
        }

        var run = ++_navigationRun;
        IsBusy = true;
        IsNavigationOpen = false;
        IsDragHandleVisible = true;

        ReturnButtonOpacity = 0;
        MicaOpacity = .78;
        PlazaOpacity = 0;
        PlazaMargin = new Thickness(0, 420, 0, -420);
        HomeMargin = new Thickness(0);
        HomeOpacity = 1;
        DragHandleOpacity = 0;
        IsBusy = false;

        await Task.Delay(600);
        if (run != _navigationRun)
        {
            return;
        }

        IsNavigationLayerVisible = false;
        ResetNavigationContentToPlaza();
    }

    [RelayCommand]
    private async Task OpenPlazaPageAsync()
    {
        if (IsPlazaPageVisible && !IsSettingPageVisible && !IsDownloadPageVisible)
        {
            return;
        }

        IsPlazaPageVisible = true;
        PlazaPageMargin = new Thickness(-34, 0, 34, 0);

        SettingPageOpacity = 0;
        SettingPageMargin = new Thickness(34, 0, -34, 0);
        DownloadPageOpacity = 0;
        DownloadPageMargin = new Thickness(34, 0, -34, 0);
        PlazaPageOpacity = 1;
        PlazaPageMargin = new Thickness(0);

        await Task.Delay(260);

        IsSettingPageVisible = false;
        IsDownloadPageVisible = false;
    }

    [RelayCommand]
    private async Task OpenSettingPageAsync()
    {
        if (IsSettingPageVisible)
        {
            return;
        }

        IsSettingPageVisible = true;
        SettingPageMargin = new Thickness(34, 0, -34, 0);

        PlazaPageOpacity = 0;
        PlazaPageMargin = new Thickness(-34, 0, 34, 0);
        DownloadPageOpacity = 0;
        DownloadPageMargin = new Thickness(-34, 0, 34, 0);
        SettingPageOpacity = 1;
        SettingPageMargin = new Thickness(0);

        await Task.Delay(260);

        IsPlazaPageVisible = false;
        IsDownloadPageVisible = false;
    }

    [RelayCommand]
    private async Task OpenDownloadPageAsync()
    {
        if (IsDownloadPageVisible)
        {
            return;
        }

        IsDownloadPageVisible = true;
        DownloadPageMargin = new Thickness(34, 0, -34, 0);

        PlazaPageOpacity = 0;
        PlazaPageMargin = new Thickness(-34, 0, 34, 0);
        SettingPageOpacity = 0;
        SettingPageMargin = new Thickness(-34, 0, 34, 0);
        DownloadPageOpacity = 1;
        DownloadPageMargin = new Thickness(0);

        await Task.Delay(260);

        IsPlazaPageVisible = false;
        IsSettingPageVisible = false;

        await RefreshDownloadDataAsync();
    }

    private void ResetNavigationContentToPlaza()
    {
        IsPlazaPageVisible = true;
        IsSettingPageVisible = false;
        IsDownloadPageVisible = false;
        PlazaPageOpacity = 1;
        PlazaPageMargin = new Thickness(0);
        SettingPageOpacity = 0;
        SettingPageMargin = new Thickness(34, 0, -34, 0);
        DownloadPageOpacity = 0;
        DownloadPageMargin = new Thickness(34, 0, -34, 0);
    }

    [RelayCommand]
    private async Task SelectDownloadSectionAsync(string section)
    {
        if (SelectedDownloadSection == section)
        {
            return;
        }

        SelectedDownloadSection = section;
        DownloadContentOpacity = 0;
        DownloadContentMargin = new Thickness(-18, 0, 18, 0);
        await Task.Delay(140);

        DownloadContentMargin = new Thickness(22, 0, -22, 0);
        DownloadContentOpacity = 1;
        DownloadContentMargin = new Thickness(0);
    }

    [RelayCommand]
    private async Task OpenGameVersionConfigAsync(string version)
    {
        DownloadContentOpacity = 0;
        DownloadContentMargin = new Thickness(-18, 0, 18, 0);
        await Task.Delay(140);

        SelectedGameVersion = version;
        SelectedForgeVersion = MinecraftService.None;
        SelectedFabricVersion = MinecraftService.None;
        SelectedOptifineVersion = MinecraftService.None;
        DownloadContentMargin = new Thickness(22, 0, -22, 0);
        DownloadContentOpacity = 1;
        DownloadContentMargin = new Thickness(0);

        await LoadModLoaderVersionsAsync(version);
    }

    [RelayCommand]
    private async Task BackGameVersionListAsync()
    {
        DownloadContentOpacity = 0;
        DownloadContentMargin = new Thickness(-18, 0, 18, 0);
        await Task.Delay(140);

        SelectedGameVersion = string.Empty;
        DownloadContentMargin = new Thickness(22, 0, -22, 0);
        DownloadContentOpacity = 1;
        DownloadContentMargin = new Thickness(0);
    }

    [RelayCommand]
    private async Task RefreshDownloadDataAsync()
    {
        await RunMinecraftTaskAsync("正在加载版本列表...", async token =>
        {
            _allOnlineGameVersions.Clear();
            foreach (var version in await _minecraft.GetOnlineVersionsAsync(token))
            {
                _allOnlineGameVersions.Add(version);
            }

            ApplyGameVersionFilter();
            RefreshInstalledVersions();
            TaskStatus = "版本列表已刷新";
        });
    }

    [RelayCommand]
    private void SelectGameVersionFilter(string filter)
    {
        SelectedGameVersionFilter = filter;
        ApplyGameVersionFilter();
    }

    private void ApplyGameVersionFilter()
    {
        var keyword = GameVersionSearchText.Trim();
        var filter = SelectedGameVersionFilter;

        OnlineGameVersions.Clear();
        foreach (var version in _allOnlineGameVersions.Where(x =>
                     (filter == "全部" || x.DisplayType == filter) &&
                     (keyword.Length == 0 || x.Id.Contains(keyword, StringComparison.OrdinalIgnoreCase))))
        {
            OnlineGameVersions.Add(version);
        }

        TaskStatus = $"显示 {OnlineGameVersions.Count} 个版本";
    }

    [RelayCommand]
    private async Task InstallSelectedGameAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedGameVersion))
        {
            TaskStatus = "请先选择游戏版本";
            return;
        }

        var options = new GameInstallOptions(
            SelectedGameVersion,
            SelectedForgeVersion,
            SelectedFabricVersion,
            SelectedOptifineVersion);

        await RunMinecraftTaskAsync($"正在安装 {SelectedGameVersion}...", async token =>
        {
            var installedId = await _minecraft.InstallAsync(MinecraftFolder, options, token);
            RefreshInstalledVersions();
            SelectedLaunchVersion = installedId;
            TaskStatus = $"安装完成：{installedId}";
        });
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        await RunMinecraftTaskAsync("正在启动游戏...", async token =>
        {
            await _minecraft.LaunchAsync(MinecraftFolder, new LaunchOptions(
                SelectedLaunchVersion,
                JavaPath,
                PlayerName,
                4096,
                1280,
                720,
                false,
                "-XX:+UseG1GC"), token);
            TaskStatus = "游戏已启动";
        });
    }

    [RelayCommand]
    private async Task InstallJavaRuntimeAsync()
    {
        await RunMinecraftTaskAsync("正在安装 Java 运行时...", async token =>
        {
            JavaPath = await _minecraft.InstallJavaAsync(MinecraftFolder, token);
            TaskStatus = "Java 运行时已安装";
        });
    }

    private async Task LoadModLoaderVersionsAsync(string version)
    {
        await RunMinecraftTaskAsync("正在读取加载器版本...", async token =>
        {
            ForgeVersions.Clear();
            foreach (var item in await ReadLoaderVersionsAsync(() => _minecraft.GetForgeVersionsAsync(version, token)))
            {
                ForgeVersions.Add(item);
            }

            FabricVersions.Clear();
            foreach (var item in await ReadLoaderVersionsAsync(() => _minecraft.GetFabricVersionsAsync(version, token)))
            {
                FabricVersions.Add(item);
            }

            OptifineVersions.Clear();
            foreach (var item in await ReadLoaderVersionsAsync(() => _minecraft.GetOptifineVersionsAsync(version, token)))
            {
                OptifineVersions.Add(item);
            }

            SelectedForgeVersion = ForgeVersions.FirstOrDefault() ?? MinecraftService.None;
            SelectedFabricVersion = FabricVersions.FirstOrDefault() ?? MinecraftService.None;
            SelectedOptifineVersion = OptifineVersions.FirstOrDefault() ?? MinecraftService.None;
            TaskStatus = $"加载器版本已读取：Forge {Math.Max(0, ForgeVersions.Count - 1)} / Fabric {Math.Max(0, FabricVersions.Count - 1)} / OptiFine {Math.Max(0, OptifineVersions.Count - 1)}";
        });
    }

    private static async Task<string[]> ReadLoaderVersionsAsync(Func<Task<IReadOnlyList<string>>> read)
    {
        try
        {
            var versions = await read();
            return versions.Count == 0 ? [MinecraftService.None] : versions.ToArray();
        }
        catch
        {
            return [MinecraftService.None];
        }
    }

    private void RefreshInstalledVersions()
    {
        InstalledVersions.Clear();
        foreach (var version in _minecraft.GetInstalledVersions(MinecraftFolder))
        {
            InstalledVersions.Add(version);
        }

        if (string.IsNullOrWhiteSpace(SelectedLaunchVersion) && InstalledVersions.Count > 0)
        {
            SelectedLaunchVersion = InstalledVersions[0];
        }
    }

    private async Task RunMinecraftTaskAsync(string status, Func<CancellationToken, Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        _minecraftTaskToken?.Cancel();
        _minecraftTaskToken = new CancellationTokenSource();

        try
        {
            IsBusy = true;
            TaskStatus = status;
            await action(_minecraftTaskToken.Token);
        }
        catch (OperationCanceledException)
        {
            TaskStatus = "任务已取消";
        }
        catch (Exception ex)
        {
            TaskStatus = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SelectSettingSectionAsync(string section)
    {
        if (SelectedSettingSection == section)
        {
            PressedSettingSection = string.Empty;
            return;
        }

        SelectedSettingSection = section;
        PressedSettingSection = string.Empty;
        SettingContentOpacity = 0;
        SettingContentMargin = new Thickness(-18, 0, 18, 0);
        await Task.Delay(140);

        SettingContentMargin = new Thickness(22, 0, -22, 0);
        SettingContentOpacity = 1;
        SettingContentMargin = new Thickness(0);
    }

    partial void OnSelectedSettingSectionChanged(string value)
    {
        UpdateSettingNavState();
    }

    partial void OnSelectedDownloadSectionChanged(string value)
    {
        OnPropertyChanged(nameof(IsGameDownloadSelected));
        OnPropertyChanged(nameof(IsExtraDownloadSelected));
        OnPropertyChanged(nameof(IsGameVersionListVisible));
        OnPropertyChanged(nameof(IsGameVersionConfigVisible));
    }

    partial void OnSelectedGameVersionChanged(string value)
    {
        OnPropertyChanged(nameof(IsGameVersionListVisible));
        OnPropertyChanged(nameof(IsGameVersionConfigVisible));
    }

    partial void OnGameVersionSearchTextChanged(string value)
    {
        ApplyGameVersionFilter();
    }

    partial void OnSelectedGameVersionFilterChanged(string value)
    {
        OnPropertyChanged(nameof(IsReleaseVersionFilterSelected));
        OnPropertyChanged(nameof(IsSnapshotVersionFilterSelected));
        OnPropertyChanged(nameof(IsOldVersionFilterSelected));
        OnPropertyChanged(nameof(IsAllVersionFilterSelected));
    }

    partial void OnPressedSettingSectionChanged(string value)
    {
        UpdateSettingNavState();
    }

    public void PressSettingSection(string section)
    {
        PressedSettingSection = section;
    }

    public void ReleaseSettingSection()
    {
        PressedSettingSection = string.Empty;
    }

    private void UpdateSettingNavState()
    {
        OnPropertyChanged(nameof(IsDownloadSettingsSelected));
        OnPropertyChanged(nameof(IsLaunchSettingsSelected));
        OnPropertyChanged(nameof(IsJavaSettingsSelected));
        OnPropertyChanged(nameof(IsWindowSettingsSelected));
        OnPropertyChanged(nameof(IsDownloadSettingsHighlighted));
        OnPropertyChanged(nameof(IsLaunchSettingsHighlighted));
        OnPropertyChanged(nameof(IsJavaSettingsHighlighted));
        OnPropertyChanged(nameof(IsWindowSettingsHighlighted));
    }

    public void UpdateHint(Point pointer, Size windowSize)
    {
        if (IsNavigationOpen || _isDraggingHandle)
        {
            return;
        }

        DragHandleOpacity = IsInsideHintZone(pointer, windowSize) ? 1 : 0;
    }

    public void HideHintIfIdle()
    {
        if (!IsNavigationOpen && !_isDraggingHandle)
        {
            DragHandleOpacity = 0;
        }
    }

    public bool BeginHandleDrag(Point pointer, Size windowSize)
    {
        if (IsNavigationOpen || !IsInsideHintZone(pointer, windowSize))
        {
            return false;
        }

        _isDraggingHandle = true;
        _handleStart = pointer;
        DragHandleOpacity = 1;
        return true;
    }

    public void MoveHandle(Point pointer, Size windowSize)
    {
        if (!_isDraggingHandle)
        {
            UpdateHint(pointer, windowSize);
            return;
        }

        var delta = pointer.Y - _handleStart.Y;
        DragHandleMargin = new Thickness(0, Math.Clamp(delta * 0.22, -12, 8), 0, 0);
    }

    public async Task EndHandleDragAsync(Point pointer)
    {
        if (!_isDraggingHandle)
        {
            return;
        }

        _isDraggingHandle = false;
        DragHandleMargin = new Thickness(0);

        var delta = pointer.Y - _handleStart.Y;
        if (!IsNavigationOpen && delta < -DragThreshold)
        {
            await OpenNavigationAsync();
        }
        else if (!IsNavigationOpen)
        {
            DragHandleOpacity = 0;
        }
    }

    private static bool IsInsideHintZone(Point pointer, Size windowSize)
    {
        var nearBottom = pointer.Y > windowSize.Height * HintTriggerRatio;
        var nearCenter = Math.Abs(pointer.X - windowSize.Width / 2) < HintCenterBand;
        return nearBottom && nearCenter;
    }
}
