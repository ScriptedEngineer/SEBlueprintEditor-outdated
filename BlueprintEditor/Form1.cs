using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.IO;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;
using System.Globalization;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;

namespace BlueprintEditor
{
    public partial class Form1 : Form
    {

        Color AllForeColor = Color.FromArgb(240, 240, 240);
        Color AllBackColor = Color.FromArgb(20, 20, 20);

        public Form1()
        {
            InitializeComponent();

        }

        void Recolor(Control.ControlCollection Controlls, Color ForeColor, Color BackColor)
        {
            foreach (Control Contr in Controlls)
            {
                Contr.ForeColor = ForeColor;
                if (Contr.BackColor != Color.Transparent && Contr.Tag != "IgnBack") Contr.BackColor = BackColor;
                Recolor(Contr.Controls, ForeColor, BackColor);
            }
        }

        string Folder; Form3 Report;
        XmlDocument Blueprint; Form4 Calculator;
        List<XmlNode> Grides = new List<XmlNode>();
        List<XmlNode> Blocks = new List<XmlNode>();
        Dictionary<string, XmlNode> BlocksSorted = new Dictionary<string, XmlNode>();
        XmlNode Grid; List<string> Sorter = new List<string>();
        List<XmlNode> Block = new List<XmlNode>();
        string BluePathc; bool CalculateShip = true;
        Form1 MainF; string GamePath = ""; int SelectedArmor, SelectedArmorB;
        Settings Settings = new Settings();

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("update.vbs"))
            {
                File.Delete("update.vbs");
            }
            label19.Text = "v" + Application.ProductVersion;
            string[] Blueprints = new string[] { };
            Folder = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\SpaceEngineers\\Blueprints\\local";
            if (File.Exists("Config.dat"))
            {
                string Xml = File.ReadAllText("Config.dat");
                if (Xml != "")
                {
                    Settings = ArhApi.DeserializeClass<Settings>(Xml);
                    Folder = Settings.BlueprintPath;
                    GamePath = Settings.GamePath;
                    comboBox9.SelectedIndex = Settings.LangID;
                    if (Settings.Theme == -1)
                    {
                        AllBackColor = Settings.BackColor.GetColor();
                        AllForeColor = Settings.ForeColor.GetColor();
                        comboBox10.Text = "Custom";
                    }
                    else
                    {
                        Settings.BackColor = new MyColor(AllBackColor);
                        Settings.ForeColor = new MyColor(AllForeColor);
                        comboBox10.SelectedIndex = Settings.Theme;
                    }
                }
            }
            else
            {
                comboBox10.SelectedIndex = 0;
                comboBox9.SelectedIndex = Settings.LangID = CultureInfo.CurrentCulture.NativeName == "русский (Россия)" ? 1 : 0;
            }
            if (GamePath == "")
            {
                try
                {
                    string SteamDir = Registry.GetValue("HKEY_CURRENT_USER\\Software\\Valve\\Steam", "SteamPath", "Error").ToString();
                    if (SteamDir != "Error")
                    {
                        string ConfigFile = File.ReadAllText(SteamDir + "\\config\\config.vdf");
                        Regex regex = new Regex(".+?(?=\"BaseInstallFolder_1\"		\"|\"SentryFile\"|$)", RegexOptions.Singleline);
                        var matches = regex.Matches(ConfigFile);
                        string[] Matches = matches[1].Value.Split('\"');
                        if (Matches[3] != "")
                        {
                            string Patch = Matches[3] + "\\steamapps\\common\\SpaceEngineers";
                            if (File.Exists(Patch + "\\Bin64\\SpaceEngineers.exe"))
                            {
                                GamePath = Patch;
                            }
                            else
                            {
                                Patch = SteamDir + "\\steamapps\\common\\SpaceEngineers";
                                if (File.Exists(Patch + "\\Bin64\\SpaceEngineers.exe"))
                                {
                                    GamePath = Patch;
                                }
                                else
                                {
                                    CalculateShip = false;
                                }
                            }
                        }
                        else
                        {
                            string Patch = SteamDir + "\\steamapps\\common\\SpaceEngineers";
                            if (File.Exists(Patch + "\\Bin64\\SpaceEngineers.exe"))
                            {
                                GamePath = Patch;
                            }
                            else
                            {
                                CalculateShip = false;
                            }
                        }
                    }
                    else
                    {
                        CalculateShip = false;
                    }
                }
                catch (Exception Expt)
                {
                    CalculateShip = false;
                }
            }
            if (GamePath == "" && !CalculateShip)
            {
                button3.Visible = false;
            }
            if (Directory.Exists(Folder))
            {
                Blueprints = Directory.GetDirectories(Folder);
            }
            if (Blueprints.Length == 0)
            {
                folderBrowserDialog1.ShowNewFolderButton = false;
                folderBrowserDialog1.Description =
                    Settings.LangID == 0 ?
                    "It seems that we couldn't find your blueprints, please select the blueprints folder." :
                    "Кажется мы не смогли найти ваши чертежи, пожалуйста, выберите папку с чертежами.";
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    Folder = folderBrowserDialog1.SelectedPath;
                    Blueprints = Directory.GetDirectories(Folder);
                }
                else
                {
                    Application.Exit();
                }
            }
            for (int i = 0; i < Blueprints.Length; i++)
            {
                Blueprints[i] = Path.GetFileName(Blueprints[i]);
            }
            listBox1.Items.Clear();
            List<string> listBox1Items = new List<string>();
            List<string> Brushes = new List<string>();
            foreach (string BlueD in Blueprints)
            {

                if (File.Exists(Folder + "\\" + BlueD + "\\bp.sbc")
                    && File.Exists(Folder + "\\" + BlueD + "\\thumb.png"))
                {
                    if (BlueD.StartsWith("PaintBrush-"))
                    {
                        Brushes.Add(BlueD.Replace("PaintBrush-", ""));
                    }
                    else
                    {
                        listBox1Items.Add(BlueD);
                    }
                }
            }
            listBox1.Items.AddRange(listBox1Items.ToArray());
            if (Brushes.Count > 0)
            {
                comboBox11.Items.AddRange(Brushes.ToArray());
                comboBox11.SelectedIndex = 0;
                button7.Visible = true;
                comboBox11.Visible = true;
                label24.Visible = true;
            }
            MainF = this;
            ArhApi.CompliteAsync(() =>
            {
                string UpdateUrl = ArhApi.Server("nalgversion");
                if (UpdateUrl != "You New" && ArhApi.IsLink(UpdateUrl))
                {
                    MainF.Invoke(new Action(() =>
                    {
                        Form2 Updater = new Form2(UpdateUrl, this);
                        ArhApi.LoadForm(Updater);
                        Updater.SetColor(AllForeColor, AllBackColor);
                    }));
                }
            });
            comboBox8.SelectedIndex = 1;
            string ModFolder = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\SpaceEngineers\\Mods";
            if (Directory.Exists(ModFolder) && Directory.GetFiles(ModFolder).Length > 0)
            {
                button5.Visible = true;
            }
            ChangeLang(this, comboBox9.SelectedIndex);
            BackColor = AllBackColor;
            Recolor(Controls, AllForeColor, AllBackColor);
            //FormsLoad();Future
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                ClearEditorGrid(); ClearEditorBlock();
                if (Calculator != null && !Calculator.IsDisposed) Calculator.Hide();
                BluePathc = Folder + "\\" + listBox1.Items[listBox1.SelectedIndex];
                Image img = Image.FromFile(BluePathc + "\\thumb.png", true);
                pictureBox1.SuspendLayout();
                //pictureBox1.Image = img;
                pictureBox1.Image = SetImgOpacity(img, 1);
                pictureBox1.ResumeLayout();
                Blueprint = new XmlDocument();
                string[] Translate = new string[] { "Blocks", "Блоки" };
                label2.Text = Translate[Settings.LangID];
                Blueprint.Load(BluePathc + "\\bp.sbc");
                XmlNodeList Grids = Blueprint.GetElementsByTagName("CubeGrid");
                listBox3.Items.Clear(); Grides.Clear(); listBox2.Items.Clear(); Blocks.Clear();
                List<string> listBox3Items = new List<string>();
                foreach (XmlNode Grid in Grids)
                {
                    Grides.Add(Grid);
                    foreach (XmlNode Child in Grid.ChildNodes)
                    {
                        if (Child.Name == "DisplayName")
                        {
                            listBox3Items.Add(Child.InnerText);
                            break;
                        }
                    }
                }
                listBox3.Items.AddRange(listBox3Items.ToArray());
                listBox3.SelectedIndex = 0;
                label3.Visible = true;
                listBox3.Visible = true;
                button3.Enabled = true;
                button2.Enabled = true;
            }
        }
        public static Image SetImgOpacity(Image imgPic, float imgOpac)
        {
            Bitmap bmpPic = new Bitmap(imgPic.Width, imgPic.Height);
            Graphics gfxPic = Graphics.FromImage(bmpPic);
            ColorMatrix cmxPic = new ColorMatrix();
            cmxPic.Matrix33 = imgOpac;
            cmxPic.Matrix23 = imgOpac;
            cmxPic.Matrix13 = imgOpac;
            cmxPic.Matrix03 = imgOpac;
            cmxPic.Matrix43 = imgOpac;
            ImageAttributes iaPic = new ImageAttributes();
            iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            gfxPic.DrawImage(imgPic, new Rectangle(0, 0, bmpPic.Width, bmpPic.Height), 0, 0, imgPic.Width, imgPic.Height, GraphicsUnit.Pixel, iaPic);
            gfxPic.Dispose();
            return bmpPic;
            GC.Collect();
        }

        private void ClearEditorGrid()
        {
            //button7.Enabled = false;
            textBox1.Text = "";
            textBox4.Text = "";
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox11.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            comboBox6.SelectedIndex = -1;
            comboBox3.Items.Clear();
            SetEnableCombo(comboBox6, false);
            textBox1.Enabled = false;
            SetEnableCombo(comboBox1, false);
            SetEnableCombo(comboBox2, false);
            panel1.Enabled = false;
            SetEnableCombo(comboBox3, false);
            button7.Enabled = false;
            SetEnableCombo(comboBox11, false);
        }
        private void ClearEditorBlock()
        {
            textBox3.Text = "";
            textBox2.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            textBox10.Text = "";
            textBox9.Text = "";
            button6.Text = "None";
            button6.Tag = "None|Ничего";
            button6.Visible = false;
            if (ImageConvert != null && !ImageConvert.IsDisposed) ImageConvert.Close();
            if (File.Exists("EditProgramTmpFile.cs")) File.Delete("EditProgramTmpFile.cs");
            if (File.Exists("EditTmpFile.txt")) File.Delete("EditTmpFile.txt");
            comboBox4.SelectedIndex = -1;
            comboBox5.SelectedIndex = -1;
            SetEnableCombo(comboBox7, false);
            comboBox7.SelectedIndex = -1;
            button4.Enabled = false;
            SetEnableCombo(comboBox4, false);
            SetEnableCombo(comboBox5, false);
            pictureBox4.Enabled = false;
            textBox2.Enabled = false;
            textBox9.Enabled = false;
            textBox3.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;
            textBox10.Enabled = false;
            textBox8.Enabled = false;
            panel2.Visible = false;
        }

        void SetEnableCombo(ComboBox Box, bool Enable)
        {
            if (Box.Size.Width != 1) Box.Tag = Box.Width.ToString();
            if (Enable)
            {
                int PArze;
                int.TryParse(Box.Tag.ToString(), out PArze);
                if (PArze != 0) Box.Width = PArze;
            }
            else
            {
                Box.Width = 1;
            }
            Box.Enabled = Enable;
        }

        int OldGridSelect;
        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OldGridSelect == listBox3.SelectedIndex)
                UpdateBlocks();
            else
                UpdateBlocksNoSett();
            OldGridSelect = listBox3.SelectedIndex;
            if (listBox2.Items.Count == 1) listBox2.SetSelected(0, true);
        }

        List<XmlNode> SelectedSaveVar = new List<XmlNode>();

        void UpdateBlocks()
        {
            SelectedSaveVar.Clear();
            foreach (int Index in listBox2.SelectedIndices)
            {
                SelectedSaveVar.Add(BlocksSorted[Sorter[Index]]);
            }
            UpdateBlocksNoSett();
            listBox2.BeginUpdate();
            foreach (XmlNode Sel in SelectedSaveVar)
            {
                if (BlocksSorted.ContainsValue(Sel))
                {
                    string Key = BlocksSorted.FirstOrDefault(x => x.Value == Sel).Key;
                    listBox2.SetSelected(Sorter.IndexOf(Key), true);
                }
            }
            listBox2.EndUpdate();
            SelectedSaveVar.Clear();
        }

        void UpdateBlocksNoSett()
        {
            int Heavy = 0, Light = 0, numerer = 0;
            ClearEditorGrid();
            ClearEditorBlock();
            if (listBox3.SelectedIndex < Grides.Count && listBox3.SelectedIndex > -1)
            {
                Grid = Grides[listBox3.SelectedIndex];
                Blocks.Clear();
                foreach (XmlNode Child in Grid.ChildNodes)
                {
                    if (Child.Name == "CubeBlocks")
                    {
                        Sorter.Clear(); BlocksSorted.Clear();
                        foreach (XmlNode Childs in Child.ChildNodes)
                        {
                            bool HasName = false;
                            foreach (XmlNode Cld in Childs.ChildNodes)
                            {
                                if (Cld.Name == "CustomName")
                                {
                                    if (comboBox8.SelectedIndex == 1)
                                    {
                                        Sorter.Add(Cld.InnerText + "|" + numerer);
                                        BlocksSorted.Add(Cld.InnerText + "|" + numerer, Childs);
                                        HasName = true;
                                        break;
                                    }
                                }
                                else
                                if (Cld.Name == "Min")
                                {
                                    if (comboBox8.SelectedIndex == 2)
                                    {
                                        string Pos = "X:" + Cld.Attributes[0].Value + " Y:" + Cld.Attributes[1].Value + " Z:" + Cld.Attributes[2].Value;
                                        Sorter.Add(Pos + "|" + numerer);
                                        BlocksSorted.Add(Pos + "|" + numerer, Childs);
                                        HasName = true;
                                        break;
                                    }
                                }
                                else
                                if (Cld.Name == "ColorMaskHSV")
                                {
                                    if (comboBox8.SelectedIndex == 3)
                                    {
                                        string Pos = "H:" + Cld.Attributes[0].Value + " S:" + Cld.Attributes[1].Value + " V:" + Cld.Attributes[2].Value;
                                        Sorter.Add(Pos + "|" + numerer);
                                        BlocksSorted.Add(Pos + "|" + numerer, Childs);
                                        HasName = true;
                                        break;
                                    }
                                }
                            }
                            if (Childs.FirstChild.InnerText != "")
                            {
                                string Type = Childs.FirstChild.InnerText;
                                if (comboBox8.SelectedIndex == 0 || !HasName)
                                {
                                    Sorter.Add(Childs.FirstChild.InnerText + "|" + numerer);
                                    BlocksSorted.Add(Childs.FirstChild.InnerText + "|" + numerer, Childs);
                                }
                                if (Type.Contains("Armor"))
                                {
                                    if (Type.Contains("Heavy"))
                                    {
                                        Heavy++;
                                    }
                                    else
                                    {
                                        Light++;
                                    }
                                }
                            }
                            else
                            {
                                foreach (XmlNode Cld in Childs.ChildNodes)
                                {
                                    if (Cld.Name == "CustomName" && !HasName)
                                    {
                                        //listBox2.Items.Add(Cld.InnerText);
                                        Sorter.Add(Cld.InnerText + "|" + numerer);
                                        BlocksSorted.Add(Cld.InnerText + "|" + numerer, Childs);
                                        break;
                                    }
                                }
                            }
                            foreach (XmlNode Cld in Childs.ChildNodes)
                            {
                                if (Cld.Name == "ColorMaskHSV")
                                {
                                    if (!comboBox3.Items.Contains(Cld.Attributes[0].Value + ":" + Cld.Attributes[1].Value + ":" + Cld.Attributes[2].Value)) comboBox3.Items.Add(Cld.Attributes[0].Value + ":" + Cld.Attributes[1].Value + ":" + Cld.Attributes[2].Value);
                                    break;
                                }
                            }
                            Blocks.Add(Childs);
                            numerer++;
                        }
                        /*if(checkBox1.Checked)Sorter.Sort();
                        ArhApi.ListBoxFill(Sorter.ToArray(), listBox2, numerer.ToString().Length);
                        label2.Text = "Blocks (" + numerer + ")";*/
                    }
                    else if (Child.Name == "DisplayName")
                    {
                        textBox1.Text = Child.InnerText;
                        textBox1.Enabled = true;
                    }
                    else if (Child.Name == "DestructibleBlocks")
                    {
                        SetEnableCombo(comboBox1, true);
                        comboBox1.SelectedIndex = Child.InnerText == "True" ? 1 : 0;
                        //comboBox1.Enabled = true;
                    }
                    else if (Child.Name == "GridSizeEnum")
                    {
                        SetEnableCombo(comboBox2, true);
                        comboBox2.SelectedIndex = Child.InnerText == "Large" ? 1 : 0;
                        //comboBox2.Enabled = true;
                    }
                }
                if (checkBox1.Checked) Sorter.Sort();
                listBox2.Items.Clear();
                listBox2.Items.AddRange(ListFill(Sorter.ToArray(), numerer.ToString().Length));
                string[] Translate = new string[] { "Blocks", "Блоки" };
                label2.Text = Translate[Settings.LangID] + " (" + numerer + ")";
                if (comboBox3.Items.Count > 0)
                {
                    SetEnableCombo(comboBox3, true);
                    panel1.Enabled = true; comboBox3.SelectedIndex = 0;
                }
                if (Light != 0 || Heavy != 0)
                {
                    //comboBox6.Enabled = true;
                    SetEnableCombo(comboBox6, true);
                    SelectedArmor = Light > Heavy ? 0 : 1;
                    comboBox6.SelectedIndex = SelectedArmor;
                }
                if (comboBox11.Items.Count > 0)
                {
                    button7.Enabled = true;
                    SetEnableCombo(comboBox11, true);
                    comboBox11.SelectedIndex = 0;
                }
            }
        }
        static public string[] ListFill(string[] Elements, int DigitLenght)
        {
            List<string> St = new List<string>();
            foreach (string Element in Elements)
            {
                string[] ElementAr = Element.Split('|');
                St.Add((ElementAr.Length > 1 ? ElementAr[1].PadLeft(DigitLenght, '0') + "." : "") + ElementAr[0]);
            }
            return St.ToArray();
        }
        void UpdateColors()
        {
            comboBox3.BeginUpdate();
            comboBox3.Items.Clear();
            foreach (XmlNode Bl in Blocks)
            {
                foreach (XmlNode Cld in Bl.ChildNodes)
                {
                    if (Cld.Name == "ColorMaskHSV")
                    {
                        if (!comboBox3.Items.Contains(Cld.Attributes[0].Value + ":" + Cld.Attributes[1].Value + ":" + Cld.Attributes[2].Value)) comboBox3.Items.Add(Cld.Attributes[0].Value + ":" + Cld.Attributes[1].Value + ":" + Cld.Attributes[2].Value);
                    }
                }
            }
            comboBox3.SelectedIndex = 0;
            comboBox3.EndUpdate();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
            if (listBox3.SelectedIndex != -1)
            {
                foreach (XmlNode Child in Grid.ChildNodes)
                {
                    if (Child.Name == "DisplayName")
                    {
                        if (textBox1.Text != "")
                        {
                            Child.InnerText = textBox1.Text;
                            listBox3.Items[listBox3.SelectedIndex] = textBox1.Text;
                        }
                        break;
                    }
                }
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (XmlNode Child in Grid.ChildNodes)
            {
                if (Child.Name == "DestructibleBlocks")
                {
                    if (comboBox1.SelectedIndex != -1) Child.InnerText = (Convert.ToBoolean(comboBox1.SelectedIndex)).ToString().ToLower();
                    break;
                }
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (XmlNode Child in Grid.ChildNodes)
            {
                if (Child.Name == "GridSizeEnum")
                {
                    if (comboBox2.SelectedIndex != -1) Child.InnerText = Convert.ToBoolean(comboBox2.SelectedIndex) ? "Large" : "Small";
                    break;
                }
            }
        }
        private void SaveBlueprint()
        {
            Blueprint.Save(BluePathc + "\\bp.sbc");
            if (File.Exists(BluePathc + "\\bp.sbcPB")) File.Delete(BluePathc + "\\bp.sbcPB");
            if (File.Exists(BluePathc + "\\bp.sbcB1")) File.Delete(BluePathc + "\\bp.sbcB1");
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox2.SelectedIndex != -1)
            {
                Block.Clear();
                ClearEditorBlock(); string SubtypeName = ""; string BuiltBy = ""; int Heavy = 0, Light = 0;
                string MinX = ""; string MinY = ""; string MinZ = ""; bool IsArmor = true; string Color = "";
                string CustomName = ""; bool FirsTrart = true; string OrForw = ""; string OrUp = "";
                int ColorCount = 0; int CustomnameCount = 0; int CountOrient = 0; int CountMin = 0;
                int CountBuilt = 0; int CountSubt = 0; string TypeName = "";
                foreach (int Index in listBox2.SelectedIndices)
                {
                    XmlNode Blocke = BlocksSorted[Sorter[Index]];
                    if (Blocke.Attributes.GetNamedItem("xsi:type") != null)
                    {
                        if (FirsTrart) TypeName = Blocke.Attributes.GetNamedItem("xsi:type").Value;
                        if (TypeName != Blocke.Attributes.GetNamedItem("xsi:type").Value) TypeName = "";
                    }
                    else
                    {
                        TypeName = "";
                    }
                    Block.Add(Blocke);
                    foreach (XmlNode Child in BlocksSorted[Sorter[Index]].ChildNodes)
                    {
                        if (Child.Name == "SubtypeName")
                        {
                            if (Child.InnerText != "")
                            {
                                if (FirsTrart) SubtypeName = Child.InnerText;
                                if (SubtypeName != Child.InnerText) SubtypeName = "";
                            }
                            else
                            {
                                SubtypeName = "";
                            }
                            IsArmor = IsArmor && Child.InnerText.Contains("Armor");
                            if (Child.InnerText.Contains("Armor") && Child.InnerText.Contains("Heavy"))
                            {
                                Heavy++;
                            }
                            else if (Child.InnerText.Contains("Armor"))
                            {
                                Light++;
                            }
                            CountSubt++;
                        }
                        else if (Child.Name == "CustomName")
                        {
                            if (Child.InnerText != "")
                            {
                                if (FirsTrart) CustomName = Child.InnerText;
                                if (CustomName != Child.InnerText) CustomName = "";
                            }
                            else
                            {
                                CustomName = "";
                            }
                            CustomnameCount++;
                        }
                        else if (Child.Name == "BuiltBy")
                        {
                            if (Child.InnerText != "")
                            {
                                if (FirsTrart) BuiltBy = Child.InnerText;
                                if (BuiltBy != Child.InnerText) BuiltBy = "";
                            }
                            else
                            {
                                BuiltBy = "";
                            }
                            CountBuilt++;
                        }
                        else if (Child.Name == "BlockOrientation")
                        {
                            if (Child.Attributes[0].Value != null && Child.Attributes[1].Value != null)
                            {
                                if (FirsTrart)
                                {
                                    OrForw = Child.Attributes[0].Value;
                                    OrUp = Child.Attributes[1].Value;
                                }
                                if (OrForw != Child.Attributes[0].Value) OrForw = "";
                                if (OrUp != Child.Attributes[1].Value) OrUp = "";
                            }
                            else
                            {
                                OrUp = "";
                                OrForw = "";
                            }
                            CountOrient++;
                        }
                        else if (Child.Name == "Min")
                        {
                            if (Child.Attributes[0].Value != null && Child.Attributes[1].Value != null && Child.Attributes[2].Value != null)
                            {
                                if (FirsTrart)
                                {
                                    MinX = Child.Attributes[0].Value;
                                    MinY = Child.Attributes[1].Value;
                                    MinZ = Child.Attributes[2].Value;
                                }
                                if (MinX != Child.Attributes[0].Value) MinX = "";
                                if (MinY != Child.Attributes[1].Value) MinY = "";
                                if (MinZ != Child.Attributes[2].Value) MinZ = "";
                            }
                            else
                            {
                                MinX = "";
                                MinY = "";
                                MinZ = "";
                            }
                            CountMin++;
                        }
                        else if (Child.Name == "ColorMaskHSV")
                        {
                            if (Child.Attributes[0].Value != null && Child.Attributes[1].Value != null && Child.Attributes[2].Value != null)
                            {
                                string Colore = Child.Attributes[0].Value + ":" + Child.Attributes[1].Value + ":" + Child.Attributes[2].Value;
                                if (FirsTrart)
                                {
                                    Color = Colore;
                                }
                                if (Color != Colore)
                                {
                                    Color = "";
                                }
                            }
                            else
                            {
                                Color = "";
                            }
                            ColorCount++;
                        }
                        else if (Child.Name == "Program" && listBox2.SelectedIndices.Count == 1)
                        {
                            button6.Text = Settings.LangID == 0 ? "Edit program" : "Изменить скрипт";
                            button6.Tag = "Edit program|Изменить скрипт";
                            EXTData = Child.InnerText;
                            button6.Visible = true;
                        }
                        else if (Child.Name == "PublicDescription" && listBox2.SelectedIndices.Count == 1 && BlocksSorted[Sorter[Index]].Attributes.GetNamedItem("xsi:type").Value == "MyObjectBuilder_TextPanel")
                        {
                            button6.Text = Settings.LangID == 0 ? "Edit text" : "Изменить текст";
                            button6.Tag = "Edit text|Изменить текст";
                            EXTData = Child.InnerText;
                            button6.Visible = true;
                            Regex Regex = new Regex("[\ue100-\ue2FF]");
                            panel2.Visible = true;
                            if (Regex.Match(EXTData).Success)
                            {
                                string[] Lines = EXTData.Split('\n');int X = 0, Y = 0;
                                Bitmap Bmp = new Bitmap(Lines[0].Length, Lines.Length);
                                foreach (string Ziline in Lines)
                                {
                                    foreach (Char Chared in Ziline)
                                    {
                                        if(X < Bmp.Width)Bmp.SetPixel(X,Y,ColorUtils.CharToColor(Chared));
                                        X++;
                                    }
                                    X = 0;
                                    Y++;
                                }
                                pictureBox5.Image = Bmp;
                            }
                            else
                            {
                                pictureBox5.Image = Properties.Resources.Undefined;
                            }
                        }
                        else if (Child.Name == "ComponentContainer" && listBox2.SelectedIndices.Count == 1 && BlocksSorted[Sorter[Index]].Attributes.GetNamedItem("xsi:type").Value == "MyObjectBuilder_CargoContainer")
                        {
                            button6.Text = Settings.LangID == 0 ? "Edit inventory" : "Изменить инвентарь";
                            button6.Tag = "Edit inventory|Изменить инвентарь";
                            EXTXML = Child.FirstChild.FirstChild.LastChild.FirstChild;
                            button6.Visible = true;
                        }
                    }
                    FirsTrart = false;
                }
                if (TypeName != "")
                {
                    textBox10.Text = TypeName;
                    textBox10.Enabled = true;
                }
                if (CustomName != "" && CustomnameCount == Block.Count)
                {
                    textBox3.Text = CustomName;
                    textBox3.Enabled = true;
                }
                if (SubtypeName != "" && CountSubt == Block.Count)
                {
                    textBox2.Text = SubtypeName;
                    textBox2.Enabled = true;
                }
                if (IsArmor && (Light != 0 || Heavy != 0))
                {
                    SelectedArmorB = Light > Heavy ? 0 : 1;
                    //comboBox7.Enabled = true;
                    SetEnableCombo(comboBox7, true);
                    comboBox7.SelectedIndex = SelectedArmorB;
                }
                if (BuiltBy != "" && CountBuilt == Block.Count)
                {
                    textBox5.Text = BuiltBy;
                    textBox5.Enabled = true;
                }
                if (CountOrient == Block.Count)
                {
                    if (OrForw != "")
                    {
                        SetEnableCombo(comboBox4, true);
                        comboBox4.SelectedIndex = comboBox4.Items.IndexOf(OrForw);
                        //comboBox4.Enabled = true;
                    }
                    if (OrUp != "")
                    {
                        SetEnableCombo(comboBox5, true);
                        comboBox5.SelectedIndex = comboBox5.Items.IndexOf(OrUp);
                        //comboBox5.Enabled = true;
                    }
                }
                if (CountMin == Block.Count)
                {
                    if (MinX != "")
                    {
                        textBox6.Text = MinX;
                        textBox6.Enabled = true;
                    }
                    if (MinY != "")
                    {
                        textBox7.Text = MinY;
                        textBox7.Enabled = true;
                    }
                    if (MinZ != "")
                    {
                        textBox8.Text = MinZ;
                        textBox8.Enabled = true;
                    }
                }
                if (Color != "" && ColorCount == Block.Count)
                {
                    textBox9.Text = Color;
                    textBox9.Enabled = true;
                    pictureBox4.Enabled = true;
                }
                if (listBox2.SelectedIndex != -1) button4.Enabled = true;
            }
        }
        string EXTData; XmlNode EXTXML;
        private void textBox2_Leave(object sender, EventArgs e)
        {
            if (Block != null)
            {
                foreach (XmlNode Bl in Block)
                {
                    foreach (XmlNode Child in Bl.ChildNodes)
                    {
                        if (Child.Name == "SubtypeName")
                        {
                            if (textBox2.Text == "")
                            {
                                textBox2.Text = Child.InnerText;
                            }
                            Child.InnerText = textBox2.Text;
                        }
                    }
                }
                UpdateBlocks();
            }
        }

        private void textBox3_Leave(object sender, EventArgs e)
        {
            if (Block != null)
            {
                foreach (XmlNode Bl in Block)
                {
                    foreach (XmlNode Child in Bl.ChildNodes)
                    {
                        if (Child.Name == "CustomName")
                        {
                            if (textBox3.Text == "")
                            {
                                textBox3.Text = Child.InnerText;
                            }
                            Child.InnerText = textBox3.Text;
                        }
                    }
                }
                UpdateBlocks();
            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex != -1)
            {
                string[] Mask = comboBox3.SelectedItem.ToString().Split(':');
                string[] X_ = Mask[0].Replace('.', ',').Split('E');
                double X = X_.Length == 1 ? double.Parse(X_[0]) : Math.Pow(double.Parse(X_[0]), double.Parse(X_[1]));
                string[] Y_ = Mask[1].Replace('.', ',').Split('E');
                double Y = Y_.Length == 1 ? double.Parse(Y_[0]) : Math.Pow(double.Parse(Y_[0]), double.Parse(Y_[1]));
                string[] Z_ = Mask[2].Replace('.', ',').Split('E');
                double Z = Z_.Length == 1 ? double.Parse(Z_[0]) : Math.Pow(double.Parse(Z_[0]), double.Parse(Z_[1]));
                pictureBox2.BackColor = ColorUtils.ColorFromHSV(X * 360, Clamp((Y + 1) / 2), Clamp((Z + 1) / 2));
                textBox4.Text = comboBox3.SelectedItem.ToString();
            }
        }

        public class ColorUtils
        {

            public static Color CharToColor(char Ch)
            {
                int Chr = (int)Ch - 0xe100, pr, pg, pb, r, g, b;
                BitArray Bin = new BitArray(BitConverter.GetBytes(Chr));
                pr = (int)((Bin[0] ? 1 : 0) + ((Bin[1] ? 1 : 0) << 1) + ((Bin[2] ? 1 : 0) << 2));
                pg = (int)((Bin[3] ? 1 : 0) + ((Bin[4] ? 1 : 0) << 1) + ((Bin[5] ? 1 : 0) << 2));
                pb = (int)((Bin[6] ? 1 : 0) + ((Bin[7] ? 1 : 0) << 1) + ((Bin[8] ? 1 : 0) << 2));
                r = (int)(((float)pb / 7) * 255);
                g = (int)(((float)pg / 7) * 255);
                b = (int)(((float)pr / 7) * 255);
                return Color.FromArgb(r, g, b);
            }
            private static bool[] GetBinaryRepresentation(int i)
            {
                List<bool> result = new List<bool>();
                while (i > 0)
                {
                    int m = i % 2;
                    i = i / 2;
                    result.Add(m == 1);
                }
                result.Reverse();
                return result.ToArray();
            }
            public static char CharRGB(byte r = 7, byte g = 7, byte b = 7)
            {
                return (char)(0xe100 + (r << 6) + (g << 3) + b);
            }
            public static Color ColorFromHSV(double H, double S, double V)
            {
                int r, g, b;
                HsvToRgb(H, S, V, out r, out g, out b);
                return Color.FromArgb(r, g, b);
            }
            /// <summary>
            /// Convert HSV to RGB
            /// h is from 0-360
            /// s,v values are 0-1
            /// r,g,b values are 0-255
            /// Based upon http://ilab.usc.edu/wiki/index.php/HSV_And_H2SV_Color_Space#HSV_Transformation_C_.2F_C.2B.2B_Code_2
            /// </summary>
            static void HsvToRgb(double h, double S, double V, out int r, out int g, out int b)
            {
                // ######################################################################
                // T. Nathan Mundhenk
                // mundhenk@usc.edu
                // C/C++ Macro HSV to RGB

                double H = h;
                while (H < 0) { H += 360; };
                while (H >= 360) { H -= 360; };
                double R, G, B;
                if (V <= 0)
                { R = G = B = 0; }
                else if (S <= 0)
                {
                    R = G = B = V;
                }
                else
                {
                    double hf = H / 60.0;
                    int i = (int)Math.Floor(hf);
                    double f = hf - i;
                    double pv = V * (1 - S);
                    double qv = V * (1 - S * f);
                    double tv = V * (1 - S * (1 - f));
                    switch (i)
                    {

                        // Red is the dominant color

                        case 0:
                            R = V;
                            G = tv;
                            B = pv;
                            break;

                        // Green is the dominant color

                        case 1:
                            R = qv;
                            G = V;
                            B = pv;
                            break;
                        case 2:
                            R = pv;
                            G = V;
                            B = tv;
                            break;

                        // Blue is the dominant color

                        case 3:
                            R = pv;
                            G = qv;
                            B = V;
                            break;
                        case 4:
                            R = tv;
                            G = pv;
                            B = V;
                            break;

                        // Red is the dominant color

                        case 5:
                            R = V;
                            G = pv;
                            B = qv;
                            break;

                        // Just in case we overshoot on our math by a little, we put these here. Since its a switch it won't slow us down at all to put these here.

                        case 6:
                            R = V;
                            G = tv;
                            B = pv;
                            break;
                        case -1:
                            R = V;
                            G = pv;
                            B = qv;
                            break;

                        // The color is not defined, we should throw an error.

                        default:
                            //LFATAL("i Value error in Pixel conversion, Value is %d", i);
                            R = G = B = V; // Just pretend its black/white
                            break;
                    }
                }
                r = Clamp((int)(R * 255.0));
                g = Clamp((int)(G * 255.0));
                b = Clamp((int)(B * 255.0));
            }

            /// <summary>
            /// Clamp a value to 0-255
            /// </summary>
            static int Clamp(int i)
            {
                if (i < 0) return 0;
                if (i > 255) return 255;
                return i;
            }
        }
        double Clamp(double i)
        {
            if (i < 0) return 0;
            if (i > 1) return 1;
            return i;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            string[] Mask = textBox4.Text.Split(':');
            if (Mask.Length == 3 && Mask[0] != "")
            {
                try
                {
                    string[] X_ = Mask[0].Replace('.', ',').Split('E');
                    double X = X_.Length == 1 ? double.Parse(X_[0]) : Math.Pow(double.Parse(X_[0]), double.Parse(X_[1]));
                    string[] Y_ = Mask[1].Replace('.', ',').Split('E');
                    double Y = Y_.Length == 1 ? double.Parse(Y_[0]) : Math.Pow(double.Parse(Y_[0]), double.Parse(Y_[1]));
                    string[] Z_ = Mask[2].Replace('.', ',').Split('E');
                    double Z = Z_.Length == 1 ? double.Parse(Z_[0]) : Math.Pow(double.Parse(Z_[0]), double.Parse(Z_[1]));
                    pictureBox3.BackColor = ColorUtils.ColorFromHSV(X * 360, Clamp((Y + 1) / 2), Clamp((Z + 1) / 2));
                }
                catch
                {

                }
            }
        }

        private void textBox4_Leave(object sender, EventArgs e)
        {
            if (comboBox3.SelectedItem != null)
            {
                foreach (XmlNode Bl in Blocks)
                {
                    foreach (XmlNode Cld in Bl.ChildNodes)
                    {
                        if (Cld.Name == "ColorMaskHSV")
                        {
                            string Hsv = Cld.Attributes[0].Value + ":" + Cld.Attributes[1].Value + ":" + Cld.Attributes[2].Value;
                            if (Hsv == comboBox3.SelectedItem.ToString())
                            {
                                string[] Strs = textBox4.Text.Split(':');
                                Cld.Attributes[0].Value = Strs[0].Replace(',', '.');
                                Cld.Attributes[1].Value = Strs[1].Replace(',', '.');
                                Cld.Attributes[2].Value = Strs[2].Replace(',', '.');
                            }
                            break;
                        }
                    }
                }
                UpdateColors();
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color C = colorDialog1.Color;
                pictureBox3.BackColor = C;
                textBox4.Text = (C.GetHue() / 360).ToString() + ":"
                    + (C.GetSaturation() * 2 - 1).ToString() + ":"
                    + (C.GetBrightness() * 2 - 1).ToString();
                if (comboBox3.SelectedItem != null)
                {
                    foreach (XmlNode Bl in Blocks)
                    {
                        foreach (XmlNode Cld in Bl.ChildNodes)
                        {
                            if (Cld.Name == "ColorMaskHSV")
                            {
                                string Hsv = Cld.Attributes[0].Value + ":" + Cld.Attributes[1].Value + ":" + Cld.Attributes[2].Value;
                                if (Hsv == comboBox3.SelectedItem.ToString())
                                {
                                    string[] Strs = textBox4.Text.Split(':');
                                    Cld.Attributes[0].Value = Strs[0].Replace(',', '.');
                                    Cld.Attributes[1].Value = Strs[1].Replace(',', '.');
                                    Cld.Attributes[2].Value = Strs[2].Replace(',', '.');
                                }
                                break;
                            }
                        }
                    }
                    UpdateColors();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveBlueprint();
            int index = listBox1.SelectedIndex;
            listBox1.SelectedIndex = -1;
            listBox1.SelectedIndex = index;
        }

        private void textBox5_Leave(object sender, EventArgs e)
        {
            if (Block != null)
            {
                foreach (XmlNode Bl in Block)
                {
                    foreach (XmlNode Child in Bl.ChildNodes)
                    {
                        if (Child.Name == "BuiltBy")
                        {
                            if (textBox5.Text == "")
                            {
                                textBox5.Text = Child.InnerText;
                            }
                            Child.InnerText = textBox5.Text;
                        }
                    }
                }
                UpdateBlocks();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Settings.BlueprintPath = Folder;
            Settings.GamePath = GamePath;
            if (!File.Exists("Config.dat"))
            {
                FileStream FileSt = File.Create("Config.dat");
                FileSt.Close();
            }
            File.WriteAllText("Config.dat", ArhApi.SerializeClass<Settings>(Settings));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (Block != null)
            {
                foreach (XmlNode Bl in Block)
                {
                    XmlNode parent = Bl.ParentNode;
                    parent.RemoveChild(Bl);
                }
                UpdateBlocks();
            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox4.SelectedIndex != -1)
            {
                if (Block != null)
                {
                    foreach (XmlNode Bl in Block)
                    {
                        foreach (XmlNode Child in Bl.ChildNodes)
                        {
                            if (Child.Name == "BlockOrientation")
                            {
                                Child.Attributes[0].Value = comboBox4.Items[comboBox4.SelectedIndex].ToString();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox5.SelectedIndex != -1)
            {
                if (Block != null)
                {
                    foreach (XmlNode Bl in Block)
                    {
                        foreach (XmlNode Child in Bl.ChildNodes)
                        {
                            if (Child.Name == "BlockOrientation")
                            {
                                Child.Attributes[1].Value = comboBox5.Items[comboBox5.SelectedIndex].ToString();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void textBox6_Leave(object sender, EventArgs e)
        {
            if (textBox6.Text != "")
            {
                if (Block != null)
                {
                    foreach (XmlNode Bl in Block)
                    {
                        foreach (XmlNode Child in Bl.ChildNodes)
                        {
                            if (Child.Name == "Min")
                            {
                                Child.Attributes[0].Value = textBox6.Text;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void textBox7_Leave(object sender, EventArgs e)
        {
            if (textBox7.Text != "")
            {
                if (Block != null)
                {
                    foreach (XmlNode Bl in Block)
                    {
                        foreach (XmlNode Child in Bl.ChildNodes)
                        {
                            if (Child.Name == "Min")
                            {
                                Child.Attributes[1].Value = textBox7.Text;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void textBox8_Leave(object sender, EventArgs e)
        {
            if (textBox8.Text != "")
            {
                if (Block != null)
                {
                    foreach (XmlNode Bl in Block)
                    {
                        foreach (XmlNode Child in Bl.ChildNodes)
                        {
                            if (Child.Name == "Min")
                            {
                                Child.Attributes[2].Value = textBox8.Text;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Report == null || Report.IsDisposed)
            {
                Report = new Form3(button1);
                Report.SetColor(AllForeColor, AllBackColor);
                Report.ChangeLang(Settings.LangID);
            }
            Report.Hide();
            Report.ChangeLang(Settings.LangID);
            Report.Clear();
            Report.Show();
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox6.SelectedIndex != -1 && comboBox6.SelectedIndex != SelectedArmor)
            {
                foreach (XmlNode Bl in Blocks)
                {
                    foreach (XmlNode Child in Bl.ChildNodes)
                    {
                        if (Child.Name == "SubtypeName")
                        {
                            string Type = Child.InnerText;
                            if (Type.Contains("Armor"))
                            {
                                Child.InnerText = comboBox6.SelectedIndex == 1 ? Type.Replace("SmallBlock", "SmallHeavyBlock").Replace("LargeBlock", "LargeHeavyBlock").Replace("HeavyHalf", "Half").Replace("Half", "HeavyHalf") : Type.Replace("SmallHeavyBlock", "SmallBlock").Replace("LargeHeavyBlock", "LargeBlock").Replace("HeavyHalf", "Half");
                            }
                            break;
                        }
                    }
                }
                UpdateBlocks();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateBlocks();
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateBlocks();
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            string[] Mask = textBox9.Text.Split(':');
            if (Mask.Length == 3 && Mask[0] != "")
            {
                try
                {
                    string[] X_ = Mask[0].Replace('.', ',').Split('E');
                    double X = X_.Length == 1 ? double.Parse(X_[0]) : Math.Pow(double.Parse(X_[0]), double.Parse(X_[1]));
                    string[] Y_ = Mask[1].Replace('.', ',').Split('E');
                    double Y = Y_.Length == 1 ? double.Parse(Y_[0]) : Math.Pow(double.Parse(Y_[0]), double.Parse(Y_[1]));
                    string[] Z_ = Mask[2].Replace('.', ',').Split('E');
                    double Z = Z_.Length == 1 ? double.Parse(Z_[0]) : Math.Pow(double.Parse(Z_[0]), double.Parse(Z_[1]));
                    pictureBox4.BackColor = ColorUtils.ColorFromHSV(X * 360, Clamp((Y + 1) / 2), Clamp((Z + 1) / 2));
                }
                catch
                {

                }
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color C = colorDialog1.Color;
                pictureBox4.BackColor = C;
                textBox9.Text = (C.GetHue() / 360).ToString() + ":"
                    + (C.GetSaturation() * 2 - 1).ToString() + ":"
                    + (C.GetBrightness() * 2 - 1).ToString();
                foreach (XmlNode Bl in Block)
                {
                    foreach (XmlNode Cld in Bl.ChildNodes)
                    {
                        if (Cld.Name == "ColorMaskHSV")
                        {
                            string[] Strs = textBox9.Text.Split(':');
                            Cld.Attributes[0].Value = Strs[0].Replace(',', '.');
                            Cld.Attributes[1].Value = Strs[1].Replace(',', '.');
                            Cld.Attributes[2].Value = Strs[2].Replace(',', '.');
                        }
                    }
                }
                UpdateColors();
            }
        }

        private void textBox9_Leave(object sender, EventArgs e)
        {
            foreach (XmlNode Bl in Block)
            {
                foreach (XmlNode Cld in Bl.ChildNodes)
                {
                    if (Cld.Name == "ColorMaskHSV")
                    {
                        string[] Strs = textBox9.Text.Split(':');
                        Cld.Attributes[0].Value = Strs[0].Replace(',', '.');
                        Cld.Attributes[1].Value = Strs[1].Replace(',', '.');
                        Cld.Attributes[2].Value = Strs[2].Replace(',', '.');
                        break;
                    }
                }
            }
            UpdateColors();
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        public void CreateBlueprint(string Name, string XML)
        {
            string Pathb = Folder + "\\" + Name;
            Directory.CreateDirectory(Pathb);
            File.WriteAllText(Pathb + "\\bp.sbc", XML);
            Properties.Resources.forthumb.Save(Pathb + "\\thumb.png", ImageFormat.Png);
            if (File.Exists(Pathb + "\\bp.sbcPB")) File.Delete(Pathb + "\\bp.sbcPB");
            if (File.Exists(Pathb + "\\bp.sbcB1")) File.Delete(Pathb + "\\bp.sbcB1");
            if (!listBox1.Items.Contains(Name)) listBox1.Items.Add(Name);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ArhApi.CompliteAsync(() =>
            {
                if (Calculator == null || Calculator.IsDisposed)
                {
                    Invoke(new Action(() => {
                        label22.Text = "Loading Data... Please Wait";
                    }));
                    Calculator = new Form4(GamePath, this);
                    Calculator.SetColor(AllForeColor, AllBackColor);
                    Calculator.ChangeLang(Settings.LangID);
                }
                Invoke(new Action(() =>
                {
                    Calculator.Hide();
                    label22.Text = "Calculating... Please Wait";
                    Calculator.ClearBlocks();
                }));
                foreach (XmlNode MyBlock in Blueprint.GetElementsByTagName("MyObjectBuilder_CubeBlock"))
                {
                    string TypeOfBlock;
                    XmlNode xsitype = MyBlock.Attributes.GetNamedItem("xsi:type");
                    if (xsitype != null)
                    {
                        TypeOfBlock = xsitype.Value.Replace("MyObjectBuilder_", "").Replace("Projector", "MyObjectBuilder_Projector") + "/" + MyBlock.FirstChild.InnerText;
                    }
                    else
                    {
                        TypeOfBlock = "CubeBlock/" + MyBlock.FirstChild.InnerText;
                    }
                    Calculator.AddBlock(TypeOfBlock);
                }
                Invoke(new Action(() =>
                {
                    Calculator.ShowBlocks(listBox1.SelectedItem.ToString());
                    Calculator.ChangeLang(Settings.LangID);
                    Calculator.Show();
                    label22.Text = "";
                }));
            });
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string ModFolder = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\SpaceEngineers\\Mods";
            ArhApi.ClearFolder(ModFolder);
            button5.Visible = false;
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e)
        {
            ChangeLang(this, comboBox9.SelectedIndex);
            Settings.LangID = comboBox9.SelectedIndex;
            if (Calculator != null && !Calculator.IsDisposed) Calculator.ChangeLang(Settings.LangID);
            if (Report != null && !Report.IsDisposed) Report.ChangeLang(Settings.LangID);
            if (ImageConvert != null && !ImageConvert.IsDisposed) ImageConvert.ChangeLang(Settings.LangID);
        }

        void ChangeLang(Control Control, int Lang)
        {
            foreach (Control Contr in Control.Controls)
            {
                ChangeLang(Contr, Lang);
                try
                {
                    if (Contr.Tag is null) continue;
                    string tag = Contr.Tag.ToString();
                    if (tag is "") continue;
                    string[] Tagge = tag.Split('|');
                    if (Tagge[0] == "") { Contr.Tag = Contr.Text + tag; tag = Contr.Tag.ToString(); }
                    Contr.Text = Lang == 1 ? Contr.Text.Replace(Tagge[0], Tagge[1]) : Contr.Text.Replace(Tagge[1], Tagge[0]);
                }
                catch
                {

                }
            }
        }

        class Theme
        {
            public Color Fore;
            public Color Back;
            public Theme(Color _Back, Color _Fore)
            {
                Fore = _Fore;
                Back = _Back;
            }
        }

        List<Theme> Themes = new List<Theme>(new Theme[] {
            new Theme(Color.FromArgb(40, 40, 40),Color.FromArgb(230, 230, 230)),
            new Theme(SystemColors.Window,Color.DarkBlue),
            new Theme(Color.Black,Color.White),
            new Theme(Color.FromArgb(204, 173, 96),Color.Brown),
            new Theme(SystemColors.Window,SystemColors.ControlText),
            new Theme(Color.Orange,Color.Black),
            new Theme(Color.FromArgb(255,104,0),Color.Black)
                });

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            Settings.Theme = comboBox10.SelectedIndex;
            AllBackColor = Themes[comboBox10.SelectedIndex].Back;
            AllForeColor = Themes[comboBox10.SelectedIndex].Fore;
            BackColor = AllBackColor;
            Settings.BackColor = new MyColor(AllBackColor);
            Settings.ForeColor = new MyColor(AllForeColor);
            Recolor(Controls, AllForeColor, AllBackColor); 
            if (Report != null && !Report.IsDisposed)
                Report.SetColor(AllForeColor, AllBackColor);
            if (ImageConvert != null && !ImageConvert.IsDisposed)
                ImageConvert.SetColor(AllForeColor, AllBackColor);
            if (Calculator != null && !Calculator.IsDisposed)
                Calculator.SetColor(AllForeColor, AllBackColor);
        }
        private void button6_Click(object sender, EventArgs e)
        {
            switch (button6.Text)
            {
                case "Edit program":
                case "Изменить скрипт":
                    #region EditProgram
                    if (Settings.EditorProgram == "" || Settings.EditorProgram == null) Settings.EditorProgram = "notepad";
                    try
                    {
                        File.WriteAllText("EditProgramTmpFile.cs", EXTData.Replace("\n", "\r\n"));
                        Process Editor = Process.Start(Settings.EditorProgram + ".exe", "EditProgramTmpFile.cs");
                        if (Editor != null)
                        {
                            Editor.WaitForExit();
                            if (File.Exists("EditProgramTmpFile.cs"))
                            {
                                string Program = File.ReadAllText("EditProgramTmpFile.cs");
                                if (Block != null && Block.Count == 1 && button6.Visible)
                                {
                                    foreach (XmlNode Bl in Block)
                                    {
                                        foreach (XmlNode Child in Bl.ChildNodes)
                                        {
                                            if (Child.Name == "Program")
                                            {
                                                Child.InnerText = Program.Replace("\r\n", "\n");
                                                break;
                                            }
                                        }
                                    }
                                    UpdateBlocks();
                                }
                                File.Delete("EditProgramTmpFile.cs");
                            }
                        }
                        else
                        {
                            File.Delete("EditProgramTmpFile.cs");
                            if (Settings.LangID == 0)
                                MessageBox.Show("Please install " + Settings.EditorProgram, "Missing " + Settings.EditorProgram);
                            else
                                MessageBox.Show("Пожалуйста, установите " + Settings.EditorProgram, "Отсутствует " + Settings.EditorProgram);
                        }
                    }
                    catch
                    {
                        File.Delete("EditProgramTmpFile.cs");
                        if (Settings.LangID == 0)
                            MessageBox.Show("Please install " + Settings.EditorProgram, "Missing " + Settings.EditorProgram);
                        else
                            MessageBox.Show("Пожалуйста, установите " + Settings.EditorProgram, "Отсутствует " + Settings.EditorProgram);
                    }
                    #endregion
                    break;
                case "Edit inventory":
                case "Изменить инвентарь":
                    #region EditInventory
                    if (Settings.EditorProgram == "" || Settings.EditorProgram == null) Settings.EditorProgram = "notepad";
                    try
                    {
                        string Inventory = "#Inventory text editor#";
                        foreach (XmlNode Xm in EXTXML.ChildNodes)
                        {
                            XmlNode Type = Xm.ChildNodes[1];
                            Inventory += "\r\n" + Type.Attributes.GetNamedItem("xsi:type").Value.Replace("MyObjectBuilder_", "") + "/" + Type.FirstChild.InnerText + ":" + Xm.FirstChild.InnerText;
                        }
                        File.WriteAllText("EditTmpFile.txt", Inventory);
                        Process Editor = Process.Start(Settings.EditorProgram + ".exe", "EditTmpFile.txt");
                        if (Editor != null)
                        {
                            Editor.WaitForExit();
                            if (File.Exists("EditTmpFile.txt"))
                            {
                                string Program = File.ReadAllText("EditTmpFile.txt");
                                if (Block != null && Block.Count == 1 && button6.Visible)
                                {
                                    foreach (XmlNode Bl in Block)
                                    {
                                        foreach (XmlNode Child in Bl.ChildNodes)
                                        {
                                            if (Child.Name == "ComponentContainer")
                                            {
                                                string Inventoryed = ""; int Couner = 0;
                                                string[] Splited = Program.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                                                foreach (string inv in Splited)
                                                {
                                                    string[] Amount = inv.Split(':'), Type = Amount[0].Split('/');
                                                    if (Amount.Length > 1 && Type.Length > 1) Inventoryed += "<MyObjectBuilder_InventoryItem><Amount>" + Amount[1] + "</Amount><PhysicalContent xsi:type=\"MyObjectBuilder_" + Type[0] + "\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><SubtypeName>" + Type[1] + "</SubtypeName></PhysicalContent><ItemId>" + Couner + "</ItemId></MyObjectBuilder_InventoryItem>";
                                                    Couner++;
                                                }
                                                Child.FirstChild.FirstChild.LastChild.FirstChild.InnerXml = Inventoryed;
                                                Child.FirstChild.FirstChild.LastChild.ChildNodes[1].InnerText = Couner.ToString();
                                                break;
                                            }
                                        }
                                    }
                                    UpdateBlocks();
                                }
                                File.Delete("EditTmpFile.txt");
                            }
                        }
                        else
                        {
                            File.Delete("EditTmpFile.txt");
                            if (Settings.LangID == 0)
                                MessageBox.Show("Please install " + Settings.EditorProgram, "Missing " + Settings.EditorProgram);
                            else
                                MessageBox.Show("Пожалуйста, установите " + Settings.EditorProgram, "Отсутствует " + Settings.EditorProgram);
                        }
                    }
                    catch
                    {
                        File.Delete("EditTmpFile.txt");
                        if (Settings.LangID == 0)
                            MessageBox.Show("Please install " + Settings.EditorProgram, "Missing " + Settings.EditorProgram);
                        else
                            MessageBox.Show("Пожалуйста, установите " + Settings.EditorProgram, "Отсутствует " + Settings.EditorProgram);
                    }
                    #endregion
                    break;
                case "Edit text":
                case "Изменить текст":
                    #region EditText
                    if (Settings.EditorProgram == "" || Settings.EditorProgram == null) Settings.EditorProgram = "notepad";
                    try
                    {
                        File.WriteAllText("EditTmpFile.txt", EXTData.Replace("\n", "\r\n"));
                        Process Editor = Process.Start(Settings.EditorProgram + ".exe", "EditTmpFile.txt");
                        if (Editor != null)
                        {
                            Editor.WaitForExit();
                            if (File.Exists("EditTmpFile.txt"))
                            {
                                string Program = File.ReadAllText("EditTmpFile.txt");
                                if (Block != null && Block.Count == 1 && button6.Visible)
                                {
                                    foreach (XmlNode Bl in Block)
                                    {
                                        foreach (XmlNode Child in Bl.ChildNodes)
                                        {
                                            if (Child.Name == "PublicDescription")
                                            {
                                                Child.InnerText = Program.Replace("\r\n", "\n");
                                                break;
                                            }
                                        }
                                    }
                                    UpdateBlocks();
                                }
                                File.Delete("EditTmpFile.txt");
                            }
                        }
                        else
                        {
                            File.Delete("EditTmpFile.txt");
                            if (Settings.LangID == 0)
                                MessageBox.Show("Please install " + Settings.EditorProgram, "Missing " + Settings.EditorProgram);
                            else
                                MessageBox.Show("Пожалуйста, установите " + Settings.EditorProgram, "Отсутствует " + Settings.EditorProgram);
                        }
                    }
                    catch
                    {
                        File.Delete("EditTmpFile.txt");
                        if (Settings.LangID == 0)
                            MessageBox.Show("Please install " + Settings.EditorProgram, "Missing " + Settings.EditorProgram);
                        else
                            MessageBox.Show("Пожалуйста, установите " + Settings.EditorProgram, "Отсутствует " + Settings.EditorProgram);
                    }
                    #endregion
                    break;
            }
        }

        private void textBox10_Leave(object sender, EventArgs e)
        {
            if (Block != null)
            {
                foreach (XmlNode Bl in Block)
                {

                    if (textBox10.Text == "")
                    {
                        textBox10.Text = Bl.Attributes.GetNamedItem("xsi:type").Value;
                    }
                    Bl.Attributes.GetNamedItem("xsi:type").Value = textBox10.Text;
                }
                UpdateBlocks();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            List<string> Painters = new List<string>();
            string Brush = "PaintBrush-" + comboBox11.SelectedItem.ToString();
            string BluesPathc = Folder + "\\" + Brush;
            XmlDocument Blueprinte = new XmlDocument();
            Blueprinte.Load(BluesPathc + "\\bp.sbc");
            XmlNodeList Paints = Blueprinte.GetElementsByTagName("MyObjectBuilder_CubeBlock");
            foreach (XmlNode Paint in Paints)
            {
                foreach (XmlNode Child in Paint.ChildNodes)
                {
                    if (Child.Name == "ColorMaskHSV")
                    {
                        Painters.Add(Child.Attributes[0].Value + "|" + Child.Attributes[1].Value + "|" + Child.Attributes[2].Value);
                        break;
                    }
                }
            }
            foreach (XmlNode Child in Grid.ChildNodes)
            {
                if (Child.Name == "CubeBlocks")
                {
                    foreach (XmlNode Childs in Child.ChildNodes)
                    {

                        foreach (XmlNode Chold in Childs.ChildNodes)
                        {
                            if (Chold.Name == "ColorMaskHSV")
                            {
                                string[] Paint = Painters[ArhApi.Rand(0, Painters.Count)].Split('|');
                                Chold.Attributes[0].Value = Paint[0];
                                Chold.Attributes[1].Value = Paint[1];
                                Chold.Attributes[2].Value = Paint[2];
                                break;
                            }
                        }
                    }
                }
            }
        }

        #region Empty
        private void label8_Click_1(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label17_Click(object sender, EventArgs e)
        {

        }
        #endregion
        private void button9_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(saveFileDialog1.FilterIndex);
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        pictureBox5.Image.Save(saveFileDialog1.FileName, ImageFormat.Png);
                        break;
                    case 2:
                        pictureBox5.Image.Save(saveFileDialog1.FileName, ImageFormat.Jpeg);
                        break;
                    case 3:
                        pictureBox5.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
                        break;
                    case 4:
                        pictureBox5.Image.Save(saveFileDialog1.FileName, ImageFormat.Icon);
                        break;
                }
            }
        }
        Form5 ImageConvert;
        private void button8_Click(object sender, EventArgs e)
        {
            if (ImageConvert == null || ImageConvert.IsDisposed)
            {
                ImageConvert = new Form5(this);
                ImageConvert.SetColor(AllForeColor, AllBackColor);
                ImageConvert.ChangeLang(Settings.LangID);
            }
            ImageConvert.ImageAndRadio(pictureBox5.Image,textBox2.Text.Contains("Wide"),EXTData);
            ImageConvert.ChangeLang(Settings.LangID);
            ImageConvert.Show();
        }

        public void WritePic(string Pic)
        {
            if (Block != null && Block.Count == 1 && button6.Visible)
            {
                foreach (XmlNode Bl in Block)
                {
                    foreach (XmlNode Child in Bl.ChildNodes)
                    {
                        if (Child.Name == "PublicDescription")
                        {
                            Child.InnerText = Pic;
                        }
                        else if (Child.Name == "Font")
                        {
                            Child.Attributes[1].Value = "Monospace";
                        }
                        else if (Child.Name == "ShowText")
                        {
                            Child.InnerText = "PUBLIC";
                        }else if (Child.Name == "FontSize")
                        {
                            Child.InnerText = "0.1";
                        }
                    }
                }
                UpdateBlocks();
            }
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox7.SelectedIndex != -1 && comboBox7.SelectedIndex != SelectedArmorB)
            {
                if (Block != null)
                {
                    foreach (XmlNode Bl in Block)
                    {
                        foreach (XmlNode Child in Bl.ChildNodes)
                        {
                            if (Child.Name == "SubtypeName")
                            {
                                string Type = Child.InnerText;
                                if (Type.Contains("Armor"))
                                {
                                    Child.InnerText = comboBox7.SelectedIndex == 1 ? Type.Replace("SmallBlock", "SmallHeavyBlock").Replace("LargeBlock", "LargeHeavyBlock").Replace("HeavyHalf", "Half").Replace("Half", "HeavyHalf") : Type.Replace("SmallHeavyBlock", "SmallBlock").Replace("LargeHeavyBlock", "LargeBlock").Replace("HeavyHalf", "Half");
                                }
                                break;
                            }
                        }
                    }
                    UpdateBlocks();
                }
            }
        }
    }
}
