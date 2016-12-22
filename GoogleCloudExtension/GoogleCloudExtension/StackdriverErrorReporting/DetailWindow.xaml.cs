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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using GoogleCloudExtension;
using GoogleCloudExtension.Accounts;
using GoogleCloudExtension.DataSources.ErrorReporting;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class DetailWindow : Window
    {
        private static DetailWindow _instance;
        public static DetailWindow Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DetailWindow();
                }

                return _instance;
            }
        }
        public ErrorReportingDetailViewModel ViewModel { get; }

        public DetailWindow()
        {
            InitializeComponent();
            ViewModel = new ErrorReportingDetailViewModel();
            detailControl.DataContext = ViewModel;
        }
    }
}
