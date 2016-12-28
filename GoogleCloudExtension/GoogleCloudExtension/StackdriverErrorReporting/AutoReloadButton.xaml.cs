using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoogleCloudExtension.StackdriverErrorReporting
{
    /// <summary>
    /// Interaction logic for AutoReloadButton.xaml
    /// </summary>
    public partial class AutoReloadButton : UserControl
    {
        public event EventHandler AutoReload;

        private bool _isChecked = false;

        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                if (value)
                {
                    _timer.Start();
                    AutoReload?.Invoke(this, new EventArgs());
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        private readonly DispatcherTimer _timer;

        public AutoReloadButton()
        {
            InitializeComponent();

            //  DispatcherTimer setup
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(10);
            _timer.Tick += (sender, e) => AutoReload?.Invoke(sender, e);
        }
    }
}
