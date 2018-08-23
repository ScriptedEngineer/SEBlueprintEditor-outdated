using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace BlueprintEditor
{
    public partial class Form5 : Form
    {
        Form1 MainForm;
        public Form5(Form1 Parrent)
        {
            InitializeComponent();
            MainForm = Parrent;
            AspectR = (double)Width / (double)Height;
            MinHeight = Height;
        }
        int MinHeight;

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

        public void ImageAndRadio(Image Img,bool Wide,string Pic)
        {
            picture = Pic;
            pictureBox1.Image = Img;
            if (Wide) radioButton2.Checked = true; else radioButton1.Checked = true;
            NormalizeForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try {
            openFileDialog1.ShowDialog();
            }
            catch (Exception ex)
            {
               MainForm.Error(ex);
            }
        }

        string picture = "";

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            pictureBox1.Image = new Bitmap(Image.FromFile(openFileDialog1.FileName), radioButton2.Checked ? 356 : 178, 178);
            NormalizeForm();
        }

        void NormalizeForm()
        {
            
            Size PicSize;
            double PicAspect = (double)pictureBox1.Image.Width / pictureBox1.Image.Height;
            double BoxAspect = (double)pictureBox1.Width / pictureBox1.Height;
            if (10 * BoxAspect > 10 * PicAspect)
            {
                PicSize = new Size((int)(pictureBox1.Height * PicAspect), pictureBox1.Height);
            }
            else
            {
                PicSize = new Size(pictureBox1.Width, (int)(pictureBox1.Width / PicAspect));
            }
            Size ChangeVec = pictureBox1.Size - PicSize;
            if (MinimumSize.Height * 1.4 > Size.Height)
            {
                Size += new Size((int)(ChangeVec.Height * PicAspect), (int)(ChangeVec.Width / PicAspect));
            }
            else
            {
                Size -= ChangeVec;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try {
            string text = "";
            Bitmap bm = new Bitmap(pictureBox1.Image ,radioButton2.Checked ? 356 : 178, 178);
            for (int i = 0; i < bm.Height; i++)
            {
                for (int j = 0; j < bm.Width; j++)
                {
                    Color pixel = bm.GetPixel(j, i);
                    int num = pixel.R >> 5;
                    int num2 = pixel.G >> 5;
                    int num3 = pixel.B >> 5;
                    int num4 = pixel.R & 0x1F;
                    int num5 = pixel.G & 0x1F;
                    int num6 = pixel.B & 0x1F;
                    int red = num << 5;
                    int green = num2 << 5;
                    int blue = num3 << 5;
                    text += ((char)(ushort)(57600 + (num << 6) + (num2 << 3) + num3)).ToString();
                    bm.SetPixel(j, i, Color.FromArgb(red, green, blue));
                    if (checkBox1.Checked)
                    {
                        AddPixelRGB(ref bm, j + 1, i, num4 * 7 >> 4, num5 * 7 >> 4, num6 * 7 >> 4);
                        AddPixelRGB(ref bm, j - 1, i + 1, num4 * 3 >> 4, num5 * 3 >> 4, num6 * 3 >> 4);
                        AddPixelRGB(ref bm, j, i + 1, num4 * 5 >> 4, num5 * 5 >> 4, num6 * 5 >> 4);
                        AddPixelRGB(ref bm, j + 1, i + 1, num4 >> 4, num5 >> 4, num6 >> 4);
                    }
                }
                text += "\n";
            }
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image.Dispose();
            }
            picture = text;
            pictureBox1.Image = bm.Clone(new Rectangle(0, 0, bm.Width, bm.Height), PixelFormat.Undefined);
            bm.Dispose();
            NormalizeForm();
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }

        private void AddPixelRGB(ref Bitmap bm, int x, int y, int R, int G, int B)
        {
            if ((x > 0) & (x < bm.Width) & (y > 0) & (y < bm.Height))
            {
                Color pixel = bm.GetPixel(x, y);
                R = Math.Min(255, pixel.R + R);
                G = Math.Min(255, pixel.G + G);
                B = Math.Min(255, pixel.B + B);
                bm.SetPixel(x, y, Color.FromArgb(R, G, B));
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //ChangeType = 3;
            //if (radioButton1.Checked)Width -= pictureBox1.Width/2;
           // MinimumSize = new Size((int)((double)MinHeight*AspectR), MinHeight);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //ChangeType = 3;
            //if (radioButton2.Checked) Width += pictureBox1.Width;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try { 
            MainForm.WritePic(picture);
            Close();
            }
            catch (Exception ex)
            {
                MainForm.Error(ex);
            }
        }
        double AspectR;int OldW, OldH,ChangeType;

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Form5_ResizeEnd(object sender, EventArgs e)
        {
            if (ChangeType != 3)
            {
                NormalizeForm();
            }
            else
            {
                ChangeType = 0;
            }
        }

        private void Form5_Resize(object sender, EventArgs e)
        {
        }
    }
}
