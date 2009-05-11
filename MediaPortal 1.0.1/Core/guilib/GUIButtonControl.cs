#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System.Diagnostics;
using System.IO;
using System.Windows.Controls;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class implementing a GUIButton.
  /// </summary>
  public class GUIButtonControl : GUIControl
  {
    [XMLSkinElement("textureFocus")] protected string _focusedTextureName = "";
    [XMLSkinElement("textureNoFocus")] protected string _nonFocusedTextureName = "";
    [XMLSkinElement("font")] protected string _fontName;
    [XMLSkinElement("label")] protected string _label = "";
    [XMLSkinElement("textcolor")] protected long _textColor = 0xFFFFFFFF;
    [XMLSkinElement("textcolorNoFocus")] protected long _textColorNoFocus = 0xFFFFFFFF;
    [XMLSkinElement("disabledcolor")] protected long _disabledColor = 0xFF606060;
    [XMLSkinElement("hyperlink")] protected int _hyperLinkWindowId = -1;
    [XMLSkinElement("action")] protected int _actionId = -1;
    [XMLSkinElement("script")] protected string _scriptAction = "";
    [XMLSkinElement("textXOff")] protected int _textOffsetX = 0;
    [XMLSkinElement("textYOff")] protected int _textOffsetY = 0;
    [XMLSkinElement("textalign")] protected Alignment _textAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("application")] protected string _application = "";
    [XMLSkinElement("arguments")] protected string _arguments = "";
    [XMLSkinElement("hover")] protected string _hoverFilename = string.Empty;
    [XMLSkinElement("hoverX")] protected int _hoverX;
    [XMLSkinElement("hoverY")] protected int _hoverY;
    [XMLSkinElement("hoverWidth")] protected int _hoverWidth;
    [XMLSkinElement("hoverHeight")] protected int _hoverHeight;

    protected int _frameCounter = 0;
    protected GUIAnimation _imageFocused = null;
    protected GUIAnimation _imageNonFocused = null;
    protected GUIAnimation _hoverImage = null;
    protected GUILabelControl _labelControl = null;

    public GUIButtonControl(int dwParentID)
      : base(dwParentID)
    {
    }

    /// <summary>
    /// The constructor of the GUIButtonControl class.
    /// </summary>
    /// <param name="dwParentID">The parent of this control.</param>
    /// <param name="dwControlId">The ID of this control.</param>
    /// <param name="dwPosX">The X position of this control.</param>
    /// <param name="dwPosY">The Y position of this control.</param>
    /// <param name="dwWidth">The width of this control.</param>
    /// <param name="dwHeight">The height of this control.</param>
    /// <param name="strTextureFocus">The filename containing the texture of the butten, when the button has the focus.</param>
    /// <param name="strTextureNoFocus">The filename containing the texture of the butten, when the button does not have the focus.</param>
    public GUIButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                            string strTextureFocus, string strTextureNoFocus)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      FinalizeConstruction();
    }

    // allow overriding the textcolor if created by GUIMenuControl
    public GUIButtonControl(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                            long ltextColor, long ltextcolorNoFocus, string strTextureFocus, string strTextureNoFocus)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _focusedTextureName = strTextureFocus;
      _nonFocusedTextureName = strTextureNoFocus;
      _textColor = ltextColor;
      _textColorNoFocus = ltextcolorNoFocus;
      FinalizeConstruction();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      _imageFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                           _focusedTextureName);
      _imageFocused.ParentControl = this;
      _imageFocused.Filtering = false;
      _imageFocused.DimColor = DimColor;
      _imageFocused.ColourDiffuse = ColourDiffuse;

      _imageNonFocused = LoadAnimationControl(_parentControlId, _controlId, _positionX, _positionY, _width, _height,
                                              _nonFocusedTextureName);
      _imageNonFocused.ParentControl = this;
      _imageNonFocused.Filtering = false;
      _imageNonFocused.DimColor = DimColor;
      _imageNonFocused.ColourDiffuse = ColourDiffuse;

      if (_hoverFilename != string.Empty)
      {
        GUIGraphicsContext.ScaleRectToScreenResolution(ref _hoverX, ref _hoverY, ref _hoverWidth, ref _hoverHeight);
        _hoverImage = LoadAnimationControl(_parentControlId, _controlId, _hoverX, _hoverY, _hoverWidth, _hoverHeight,
                                           _hoverFilename);
        _hoverImage.ParentControl = this;
        _hoverImage.DimColor = DimColor;
        _hoverImage.ColourDiffuse = ColourDiffuse;
      }

      GUILocalizeStrings.LocalizeLabel(ref _label);
      _labelControl = new GUILabelControl(_parentControlId, 0, _positionX, _positionY, _width, _height, _fontName,
                                          _label, _textColor, Alignment.ALIGN_LEFT, false);
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.DimColor = DimColor;
      _labelControl.ParentControl = this;
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control to scale itself to the current screen resolution
    /// </summary>
    public override void ScaleToScreenResolution()
    {
      base.ScaleToScreenResolution();
      GUIGraphicsContext.ScalePosToScreenResolution(ref _textOffsetX, ref _textOffsetY);
    }


    public override bool Focus
    {
      get { return IsFocused; }
      set
      {
        if (value != IsFocused)
        {
          if (value == true)
          {
            if (_imageFocused != null)
            {
              _imageFocused.Begin();
            }
            GUIPropertyManager.SetProperty("#highlightedbutton", Label);
          }
          else
          {
            if (_imageNonFocused != null)
            {
              _imageNonFocused.Begin();
            }
          }
        }

        base.Focus = value;
      }
    }

    /// <summary>
    /// Renders the GUIButtonControl.
    /// </summary>
    public override void Render(float timePassed)
    {
      // Do not render if not visible.
      if (GUIGraphicsContext.EditMode == false)
      {
        if (!IsVisible)
        {
          base.Render(timePassed);
          return;
        }
      }

      // The GUIButtonControl has the focus
      if (Focus)
      {
        //render the focused image
        _imageFocused.Render(timePassed);

        if (_hoverImage != null)
        {
          _hoverImage.Render(timePassed);
        }
      }
      else
      {
        //render the non-focused image
        _imageNonFocused.Render(timePassed);
      }

      int labelWidth = _width - 2*_textOffsetX;
      if (labelWidth <= 0)
      {
        base.Render(timePassed);
        return;
      }
      _labelControl.Width = labelWidth;
      _labelControl.TextAlignment = _textAlignment;
      _labelControl.Label = _label;
      _labelControl.TextColor = Disabled ? _disabledColor : Focus ? _textColor : _textColorNoFocus;

      // render the text on the button
      int x = 0;

      switch (_textAlignment)
      {
        case Alignment.ALIGN_LEFT:
          x = _textOffsetX + _positionX;
          break;

        case Alignment.ALIGN_RIGHT:
          x = _positionX + _width - _textOffsetX;
          break;

        case Alignment.ALIGN_CENTER:
          x = _positionX + ((_width/2) - (labelWidth/2));
          break;
      }
      _labelControl.SetPosition(x, _textOffsetY + _positionY);
      _labelControl.Render(timePassed);
      base.Render(timePassed);
    }

    /// <summary>
    /// OnAction() method. This method gets called when there's a new action like a 
    /// keypress or mousemove or... By overriding this method, the control can respond
    /// to any action
    /// </summary>
    /// <param name="action">action : contains the action</param>
    public override void OnAction(Action action)
    {
      base.OnAction(action);
      GUIMessage message;
      if (Focus)
      {
        if (action.wID == Action.ActionType.ACTION_MOUSE_CLICK || action.wID == Action.ActionType.ACTION_SELECT_ITEM)
        {
          if (ContextMenu != null)
          {
            DoContextMenu();
            return;
          }

          // If this button contains scriptactions call the scriptactions.
          if (_application.Length != 0)
          {
            //button should start an external application, so start it
            Process proc = new Process();

            string workingFolder = Path.GetFullPath(_application);
            string fileName = Path.GetFileName(_application);
            workingFolder = workingFolder.Substring(0, workingFolder.Length - (fileName.Length + 1));
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.WorkingDirectory = workingFolder;
            proc.StartInfo.Arguments = _arguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            proc.StartInfo.CreateNoWindow = true;
            proc.Start();
            //proc.WaitForExit();
          }

          // If this links to another window go to the window.
          if (_hyperLinkWindowId >= 0)
          {
            GUIWindowManager.ActivateWindow((int) _hyperLinkWindowId);
            return;
          }
          // If this button corresponds to an action generate that action.
          if (ActionID >= 0)
          {
            Action newaction = new Action((Action.ActionType) ActionID, 0, 0);
            GUIGraphicsContext.OnAction(newaction);
            return;
          }

          // button selected.
          if (SubItemCount > 0)
          {
            // if we got subitems, then change the label of the control to the next
            //subitem
            SelectedItem++;
            if (SelectedItem >= SubItemCount)
            {
              SelectedItem = 0;
            }
            Label = (string) GetSubItem(SelectedItem);
          }

          // send a message to anyone interested 
          message = new GUIMessage(GUIMessage.MessageType.GUI_MSG_CLICKED, WindowId, GetID, ParentID, 0, 0, null);
          GUIGraphicsContext.SendMessage(message);
        }
      }
    }

    /// <summary>
    /// OnMessage() This method gets called when there's a new message. 
    /// Controls send messages to notify their parents about their state (changes)
    /// By overriding this method a control can respond to the messages of its controls
    /// </summary>
    /// <param name="message">message : contains the message</param>
    /// <returns>true if the message was handled, false if it wasnt</returns>
    public override bool OnMessage(GUIMessage message)
    {
      // Handle the GUI_MSG_LABEL_SET message
      if (message.TargetControlId == GetID)
      {
        if (message.Message == GUIMessage.MessageType.GUI_MSG_LABEL_SET)
        {
          if (message.Label != null)
          {
            Label = message.Label;
          }
          return true;
        }
      }
      // Let the base class handle the other messages
      if (base.OnMessage(message))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Preallocates the control its DirectX resources.
    /// </summary>
    public override void PreAllocResources()
    {
      base.PreAllocResources();
      _imageFocused.PreAllocResources();
      _imageNonFocused.PreAllocResources();

      if (_hoverImage != null)
      {
        _hoverImage.PreAllocResources();
      }
    }

    /// <summary>
    /// Allocates the control its DirectX resources.
    /// </summary>
    public override void AllocResources()
    {
      base.AllocResources();
      _frameCounter = 0;
      _imageFocused.AllocResources();
      _imageNonFocused.AllocResources();

      if (_hoverImage != null)
      {
        _hoverImage.AllocResources();
      }

      _width = _imageFocused.Width;
      _height = _imageFocused.Height;

      if (SubItemCount > 0)
      {
        Label = (string) GetSubItem(SelectedItem);
      }
      _labelControl.Width = _width;
      _labelControl.Height = _height;
      _labelControl.AllocResources();
    }

    /// <summary>
    /// Frees the control its DirectX resources.
    /// </summary>
    public override void FreeResources()
    {
      base.FreeResources();
      _imageFocused.FreeResources();
      _imageNonFocused.FreeResources();
      _labelControl.FreeResources();

      if (_hoverImage != null)
      {
        _hoverImage.FreeResources();
      }
    }

    /// <summary>
    /// Sets the position of the control.
    /// </summary>
    /// <param name="dwPosX">The X position.</param>
    /// <param name="dwPosY">The Y position.</param>		
    public override void SetPosition(int dwPosX, int dwPosY)
    {
      base.SetPosition(dwPosX, dwPosY);
      _imageFocused.SetPosition(dwPosX, dwPosY);
      _imageNonFocused.SetPosition(dwPosX, dwPosY);
    }

    /// <summary>
    /// Changes the alpha transparency component of the colordiffuse.
    /// </summary>
    /// <param name="dwAlpha">The new value of the colordiffuse.</param>
    public override void SetAlpha(int dwAlpha)
    {
      base.SetAlpha(dwAlpha);
      _imageFocused.SetAlpha(dwAlpha);
      _imageNonFocused.SetAlpha(dwAlpha);

      if (_hoverImage != null)
      {
        _hoverImage.SetAlpha(dwAlpha);
      }
    }

    /// <summary>
    /// Get/set the color of the text when the GUIButtonControl is disabled.
    /// </summary>
    public long DisabledColor
    {
      get { return _disabledColor; }
      set { _disabledColor = value; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIButtonControl does not have the focus.
    /// </summary>
    public string TexutureNoFocusName
    {
      get { return _nonFocusedTextureName; } //_imageNonFocused.FileName; }
    }

    /// <summary>
    /// Get the filename of the texture when the GUIButtonControl has the focus.
    /// </summary>
    public string TexutureFocusName
    {
      get { return _focusedTextureName; } //_imageFocused.FileName; }
    }

    /// <summary>
    /// Get the filename of the hover texture when the GUIButtonControl.
    /// </summary>
    public string HoverFilename
    {
      get { return _hoverFilename; }
    }

    /// <summary>
    /// Set the color of the text on the GUIButtonControl. 
    /// </summary>
    public long TextColor
    {
      get { return _textColor; }
      set { _textColor = value; }
    }

    /// <summary>
    /// Get/Set the color of the text on the GUIButtonControl when it has no focus. 
    /// </summary>
    public long TextColorNoFocus
    {
      get { return _textColorNoFocus; }
      set { _textColorNoFocus = value; }
    }


    /// <summary>
    /// Get/set the name of the font of the text of the GUIButtonControl.
    /// </summary>
    public string FontName
    {
      get { return _fontName; }
      set
      {
        if (value == null)
        {
          return;
        }
        _fontName = value;
        _labelControl.FontName = _fontName;
      }
    }

    /// <summary>
    /// Set the text of the GUIButtonControl. 
    /// </summary>
    /// <param name="strFontName">The font name.</param>
    /// <param name="strLabel">The text.</param>
    /// <param name="dwColor">The font color.</param>
    public void SetLabel(string strFontName, string strLabel, long dwColor)
    {
      if (strFontName == null)
      {
        return;
      }
      if (strLabel == null)
      {
        return;
      }
      Label = strLabel;
      _textColor = dwColor;
      _fontName = strFontName;

      _labelControl.FontName = _fontName;
      _labelControl.TextColor = dwColor;
      _labelControl.Label = strLabel;
    }

    /// <summary>
    /// Get/set the text of the GUIButtonControl.
    /// </summary>
    public string Label
    {
      get { return _label; }
      set
      {
        if (value == null)
        {
          return;
        }

        _label = value;
        _labelControl.Label = _label;
      }
    }

    /// <summary>
    /// Get/set the window ID to which the GUIButtonControl links.
    /// </summary>
    public int HyperLink
    {
      get { return _hyperLinkWindowId; }
      set { _hyperLinkWindowId = value; }
    }

    /// <summary>
    /// Get/set the scriptaction that needs to be performed when the button is clicked.
    /// </summary>
    public string ScriptAction
    {
      get { return _scriptAction; }
      set
      {
        if (value == null)
        {
          return;
        }
        _scriptAction = value;
      }
    }

    /// <summary>
    /// Get/set the action ID that corresponds to this button.
    /// </summary>
    public int ActionID
    {
      get { return _actionId; }
      set { _actionId = value; }
    }

    /// <summary>
    /// Get/set the X-offset of the label.
    /// </summary>
    public int TextOffsetX
    {
      get { return _textOffsetX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetX = value;
      }
    }

    /// <summary>
    /// Get/set the Y-offset of the label.
    /// </summary>
    public int TextOffsetY
    {
      get { return _textOffsetY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _textOffsetY = value;
      }
    }

    /// <summary>
    /// Get/set the X-Position of the hover.
    /// </summary>
    public int HoverX
    {
      get { return _hoverX; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverX = value;
      }
    }

    /// <summary>
    /// Get/set the Y-Position of the hover.
    /// </summary>
    public int HoverY
    {
      get { return _hoverY; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverY = value;
      }
    }

    /// <summary>
    /// Get/set the width of the hover.
    /// </summary>
    public int HoverWidth
    {
      get { return _hoverWidth; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverWidth = value;
      }
    }

    /// <summary>
    /// Get/set the height of the hover.
    /// </summary>
    public int HoverHeight
    {
      get { return _hoverHeight; }
      set
      {
        if (value < 0)
        {
          return;
        }
        _hoverHeight = value;
      }
    }

    public Alignment TextAlignment
    {
      get { return _textAlignment; }
      set { _textAlignment = value; }
    }

    /// <summary>
    /// Perform an update after a change has occured. E.g. change to a new position.
    /// </summary>
    protected override void Update()
    {
      base.Update();

      _imageFocused.ColourDiffuse = ColourDiffuse;
      _imageFocused.Width = _width;
      _imageFocused.Height = _height;

      _imageNonFocused.ColourDiffuse = ColourDiffuse;
      _imageNonFocused.Width = _width;
      _imageNonFocused.Height = _height;

      _imageFocused.SetPosition(_positionX, _positionY);
      _imageNonFocused.SetPosition(_positionX, _positionY);
    }

    public void Refresh()
    {
      Update();
    }

    /// <summary>
    /// Get/Set the the application filename
    /// which should be launched when this button gets clicked
    /// </summary>
    public string Application
    {
      get { return _application; }
      set
      {
        if (value == null)
        {
          return;
        }
        _application = value;
      }
    }

    /// <summary>
    /// Get/Set the arguments for the application
    /// which should be launched when this button gets clicked
    /// </summary>
    public string Arguments
    {
      get { return _arguments; }
      set
      {
        if (value == null)
        {
          return;
        }
        _arguments = value;
      }
    }

    /// <summary>
    /// get/set the current selected item
    /// A button can have 1 or more subitems
    /// each subitem has its own text to render on the button
    /// When the user presses the button, the next item will be selected
    /// and shown on the button
    /// </summary>
    public override int SelectedItem
    {
      get { return _selectedItem; }
      set
      {
        if (value < 0)
        {
          return;
        }
        if (SubItemCount > 0)
        {
          _selectedItem = value;
          if (_selectedItem < 0 || _selectedItem >= SubItemCount)
          {
            _selectedItem = 0;
          }
          Label = (string) GetSubItem(_selectedItem);
        }
        else
        {
          _selectedItem = 0;
        }
      }
    }

    private void DoContextMenu()
    {
      IDialogbox dialog = (IDialogbox) GUIWindowManager.GetWindow((int) GUIWindow.Window.WINDOW_DIALOG_MENU);

      if (dialog == null)
      {
        return;
      }

      dialog.Reset();
      dialog.SetHeading(924); // menu

      foreach (object item in ContextMenu.Items)
      {
        if (item is MenuItem)
        {
          dialog.Add(((MenuItem) item).Header as string);
        }
      }

      dialog.DoModal(ParentID);

      if (dialog.SelectedId == -1)
      {
        return;
      }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_imageFocused != null)
        {
          _imageFocused.DimColor = value;
        }
        if (_imageNonFocused != null)
        {
          _imageNonFocused.DimColor = value;
        }
        if (_labelControl != null)
        {
          _labelControl.DimColor = value;
        }
      }
    }
  }
}