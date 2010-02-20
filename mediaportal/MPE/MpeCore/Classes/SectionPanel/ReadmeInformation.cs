#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

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
  public partial class ReadmeInformation : BaseHorizontalLayout, ISectionPanel
  {
    //private SectionResponseEnum Resp = SectionResponseEnum.Cancel;


    private const string CONST_TEXT = "Readme text";
    private const string CONST_TEXT_FILE = "Rreadme text file";

    public ReadmeInformation()
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
      _param.Add(new SectionParam(CONST_TEXT, "", ValueTypeEnum.String, "The readme text"));
      _param.Add(new SectionParam(CONST_TEXT_FILE, "", ValueTypeEnum.File, "The readme file should be RTF file"));
      _param[Const_LABEL_BIG].Value = "Readme Information";
      _param[Const_LABEL_SMALL].Value = "Readme Information for [Name]";
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
      if (File.Exists(Params[CONST_TEXT_FILE].Value))
      {
        if (Path.GetExtension(Params[CONST_TEXT_FILE].Value).CompareTo(".rtf") == 0)
          richTextBox1.LoadFile(Params[CONST_TEXT_FILE].Value, RichTextBoxStreamType.RichText);
        else
          richTextBox1.LoadFile(Params[CONST_TEXT_FILE].Value, RichTextBoxStreamType.PlainText);
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
      get { return "Readme Information"; }
    }

    public string Guid
    {
      get { return "{D8242EB9-0D28-4e67-B124-08E7B76266D2}"; }
    }

    #endregion
  }
}