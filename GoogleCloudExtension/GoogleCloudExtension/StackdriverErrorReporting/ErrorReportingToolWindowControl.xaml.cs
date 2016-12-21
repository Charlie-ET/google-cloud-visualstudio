//------------------------------------------------------------------------------
// <copyright file="ErrorReportingToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using GoogleCloudExtension;
using GoogleCloudExtension.Accounts;
using GoogleCloudExtension.DataSources.ErrorReporting;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    /// <summary>
    /// Interaction logic for ErrorReportingToolWindowControl.
    /// </summary>
    public partial class ErrorReportingToolWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorReportingToolWindowControl"/> class.
        /// </summary>
        public ErrorReportingToolWindowControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(
            //    string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
            //    "ErrorReportingToolWindow");

            TestGetGroupStats();
        }

        private async void TestGetGroupStats()
        {
            var ds = new SerDataSource(CredentialsStore.Default.CurrentProjectId,
                CredentialsStore.Default.CurrentGoogleCredential,
                GoogleCloudExtensionPackage.ApplicationName);

            var results = await ds.ListGroupStatusAsync();
        }
    }
}               