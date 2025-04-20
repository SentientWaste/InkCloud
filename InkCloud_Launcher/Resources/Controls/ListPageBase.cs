using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media.TextFormatting;
using Avalonia.Metadata;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Threading;
using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;

namespace InkCloud_Launcher.Resources.Controls;

public class ListPageBase : ItemsControl {
    private int _totalCount;
    private TimeSpan _delay;

    public Task RunUnLoadedAnimationAsync() {
        TimeSpan totalDuration = default;

        Dispatcher.UIThread.Post(() => {
            if (this is IChildIndexProvider provider) {
                foreach (var item in Items.Cast<Visual>()) {
                    var index = provider.GetChildIndex(item);
                    if (index is -1)
                        return;

                    _delay = TimeSpan.FromMilliseconds(index * 50);
                    var compositionVisual = ElementComposition.GetElementVisual(item);
                    if (compositionVisual is null)
                        continue;

                    var compositor = compositionVisual.Compositor;

                    var scaleAni = compositor!.CreateVector3KeyFrameAnimation();
                    scaleAni.Target = "Scale";
                    scaleAni.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                    scaleAni!.Duration = TimeSpan.FromSeconds(1);
                    scaleAni.DelayTime = _delay;

                    scaleAni.InsertKeyFrame(0f, Vector3.One, new ExponentialEaseOut());
                    scaleAni.InsertKeyFrame(1f, new(0.7f), new ExponentialEaseOut());

                    var opacityAni = compositor?.CreateScalarKeyFrameAnimation();
                    opacityAni!.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                    opacityAni!.Target = "Opacity";

                    opacityAni?.InsertKeyFrame(0f, 1f, new CubicEaseOut());
                    opacityAni?.InsertKeyFrame(1f, 0f, new CubicEaseOut());
                    opacityAni!.Duration = TimeSpan.FromMicroseconds(10 * (_delay.TotalMicroseconds is <= 0 ? 200 : _delay.TotalMicroseconds));
                    opacityAni.DelayTime = _delay;

                    var group = compositor!.CreateAnimationGroup();
                    group.Add(scaleAni);
                    group.Add(opacityAni);

                    var size = compositionVisual!.Size;
                    totalDuration = opacityAni!.Duration + scaleAni!.Duration;
                    compositionVisual!.CenterPoint = new Vector3((float)size.X / 2, (float)size.Y, (float)compositionVisual.CenterPoint.Z);
                    compositionVisual?.StartAnimationGroup(group);
                }
            }
        });

        return Task.Delay(totalDuration);
    }

    private static Vector3 GetScaleOffset(int index, int total, double minOffset = 0.5) {
        double maxOffset = 0.55;
        double step = (maxOffset - minOffset) / (total - 1);

        return new((float)(maxOffset - index * step));
    }

    protected override Type StyleKeyOverride => typeof(ListPageBase);

    protected override async void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);

        await Task.Delay(250);
        if (this is IChildIndexProvider provider) {
            if (!provider.TryGetTotalCount(out _totalCount))
                return;

            foreach (var item in Items.Cast<Visual>()) {
                var index = provider.GetChildIndex(item);
                if (index is -1)
                    return;

                _delay = TimeSpan.FromMilliseconds(index * 50);
                var compositionVisual = ElementComposition.GetElementVisual(item);
                if (compositionVisual is null)
                    continue;

                var compositor = compositionVisual.Compositor;

                var scaleAni = compositor!.CreateVector3KeyFrameAnimation();
                scaleAni.Target = "Scale";
                scaleAni.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                scaleAni!.Duration = TimeSpan.FromSeconds(1);
                scaleAni.DelayTime = _delay;

                //GetScaleOffset(index, _totalCount)
                scaleAni.InsertKeyFrame(0f, new(0.7f), new ExponentialEaseOut());
                scaleAni.InsertKeyFrame(1f, Vector3.One, new ExponentialEaseOut());

                var opacityAni = compositor?.CreateScalarKeyFrameAnimation();
                opacityAni!.DelayBehavior = AnimationDelayBehavior.SetInitialValueBeforeDelay;
                opacityAni!.Target = "Opacity";

                opacityAni?.InsertKeyFrame(0f, 0f, new CubicEaseOut());
                opacityAni?.InsertKeyFrame(1f, 1f, new CubicEaseOut());
                opacityAni!.Duration = TimeSpan.FromMicroseconds(10 * (_delay.TotalMicroseconds is <= 0 ? 200 : _delay.TotalMicroseconds));
                opacityAni.DelayTime = _delay;

                var group = compositor!.CreateAnimationGroup();
                group.Add(scaleAni);
                group.Add(opacityAni);

                var size = compositionVisual!.Size;
                compositionVisual!.CenterPoint = new Vector3((float)size.X / 2, (float)size.Y, (float)compositionVisual.CenterPoint.Z);
                compositionVisual?.StartAnimationGroup(group);
            }
        }
    }
}