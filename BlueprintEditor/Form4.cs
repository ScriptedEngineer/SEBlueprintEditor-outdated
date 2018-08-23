using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Xml.Linq;
using System.Xml;
using System.Reflection;
using System.Globalization;
using System.Threading;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO.Compression;

namespace BlueprintEditor
{
    public partial class Form4 : Form
    {
        XmlDocument CubeBlocks;
        XmlDocument Blueprints;

        string PrintName;

        double ReginaryMultiplier = 0;
        double RefineryEfficensy = 0;

        public void Test()
        {
            double Node = ShipComponents["Zero"];
        }

        class Item
        {
            string Name;
            float Amount;
        }
        Dictionary<string, double> ShipComponents = new Dictionary<string, double>();
        Dictionary<string, double> ShipIngots = new Dictionary<string, double>();
        Dictionary<string, double> ShipOres = new Dictionary<string, double>();

        Dictionary<string, Dictionary<string, double>> DictComponents = new Dictionary<string, Dictionary<string, double>>();
        Dictionary<string, Dictionary<string, double>> DictBlueprint = new Dictionary<string, Dictionary<string, double>>();

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

        string UndefinedBlocks = "";
        Form1 MainForm;
        public Form4(string GamePatch,Form1 MainF)
        {
            try {
            InitializeComponent();
            string str = GamePatch + "\\Content\\Data\\";
            MainForm = MainF;
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(str + "CubeBlocks.sbc");
            CubeBlocks = xmlDocument;
            XmlDocument xmlDocument3 = new XmlDocument();
            xmlDocument3.Load(str + "Blueprints.sbc");
            Blueprints = xmlDocument3;
            foreach (XmlNode Def in CubeBlocks.GetElementsByTagName("Definition"))
            {
                Dictionary<string, double> Componentse = new Dictionary<string, double>();
                //XmlNode xsitype = Def.Attributes.GetNamedItem("xsi:type");
                string TypeID = "", SybtypeID = "";
                foreach (XmlNode Ndex in Def.ChildNodes)
                {
                    if (Ndex.Name == "Id")
                    {
                        foreach (XmlNode nNdex in Ndex.ChildNodes)
                        {
                            if (nNdex.Name == "TypeId")
                            {
                                TypeID = nNdex.InnerText;
                            }
                            else if (nNdex.Name == "SubtypeId")
                            {
                                SybtypeID = nNdex.InnerText;
                            }
                        }
                        break;
                    }
                }
                string Type = TypeID + "/" + SybtypeID;
                foreach (XmlNode Node in Def.ChildNodes)
                {
                    if (Node.Name == "Components")
                    {
                        foreach (XmlNode Component in Node.ChildNodes)
                        {
                            if (Component.Attributes != null)
                            {
                                string Count = Component.Attributes.GetNamedItem("Count").Value.Replace('.', ',');
                                string Subtype = Component.Attributes.GetNamedItem("Subtype").Value;
                                if (Componentse.ContainsKey(Subtype))
                                    Componentse[Subtype] += double.Parse(Count);
                                else
                                    Componentse.Add(Subtype, double.Parse(Count));
                            }
                        }
                        if (Type != "Refinery/LargeRefinery" && Type != "UpgradeModule/LargeEffectivenessModule") break;
                    }
                    if (Type == "Refinery/LargeRefinery" && Node.Name == "MaterialEfficiency")
                    {
                        RefineryEfficensy = double.Parse(Node.InnerText.Replace('.', ','));
                    }
                    if (Type == "UpgradeModule/LargeEffectivenessModule" && Node.Name == "Upgrades")
                    {
                        ReginaryMultiplier = double.Parse(Node.FirstChild.ChildNodes[1].InnerText.Replace('.', ','));
                    }
                }
                DictComponents.Add(Type, Componentse);
            }
            foreach (XmlNode Blue in Blueprints.GetElementsByTagName("Blueprint"))
            {
                Dictionary<string, double> Componentse = new Dictionary<string, double>();
                double ToOne = 0; string ResyltID = "";
                foreach (XmlNode Node in Blue.ChildNodes)
                {
                    if (Node.Name == "Result")
                    {
                        if (Node.Attributes != null)
                        {
                            if (double.TryParse(Node.Attributes.GetNamedItem("Amount").Value.Replace('.', ','), out ToOne))
                            {
                                ToOne = 1 / ToOne;
                                ResyltID = Node.Attributes.GetNamedItem("TypeId").Value + "/" + Node.Attributes.GetNamedItem("SubtypeId").Value;
                            }
                        }
                        break;
                    }
                }
                if (ResyltID != "")
                {
                    foreach (XmlNode Node in Blue.ChildNodes)
                    {
                        if (Node.Name == "Prerequisites")
                        {
                            foreach (XmlNode Component in Node.ChildNodes)
                            {
                                if (Component.Attributes != null)
                                {
                                    string ID = Component.Attributes.GetNamedItem("TypeId").Value + "/" + Component.Attributes.GetNamedItem("SubtypeId").Value;
                                    if (Componentse.ContainsKey(ID))
                                        Componentse[ID] += double.Parse(Component.Attributes.GetNamedItem("Amount").Value.Replace('.', ',')) * ToOne;
                                    else
                                        Componentse.Add(ID, double.Parse(Component.Attributes.GetNamedItem("Amount").Value.Replace('.', ',')) * ToOne);
                                }
                            }
                            break;
                        }
                    }
                    if (!DictBlueprint.ContainsKey(ResyltID)) DictBlueprint.Add(ResyltID, Componentse);
                }
            }
            string ModFolder = "C:\\Users\\" + Environment.UserName + "\\AppData\\Roaming\\SpaceEngineers\\Mods";
            if (Directory.Exists(ModFolder))
            {
                string[] ModsFiles = Directory.GetFiles(ModFolder);
                foreach (string ModsFile in ModsFiles)
                {
                    string SbmPatch = Path.GetFileNameWithoutExtension(ModsFile);
                    if (Path.GetExtension(ModsFile) == ".sbm")
                        try
                        {
                            using (ZipArchive zipMode = ZipFile.OpenRead(ModsFile))
                            {
                                //if(!Directory.Exists("Mods"))Directory.CreateDirectory("Mods");
                                //if(!Directory.Exists("Mods\\" + SbmPatch)) Directory.CreateDirectory("Mods\\"+ SbmPatch);
                                foreach (ZipArchiveEntry ModFile in zipMode.Entries)
                                {
                                    try
                                    {
                                        if (Path.GetExtension(ModFile.Name) == ".sbc")
                                        {
                                            //ModFile.ExtractToFile("Mods\\" + SbmPatch + "\\" + ModFile.Name, true);
                                            using (Stream XmlFile = ModFile.Open())
                                            {
                                                XmlDocument ModDocument = new XmlDocument();
                                                ModDocument.Load(XmlFile);
                                                foreach (XmlNode Def in ModDocument.GetElementsByTagName("Definition"))
                                                {
                                                    try
                                                    {
                                                        string TypeID = "", SybtypeID = "";
                                                        foreach (XmlNode Ndex in Def.ChildNodes)
                                                        {
                                                            if (Ndex.Name == "Id")
                                                            {
                                                                foreach (XmlNode nNdex in Ndex.ChildNodes)
                                                                {
                                                                    if (nNdex.Name == "TypeId")
                                                                    {
                                                                        TypeID = nNdex.InnerText;
                                                                    }
                                                                    else if (nNdex.Name == "SubtypeId")
                                                                    {
                                                                        SybtypeID = nNdex.InnerText;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        }
                                                        string Type = TypeID + "/" + SybtypeID;
                                                        if (!DictComponents.ContainsKey(Type))
                                                        {
                                                            Dictionary<string, double> Componentse = new Dictionary<string, double>();
                                                            foreach (XmlNode Node in Def.ChildNodes)
                                                            {
                                                                if (Node.Name == "Components")
                                                                {
                                                                    foreach (XmlNode Component in Node.ChildNodes)
                                                                    {
                                                                        if (Component.Attributes != null)
                                                                        {
                                                                            string Count = Component.Attributes.GetNamedItem("Count").Value.Replace('.', ',');
                                                                            string Subtype = Component.Attributes.GetNamedItem("Subtype").Value;
                                                                            if (Componentse.ContainsKey(Subtype))
                                                                                Componentse[Subtype] += double.Parse(Count);
                                                                            else
                                                                                Componentse.Add(Subtype, double.Parse(Count));
                                                                        }
                                                                    }
                                                                    if (Type != "Refinery/LargeRefinery" && Type != "UpgradeModule/LargeEffectivenessModule") break;
                                                                }
                                                            }
                                                            DictComponents.Add(Type, Componentse);
                                                        }
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                                foreach (XmlNode Blue in ModDocument.GetElementsByTagName("Blueprint"))
                                                {
                                                    try
                                                    {
                                                        Dictionary<string, double> Componentse = new Dictionary<string, double>();
                                                        double ToOne = 0; string ResyltID = "";
                                                        foreach (XmlNode Node in Blue.ChildNodes)
                                                        {
                                                            if (Node.Name == "Result")
                                                            {
                                                                if (Node.Attributes != null)
                                                                {
                                                                    if (double.TryParse(Node.Attributes.GetNamedItem("Amount").Value.Replace('.', ','), out ToOne))
                                                                    {
                                                                        ToOne = 1 / ToOne;
                                                                        ResyltID = Node.Attributes.GetNamedItem("TypeId").Value + "/" + Node.Attributes.GetNamedItem("SubtypeId").Value;
                                                                    }
                                                                }
                                                                break;
                                                            }
                                                        }
                                                        if (ResyltID != "" && !DictBlueprint.ContainsKey(ResyltID))
                                                        {
                                                            foreach (XmlNode Node in Blue.ChildNodes)
                                                            {
                                                                if (Node.Name == "Prerequisites")
                                                                {
                                                                    foreach (XmlNode Component in Node.ChildNodes)
                                                                    {
                                                                        if (Component.Attributes != null)
                                                                        {
                                                                            string ID = Component.Attributes.GetNamedItem("TypeId").Value + "/" + Component.Attributes.GetNamedItem("SubtypeId").Value;
                                                                            if (Componentse.ContainsKey(ID))
                                                                                Componentse[ID] += double.Parse(Component.Attributes.GetNamedItem("Amount").Value.Replace('.', ',')) * ToOne;
                                                                            else
                                                                                Componentse.Add(ID, double.Parse(Component.Attributes.GetNamedItem("Amount").Value.Replace('.', ',')) * ToOne);
                                                                        }
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                            DictBlueprint.Add(ResyltID, Componentse);
                                                        }
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }
                                                //File.Delete("tmpFiles\\" + ModFile.Name);
                                            }
                                        }
                                    }
                                    catch
                                    {

                                    }
                                }
                            }
                        }
                        catch
                        {

                        }
                    //GC.Collect();
                }
                if (Directory.Exists("Mods")) ArhApi.DeleteFolder("Mods");
                GC.Collect();
            }
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
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

        Dictionary<string, double> GetCopmonents(string TypeOfBlock)
        {
            if (DictComponents.ContainsKey(TypeOfBlock)) return DictComponents[TypeOfBlock];
            else
            {
                if (!UndefinedBlocks.Contains(TypeOfBlock + "\r\n")) UndefinedBlocks += TypeOfBlock + "\r\n";
                return new Dictionary<string, double>();
            }
        }
        Dictionary<string, double> GetIngots(string Component)
        {
            string TypeOfComp = "Component/" + Component;
            if (DictBlueprint.ContainsKey(TypeOfComp)) return DictBlueprint[TypeOfComp];
            else
            {
                return new Dictionary<string, double>();
            }
        }
        Dictionary<string, double> GetOres(string Ore)
        {
            string TypeOfComp = "Ingot/" + Ore;
            if (DictBlueprint.ContainsKey(TypeOfComp)) return DictBlueprint[TypeOfComp];
            else
            {
                return new Dictionary<string, double>();
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {

        }

        public void ClearBlocks()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            UndefinedBlocks = "";
            ShipComponents.Clear();
            ShipIngots.Clear();
            ShipOres.Clear();
        }

        public void ShowBlocks(string BlueName)
        {
            string Faster3 = ""; PrintName = BlueName;
            foreach (string Key in ShipComponents.Keys)
            {
                double Amount = ShipComponents[Key];
                Faster3 += Key + ": " + (Amount).ToString() + "\r\n";
            }
            string Faster1 = "", Faster2 = "";
            double Pos = (double)trackBar1.Value; ShipOres.Clear();
            double Efficinsy = RefineryEfficensy * Math.Pow(ReginaryMultiplier, (double)trackBar2.Value * 2);
            foreach (string Key in ShipIngots.Keys)
            {
                double Amount = ShipIngots[Key] / Pos;
                Faster1 += Key.Replace("Ingot/", "") + ": " + AddCounters(Amount) + "\r\n";
                Dictionary<string, double> Ingotz = GetOres(Key.Replace("Ingot/", ""));
                foreach (string Key2 in Ingotz.Keys)
                {
                    if (ShipOres.ContainsKey(Key2))
                    {
                        ShipOres[Key2] += Math.Max((Ingotz[Key2] * Amount) / Efficinsy, Amount);
                    }
                    else
                    {
                        ShipOres.Add(Key2, Math.Max((Ingotz[Key2] * Amount) / Efficinsy, Amount));
                    }
                }
            }
            foreach (string Key in ShipOres.Keys)
            {
                Faster2 += Key.Replace("Ore/", "") + ": " + AddCounters(ShipOres[Key]) + "\r\n";
            }
            textBox4.Text = Faster2;
            textBox3.Text = Faster3;
            textBox2.Text = UndefinedBlocks;
            textBox1.Text = Faster1;
        }

        public void AddBlock(string Types)
        {
            Dictionary<string, double> Componentz = GetCopmonents(Types);
            foreach (string Key in Componentz.Keys)
            {
                if (ShipComponents.ContainsKey(Key))
                    ShipComponents[Key] += Componentz[Key];
                else
                    ShipComponents.Add(Key, Componentz[Key]);
            }
            foreach (string Key in ShipComponents.Keys)
            {
                double Amount = ShipComponents[Key];
                Dictionary<string, double> Ingotz = GetIngots(Key);
                foreach (string Key2 in Ingotz.Keys)
                {
                    if (ShipIngots.ContainsKey(Key2))
                    {
                        ShipIngots[Key2] += Ingotz[Key2] * Amount;
                    }
                    else
                    {
                        ShipIngots.Add(Key2, Ingotz[Key2] * Amount);
                    }
                }
            }

        }

        private void Form4_FormClosing(object sender, FormClosingEventArgs e)
        {
            try{ 
            ClearBlocks();
            e.Cancel = true;
            Hide();
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            try {
            string Faster1 = "", Faster2 = "";
            double Pos = (double)trackBar1.Value; ShipOres.Clear();
            double Efficinsy = RefineryEfficensy * Math.Pow(ReginaryMultiplier, (double)trackBar2.Value * 2);
            foreach (string Key in ShipIngots.Keys)
            {
                double Amount = ShipIngots[Key] / Pos;
                Faster1 += Key.Replace("Ingot/", "") + ": " + AddCounters(Amount) + "\r\n";
                Dictionary<string, double> Ingotz = GetOres(Key.Replace("Ingot/", ""));
                foreach (string Key2 in Ingotz.Keys)
                {
                    if (ShipOres.ContainsKey(Key2))
                    {
                        ShipOres[Key2] += Math.Max((Ingotz[Key2] * Amount) / Efficinsy, Amount);
                    }
                    else
                    {
                        ShipOres.Add(Key2, Math.Max((Ingotz[Key2] * Amount) / Efficinsy, Amount));
                    }
                }
            }
            foreach (string Key in ShipOres.Keys)
            {
                Faster2 += Key.Replace("Ore/", "") + ": " + AddCounters(ShipOres[Key]) + "\r\n";
            }
            textBox4.Text = Faster2;
            textBox1.Text = Faster1;
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            string textBox4Text = "";
            double Pos = (double)trackBar1.Value;
            double Efficinsy = RefineryEfficensy * Math.Pow(ReginaryMultiplier, (double)trackBar2.Value * 2);
            ShipOres.Clear();
            foreach (string Key in ShipIngots.Keys)
            {
                double Amount = ShipIngots[Key] / Pos;
                Dictionary<string, double> Ingotz = GetOres(Key.Replace("Ingot/", ""));
                foreach (string Key2 in Ingotz.Keys)
                {
                    if (ShipOres.ContainsKey(Key2))
                    {
                        ShipOres[Key2] += Math.Max((Ingotz[Key2] * Amount) / Efficinsy, Amount);
                    }
                    else
                    {
                        ShipOres.Add(Key2, Math.Max((Ingotz[Key2] * Amount) / Efficinsy, Amount));
                    }
                }
            }
            foreach (string Key in ShipOres.Keys)
            {
                textBox4Text += Key.Replace("Ore/", "") + ": " + AddCounters(ShipOres[Key]) + "\r\n";
            }
            textBox4.Text = textBox4Text;
        }

        string AddCounters(double Num)
        {
            string Oute = Num.ToString("0.00") + " Kg";
            if (Num > 10000000000) Oute = (Num / 1000000000).ToString("0.00") + " MT";
            else if (Num > 10000000) Oute = (Num / 1000000).ToString("0.00") + " KT";
            else if (Num > 10000) Oute = (Num / 1000).ToString("0.00") + " T";
            else if (Num < 0.1) Oute = (Num * 1000).ToString("0.00") + " g";
            return Oute;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try {
            textBox2.Text = UndefinedBlocks;
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try {
            string Faster2 = "";
            double Pos = (double)trackBar1.Value;
            foreach (string Key in ShipIngots.Keys)
            {
                double Amount = ShipIngots[Key] / Pos;
                Faster2 += Key.Replace("Ingot/", "") + ": " + AddCounters(Amount) + "\r\n";
            }
            textBox1.Text = Faster2;
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try {
            string Faster3 = "";
            foreach (string Key in ShipComponents.Keys)
            {
                double Amount = ShipComponents[Key];
                Faster3 += Key + ": " + (Amount).ToString() + "\r\n";
            }
            textBox3.Text = Faster3;
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            try {
            string Faster2 = "";
            foreach (string Key in ShipOres.Keys)
            {
                Faster2 += Key.Replace("Ore/", "") + ": " + AddCounters(ShipOres[Key]) + "\r\n";
            }
            textBox4.Text = Faster2;
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.FileName = "Resources for " + PrintName;
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string Data = "#This file created by SE BlueprintEditor#\r\n" +
                                    "#Resources need to build \"" + PrintName + "\"#\r\n" +
                                    "#Assembler Efficiency: " + trackBar1.Value.ToString() + "x #\r\n" +
                                    "#Refinary Yield Mods: " + trackBar2.Value.ToString() + " #\r\n\r\n" +
                                    "#List of undefined block types#\r\n" + textBox2.Text + "#End list of undefined block types#\r\n\r\n" +
                                    "#List of components#\r\n" + textBox3.Text + "#End list of components#\r\n\r\n" +
                                    "#List of ingots#\r\n" + textBox1.Text + "#End list of ingots#\r\n\r\n" +
                                    "#List of ores#\r\n" + textBox4.Text + "#End list of ores#\r\n";
                    File.WriteAllText(saveFileDialog1.FileName, Data);
                }
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try { 
            bool TrueS = false;int Count = 0;
            DialogResult DialogResulte = MessageBox.Show("Put Components?(Components or (ingots or ores))", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            string CompData = "";
            if (DialogResulte == DialogResult.Yes)
            {
                foreach (string Key in ShipComponents.Keys)
                {
                    CompData += "<MyObjectBuilder_InventoryItem><Amount>"+ShipComponents[Key].ToString()+ "</Amount><PhysicalContent xsi:type=\"MyObjectBuilder_Component\"><SubtypeName>" + Key+"</SubtypeName></PhysicalContent><ItemId>"+ Count + "</ItemId></MyObjectBuilder_InventoryItem>";
                    Count++;
                }
                TrueS = true;
            }
            else if(DialogResulte != DialogResult.Cancel)
            {
                DialogResulte = MessageBox.Show("Put Ingots?(Ingots or ores)", "Question", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (DialogResulte == DialogResult.Yes)
                {
                    foreach (string Key in ShipIngots.Keys)
                    {
                        string[] Typez = Key.Split('/');
                        CompData += "<MyObjectBuilder_InventoryItem><Amount>" + ShipIngots[Key].ToString("0.000000").Replace(',', '.') + "</Amount><PhysicalContent xsi:type=\"MyObjectBuilder_" + Typez[0] + "\"><SubtypeName>" + Typez[1] + "</SubtypeName></PhysicalContent><ItemId>" + Count + "</ItemId></MyObjectBuilder_InventoryItem>";
                        Count++;
                    }
                    TrueS = true;
                }
                else if (DialogResulte != DialogResult.Cancel&&MessageBox.Show("Putting ores!Ok?", "Question", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    foreach (string Key in ShipOres.Keys)
                    {
                        string[] Typez = Key.Split('/');
                        CompData += "<MyObjectBuilder_InventoryItem><Amount>" + (ShipOres[Key]).ToString("0.000000").Replace(',','.') + "</Amount><PhysicalContent xsi:type=\"MyObjectBuilder_" + Typez[0] + "\"><SubtypeName>" + Typez[1] + "</SubtypeName></PhysicalContent><ItemId>" + Count + "</ItemId></MyObjectBuilder_InventoryItem>";
                        Count++;
                    }
                    TrueS = true;
                }
            }
            if (TrueS)
            {
                string ToFileST = "<?xml version=\"1.0\"?><Definitions xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><ShipBlueprints><ShipBlueprint xsi:type=\"MyObjectBuilder_ShipBlueprintDefinition\"><Id Type=\"MyObjectBuilder_ShipBlueprintDefinition\" Subtype=\"OwerLoadedCargo\" /><DisplayName>SE_BlueprintEditor</DisplayName><CubeGrids><CubeGrid><SubtypeName /><EntityId>118739124154775050</EntityId><PersistentFlags>CastShadows InScene</PersistentFlags><PositionAndOrientation><Position x=\"148226.6011545636\" y=\"209903.54307619989\" z=\"169702.54195813951\" /><Forward x=\"0.595039845\" y=\"-0.0242931172\" z=\"0.8033289\" /><Up x=\"-0.390973866\" y=\"-0.8820478\" z=\"0.262927562\" /><Orientation><X>0.9202668</X><Y>-0.23403728</Y><Z>-0.306810915</Z><W>0.06482753</W></Orientation></PositionAndOrientation><GridSizeEnum>Large</GridSizeEnum><CubeBlocks><MyObjectBuilder_CubeBlock xsi:type=\"MyObjectBuilder_CargoContainer\"><SubtypeName>LargeBlockLargeContainer</SubtypeName><EntityId>118739124154775051</EntityId><Min x=\"-1\" y=\"-1\" z=\"-1\" /><ColorMaskHSV x=\"0.108333334\" y=\"-0.04\" z=\"0.43\" /><Owner>0</Owner><BuiltBy>0</BuiltBy><ComponentContainer><Components><ComponentData><TypeId>MyInventoryBase</TypeId><Component xsi:type=\"MyObjectBuilder_Inventory\"><Items>";
                string ToFileND = "</Items><nextItemId>"+ Count.ToString() + "</nextItemId><Volume>10000</Volume><Mass>10</Mass><MaxItemCount>2147483647</MaxItemCount><Size xsi:nil=\"true\" /><InventoryFlags>CanReceive CanSend</InventoryFlags><RemoveEntityOnEmpty>false</RemoveEntityOnEmpty></Component></ComponentData></Components></ComponentContainer><CustomName>Auto_Container_SEBE</CustomName><ShowOnHUD>false</ShowOnHUD><ShowInTerminal>true</ShowInTerminal><ShowInToolbarConfig>true</ShowInToolbarConfig><ShowInInventory>true</ShowInInventory></MyObjectBuilder_CubeBlock></CubeBlocks><DisplayName>Auto_Container_SEBE</DisplayName><DestructibleBlocks>false</DestructibleBlocks><IsRespawnGrid>false</IsRespawnGrid><LocalCoordSys>0</LocalCoordSys><TargetingTargets /></CubeGrid></CubeGrids><WorkshopId>0</WorkshopId><OwnerSteamId>0</OwnerSteamId><Points>0</Points></ShipBlueprint></ShipBlueprints></Definitions>";
                string File = ToFileST + CompData + ToFileND;
                MainForm.CreateBlueprint("Auto_Container_SEBE", File);
            }
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }
    }
}
