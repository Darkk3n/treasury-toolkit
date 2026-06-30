namespace TreasuryToolkit.App
{
    partial class MainForm
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
            PnlSideBar = new Panel();
            ChkGlobalTheme = new CheckBox();
            BtnAbout = new Button();
            BtnExcelTool = new Button();
            BtnPdfTool = new Button();
            PnlMainContent = new Panel();
            PnlSideBar.SuspendLayout();
            SuspendLayout();
            // 
            // PnlSideBar
            // 
            PnlSideBar.BackColor = SystemColors.ControlDarkDark;
            PnlSideBar.Controls.Add(ChkGlobalTheme);
            PnlSideBar.Controls.Add(BtnAbout);
            PnlSideBar.Controls.Add(BtnExcelTool);
            PnlSideBar.Controls.Add(BtnPdfTool);
            PnlSideBar.Dock = DockStyle.Left;
            PnlSideBar.Location = new Point(0, 0);
            PnlSideBar.Name = "PnlSideBar";
            PnlSideBar.Size = new Size(236, 603);
            PnlSideBar.TabIndex = 0;
            // 
            // ChkGlobalTheme
            // 
            ChkGlobalTheme.AutoSize = true;
            ChkGlobalTheme.Dock = DockStyle.Bottom;
            ChkGlobalTheme.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            ChkGlobalTheme.Location = new Point(0, 578);
            ChkGlobalTheme.Name = "ChkGlobalTheme";
            ChkGlobalTheme.Padding = new Padding(40, 0, 0, 0);
            ChkGlobalTheme.Size = new Size(236, 25);
            ChkGlobalTheme.TabIndex = 3;
            ChkGlobalTheme.Text = "Modo Oscuro";
            ChkGlobalTheme.UseVisualStyleBackColor = true;
            ChkGlobalTheme.CheckedChanged += ChkGlobalTheme_CheckedChanged;
            // 
            // BtnAbout
            // 
            BtnAbout.Dock = DockStyle.Top;
            BtnAbout.FlatAppearance.BorderSize = 0;
            BtnAbout.FlatStyle = FlatStyle.Flat;
            BtnAbout.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BtnAbout.Location = new Point(0, 64);
            BtnAbout.Name = "BtnAbout";
            BtnAbout.Size = new Size(236, 31);
            BtnAbout.TabIndex = 2;
            BtnAbout.Text = "Acerca De";
            BtnAbout.UseVisualStyleBackColor = true;
            // 
            // BtnExcelTool
            // 
            BtnExcelTool.Dock = DockStyle.Top;
            BtnExcelTool.FlatAppearance.BorderSize = 0;
            BtnExcelTool.FlatStyle = FlatStyle.Flat;
            BtnExcelTool.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BtnExcelTool.Location = new Point(0, 35);
            BtnExcelTool.Name = "BtnExcelTool";
            BtnExcelTool.Size = new Size(236, 29);
            BtnExcelTool.TabIndex = 1;
            BtnExcelTool.Text = "Excel";
            BtnExcelTool.UseVisualStyleBackColor = true;
            BtnExcelTool.Click += BtnExcelTool_Click;
            // 
            // BtnPdfTool
            // 
            BtnPdfTool.Dock = DockStyle.Top;
            BtnPdfTool.FlatAppearance.BorderSize = 0;
            BtnPdfTool.FlatStyle = FlatStyle.Flat;
            BtnPdfTool.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            BtnPdfTool.Location = new Point(0, 0);
            BtnPdfTool.Name = "BtnPdfTool";
            BtnPdfTool.Size = new Size(236, 35);
            BtnPdfTool.TabIndex = 0;
            BtnPdfTool.Text = "Renombrador de PDFs";
            BtnPdfTool.UseVisualStyleBackColor = true;
            BtnPdfTool.Click += BtnPdfTool_Click;
            // 
            // PnlMainContent
            // 
            PnlMainContent.Dock = DockStyle.Fill;
            PnlMainContent.Location = new Point(236, 0);
            PnlMainContent.Name = "PnlMainContent";
            PnlMainContent.Size = new Size(1013, 603);
            PnlMainContent.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1249, 603);
            Controls.Add(PnlMainContent);
            Controls.Add(PnlSideBar);
            ForeColor = Color.White;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tesoreria";
            PnlSideBar.ResumeLayout(false);
            PnlSideBar.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Panel PnlSideBar;
        private Button BtnExcelTool;
        private Button BtnPdfTool;
        private Button BtnAbout;
        private Panel PnlMainContent;
        private CheckBox ChkGlobalTheme;
    }
}