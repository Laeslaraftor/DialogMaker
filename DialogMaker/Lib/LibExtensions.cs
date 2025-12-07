using DialogMaker.Lib;
using System.Collections;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
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

        extension (Visual visual)
        {
            public Point GetPosition(Visual relativeTo)
            {
                return visual.GetPosition(relativeTo, new(0, 0));
            }
            public Point GetPosition(Visual relativeTo, Point origin)
            {
                return visual.TransformToAncestor(relativeTo).Transform(origin);
            }

            
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

                HitTestFilterBehavior Filter(DependencyObject potentialHitTestTarget)
                {
                    targetHandler(potentialHitTestTarget);
                    return HitTestFilterBehavior.Continue;
                }
                HitTestResultBehavior Callback(HitTestResult result)
                {
                    completed = true;

                    if (hitCallback(result))
                    {
                        return HitTestResultBehavior.Stop;
                    }

                    return HitTestResultBehavior.Continue;
                }

                VisualTreeHelper.HitTest(visual, Filter, Callback, new PointHitTestParameters(position));

                while (!completed)
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
        extension(Point point)
        {
            public double Distance(Point other)
            {
                return Vector2.Distance(new((float)point.X, (float)point.Y), new((float)other.X, (float)other.Y));
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
    }
}
