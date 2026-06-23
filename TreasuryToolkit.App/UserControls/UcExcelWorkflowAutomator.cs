using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using iText.Layout.Element;

namespace TreasuryToolkit.App
{
    public partial class UcExcelWorkflowAutomator : UserControl
    {
        public UcExcelWorkflowAutomator()
        {
            InitializeComponent();
            SetupGrid();
        }

        private void SetupGrid()
        {
            foreach (DataGridViewColumn column in DgvResults.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        public void ApplyTheme(bool isDarkMode)
        {
            Color controlBg = isDarkMode ? Color.FromArgb(45, 45, 48) : Color.White;
            Color textColor = isDarkMode ? Color.White : Color.FromArgb(51, 51, 51);
            Color buttonBg = isDarkMode ? Color.FromArgb(0, 122, 204) : Color.FromKnownColor(KnownColor.ControlLightLight);

            this.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromKnownColor(KnownColor.ControlLightLight);

            foreach (Control c in Controls)
            {
                if (c is Button btn)
                {
                    btn.BackColor = buttonBg;
                    btn.ForeColor = isDarkMode ? Color.White : Color.Black;
                }
                else if (c is Label)
                {
                    c.ForeColor = textColor;
                    c.BackColor = controlBg;
                }
                else if (c is DataGridView dgv)
                {
                    dgv.BackgroundColor = controlBg;
                    dgv.DefaultCellStyle.BackColor = controlBg;
                    dgv.DefaultCellStyle.ForeColor = textColor;
                    dgv.GridColor = isDarkMode ? Color.FromArgb(63, 63, 70) : Color.LightGray;
                }
            }
        }             
    }
}
