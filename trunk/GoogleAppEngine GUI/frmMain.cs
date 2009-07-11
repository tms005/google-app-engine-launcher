using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;               // For prcesss related information
using System.Runtime.InteropServices;   // For DLL importing 
 
namespace GoogleAppEngine_GUI
{
    public partial class frmMain : Form
    {
        delegate void ServerClosedDelegate(int i);

        System.Diagnostics.Process[] procs = new System.Diagnostics.Process[100];

        [DllImport("User32")] 
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        public frmMain()
        {
            InitializeComponent();
        }
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private void btnRun_Click(object sender, EventArgs e)
        {
            if (procs[listApps.SelectedIndices[0]] == null)
            {
                string path1 = listApps.SelectedItems[0].SubItems[1].Text;
                string name1 = listApps.SelectedItems[0].SubItems[0].Text;
                string port1 = listApps.SelectedItems[0].SubItems[2].Text;
                StreamWriter sw = new StreamWriter("lastServer.bat");
                //sw.WriteLine("@echo off");
                sw.WriteLine("title Running Google Apps Engine: " + name1);
                sw.WriteLine(path1.Substring(0, 2));
                string[] s = path1.Split('\\');

                sw.WriteLine("cd " + path1.Substring(0, path1.Length - s[s.Length - 1].Length));
                sw.WriteLine("dev_appserver.py " + "--p=" + port1 + " " + name1);
                sw.Close();
                System.Diagnostics.Process server = new System.Diagnostics.Process();
                server.StartInfo.FileName = "lastServer.bat";
                server.EnableRaisingEvents = true;
                server.Exited += new EventHandler(server_Exited);
                procs[listApps.SelectedIndices[0]] = server;
                server.Start();
                
                btnRun.Enabled = false;
                btnStop.Enabled = true;
                btnHide.Enabled = true;
                listApps.SelectedItems[0].SubItems[3].Text = "Running - Visible Window";
            }
        }
        
        private void btnStop_Click(object sender, EventArgs e)
        {
            if (procs[listApps.SelectedIndices[0]] != null)
            {
                procs[listApps.SelectedIndices[0]].CloseMainWindow();
                procs[listApps.SelectedIndices[0]].Close();
                procs[listApps.SelectedIndices[0]] = null;
                btnRun.Enabled = true;
                btnStop.Enabled = false;
                btnHide.Enabled = false;
                listApps.SelectedItems[0].SubItems[3].Text = "Stopped";
            }
        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            string status = listApps.SelectedItems[0].SubItems[3].Text;
            if (status == "Running - Visible Window")
            {
                ShowWindow((int)procs[listApps.SelectedIndices[0]].MainWindowHandle, SW_HIDE);
                listApps.SelectedItems[0].SubItems[3].Text = "Running - Hidden Window";
                btnHide.Text = "Show Server Window";
            }
            else if (status == "Running - Hidden Window")
            {
                ShowWindow((int)procs[listApps.SelectedIndices[0]].MainWindowHandle, SW_SHOW);
                listApps.SelectedItems[0].SubItems[3].Text = "Running - Visible Window";
                btnHide.Text = "Hide Server Window";
            }
        }

        private void server_Exited(object sender, EventArgs e)
        {
            Process ended = (Process)sender;
            for (int i = 0; i < procs.Length;i++ )
            {
                if (procs[i] != null && procs[i].Id == ended.Id)
                {
                    this.Invoke(new ServerClosedDelegate(server_closed),
                        new object[] { i });
                    break;
                }
            }
        }
        private void server_closed(int i){
            listApps.Items[i].SubItems[3].Text = "Stopped";
            if (listApps.SelectedIndices[0] == i)
            {
                procs[listApps.SelectedIndices[0]] = null;
                btnRun.Enabled = true;
                btnStop.Enabled = false;
                btnHide.Enabled = false;
                listApps.SelectedItems[0].SubItems[3].Text = "Stopped";
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /*ListViewItem item = new ListViewItem("sst");
            item.SubItems.Add("G:\\Pieces\\Python\\Google App Engine\\app.yaml");
            item.SubItems.Add("8080");
            item.SubItems.Add("Stopped");
            listApps.Items.Add(item);*/
            StreamReader sr = new StreamReader("servers.ini");
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                ListViewItem item = new ListViewItem(line.Substring(1,line.Length-2));
                line = sr.ReadLine();
                item.SubItems.Add(line.Substring(5));
                line = sr.ReadLine();
                item.SubItems.Add(line.Substring(5));
                item.SubItems.Add("Stopped");
                listApps.Items.Add(item);
            }
            sr.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ListViewItem item = new ListViewItem("");
            item.SubItems.Add("");
            item.SubItems.Add((8080 + listApps.Items.Count * 10).ToString());
            item.SubItems.Add("Stopped");
            

            Form dlg = new dlgProps(item);
            DialogResult dr = dlg.ShowDialog();
            if (dr == DialogResult.OK)
            {
                listApps.Items.Add(item);
                updateFile();
            }

        }

        private void listApps_Click(object sender, EventArgs e)
        {
            int s = listApps.SelectedItems[0].Index;
            if (procs[listApps.SelectedIndices[0]] == null)
            {
                btnRun.Enabled = true;
                btnStop.Enabled = false;
                btnHide.Enabled = false;
            }
            else
            {
                btnRun.Enabled = false;
                btnStop.Enabled = true;
                btnHide.Enabled = true;
            }

            string status = listApps.SelectedItems[0].SubItems[3].Text;
            if (status == "Running - Visible Window")
            {
                btnHide.Text = "Hide Server Window";
            }
            else if (status == "Running - Hidden Window")
            {
                btnHide.Text = "Show Server Window";
            }
            else
            {
                btnHide.Text = "Hide Server Window";
            }
        }

        private void listApps_DoubleClick(object sender, EventArgs e)
        {
            if (listApps.SelectedItems.Count != 0)
            {
                Form dlg = new dlgProps(listApps.SelectedItems[0]);
                dlg.ShowDialog();
     
                updateFile();
            }
        }
        private void updateFile()
        {
            try
            {
                StreamWriter sw = new StreamWriter("servers.ini");
                foreach (ListViewItem item in listApps.Items)
                {
                    sw.WriteLine("[" + item.SubItems[0].Text + "]");
                    sw.WriteLine("Path=" + item.SubItems[1].Text);
                    sw.WriteLine("Port=" + item.SubItems[2].Text);
                }
                sw.Close();
            }
            catch { }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listApps.SelectedItems[0].Remove();
            updateFile();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            string path1 = listApps.SelectedItems[0].SubItems[1].Text;
            string name1 = listApps.SelectedItems[0].SubItems[0].Text;
            StreamWriter sw = new StreamWriter("lastUpload.bat");
            //sw.WriteLine("@echo off");
            sw.WriteLine("title Uploading to Google Apps Engine: " + name1);
            sw.WriteLine(path1.Substring(0, 2));

            string[] s = path1.Split('\\');
            sw.WriteLine("cd " + path1.Substring(0, path1.Length - s[s.Length - 1].Length));

            sw.WriteLine("appcfg.py update " + name1);
            sw.Close();
            System.Diagnostics.Process uploader = new System.Diagnostics.Process();
            uploader.StartInfo.FileName = "lastUpload.bat";
            uploader.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (Process p in procs)
            {
                if (p != null)
                {
                    p.CloseMainWindow();
                    p.Close();
                }
            }
        }


    }

}