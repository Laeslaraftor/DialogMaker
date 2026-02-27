using Acly;
using DialogMaker.Lib;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Vector = System.Windows.Vector;
using WColor = System.Windows.Media.Color;

namespace DialogMaker
{
    public static class LibExtensions
    {
        public static void Alert(this Exception error)
        {
            Alerts.Show(error);
        }
        public static bool Try(Action method)
        {
            try
            {
                method();
            }
            catch (Exception error)
            {
                error.Alert();
                return false;
            }

            return true;
        }

        public static bool IsOpen(this Menu menu)
        {
            foreach (MenuItem item in menu.Items)
            {
                if (item.IsSubmenuOpen)
                {
                    return true;
                }
            }

            return false;
        }
        public static object? GetValue(this IEnumerable enumerable, int index)
        {
            int i = 0;

            foreach (var element in enumerable)
            {
                if (i == index)
                {
                    return element;
                }

                i++;
            }

            return null;
        }
        public static IEnumerable<T> EmptyEnumerable<T>()
        {
            yield break;
        }
        public static IEnumerable<T> GetValues<T>(this T flags) where T : Enum
        {
            long value = Convert.ToInt64(flags);

            foreach (Enum enumValue in Enum.GetValues(flags.GetType()))
            {
                long enumNumberValue = Convert.ToInt64(enumValue);

                if ((value & enumNumberValue) > 0)
                {
                    yield return (T)enumValue;
                }
            }
        }
        public static WColor Lerp(this WColor color, WColor color2, float value)
        {
            float r = Helper.Lerp(color.ScR, color2.ScR, value);
            float g = Helper.Lerp(color.ScG, color2.ScG, value);
            float b = Helper.Lerp(color.ScB, color2.ScB, value);
            float a = Helper.Lerp(color.ScA, color2.ScA, value);

            return WColor.FromScRgb(a, r, g, b);
        }

        public static TimeSpan ToTimeSpan(this Duration duration)
        {
            if (duration.HasTimeSpan)
            {
                return duration.TimeSpan;
            }

            return TimeSpan.Zero;
        }
        public static MediaState GetState(this MediaElement media)
        {
            var helperField = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);

            if (helperField == null)
            {
                return MediaState.Stop;
            }

            var helperObject = helperField.GetValue(media);
            var stateField = helperObject?.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
            var value = stateField?.GetValue(helperObject);

            if (value == null)
            {
                return MediaState.Stop;
            }

            return (MediaState)value;
        }

        public static Point ToPoint(this Vector vector)
        {
            return new(vector.X, vector.Y);
        }
        public static bool IsPressed(this MouseEventArgs mouse, MouseButton button)
        {
            return (mouse.LeftButton == MouseButtonState.Pressed && button == MouseButton.Left) ||
                   (mouse.RightButton == MouseButtonState.Pressed && button == MouseButton.Right) ||
                   (mouse.MiddleButton == MouseButtonState.Pressed && button == MouseButton.Middle) ||
                   (mouse.XButton1 == MouseButtonState.Pressed && button == MouseButton.XButton1) ||
                   (mouse.XButton2 == MouseButtonState.Pressed && button == MouseButton.XButton2);
        }
        public static void ForceAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryAdd(key, value))
            {
                dictionary[key] = value;
            }
        }

        extension(DependencyObject dp)
        {
            public bool TryGetParent<T>([NotNullWhen(true)] out T? result)
            {
                result = default;

                while (dp != null && dp is not T)
                {
                    dp = VisualTreeHelper.GetParent(dp);
                }

                if (dp is T typedObj)
                {
                    result = typedObj;
                    return true;
                }

                return false;
            }
            public T GetParent<T>()
            {
                while (dp != null && dp is not T)
                {
                    dp = VisualTreeHelper.GetParent(dp);
                }

                if (dp == null)
                {
                    throw new ArgumentException($"Не удалось получить родительский объект", nameof(dp));
                }

                return (T)Convert.ChangeType(dp, typeof(T));
            }
        }
        extension(Canvas canvas)
        {
            public static Point GetElementPosition(UIElement element)
            {
                double x = Canvas.GetLeft(element);
                double y = Canvas.GetTop(element);

                x = double.IsNaN(x) ? 0 : x;
                y = double.IsNaN(y) ? 0 : y;

                return new(x, y);
            }
            public static void SetElementPosition(UIElement element, Point position)
            {
                Canvas.SetLeft(element, position.X);
                Canvas.SetTop(element, position.Y);
            }
        }
        extension(FrameworkElement element)
        {
            public Window GetWindow()
            {
                if (element is Window window)
                {
                    return window;
                }

                while (element is not Window)
                {
                    var parent = VisualTreeHelper.GetParent(element);

                    if (parent is not FrameworkElement frameworkElement)
                    {
                        throw new InvalidOperationException("Не удалось найти окно, содержащее элемент");
                    }

                    element = frameworkElement;
                }

                return (Window)element;
            }
            public Point GetVisualTreeScale()
            {
                FrameworkElement? parent = element;
                Point scale = new(1, 1);

                while (parent != null)
                {
                    if (parent.TryGetTransform<ScaleTransform>(out var transform))
                    {
                        scale.X *= transform.ScaleX;
                        scale.Y *= transform.ScaleY;
                    }

                    parent = parent.Parent as FrameworkElement;
                }

                return scale;
            }
            public Point GetVisualTreeTranslation()
            {
                FrameworkElement? parent = element;
                Point translation = new(1, 1);

                while (parent != null)
                {
                    if (parent.TryGetTransform<TranslateTransform>(out var transform))
                    {
                        translation.X += transform.X;
                        translation.Y += transform.Y;
                    }

                    parent = parent.Parent as FrameworkElement;
                }

                return translation;
            }
        }
        extension(UIElement element)
        {
            public bool TryGetTransform<T>([NotNullWhen(true)] out T? result)
                where T : Transform, new()
            {
                result = null;

                if (element.RenderTransform is T typedTransform)
                {
                    result = typedTransform;
                    return true;
                }
                if (element.RenderTransform is TransformGroup group)
                {
                    foreach (var child in group.Children)
                    {
                        if (child is T typedChild)
                        {
                            result = typedChild;
                            return true;
                        }
                    }
                }

                return false;
            }
            public T GetTransform<T>() where T : Transform, new()
            {
                if (element.RenderTransform == null)
                {
                    T transform = new();
                    element.RenderTransform = transform;

                    return transform;
                }
                if (element.RenderTransform is T currentTransform)
                {
                    return currentTransform;
                }

                Transform existsTransform = element.RenderTransform;

                if (existsTransform is TransformGroup group)
                {
                    foreach (var transform in group.Children)
                    {
                        if (transform is T child)
                        {
                            return child;
                        }
                    }

                    T childTransform = new();
                    group.Children.Add(childTransform);

                    return childTransform;
                }

                T newTransform = new();
                group = new();
                group.Children.Add(existsTransform);
                group.Children.Add(newTransform);

                element.RenderTransform = group;

                return newTransform;
            }
        }
        extension(Visual visual)
        {
            public Point GetPosition(Visual relativeTo)
            {
                return visual.GetPosition(relativeTo, new(0, 0));
            }
            public Point GetPosition(Visual relativeTo, Point origin)
            {
                return visual.TransformToAncestor(relativeTo).Transform(origin);
                //try
                //{
                //}
                //catch (Exception error)
                //{
                //    error.Alert();
                //}

                //return new();
            }


        }

        public static bool IsGeometryHit(this Visual visual, Shape shape, Point position)
        {
            return false;
        }
        extension<T>(T visual) where T : Visual, IInputElement
        {
            public async Task Fetch(MouseEventArgs mouse, Action<DependencyObject> targetHandler)
            {
                await visual.Fetch(mouse, targetHandler, callback => true);
            }
            public async Task Fetch(Point position, Action<DependencyObject> targetHandler)
            {
                await visual.Fetch(position, targetHandler, callback => true);
            }
            public async Task Fetch(MouseEventArgs mouse, Action<DependencyObject> targetHandler, Predicate<HitTestResult> hitCallback)
            {
                Point position = mouse.GetPosition(visual);
                await visual.Fetch(position, targetHandler, hitCallback);
            }
            public async Task Fetch(Point position, Action<DependencyObject> targetHandler, Predicate<HitTestResult> hitCallback)
            {
                bool completed = false;
                var timeout = TimeSpan.FromSeconds(0.1);
                var startTime = DateTime.Now;

                HitTestFilterBehavior Filter(DependencyObject potentialHitTestTarget)
                {
                    targetHandler(potentialHitTestTarget);
                    return HitTestFilterBehavior.Continue;
                }
                HitTestResultBehavior Callback(HitTestResult result)
                {
                    completed = true;

                    if (((result.VisualHit is UIElement v && v.IsHitTestVisible) ||
                        result.VisualHit is not UIElement) && hitCallback(result))
                    {
                        return HitTestResultBehavior.Stop;
                    }

                    return HitTestResultBehavior.Continue;
                }

                VisualTreeHelper.HitTest(visual, Filter, Callback, new PointHitTestParameters(position));

                while (!completed && timeout > DateTime.Now - startTime)
                {
                    await Task.Delay(50);
                }
            }
            public DependencyObject Fetch(MouseEventArgs mouse)
            {
                Point position = mouse.GetPosition(visual);
                return visual.Fetch(position);
            }
            public DependencyObject Fetch(Point position)
            {
                return VisualTreeHelper.HitTest(visual, position).VisualHit;
            }
            public async Task<bool> PositionContains(MouseEventArgs mouse, Predicate<DependencyObject> predicate)
            {
                return await visual.PositionContains(mouse.GetPosition(null), predicate);
            }
            public async Task<bool> PositionContains(Point position, Predicate<DependencyObject> predicate)
            {
                bool result = false;

                await visual.Fetch(position, target =>
                {
                    if (predicate(target))
                    {
                        result = true;
                    }
                }, obj => result);

                return result;
            }

            public async Task<TElement?> Fetch<TElement>(Point position) where TElement : DependencyObject
            {
                TElement? result = null;

                await visual.Fetch(position, obj =>
                {
                    if (obj is TElement element)
                    {
                        result = element;
                    }
                });

                return result;
            }
        }
        extension(Rect rect)
        {
            public bool IntersectsWith(Point point)
            {
                return point.X >= rect.Left && 
                       point.Y >= rect.Top &&
                       rect.Right >= point.X &&
                       rect.Bottom >= point.Y;
            }

            public static Rect operator *(Rect r, Point p)
            {
                return new(r.TopLeft, r.Size * p);
            }
            public static Rect operator /(Rect r, Point p)
            {
                return new(r.TopLeft, r.Size / p);
            }
        }
        extension(Point point)
        {
            public double Distance(Point other)
            {
                return Vector2.Distance(new((float)point.X, (float)point.Y), new((float)other.X, (float)other.Y));
            }

            public static Point Clamp(Point p, double min, double max)
            {
                return new()
                {
                    X = Helper.Clamp(p.X, min, max),
                    Y = Helper.Clamp(p.Y, min, max)
                };
            }

            public static Point operator +(Point p1, Vector2 p2)
            {
                return new()
                {
                    X = p1.X + p2.X,
                    Y = p1.Y + p2.Y
                };
            }
            public static Point operator -(Point p1, Vector2 p2)
            {
                return new()
                {
                    X = p1.X - p2.X,
                    Y = p1.Y - p2.Y
                };
            }
            public static Point operator *(Point p1, Vector2 p2)
            {
                return new()
                {
                    X = p1.X * p2.X,
                    Y = p1.Y * p2.Y
                };
            }
            public static Point operator /(Point p1, Vector2 p2)
            {
                return new()
                {
                    X = p1.X / p2.X,
                    Y = p1.Y / p2.Y
                };
            }

            public static Point operator +(Point p1, Point p2)
            {
                return new()
                {
                    X = p1.X + p2.X,
                    Y = p1.Y + p2.Y
                };
            }
            public static Point operator -(Point p1, Point p2)
            {
                return new()
                {
                    X = p1.X - p2.X,
                    Y = p1.Y - p2.Y
                };
            }
            public static Point operator *(Point p1, Point p2)
            {
                return new()
                {
                    X = p1.X * p2.X,
                    Y = p1.Y * p2.Y
                };
            }
            public static Point operator /(Point p1, Point p2)
            {
                return new()
                {
                    X = p1.X / p2.X,
                    Y = p1.Y / p2.Y
                };
            }
            public static Point operator +(Point p1, Size p2)
            {
                return new()
                {
                    X = p1.X + p2.Width,
                    Y = p1.Y + p2.Height
                };
            }
            public static Point operator -(Point p1, Size s2)
            {
                return new()
                {
                    X = p1.X - s2.Width,
                    Y = p1.Y - s2.Height
                };
            }
            public static Point operator +(Point p1, double p2)
            {
                return new()
                {
                    X = p1.X + p2,
                    Y = p1.Y + p2
                };
            }
            public static Point operator -(Point p1, double s2)
            {
                return new()
                {
                    X = p1.X - s2,
                    Y = p1.Y - s2
                };
            }
            public static Point operator /(Point p1, double p2)
            {
                return new()
                {
                    X = p1.X / p2,
                    Y = p1.Y / p2
                };
            }
            public static Point operator *(Point p1, double s2)
            {
                return new()
                {
                    X = p1.X * s2,
                    Y = p1.Y * s2
                };
            }
            public static Point operator *(Point p1, Size p2)
            {
                return new()
                {
                    X = p1.X * p2.Width,
                    Y = p1.Y * p2.Height
                };
            }
            public static Point operator /(Point p1, Size p2)
            {
                return new()
                {
                    X = p1.X / p2.Width,
                    Y = p1.Y / p2.Height
                };
            }

            public static Point Lerp(Point from, Point to, double weight)
            {
                return Lerp(from, to, (float)weight);
            }
            public static Point Lerp(Point from, Point to, float weight)
            {
                return Vector2.Lerp(from.ToVector2(), to.ToVector2(), weight).ToPoint();
            }
            public Vector2 ToVector2()
            {
                return new((float)point.X, (float)point.Y);
            }
        }
        extension(Vector2 vector)
        {
            public Point ToPoint()
            {
                return new(vector.X, vector.Y);
            }

            public static Vector2 operator +(Vector2 p1, Point p2)
            {
                return new()
                {
                    X = (float)(p1.X + p2.X),
                    Y = (float)(p1.Y + p2.Y)
                };
            }
            public static Vector2 operator -(Vector2 p1, Point p2)
            {
                return new()
                {
                    X = (float)(p1.X - p2.X),
                    Y = (float)(p1.Y - p2.Y)
                };
            }
            public static Vector2 operator *(Vector2 p1, Point p2)
            {
                return new()
                {
                    X = (float)(p1.X * p2.X),
                    Y = (float)(p1.Y * p2.Y)
                };
            }
            public static Vector2 operator /(Vector2 p1, Point p2)
            {
                return new()
                {
                    X = (float)(p1.X / p2.X),
                    Y = (float)(p1.Y / p2.Y)
                };
            }
        }
        extension(Size Size)
        {
            public static Size operator +(Size s1, Size s2)
            {
                return new()
                {
                    Width = s1.Width + s2.Width,
                    Height = s1.Height + s2.Height
                };
            }
            public static Size operator -(Size s1, Size s2)
            {
                return new()
                {
                    Width = s1.Width - s2.Width,
                    Height = s1.Height - s2.Height
                };
            }
            public static Size operator +(Size p1, Point p2)
            {
                return new()
                {
                    Width = p1.Width + p2.X,
                    Height = p1.Height + p2.Y
                };
            }
            public static Size operator -(Size p1, Point p2)
            {
                return new()
                {
                    Width = p1.Width - p2.X,
                    Height = p1.Height - p2.Y
                };
            }
            public static Size operator *(Size p1, Point p2)
            {
                return new()
                {
                    Width = p1.Width * p2.X,
                    Height = p1.Height * p2.Y
                };
            }
            public static Size operator /(Size p1, Point p2)
            {
                return new()
                {
                    Width = p1.Width / p2.X,
                    Height = p1.Height / p2.Y
                };
            }
            public static Size operator *(Size p1, double value)
            {
                return new()
                {
                    Width = p1.Width * value,
                    Height = p1.Height * value
                };
            }
            public static Size operator /(Size p1, double value)
            {
                return new()
                {
                    Width = p1.Width / value,
                    Height = p1.Height / value
                };
            }
        }

        public static WColor ToWindows(this System.Drawing.Color color)
        {
            return WColor.FromArgb(color.A, color.R, color.G, color.B);
        }
        public static int IndexOf(this IEnumerable enumerable, object? item)
        {
            int index = 0;

            foreach (var i in enumerable)
            {
                if (i?.Equals(item) == true)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
        public static void RemoveFromParent(this FrameworkElement element)
        {
            if (element.Parent is Panel panel)
            {
                panel.Children.Remove(element);
                return;
            }
            if (element.Parent is Decorator decorator)
            {
                if (decorator.Child == element)
                {
                    decorator.Child = null;
                }
                return;
            }
            if (element.Parent is ContentPresenter contentPresenter)
            {
                if (contentPresenter.Content == element)
                {
                    contentPresenter.Content = null;
                }
                return;
            }
            if (element.Parent is ContentControl contentControl)
            {
                if (contentControl.Content == element)
                {
                    contentControl.Content = null;
                }
                return;
            }
        }
    }
}
