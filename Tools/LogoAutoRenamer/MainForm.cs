using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;

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
      Text = Assembly.GetExecutingAssembly().GetName().Name + " v0.9";
    }

    private void buttonStart_Click(object sender, EventArgs e)
    {
      XmlDocument doc = new XmlDocument();
      List<string> rules = new List<string>();
      int i_ch_with_logo = 0;
      int i_ch_no_logo = 0;
      string ch_name;
      string ch_type;
      string logo_name = string.Empty;
      string logo_ext = string.Empty;
      string log = "c:\\" + Assembly.GetExecutingAssembly().GetName().Name + ".log";
      string[] dirs = { textBoxDst.Text, textBoxDst.Text + "\\TV", textBoxDst.Text + "\\Radio" };
      char[] invalid_chs = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

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

      DirectoryInfo dirSrc = new DirectoryInfo(textBoxSrc.Text);

      doc.Load(textboxXml.Text);
      if (doc.DocumentElement != null)
      {
        XmlNodeList NodeList = doc.DocumentElement.SelectNodes("/tvserver/channels/channel");
        foreach (XmlNode Node in NodeList)
        {
          ch_name = Node.Attributes["DisplayName"].Value.ToLower();    // "rete 4"
          string ch_search = ch_name;
          ch_type = Node.Attributes["IsTv"].Value.ToLower() == "true" ? "TV   " : "Radio";

          foreach (char invalid_ch in invalid_chs)
          {
            if (ch_name.Contains(invalid_ch.ToString()))
            {
              ch_search = ch_name.Replace(invalid_ch, ' ');
              ch_name = ch_name.Replace(invalid_ch, '_');
            }
          }

          rules.Clear();
          if (ch_name.Contains("+"))
          {
            ch_search = ch_name.Replace("+", " plus");
            rules.AddRange(AddRules(ch_name.Replace("+", "_plus")));
            rules.AddRange(AddRules(ch_name.Replace("+", "plus_")));
            rules.AddRange(AddRules(ch_name.Replace("+", "plus")));
          }
          rules.AddRange(AddRules(ch_search));

          bool FoundLogo = false;

          foreach (FileInfo f in dirSrc.GetFiles("*"))
          {
            foreach (string rule in rules)
            {
              logo_name = f.Name.ToLower();
              logo_ext = f.Extension.ToLower();

              if (ch_name.Length >= 2 && (logo_name.StartsWith(rule) || logo_name.EndsWith(rule + logo_ext)))
              {
#if DEBUG
                Console.WriteLine("Channelname : <" + ch_name + ">");
                Console.WriteLine("FileName    : <" + logo_name + ">");
                Console.WriteLine("Rule        : <" + rule + ">");
                Console.WriteLine("***");
#endif
                FoundLogo = true;
                break;
              }
            }
            if (FoundLogo) break;
          }

          string ch_name_log = ("<" + ch_name + ">").PadRight(40, ' ') + "[" + ch_type + "]";
          if (FoundLogo)
          {
            File.Copy(textBoxSrc.Text + "\\" + logo_name, textBoxDst.Text + "\\" + ch_type.Trim() + "\\" + ch_name + logo_ext, true);
            tw.WriteLine(ch_name_log + ":  found logo <" + logo_name + ">");
            i_ch_with_logo++;
          }
          else
          {
            tw.WriteLine(ch_name_log + ":  no logo available");
            i_ch_no_logo++;
          }
        }
      }

      tw.WriteLine("");
      tw.WriteLine("******************************");
      tw.WriteLine("Statistics:");
      tw.WriteLine("");
      tw.WriteLine("  found   : " + i_ch_with_logo.ToString().PadLeft(5, ' '));
      tw.WriteLine("  missing : " + i_ch_no_logo.ToString().PadLeft(5, ' '));
      tw.WriteLine("******************************");
      tw.Close();

      MessageBox.Show("Rename complete !");
      Process process = new Process();
      process.StartInfo.FileName = "notepad.exe";
      process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
      process.StartInfo.Arguments = log;
      process.StartInfo.UseShellExecute = true;
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

    private static IEnumerable<string> AddRules(string ch_search)
    {
      //
      // RemoveDiacritics convert щ->u , т->o, м->i, а->a and ий->e
      //
      string ch_clean_search = RemoveDiacritics(ch_search);

      List<string> rules = new List<string>();
      rules.Clear();
      if (ch_clean_search.Contains("-"))
      {
        string tmp_search = ch_clean_search.Split('-')[0].Trim();     // "joi - mediaset premium" -> "joi.xxx"
        if (tmp_search.Length < 2)
        {
          rules.Add(ch_clean_search + ".");
          return rules;
        }
        ch_clean_search = tmp_search;
      }

      rules.Add(ch_clean_search.Replace(".", "") + ".");              // "rtl 102.5"              -> "rtl 1025"
      rules.Add(ch_clean_search + ".");                               // "rete 4"                 -> "rete 4.xxx"
      rules.Add(ch_clean_search.Replace(" ", "") + ".");              // "rete 4"                 -> "rete4.xxx"
      rules.Add(ch_clean_search.Replace(" ", "_") + ".");             // "rete 4"                 -> "rete_4.xxx"

      return rules;
    }

    static string RemoveDiacritics(string stIn)
    {
      string stFormD = stIn.Normalize(NormalizationForm.FormD);
      StringBuilder sb = new StringBuilder();

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

  }
}