#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool.Sections
{
  public partial class ApplicationCtrl : UserControl
  {
    public ApplicationCtrl()
    {
      InitializeComponent();
    }

    private Image _application;
    private Image _statusicon;
    private string _state;
    private string _action;
    private bool _inaction;
    private string _name;
    private string _iconname;
    private string _statusname;

    public object Tag { get; set; }

    public Image Application
    {
      get
      {
        return _application;
      }
      set
      {
        _application = value;
        pbImage.Image = value;
      }
    }

    public Image StatusIcon
    {
      get
      {
        return _statusicon;
      }
      set
      {
        _statusicon = value;
        pbStatusIcon.Image = value;
      }
    }

    public string State
    {
      get
      {
        return _state;
      }
      set
      {
        _state = value;
        lbState.Text = value;
        toolTip1.SetToolTip(lbState, Localizer.GetBestTranslation("Install_colState") + ": " + value);
      }
    }

    public string Action
    {
      get
      {
        return _action;
      }
      set
      {
        _action = value;
        lbAction.Text = value;
        toolTip1.SetToolTip(lbAction, Localizer.GetBestTranslation("Install_colAction") + ": " + value);
      }
    }

    public string Name
    {
      get
      {
        return _name;
      }
      set
      {
        _name = value;
        lbApplication.Text = value;
        toolTip1.SetToolTip(lbApplication, Localizer.GetBestTranslation("Install_colApplication") + ": " + value);
      }
    }

    public string IconName
    {
      get
      {
        return _iconname;
      }
      set
      {
        _iconname = value;
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string strBaseName = assembly.GetName().Name + ".Images";
        try
        {
          System.Resources.ResourceManager rm = new System.Resources.ResourceManager(strBaseName, assembly);
          object im = rm.GetObject(value);
          if (im is Image)
          {
            pbImage.Image = im as Image;
          }
        }
        catch { }
      }
    }

    public string StatusName
    {
      get
      {
        return _statusname;
      }
      set
      {
        _statusname = value;
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        string strBaseName = assembly.GetName().Name + ".Images";
        try
        {
          System.Resources.ResourceManager rm = new System.Resources.ResourceManager(strBaseName, assembly);
          object im = rm.GetObject(value);
          if (im is Image)
          {
            pbStatusIcon.Image = im as Image;
          }
        }
        catch { }
      }
    }

    public bool InAction
    {
      get
      {
        return _inaction;
      }
      set
      {
        _inaction = value;
        lbAction.Font =new Font(lbAction.Font, value ? FontStyle.Bold : FontStyle.Regular);
      }
    }

  }

}
