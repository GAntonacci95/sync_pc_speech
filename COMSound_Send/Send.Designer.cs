namespace COMSound_Send
{
    partial class Send
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
            this.txtGeneral = new System.Windows.Forms.RichTextBox();
            this.txtCmd = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtGeneral
            // 
            this.txtGeneral.Location = new System.Drawing.Point(12, 12);
            this.txtGeneral.Name = "txtGeneral";
            this.txtGeneral.Size = new System.Drawing.Size(668, 533);
            this.txtGeneral.TabIndex = 0;
            this.txtGeneral.Text = "";
            // 
            // txtCmd
            // 
            this.txtCmd.Location = new System.Drawing.Point(12, 551);
            this.txtCmd.Multiline = true;
            this.txtCmd.Name = "txtCmd";
            this.txtCmd.Size = new System.Drawing.Size(587, 23);
            this.txtCmd.TabIndex = 1;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(605, 551);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 2;
            this.btnSend.Text = "INVIA";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // Send
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(692, 586);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtCmd);
            this.Controls.Add(this.txtGeneral);
            this.Name = "Send";
            this.Text = "COMSound";
            this.Load += new System.EventHandler(this.Send_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtGeneral;
        private System.Windows.Forms.TextBox txtCmd;
        private System.Windows.Forms.Button btnSend;
    }
}

