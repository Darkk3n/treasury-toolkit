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
            BtnClean.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnClean.FlatStyle = FlatStyle.Flat;
            BtnClean.Location = new Point(252, 15);
            BtnClean.Margin = new Padding(4);
            BtnClean.Name = "BtnClean";
            BtnClean.Size = new Size(96, 40);
            BtnClean.TabIndex = 0;
            BtnClean.Text = "Limpiar";
            BtnClean.UseVisualStyleBackColor = true;
            BtnClean.Click += BtnClean_Click;
            // 
            // DgvPayments
            // 
            DgvPayments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            DgvPayments.BackgroundColor = Color.White;
            DgvPayments.BorderStyle = BorderStyle.None;
            DgvPayments.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DgvPayments.Columns.AddRange(new DataGridViewColumn[] { dgvPaymentsColDate, Column1, dgvPaymentsColVendor, dgvPaymentsColConcept, dgvPaymentsColAmount, Column2 });
            DgvPayments.Dock = DockStyle.Bottom;
            DgvPayments.EnableHeadersVisualStyles = false;
            DgvPayments.Location = new Point(0, 138);
            DgvPayments.Margin = new Padding(4);
            DgvPayments.Name = "DgvPayments";
            DgvPayments.ScrollBars = ScrollBars.Vertical;
            DgvPayments.Size = new Size(1308, 505);
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
            label1.Location = new Point(9, 21);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(73, 21);
            label1.TabIndex = 2;
            label1.Text = "Empresa:";
            // 
            // CmbCompany
            // 
            CmbCompany.DropDownStyle = ComboBoxStyle.DropDownList;
            CmbCompany.FormattingEnabled = true;
            CmbCompany.Items.AddRange(new object[] { "--SELECCIONE--", "WOREGG", "BECHEM", "UHLMANN", "NETSTAL", "EMKA" });
            CmbCompany.Location = new Point(77, 17);
            CmbCompany.Margin = new Padding(4);
            CmbCompany.Name = "CmbCompany";
            CmbCompany.Size = new Size(167, 29);
            CmbCompany.TabIndex = 3;
            CmbCompany.SelectedIndexChanged += CmbCompany_SelectedIndexChanged;
            // 
            // BtnStart
            // 
            BtnStart.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnStart.FlatStyle = FlatStyle.Flat;
            BtnStart.Location = new Point(356, 15);
            BtnStart.Margin = new Padding(4);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(101, 40);
            BtnStart.TabIndex = 4;
            BtnStart.Text = "Renombrar";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = SystemColors.ControlLightLight;
            label2.Location = new Point(15, 67);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(166, 21);
            label2.TabIndex = 5;
            label2.Text = "Ubicacion de Archivos:";
            // 
            // LblFolder
            // 
            LblFolder.AutoSize = true;
            LblFolder.Location = new Point(233, 67);
            LblFolder.Margin = new Padding(4, 0, 4, 0);
            LblFolder.Name = "LblFolder";
            LblFolder.Size = new Size(19, 21);
            LblFolder.TabIndex = 6;
            LblFolder.Text = "...";
            // 
            // BtnFileDialog
            // 
            BtnFileDialog.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnFileDialog.FlatStyle = FlatStyle.Flat;
            BtnFileDialog.Location = new Point(188, 59);
            BtnFileDialog.Margin = new Padding(4);
            BtnFileDialog.Name = "BtnFileDialog";
            BtnFileDialog.Size = new Size(37, 36);
            BtnFileDialog.TabIndex = 7;
            BtnFileDialog.Text = "...";
            BtnFileDialog.UseVisualStyleBackColor = true;
            BtnFileDialog.Click += BtnFileDialog_Click;
            // 
            // LblConsecutive
            // 
            LblConsecutive.AutoSize = true;
            LblConsecutive.Location = new Point(465, 22);
            LblConsecutive.Margin = new Padding(4, 0, 4, 0);
            LblConsecutive.Name = "LblConsecutive";
            LblConsecutive.Size = new Size(98, 21);
            LblConsecutive.TabIndex = 8;
            LblConsecutive.Text = "Consecutivo:";
            LblConsecutive.Visible = false;
            // 
            // TxtConsecutive
            // 
            TxtConsecutive.BorderStyle = BorderStyle.FixedSingle;
            TxtConsecutive.Location = new Point(571, 18);
            TxtConsecutive.Margin = new Padding(4);
            TxtConsecutive.Name = "TxtConsecutive";
            TxtConsecutive.Size = new Size(152, 29);
            TxtConsecutive.TabIndex = 9;
            TxtConsecutive.Visible = false;
            TxtConsecutive.KeyPress += TxtConsecutive_KeyPress;
            // 
            // FileRenamerForm
            // 
            AutoScaleDimensions = new SizeF(9F, 21F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            ClientSize = new Size(1308, 643);
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
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4);
            MaximizeBox = false;
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