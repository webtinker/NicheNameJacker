using NicheNameJacker.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Application.UserControls
{
    /// <summary>
    /// Interaction logic for BanerControl.xaml
    /// </summary>
    public partial class BanerControl : UserControl
    {
        #region Contructor      
        public BanerControl()
        {
            InitializeComponent();
        }
        #endregion Contructor

        public ImageSource ImageSource
        {
            get
            {
                return (ImageSource)GetValue(ImageSourceProperty);
            }
            set
            {
                SetValue(ImageSourceProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.RegisterAttached("ImageSource", typeof(ImageSource), typeof(BanerControl));


        /// <summary>Gets or sets hyperlink of banner.</summary>
        public string HyperLink
        {
            get
            {
                return (string)GetValue(HyperLinkProperty);
            }
            set
            {
                SetValue(HyperLinkProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for HyperLink.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HyperLinkProperty =
            DependencyProperty.Register("HyperLink", typeof(string), typeof(BanerControl), new PropertyMetadata(""));
        #region Commands
        /// <summary>Command on banner clicked.</summary>
        public ICommand BannerClicked
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (string.IsNullOrEmpty(HyperLink))
                        {
                            return;
                        }

                        Process.Start(HyperLink);
                    }
                    catch
                    { }
                });
            }
        }
        #endregion Commands    
    }
}
