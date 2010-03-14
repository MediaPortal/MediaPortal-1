using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace LogoAutoRenamer
{
  public partial class MainForm : Form
  {
    public MainForm()
    {
      InitializeComponent();
      string curDir = Environment.CurrentDirectory;
      textBoxSrc.Text = curDir + @"\logos src";
      textBoxDst.Text = curDir + @"\logos dst";
      textboxXml.Text = curDir + @"\export.xml";
      Text = Assembly.GetExecutingAssembly().GetName().Name + " v." + Assembly.GetExecutingAssembly().GetName().Version;

      if ((Environment.OSVersion.Version.Major * 10 + Environment.OSVersion.Version.Minor) >= 60)
      {
        DisableProcessWindowsGhosting();
      }
    }

    public override sealed string Text
    {
      get { return base.Text; }
      set { base.Text = value; }
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
      var doc = new XmlDocument();
      var rules = new List<string>();
      int iChWithLogo = 0;
      int iChNoLogo = 0;
      string chName;
      string chType;
      string logoName = string.Empty;
      string logoExt = string.Empty;
      string log = "c:\\" + Assembly.GetExecutingAssembly().GetName().Name + ".log";
      string[] dirs = { textBoxDst.Text, textBoxDst.Text + "\\TV", textBoxDst.Text + "\\Radio" };
      char[] invalidChs = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

      buttonStart.Enabled = false;

      File.Delete(log);
      StreamWriter tw = File.AppendText(log);
      foreach (string dir in dirs)
      {
        if (!Directory.Exists(dir))
        {
          Directory.CreateDirectory(dir);
        }
      }

      var dirSrc = new DirectoryInfo(textBoxSrc.Text);

      doc.Load(textboxXml.Text);
      if (doc.DocumentElement != null)
      {
        XmlNodeList nodeList = doc.DocumentElement.SelectNodes("/tvserver/channels/channel");
        if (nodeList != null)
        {
          progressBarStatus.Maximum = nodeList.Count;
          progressBarStatus.Minimum = 0;
          progressBarStatus.Step = 1;
          progressBarStatus.Show();
          foreach (XmlNode node in nodeList)
          {
            progressBarStatus.PerformStep();
            chName = node.Attributes["DisplayName"].Value.ToLower(); // "rete 4"
            string chSearch = chName;
            chType = node.Attributes["IsTv"].Value.ToLower() == "true" ? "TV   " : "Radio";

            //Remove invalid chars
            foreach (char invalidCh in invalidChs)
            {
              if (chName.Contains(invalidCh.ToString()))
              {
                chSearch = chName.Replace(invalidCh, ' ');
                chName = chName.Replace(invalidCh, '_');
              }
            }

            //Clears all rules
            rules.Clear();

            //Add rules for channels with the "+" in the name
            if (chName.Contains("+"))
            {
              chSearch = chName.Replace("+", " plus");
              rules.AddRange(AddRules(chName.Replace("+", "_plus")));
              rules.AddRange(AddRules(chName.Replace("+", "plus_")));
              rules.AddRange(AddRules(chName.Replace("+", "plus")));
            }

            //Add rules for channels that end with (?) when ? is a number
            rules.AddRange(AddRules(Regex.Replace(chName, @"(\([0-9]\))", string.Empty)));

            //Add standard rules
            rules.AddRange(AddRules(chSearch));

            bool foundLogo = false;

            foreach (FileInfo f in GetFiles(dirSrc, "*.png,*.jpg,*.gif"))
            {
              foreach (string rule in rules)
              {
                logoName = f.Name.ToLower();
                logoExt = f.Extension.ToLower();

                if (chName.Length >= 2 && (logoName.StartsWith(rule) || logoName.EndsWith(rule + logoExt)))
                {
#if DEBUG
                  Console.WriteLine("Channelname : <" + chName + ">");
                  Console.WriteLine("FileName    : <" + logoName + ">");
                  Console.WriteLine("Rule        : <" + rule + ">");
                  Console.WriteLine("***");
#endif
                  foundLogo = true;
                  break;
                }
              }
              if (foundLogo) break;
            }

            string chNameLog = ("<" + chName + ">").PadRight(40, ' ') + "[" + chType + "]";
            if (foundLogo)
            {
              string src = textBoxSrc.Text + "\\" + logoName;
              string dst = textBoxDst.Text + "\\" + chType.Trim() + "\\" + chName;
              if (logoExt != ".png")
              {
                Image image = Image.FromFile(src);
                image.Save(dst + ".png", ImageFormat.Png);
              }
              else
              {
                File.Copy(src, dst + logoExt, true);
              }
              tw.WriteLine(chNameLog + ":  found logo <" + logoName + ">");
              iChWithLogo++;
            }
            else
            {
              tw.WriteLine(chNameLog + ":  no logo available");
              iChNoLogo++;
            }
          }
        }
      }

      tw.WriteLine("");
      tw.WriteLine("******************************");
      tw.WriteLine("Statistics:");
      tw.WriteLine("");
      tw.WriteLine("  found   : " + iChWithLogo.ToString().PadLeft(5, ' '));
      tw.WriteLine("  missing : " + iChNoLogo.ToString().PadLeft(5, ' '));
      tw.WriteLine("******************************");
      tw.Close();

      MessageBox.Show("Rename complete !");
      var process = new Process
                      {
                        StartInfo =
                          {
                            FileName = "notepad.exe",
                            WindowStyle = ProcessWindowStyle.Maximized,
                            Arguments = log,
                            UseShellExecute = true
                          }
                      };
      process.Start();
      Application.Exit();
    }

    private void buttonSrc_Click(object sender, EventArgs e)
    {
      if (textBoxSrc.Text != string.Empty)
      {
        folderBrowserDialog1.SelectedPath = textBoxSrc.Text;
      }
      folderBrowserDialog1.ShowDialog();
      textBoxSrc.Text = folderBrowserDialog1.SelectedPath;
    }

    private void buttonDst_Click(object sender, EventArgs e)
    {
      if (textBoxDst.Text != string.Empty)
      {
        folderBrowserDialog1.SelectedPath = textBoxDst.Text;
      }
      folderBrowserDialog1.ShowDialog();
      textBoxDst.Text = folderBrowserDialog1.SelectedPath;
    }

    private void buttonXml_Click(object sender, EventArgs e)
    {
      if (textboxXml.Text != string.Empty)
      {
        folderBrowserDialog1.SelectedPath = textboxXml.Text;
      }
      openFileDialog1.ShowDialog();
      textboxXml.Text = openFileDialog1.FileName;
    }

    private static IEnumerable<string> AddRules(string chSearch)
    {
      //
      // RemoveDiacritics convert щ->u , т->o, м->i, а->a and ий->e
      //
      string chCleanSearch = RemoveDiacritics(chSearch);

      var rules = new List<string>();
      rules.Clear();
      if (chCleanSearch.Contains("-"))
      {
        string tmpSearch = chCleanSearch.Split('-')[0].Trim(); // "joi - mediaset premium" -> "joi.xxx"
        if (tmpSearch.Length < 2)
        {
          rules.Add(chCleanSearch + ".");
          return rules;
        }
        chCleanSearch = tmpSearch;
      }

      rules.Add(chCleanSearch.Replace(".", "") + "."); // "rtl 102.5"              -> "rtl 1025"
      rules.Add(chCleanSearch + "."); // "rete 4"                 -> "rete 4.xxx"
      rules.Add(chCleanSearch.Replace(" ", "") + "."); // "rete 4"                 -> "rete4.xxx"
      rules.Add(chCleanSearch.Replace(" ", "_") + "."); // "rete 4"                 -> "rete_4.xxx"

      return rules;
    }

    private static string RemoveDiacritics(string stIn)
    {
      string stFormD = stIn.Normalize(NormalizationForm.FormD);
      var sb = new StringBuilder();

      for (int ich = 0; ich < stFormD.Length; ich++)
      {
        UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
        if (uc != UnicodeCategory.NonSpacingMark)
        {
          sb.Append(stFormD[ich]);
        }
      }
      return (sb.ToString().Normalize(NormalizationForm.FormC));
    }

    private static FileInfo[] GetFiles(DirectoryInfo dir, string searchPatterns)
    {
      ArrayList files = new ArrayList();
      string[] patterns = searchPatterns.Split(',');
      foreach (string pattern in patterns)
      {
        if (pattern.Length != 0)
        {
          files.AddRange(dir.GetFiles(pattern));
        }
      }
      return (FileInfo[])files.ToArray(typeof(FileInfo));
    }

    #region DLL import
    [DllImport("User32.dll", CharSet = CharSet.Auto)]
    public static extern void DisableProcessWindowsGhosting();
    #endregion

  }
}