using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InkCloud_Launcher.Media.Transitions.Page;

public sealed class DefaultPageTransitions : IPageTransition {
    public TimeSpan Duration { get; set; }
    public Easing Easing { get; set; } = new CircularEaseInOut();

    public DefaultPageTransitions(TimeSpan duration, Easing easing) {
        Easing = easing;
        Duration = duration;
    }

    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken) {
        if (cancellationToken.IsCancellationRequested)
            return;

        List<Task> tasks = [];
        var parent = GetVisualParent(from, to);
        var distance = parent.Bounds.Height;
        var controlRenderTransfrom = from!.RenderTransform as TranslateTransform;

        double start = controlRenderTransfrom?.Y ?? 0d;

        if (from is not null) {
            double end = forward ? -distance : distance;
            var animation = new Animation {
                Easing = Easing,
                Duration = Duration,
                FillMode = FillMode.Forward,
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0d),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0d),
                            new Setter(TranslateTransform.YProperty, start),
                            new Setter(ScaleTransform.ScaleXProperty, 1d),
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(0.5d),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0d),
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1d),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0d),
                            new Setter(TranslateTransform.YProperty, end),
                            new Setter(ScaleTransform.ScaleXProperty, 0.35d),
                        }
                    }
                }
            };

            tasks.Add(animation.RunAsync(from!, cancellationToken));
        }

        if (to is not null) {
            to.IsVisible = true;
            double end = forward ? distance : -distance;

            var animation = new Animation {
                Easing = Easing,
                Duration = Duration,
                FillMode = FillMode.Forward,
                Children = {
                    new KeyFrame {
                        Cue = new Cue(0d),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 0.0d),
                            new Setter(TranslateTransform.YProperty, end),
                            new Setter(ScaleTransform.ScaleXProperty, 0.35d),
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(0.5d),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0d),
                        }
                    },
                    new KeyFrame {
                        Cue = new Cue(1d),
                        Setters = {
                            new Setter(Visual.OpacityProperty, 1.0d),
                            new Setter(TranslateTransform.YProperty, 0d),
                            new Setter(ScaleTransform.ScaleXProperty, 1d),
                        }
                    }
                }
            };

            tasks.Add(animation.RunAsync(to!, cancellationToken));
        }

        await Task.WhenAll(tasks);

        if (from != null && !cancellationToken.IsCancellationRequested)
            from.IsVisible = false;
    }

    private static Visual GetVisualParent(Visual? from, Visual? to) {
        var p1 = (from ?? to)!.GetVisualParent();
        var p2 = (to ?? from)!.GetVisualParent();

        if (p1 != null && p2 != null && p1 != p2)
            throw new ArgumentException("Controls for PageSlide must have same parent.");

        return p1 ??
            throw new InvalidOperationException("Cannot determine visual parent.");
    }
}