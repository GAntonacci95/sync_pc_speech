namespace COMSound_Receive
{
    partial class Receive
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
            this.pictureBoxFrequencyDomainLeft = new System.Windows.Forms.PictureBox();
            this.pictureBoxFrequencyDomainRight = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtReceived = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFrequencyDomainLeft)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFrequencyDomainRight)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxFrequencyDomainLeft
            // 
            this.pictureBoxFrequencyDomainLeft.BackColor = System.Drawing.Color.Black;
            this.pictureBoxFrequencyDomainLeft.Location = new System.Drawing.Point(15, 25);
            this.pictureBoxFrequencyDomainLeft.Name = "pictureBoxFrequencyDomainLeft";
            this.pictureBoxFrequencyDomainLeft.Size = new System.Drawing.Size(326, 94);
            this.pictureBoxFrequencyDomainLeft.TabIndex = 11;
            this.pictureBoxFrequencyDomainLeft.TabStop = false;
            // 
            // pictureBoxFrequencyDomainRight
            // 
            this.pictureBoxFrequencyDomainRight.BackColor = System.Drawing.Color.Black;
            this.pictureBoxFrequencyDomainRight.Location = new System.Drawing.Point(383, 25);
            this.pictureBoxFrequencyDomainRight.Name = "pictureBoxFrequencyDomainRight";
            this.pictureBoxFrequencyDomainRight.Size = new System.Drawing.Size(326, 94);
            this.pictureBoxFrequencyDomainRight.TabIndex = 10;
            this.pictureBoxFrequencyDomainRight.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(169, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Dominio delle frequenze di sinistra:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(543, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(166, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Dominio delle frequenze di destra:";
            // 
            // txtReceived
            // 
            this.txtReceived.Location = new System.Drawing.Point(15, 125);
            this.txtReceived.Name = "txtReceived";
            this.txtReceived.Size = new System.Drawing.Size(694, 460);
            this.txtReceived.TabIndex = 14;
            this.txtReceived.Text = "";
            // 
            // Receive
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(721, 597);
            this.Controls.Add(this.txtReceived);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBoxFrequencyDomainLeft);
            this.Controls.Add(this.pictureBoxFrequencyDomainRight);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "Receive";
            this.Text = "COMSound";
            this.Load += new System.EventHandler(this.Receive_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFrequencyDomainLeft)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxFrequencyDomainRight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxFrequencyDomainLeft;
        private System.Windows.Forms.PictureBox pictureBoxFrequencyDomainRight;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox txtReceived;

    }
}

