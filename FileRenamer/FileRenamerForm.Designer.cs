namespace FileRenamer
{
    partial class FileRenamerForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnClean = new Button();
            DgvPayments = new DataGridView();
            dgvPaymentsColDate = new DataGridViewTextBoxColumn();
            Column1 = new DataGridViewTextBoxColumn();
            dgvPaymentsColVendor = new DataGridViewTextBoxColumn();
            dgvPaymentsColConcept = new DataGridViewTextBoxColumn();
            dgvPaymentsColAmount = new DataGridViewTextBoxColumn();
            Column2 = new DataGridViewTextBoxColumn();
            label1 = new Label();
            CmbCompany = new ComboBox();
            BtnStart = new Button();
            label2 = new Label();
            LblFolder = new Label();
            BtnFileDialog = new Button();
            LblConsecutive = new Label();
            TxtConsecutive = new TextBox();
            ((System.ComponentModel.ISupportInitialize)DgvPayments).BeginInit();
            SuspendLayout();
            // 
            // BtnClean
            // 
            BtnClean.Location = new Point(196, 11);
            BtnClean.Name = "BtnClean";
            BtnClean.Size = new Size(75, 23);
            BtnClean.TabIndex = 0;
            BtnClean.Text = "Limpiar";
            BtnClean.UseVisualStyleBackColor = true;
            BtnClean.Click += BtnClean_Click;
            // 
            // DgvPayments
            // 
            DgvPayments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DgvPayments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DgvPayments.Columns.AddRange(new DataGridViewColumn[] { dgvPaymentsColDate, Column1, dgvPaymentsColVendor, dgvPaymentsColConcept, dgvPaymentsColAmount, Column2 });
            DgvPayments.Dock = DockStyle.Bottom;
            DgvPayments.Location = new Point(0, 98);
            DgvPayments.Name = "DgvPayments";
            DgvPayments.ScrollBars = ScrollBars.Vertical;
            DgvPayments.Size = new Size(1017, 361);
            DgvPayments.TabIndex = 1;
            DgvPayments.CellValidated += DgvPayments_CellValidated;
            DgvPayments.CellValidating += DgvPayments_CellValidating;
            DgvPayments.DefaultValuesNeeded += DgvPayments_DefaultValuesNeeded;
            // 
            // dgvPaymentsColDate
            // 
            dgvPaymentsColDate.HeaderText = "Fecha";
            dgvPaymentsColDate.Name = "dgvPaymentsColDate";
            // 
            // Column1
            // 
            Column1.HeaderText = "Archivo";
            Column1.Name = "Column1";
            // 
            // dgvPaymentsColVendor
            // 
            dgvPaymentsColVendor.HeaderText = "Proveedor";
            dgvPaymentsColVendor.Name = "dgvPaymentsColVendor";
            // 
            // dgvPaymentsColConcept
            // 
            dgvPaymentsColConcept.HeaderText = "Concepto";
            dgvPaymentsColConcept.Name = "dgvPaymentsColConcept";
            // 
            // dgvPaymentsColAmount
            // 
            dgvPaymentsColAmount.HeaderText = "Monto";
            dgvPaymentsColAmount.Name = "dgvPaymentsColAmount";
            // 
            // Column2
            // 
            Column2.HeaderText = "Divisa";
            Column2.Name = "Column2";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(7, 15);
            label1.Name = "label1";
            label1.Size = new Size(55, 15);
            label1.TabIndex = 2;
            label1.Text = "Empresa:";
            // 
            // CmbCompany
            // 
            CmbCompany.DropDownStyle = ComboBoxStyle.DropDownList;
            CmbCompany.FormattingEnabled = true;
            CmbCompany.Items.AddRange(new object[] { "--SELECCIONE--", "WOREGG", "BECHEM", "UHLMANN", "NETSTAL", "EMKA" });
            CmbCompany.Location = new Point(59, 12);
            CmbCompany.Name = "CmbCompany";
            CmbCompany.Size = new Size(131, 23);
            CmbCompany.TabIndex = 3;
            CmbCompany.SelectedIndexChanged += CmbCompany_SelectedIndexChanged;
            // 
            // BtnStart
            // 
            BtnStart.Location = new Point(277, 12);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(75, 23);
            BtnStart.TabIndex = 4;
            BtnStart.Text = "Renombrar";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 48);
            label2.Name = "label2";
            label2.Size = new Size(128, 15);
            label2.TabIndex = 5;
            label2.Text = "Ubicacion de Archivos:";
            // 
            // LblFolder
            // 
            LblFolder.AutoSize = true;
            LblFolder.Location = new Point(181, 48);
            LblFolder.Name = "LblFolder";
            LblFolder.Size = new Size(16, 15);
            LblFolder.TabIndex = 6;
            LblFolder.Text = "...";
            // 
            // BtnFileDialog
            // 
            BtnFileDialog.Location = new Point(146, 42);
            BtnFileDialog.Name = "BtnFileDialog";
            BtnFileDialog.Size = new Size(29, 26);
            BtnFileDialog.TabIndex = 7;
            BtnFileDialog.Text = "...";
            BtnFileDialog.UseVisualStyleBackColor = true;
            BtnFileDialog.Click += BtnFileDialog_Click;
            // 
            // LblConsecutive
            // 
            LblConsecutive.AutoSize = true;
            LblConsecutive.Location = new Point(362, 16);
            LblConsecutive.Name = "LblConsecutive";
            LblConsecutive.Size = new Size(76, 15);
            LblConsecutive.TabIndex = 8;
            LblConsecutive.Text = "Consecutivo:";
            LblConsecutive.Visible = false;
            // 
            // TxtConsecutive
            // 
            TxtConsecutive.Location = new Point(444, 11);
            TxtConsecutive.Name = "TxtConsecutive";
            TxtConsecutive.Size = new Size(119, 23);
            TxtConsecutive.TabIndex = 9;
            TxtConsecutive.Visible = false;
            TxtConsecutive.KeyPress += TxtConsecutive_KeyPress;
            // 
            // FileRenamerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1017, 459);
            Controls.Add(TxtConsecutive);
            Controls.Add(LblConsecutive);
            Controls.Add(BtnFileDialog);
            Controls.Add(LblFolder);
            Controls.Add(label2);
            Controls.Add(BtnStart);
            Controls.Add(CmbCompany);
            Controls.Add(label1);
            Controls.Add(DgvPayments);
            Controls.Add(BtnClean);
            Name = "FileRenamerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tesoreria - Renombrador de Archivos";
            Load += FileRenamerForm_Load;
            ((System.ComponentModel.ISupportInitialize)DgvPayments).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button BtnClean;
        private DataGridView DgvPayments;
        private Label label1;
        private ComboBox CmbCompany;
        private Button BtnStart;
        private Label label2;
        private Label LblFolder;
        private Button BtnFileDialog;
        private DataGridViewTextBoxColumn dgvPaymentsColDate;
        private DataGridViewTextBoxColumn Column1;
        private DataGridViewTextBoxColumn dgvPaymentsColVendor;
        private DataGridViewTextBoxColumn dgvPaymentsColConcept;
        private DataGridViewTextBoxColumn dgvPaymentsColAmount;
        private DataGridViewTextBoxColumn Column2;
        private Label LblConsecutive;
        private TextBox TxtConsecutive;
    }
}