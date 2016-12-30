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

namespace GoogleCloudExtension.Controls
{
    /// <summary>
    /// Interaction logic for LinkButton.xaml
    /// </summary>
    public partial class LinkButton : Button
    {
        public LinkButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MouseOverForegroudProperty =
            DependencyProperty.Register(
                nameof(MouseOverForeground),
                typeof(Brush),
                typeof(LinkButton),
                new PropertyMetadata(Brushes.BlueViolet));

        /// <summary>
        /// The brush of foreground in the mouse over state.
        /// </summary>
        public Brush MouseOverForeground
        {
            get { return (Brush)GetValue(MouseOverForegroudProperty); }
            set { SetValue(MouseOverForegroudProperty, value); }
        }
    }
}
