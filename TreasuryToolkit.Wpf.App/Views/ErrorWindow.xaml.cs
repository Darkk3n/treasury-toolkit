using System.Windows;
using System.Windows.Threading;

namespace TreasuryToolkit.Wpf.App.Views
{
    /// <summary>
    /// Shows a prepared error report (see ErrorReportBuilder). Code-behind only handles view concerns:
    /// copying the text and the "copied" notice.
    /// </summary>
    public partial class ErrorWindow : Window
    {
        private readonly DispatcherTimer copiedNoticeTimer = new() { Interval = TimeSpan.FromSeconds(2) };

        public ErrorWindow(string report)
        {
            InitializeComponent();
            DetailsText.Text = report;

            copiedNoticeTimer.Tick += (_, _) =>
            {
                CopiedNotice.Visibility = Visibility.Collapsed;
                copiedNoticeTimer.Stop();
            };
        }

        private void CopyDetails_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(DetailsText.Text);
            CopiedNotice.Visibility = Visibility.Visible;
            copiedNoticeTimer.Stop();
            copiedNoticeTimer.Start();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
