using System.Globalization;
using System.Windows.Controls;

namespace TreasuryToolkit.Wpf.App.Validation
{
    /// <summary>
    /// Rejects non-numeric amounts while the cell is edited, so the DataGrid keeps the cell in edit mode until it's
    /// fixed or Esc is pressed (v1's CellValidating with e.Cancel). Empty is allowed. Values loaded from a scan are
    /// not checked, because the rule only runs on user edits.
    /// </summary>
    public class AmountValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string input = value as string;
            if (string.IsNullOrEmpty(input) || double.TryParse(input, out _))
            {
                return ValidationResult.ValidResult;
            }

            return new ValidationResult(false, "Monto: Solo se permiten numeros.");
        }
    }
}
