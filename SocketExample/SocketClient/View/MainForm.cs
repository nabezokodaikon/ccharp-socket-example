using SocketClient.ConnectionIF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SocketClient.View
{
    public partial class MainForm : Form
    {
        private readonly ClientConnection connection = new ClientConnection();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.connection.Disconnect();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            this.connection.Connect();
        }

        private void disconnectButton_Click(object sender, EventArgs e)
        {
            this.connection.Disconnect();
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            var contents = this.contentTextBox.Text;
            if (contents == null)
            {
                return;
            }

            this.connection.Send(contents);
        }
    }
}
