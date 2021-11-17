using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using COMSound_Lib.Audio;

namespace COMSound_Send
{
    public partial class Send : Form
    {
        public Send()
        {
            InitializeComponent();
        }

        private void Send_Load(object sender, EventArgs e)
        {
            txtGeneral.Enabled = false;
            txtCmd.Focus();
            this.AcceptButton = btnSend;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            Speaker s = new Speaker();
            
            txtGeneral.Text+=txtCmd.Text+"\n";
            s.Transmit(txtCmd.Text);

            txtCmd.Clear();
        }
    }
}
