using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MpeCore.Interfaces;

namespace MpeCore.Classes.SectionPanel
{
  public partial class LicenseAgreement : BaseHorizontalLayout, ISectionPanel
  {
    //private SectionResponseEnum Resp = SectionResponseEnum.Cancel;


    private const string CONST_TEXT = "License text";
    private const string CONST_TEXT_FILE = "License text file";
    private const string CONST_Check = "Checkbox text";

    public LicenseAgreement()
    {
      InitializeComponent();
    }

    #region ISectionPanel Members

    public bool Unique
    {
      get { throw new NotImplementedException(); }
      set { throw new NotImplementedException(); }
    }

    public SectionParamCollection Init()
    {
      throw new NotImplementedException();
    }

    public SectionParamCollection GetDefaultParams()
    {
      SectionParamCollection _param = new SectionParamCollection(Params);
      _param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String, "The text of license agreement"));
      _param.Add(new SectionParam(CONST_TEXT_FILE, "", ValueTypeEnum.File,
                                  "The file of license agreement should be RTF file"));
      _param[Const_LABEL_BIG].Value = "License Agreement";
      _param[Const_LABEL_SMALL].Value = "Please read the following license agreement carefully.";
      _param.Add(new SectionParam(CONST_Check, "I accept the terms of the license agreement.", ValueTypeEnum.String,
                                  "Text of agree checkbox"));
      return _param;
    }

    public void Preview(PackageClass packageClass, SectionItem sectionItem)
    {
      //Mode = ShowModeEnum.Preview;
      Package = packageClass;
      Params = sectionItem.Params;
      Section = sectionItem;
      SetValues();
      ShowDialog();
    }

    private void SetValues()
    {
      BaseHorizontalLayout_Shown(null, null);
      base.button_next.Enabled = false;
      checkBox1.Text = Params[CONST_Check].Value;
      if (File.Exists(Params[CONST_TEXT_FILE].Value))
      {
        richTextBox1.LoadFile(Params[CONST_TEXT_FILE].Value);
      }
      else
      {
        richTextBox1.Text = Params[CONST_TEXT].Value;
      }
    }


    public SectionResponseEnum Execute(PackageClass packageClass, SectionItem sectionItem)
    {
      Package = packageClass;
      Params = sectionItem.Params;
      Section = sectionItem;
      SetValues();
      Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.BeforPanelShow);
      Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelShow);
      if (!packageClass.Silent)
        ShowDialog();
      else
        base.Resp = SectionResponseEnum.Next;
      Base.ActionExecute(Package, Section, ActionExecuteLocationEnum.AfterPanelHide);

      return base.Resp;
    }

    #endregion

    #region ISectionPanel Members

    public string DisplayName
    {
      get { return "License Agreement Screen"; }
    }

    public string Guid
    {
      get { return "{04854407-930E-4c5d-88E8-97CF99878052}"; }
    }

    #endregion

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
      if (checkBox1.Checked)
      {
        base.button_next.Enabled = true;
      }
      else
      {
        base.button_next.Enabled = false;
      }
    }
  }
}