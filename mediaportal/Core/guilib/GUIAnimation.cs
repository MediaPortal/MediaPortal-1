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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Animation;
using MediaPortal.Drawing;
using MediaPortal.Profile;

namespace MediaPortal.GUI.Library
{
  public class GUIAnimation : GUIControl
  {
    #region Properties (Skin)

    [XMLSkinElement("Easing")] protected Easing _easing = Easing.Linear;
    [XMLSkinElement("FillBehavior")] protected FillBehavior _fillBehavior = FillBehavior.HoldEnd;

    [XMLSkinElement("HorizontalAlignment")] protected HorizontalAlignment _horizontalAlignment =
      HorizontalAlignment.Left;

    [XMLSkinElement("textures")] protected string _textureNames = string.Empty;
    [XMLSkinElement("rate")] protected double _rate = 1;
    [XMLSkinElement("Duration")] protected Duration _duration = Duration.Automatic;
    [XMLSkinElement("RepeatBehavior")] protected RepeatBehavior _repeatBehavior = RepeatBehavior.Forever;
    [XMLSkinElement("VerticalAlignment")] protected VerticalAlignment _verticalAlignment = VerticalAlignment.Top;
    [XMLSkinElement("Triggers")] protected string _triggerNames = "init";
    [XMLSkinElement("keepaspectratio")] private bool _keepAspectRatio = false;

    #endregion Properties (Skin)

    #region Fields

    private ArrayList _filenames;
    private GUIImage[] _images;
    private bool _animating = false;
    private bool _isFirstRender = true;
    private int _iterationCount = 0;
    private static int _imageId = 200000;
    private double _startTick = 0;
    private bool _hidePngAnimations = false;
    protected List<GUIMessage.MessageType> _triggerList = new List<GUIMessage.MessageType>();
    private int _renderWidth = 0;
    private int _renderHeight = 0;
    private int _textureWidth = 0;
    private int _textureHeight = 0;

    protected bool _flipX = false;
    protected bool _flipY = false;
    protected string _diffuseFileName = "";
    private string _strBorder = "";
    private string _strBorderPosition = "";
    private bool _borderTextureRepeat = false;
    private bool _borderTextureRotate = false;
    private string _borderTextureFileName = "";
    private long _borderColorKey = 0;

    #endregion Fields

    #region Properties

    public Duration Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    public Easing Easing
    {
      get { return _easing; }
      set { _easing = value; }
    }

    public ArrayList Filenames
    {
      get
      {
        if (_filenames == null)
        {
          _filenames = new ArrayList();
        }
        return _filenames;
      }
    }

    public string FileName
    {
      get { return _textureNames; }
    }

    public new HorizontalAlignment HorizontalAlignment
    {
      get { return _horizontalAlignment; }
      set { _horizontalAlignment = value; }
    }

    public RepeatBehavior RepeatBehavior
    {
      get { return _repeatBehavior; }
      set
      {
        _repeatBehavior = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].RepeatBehavior = value;
        }
      }
    }

    public new VerticalAlignment VerticalAlignment
    {
      get { return _verticalAlignment; }
      set { _verticalAlignment = value; }
    }

    public bool FlipY
    {
      get { return _flipY; }
      set { _flipY = value; }
    }

    public bool FlipX
    {
      get { return _flipX; }
      set { _flipX = value; }
    }

    public string DiffuseFileName
    {
      get { return _diffuseFileName; }
      set { _diffuseFileName = value; }
    }

    #endregion Properties

    #region Constructors

    public GUIAnimation() {}

    public GUIAnimation(int parentId)
      : base(parentId)
    {
      InitTriggerList();
    }

    public GUIAnimation(GUIAnimation a)
      : base(a._parentControlId, a._controlId, a._positionX, a._positionY, a._width, a._height)
    {
      _animating = false;
      _isFirstRender = true;
      _iterationCount = 0;
      _startTick = 0;
      _hidePngAnimations = false;
      _triggerList = a._triggerList.GetRange(0, a._triggerList.Count);
      _easing = a._easing;
      _fillBehavior = a._fillBehavior;
      _horizontalAlignment = a._horizontalAlignment;
      _textureNames = a._textureNames;
      _rate = a._rate;
      _duration = a._duration;
      _repeatBehavior = a._repeatBehavior;
      _verticalAlignment = a._verticalAlignment;
      _triggerNames = a._triggerNames;
      InitTriggerList();
    }

    public GUIAnimation(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                        string strTextureNames)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _animating = false;
      _isFirstRender = true;
      _iterationCount = 0;
      _startTick = 0;
      _hidePngAnimations = false;
      _textureNames = strTextureNames;
      InitTriggerList();
    }

    #endregion Constructors

    #region Methods

    protected void InitTriggerList()
    {
      _triggerList.Clear();
      if (_triggerNames == string.Empty)
      {
        return;
      }

      foreach (string trigger in _triggerNames.Split(';'))
      {
        switch ((trigger.Trim()).ToUpper())
        {
          case "INIT":
            _triggerList.Add(GUIMessage.MessageType.GUI_MSG_WINDOW_INIT);
            break;
          case "FOCUS":
            _triggerList.Add(GUIMessage.MessageType.GUI_MSG_SETFOCUS);
            break;
        }
      }
    }

    public void Begin()
    {
      _startTick = AnimationTimer.TickCount;
      _animating = true;
    }

    public override void AllocResources()
    {
      using (Settings xmlreader = new MPSettings())
      {
        _hidePngAnimations = (xmlreader.GetValueAsBool("general", "hidepnganimations", false));
      }

      if (_filenames == null)
      {
        _filenames = new ArrayList();

        foreach (string filename in _textureNames.Split(';'))
        {
          if (filename.IndexOfAny(new char[] {'?', '*'}) != -1)
          {
            foreach (string match in Directory.GetFiles(GUIGraphicsContext.Skin + @"\media\", filename))
            {
              _filenames.Add(Path.GetFileName(match));
            }
          }
          else
          {
            _filenames.Add(filename.Trim());
          }
        }
      }

      _images = new GUIImage[_filenames.Count];

      int w = 0;
      int h = 0;

      for (int index = 0; index < _images.Length; index++)
      {
        _imageId++;
        _images[index] = new GUIImage(ParentID, _imageId + index, 0, 0, Width, Height, (string)_filenames[index], 0);
        _images[index].ParentControl = this;
        _images[index].ColourDiffuse = ColourDiffuse;
        _images[index].DimColor = DimColor;
        _images[index].KeepAspectRatio = _keepAspectRatio;
        _images[index].Filtering = Filtering;
        _images[index].RepeatBehavior = _repeatBehavior;
        _images[index].DiffuseFileName = _diffuseFileName;
        _images[index].FlipY = _flipX;
        _images[index].FlipY = _flipY;
        _images[index].SetBorder(_strBorder, _strBorderPosition, _borderTextureRepeat, _borderTextureRotate,
                                 _borderTextureFileName, _borderColorKey);
        _images[index].AllocResources();
        //_images[index].ScaleToScreenResolution(); -> causes too big images in fullscreen

        w = Math.Max(w, _images[index].Width);
        h = Math.Max(h, _images[index].Height);
        _renderWidth = Math.Max(_renderWidth, _images[index].RenderWidth);
        _renderHeight = Math.Max(_renderHeight, _images[index].RenderHeight);
        _textureWidth = Math.Max(_textureWidth, _images[index].TextureWidth);
        _textureHeight = Math.Max(_textureHeight, _images[index].TextureHeight);
      }

      int x = _positionX;
      int y = _positionY;

      if (_horizontalAlignment == HorizontalAlignment.Center)
      {
        x = x - (w / 2);
      }
      else if (_horizontalAlignment == HorizontalAlignment.Right)
      {
        x = x - w;
      }

      if (_verticalAlignment == VerticalAlignment.Center)
      {
        y = y - (h / 2);
      }
      else if (_verticalAlignment == VerticalAlignment.Bottom)
      {
        y = y - h;
      }

      for (int index = 0; index < _images.Length; index++)
      {
        _images[index].SetPosition(x, y);
      }
    }

    public override void FreeResources()
    {
      if (_images == null)
      {
        return;
      }

      for (int index = 0; index < _images.Length; index++)
      {
        _images[index].FreeResources();
      }

      _images = null;

      if (_filenames != null)
      {
        _filenames = null;
      }
    }

    // recalculate the image dimensions & position
    public void Refresh()
    {
      if (_images == null)
      {
        return;
      }
      for (int index = 0; index < _images.Length; index++)
      {
        _images[index].Refresh();
      }
    }

    public void SetFileName(string newFilename)
    {
      FreeResources();
      _textureNames = newFilename;
      AllocResources();
    }

    public override void SetPosition(int dwPosX, int dwPosY)
    {
      base.SetPosition(dwPosX, dwPosY);

      if (_images == null)
      {
        return;
      }
      for (int index = 0; index < _images.Length; index++)
      {
        _images[index].SetPosition(dwPosX, dwPosY);
      }
    }

    public void SetBorder(string border, string position, bool textureRepeat, bool textureRotate, string textureFilename,
                          long colorKey)
    {
      _strBorder = border;
      _strBorderPosition = position;
      _borderTextureRepeat = textureRepeat;
      _borderTextureRotate = textureRotate;
      _borderTextureFileName = textureFilename;
      _borderColorKey = colorKey;
    }

    public override int Width
    {
      get { return base.Width; }
      set
      {
        base.Width = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].Width = value;
        }
      }
    }

    public override int Height
    {
      get { return base.Height; }
      set
      {
        base.Height = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].Height = value;
        }
      }
    }

    public bool Filtering
    {
      get
      {
        if ((_images != null) && (_images.Length > 0))
        {
          return _images[0].Filtering;
        }
        return false;
      }
      set
      {
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].Filtering = value;
        }
      }
    }

    /// <summary>
    /// Get/Set if the aspectratio of the texture needs to be preserved during rendering.
    /// </summary>
    public bool KeepAspectRatio
    {
      get { return _keepAspectRatio; }
      set
      {
        _keepAspectRatio = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].KeepAspectRatio = value;
        }
      }
    }


    /// <summary>
    /// Get the width in which the control is rendered.
    /// </summary>
    public int RenderWidth
    {
      get { return _renderWidth; }
    }

    /// <summary>
    /// Get the height in which the control is rendered.
    /// </summary>
    public int RenderHeight
    {
      get { return _renderHeight; }
    }

    /// <summary>
    /// Get/Set the TextureWidth
    /// </summary>
    public int TextureWidth
    {
      get { return _textureWidth; }
      set
      {
        _textureWidth = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].TextureWidth = value;
        }
      }
    }

    /// <summary>
    /// Get/Set the TextureHeight
    /// </summary>
    public int TextureHeight
    {
      get { return _textureHeight; }
      set
      {
        _textureHeight = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].TextureHeight = value;
        }
      }
    }

    public override int DimColor
    {
      get { return base.DimColor; }
      set
      {
        base.DimColor = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].DimColor = value;
        }
      }
    }

    public override long ColourDiffuse
    {
      get { return base.ColourDiffuse; }
      set
      {
        base.ColourDiffuse = value;
        if (_images == null)
        {
          return;
        }
        for (int index = 0; index < _images.Length; index++)
        {
          _images[index].ColourDiffuse = value;
        }
      }
    }

    public override bool OnMessage(GUIMessage message)
    {
      foreach (GUIMessage.MessageType triggerMsg in _triggerList)
      {
        if (triggerMsg == message.Message)
        {
          Begin();
        }
      }

      return (base.OnMessage(message));
    }

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
      if (_images == null)
      {
        base.Render(timePassed);
        return;
      }

      if (_images.Length == 0)
      {
        base.Render(timePassed);
        return;
      }

      if (_isFirstRender)
      {
        _startTick = AnimationTimer.TickCount;
        _animating = true;
        _isFirstRender = false;
      }

      if (_hidePngAnimations)
      {
        _animating = false;
      }

      double elapsedTicks = AnimationTimer.TickCount - _startTick;
      double progress = Math.Min(1, TweenHelper.Interpolate(_easing, 0, 1, _startTick, _duration));

      // determine whether we are repeating
      if (_animating && _duration < elapsedTicks)
      {
        // keep track of iterations regardless of the repeat behaviour
        _iterationCount++;

        if (_repeatBehavior.IsIterationCount && _repeatBehavior.IterationCount <= _iterationCount)
        {
          _animating = false;
        }
        else if (_repeatBehavior.IsRepeatDuration && _repeatBehavior.RepeatDuration <= elapsedTicks)
        {
          _animating = false;
        }
        if (_animating)
        {
          _startTick = AnimationTimer.TickCount;
        }
      }

      int index = _fillBehavior == FillBehavior.Stop ? 0 : _images.Length - 1;

      if (_animating && progress <= 1)
      {
        index = (int)(progress * _images.Length);
      }

      if (index >= _images.Length)
      {
        index = _images.Length - 1;
      }
      //if (_animating) _images[index].BeginAnimation();


      _images[index].Render(timePassed);
      base.Render(timePassed);
    }

    #endregion Methods
  }
}