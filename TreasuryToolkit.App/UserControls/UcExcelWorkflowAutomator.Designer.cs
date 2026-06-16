namespace TreasuryToolkit.App
{
    partial class UcExcelWorkflowAutomator
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            DgvResults = new DataGridView();
            LblStatus = new Label();
            BtnProcess = new Button();
            ((System.ComponentModel.ISupportInitialize)DgvResults).BeginInit();
            SuspendLayout();
            // 
            // DgvResults
            // 
            DgvResults.BackgroundColor = Color.White;
            DgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DgvResults.Dock = DockStyle.Bottom;
            DgvResults.Location = new Point(0, 129);
            DgvResults.Name = "DgvResults";
            DgvResults.Size = new Size(858, 310);
            DgvResults.TabIndex = 0;
            // 
            // LblStatus
            // 
            LblStatus.AutoSize = true;
            LblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LblStatus.Location = new Point(15, 10);
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(151, 21);
            LblStatus.TabIndex = 1;
            LblStatus.Text = "Procesando Archivo:";
            // 
            // BtnProcess
            // 
            BtnProcess.BackColor = SystemColors.ControlLightLight;
            BtnProcess.FlatStyle = FlatStyle.Flat;
            BtnProcess.Location = new Point(15, 43);
            BtnProcess.Name = "BtnProcess";
            BtnProcess.Size = new Size(37, 36);
            BtnProcess.TabIndex = 2;
            BtnProcess.Text = "...";
            BtnProcess.UseVisualStyleBackColor = false;
            // 
            // UcExcelWorkflowAutomator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            Controls.Add(BtnProcess);
            Controls.Add(LblStatus);
            Controls.Add(DgvResults);
            Name = "UcExcelWorkflowAutomator";
            Size = new Size(858, 439);
            ((System.ComponentModel.ISupportInitialize)DgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView DgvResults;
        private Label LblStatus;
        private Button BtnProcess;
    }
}
