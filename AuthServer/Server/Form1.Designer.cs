namespace Server
{
    partial class ServerForm
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
            this.rtbEventLog = new System.Windows.Forms.RichTextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.numPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.lblIPLabel = new System.Windows.Forms.Label();
            this.lblIP = new System.Windows.Forms.Label();
            this.lblEventLog = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rtbAuthenticatedUsers = new System.Windows.Forms.RichTextBox();
            this.lblAuthenticated = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).BeginInit();
            this.SuspendLayout();
            // 
            // rtbEventLog
            // 
            this.rtbEventLog.Location = new System.Drawing.Point(12, 133);
            this.rtbEventLog.Name = "rtbEventLog";
            this.rtbEventLog.Size = new System.Drawing.Size(366, 383);
            this.rtbEventLog.TabIndex = 0;
            this.rtbEventLog.Text = "";
            this.rtbEventLog.TextChanged += new System.EventHandler(this.rtbEventLog_TextChanged);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(236, 39);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(142, 31);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "START SERVER";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // numPort
            // 
            this.numPort.Location = new System.Drawing.Point(47, 84);
            this.numPort.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numPort.Name = "numPort";
            this.numPort.Size = new System.Drawing.Size(101, 20);
            this.numPort.TabIndex = 2;
            this.numPort.Value = new decimal(new int[] {
            8888,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Port:";
            // 
            // lblIPLabel
            // 
            this.lblIPLabel.AutoSize = true;
            this.lblIPLabel.Location = new System.Drawing.Point(11, 45);
            this.lblIPLabel.Name = "lblIPLabel";
            this.lblIPLabel.Size = new System.Drawing.Size(54, 13);
            this.lblIPLabel.TabIndex = 4;
            this.lblIPLabel.Text = "Server IP:";
            this.lblIPLabel.Click += new System.EventHandler(this.lblIPLabel_Click);
            // 
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(71, 45);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(0, 13);
            this.lblIP.TabIndex = 5;
            // 
            // lblEventLog
            // 
            this.lblEventLog.AutoSize = true;
            this.lblEventLog.Location = new System.Drawing.Point(322, 117);
            this.lblEventLog.Name = "lblEventLog";
            this.lblEventLog.Size = new System.Drawing.Size(56, 13);
            this.lblEventLog.TabIndex = 6;
            this.lblEventLog.Text = "Event Log";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 23);
            this.label2.TabIndex = 0;
            // 
            // rtbAuthenticatedUsers
            // 
            this.rtbAuthenticatedUsers.Location = new System.Drawing.Point(476, 133);
            this.rtbAuthenticatedUsers.Name = "rtbAuthenticatedUsers";
            this.rtbAuthenticatedUsers.Size = new System.Drawing.Size(125, 383);
            this.rtbAuthenticatedUsers.TabIndex = 7;
            this.rtbAuthenticatedUsers.Text = "";
            // 
            // lblAuthenticated
            // 
            this.lblAuthenticated.AutoSize = true;
            this.lblAuthenticated.Location = new System.Drawing.Point(498, 117);
            this.lblAuthenticated.Name = "lblAuthenticated";
            this.lblAuthenticated.Size = new System.Drawing.Size(103, 13);
            this.lblAuthenticated.TabIndex = 8;
            this.lblAuthenticated.Text = "Authenticated Users";
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 528);
            this.Controls.Add(this.lblAuthenticated);
            this.Controls.Add(this.rtbAuthenticatedUsers);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblEventLog);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.lblIPLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numPort);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.rtbEventLog);
            this.Name = "ServerForm";
            this.Text = "Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ServerForm_FormClosing);
            this.Load += new System.EventHandler(this.ServerForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbEventLog;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblIPLabel;
        private System.Windows.Forms.Label lblIP;
        private System.Windows.Forms.Label lblEventLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox rtbAuthenticatedUsers;
        private System.Windows.Forms.Label lblAuthenticated;
    }
}

