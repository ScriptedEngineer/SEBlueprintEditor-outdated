using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace BlueprintEditor
{
    public partial class Form2 : Form
    {
        string UpdateUrl; Form2 ThisForm; string AppFile; Form1 MainF;
        public Form2(string UpdUrl, Form1 Main)
        {
            InitializeComponent();
            UpdateUrl = UpdUrl;
            ThisForm = this;
            MainF = Main;
        }

        public void SetColor(Color Fore, Color Back)
        {
            BackColor = Back;
            Recolor(Controls, Fore, Back);
        }

        void Recolor(Control.ControlCollection Controlls, Color ForeColor, Color BackColor)
        {
            foreach (Control Contr in Controlls)
            {
                Contr.ForeColor = ForeColor;
                if (Contr.BackColor != Color.Transparent) Contr.BackColor = BackColor;
                Recolor(Contr.Controls, ForeColor, BackColor);
            }
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            if (ArhApi.IsLink(UpdateUrl))
            {
                ArhApi.CompliteAsync(() =>
                {
                    WebClient web = new WebClient();
                    AppFile = Application.ExecutablePath;
                    web.DownloadFileAsync(new Uri(UpdateUrl), Path.GetFileNameWithoutExtension(AppFile) + ".update");
                    web.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged2);
                    web.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted2);
                });
                MainF.Hide();
            }
            else
            {
                label2.Text = "Error";
            }
        }
        public void DownloadProgressChanged2(object sender, DownloadProgressChangedEventArgs e)
        {
            ThisForm.Invoke(new Action(() =>
            {
                progressBar1.Value = e.ProgressPercentage;
                //label1.Text = "Updating: Loading " + (e.BytesReceived / 1024).ToString() + "Kb/" + (e.TotalBytesToReceive / 1024).ToString() + "Kb";
            }));
        }
        public void DownloadFileCompleted2(object sender, AsyncCompletedEventArgs e)
        {
            FileStream Batch = File.Create("update.vbs");
            byte[] Data = Encoding.Default.GetBytes("WScript.Sleep(2000)"
+ "\nOn Error Resume next"
+ "\nDim fso, f1, f2, s"
+ "\nSet fso = CreateObject(\"Scripting.FileSystemObject\")"
+ "\nSet Del = fso.GetFile(\"" +AppFile+"\")"
+ "\nSet Upd = fso.GetFile(\"" +Path.GetDirectoryName(AppFile) + "\\"+Path.GetFileNameWithoutExtension(AppFile) + ".update\")"
+ "\nDel.Delete"
+ "\nUpd.Name = \"" + Path.GetFileName(AppFile)+"\""
+ "\nSet FSO = CreateObject(\"Scripting.FileSystemObject\")"
+ "\nSet F = FSO.GetFile(Wscript.ScriptFullName)"
+ "\npath = FSO.GetParentFolderName(F)"
+ "\nSet WshShell = WScript.CreateObject(\"WScript.Shell\")"
+ "\nWshShell.Run \"" + Path.GetFileName(AppFile) + "\""
+ "\nOn Error GoTo 0");
            Batch.Write(Data, 0, Data.Length);
            Batch.Close();
            Process.Start("update.vbs");
            Application.Exit();
        }
    }
}
