//------------------------------------------------------------------------------
// <copyright file="ErrorReportingDetailToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    using GoogleCloudExtension.Utils;
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for ErrorReportingDetailToolWindowControl.
    /// </summary>
    public partial class ErrorReportingDetailToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorReportingDetailToolWindowControl"/> class.
        /// </summary>
        public ErrorReportingDetailToolWindowControl()
        {
            this.InitializeComponent();
            ViewModel = new ErrorReportingDetailViewModel();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DataContext = ViewModel;
            autoReloadToggleButton.AutoReload += (sender, e) => ViewModel.UpdateGroupAndEventAsync();
        }

        public ErrorReportingDetailViewModel ViewModel { get; }

        /// <summary>
        /// Get the first ancestor control element of type TControl.
        /// </summary>
        /// <typeparam name="TControl">A <seealso cref="Control"/> type.</typeparam>
        /// <param name="dependencyObj">A <seealso cref="DependencyObject"/> element. </param>
        /// <returns>null or TControl object.</returns>
        private TControl FindAncestorControl<TControl>(DependencyObject dependencyObj) where TControl : Control
        {
            while ((dependencyObj != null) && !(dependencyObj is TControl))
            {
                dependencyObj = VisualTreeHelper.GetParent(dependencyObj);
            }

            return dependencyObj as TControl;  // Note, null as Class is val 
        }

        /// <summary>
        /// Response to data grid scroll change event.
        /// Auto load more logs when it scrolls down to bottom.
        /// </summary>
        private void dataGrid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            ScrollViewer sv = e.OriginalSource as ScrollViewer;
            if (sv == null)
            {
                return;
            }

            if (e.VerticalOffset == sv.ScrollableHeight)
            {
                Debug.WriteLine("Now it is at bottom");
                //ViewModel?.LoadNextPage();
            }
        }

        /// <summary>
        /// When mouse click on a row, toggle display the row detail.
        /// 
        /// Note, it is necessay to find cell before find row. 
        /// Otherwise when clicking at the detail view area, it 'finds' the DataGridRow too.
        /// </summary>
        private void dataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var cell = FindAncestorControl<DataGridCell>(e.OriginalSource as DependencyObject);
            DataGridRow row = FindAncestorControl<DataGridRow>(cell);
            if (row != null)
            {
                row.DetailsVisibility =
                    row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}