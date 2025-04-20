using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;
using InkCloud_Launcher.Media.Transitions.Page;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace InkCloud_Launcher.Resources.Controls;

public sealed partial class ListPageHost : ContentControl {
    private ControlType _controlType;
    private ContentPresenter? _control1;
    private ContentPresenter? _control2;
    private CancellationTokenSource? _cancellationTokenSource = new();

    public static readonly StyledProperty<object> ActualPageContentProperty =
        AvaloniaProperty.Register<ListPageBase, object>(nameof(ActualPageContent));

    public static readonly StyledProperty<IPageTransition> PageTransitionProperty =
        AvaloniaProperty.Register<ListPageHost, IPageTransition>(nameof(PageTransition),
            new DefaultPageTransitions(TimeSpan.FromSeconds(1)));

    public IPageTransition PageTransition {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    public object ActualPageContent {
        get => GetValue(ActualPageContentProperty);
        set => SetValue(ActualPageContentProperty, value);
    }

    public async Task NavigationAsync(object control) {
        //flyout old page.
        if (ActualPageContent is ListPageBase lp)
            await lp.RunUnLoadedAnimationAsync();

        NavigationCore(control);
    }

    private void NavigationCore(object control) {
        using (_cancellationTokenSource) {
            _cancellationTokenSource!.Cancel();
            _cancellationTokenSource = new();
        }

        Dispatcher.UIThread.Post(async () => {
            ActualPageContent = control;

            if (_controlType is ControlType.Control1) {
                _control1!.Content = control;

                if (PageTransition != null) {
                    await PageTransition.Start(_control2, _control1, true, _cancellationTokenSource.Token);
                } else {
                    _control2!.IsVisible = false;
                    _control1.IsVisible = true;
                }

                _controlType = ControlType.Control2;
            } else {
                _control2!.Content = control;

                if (PageTransition != null) {
                    await PageTransition.Start(_control1, _control2, false, _cancellationTokenSource.Token);
                } else {
                    _control1!.IsVisible = false;
                    _control2.IsVisible = true;
                }

                _controlType = ControlType.Control1;
            }
        });
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);

        _control2 = e.NameScope.Find<ContentPresenter>("PART_ToContentPresenter");
        _control1 = e.NameScope.Find<ContentPresenter>("PART_FromContentPresenter");
    }
}

enum ControlType {
    Control1,
    Control2
}