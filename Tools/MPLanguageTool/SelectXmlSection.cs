using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MPLanguageTool
{
  public partial class SelectXmlSection : Form
  {
    public SelectXmlSection()
    {
      InitializeComponent();
      string strSection = string.Empty;
      string strAttrib = string.Empty;

      switch (frmMain.LangType)
      {
        case frmMain.StringsType.MpTagThat:
        case frmMain.StringsType.MediaPortal_II:
          strAttrib = "name";
          strSection = "/Language/Section";
          break;
      }
      List<string> Sections = XmlHandler.ListSections(strSection, strAttrib);
      foreach (string str in Sections)
      {
        cbXmlSection.Items.Add(str);
      }
      cbXmlSection.Sorted = true;
      cbXmlSection.SelectedItem = cbXmlSection.Items[0];
    }

    public string GetSelectedSection()
    {
      return (string)cbXmlSection.SelectedItem;
    }

    private void buttonOk_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.OK;
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }
  }
}
