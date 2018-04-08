using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls.Primitives;

namespace NicheNameJacker.Controls
{
    public class ScrollableTabControl : TabControl
    {
        private const int ScrollStep = 25;

        private RepeatButton tabLeftButton;
        private RepeatButton tabRightButton;
        private Button tabAddItemButton;
        private ScrollViewer tabScrollViewer;
        private Panel tabPanelTop;

        static ScrollableTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollableTabControl), new FrameworkPropertyMetadata(typeof(ScrollableTabControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            tabLeftButton = GetTemplateChild("TabLeftButtonTop") as RepeatButton;
            tabRightButton = GetTemplateChild("TabRightButtonTop") as RepeatButton;
            tabScrollViewer = GetTemplateChild("TabScrollViewerTop") as ScrollViewer;
            tabPanelTop = GetTemplateChild("HeaderPanel") as Panel;
            tabAddItemButton = GetTemplateChild("TabAddItemButton") as Button;

            if (tabLeftButton != null)
                tabLeftButton.Click += tabLeftButton_Click;

            if (tabRightButton != null)
                tabRightButton.Click += tabRightButton_Click;

            if (tabAddItemButton != null)
            {
                tabAddItemButton.Click += tabAddItemButton_Click;
                tabAddItemButton.Visibility = isAddItemEnabled ? Visibility.Visible : Visibility.Collapsed;
            }

            tabScrollViewer.Loaded += (s, e) => UpdateScrollButtonsAvailability();
            tabScrollViewer.ScrollChanged += (s, e) => UpdateScrollButtonsAvailability();

            SelectionChanged += (s, e) => ScrollToSelectedItem();

        }

        public object TabHeaderAdditionalContent
        {
            get { return (object)GetValue(TabHeaderAdditionalContentProperty); }
            set { SetValue(TabHeaderAdditionalContentProperty, value); }
        }
        
        public static readonly DependencyProperty TabHeaderAdditionalContentProperty =
            DependencyProperty.Register("TabHeaderAdditionalContent", typeof(object), typeof(ScrollableTabControl), new PropertyMetadata(0));

        #region Add item functionality

        private bool isAddItemEnabled;

        public bool IsAddItemEnabled
        {
            get { return isAddItemEnabled; }
            set
            {
                isAddItemEnabled = value;

                if (tabAddItemButton != null)
                    tabAddItemButton.Visibility = isAddItemEnabled ? Visibility.Visible : Visibility.Collapsed;
            }
        }


        public ICommand AddItemCommand
        {
            get { return (ICommand)GetValue(AddItemCommandProperty); }
            set { SetValue(AddItemCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddItemCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddItemCommandProperty =
            DependencyProperty.Register("AddItemCommand", typeof(ICommand), typeof(ScrollableTabControl), new PropertyMetadata(null));


        private void tabAddItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (AddItemCommand != null && AddItemCommand.CanExecute(null))
                AddItemCommand.Execute(null);
        }
        #endregion

        #region Scrollable tabs
        /// <summary>
        /// Gets or sets the Tab Top Left Button style.
        /// </summary>
        /// <value>The left button style style.</value>
        [Description("Gets or sets the tab top left button style")]
        [Category("ScrollButton")]
        public Style TabLeftButtonTopStyle
        {
            get { return (Style)GetValue(TabLeftButtonTopStyleProperty); }
            set { SetValue(TabLeftButtonTopStyleProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Tab Top Right Button style.
        /// </summary>
        /// <value>The left button style style.</value>
        [Description("Gets or sets the tab top right button style")]
        [Category("ScrollButton")]
        public Style TabRightButtonTopStyle
        {
            get { return (Style)GetValue(TabRightButtonTopStyleProperty); }
            set { SetValue(TabRightButtonTopStyleProperty, value); }
        }

        /// <summary>
        /// Tab Top Left Button style
        /// </summary>
        public static readonly DependencyProperty TabLeftButtonTopStyleProperty = DependencyProperty.Register(
            "TabLeftButtonTopStyle",
            typeof(Style),
            typeof(ScrollableTabControl),
            new PropertyMetadata(null));

        /// <summary>
        /// Tab Top Left Button style
        /// </summary>
        public static readonly DependencyProperty TabRightButtonTopStyleProperty = DependencyProperty.Register(
            "TabRightButtonTopStyle",
            typeof(Style),
            typeof(ScrollableTabControl),
            new PropertyMetadata(null));

        private void tabRightButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != tabScrollViewer && null != tabPanelTop)
            {
                // added margin left for sure that the item will be scrolled
                var rightItemOffset = Math.Min(tabScrollViewer.HorizontalOffset + tabScrollViewer.ViewportWidth + tabPanelTop.Margin.Left, tabScrollViewer.ExtentWidth);

                var rightItem = GetItemByOffset(rightItemOffset);
                ScrollToItem(rightItem);
            }
        }
        private void tabLeftButton_Click(object sender, RoutedEventArgs e)
        {
            if (null != tabScrollViewer)
            {
                var leftItemOffset = Math.Max(tabScrollViewer.HorizontalOffset - tabPanelTop.Margin.Left, 0);

                var leftItem = GetItemByOffset(leftItemOffset);
                ScrollToItem(leftItem);
            }

        }

        /// <summary>
        /// Change visibility and avalability of buttons if it is necessary
        /// </summary>
        /// <param name="horizontalOffset">the real offset instead of outdated one from the scroll viewer</param>
        private void UpdateScrollButtonsAvailability(double? horizontalOffset = null)
        {
            if (tabScrollViewer == null) return;

            var hOffset = horizontalOffset ?? tabScrollViewer.HorizontalOffset;
            hOffset = Math.Max(hOffset, 0);

            var scrWidth = tabScrollViewer.ScrollableWidth;
            scrWidth = Math.Max(scrWidth, 0);

            if (tabLeftButton != null)
            {
                tabLeftButton.Visibility = scrWidth == 0 ? Visibility.Collapsed : Visibility.Visible;

                //tabLeftButton.IsEnabled = hOffset > 0;
            }
            if (tabRightButton != null)
            {
                tabRightButton.Visibility = scrWidth == 0 ? Visibility.Collapsed : Visibility.Visible;

                //tabRightButton.IsEnabled = hOffset < scrWidth;
            }
        }

        /// <summary>
        /// Scrolls to a selected tab
        /// </summary>
        private void ScrollToSelectedItem()
        {
            var model = base.SelectedItem;
            var si = ItemContainerGenerator.ContainerFromItem(model) as TabItem;
            if (si == null || tabScrollViewer == null)
                return;
            if (si.ActualWidth == 0 && !si.IsLoaded)
            {
                si.Loaded += (s, e) => ScrollToSelectedItem();
                return;
            }

            ScrollToItem(si);
        }

        /// <summary>
        /// Scrolls to the specified tab
        /// </summary>
        private void ScrollToItem(TabItem si)
        {
            var tabItems = Items.Cast<object>()
                .Select(item => ItemContainerGenerator.ContainerFromItem(item) as TabItem);

            var leftItems = tabItems
                .Where(ti => ti != null)
                .TakeWhile(ti => ti != si).ToList();

            var leftItemsWidth = leftItems.Sum(ti => ti.ActualWidth);

            //If the selected item is situated somewhere at the right area
            if (leftItemsWidth + si.ActualWidth > tabScrollViewer.HorizontalOffset + tabScrollViewer.ViewportWidth)
            {
                var currentHorizontalOffset = (leftItemsWidth + si.ActualWidth) - tabScrollViewer.ViewportWidth;
                // the selected item has extra width, so I add it to the offset
                var hMargin = !leftItems.Any(ti => ti.IsSelected) && !si.IsSelected ? tabPanelTop.Margin.Left + tabPanelTop.Margin.Right : 0;
                currentHorizontalOffset += hMargin;

                tabScrollViewer.ScrollToHorizontalOffset(currentHorizontalOffset);
            }
            //if the selected item somewhere at the left
            else if (leftItemsWidth < tabScrollViewer.HorizontalOffset)
            {
                var currentHorizontalOffset = leftItemsWidth;
                // the selected item has extra width, so I remove it from the offset
                var hMargin = leftItems.Any(ti => ti.IsSelected) ? tabPanelTop.Margin.Left + tabPanelTop.Margin.Right : 0;
                currentHorizontalOffset -= hMargin;

                tabScrollViewer.ScrollToHorizontalOffset(currentHorizontalOffset);
            }
        }

        /// <summary>
        /// Returns the tab item by using some kind of a hit-test
        /// </summary>
        /// <param name="offset">the absolute coordinate in pixels starting from the left</param>
        private TabItem GetItemByOffset(double offset)
        {
            var tabItems = Items.Cast<object>()
                .Select(item => ItemContainerGenerator.ContainerFromItem(item) as TabItem)
                .ToList();

            double currentItemsWidth = 0;
            // get tabs one by one and calculate their aggregated width until the offset value is reached
            foreach (var ti in tabItems)
            {
                if (currentItemsWidth + ti.ActualWidth >= offset)
                    return ti;

                currentItemsWidth += ti.ActualWidth;
            }

            return tabItems.LastOrDefault();
        }

        #endregion
    }
}
