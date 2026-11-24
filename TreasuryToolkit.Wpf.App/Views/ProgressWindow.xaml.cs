using System.Windows;

namespace TreasuryToolkit.Wpf.App.Views
{
    public partial class ProgressWindow : Window
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }

        public string Status
        {
            get => StatusText.Text;
            set => StatusText.Text = value;
        }
    }
}
