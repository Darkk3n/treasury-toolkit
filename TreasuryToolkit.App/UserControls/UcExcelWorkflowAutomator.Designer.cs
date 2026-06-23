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
            Column1 = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            Column3 = new DataGridViewTextBoxColumn();
            Column4 = new DataGridViewTextBoxColumn();
            Column5 = new DataGridViewTextBoxColumn();
            Column6 = new DataGridViewTextBoxColumn();
            Column7 = new DataGridViewTextBoxColumn();
            Column8 = new DataGridViewTextBoxColumn();
            Column9 = new DataGridViewTextBoxColumn();
            Column10 = new DataGridViewTextBoxColumn();
            Column11 = new DataGridViewTextBoxColumn();
            Column12 = new DataGridViewTextBoxColumn();
            LblStatus = new Label();
            BtnProcess = new Button();
            BtnStart = new Button();
            LblFolder = new Label();
            ((System.ComponentModel.ISupportInitialize)DgvResults).BeginInit();
            SuspendLayout();
            // 
            // DgvResults
            // 
            DgvResults.AllowUserToAddRows = false;
            DgvResults.AllowUserToDeleteRows = false;
            DgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DgvResults.BackgroundColor = Color.White;
            DgvResults.BorderStyle = BorderStyle.None;
            DgvResults.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DgvResults.Columns.AddRange(new DataGridViewColumn[] { Column1, Column2, Column3, Column4, Column5, Column6, Column7, Column8, Column9, Column10, Column11, Column12 });
            DgvResults.Dock = DockStyle.Bottom;
            DgvResults.EnableHeadersVisualStyles = false;
            DgvResults.Location = new Point(0, 127);
            DgvResults.Name = "DgvResults";
            DgvResults.Size = new Size(1323, 516);
            DgvResults.TabIndex = 0;
            // 
            // Column1
            // 
            Column1.HeaderText = "Tarjetas: # Operaciones";
            Column1.Name = "Column1";
            // 
            // Column2
            // 
            Column2.HeaderText = "Tarjetas: $ Operaciones";
            Column2.Name = "Column2";
            // 
            // Column3
            // 
            Column3.HeaderText = "Cheques: # Operaciones";
            Column3.Name = "Column3";
            // 
            // Column4
            // 
            Column4.HeaderText = "Cheques: $ Operaciones";
            Column4.Name = "Column4";
            // 
            // Column5
            // 
            Column5.HeaderText = "Transferencias Interbancarias: # Operaciones";
            Column5.Name = "Column5";
            // 
            // Column6
            // 
            Column6.HeaderText = "Transferencias Interbancarias: $ Operaciones";
            Column6.Name = "Column6";
            // 
            // Column7
            // 
            Column7.HeaderText = "Transferencia Mismo Banco: # Opereaciones";
            Column7.Name = "Column7";
            // 
            // Column8
            // 
            Column8.HeaderText = "Transferencia Mismo Banco: $ Operaciones";
            Column8.Name = "Column8";
            // 
            // Column9
            // 
            Column9.HeaderText = "Transferencia Internacional: # Operaciones";
            Column9.Name = "Column9";
            // 
            // Column10
            // 
            Column10.HeaderText = "Transferencia Internacional: $ Operaciones";
            Column10.Name = "Column10";
            // 
            // Column11
            // 
            Column11.HeaderText = "Total de Operaciones";
            Column11.Name = "Column11";
            // 
            // Column12
            // 
            Column12.HeaderText = "Total $ Operaciones";
            Column12.Name = "Column12";
            // 
            // LblStatus
            // 
            LblStatus.AutoSize = true;
            LblStatus.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LblStatus.Location = new Point(15, 16);
            LblStatus.Name = "LblStatus";
            LblStatus.Size = new Size(176, 21);
            LblStatus.TabIndex = 1;
            LblStatus.Text = "Ubicacion de Archivo(s):";
            // 
            // BtnProcess
            // 
            BtnProcess.BackColor = SystemColors.ControlLightLight;
            BtnProcess.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnProcess.FlatStyle = FlatStyle.Flat;
            BtnProcess.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BtnProcess.Location = new Point(197, 10);
            BtnProcess.Name = "BtnProcess";
            BtnProcess.Size = new Size(37, 36);
            BtnProcess.TabIndex = 2;
            BtnProcess.Text = "...";
            BtnProcess.UseVisualStyleBackColor = false;
            // 
            // BtnStart
            // 
            BtnStart.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnStart.FlatStyle = FlatStyle.Flat;
            BtnStart.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BtnStart.Location = new Point(197, 52);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(84, 36);
            BtnStart.TabIndex = 3;
            BtnStart.Text = "Generar";
            BtnStart.UseVisualStyleBackColor = true;
            // 
            // LblFolder
            // 
            LblFolder.AutoSize = true;
            LblFolder.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            LblFolder.Location = new Point(240, 16);
            LblFolder.Name = "LblFolder";
            LblFolder.Size = new Size(19, 21);
            LblFolder.TabIndex = 4;
            LblFolder.Text = "...";
            // 
            // UcExcelWorkflowAutomator
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            Controls.Add(LblFolder);
            Controls.Add(BtnStart);
            Controls.Add(BtnProcess);
            Controls.Add(LblStatus);
            Controls.Add(DgvResults);
            Name = "UcExcelWorkflowAutomator";
            Size = new Size(1323, 643);
            ((System.ComponentModel.ISupportInitialize)DgvResults).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView DgvResults;
        private Label LblStatus;
        private Button BtnProcess;
        private Button BtnStart;
        private Label LblFolder;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn Column2;
        private DataGridViewTextBoxColumn Column3;
        private DataGridViewTextBoxColumn Column4;
        private DataGridViewTextBoxColumn Column5;
        private DataGridViewTextBoxColumn Column6;
        private DataGridViewTextBoxColumn Column7;
        private DataGridViewTextBoxColumn Column8;
        private DataGridViewTextBoxColumn Column9;
        private DataGridViewTextBoxColumn Column10;
        private DataGridViewTextBoxColumn Column11;
        private DataGridViewTextBoxColumn Column12;
    }
}
