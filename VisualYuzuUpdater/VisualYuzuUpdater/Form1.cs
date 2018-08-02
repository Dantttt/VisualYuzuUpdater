using SharpCompress.Archives;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace VisualYuzuUpdater
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public struct LinkItem
        {
            public string Href;
            public string Text;

            public override string ToString()
            {
                return Href + "\n\t" + Text;
            }
        }

        static class LinkFinder
        {
            public static List<LinkItem> Find(string file)
            {
                List<LinkItem> list = new List<LinkItem>();

                MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
                    RegexOptions.Singleline);


                foreach (Match m in m1)
                {
                    string value = m.Groups[1].Value;
                    LinkItem i = new LinkItem();

                    Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                        RegexOptions.Singleline);
                    if (m2.Success)
                    {
                        i.Href = m2.Groups[1].Value;
                    }

                    string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                        RegexOptions.Singleline);
                    i.Text = t;

                    list.Add(i);
                }
                return list;
            }
        }

        class release
        {
            public string versionName = "";
            public string urlVersion = "";
            public string urlFile = "";
            public string oldversion = "";
            public string newversion = "";
        }

        string version = "Visual Yuzu Updater - Ver 1.0";
        string instpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";
        string home = "https://github.com";
        string versionError = "Error";
        string cfgfile = "VisualYuzuUpdater.ini";
        string ntemp = "temp.zip";
        List<release> releases;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Activated += AfterLoading;
        }
        private void AfterLoading(object sender, EventArgs e)
        {
            this.Activated -= AfterLoading;
            this.Enabled = false;
            UpdateReport(logo1);
            UpdateReport("Visual Yuzu Updater 1.00- By Danuer 2018");
            UpdateReport("GitHub: https://github.com/Dantttt/VisualYuzuUpdater");
            this.Text = version;
            releases = new List<release>();
            releases.Add(populateClass("nightly"));
            releases.Add(populateClass("canary"));
            cleantemp();
            if (File.Exists(instpath + cfgfile))
            {
                UpdateReport("Read config file");
                StreamReader sr = new StreamReader(instpath + cfgfile);
                string b = sr.ReadToEnd();
                sr.Close();
                string[] buffer = b.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                if (buffer.Length == releases.Count + 2)
                {
                    textBox1.Text = buffer[0];
                    int pos = 1;
                    foreach (release item in releases)
                    {
                        item.oldversion = buffer[pos];
                        pos++;
                    }
                }

            }
            else
            {
                UpdateReport("Configuration file missing");
                ShowHelp();
            }
            if (!Directory.Exists(textBox1.Text))
                textBox1.Text = instpath;
            listView1.Items.Clear();
            for (int i = 0; i < releases.Count; i++)
            {
                UpdateReport("Checking " + releases[i].versionName.ToUpper() + " latest version");
                releases[i] = DownloadVersion(releases[i]);
                UpdateReport(" found %%release%% version: ".Replace("%%release%%", releases[i].versionName) + releases[i].newversion);

                ListViewItem lv = new ListViewItem(releases[i].versionName);
                lv.UseItemStyleForSubItems = false;
                string d = textBox1.Text + @"\" + releases[i].versionName;
                if (!Directory.Exists(d))
                    releases[i].oldversion = "";
                lv.SubItems.Add(releases[i].oldversion);
                Font f = lv.Font;
                Color c = Color.Black;
                if (releases[i].oldversion != releases[i].newversion)
                {
                    c = Color.Navy;
                }
                lv.SubItems.Add(releases[i].newversion,c,Color.White,f);
                listView1.Items.Add(lv);
            }
            this.Enabled = true;
        }

        private release DownloadVersion(release r)
        {
            try
            {
                using (var myWebClient = new WebClient())
                {
                    myWebClient.Headers["User-Agent"] = "MOZILLA/5.0 (WINDOWS NT 6.1; WOW64) APPLEWEBKIT/537.1 (KHTML, LIKE GECKO) CHROME/21.0.1180.75 SAFARI/537.1";

                    string page = myWebClient.DownloadString(r.urlVersion);
                    foreach (LinkItem i in LinkFinder.Find(page))
                    {
                        int q = i.Href.IndexOf("yuzu-windows-mingw");
                        if (q > 0)
                        {
                            string[] ver = i.Href.Split('/', '-');
                            r.urlFile = i.Href;
                            r.newversion = ver[8] + "-" + ver[12];
                            break;
                        }
                    }
                }
            }
            catch
            {
                r.newversion = versionError;
            }
            return r;
        }

        private release populateClass(string releaseName)
        {
            release r = new release();
            r.versionName = releaseName;
            r.urlVersion = home + "/yuzu-emu/yuzu-%%release%%/releases/".Replace("%%release%%", releaseName);
            return r;
        }

        private void ShowTooltip(IWin32Window window, string text, int x, int y)
        {
            ToolTip tt = new ToolTip();
            tt.IsBalloon = true;
            tt.BackColor = Color.Lime;
            tt.Show(text, window, x, y, 10000);
        }
        static string logo1 =
    @"____   ____.__                    .__    _____.___.                     ____ ___            .___       __" + "\r\n" +
    @"\   \ /   /|__| ________ _______  |  |   \__  |   |__ ____________ __  |    |   \______   __| _/____ _/  |_  ___________" + "\r\n" +
    @" \   Y   / |  |/  ___/  |  \__  \ |  |    /   |   |  |  \___   /  |  \ |    |   /\____ \ / __ |\__  \\   __\/ __ \_  __ \" + "\r\n" +
    @"  \     /  |  |\___ \|  |  // __ \|  |__  \____   |  |  //    /|  |  / |    |  / |  |_> > /_/ | / __ \|  | \  ___/|  | \/" + "\r\n" +
    @"   \___/   |__/____  >____/(____  /____/  / ______|____//_____ \____/  |______/  |   __/\____ |(____  /__|  \___  >__|  " + "\r\n" +
    @"                   \/           \/        \/                  \/                 |__|        \/     \/          \/  " + "\r\n";

        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            textBox1.Text = FileList[0];
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] listFile = (string[])e.Data.GetData(DataFormats.FileDrop, false);
                if (listFile.Length == 1)
                {
                    if (Directory.Exists(listFile[0]))
                    {
                        e.Effect = DragDropEffects.Copy;
                    }
                }
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void UpdateReport(string s)
        {
            textBox2.AppendText(s + "\r\n");
            Application.DoEvents();
        }
        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void ShowHelp()
        {
            ShowTooltip(listView1, "Double Click = Execute selected version\r\nRight Click = Show options", 50, -40);
            ShowTooltip(textBox1, "Drag your output folder HERE!!", 50, -40);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteSetting();
        }

        private void WriteSetting()
        {
            StreamWriter sw = new StreamWriter(instpath + cfgfile, false);
            sw.WriteLine(textBox1.Text);
            foreach (release item in releases)
            {
                sw.WriteLine(item.oldversion);
            }
            sw.Close();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int idx = listView1.SelectedIndices[0];
                contextMenuStrip1.Items[0].Enabled = (releases[idx].oldversion != "") ? true : false;
                contextMenuStrip1.Items[3].Enabled = (releases[idx].oldversion != "") ? true : false;
                contextMenuStrip1.Items[7].Enabled = true;

                contextMenuStrip1.Items[2].Enabled = (releases[idx].newversion != versionError) ? true : false;
            }
            else
            {
                contextMenuStrip1.Items[0].Enabled = false;
                contextMenuStrip1.Items[3].Enabled = false;
                contextMenuStrip1.Items[7].Enabled = false;

                contextMenuStrip1.Items[2].Enabled = false;

            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
            {
                int idx = listView1.SelectedIndices[0];
                UpdateReport("Clean " + releases[idx].versionName + " profile");
                releases[idx].oldversion = "";
                listView1.Items[idx].SubItems[1].Text = "";

                cleantemp();
                string d = textBox1.Text + @"\" + releases[idx].versionName;
                if (Directory.Exists(d))
                    Directory.Delete(d, true);
            }
        }
        private void cleantemp()
        {
            try
            {
                string t = instpath + ntemp;
                if (File.Exists(t))
                {
                    UpdateReport("Cleaning old cache...\r\n");
                    File.Delete(t);
                }
            }
            catch
            {

            }
        }
        private static byte[] GetHashSha256(string filename)
        {
            SHA256 Sha256 = SHA256.Create();
            using (FileStream stream = File.OpenRead(filename))
            {
                return Sha256.ComputeHash(stream);
            }
        }
        public static string BytesToString(byte[] bytes)
        {
            string result = "";
            foreach (byte b in bytes) result += b.ToString("x2");
            return result;
        }

        private void donwloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            using (var myWebClient = new WebClient())
            {
                int idx = listView1.SelectedIndices[0];
                UpdateReport("Downloading new " + releases[idx].versionName+ " version...");
                myWebClient.DownloadFile(home + releases[idx].urlFile, ntemp);
                UpdateReport("Bytes: " + new System.IO.FileInfo(ntemp).Length.ToString() + "\r\nSHA256: [" + BytesToString(GetHashSha256(ntemp)) + "]");
                string d = textBox1.Text + @"\" + releases[idx].versionName;
                if (Directory.Exists(d))
                    Directory.Delete(d, true);
                UpdateReport("Unpacking...");
                var compressed = ArchiveFactory.Open(ntemp);
                ExtractionOptions ex = new ExtractionOptions();
                ex.ExtractFullPath = true;
                ex.Overwrite = true;
                foreach (var entry in compressed.Entries)
                {
                    UpdateReport(entry.Key + "\tCRC: [" + entry.Crc + "]\tSize: " + entry.Size + " bytes");
                    if (!entry.IsDirectory)
                    {
                        entry.WriteToDirectory(d, ex);
                    }
                    else
                    {
                        Directory.CreateDirectory(d + @"\" + entry);
                    }
                }
                compressed.Dispose();
                releases[idx].oldversion = releases[idx].newversion;
                listView1.Items[idx].SubItems[1].Text = releases[idx].oldversion;
                listView1.Items[idx].SubItems[2].ForeColor= Color.Black;
                UpdateReport("Done");
                this.Enabled = true;
            }
        }

        private void executeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = listView1.SelectedIndices[0];
            string d = textBox1.Text + @"\" + releases[idx].versionName + @"\" + releases[idx].versionName + @"-mingw\yuzu.exe";
            UpdateReport("Execute Yuzu.exe and exit");
            try
            {
                System.Diagnostics.Process.Start(d, "");
                this.Close();
            }
            catch { }
        }

        private void browseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int idx = listView1.SelectedIndices[0];
            string d = textBox1.Text + @"\" + releases[idx].versionName + @"\" + releases[idx].versionName + @"-mingw\yuzu.exe";

            string argument = "/select, \"" + d+ "\"";
            System.Diagnostics.Process.Start("explorer.exe", argument);
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WriteSetting();
            UpdateReport("Refreshing info");
            UpdateReport("-------------------------------------------------------------------------------------------------------------------------");
            AfterLoading(sender, e);
        }
    }
}
