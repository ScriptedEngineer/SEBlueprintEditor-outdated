using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace BlueprintEditor
{
    static class Program
    {
        public static Form1 GlobalMainForm;
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[1])
                {

                }
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            GlobalMainForm = new Form1(args);
            Application.Run(GlobalMainForm);
            //Application.Run(new EXTS.Form1());
        }
    }


    public class MyColor
    {
        public int R, G, B;
        public MyColor()
        {

        }
        public MyColor(System.Drawing.Color Color)
        {
            R = Color.R;
            G = Color.G;
            B = Color.B;
        }
        public System.Drawing.Color GetColor()
        {
            return System.Drawing.Color.FromArgb(R,G,B);
        }

    }
    public class Settings
    {
        public string BlueprintPath;
        public string GamePath;
        public int LangID;
        public int Theme;
        public string EditorProgram;
        public MyColor ForeColor;
        public MyColor BackColor;
        public Settings()
        {

        }
    }
    static class ArhApi
    {
        public const string ApplicationName = "SEBlueprintEditor";
        public const string ApplicationID = "SEbe";
        static public void LoadForm(Form ToLoad, Form ToClose = null, bool Close = false)
        {
            ToLoad.Show();
            if (ToClose != null)
            {
                if (Close) ToClose.Close();
                else ToClose.Hide();
            }
        }
        static public void ListBoxFill(string[] Elements, ComboBox ThisComboBox, bool Append = false, string NoElem = "NoElements")
        {
            ThisComboBox.BeginUpdate();
            if (!Append) ThisComboBox.Items.Clear();
            if (Elements.Length > 0) ThisComboBox.Text = Elements[0];
            else ThisComboBox.Text = NoElem;
            foreach (string Element in Elements)
            {
                ThisComboBox.Items.Add(Element.Split('|')[0]);
            }
            ThisComboBox.EndUpdate();
        }
        static public void ListBoxFill(string[] Elements, ListBox ThisComboBox, bool Append = false, string NoElem = "NoElements")
        {
            ThisComboBox.BeginUpdate();
            ListBox.ObjectCollection Collect;
            if (!Append) ThisComboBox.Items.Clear();
            if (Elements.Length > 0) ThisComboBox.Text = Elements[0];
            else ThisComboBox.Text = NoElem;
            foreach (string Element in Elements)
            {
                ThisComboBox.Items.Add(Element.Split('|')[0]);
            }
            ThisComboBox.EndUpdate();
        }
        static public void ListBoxFill(string[] Elements, ListBox ThisComboBox, int DigitLenght, bool Append = false, string NoElem = "NoElements")
        {
            ThisComboBox.BeginUpdate();
            if (!Append) ThisComboBox.Items.Clear();
            if (Elements.Length > 0) ThisComboBox.Text = Elements[0];
            else ThisComboBox.Text = NoElem;
            foreach (string Element in Elements)
            {
                string[] ElementAr = Element.Split('|');
                ThisComboBox.Items.Add((ElementAr.Length > 1 ? ElementAr[1].PadLeft(DigitLenght, '0') + "." : "") + ElementAr[0]);
            }
            ThisComboBox.EndUpdate();
        }
        static public string[] ArrayStringFileName(string[] Paths, bool NoExt = false)
        {
            List<string> Out = new List<string>();
            foreach (string Pathe in Paths)
            {
                if (NoExt) Out.Add(System.IO.Path.GetFileNameWithoutExtension(Pathe));
                else Out.Add(System.IO.Path.GetFileName(Pathe));
            }
            return Out.ToArray();
        }
        static public bool IsLink(string link)
        {
            return (link.Contains("http://") || link.Contains("https://")) && link.Contains('.');
        }
        static public void SendMail(string Recipient, string Subject, string Body)
        {
            System.Net.Http.HttpClient Http = new System.Net.Http.HttpClient();
            using (var client = new System.Net.WebClient())
            {
                var values = new System.Collections.Specialized.NameValueCollection();
                values["app"] = ApplicationID;
                values["act"] = "sendmail";
                values["ver"] = "v" + Application.ProductVersion;
                values["lang"] = "0";
                values["pc"] = "";
                values["key"] = "";
                values["dat"] = ApplicationName+"|"+ Recipient+"|"+ Subject+"|"+ Body;
                var response = client.UploadValues("https://arhsite.tk/ArhApi.php", values);
            }
        }
        static public bool IsEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        static Random rand = new Random();
        static public int Rand(int Min, int Max)
        {
            return rand.Next(Min, Max);
        }
        static public void CompliteAsync(System.Threading.ThreadStart Instruction)
        {
            System.Threading.Thread MultiThreaded = new System.Threading.Thread(Instruction);
            MultiThreaded.Start();
        }
        static public int GetPassStrength(string Pass)
        {
            string[] badPasswords = new string[] { "123456", "123456789", "qwerty", "111111", "1234567", "666666", "12345678", "7777777", "123321", "0", "654321", "1234567890", "123123", "555555", "vkontakte", "gfhjkm", "159753", "777777", "TempPassWord", "qazwsx", "1q2w3e", "1234", "112233", "121212", "qwertyuiop", "qq18ww899", "987654321", "12345", "zxcvbn", "zxcvbnm", "999999", "samsung", "ghbdtn", "1q2w3e4r", "1111111", "123654", "159357", "131313", "qazwsxedc", "123qwe", "222222", "asdfgh", "333333", "9379992", "asdfghjkl", "4815162342", "12344321", "любовь", "88888888", "11111111", "knopka", "пароль", "789456", "qwertyu", "1q2w3e4r5t", "iloveyou", "vfhbyf", "marina", "password", "qweasdzxc", "10203", "987654", "yfnfif", "cjkysirj", "nikita", "888888", "йцукен", "vfrcbv", "k.,jdm", "qwertyuiop[]", "qwe123", "qweasd", "natasha", "123123123", "fylhtq", "q1w2e3", "stalker", "1111111111", "q1w2e3r4", "nastya", "147258369", "147258", "fyfcnfcbz", "1234554321", "1qaz2wsx", "andrey", "111222", "147852", "genius", "sergey", "7654321", "232323", "123789", "fktrcfylh", "spartak", "admin", "test", "123", "azerty", "abc123", "lol123", "easytocrack1", "hello", "saravn", "holysh!t", "1", "Test123", "tundra_cool2", "456", "dragon", "thomas", "killer", "root", "1111", "pass", "master", "aaaaaa", "a", "monkey", "daniel", "asdasd", "e10adc3949ba59abbe56e057f20f883e", "changeme", "computer", "jessica", "letmein", "mirage", "loulou", "lol", "superman", "shadow", "admin123", "secret", "administrator", "sophie", "kikugalanetroot", "doudou", "liverpool", "hallo", "sunshine", "charlie", "parola", "100827092", "/", "michael", "andrew", "password1", "fuckyou", "matrix", "cjmasterinf", "internet", "hallo123", "eminem", "demo", "gewinner", "pokemon", "abcd1234", "guest", "ngockhoa", "martin", "sandra", "asdf", "hejsan", "george", "qweqwe", "lollipop", "lovers", "q1q1q1", "tecktonik", "naruto", "12", "password12", "password123", "password1234", "password12345", "password123456", "password1234567", "password12345678", "password123456789", "000000", "maximius", "123abc", "baseball1", "football1", "soccer", "princess", "slipknot", "11111", "nokia", "super", "star", "666999", "12341234", "1234321", "135790", "159951", "212121", "zzzzzz", "121314", "134679", "142536", "19921992", "753951", "7007", "1111114", "124578", "19951995", "258456", "qwaszx", "zaqwsx", "55555", "77777", "54321", "qwert", "22222", "33333", "99999", "88888", "66666", "iloveu", "пароль" };
            if (badPasswords.Contains<string>(Pass))
            {
                return 0;
            }
            else
            {
                int score = 0;
                score += Pass.Length / 2;
                Dictionary<string, double> patterns = new Dictionary<string, double> { { @"1234567890", 0.1 }, { @"[a-z]", 0.2 }, { @"[ёа-я]", 0.4 }, { @"[A-Z]", 0.2 }, { @"[ЁА-Я]", 0.4 }, { "[!,@#\\$%\\^&\\*?_~=;:'\"<>[]()~`\\\\|/]", 0.6 }, { @"[¶©]", 0.8 } };
                foreach (var pattern in patterns)
                    score += (int)(System.Text.RegularExpressions.Regex.Matches(Pass, pattern.Key).Count * pattern.Value);
                return score;
            }
        }
        static public string GetMd5Hash(string input)
        {
            System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();

            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        static public bool SiteAcess(string url)
        {
            Uri uri = new Uri(url);
            try
            {
                System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
                System.Net.HttpWebResponse httpWebResponse = (System.Net.HttpWebResponse)httpWebRequest.GetResponse();
                if (uri == httpWebRequest.GetResponse().ResponseUri)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
        static public string Server(string Action,string AddtionalJson = "", string OutputType = "string")
        {
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    client.Encoding = Encoding.UTF8;
                    return client.UploadString("https://xyzs.ru/api/" + Action + "/" + OutputType,
                        "{\"token\":\"L6Pv566loZC7JsDzjr83psLMoktWQqDb\",\"app\":\"SEBE\",\"version\":\"" +
                        Application.ProductVersion + "\"" + AddtionalJson + "}");
                }
            }
            catch (Exception e)
            {
                return "No have access to API.";
            }
        }
        static public string IPtoBase64(string IP)
        {
            List<byte> Codes = new List<byte>();
            string[] IPPort = IP.Split(':');
            string[] bytes = IPPort[0].Split('.');
            foreach (string bytest in bytes) Codes.Add(Convert.ToByte(bytest));
            byte[] porta = BitConverter.GetBytes(Convert.ToUInt16(IPPort[1]));
            foreach (byte portab in porta) Codes.Add(Convert.ToByte(portab));
            return Convert.ToBase64String(Codes.ToArray());
        }
        static public string IPFromBase64(string Base64)
        {
            string output = "";
            byte[] BytedIP = Convert.FromBase64String(Base64);
            output += BytedIP[0].ToString() + "." + BytedIP[1].ToString() + "." + BytedIP[2].ToString() + "." + BytedIP[3].ToString();
            output += ":" + BitConverter.ToUInt16(BytedIP, 4);
            return output;
        }
        static public bool SendUdpData(string Address, int Port, byte[] Data, System.Security.Cryptography.ICryptoTransform Encoder = null)
        {
            System.Net.Sockets.UdpClient udp = new System.Net.Sockets.UdpClient();
            if (Encoder != null) Data = Encode(Data, Encoder);
            int sended = udp.Send(Data, Data.Length, Address, Port);
            udp.Close();
            return (sended == Data.Length);
        }
        static public byte[] Decode(byte[] Data, System.Security.Cryptography.ICryptoTransform Decoder)
        {
            byte[] Outer = new byte[] { }; int rdlen = 0;
            if (Data.Length > 0)
            {
                using (System.IO.MemoryStream memStream = new System.IO.MemoryStream())
                {
                    byte[] bin = new byte[128]; int len;
                    System.IO.MemoryStream InputStr = new System.IO.MemoryStream();
                    InputStr.Write(Data, 0, Data.Length);
                    InputStr.Position = 0;
                    System.Security.Cryptography.CryptoStream CryptStream = new System.Security.Cryptography.CryptoStream(memStream, Decoder, System.Security.Cryptography.CryptoStreamMode.Write);
                    while (rdlen < Data.Length)
                    {
                        len = InputStr.Read(bin, 0, 128);
                        CryptStream.Write(bin, 0, len);
                        rdlen = rdlen + len;
                    }
                    CryptStream.FlushFinalBlock();
                    Outer = memStream.ToArray();
                    CryptStream.Close();
                }
            }
            return Outer;
        }
        static public byte[] Encode(byte[] Data, System.Security.Cryptography.ICryptoTransform Encoder)
        {
            byte[] Outer; int rdlen = 0;
            using (System.IO.MemoryStream memStream = new System.IO.MemoryStream())
            {
                byte[] bin = new byte[128]; int len;
                System.IO.MemoryStream InputStr = new System.IO.MemoryStream();
                InputStr.Write(Data, 0, Data.Length);
                InputStr.Position = 0;
                System.Security.Cryptography.CryptoStream CryptStream = new System.Security.Cryptography.CryptoStream(memStream, Encoder, System.Security.Cryptography.CryptoStreamMode.Write);
                while (rdlen < Data.Length)
                {
                    len = InputStr.Read(bin, 0, 128);
                    CryptStream.Write(bin, 0, len);
                    rdlen = rdlen + len;
                }
                CryptStream.FlushFinalBlock();
                Outer = memStream.ToArray();
                CryptStream.Close();
            }
            return Outer;
        }
        static public string SerializeClass<Class>(Class ToSerialize) where Class : class
        {
            System.Xml.Serialization.XmlSerializer Serial = new System.Xml.Serialization.XmlSerializer(typeof(Class));
            using (System.IO.StringWriter textWriter = new System.IO.StringWriter())
            {
                Serial.Serialize(textWriter, ToSerialize);
                return textWriter.ToString();
            }
        }
        static public Class DeserializeClass<Class>(string ToDeserialize)
        {
            System.Xml.Serialization.XmlSerializer Serial = new System.Xml.Serialization.XmlSerializer(typeof(Class));
            Class result;

            using (System.IO.TextReader reader = new System.IO.StringReader(ToDeserialize))
            {
                result = (Class)Serial.Deserialize(reader);
            }

            return result;
        }
        static public void ClearFolder(string dir)
        {
            string[] files = System.IO.Directory.GetFiles(dir);
            foreach (string file in files)
            {
                System.IO.File.Delete(file);
            }
            files = System.IO.Directory.GetDirectories(dir);
            foreach (string file in files)
            {
                ClearFolder(file);
                System.IO.Directory.Delete(file);
            }
        }
        static public void DeleteFolder(string dir)
        {
            ClearFolder(dir);
            System.IO.Directory.Delete(dir);
        }
    }
}
