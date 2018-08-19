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
using System.Text.RegularExpressions;

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
            foreach (string BlueD in Blueprints) {

                if (File.Exists(Folder + "\\" + BlueD + "\\bp.sbc")
                    && File.Exists(Folder + "\\" + BlueD + "\\thumb.png")) listBox1Items.Add(BlueD);
            }
            listBox1.Items.AddRange(listBox1Items.ToArray());
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
            ClearEditorGrid(); ClearEditorBlock();
            if (Calculator != null && !Calculator.IsDisposed) Calculator.Hide();
            BluePathc = Folder + "\\" + listBox1.Items[listBox1.SelectedIndex];
            Image img = Image.FromFile(BluePathc + "\\thumb.png", true);
            pictureBox1.Image = SetImgOpacity(img, 5000);
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
            label3.Visible = true;
            listBox3.Visible = true;
            button3.Enabled = true;
            button2.Enabled = true;
        }
        public static Image SetImgOpacity(Image imgPic, float imgOpac)
        {
            Bitmap bmpPic = new Bitmap(imgPic.Width, imgPic.Height);
            Graphics gfxPic = Graphics.FromImage(bmpPic);
            ColorMatrix cmxPic = new ColorMatrix();
            cmxPic.Matrix33 = imgOpac;

            ImageAttributes iaPic = new ImageAttributes();
            iaPic.SetColorMatrix(cmxPic, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            gfxPic.DrawImage(imgPic, new Rectangle(0, 0, bmpPic.Width, bmpPic.Height), 0, 0, imgPic.Width, imgPic.Height, GraphicsUnit.Pixel, iaPic);
            gfxPic.Dispose();
            return bmpPic;
            GC.Collect();
        }

        private void ClearEditorGrid()
        {
            textBox1.Text = "";
            textBox4.Text = "";
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            comboBox6.SelectedIndex = -1;
            comboBox3.Items.Clear();
            //comboBox6.Enabled = false;
            SetEnableCombo(comboBox6, false);
            textBox1.Enabled = false;
            //comboBox1.Enabled = false;
            //comboBox2.Enabled = false;
            SetEnableCombo(comboBox1, false);
            SetEnableCombo(comboBox2, false);
            panel1.Enabled = false;
            SetEnableCombo(comboBox3, false);
            //pictureBox2.BackColor = Color.Black;
            //pictureBox3.BackColor = Color.Black;
        }
        private void ClearEditorBlock()
        {
            textBox3.Text = "";
            textBox2.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            button6.Text = "None";
            button6.Visible = false;
            comboBox4.SelectedIndex = -1;
            comboBox5.SelectedIndex = -1;
            //comboBox7.Enabled = false;
            SetEnableCombo(comboBox7, false);
            comboBox7.SelectedIndex = -1;
            button4.Enabled = false;
            //comboBox4.Enabled = false;
            //comboBox5.Enabled = false;
            SetEnableCombo(comboBox4, false);
            SetEnableCombo(comboBox5, false);
            pictureBox4.Enabled = false;
            textBox2.Enabled = false;
            textBox9.Enabled = false;
            textBox3.Enabled = false;
            textBox5.Enabled = false;
            textBox6.Enabled = false;
            textBox7.Enabled = false;
            textBox8.Enabled = false;
            //pictureBox4.BackColor = Color.Black;
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
        }

        List<int> SelectedSaveVar = new List<int>();

        void UpdateBlocks()
        {
            SelectedSaveVar.Clear();
            foreach (int Sel in listBox2.SelectedIndices)
            {
                SelectedSaveVar.Add(Sel);
            }
            UpdateBlocksNoSett();
            listBox2.BeginUpdate();
            foreach (int Sel in SelectedSaveVar)
            {
                listBox2.SetSelected(Sel, true);
            }
            listBox2.EndUpdate();
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
            else if (File.Exists(BluePathc + "\\bp.sbcB1")) File.Delete(BluePathc + "\\bp.sbcB1");
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Block.Clear();
            ClearEditorBlock(); string SubtypeName = ""; string BuiltBy = ""; int Heavy = 0, Light = 0;
            string MinX = ""; string MinY = ""; string MinZ = ""; bool IsArmor = true; string Color = "";
            string CustomName = ""; bool FirsTrart = true; string OrForw = ""; string OrUp = "";
            int ColorCount = 0; int CustomnameCount = 0; int CountOrient = 0; int CountMin = 0;
            int CountBuilt = 0; int CountSubt = 0;
            foreach (int Index in listBox2.SelectedIndices)
            {
                Block.Add(BlocksSorted[Sorter[Index]]);
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
                        button6.Text = "EditProgram";
                        EXTData = Child.InnerText;
                        //button6.Visible = true;Future
                    }
                }
                FirsTrart = false;
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
        string EXTData;
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
            public static Color ColorFromHSV(double H, double S, double V)
            {
                double r = 0, g = 0, b = 0;

                if (S == 0)
                {
                    r = V;
                    g = V;
                    b = V;
                }
                else
                {
                    int i;
                    double f, p, q, t;

                    if (H == 360)
                        H = 0;
                    else
                        H = H / 60;

                    i = (int)Math.Truncate(H);
                    f = H - i;

                    p = V * (1.0 - S);
                    q = V * (1.0 - (S * f));
                    t = V * (1.0 - (S * (1.0 - f)));

                    switch (i)
                    {
                        case 0:
                            r = V;
                            g = t;
                            b = p;
                            break;

                        case 1:
                            r = q;
                            g = V;
                            b = p;
                            break;

                        case 2:
                            r = p;
                            g = V;
                            b = t;
                            break;

                        case 3:
                            r = p;
                            g = q;
                            b = V;
                            break;

                        case 4:
                            r = t;
                            g = p;
                            b = V;
                            break;

                        default:
                            r = V;
                            g = p;
                            b = q;
                            break;
                    }

                }

                return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
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

        private void button3_Click(object sender, EventArgs e)
        {
            ArhApi.CompliteAsync(() =>
            {
                if (Calculator == null || Calculator.IsDisposed)
                {
                    Invoke(new Action(() => {
                        label22.Text = "Loading Data... Please Wait";
                    }));
                    Calculator = new Form4(GamePath);
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
                    Calculator.ShowBlocks();
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
            if (Calculator != null && !Calculator.IsDisposed)
                Calculator.SetColor(AllForeColor, AllBackColor);
        }

        /*Future
        Dictionary<string, Form> Forms = new Dictionary<string, Form>();
        void FormsLoad()
        {
            //Forms.Add("EditProgram",new EXTS.Form1());
        }*/
        private void button6_Click(object sender, EventArgs e)
        {
            /*switch (button6.Text) {
                case "EditProgram":
                    Form Frome = Forms[button6.Text];
                    Frome.Hide();
                    EXTS.Form1 FormAs = Frome as EXTS.Form1;
                    FormAs.Data(EXTData);
                    Frome.Show();
                    break;
            }*/
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
