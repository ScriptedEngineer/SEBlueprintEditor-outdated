using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BlueprintEditor
{
    public partial class Form7 : Form
    {
        List<XmlNode> Blocks;
        public Form7(List<XmlNode> _Block)
        {
            Blocks = _Block;
            InitializeComponent();
            AddThree(Blocks[0]);
        }

        Regex NodeRegex = new Regex("#text|SubtypeName|CustomName|BuiltBy|BlockOrientation|Min|ColorMaskHSV|Program|PublicDescription|ComponentContainer", RegexOptions.None);
        static public Regex NodeRegexD = new Regex("^(UseSingleWeaponMode|PlaySound|IsActive|DetectAsteroids|DetectOwner|DetectFriendly|DetectNeutral|DetectEnemy|DetectPlayers|DetectFloatingObjects|DetectSmallShips|DetectLargeShips|DetectStations|DetectSubgrids|GyroOverride|Storage|Owner|ShareMode|ShowOnHUD|ShowInTerminal|ShowInToolbarConfig|ShowInInventory|Enabled|BroadcastRadius|ShowShipName|EnableBroadcasting|IgnoreAllied|IgnoreOther|AutoPilotEnabled|DockingModeEnabled|IsMainRemoteControl)$", RegexOptions.None);

        void AddThree(XmlNode node,string nameleveling = "")
        {
            treeView1.Nodes.Clear();
            treeView1.Nodes.AddRange(AddNode(node));
        }
        TreeNode[] AddNode(XmlNode node)
        {
            List<TreeNode> nodes = new List<TreeNode>();
            if(node.ChildNodes.Count == 0 && node.Name != "#text")return null;
            if (node.ChildNodes.Count == 1 && node.ChildNodes[0].Name != "#text") return null;
            foreach (XmlNode nodee in node.ChildNodes)
            {
                TreeNode[] childs = AddNode(nodee);
                if (NodeRegexD.IsMatch(nodee.Name) && childs != null) nodes.Add(new TreeNode(nodee.Name, childs));
            }
            return nodes.ToArray();
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

        private void Form7_Load(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {
            textBox1.Enabled = false;
            textBox1.Text = "";
            comboBox1.Enabled = false;
            comboBox1.Width = 1;
            comboBox2.Enabled = false;
            comboBox2.Width = 1;
            if (treeView1.SelectedNode.Nodes.Count == 0)
            {
                string data = Blocks[0].SelectSingleNode(treeView1.SelectedNode.FullPath).InnerText;
                switch (data)
                {
                    case "true":
                    case "True":
                        comboBox1.Width = 70;
                        comboBox1.Enabled = true;
                        comboBox1.SelectedIndex = 1;
                        break;
                    case "false":
                    case "False":
                        comboBox1.Width = 70;
                        comboBox1.Enabled = true;
                        comboBox1.SelectedIndex = 0;
                        break;
                    case "All":
                        comboBox2.Width = 120;
                        comboBox2.Enabled = true;
                        comboBox2.SelectedIndex = 0;
                        break;
                    case "Faction":
                        comboBox2.Width = 120;
                        comboBox2.Enabled = true;
                        comboBox2.SelectedIndex = 1;
                        break;
                    case "None":
                        comboBox2.Width = 120;
                        comboBox2.Enabled = true;
                        comboBox2.SelectedIndex = 2;
                        break;
                    default:
                        textBox1.Enabled = true;
                        textBox1.Text = data;
                        break;
                }

                //textBox1.Text = ;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    foreach (var Block in Blocks)
                    {
                        Block.SelectSingleNode(treeView1.SelectedNode.FullPath).InnerText = "false";
                    }
                    break;
                case 1:
                    foreach (var Block in Blocks)
                    {
                        Block.SelectSingleNode(treeView1.SelectedNode.FullPath).InnerText = "true";
                    }
                    break;
            }
        }
    }
}
