using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GoogleAppEngine_GUI
{
    public partial class dlgProps : Form
    {
        private ListViewItem appItem;
        public dlgProps(ListViewItem appItem)
        {
            InitializeComponent();
            this.appItem = appItem;
        }

        private void dlgProps_Load(object sender, EventArgs e)
        {
            txtName.Text = appItem.SubItems[0].Text;
            txtPath.Text = appItem.SubItems[1].Text;
            txtPort.Text = appItem.SubItems[2].Text;
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog open = new FolderBrowserDialog();
            open.SelectedPath = txtPath.Text;
            open.Description = "Choose the application folder.";
            if (open.ShowDialog() == DialogResult.OK)
            {
                txtPath.Text = open.SelectedPath;
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            appItem.SubItems[0].Text = txtName.Text;
            appItem.SubItems[1].Text = txtPath.Text;
            appItem.SubItems[2].Text = txtPort.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}