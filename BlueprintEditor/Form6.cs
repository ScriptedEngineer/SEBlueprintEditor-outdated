using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BlueprintEditor
{
    public partial class Form6 : Form
    {
        string UpdateUrl; Form1 MainF;
        public Form6(string UpdUrl, Form1 Main,string Avai)
        {
            InitializeComponent();
            UpdateUrl = UpdUrl;
            MainF = Main;
            label1.Text = Form1.Settings.LangID == 0?
                "Current version " + Application.ProductVersion+ ", available " + Avai :
                "Текущая версия " + Application.ProductVersion+ ", доступна " + Avai;
            ArhApi.CompliteAsync(() =>
            {
                string Log = PrepareLog(ArhApi.Server("GetUpdateLog"),true);
                Invoke(new Action(() => { textBox1.Text = Log; }));
            });
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
        public void ChangeLang(int Lang, Control Contre = null)
        {
            if (Contre == null)
            {
                Contre = this;
                if (Contre.Tag != null)
                {
                    string tag = Contre.Tag.ToString();
                    if (tag != "")
                    {
                        string[] Tagge = tag.Split('|');
                        if (Tagge[0] == "") Contre.Tag = Contre.Text + tag;
                        Contre.Text = Tagge[Lang];
                    }
                }
            }
            foreach (Control Contr in Contre.Controls)
            {
                ChangeLang(Lang, Contr);
                try
                {
                    if (Contr.Tag is null) continue;
                    string tag = Contr.Tag.ToString();
                    if (tag is "") continue;
                    string[] Tagge = tag.Split('|');
                    if (Tagge[0] == "") Contr.Tag = Contr.Text + tag;
                    Contr.Text = Tagge[Lang];
                }
                catch
                {

                }
            }
        }

        private void Form6_Load(object sender, EventArgs e)
        {

        }

        string PrepareLog(string log,bool cut = false)
        {
            string[] Versions = log.Split('*');
            string Backlog = "";
            foreach (var version in Versions)
            {
                bool breaked = false;
                string[] Strings = version.Split(new string[] {"\n", "\r", "\r\n"},StringSplitOptions.RemoveEmptyEntries);
                foreach (var stringe in Strings)
                {
                    string[] langs = stringe.Split('|');
                    if (langs.Length > 1)
                        Backlog += (langs[Form1.Settings.LangID])+ "\r\n";
                    else
                    {
                        if (cut && langs[0] == Application.ProductVersion + ":")
                        {
                            breaked = true;
                            break;
                        }
                        Backlog +=  langs[0]+"\r\n";
                    }

                    
                }
                if (breaked) break;
            }

            return Backlog;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 Updater = new Form2(UpdateUrl, MainF);
            ArhApi.LoadForm(Updater);
            Updater.SetColor(MainF.AllForeColor, MainF.AllBackColor);
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
