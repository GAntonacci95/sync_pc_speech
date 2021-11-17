using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using COMSound_Lib.Audio;

namespace COMSound_Receive
{
    public partial class Receive : Form
    {
        public Receive()
        {
            InitializeComponent();
            //COMSound_Lib.Comandi.Exe.cd();
        }

        private void Receive_Load(object sender, EventArgs e)
        {
            pictureBoxFrequencyDomainLeft.Width = 326;
            pictureBoxFrequencyDomainLeft.Height = 94;
            pictureBoxFrequencyDomainRight.Width = 326;
            pictureBoxFrequencyDomainRight.Height = 94;

            txtReceived.Enabled = false;

            Microphone m = new Microphone(ref pictureBoxFrequencyDomainLeft, ref pictureBoxFrequencyDomainRight, ref txtReceived);
            m.Receive();
        }
    }
}
