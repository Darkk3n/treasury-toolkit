using System.Media;
using System.Windows;

namespace TreasuryToolkit.Wpf.App.Views
{
    public partial class MessageWindow : Window
    {
        public MessageWindow(string title, string message, string primaryButtonText, string secondaryButtonText)
        {
            InitializeComponent();
            Title = title;
            MessageText.Text = message;
            PrimaryButton.Content = primaryButtonText;
            SecondaryButton.Content = secondaryButtonText;

            // Same alert sound MessageBox plays for a warning
            Loaded += (_, _) => SystemSounds.Exclamation.Play();
        }

        // The secondary button is IsCancel, which closes the window with DialogResult false on its own
        private void Primary_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
