using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TreasuryToolkit.App
{
    public partial class UcExcelWorkflowAutomator : UserControl
    {
        public UcExcelWorkflowAutomator()
        {
            InitializeComponent();
        }

        public void ApplyTheme(bool isDarkMode)
        {
            this.BackColor = isDarkMode ? Color.FromArgb(30, 30, 30) : Color.FromKnownColor(KnownColor.ControlLightLight);
        }
    }
}
