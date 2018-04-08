using System;
using System.Collections.Generic;
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

namespace NicheNameJacker.Controls
{
    /// <summary>
    /// Interaction logic for SimpleNotificationPopup.xaml
    /// </summary>
    public partial class SimpleNotificationPopup : UserControl
    {
        public SimpleNotificationPopup()
        {
            InitializeComponent();
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (showPopup == null)
            {
                showPopup = GetShowPopupAnimation();
            }
            if (hidePopup == null)
            {
                hidePopup = GetHidePopupAnimation();
            }

            //Initially hide the control once layout bounds are calculated and animations are set.
            Visibility = Visibility.Collapsed;
        }

        Storyboard showPopup;
        Storyboard hidePopup;

        public Visibility PopupVisibility
        {
            get { return (Visibility)GetValue(PopupVisibilityProperty); }
            set { SetValue(PopupVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PopupVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PopupVisibilityProperty =
            DependencyProperty.Register("PopupVisibility", typeof(Visibility), typeof(SimpleNotificationPopup), new PropertyMetadata(Visibility.Hidden, popupVisibilityChanged));

        private static async void popupVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Visibility v = (Visibility)e.NewValue;
            var p = d as SimpleNotificationPopup;

            if (v == Visibility.Visible)
            {
                //To let the control get loaded.
                await Task.Delay(200);

                p.Visibility = Visibility.Visible;
                p.notification.Visibility = Visibility.Visible;
                p.showPopup?.Begin(p, true);
            }
            else
            {
                p.hidePopup?.Begin(p, true);
            }
        }


        public Brush TextColor
        {
            get { return (Brush)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register("TextColor", typeof(Brush), typeof(SimpleNotificationPopup), new PropertyMetadata(new SolidColorBrush(Colors.White)));


        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SimpleNotificationPopup), new PropertyMetadata(""));



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PopupVisibility = Visibility.Hidden;
            //Visibility = Visibility.Collapsed;
        }

        private Storyboard GetShowPopupAnimation()
        {
            Storyboard sb = new Storyboard();
            sb.Children.Add(GetDoubleAnimationFor(notification, "Height", 0, notification.ActualHeight, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(300), false));
            return sb;
        }

        private Storyboard GetHidePopupAnimation()
        {
            Storyboard sb = new Storyboard();

            sb.Children.Add(GetDoubleAnimationFor(notification, "Height", notification.ActualHeight, 0, TimeSpan.FromMilliseconds(0), TimeSpan.FromMilliseconds(300), false));
            return sb;
        }

        private DoubleAnimation GetDoubleAnimationFor(DependencyObject target, string targetProperty, double? from, double? to, TimeSpan beginTime, TimeSpan duration, bool autoReverse)
        {
            var da = new DoubleAnimation
            {
                BeginTime = beginTime,
                Duration = duration,
                From = from,
                To = to,
                AutoReverse = autoReverse
            };

            Storyboard.SetTarget(da, target);
            Storyboard.SetTargetProperty(da, new PropertyPath(targetProperty));

            return da;
        }


    }
}
