using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace MpeCore.Classes.SectionPanel
{
  public partial class BaseHorizontalLayout : Form
  {
    protected const string Const_LABEL_BIG = "Header Title";
    protected const string Const_LABEL_SMALL = "Header description";
    protected const string Const_IMAGE = "Header image";

    public SectionResponseEnum Resp = SectionResponseEnum.Cancel;
    public PackageClass Package = new PackageClass();
    public ShowModeEnum Mode = ShowModeEnum.Preview;
    public SectionItem Section = new SectionItem();

    public SectionParamCollection Params { get; set; }

    public BaseHorizontalLayout()
    {
      InitializeComponent();
      Params = new SectionParamCollection();
      Params.Add(new SectionParam(Const_LABEL_BIG, "", ValueTypeEnum.String,
                                  "Header title"));
      Params.Add(new SectionParam(Const_LABEL_SMALL, "", ValueTypeEnum.String,
                                  "Description of section, shown in under section title"));
      Params.Add(new SectionParam(Const_IMAGE, "", ValueTypeEnum.File,
                                  "Image in upper right part"));
    }

    private void button_back_Click(object sender, EventArgs e)
    {
      Resp = SectionResponseEnum.Back;
      this.Close();
    }

    private void button_next_Click(object sender, EventArgs e)
    {
      Resp = SectionResponseEnum.Next;
      this.Close();
    }

    private void button_cancel_Click(object sender, EventArgs e)
    {
      Resp = SectionResponseEnum.Cancel;
      this.Close();
    }

    private void BaseVerticalLayout_Load(object sender, EventArgs e) {}

    protected void BaseHorizontalLayout_Shown(object sender, EventArgs e)
    {
      lbl_large.Text = Package.ReplaceInfo(Section.Params[Const_LABEL_BIG].Value);
      lbl_small.Text = Package.ReplaceInfo(Section.Params[Const_LABEL_SMALL].Value);
      if (File.Exists(Section.Params[Const_IMAGE].Value))
        pictureBox1.Load(Section.Params[Const_IMAGE].Value);
      pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
      Text = string.Format("Extension Installer for  {0} - {1}", Package.GeneralInfo.Name,
                           Package.GeneralInfo.Version);
      button_next.Text = "Next>";
      switch (Section.WizardButtonsEnum)
      {
        case WizardButtonsEnum.BackNextCancel:
          button_next.Visible = true;
          button_cancel.Visible = true;
          button_back.Visible = true;
          break;
        case WizardButtonsEnum.NextCancel:
          button_next.Visible = true;
          button_cancel.Visible = true;
          button_back.Visible = false;
          break;
        case WizardButtonsEnum.BackFinish:
          button_next.Visible = true;
          button_cancel.Visible = false;
          button_back.Visible = true;
          button_next.Text = "Finish";
          break;
        case WizardButtonsEnum.Cancel:
          button_next.Visible = false;
          button_cancel.Visible = true;
          button_back.Visible = false;
          break;
        case WizardButtonsEnum.Next:
          button_next.Visible = true;
          button_cancel.Visible = false;
          button_back.Visible = false;
          break;
        case WizardButtonsEnum.Finish:
          button_next.Visible = true;
          button_cancel.Visible = false;
          button_back.Visible = false;
          button_next.Text = "Finish";
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      this.Refresh();
    }
  }
}