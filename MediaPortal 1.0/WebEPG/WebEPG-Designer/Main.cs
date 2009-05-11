using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MediaPortal.Utils.Web;

namespace WebEPG_Designer
{
  public partial class Main : Form
  {
    public Main()
    {
      InitializeComponent();

      lvFound.Columns.Add("Index", 50, HorizontalAlignment.Left);
      lvFound.Columns.Add("Source", 300, HorizontalAlignment.Left);

      lvFields.Columns.Add("Field", 80, HorizontalAlignment.Left);
      lvFields.Columns.Add("Value", 130, HorizontalAlignment.Left);

      rbNormal.Checked = true;
    }

    private void bLoad_Click(object sender, EventArgs e)
    {
      HTTPRequest request = new HTTPRequest(tbUrl.Text);
      request.PostQuery = tbPost.Text;
      request.External = cbExternal.Checked;

      HTMLPage page = new HTMLPage(request);

      string source = page.GetPage();

      int startIndex = 0;
      if (tbStart.Text != string.Empty)
      {
        startIndex = source.IndexOf(tbStart.Text, 0, StringComparison.OrdinalIgnoreCase);
        if (startIndex == -1)
          startIndex = 0;
        //return -1;
      }


      int endIndex = source.Length;

      if (tbEnd.Text != string.Empty)
      {
        endIndex = source.IndexOf(tbEnd.Text, startIndex, StringComparison.OrdinalIgnoreCase);
        if (endIndex == -1)
          endIndex = source.Length;
        //return -1;
      }

      source = source.Substring(startIndex, endIndex - startIndex);

      tbSource.Text = source;
    }

    private void bParse_Click(object sender, EventArgs e)
    {
      rbNormal.Checked = true;
      HtmlSectionTemplate template = new HtmlSectionTemplate();

      template.Tags = tbTags.Text;
      template.Template = tbTemplate.Text;

      HtmlProfiler profiler = new HtmlProfiler(template);
      
      int count = 0;

      if (tbSource.Text != null)
      {
        count = profiler.MatchCount(tbSource.Text);
        lvFound.Items.Clear();

        for (int i = 0; i < count; i++)
        {
          ListViewItem lvItem = new ListViewItem(i.ToString());
          lvItem.SubItems.Add(profiler.GetSource(i));
          lvFound.Items.Add(lvItem);
        }
      }
      tbCount.Text = count.ToString();
    }

    private void lvFound_SelectedIndexChanged(object sender, EventArgs e)
    {
      ListView.SelectedListViewItemCollection items = lvFound.SelectedItems;

      if (items.Count > 0)
      {
        ListViewItem item = items[0];

        string sectionSource = item.SubItems[1].Text;

        HtmlSectionTemplate template = new HtmlSectionTemplate();

        template.Tags = tbTags.Text;
        template.Template = tbTemplate.Text;

        HtmlSectionParser parser = new HtmlSectionParser(template);

        ParserData data = new ParserData();

        IParserData iData = data;

        parser.ParseSection(sectionSource, ref iData);

        lvFields.Items.Clear();

        for (int i = 0; i < data.Count; i++)
        {
          ListViewItem lvItem = new ListViewItem(data.GetElementName(i));
          lvItem.SubItems.Add(data.GetElementValue(i).Trim(' ', '\t', '\n'));
          lvFields.Items.Add(lvItem);
        }
      }
    }

    private void rbNormal_CheckedChanged(object sender, EventArgs e)
    {
      if (rbNormal.Checked)
      {
        tbTemplate.Text = tbTemplate.Text.Replace("&lt;", "<");
        tbTemplate.Text = tbTemplate.Text.Replace("&gt;", ">");
        tbTemplate.Text = tbTemplate.Text.Replace("&#60;", "<");
        tbTemplate.Text = tbTemplate.Text.Replace("&#62;", ">");
      }
    }

    private void rbXml_CheckedChanged(object sender, EventArgs e)
    {
      if (rbXml.Checked)
      {
        tbTemplate.Text = tbTemplate.Text.Replace("<", "&lt;");
        tbTemplate.Text = tbTemplate.Text.Replace(">", "&gt;");
      }
    }

    private void tbTemplate_TextChanged(object sender, EventArgs e)
    {
      if (rbNormal.Checked)
      {
        if (tbTemplate.Text.IndexOf("&gt;") != -1 ||
          tbTemplate.Text.IndexOf("&lt;") != -1 ||
          tbTemplate.Text.IndexOf("&#60;") != -1 ||
          tbTemplate.Text.IndexOf("&#62;") != -1)
        {
          tbTemplate.Text = tbTemplate.Text.Replace("&lt;", "<");
          tbTemplate.Text = tbTemplate.Text.Replace("&gt;", ">");
          tbTemplate.Text = tbTemplate.Text.Replace("&#60;", "<");
          tbTemplate.Text = tbTemplate.Text.Replace("&#62;", ">");
        }
      }
    }
  }
}