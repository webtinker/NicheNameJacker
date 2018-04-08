using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Application.UserControls
{
    /// <summary>
    /// Interaction logic for SimpleProgress.xaml
    /// </summary>
    public partial class SimpleProgress : UserControl
    {
        public SimpleProgress()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (progressAnimation == null)
            {
                progressAnimation = GetAnimationStoryboard();
            }
            if (Visibility == Visibility.Visible)
            {
                StartAnimation();
            }
        }

        private Storyboard progressAnimation;

        private void StartAnimation()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            var milis = rand.Next(0, 400);
            progressAnimation.BeginTime = TimeSpan.FromMilliseconds(milis);
            progressAnimation.Begin(this, true);
        }

        public SolidColorBrush ProgressBackground
        {
            get { return (SolidColorBrush)GetValue(ProgressBackgroundProperty); }
            set { SetValue(ProgressBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressBackgroundProperty =
            DependencyProperty.Register("ProgressBackground", typeof(SolidColorBrush), typeof(SimpleProgress), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), new PropertyChangedCallback(ProgressBackgroundChanged)));


        private static void ProgressBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs ea)
        {
            if (ea.NewValue != ea.OldValue)
            {
                var simpleProgress = obj as SimpleProgress;
                var brush = ea.NewValue as SolidColorBrush;

                simpleProgress.ellipse1.Fill = brush;
                simpleProgress.ellipse2.Fill = brush;
                simpleProgress.ellipse3.Fill = brush;
            }
        }

        public SolidColorBrush ProgressForeground
        {
            get { return (SolidColorBrush)GetValue(ProgressForegroundProperty); }
            set { SetValue(ProgressForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressForegroundProperty =
            DependencyProperty.Register("ProgressForeground", typeof(SolidColorBrush), typeof(SimpleProgress), new PropertyMetadata(new SolidColorBrush(Colors.Transparent), new PropertyChangedCallback(ProgressForegroundChanged)));

        private static void ProgressForegroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs ea)
        {
        }


        public double DotSize
        {
            get { return (double)GetValue(DotSizeProperty); }
            set { SetValue(DotSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DotRadias.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DotSizeProperty =
            DependencyProperty.Register("DotSize", typeof(double), typeof(SimpleProgress), new PropertyMetadata((double)5));


        private void userControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsLoaded)
            {
                var sp = sender as SimpleProgress;
                if (sp.Visibility == Visibility.Visible)
                {
                    if (progressAnimation == null)
                    {
                        progressAnimation = GetAnimationStoryboard();
                    }
                    StartAnimation();
                }
                else
                {
                    progressAnimation.Stop(this);
                }
            }

        }


        private Storyboard GetAnimationStoryboard()
        {
            DependencyObject[] objects = new DependencyObject[] { ellipse1, ellipse2, ellipse3 };

            var bgColor = ProgressBackground.Color;
            var fgColor = ProgressForeground.Color;

            Storyboard sb = new Storyboard();
            sb.RepeatBehavior = RepeatBehavior.Forever;

            double scale = 1.2;

            int duration = 150;
            TimeSpan totalDuration = new TimeSpan(0, 0, 0, 0, duration);

            int itemNumber = 0;
            int beginTime = 0;
            foreach (var key in objects)
            {
                beginTime = itemNumber * duration;

                sb.Children.Add(GetDoubleAnimationFor(key, "RenderTransform.ScaleX", 1, scale, TimeSpan.FromMilliseconds(beginTime), totalDuration));
                sb.Children.Add(GetDoubleAnimationFor(key, "RenderTransform.ScaleY", 1, scale, TimeSpan.FromMilliseconds(beginTime), totalDuration));
                sb.Children.Add(GetColorAnimationFor(key, "(Path.Fill).(SolidColorBrush.Color)", bgColor, fgColor, TimeSpan.FromMilliseconds(beginTime), totalDuration));

                sb.Children.Add(GetDoubleAnimationFor(key, "RenderTransform.ScaleX", scale, 1, TimeSpan.FromMilliseconds(beginTime + duration), totalDuration));
                sb.Children.Add(GetDoubleAnimationFor(key, "RenderTransform.ScaleY", scale, 1, TimeSpan.FromMilliseconds(beginTime + duration), totalDuration));
                sb.Children.Add(GetColorAnimationFor(key, "(Path.Fill).(SolidColorBrush.Color)", fgColor, bgColor, TimeSpan.FromMilliseconds(beginTime + duration), totalDuration));

                itemNumber++;
            }

            beginTime = itemNumber * duration;
            sb.Children.Add(GetDoubleAnimationFor(objects[0], "RenderTransform.ScaleX", null, null, TimeSpan.FromMilliseconds(beginTime), totalDuration));

            return sb;
        }

        private DoubleAnimation GetDoubleAnimationFor(DependencyObject target, string targetProperty, Nullable<double> from, Nullable<double> to, TimeSpan beginTime, TimeSpan duration)
        {
            var da = new DoubleAnimation
            {
                BeginTime = beginTime,
                Duration = duration,
                From = from,
                To = to
            };

            Storyboard.SetTarget(da, target);
            Storyboard.SetTargetProperty(da, new PropertyPath(targetProperty));

            return da;
        }

        private ColorAnimation GetColorAnimationFor(DependencyObject target, string targetProperty, Nullable<Color> from, Nullable<Color> to, TimeSpan beginTime, TimeSpan duration)
        {
            var ca = new ColorAnimation
            {
                BeginTime = beginTime,
                Duration = duration,
                From = from,
                To = to
            };

            Storyboard.SetTarget(ca, target);
            Storyboard.SetTargetProperty(ca, new PropertyPath(targetProperty));

            return ca;
        }


    }

    public class DivideByValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                return double.Parse(value.ToString()) / double.Parse(parameter.ToString());
            }
            catch
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
