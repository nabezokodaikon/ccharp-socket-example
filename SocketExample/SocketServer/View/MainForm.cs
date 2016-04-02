using SocketServer.ConnectionIF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SocketServer.View
{
    public partial class MainForm : Form
    {
        private readonly ServerConnection connection = new ServerConnection();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.connection.Close();
        }

        private void openButton_Click(object sender, EventArgs e)
        {
            this.connection.Open();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.connection.Close();
        }
    }
}
