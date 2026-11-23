namespace TreasuryToolkit.App
{
    partial class ExceptionHandlerForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExceptionHandlerForm));
            label1 = new Label();
            label2 = new Label();
            TxtExceptionDetails = new TextBox();
            pictureBox1 = new PictureBox();
            BtnCopyDtls = new Button();
            BtnClose = new Button();
            label3 = new Label();
            ToolTipTimer = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.BackColor = SystemColors.ControlLightLight;
            label1.Font = new Font("Segoe UI", 12F);
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(652, 51);
            label1.TabIndex = 0;
            label1.Text = "La aplicación ha detectado un error no controlado, pero no se preocupe: su información está a salvo y no es necesario reiniciar la herramienta.";
            // 
            // label2
            // 
            label2.Font = new Font("Segoe UI", 12F);
            label2.Location = new Point(12, 69);
            label2.Name = "label2";
            label2.Size = new Size(652, 74);
            label2.TabIndex = 1;
            label2.Text = "Para ayudar a solucionar este problema lo antes posible, por favor copie el detalle técnico que aparece a continuación y envíelo por correo electrónico a: gerardo.aguilar01@outlook.com";
            // 
            // TxtExceptionDetails
            // 
            TxtExceptionDetails.Font = new Font("Segoe UI", 12F);
            TxtExceptionDetails.Location = new Point(12, 203);
            TxtExceptionDetails.Multiline = true;
            TxtExceptionDetails.Name = "TxtExceptionDetails";
            TxtExceptionDetails.ReadOnly = true;
            TxtExceptionDetails.ScrollBars = ScrollBars.Vertical;
            TxtExceptionDetails.Size = new Size(853, 194);
            TxtExceptionDetails.TabIndex = 2;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.Error;
            pictureBox1.Location = new Point(681, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(159, 146);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // BtnCopyDtls
            // 
            BtnCopyDtls.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnCopyDtls.FlatStyle = FlatStyle.Flat;
            BtnCopyDtls.Font = new Font("Segoe UI", 12F);
            BtnCopyDtls.Location = new Point(602, 420);
            BtnCopyDtls.Margin = new Padding(4);
            BtnCopyDtls.Name = "BtnCopyDtls";
            BtnCopyDtls.Size = new Size(131, 34);
            BtnCopyDtls.TabIndex = 4;
            BtnCopyDtls.Text = "Copiar Detalles";
            BtnCopyDtls.UseVisualStyleBackColor = true;
            BtnCopyDtls.Click += BtnCopyDtls_Click;
            // 
            // BtnClose
            // 
            BtnClose.FlatAppearance.BorderColor = Color.DodgerBlue;
            BtnClose.FlatStyle = FlatStyle.Flat;
            BtnClose.Font = new Font("Segoe UI", 12F);
            BtnClose.Location = new Point(765, 420);
            BtnClose.Margin = new Padding(4);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(90, 34);
            BtnClose.TabIndex = 5;
            BtnClose.Text = "Cerrar";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // label3
            // 
            label3.Font = new Font("Segoe UI", 12F);
            label3.Location = new Point(12, 147);
            label3.Name = "label3";
            label3.Size = new Size(652, 49);
            label3.TabIndex = 6;
            label3.Text = "Sugerencia: Incluir una breve descripción de las acciones o pasos que realizaba justo antes de que apareciera esta ventana facilitará enormemente la corrección.";
            // 
            // ToolTipTimer
            // 
            ToolTipTimer.Interval = 2000;
            ToolTipTimer.Tick += ToolTipTimer_Tick;
            // 
            // ExceptionHandlerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ControlLightLight;
            CancelButton = BtnClose;
            ClientSize = new Size(877, 469);
            Controls.Add(label3);
            Controls.Add(BtnClose);
            Controls.Add(BtnCopyDtls);
            Controls.Add(pictureBox1);
            Controls.Add(TxtExceptionDetails);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "ExceptionHandlerForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tesoreria - Ocurrio un problema";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private TextBox TxtExceptionDetails;
        private PictureBox pictureBox1;
        private Button BtnCopyDtls;
        private Button BtnClose;
        private Label label3;
        private System.Windows.Forms.Timer ToolTipTimer;
    }
}