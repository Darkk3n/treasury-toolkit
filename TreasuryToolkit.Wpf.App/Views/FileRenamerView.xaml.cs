using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TreasuryToolkit.Wpf.App.ViewModels;

namespace TreasuryToolkit.Wpf.App.Views
{
    /// <summary>
    /// The DataGrid keeps a cell's typed value in a pending edit until the edit is committed, and clicking a
    /// button outside the grid doesn't commit it. These handlers finish that edit before the button's action
    /// runs, so the ViewModel always sees what's on screen. A button's Click handlers run before its Command.
    /// </summary>
    public partial class FileRenamerView : UserControl
    {
        public FileRenamerView()
        {
            InitializeComponent();
        }

        private FileRenamerViewModel ViewModel => (FileRenamerViewModel)DataContext;

        // "Limpiar" and "..." replace the rows anyway, so an invalid pending edit is simply discarded
        private void FinishGridEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!TryCommitGridEdit())
            {
                PaymentsGrid.CancelEdit(DataGridEditingUnit.Row);
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // v1 wouldn't let focus leave an invalid amount; here the batch just doesn't start until it's fixed
            if (!TryCommitGridEdit())
            {
                ViewModel.ReportInvalidEdit();
                return;
            }

            if (ViewModel.StartCommand.CanExecute(null))
            {
                ViewModel.StartCommand.Execute(null);
            }
        }

        private bool TryCommitGridEdit() => PaymentsGrid.CommitEdit(DataGridEditingUnit.Row, true);

        // Instance command bindings are checked before the DataGrid's own, so this disables its Ctrl+A
        private void BlockSelectAll_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = false;
            e.Handled = true;
        }
    }
}
