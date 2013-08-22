#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

#region Usings

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Media.Animation;
using MediaPortal.Profile;
using Microsoft.DirectX.Direct3D;
using MediaPortal.ExtensionMethods;

// used for Keys definition

#endregion

namespace MediaPortal.GUI.Library
{
  public class GUIMenuControl : GUIControl, IComparer<MenuButtonInfo>
  {
    #region Properties (Skin)

    [XMLSkinElement("spaceBetweenButtons")]
    protected int _spaceBetweenButtons = 8;

    [XMLSkinElement("textcolor")]
    protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textColorNoFocus")]
    protected long _textColorNoFocus = 0xFFFFFFFF;

    [XMLSkinElement("textAlign")]
    protected Alignment _textAlignment = Alignment.ALIGN_LEFT;

    [XMLSkinElement("buttonWidth")]
    protected int _buttonWidth = 60;
    [XMLSkinElement("buttonHeight")]
    protected int _buttonHeight = 30;

    [XMLSkinElement("buttonTextXOff")]
    protected int _buttonTextXOffset = 10;
    [XMLSkinElement("buttonTextYOff")]
    protected int _buttonTextYOffset = 8;

    [XMLSkinElement("buttonOffset")]
    protected int _buttonOffset = 25; // offset from the border to the buttons

    [XMLSkinElement("buttonFont")]
    protected string _buttonFont = "font16";

    [XMLSkinElement("textureBackground")]
    protected string _textureBackground = string.Empty;

    [XMLSkinElement("textureButtonFocus")]
    protected string _textureButtonFocus = string.Empty;
    [XMLSkinElement("textureButtonNoFocus")]
    protected string _textureButtonNoFocus = string.Empty;

    [XMLSkinElement("textureHoverNoFocus")]
    protected string _textureHoverNoFocus = string.Empty;

    [XMLSkinElement("hoverX")]
    protected int _hoverPositionX = 0;
    [XMLSkinElement("hoverY")]
    protected int _hoverPositionY = 0;

    [XMLSkinElement("hoverWidth")]
    protected int _hoverWidth = 0;
    [XMLSkinElement("hoverHeight")]
    protected int _hoverHeight = 0;

    [XMLSkinElement("hoverKeepAspectratio")]
    protected bool _hoverKeepAspectRatio = true;

    [XMLSkinElement("horizontal")]
    protected bool _horizontal = false;

    [XMLSkinElement("showAllHover")]
    protected bool _showAllHover = false;

    [XMLSkinElement("shadowAngle")]
    protected int _shadowAngle = 0;
    [XMLSkinElement("shadowDistance")]
    protected int _shadowDistance = 0;
    [XMLSkinElement("shadowColor")]
    protected long _shadowColor = 0xFF000000;

    [XMLSkinElement("onclick")]
    protected string _onclick = "";

    [XMLSkin("hover", "flipX")]
    protected bool _flipX = false;
    [XMLSkin("hover", "flipY")]
    protected bool _flipY = false;
    [XMLSkin("hover", "diffuse")]
    protected string _diffuseFileName = "";

    #endregion

    #region Enums

    #endregion

    #region Variables

    protected List<MenuButtonInfo> _buttonInfos = new List<MenuButtonInfo>();
    protected List<GUIButtonControl> _buttonList = new List<GUIButtonControl>();
    protected List<GUIAnimation> _hoverList = new List<GUIAnimation>();
    protected GUIAnimation _hoverImage = null;
    protected GUIAnimation _backgroundImage = null;

    protected int _focusPosition = 0; // current position of the focus 

    #endregion

    #region Constructors/Destructors

    public GUIMenuControl(int dwParentID)
      : base(dwParentID) { }

    #endregion

    #region Base class overrides

    #region Methods

    public override Type GetSubType(string subType)
    {
      if (string.IsNullOrEmpty(subType))
        return typeof(GUIMenuClassic);

      Type[] types = Assembly.GetExecutingAssembly().GetTypes();
      foreach (Type type in types)
      {
        if (type.IsSubclassOf(typeof(GUIMenuControl)))
        {
          if (type.Name.Equals("guimenu" + subType, StringComparison.InvariantCultureIgnoreCase))
            return type;
        }
      }
      return typeof(GUIMenuClassic);
    }

    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
    }

    public override void OnInit()
    {
      LoadSetting();

      base.OnInit();
    }

    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();

      GUIGraphicsContext.ScalePosToScreenResolution(ref _buttonWidth, ref _buttonHeight);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _buttonTextXOffset, ref _buttonTextYOffset);

      GUIGraphicsContext.ScalePosToScreenResolution(ref _hoverPositionX, ref _hoverPositionY);
      GUIGraphicsContext.ScalePosToScreenResolution(ref _hoverWidth, ref _hoverHeight);
    }

    public override void AllocResources()
    {
      base.AllocResources();
    }

    public override void Dispose()
    {
      SaveSetting();

      base.Dispose();
    }

    public override void OnAction(Action action)
    {
      base.OnAction(action);
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
    }

    #endregion

    #region Properties

    public override bool Focus
    {
      get { return base.Focus; }
      set
      {
        if (value)
        {
          if (_backgroundImage != null)
          {
            _backgroundImage.Begin();
          }
        }
        base.Focus = value;
        if (FocusedButton >= 0 && FocusedButton < _buttonList.Count)
        {
          _buttonList[FocusedButton].Focus = value;
        }
      }
    }

    #endregion

    #endregion

    #region IComparer implementation

    public int Compare(MenuButtonInfo x, MenuButtonInfo y)
    {
      return (x.Index - y.Index);
    }

    #endregion

    #region Public virtual properties

    public virtual int FocusedButton { get; set; }

    public virtual bool FixedScroll { get; set; }

    public virtual bool EnableAnimation { get; set; }

    public virtual List<MenuButtonInfo> ButtonInfos { get; private set; }

    #endregion

    #region Protected virtual methods

    protected virtual void LoadSetting()
    {
      using (Settings xmlreader = new MPSettings())
      {
        string section = "Menu" + this.ParentID.ToString();
        int focus = xmlreader.GetValueAsInt(section, "focus", 3);
        string label = xmlreader.GetValue(section, "label");

        if (label == null)
        {
          return;
        }

        for (int i = 0; i < _buttonList.Count; i++)
        {
          if (_buttonList[focus].Label.Equals(label))
          {
            FocusedButton = focus;
            break;
          }
        }
      }
    }

    protected virtual void SaveSetting()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        string section = "Menu" + this.ParentID.ToString();
        xmlwriter.SetValue(section, "focus", FocusedButton.ToString());
        if (FocusedButton >= 0 && FocusedButton < _buttonList.Count)
        {
          xmlwriter.SetValue(section, "label", _buttonList[FocusedButton].Label);
        }
      }
    }

    #endregion
  }

  public class MenuButtonInfo
  {
    #region Variables

    protected string _text;
    protected int _pluginID;
    protected string _focusedTextureName;
    protected string _nonFocusedTextureName;
    protected string _hoverName;
    protected string _nonFocusHoverName;
    protected int _index;

    #endregion

    #region Constructors/Destructors

    public MenuButtonInfo(string Text, int PlugInID, string FocusTextureName, string NonFocusName, string HoverName,
                          string NonFocusHoverName, int Index)
    {
      _text = Text;
      _pluginID = PlugInID;
      _focusedTextureName = FocusTextureName;
      _nonFocusedTextureName = NonFocusName;
      _hoverName = HoverName;
      _nonFocusHoverName = NonFocusHoverName;
      _index = Index;
    }

    public MenuButtonInfo(string Text, int PlugInID, string FocusTextureName, string NonFocusName, string HoverName,
                          int index)
    {
      _text = Text;
      _pluginID = PlugInID;
      _focusedTextureName = FocusTextureName;
      _nonFocusedTextureName = NonFocusName;
      _hoverName = HoverName;
      _nonFocusHoverName = NonFocusName;
      _index = Index;
    }

    #endregion

    #region Public properties

    public string Text
    {
      get { return _text; }
    }

    public int PluginID
    {
      get { return _pluginID; }
    }

    public string FocusTextureName
    {
      get { return _focusedTextureName; }
    }

    public string NonFocusTextureName
    {
      get { return _nonFocusedTextureName; }
    }

    public string HoverName
    {
      get { return _hoverName; }
    }

    public string NonFocusHoverName
    {
      get { return _nonFocusHoverName; }
    }

    public int Index
    {
      get { return _index; }
      set { _index = value; }
    }

    #endregion
  }
}