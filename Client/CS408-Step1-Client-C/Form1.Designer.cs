namespace CS408_Step1_Client_C
{
    partial class frmConnect
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConnect));
            this.grpConnect = new System.Windows.Forms.GroupBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.txtUsername = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.grpEvent = new System.Windows.Forms.GroupBox();
            this.rtbEvent = new System.Windows.Forms.RichTextBox();
            this.grpUserList = new System.Windows.Forms.GroupBox();
            this.lstUserList = new System.Windows.Forms.ListBox();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnUserList = new System.Windows.Forms.Button();
            this.grpConnect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.grpEvent.SuspendLayout();
            this.grpUserList.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpConnect
            // 
            this.grpConnect.Controls.Add(this.txtPassword);
            this.grpConnect.Controls.Add(this.lblPassword);
            this.grpConnect.Controls.Add(this.txtUsername);
            this.grpConnect.Controls.Add(this.txtIP);
            this.grpConnect.Controls.Add(this.numPort);
            this.grpConnect.Controls.Add(this.label3);
            this.grpConnect.Controls.Add(this.label2);
            this.grpConnect.Controls.Add(this.label1);
            this.grpConnect.Location = new System.Drawing.Point(9, 10);
            this.grpConnect.Margin = new System.Windows.Forms.Padding(2);
            this.grpConnect.Name = "grpConnect";
            this.grpConnect.Padding = new System.Windows.Forms.Padding(2);
            this.grpConnect.Size = new System.Drawing.Size(266, 147);
            this.grpConnect.TabIndex = 0;
            this.grpConnect.TabStop = false;
            this.grpConnect.Text = "Connection";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new System.Drawing.Point(64, 45);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '#';
            this.txtPassword.Size = new System.Drawing.Size(150, 20);
            this.txtPassword.TabIndex = 7;
            this.txtPassword.Text = "pass1";
            this.txtPassword.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(4, 48);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(53, 13);
            this.lblPassword.TabIndex = 6;
            this.lblPassword.Text = "Password";
            this.lblPassword.Click += new System.EventHandler(this.label4_Click);
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new System.Drawing.Point(64, 21);
            this.txtUsername.Margin = new System.Windows.Forms.Padding(2);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new System.Drawing.Size(150, 20);
            this.txtUsername.TabIndex = 5;
            this.txtUsername.Text = "c1";
            this.txtUsername.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtIP
            // 
            this.txtIP.HideSelection = false;
            this.txtIP.Location = new System.Drawing.Point(64, 68);
            this.txtIP.Margin = new System.Windows.Forms.Padding(2);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(150, 20);
            this.txtIP.TabIndex = 4;
            this.txtIP.Text = "159.20.87.161";
            this.txtIP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(64, 91);
            this.numPort.Margin = new System.Windows.Forms.Padding(2);
            this.numPort.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(149, 20);
            this.numPort.TabIndex = 3;
            this.numPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numPort.Value = new decimal(new int[] {
            8888,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 93);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 71);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(17, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "IP";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Username";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(9, 161);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(2);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(266, 43);
            this.btnConnect.TabIndex = 1;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // grpEvent
            // 
            this.grpEvent.Controls.Add(this.rtbEvent);
            this.grpEvent.Location = new System.Drawing.Point(9, 222);
            this.grpEvent.Margin = new System.Windows.Forms.Padding(2);
            this.grpEvent.Name = "grpEvent";
            this.grpEvent.Padding = new System.Windows.Forms.Padding(2);
            this.grpEvent.Size = new System.Drawing.Size(336, 380);
            this.grpEvent.TabIndex = 2;
            this.grpEvent.TabStop = false;
            this.grpEvent.Text = "Event Log";
            // 
            // rtbEvent
            // 
            this.rtbEvent.Location = new System.Drawing.Point(9, 19);
            this.rtbEvent.Margin = new System.Windows.Forms.Padding(2);
            this.rtbEvent.Name = "rtbEvent";
            this.rtbEvent.Size = new System.Drawing.Size(323, 355);
            this.rtbEvent.TabIndex = 0;
            this.rtbEvent.Text = "";
            // 
            // grpUserList
            // 
            this.grpUserList.Controls.Add(this.lstUserList);
            this.grpUserList.Location = new System.Drawing.Point(375, 16);
            this.grpUserList.Margin = new System.Windows.Forms.Padding(2);
            this.grpUserList.Name = "grpUserList";
            this.grpUserList.Padding = new System.Windows.Forms.Padding(2);
            this.grpUserList.Size = new System.Drawing.Size(266, 370);
            this.grpUserList.TabIndex = 3;
            this.grpUserList.TabStop = false;
            this.grpUserList.Text = "User List";
            this.grpUserList.Visible = false;
            this.grpUserList.Enter += new System.EventHandler(this.grpUserList_Enter);
            // 
            // lstUserList
            // 
            this.lstUserList.FormattingEnabled = true;
            this.lstUserList.Location = new System.Drawing.Point(8, 18);
            this.lstUserList.Margin = new System.Windows.Forms.Padding(2);
            this.lstUserList.Name = "lstUserList";
            this.lstUserList.Size = new System.Drawing.Size(254, 342);
            this.lstUserList.TabIndex = 0;
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(9, 162);
            this.btnDisconnect.Margin = new System.Windows.Forms.Padding(2);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(266, 42);
            this.btnDisconnect.TabIndex = 3;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Visible = false;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnUserList
            // 
            this.btnUserList.Location = new System.Drawing.Point(553, 461);
            this.btnUserList.Margin = new System.Windows.Forms.Padding(2);
            this.btnUserList.Name = "btnUserList";
            this.btnUserList.Size = new System.Drawing.Size(135, 42);
            this.btnUserList.TabIndex = 4;
            this.btnUserList.Text = "User List";
            this.btnUserList.UseVisualStyleBackColor = true;
            this.btnUserList.Visible = false;
            this.btnUserList.Click += new System.EventHandler(this.btnUserList_Click);
            // 
            // frmConnect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(357, 607);
            this.Controls.Add(this.grpUserList);
            this.Controls.Add(this.btnUserList);
            this.Controls.Add(this.btnDisconnect);
            this.Controls.Add(this.grpEvent);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.grpConnect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "frmConnect";
            this.Text = "Connect";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmConnect_FormClosing);
            this.grpConnect.ResumeLayout(false);
            this.grpConnect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.grpEvent.ResumeLayout(false);
            this.grpUserList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpConnect;
        private System.Windows.Forms.TextBox txtUsername;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.GroupBox grpEvent;
        private System.Windows.Forms.RichTextBox rtbEvent;
        private System.Windows.Forms.GroupBox grpUserList;
        private System.Windows.Forms.ListBox lstUserList;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Button btnUserList;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
    }
}

