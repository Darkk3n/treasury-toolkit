using System.Media;
using System.Windows;

namespace TreasuryToolkit.Wpf.App.Views
{
    public enum MessageKind
    {
        Information,
        Warning,
        Error,
        Question
    }

    public partial class MessageWindow : Window
    {
        /// <param name="secondaryButtonText">Null shows only the primary button (a plain alert).</param>
        public MessageWindow(string title, string message, MessageKind kind, string primaryButtonText, string secondaryButtonText = null)
        {
            InitializeComponent();
            HeadingText.Text = title;
            MessageText.Text = message;

            // Title bar left empty (the heading shows the title); screen readers still announce it
            System.Windows.Automation.AutomationProperties.SetName(this, title);
            PrimaryButton.Content = primaryButtonText;

            if (secondaryButtonText == null)
            {
                // A single button also answers Esc, like a MessageBox with only OK
                SecondaryButton.Visibility = Visibility.Collapsed;
                PrimaryButton.IsCancel = true;
            }
            else
            {
                SecondaryButton.Content = secondaryButtonText;
            }

            var (glyph, brushKey, sound) = kind switch
            {
                MessageKind.Warning => ("", "WarningBrush", SystemSounds.Exclamation),
                MessageKind.Error => ("", "ErrorBrush", SystemSounds.Hand),
                MessageKind.Question => ("", "AccentBrush", SystemSounds.Question),
                _ => ("", "AccentBrush", SystemSounds.Asterisk)
            };

            IconText.Text = glyph;
            IconText.SetResourceReference(ForegroundProperty, brushKey);

            // Same alert sounds the native MessageBox plays for each icon
            Loaded += (_, _) => sound.Play();
        }

        // The secondary button is IsCancel, which closes the window with DialogResult false on its own
        private void Primary_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    }
}
