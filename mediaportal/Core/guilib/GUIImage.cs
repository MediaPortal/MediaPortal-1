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

using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Media.Animation;
using System.Xml;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing.Imaging;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying Images.
  /// </summary>
  public class GUIImage : GUIControl
  {
    public enum FontEngineBlendMode
    {
      BLEND_NONE = 0,
      BLEND_DIFFUSE = 1,
      BLEND_OVERLAY = 2
    }

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawTexture(int textureNo, float x, float y, float nw, float nh,
                                                            float uoff, float voff, float umax, float vmax, int color,
                                                            float[,] matrix);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawTexture2(int textureNo1, float x, float y, float nw, float nh,
                                                             float uoff, float voff, float umax, float vmax, int color,
                                                             float[,] matrix, int textureNo2, float uoff2, float voff2,
                                                             float umax2, float vmax2,
                                                             FontEngineBlendMode blendMode);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawMaskedTexture(int textureNo1, float x, float y, float nw, float nh,
                                                                  float uoff, float voff, float umax, float vmax,
                                                                  int color,
                                                                  float[,] matrix, int textureNo2, float uoff2,
                                                                  float voff2,
                                                                  float umax2, float vmax2);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawMaskedTexture2(int textureNo1, float x, float y, float nw, float nh,
                                                                   float uoff, float voff, float umax, float vmax,
                                                                   int color,
                                                                   float[,] matrix, int textureNo2, float uoff2,
                                                                   float voff2,
                                                                   float umax2, float vmax2, int textureNo3, float uoff3,
                                                                   float voff3, float umax3, float vmax3,
                                                                   FontEngineBlendMode blendMode);

    /// <summary>
    /// enum to specify the border position
    /// </summary>
    public enum BorderPosition
    {
      BORDER_IMAGE_OUTSIDE,
      BORDER_IMAGE_INSIDE,
      BORDER_IMAGE_CENTER,
      BORDER_CONTROL_OUTSIDE,
      BORDER_CONTROL_INSIDE,
      BORDER_CONTROL_CENTER,

      // added to support XAML parser
      OutsideImage = BORDER_IMAGE_OUTSIDE,
      InsideImage = BORDER_IMAGE_INSIDE,
      CenterImage = BORDER_IMAGE_CENTER,
      OutsideControl = BORDER_CONTROL_OUTSIDE,
      InsideControl = BORDER_CONTROL_INSIDE,
      CenterControl = BORDER_CONTROL_CENTER
    }

    /// <summary>The width of the current texture.</summary>
    private int _textureWidth = 0;

    private int _textureHeight = 0;

    /// <summary>The width of the image containing the textures.</summary>
    private int _imageWidth = 0;

    private int _imageHeight = 0;
    private int _selectedFrameNumber = 0;
    private int m_dwItems = 0;
    private int _currentAnimationLoop = 0;
    private int _currentFrameNumber = 0;

    [XMLSkinElement("colorkey")] protected long m_dwColorKey = 0;
    [XMLSkinElement("texture")] protected string _textureFileNameTag = "";
    [XMLSkinElement("keepaspectratio")] protected bool _keepAspectRatio = false;
    [XMLSkinElement("zoom")] protected bool _zoomIn = false;
    [XMLSkinElement("zoomfromtop")] protected bool _zoomFromTop = false;
    [XMLSkinElement("fixedheight")] protected bool _isFixedHeight = false;
    [XMLSkinElement("RepeatBehavior")] protected RepeatBehavior _repeatBehavior = RepeatBehavior.Forever;
    [XMLSkin("texture", "flipX")] protected bool _flipX = false;
    [XMLSkin("texture", "flipY")] protected bool _flipY = false;
    [XMLSkin("texture", "diffuse")] protected string _diffuseFileName = "";
    [XMLSkin("texture", "overlay")] protected string _overlayFileName = "";
    [XMLSkin("texture", "mask")] protected string _maskFileName = "";
    [XMLSkinElement("filtered")] protected bool _filterImage = true;
    [XMLSkinElement("align")] protected Alignment _imageAlignment = Alignment.ALIGN_LEFT;
    [XMLSkinElement("valign")] protected VAlignment _imageVAlignment = VAlignment.ALIGN_TOP;
    [XMLSkinElement("border")] protected string _strBorder = "";
    [XMLSkin("border", "position")] protected BorderPosition _borderPosition = BorderPosition.BORDER_IMAGE_OUTSIDE;
    [XMLSkin("border", "textureRepeat")] protected bool _borderTextureRepeat = false;
    [XMLSkin("border", "textureRotate")] protected bool _borderTextureRotate = false;
    [XMLSkin("border", "texture")] protected string _borderTextureFileName = "image_border.png";
    [XMLSkin("border", "colorKey")] protected long _borderColorKey = 0xFFFFFFFF;

    [XMLSkin("border", "corners")] protected bool _borderHasCorners = false;
    // implies use of e.g., "image_border_corner.png"

    [XMLSkin("border", "cornerRotate")] protected bool _borderCornerTextureRotate = true;
    [XMLSkinElement("imagepath")] private string _imagePath = ""; // Image path used to store VUMeter files

    [XMLSkinElement("tileFill")] private bool _tileFill = false;
    // Will tile a texture to the rectangle rather than stretch it

    [XMLSkinElement("shouldCache")] private bool _shouldCache = false;
    // hint from the skin that the particular texture should be cached for perf reasons

    private int _blendableTexWidth = 0;
    private int _blendableTexHeight = 0;
    private Texture _blendableTexture = null;
    private string _blendableFileName = "";
    private int _maskTexWidth = 0;
    private int _maskTexHeight = 0;
    private Texture _maskTexture = null;
    private CachedTexture.Frame[] _listTextures = null;

    //TODO GIF PALLETTE
    //private PaletteEntry						m_pPalette=null;
    /// <summary>The width of in which the texture will be rendered after scaling texture.</summary>
    private int m_iRenderWidth = 0;

    private int m_iRenderHeight = 0;
    //private System.Drawing.Image m_image = null;
    private Rectangle m_destRect;
    private string _cachedTextureFileName = "";

    //using for debugging leaks;
    private string _debugCachedTextureFileName = "";
    private string _debugCaller = "";
    //private bool _debugDisposed = false;
    //private bool _debugAllocResourcesCalled = false;
    private Guid _debugGuid = Guid.NewGuid();


    private DateTime _animationTimer = DateTime.MinValue;
    private bool _containsProperty = false;
    private bool _propertyChanged = false;
    //    StateBlock                      savedStateBlock;
    private Rectangle sourceRect;
    private Rectangle destinationRect;
    private Vector3 pntPosition;
    private float scaleX = 1;
    private float scaleY = 1;
    private float _fx, _fy, _nw, _nh;
    private float _uoff, _voff, _umax, _vmax;

    private float _texUoff, _texVoff, _texUmax, _texVmax;
    private float _blendabletexUoff, _blendabletexVoff, _blendabletexUmax, _blendabletexVmax;
    private float _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc;
    private float _masktexUoff, _masktexVoff, _masktexUmax, _masktexVmax;
    private Texture _packedTexture = null;
    private int _packedTextureNo = -1;
    private int _packedBlendableTextureNo = -1;
    private int _packedMaskTextureNo = -1;
    private static bool logtextures = false;
    private bool _isFullScreenImage = false;
    private bool _reCalculate = false;
    private bool _allocated = false;
    private bool _registeredForEvent = false;
    private FontEngineBlendMode _blendMode = FontEngineBlendMode.BLEND_DIFFUSE;

    private object _lockingObject = new object();

    private int _borderLeft = 0;
    private int _borderRight = 0;
    private int _borderTop = 0;
    private int _borderBottom = 0;

    private int _memoryImageWidth = 0;
    private int _memoryImageHeight = 0;
    private Texture _memoryImageTexture;

    private Image _memoryImage = null;

    private GUIImage() {}

    public GUIImage(int dwParentID)
      : base(dwParentID) {}

    public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                    string strTexture, Color color)
      : this(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTexture, color.ToArgb()) {}

    public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                    string strTexture, Color color, int[] border, int strBorderPosition, bool borderTextureRotate,
                    bool borderTextureRepeat, Color borderColor)
      : this(
        dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTexture, color.ToArgb(), border,
        strBorderPosition, borderTextureRepeat, borderTextureRotate, borderColor.ToArgb()) {}

    public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                    string strTexture, long dwColorKey, int[] border, int strBorderPosition, bool borderTextureRepeat,
                    bool borderTextureRotate, long dwBorderColorKey)
      : this(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight, strTexture, dwColorKey)
    {
      // Border array parameter order is left,right,top,bottom.
      if (border.Length >= 4)
      {
        _borderLeft = border[0];
        _borderRight = border[1];
        _borderTop = border[2];
        _borderBottom = border[3];
      }
      else if (border.Length >= 1)
      {
        _borderLeft = border[0];
        _borderRight = border[0];
        _borderTop = border[0];
        _borderBottom = border[0];
      }

      _borderTextureRepeat = borderTextureRepeat;
      _borderTextureRotate = borderTextureRotate;
      _borderColorKey = dwBorderColorKey;
    }

    /// <summary>
    /// The constructor of the GUIImage class.
    /// </summary>
    /// <param name="dwParentID">The parent of this GUIImage control.</param>
    /// <param name="dwControlId">The ID of this GUIImage control.</param>
    /// <param name="dwPosX">The X position of this GUIImage control.</param>
    /// <param name="dwPosY">The Y position of this GUIImage control.</param>
    /// <param name="dwWidth">The width of this GUIImage control.</param>
    /// <param name="dwHeight">The height of this GUIImage control.</param>
    /// <param name="strTexture">The filename of the texture of this GUIImage control.</param>
    /// <param name="dwColorKey">The color that indicates transparancy.</param>
    public GUIImage(int dwParentID, int dwControlId, int dwPosX, int dwPosY, int dwWidth, int dwHeight,
                    string strTexture, long dwColorKey)
      : base(dwParentID, dwControlId, dwPosX, dwPosY, dwWidth, dwHeight)
    {
      _diffuseColor = 0xFFFFFFFF;
      _textureFileNameTag = strTexture;
      _textureWidth = 0;
      _textureHeight = 0;
      m_dwColorKey = dwColorKey;
      _selectedFrameNumber = 0;

      _currentFrameNumber = 0;
      _keepAspectRatio = false;
      _zoomIn = false;
      _currentAnimationLoop = 0;
      _imageWidth = 0;
      _imageHeight = 0;
      FinalizeConstruction();
    }

    public override void UpdateVisibility()
    {
      base.UpdateVisibility();

      // check for conditional information before we free and
      // alloc as this does free and allocation as well
      if (Info.Count == 1)
      {
        SetFileName(GUIInfoManager.GetImage(Info[0], (uint)ParentID));
      }
    }

    public Image MemoryImage
    {
      get { return _memoryImage; }
      set { _memoryImage = value; }
    }

    /// <summary>
    /// Does any scaling on the inital size\position values to fit them to screen 
    /// resolution. 
    /// </summary>
    public override void ScaleToScreenResolution()
    {
      if (_textureFileNameTag == null)
      {
        _textureFileNameTag = string.Empty;
      }
      if (_textureFileNameTag != "-" && _textureFileNameTag != "")
      {
        if (_width == 0 || _height == 0)
        {
          try
          {
            string strFileNameTemp = "";

            if (!MediaPortal.Util.Utils.FileExistsInCache(_textureFileNameTag))
            {
              if (_textureFileNameTag[1] != ':')
              {
                strFileNameTemp = GUIGraphicsContext.GetThemedSkinFile(@"\media\" + _textureFileNameTag);
              }
            }

            if (strFileNameTemp.Length > 0 && strFileNameTemp.IndexOf(@"\#") != -1)
            {
              return;
            }

            using (Image img = Image.FromFile(strFileNameTemp))
            {
              if (0 == _width)
              {
                _width = img.Width;
              }
              if (0 == _height)
              {
                _height = img.Height;
              }
            }
          }
          catch (Exception) {}
        }
      }
      base.ScaleToScreenResolution();
    }

    /// <summary> 
    /// This function is called after all of the XmlSkinnable fields have been filled
    /// with appropriate data.
    /// Use this to do any construction work other than simple data member assignments,
    /// for example, initializing new reference types, extra calculations, etc..
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

      m_dwItems = 1;

      m_iRenderWidth = _width;
      m_iRenderHeight = _height;
      if (_textureFileNameTag != null && _textureFileNameTag.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }

      // Choose one of the blendable files to use.  Specifying more than one of the optional filenames results in unpredictable behavior.
      if (_diffuseFileName.Length > 0)
      {
        DiffuseFileName = _diffuseFileName;
      }
      else if (_overlayFileName.Length > 0)
      {
        OverlayFileName = _overlayFileName;
      }

      FinalizeBorder();
    }

    private void FinalizeBorder()
    {
      // Set the border sizes for user specified values (overrides the default values).
      _strBorder = _strBorder.Trim();
      if (!"".Equals(_strBorder))
      {
        int[] valueParameters = ParseParameters(_strBorder);

        // The user may specify either all four values in the order left,right,top,bottom or a single value that will
        // be used for all four sides.
        if (valueParameters.Length >= 4)
        {
          _borderLeft = valueParameters[0];
          _borderRight = valueParameters[1];
          _borderTop = valueParameters[2];
          _borderBottom = valueParameters[3];
        }
        else if (valueParameters.Length >= 1)
        {
          _borderLeft = valueParameters[0];
          _borderRight = valueParameters[0];
          _borderTop = valueParameters[0];
          _borderBottom = valueParameters[0];
        }
      }
    }

    private static int[] ParseParameters(string valueText)
    {
      if ("".Equals(valueText))
      {
        return new int[0];
      }

      try
      {
        ArrayList valuesTemp = new ArrayList();

        foreach (string token in valueText.Split(new char[] {',', ' '}))
        {
          if (token == string.Empty)
          {
            continue;
          }
          valuesTemp.Add(int.Parse(token));
        }

        int[] values = new int[valuesTemp.Count];
        Array.Copy(valuesTemp.ToArray(), values, values.Length);

        return values;
      }
      catch {}

      return new int[0];
    }

    /// <summary>
    /// Get/Set the TextureWidth
    /// </summary>
    public int TextureWidth
    {
      get { return _textureWidth; }
      set
      {
        if (value < 0 || value == _textureWidth)
        {
          return;
        }
        _textureWidth = value;
        _reCalculate = true;
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
        if (value < 0 || value == _textureHeight)
        {
          return;
        }
        _textureHeight = value;
        _reCalculate = true;
      }
    }

    /// <summary>
    /// Get the filename of the texture.
    /// </summary>
    public string FileName
    {
      get { return _textureFileNameTag; }
      set { SetFileName(value); }
    }

    /// <summary>
    /// Returns the Imagepath for the Control
    /// </summary>
    public string ImagePath
    {
      get { return _imagePath; }
      set { _imagePath = value; }
    }

    /// <summary>
    /// Get the transparent color.
    /// </summary>
    public long ColorKey
    {
      get { return m_dwColorKey; }
      set
      {
        if (m_dwColorKey != value)
        {
          m_dwColorKey = value;
          _reCalculate = true;
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
        if (_keepAspectRatio != value)
        {
          _keepAspectRatio = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Get the width in which the control is rendered.
    /// </summary>
    public int RenderWidth
    {
      get { return m_iRenderWidth; }
    }

    /// <summary>
    /// Get the height in which the control is rendered.
    /// </summary>
    public int RenderHeight
    {
      get { return m_iRenderHeight; }
    }

    /// <summary>
    /// Returns if the control can have the focus.
    /// </summary>
    /// <returns>False</returns>
    public override bool CanFocus()
    {
      return false;
    }

    /// <summary>
    /// If the texture holds more then 1 frame (like an animated gif)
    /// then you can select the current frame with this method
    /// </summary>
    /// <param name="iBitmap"></param>
    public void Select(int frameNumber)
    {
      if (_selectedFrameNumber == frameNumber)
      {
        return;
      }
      _selectedFrameNumber = frameNumber;
      _reCalculate = true;
    }

    /// <summary>
    /// If the texture has more then 1 frame like an animated gif then
    /// you can specify the max# of frames to play with this method
    /// </summary>
    /// <param name="iItems"></param>
    public void SetItems(int iItems)
    {
      m_dwItems = iItems;
    }

    public void BeginAnimation()
    {
      _currentAnimationLoop = 0;
      _currentFrameNumber = 0;
    }

    public bool AnimationRunning
    {
      get
      {
        if (_listTextures == null)
        {
          return false;
        }
        if (_listTextures.Length <= 1)
        {
          return false;
        }
        if (_currentFrameNumber + 1 >= _listTextures.Length)
        {
          return false;
        }
        return true;
      }
    }

    /// <summary>
    /// This function will do the animation (when texture is an animated gif)
    /// by switching from frame 1->frame2->frame 3->...
    /// </summary>
    protected void Animate()
    {
      if (_listTextures == null)
      {
        return;
      }
      // If the number of textures that correspond to this control is lower than or equal to 1 do not change the texture.
      if (_listTextures.Length <= 1)
      {
        _currentFrameNumber = 0;
        return;
      }

      if (_currentFrameNumber >= _listTextures.Length)
      {
        _currentFrameNumber = 0;
      }

      CachedTexture.Frame frame = _listTextures[_currentFrameNumber];
      // Check the delay.
      int dwDelay = 0;
      if (frame != null)
      {
        dwDelay = frame.Duration;
      }
      //int iMaxLoops = 0;
      frame = null;

      // Default delay = 100;
      if (0 == dwDelay)
      {
        dwDelay = 100;
      }

      TimeSpan ts = DateTime.Now - _animationTimer;
      if (ts.TotalMilliseconds > dwDelay)
      {
        _animationTimer = DateTime.Now;

        // Reset the current image
        if (_currentFrameNumber + 1 >= _listTextures.Length)
        {
          // Check if another loop is required
          if (RepeatBehavior.IterationCount > 0)
          {
            // Go to the next loop
            if (_currentAnimationLoop + 1 < RepeatBehavior.IterationCount)
            {
              _currentAnimationLoop++;
              _currentFrameNumber = 0;
            }
          }
          else
          {
            // 0 == loop forever
            _currentFrameNumber = 0;
          }
        }
          // Switch to the next image.
        else
        {
          _currentFrameNumber++;
        }
      }
    }

    /// <summary>
    /// Allocate the DirectX resources needed for rendering this GUIImage.
    /// </summary>
    public override void AllocResources()
    {
      //used for debugging leaks, comment in when needed-.
      /*_debugAllocResourcesCalled = true;
      _debugCachedTextureFileName = _textureFileNameTag;
      _debugCaller = ""; //  System.Environment.StackTrace.ToString();
      */
      try
      {
        if (GUIGraphicsContext.DX9Device == null)
        {
          return;
        }
        if (GUIGraphicsContext.DX9Device.Disposed)
        {
          return;
        }
        if (string.IsNullOrEmpty(_textureFileNameTag))
        {
          return;
        }

        if (_registeredForEvent == false && _containsProperty)
        {
          GUIPropertyManager.OnPropertyChanged -=
            new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
          GUIPropertyManager.OnPropertyChanged +=
            new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
          _registeredForEvent = true;
        }
        _propertyChanged = false;

        //reset animation
        BeginAnimation();

        _listTextures = null;

        string textureFiles = _textureFileNameTag;
        if (textureFiles.ToUpper().Contains(".XML"))
        {
          LoadAnimation(ref textureFiles);
        }
        if (_blendableFileName != "")
        {
          if (GUITextureManager.GetPackedTexture(_blendableFileName, out _blendabletexUoff, out _blendabletexVoff,
                                                 out _blendabletexUmax, out _blendabletexVmax, out _blendableTexWidth,
                                                 out _blendableTexHeight, out _blendableTexture, out _packedBlendableTextureNo))
          {
            _reCalculate = true;
          }
        }

        foreach (string file in textureFiles.Split(';'))
        {
          //get the filename of the texture
          string fileName = file;
          if (_containsProperty)
          {
            fileName = _cachedTextureFileName = GUIPropertyManager.Parse(file);
          }
          if (fileName.Length == 0)
          {
            continue;
          }

          if (logtextures)
          {
            Log.Info("GUIImage:AllocResources:{0} {1}", fileName, _debugGuid);
            Log.Info("stacktrace: {0} ", _debugCaller);
          }
          if (GUITextureManager.GetPackedTexture(fileName, out _texUoff, out _texVoff, out _texUmax, out _texVmax,
                                                 out _textureWidth, out _textureHeight, out _packedTexture,
                                                 out _packedTextureNo))
          {
            _reCalculate = true;
            _packedTexture.Disposing -= new EventHandler(OnPackedTexturesDisposedEvent);
            _packedTexture.Disposing += new EventHandler(OnPackedTexturesDisposedEvent);
            return;
          }

          //load the texture
          int frameCount = 0;
          if (fileName.StartsWith("["))
          {
            if (_memoryImageWidth != 0 && _memoryImageHeight != 0)
            {
              Bitmap bitmap = new Bitmap(_memoryImageWidth, _memoryImageHeight, PixelFormat.Format32bppArgb);
              using (Image memoryImage = bitmap)
              {
                frameCount = GUITextureManager.LoadFromMemoryEx(memoryImage, fileName, m_dwColorKey,
                                                                out _memoryImageTexture);
              }
            }
            else
              frameCount = GUITextureManager.LoadFromMemoryEx(_memoryImage, fileName, m_dwColorKey,
                                                              out _memoryImageTexture);

            if (0 == frameCount)
            {
              continue; // unable to load texture
            }
          }
          else
          {
            //Log.Info("load:{0}", fileName);
            frameCount = GUITextureManager.Load(fileName, m_dwColorKey, m_iRenderWidth, _textureHeight, _shouldCache);
            if (0 == frameCount)
            {
              continue; // unable to load texture
            }
          }
          //get each frame of the texture
          int iStartCopy = 0;
          CachedTexture.Frame[] _saveList = null;
          if (_listTextures == null)
          {
            _listTextures = new CachedTexture.Frame[frameCount];
          }
          else
          {
            int newLength = _listTextures.Length + frameCount;
            iStartCopy = _listTextures.Length;
            CachedTexture.Frame[] _newList = new CachedTexture.Frame[newLength];
            _saveList = new CachedTexture.Frame[_listTextures.Length];
            _listTextures.CopyTo(_saveList, 0);
            _listTextures.CopyTo(_newList, 0);
            _listTextures = new CachedTexture.Frame[newLength];
            _newList.CopyTo(_listTextures, 0);
          }
          for (int i = 0; i < frameCount; i++)
          {
            _listTextures[i + iStartCopy] = GUITextureManager.GetTexture(fileName, i, out _textureWidth,
                                                                         out _textureHeight); //,m_pPalette);
            if (_listTextures[i + iStartCopy] != null)
            {
              _listTextures[i + iStartCopy].Disposed += new EventHandler(OnListTexturesDisposedEvent);
            }
            else
            {
              Log.Debug("GUIImage.AllocResources -> Filename = (" + fileName + ") i=" + i.ToString() + " FrameCount=" +
                        frameCount.ToString());
              if (_saveList != null)
              {
                _listTextures = new CachedTexture.Frame[_saveList.Length];
                _saveList.CopyTo(_listTextures, 0);
              }
              else
              {
                UnsubscribeAndReleaseListTextures();
              }
              _currentFrameNumber = 0;
              break;
            }
          }
        }
        // Set state to render the image
        _reCalculate = true;
        base.AllocResources();
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
      finally
      {
        _allocated = true;
      }
    }

    private void OnPackedTexturesDisposedEvent(object sender, EventArgs e)
    {
      if (_packedTexture == (Texture)sender)
      {
        _packedTexture.Disposing -= new EventHandler(OnPackedTexturesDisposedEvent);
        _packedTexture.Dispose();
        _packedTexture = null;
      }
      if (!App.IsShuttingDown)
        // if the app is shutting down, this is useless and causes huge delays in case many images have been allocated
        UnsubscribeOnPropertyChanged();
    }

    private void OnListTexturesDisposedEvent(object sender, EventArgs e)
    {
      if (_listTextures == null)
      {
        return;
      }
      if (sender == null)
      {
        return;
      }
      for (int i = 0; i < _listTextures.Length; ++i)
      {
        if (_listTextures[i] == sender)
        {
          _listTextures[i].Disposed -= new EventHandler(OnListTexturesDisposedEvent);
          _listTextures[i].SafeDispose();
          _listTextures[i] = null;
        }
      }
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (!_containsProperty)
      {
        return;
      }
      if (_textureFileNameTag.IndexOf(tag) >= 0)
      {
        _propertyChanged = true;
      }
      string lockid = GUIPropertyManager.Parse(_textureFileNameTag);
      if ((_lockingObject = GUITextureManager.GetCachedTexture(lockid)) == null)
      {
        _lockingObject = new object();
      }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void FreeResourcesAndRegEvent()
    {
      Dispose();
      if (_registeredForEvent == false && _containsProperty)
      {
        GUIPropertyManager.OnPropertyChanged -=
          new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
        GUIPropertyManager.OnPropertyChanged +=
          new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
        _registeredForEvent = true;
      }
    }

    /// <summary>
    /// Free the DirectX resources needed for rendering this GUIImage.
    /// </summary>
    public override void Dispose()
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        lock (this)
        {
          if (!_allocated)
          {
            return;
          }

          if (logtextures)
          {
            Log.Info("GUIImage:Dispose:{0} {1}", _debugCachedTextureFileName, _debugGuid);
          }

          //base.Dispose(); // breaks fade in/out animations-.
          _allocated = false;
          UnsubscribeOnPropertyChanged();
          UnsubscribeAndReleaseListTextures();
          Cleanup();

          _memoryImage.SafeDispose();
          _memoryImageTexture = null;
          //_debugDisposed = true;   
        }
      }
    }

    private void UnsubscribeOnPropertyChanged()
    {
      GUIPropertyManager.OnPropertyChanged -=
        new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
      _registeredForEvent = false;
    }

    private void UnsubscribeListTextures()
    {
      if (_listTextures != null)
      {
        for (int i = 0; i < _listTextures.Length; ++i)
        {
          CachedTexture.Frame frame = _listTextures[i];
          if (frame != null)
          {
            frame.Disposed -= new EventHandler(OnListTexturesDisposedEvent);
          }
        }
      }
      _listTextures = null;
    }

    private void UnsubscribeAndReleaseListTextures()
    {
      if (_listTextures != null)
      {
        for (int i = 0; i < _listTextures.Length; ++i)
        {
          CachedTexture.Frame frame = _listTextures[i];
          if (frame != null)
          {
            frame.Disposed -= new EventHandler(OnListTexturesDisposedEvent);
            ReleaseTexture(frame.ImageName, frame.Image);
          }
        }
      }
      _listTextures = null;
    }

    private void ReleaseTexture(string file, Texture texture)
    {
      if (!string.IsNullOrEmpty(file))
      {
        if (logtextures)
        {
          Log.Debug("GUIImage: Dispose - {0}", file);
        }
        if (GUITextureManager.IsTemporary(file))
        {
          texture = null;
          GUITextureManager.ReleaseTexture(file);
        }
      }
    }

    private void Cleanup()
    {
      _cachedTextureFileName = "";
      //m_image = null;
      UnsubscribeListTextures();
      if (_packedTexture != null)
      {
        _packedTexture.Disposing -= new EventHandler(OnPackedTexturesDisposedEvent);
      }

      _currentFrameNumber = 0;
      _currentAnimationLoop = 0;
      _imageWidth = 0;
      _imageHeight = 0;
      _textureWidth = 0;
      _textureHeight = 0;
      _allocated = false;

      ReleaseTexture(_cachedTextureFileName, _packedTexture);
      ReleaseTexture(_blendableFileName, _blendableTexture);

      _packedBlendableTextureNo = -1;
      _packedTexture = null;
      _blendableTexture = null;
    }

    /// <summary>
    /// Sets the state to render the image
    /// </summary>
    protected void Calculate()
    {
      _reCalculate = false;
      float x = (float)_positionX;
      float y = (float)_positionY;
      if (_packedTexture != null)
      {
        if (0 == _imageWidth || 0 == _imageHeight)
        {
          _imageWidth = _textureWidth;
          _imageHeight = _textureHeight;
        }
      }
      else
      {
        if (_listTextures == null)
        {
          return;
        }
        if (_listTextures.Length == 0)
        {
          return;
        }
        if (_currentFrameNumber < 0 || _currentFrameNumber >= _listTextures.Length)
        {
          return;
        }

        CachedTexture.Frame frame = _listTextures[_currentFrameNumber];
        if (frame == null)
        {
          Cleanup();
          AllocResources();
          if (_listTextures == null || _listTextures.Length < 1)
          {
            return;
          }
          frame = _listTextures[_currentFrameNumber];
          if (frame == null)
          {
            return;
          }
        }
        Texture texture = frame.Image;
        frame = null;
        if (texture == null)
        {
          //no texture? then nothing todo
          return;
        }

        // if texture is disposed then free its resources and return
        if (texture.Disposed)
        {
          texture = null;
          FreeResourcesAndRegEvent();
          texture = null;
          return;
        }

        // on first run, get the image width/height of the texture
        if (0 == _imageWidth || 0 == _imageHeight)
        {
          SurfaceDescription desc;
          desc = texture.GetLevelDescription(0);
          _imageWidth = desc.Width;
          _imageHeight = desc.Height;
        }
        texture = null;
      }

      // Calculate the _textureWidth and _textureHeight 
      // based on the _imageWidth and _imageHeight
      if (0 == _textureWidth || 0 == _textureHeight)
      {
        _textureWidth = (int)Math.Round(((float)_imageWidth) / ((float)m_dwItems));
        _textureHeight = _imageHeight;

        if (_textureHeight > (int)GUIGraphicsContext.Height)
        {
          _textureHeight = (int)GUIGraphicsContext.Height;
        }

        if (_textureWidth > (int)GUIGraphicsContext.Width)
        {
          _textureWidth = (int)GUIGraphicsContext.Width;
        }
      }

      // If there are multiple frames in the GUIImage thne the e _textureWidth is equal to the _width
      if (_width > 0 && m_dwItems > 1)
      {
        _textureWidth = (int)_width;
      }

      // Initialize the with of the control based on the texture width
      if (_width == 0)
      {
        _width = _textureWidth;
      }

      // Initialize the height of the control based on the texture height
      if (_height == 0)
      {
        _height = _textureHeight;
      }


      float nw = (float)_width;
      float nh = (float)_height;

      //adjust image based on current aspect ratio setting
      float fSourceFrameRatio = 1;
      float fOutputFrameRatio = 1;
      if (!_zoomIn && !_zoomFromTop && _keepAspectRatio && _textureWidth != 0 && _textureHeight != 0)
      {
        // TODO: remove or complete HDTV_1080i code
        //int iResolution=g_stSettings.m_ScreenResolution;
        fSourceFrameRatio = ((float)_textureWidth) / ((float)_textureHeight);
        fOutputFrameRatio = fSourceFrameRatio / GUIGraphicsContext.PixelRatio;
        //if (iResolution == HDTV_1080i) fOutputFrameRatio *= 2;

        // maximize the thumbnails width
        float fNewWidth = (float)_width;
        float fNewHeight = fNewWidth / fOutputFrameRatio;

        // make sure the height is not larger than the maximum
        if (fNewHeight > _height)
        {
          fNewHeight = (float)_height;
          fNewWidth = fNewHeight * fOutputFrameRatio;
        }
        // this shouldnt happen, but just make sure that everything still fits onscreen
        if (fNewWidth > _width || fNewHeight > _height)
        {
          fNewWidth = (float)_width;
          fNewHeight = (float)_height;
        }
        nw = fNewWidth;
        nh = fNewHeight;
      }

      // set the width/height the image gets rendererd
      m_iRenderWidth = (int)Math.Round(nw);
      m_iRenderHeight = (int)Math.Round(nh);

      // if necessary then align the image 
      // in the controls rectangle
      if (_imageAlignment == Alignment.ALIGN_CENTER)
      {
        x += ((((float)_width) - nw) / 2.0f);
      }
      else if (_imageAlignment == Alignment.ALIGN_RIGHT)
      {
        x += ((float)_width - nw);
      }

      if (_imageVAlignment == VAlignment.ALIGN_MIDDLE)
      {
        y += ((((float)_height) - nh) / 2.0f);
      }
      else if (_imageVAlignment == VAlignment.ALIGN_BOTTOM)
      {
        y += ((float)_height - nh);
      }

      // Calculate source Texture
      int iSourceX = 0;
      int iSourceY = 0;
      int iSourceWidth = _textureWidth;
      int iSourceHeight = _textureHeight;

      if ((_zoomIn || _zoomFromTop) && _keepAspectRatio)
      {
        fSourceFrameRatio = ((float)nw) / ((float)nh);
        fOutputFrameRatio = fSourceFrameRatio * GUIGraphicsContext.PixelRatio;

        if (((float)iSourceWidth / (nw * GUIGraphicsContext.PixelRatio)) < ((float)iSourceHeight / nh))
        {
          //Calc height
          iSourceHeight = (int)((float)iSourceWidth / fOutputFrameRatio);
          if (iSourceHeight > _textureHeight)
          {
            iSourceHeight = _textureHeight;
            iSourceWidth = (int)((float)iSourceHeight * fOutputFrameRatio);
          }
        }
        else
        {
          //Calc width
          iSourceWidth = (int)((float)iSourceHeight * fOutputFrameRatio);
          if (iSourceWidth > _textureWidth)
          {
            iSourceWidth = _textureWidth;
            iSourceHeight = (int)((float)iSourceWidth / fOutputFrameRatio);
          }
        }

        if (!_zoomFromTop)
        {
          iSourceY = (_textureHeight - iSourceHeight) / 2;
        }
        iSourceX = (_textureWidth - iSourceWidth) / 2;
      }

      if (_isFixedHeight)
      {
        y = (float)_positionY;
        nh = (float)_height;
      }

      // check and compensate image
      if (x < 0)
      {
        // calc percentage offset
        iSourceX -= (int)((float)_textureWidth * (x / nw));
        iSourceWidth += (int)((float)_textureWidth * (x / nw));

        nw += x;
        x = 0;
      }
      if (y < 0)
      {
        iSourceY -= (int)((float)_textureHeight * (y / nh));
        iSourceHeight += (int)((float)_textureHeight * (y / nh));

        nh += y;
        y = 0;
      }
      int outWidth = GUIGraphicsContext.Width - GUIGraphicsContext.OffsetX;
      int outHeight = GUIGraphicsContext.Height - GUIGraphicsContext.OffsetY;
      if (x > outWidth)
      {
        x = outWidth;
      }
      if (y > outHeight)
      {
        y = outHeight;
      }

      if (nw < 0)
      {
        nw = 0;
      }
      if (nh < 0)
      {
        nh = 0;
      }
      if (x + nw > outWidth)
      {
        iSourceWidth = (int)((float)_textureWidth * (((float)outWidth - x) / nw));
        nw = outWidth - x;
      }
      if (y + nh > outHeight)
      {
        iSourceHeight = (int)((float)_textureHeight * (((float)outHeight - y) / nh));
        nh = outHeight - y;
      }

      // copy all coordinates to the vertex buffer
      // x-offset in texture
      float uoffs = ((float)(_selectedFrameNumber * _width + iSourceX)) / ((float)_imageWidth);

      // y-offset in texture
      float voffs = ((float)iSourceY) / ((float)_imageHeight);

      // width copied from texture
      float u = ((float)iSourceWidth) / ((float)_imageWidth);

      // height copied from texture
      float v = ((float)iSourceHeight) / ((float)_imageHeight);


      if (uoffs < 0 || uoffs > 1)
      {
        uoffs = 0;
      }
      if (u < 0 || u > 1)
      {
        u = 1;
      }
      if (v < 0 || v > 1)
      {
        v = 1;
      }
      if (u + uoffs > 1)
      {
        uoffs = 0;
        u = 1;
      }

      _fx = x;
      _fy = y;
      _nw = nw;
      _nh = nh;

      _uoff = uoffs;
      _voff = voffs;
      _umax = u;
      _vmax = v;

      if (_packedTexture != null)
      {
        _uoff = _texUoff + (uoffs * _texUmax);
        _voff = _texVoff + (voffs * _texVmax);
        _umax = _umax * _texUmax;
        _vmax = _vmax * _texVmax;
      }

      pntPosition = new Vector3(x, y, 0);
      sourceRect = new Rectangle(_selectedFrameNumber * _width + iSourceX, iSourceY, iSourceWidth, iSourceHeight);
      destinationRect = new Rectangle(0, 0, (int)nw, (int)nh);
      m_destRect = new Rectangle((int)x, (int)y, (int)nw, (int)nh);

      scaleX = (float)destinationRect.Width / (float)iSourceWidth;
      scaleY = (float)destinationRect.Height / (float)iSourceHeight;
      pntPosition.X /= scaleX;
      pntPosition.Y /= scaleY;

      _isFullScreenImage = false;
      if (m_iRenderWidth == Math.Round((float)GUIGraphicsContext.Width * GUIGraphicsContext.ZoomHorizontal)
          && m_iRenderHeight == Math.Round((float)GUIGraphicsContext.Height * GUIGraphicsContext.ZoomVertical))
      {
        _isFullScreenImage = true;
      }
    }

    public void RenderRect(float timePassed, Rectangle rectSrc, Rectangle rectDst)
    {
      _fx = rectDst.Left;
      _fy = rectDst.Top;
      _nw = rectDst.Width;
      _nh = rectDst.Height;
      float uoffs = ((float)(rectSrc.Left)) / ((float)(_textureWidth));
      float voffs = ((float)(rectSrc.Top)) / ((float)(_textureHeight));
      _umax = ((float)(rectSrc.Width)) / ((float)(_textureWidth));
      _vmax = ((float)(rectSrc.Height)) / ((float)(_textureHeight));

      if (_packedTexture != null)
      {
        _uoff = _texUoff + (uoffs * _texUmax);
        _voff = _texVoff + (voffs * _texVmax);
        _umax = _umax * _texUmax;
        _vmax = _vmax * _texVmax;
      }
      Render(timePassed);
      base.Render(timePassed);
    }

    public override void GetCenter(ref float centerX, ref float centerY)
    {
      if (_reCalculate)
      {
        Calculate();
      }
      centerX = (float)(_fx + (_nw / 2));
      centerY = (float)(_fy + (_nh / 2));
    }

    /// <summary>
    /// Renders the Image
    /// </summary>
    public override void Render(float timePassed)
    {
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }
      if (!GUIGraphicsContext.ShowBackground && _isFullScreenImage)
      {
        base.Render(timePassed);
        return;
      }

      // if this image is managed by the GUITextureManager then lock the cached texture
      // to prevent it from being unloaded in another thread while we try to render
      lock (_lockingObject)
      {
        if (_containsProperty && _propertyChanged)
        {
          _propertyChanged = false;
          string fileName = GUIPropertyManager.Parse(_textureFileNameTag);

          // if value changed or if we dont got any textures yet
          if (_cachedTextureFileName != fileName || _listTextures == null || 0 == _listTextures.Length)
          {
            // then free our resources, and reload the (new) image
            if (logtextures)
            {
              Log.Debug("GUIImage:PreRender() image changed:{0}->{1}", _cachedTextureFileName, fileName);
            }
            FreeResourcesAndRegEvent();
            _cachedTextureFileName = fileName;
            if (fileName.Length == 0)
            {
              // filename for new image is empty
              // no need to load it
              base.Render(timePassed);
              return;
            }
            //IsVisible = true;
            AllocResources();
            _reCalculate = true;
          }
        }
        if (!_allocated)
        {
          base.Render(timePassed);
          return;
        }

        if (_reCalculate)
        {
          Calculate();
        }


        try
        {
          if (!CalibrationEnabled)
          {
            GUIGraphicsContext.BypassUICalibration(true);
          }
          //get the current frame
          if (_packedTexture != null)
          {
            uint color = (uint)_diffuseColor;
            if (Dimmed)
            {
              color = (uint)(_diffuseColor & DimColor);
            }
            color = GUIGraphicsContext.MergeAlpha(color);
            float[,] matrix = GUIGraphicsContext.GetFinalMatrix();

            if (_tileFill)
            {
              // Get a texture from the texture file.
              if (GUITextureManager.Load(_textureFileNameTag, m_dwColorKey, -1, -1, true) == 0)
              {
                return; // Could not load the texture.
              }

              // Get the texture and scale its size for the screen.
              int tW;
              int tH;
              CachedTexture.Frame texture = GUITextureManager.GetTexture(_textureFileNameTag, 0, out tW, out tH);
              GUIGraphicsContext.ScaleHorizontal(ref tW);
              GUIGraphicsContext.ScaleVertical(ref tH);

              // Find the next nearest texture size (a power of 2 square).
              int x = Math.Max(tW, tH);
              --x;
              x |= x >> 1;
              x |= x >> 2;
              x |= x >> 4;
              x |= x >> 8;
              x |= x >> 16;
              ++x;

              // Compute the number of textures to draw in the control and draw the texture.
              float umax = _nw / x;
              float vmax = _nh / x;
              texture.Draw(_fx, _fy, _nw, _nh, 0, 0, umax, vmax, (int)color);
            }
            else
            {
              if (_maskFileName.Length > 0)
              {
                // Draw the image texture with the alpha-mask.
                if (_packedMaskTextureNo < 0)
                {
                  GUITextureManager.GetPackedTexture(_maskFileName, out _masktexUoff, out _masktexVoff,
                                                     out _masktexUmax, out _masktexVmax, out _maskTexWidth,
                                                     out _maskTexHeight, out _maskTexture,
                                                     out _packedMaskTextureNo);
                }
                if (_packedMaskTextureNo >= 0)
                {
                  float uoff, voff, umax, vmax, uoffm, voffm, umaxm, vmaxm;
                  uoff = _uoff;
                  voff = _voff;
                  umax = _umax + _uoff;
                  vmax = _vmax + _voff;
                  uoffm = _masktexUoff;
                  voffm = _masktexVoff;
                  umaxm = _masktexUmax + _masktexUoff;
                  vmaxm = _masktexVmax + _masktexVoff;

                  FontEngineDrawMaskedTexture(_packedTextureNo, _fx, _fy, _nw, _nh, uoff, voff, umax, vmax,
                                              (int)color, matrix,
                                              _packedMaskTextureNo, uoffm, voffm, umaxm, vmaxm);
                }
              }
              else
              {
                // Default behavior, draw the image texture with no mask.
                FontEngineDrawTexture(_packedTextureNo, _fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)color,
                                    matrix);
              }
            }

//            if ((_flipX || _flipY) && _diffuseFileName.Length > 0)
            if (_blendableFileName.Length > 0)
            {
              if (_packedBlendableTextureNo < 0)
              {
                if (GUITextureManager.GetPackedTexture(_blendableFileName, out _blendabletexUoff, out _blendabletexVoff,
                                                       out _blendabletexUmax, out _blendabletexVmax, out _blendableTexWidth,
                                                       out _blendableTexHeight, out _blendableTexture,
                                                       out _packedBlendableTextureNo)) {}
              }
              if (_packedBlendableTextureNo >= 0)
              {
                float fx, fy, nw, nh, uoff, voff, umax, vmax, uoff1, voff1, umax1, vmax1;
                fx = _fx;
                fy = _fy;
                nw = _nw;
                nh = _nh;
                uoff = _blendabletexUoff;
                voff = _blendabletexVoff;
                umax = _blendabletexUmax + _blendabletexUoff;
                vmax = _blendabletexVmax + _blendabletexVoff;
                uoff1 = _uoff;
                voff1 = _voff;
                umax1 = _umax + _uoff;
                vmax1 = _vmax + _voff;

                if (_flipX)
                {
                  fx += nw + _borderRight;
                  uoff1 = _umax + _uoff;
                  umax1 = _uoff;

                  uoff = _blendabletexUmax + _blendabletexUoff;
                  umax = _blendabletexUoff;
                }

                if (_flipY)
                {
                  fy += nh + _borderBottom;
                  voff1 = _vmax + _voff;
                  vmax1 = _voff;

                  voff = _blendabletexVmax + _blendabletexVoff;
                  vmax = _blendabletexVoff;
                }

                // Set final coordinates for diffuse texture.
                _blendabletexUoffCalc = uoff;
                _blendabletexUmaxCalc = umax;
                _blendabletexVoffCalc = voff;
                _blendabletexVmaxCalc = vmax;

                float[,] m = GUIGraphicsContext.GetFinalMatrix();

                if (_maskFileName.Length > 0)
                {
                  // Draw the image texture with the alpha-mask.
                  if (_packedMaskTextureNo < 0)
                  {
                    GUITextureManager.GetPackedTexture(_maskFileName, out _masktexUoff, out _masktexVoff,
                                                       out _masktexUmax, out _masktexVmax, out _maskTexWidth,
                                                       out _maskTexHeight, out _maskTexture,
                                                       out _packedMaskTextureNo);
                  }
                  if (_packedMaskTextureNo >= 0)
                  {
                    float uoffm, voffm, umaxm, vmaxm;
                    uoffm = _masktexUoff;
                    voffm = _masktexVoff;
                    umaxm = _masktexUmax + _masktexUoff;
                    vmaxm = _masktexVmax + _masktexVoff;

                    FontEngineDrawMaskedTexture2(_packedTextureNo, fx, fy, nw, nh, uoff1, voff1, umax1, vmax1,
                                                 (int)color, m,
                                                 _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                                 _packedMaskTextureNo, uoffm, voffm, umaxm, vmaxm,
                                                 _blendMode);
                  }
                }
                else
                {
                  FontEngineDrawTexture2(_packedTextureNo, fx, fy, nw, nh, uoff1, voff1, umax1, vmax1, (int)color, m,
                                         _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                         _blendMode);
                }

                // Draw flipped image border.
                if (_flipX || _flipY)
                {
                  if ((_borderLeft > 0 || _borderRight > 0 || _borderTop > 0 || _borderBottom > 0) &&
                      _borderTextureFileName.Length > 0)
                  {
                    DrawBorder(true);
                  }
                }
              }
            }

            // Draw main image border.
            if ((_borderLeft > 0 || _borderRight > 0 || _borderTop > 0 || _borderBottom > 0) &&
                _borderTextureFileName.Length > 0)
            {
              DrawBorder(false);
            }

            base.Render(timePassed);
            return;
          }
          else if (_listTextures != null)
          {
            if (_listTextures.Length > 0)
            {
              Animate();
              CachedTexture.Frame frame = _listTextures[_currentFrameNumber];
              if (frame == null)
              {
                Cleanup();
                AllocResources();
                if (_listTextures == null || _listTextures.Length < 1)
                {
                  base.Render(timePassed);
                  return;
                }
                frame = _listTextures[_currentFrameNumber];
                if (frame == null)
                {
                  base.Render(timePassed);
                  return;
                }
              }
              if (frame.Image == null)
              {
                Cleanup();

                AllocResources();
                base.Render(timePassed);
                return;
              }

              uint color = (uint)_diffuseColor;
              if (Dimmed)
              {
                color = (uint)(_diffuseColor & DimColor);
              }
              color = GUIGraphicsContext.MergeAlpha(color);

              if (_tileFill)
              {
                // Get the texture and scale its size for the screen.
                int tW = GUITextureManager.GetCachedTexture(frame.ImageName).Width;
                int tH = GUITextureManager.GetCachedTexture(frame.ImageName).Height;
                GUIGraphicsContext.ScaleHorizontal(ref tW);
                GUIGraphicsContext.ScaleVertical(ref tH);

                // Find the next nearest texture size (a power of 2 square).
                int x = Math.Max(tW, tH);
                --x;
                x |= x >> 1;
                x |= x >> 2;
                x |= x >> 4;
                x |= x >> 8;
                x |= x >> 16;
                ++x;

                // Compute the number of textures to draw in the control and draw the texture.
                float umax = _nw / x;
                float vmax = _nh / x;
                frame.Draw(_fx, _fy, _nw, _nh, 0, 0, umax, vmax, (int)color);
              }
              else
              {
                if (_maskFileName.Length > 0)
                {
                  // Draw the image texture with the alpha-mask.
                  if (_packedMaskTextureNo < 0)
                  {
                    GUITextureManager.GetPackedTexture(_maskFileName, out _masktexUoff, out _masktexVoff,
                                                       out _masktexUmax, out _masktexVmax, out _maskTexWidth,
                                                       out _maskTexHeight, out _maskTexture,
                                                       out _packedMaskTextureNo);
                  }
                  if (_packedMaskTextureNo >= 0)
                  {
                    float uoffm, voffm, umaxm, vmaxm;
                    uoffm = _masktexUoff;
                    voffm = _masktexVoff;
                    umaxm = _masktexUmax + _masktexUoff;
                    vmaxm = _masktexVmax + _masktexVoff;

                    frame.DrawMasked(_fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)color,
                                     _packedMaskTextureNo, uoffm, voffm, umaxm, vmaxm);
                  }
                }
                else
                {
                  frame.Draw(_fx, _fy, _nw, _nh, _uoff, _voff, _umax, _vmax, (int)color);
                }
              }

//              if ((_flipX || _flipY) && _blendableFileName.Length > 0)
              if (_blendableFileName.Length > 0)
              {
                if (_packedBlendableTextureNo < 0)
                {
                  if (GUITextureManager.GetPackedTexture(_blendableFileName, out _blendabletexUoff, out _blendabletexVoff,
                                                         out _blendabletexUmax, out _blendabletexVmax, out _blendableTexWidth,
                                                         out _blendableTexHeight, out _blendableTexture,
                                                         out _packedBlendableTextureNo)) {}
                }
                if (_packedBlendableTextureNo >= 0)
                {
                  float fx, fy, nw, nh, uoff, voff, umax, vmax, uoff1, voff1, umax1, vmax1;
                  fx = _fx;
                  fy = _fy;
                  nw = _nw;
                  nh = _nh;
                  uoff = _blendabletexUoff;
                  voff = _blendabletexVoff;
                  umax = _blendabletexUmax + _blendabletexUoff;
                  vmax = _blendabletexVmax + _blendabletexVoff;
                  uoff1 = _uoff;
                  voff1 = _voff;
                  umax1 = _umax + _uoff;
                  vmax1 = _vmax + _voff;

                  if (_flipX)
                  {
                    fx += nw + _borderRight;
                    uoff1 = _umax + _uoff;
                    umax1 = _uoff;

                    uoff = _blendabletexUmax + _blendabletexUoff;
                    umax = _blendabletexUoff;
                  }
                  if (_flipY)
                  {
                    fy += nh + _borderBottom;
                    //uoff1 = _umax + _uoff;
                    //umax1 = _uoff;

                    voff1 = _vmax + _voff;
                    vmax1 = _voff;


                    voff = _blendabletexVmax + _blendabletexVoff;
                    vmax = _blendabletexVoff;
                  }

                  // Set final calculated coordinates for diffuse texture.
                  _blendabletexUoffCalc = uoff;
                  _blendabletexUmaxCalc = umax;
                  _blendabletexVoffCalc = voff;
                  _blendabletexVmaxCalc = vmax;

                  float[,] matrix = GUIGraphicsContext.GetFinalMatrix();

                  if (_maskFileName.Length > 0)
                  {
                    // Draw the image texture with the alpha-mask.
                    if (_packedMaskTextureNo < 0)
                    {
                      GUITextureManager.GetPackedTexture(_maskFileName, out _masktexUoff, out _masktexVoff,
                                                         out _masktexUmax, out _masktexVmax, out _maskTexWidth,
                                                         out _maskTexHeight, out _maskTexture,
                                                         out _packedMaskTextureNo);
                    }
                    if (_packedMaskTextureNo >= 0)
                    {
                      float uoffm, voffm, umaxm, vmaxm;
                      uoffm = _masktexUoff;
                      voffm = _masktexVoff;
                      umaxm = _masktexUmax + _masktexUoff;
                      vmaxm = _masktexVmax + _masktexVoff;

                      FontEngineDrawMaskedTexture2(frame.TextureNumber, fx, fy, nw, nh, uoff1, voff1, umax1, vmax1,
                                                   (int)color, matrix,
                                                   _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                                   _packedMaskTextureNo, uoffm, voffm, umaxm, vmaxm,
                                                   _blendMode);
                    }
                  }
                  else
                  {
                    FontEngineDrawTexture2(frame.TextureNumber, fx, fy, nw, nh, uoff1, voff1, umax1, vmax1, (int)color,
                                           matrix,
                                           _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                           _blendMode);
                  }
                }
                
                // Draw flipped image border.
                if (_flipX || _flipY)
                {
                  if ((_borderLeft > 0 || _borderRight > 0 || _borderTop > 0 || _borderBottom > 0) &&
                      _borderTextureFileName.Length > 0)
                  {
                    DrawBorder(true);
                  }
                }
              }

              // Draw main image border.
              if ((_borderLeft > 0 || _borderRight > 0 || _borderTop > 0 || _borderBottom > 0) &&
                  _borderTextureFileName.Length > 0)
              {
                DrawBorder(false);
              }

              frame = null;
              base.Render(timePassed);
            }
          }
          else
          {
            base.Render(timePassed);
          }
        }
        finally
        {
          if (!CalibrationEnabled)
          {
            GUIGraphicsContext.BypassUICalibration(false);
          }
        }
      }
    }

    //*******************************************************************************************************************
    // Draw a rectangle using one texture for four rectangles around the specified rectangle; one on each side.
    // U------U
    // |      |
    // |      |
    // L------L
    // bl,br,bt,bb - border width on the left,right,top,bottom
    // The border position (pos) is specified by pos relative to the x,y,nw,nh rectangle
    // U=upper corners, L=lower corners
    // pos values 0=outside, 1=inside, 2=center
    private void DrawBorder(bool flip)
    {
      float bl = _borderLeft;
      float br = _borderRight;
      float bt = _borderTop;
      float bb = _borderBottom;

      float posX = 0f, posY = 0f, width = 0f, height = 0f; // Describes the bounding rectangle to border
      float tx = 0f, ty = 0f, tw = 0f, th = 0f; // Scaling translations for border position (center, inside, outside)
      float bx = 0f, by = 0f, bw = 0f, bh = 0f; // One border rectangle (reused for each side)
      float uoff = 0f, voff = 0f, umax = 0f, vmax = 0f; // Texture coordinates
      float zrot = 0f; // Rotation of a textured border edge if specified by _borderTextureRotate
      float cxLeft = 0f, cyLeft = 0f, cwLeft = 0f, chLeft = 0f; // Left corner rectangle (reused for each left corner)
      float cxRight = 0f, cyRight = 0f, cwRight = 0f, chRight = 0f; // Right corner rectangle (reused for each right corner)
      int cuLeft = 0, cvLeft = 0, cumaxLeft = 0, cvmaxLeft = 0; // Left corner texture coordinates
      int cuRight = 0, cvRight = 0, cumaxRight = 0, cvmaxRight = 0; // Right corner texture coordinates

      CachedTexture.Frame texture = null;
      CachedTexture.Frame cornerTexture = null;
      int mergedBorderColorKey = (int)GUIGraphicsContext.MergeAlpha((uint)_borderColorKey);
      int itw, ith;
      int ictw, icth;
      float textureWidth;
      float textureHeight;

      // Get a texture from the texture file.
      if (GUITextureManager.Load(_borderTextureFileName, mergedBorderColorKey, -1, -1, true) == 0)
      {
        return; // Could not load the texture.
      }
      texture = GUITextureManager.GetTexture(_borderTextureFileName, 0, out itw, out ith);

      if (_borderHasCorners)
      {
        // Compute the name of the corner texture.
        string _cornerTextureFileName = Path.GetFileNameWithoutExtension(_borderTextureFileName);
        _cornerTextureFileName += "_corner" + Path.GetExtension(_borderTextureFileName);

        if (GUITextureManager.Load(_cornerTextureFileName, mergedBorderColorKey, -1, -1, true) == 0)
        {
          _borderHasCorners = false; // Could not load the texture; ignore the corners.
        }
        else
        {
          cornerTexture = GUITextureManager.GetTexture(_cornerTextureFileName, 0, out ictw, out icth);
        }
      }

      textureWidth = (float)itw;
      textureHeight = (float)ith;

      switch (_borderPosition)
      {
          // Border the image
        case BorderPosition.BORDER_IMAGE_OUTSIDE:
        case BorderPosition.BORDER_IMAGE_INSIDE:
        case BorderPosition.BORDER_IMAGE_CENTER:
          posX = _fx;
          posY = _fy;
          width = _nw;
          height = _nh;
          break;

          // Border the control rectangle
        case BorderPosition.BORDER_CONTROL_OUTSIDE:
        case BorderPosition.BORDER_CONTROL_INSIDE:
        case BorderPosition.BORDER_CONTROL_CENTER:
          posX = XPosition;
          posY = YPosition;
          width = Width;
          height = Height;
          break;
      }

      // Compute new position for flipped border.
      if (flip)
      {
        if (_flipX)
        {
          posX += width;
        }

        if (_flipY)
        {
          posY += height;
        }
      }

      switch (_borderPosition)
      {
          // Border at center position
        case BorderPosition.BORDER_IMAGE_CENTER:
        case BorderPosition.BORDER_CONTROL_CENTER:
          // Use Ceiling(), need an even numbered pixel count in border width to avoid aliasing and gaps due to rounding during presentation.
          tx = posX + (float)Math.Ceiling(bl / 2);
          ty = posY + (float)Math.Ceiling(bt / 2);
          tw = width - (float)Math.Ceiling(bl / 2) - (float)Math.Ceiling(br / 2);
          th = height - (float)Math.Ceiling(bt / 2) - (float)Math.Ceiling(bb / 2);
          break;

          // Border at inside position
        case BorderPosition.BORDER_IMAGE_INSIDE:
        case BorderPosition.BORDER_CONTROL_INSIDE:
          tx = posX + bl;
          ty = posY + bt;
          tw = width - bl - br;
          th = height - bt - bb;
          break;

          // Border at outside position
        case BorderPosition.BORDER_IMAGE_OUTSIDE:
        case BorderPosition.BORDER_CONTROL_OUTSIDE:
          tx = posX;
          ty = posY;
          tw = width;
          th = height;
          break;
      }

      // Left border rectangle
      // Rotated 270 deg (-PI/2 radians)
      bx = tx - bl;
      by = ty - bt;
      bw = bl;
      bh = bt + th + bb;

      if (_borderHasCorners)
      {
        by = ty; // Trim to make room to draw the corners
        bh = th;
      }

      if ((bw > 0) && (bh > 0))
      {
        int rotOffset = 0;
        if (_borderTextureRotate)
        {
          zrot = -(float)Math.PI / 2;
          rotOffset = 1; // Offset needed due to base of rotation of border edge

          // Transpose the border rectangle
          float temp = bw;
          bw = bh;
          bh = temp;

          // Translate the border rectangle origin
          // Pixel offset accounts for rotation point being at upper left of the pixel at (bx,by)
          //bx = bx;
          by = by + bw - rotOffset;

          // Calculate the texture extent for repeat behaviour while maintaining the textures aspect ratio
          umax = bw / (bh * (textureWidth / textureHeight));
          vmax = 1;
        }
        else
        {
          // Calculate the texture extent for repeat behaviour while maintaining the textures aspect ratio
          umax = 1;
          vmax = (bh * (textureHeight / textureWidth)) / bw;
        }

        // Force texture to stretch if repeat not specified.
        if (!_borderTextureRepeat)
        {
          umax = 1;
          vmax = 1;
        }

        if (flip)
        {
          // Compute new texture coordinates for flipped left side border.
          float temp;
          if (_flipX)
          {
            temp = uoff;
            uoff = umax + temp;
            umax = temp;
          }

          if (_flipY)
          {
            temp = voff;
            voff = vmax + temp;
            vmax = temp;
          }

          // Draw left border flipped.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax,
                       mergedBorderColorKey,
                       _packedBlendableTextureNo, _blendabletexVoffCalc, _blendabletexUoffCalc, _blendabletexVmaxCalc, _blendabletexUmaxCalc,
                       FontEngineBlendMode.BLEND_NONE);
// flipped 180         _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVmaxCalc, _blendabletexUmaxCalc, _blendabletexVoffCalc,
//                     FontEngineBlendMode.BLEND_NONE);
// orig                _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
//                     FontEngineBlendMode.BLEND_NONE);
        }
        else
        {
          // Draw left border.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax, mergedBorderColorKey);
        }
      }

      // Right border rectangle
      // Rotated 90 deg (PI/2 radians)
      bx = tx + tw;
      by = ty - bt;
      bw = br;
      bh = bt + th + bb;

      if (_borderHasCorners)
      {
        by = ty; // Trim to make room to draw the corners
        bh = th;
      }

      if ((bw > 0) && (bh > 0))
      {
        int rotOffset = 0;
        if (_borderTextureRotate)
        {
          zrot = (float)Math.PI / 2;
          rotOffset = 1; // Offset needed due to base of rotation of border edge

          // Transpose the border rectangle
          float temp = bw;
          bw = bh;
          bh = temp;

          // Translate the border rectangle origin
          // Pixel offset accounts for rotation point being at upper left of the pixel at (bx,by)
          bx = bx + bh - rotOffset;
          //by = by;

          // Calculate the texture extent for repeat behaviour while maintaining the textures aspect ratio
          umax = bw / (bh * (textureWidth / textureHeight));
          vmax = 1;
        }
        else
        {
          // Calculate the texture extent for repeat behaviour while maintaining the textures aspect ratio
          umax = 1;
          vmax = (bh * (textureHeight / textureWidth)) / bw;
        }

        // Force texture to stretch if repeat not specified.
        if (!_borderTextureRepeat)
        {
          umax = 1;
          vmax = 1;
        }

        if (flip)
        {
          // Compute new texture coordinates for flipped right side border.
          float temp;
          if (_flipX)
          {
            temp = uoff;
            uoff = umax + temp;
            umax = temp;
          }

          if (_flipY)
          {
            temp = voff;
            voff = vmax + temp;
            vmax = temp;
          }

          // Draw right border flipped.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax,
                       mergedBorderColorKey,
                       _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                       FontEngineBlendMode.BLEND_NONE);
        }
        else
        {
          // Draw right border.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax, mergedBorderColorKey);
        }
      }

      // Top border rectangle
      // No rotation
      bx = tx;
      by = ty - bt;
      bw = tw;
      bh = bt;
      if ((bw > 0) && (bh > 0))
      {
        if (_borderTextureRotate)
        {
          zrot = 0f;
        }

        // Calculate the texture extent for repeat behaviour while maintaining the textures aspect ratio
        umax = bw / (bh * (textureWidth / textureHeight));
        vmax = 1;

        // Force texture to stretch if repeat not specified.
        if (!_borderTextureRepeat)
        {
          umax = 1;
          vmax = 1;
        }

        // Compute corner coordinates.
        if (_borderHasCorners)
        {
          // Upper left corner position, size, texture coords
          cxLeft = bx - bl;
          cyLeft = by;
          cwLeft = bl;
          chLeft = bt;

          cuLeft = 0;
          cvLeft = 0;
          cumaxLeft = 1;
          cvmaxLeft = 1;

          // Upper right corner position, size, texture coords
          cxRight = bx + bw;
          cyRight = by;
          cwRight = br;
          chRight = bt;

          cuRight = 0;
          cvRight = 0;
          cumaxRight = 1;
          cvmaxRight = 1;

          if (_borderCornerTextureRotate)
          {
            // Flip texture horizontally
            cuRight = 1;
            cvRight = 0;
            cumaxRight = 0;
            cvmaxRight = 1;
          }
        }

        if (flip)
        {
          // Compute new texture coordinates for flipped top side border.
          float temp;
          if (_flipX)
          {
            temp = uoff;
            uoff = umax + temp;
            umax = temp;
          }

          if (_flipY)
          {
            temp = voff;
            voff = vmax + temp;
            vmax = temp;
          }

          // Draw top border flipped.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax,
                       mergedBorderColorKey,
                       _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                       FontEngineBlendMode.BLEND_NONE);
        }
        else
        {
          // Draw top border.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax, mergedBorderColorKey);
        }

        if (_borderHasCorners)
        {
          // Using FontEngineDrawTexture2 for its ability to flip the corner texture.
          float[,] m = GUIGraphicsContext.GetFinalMatrix();

          if (flip)
          {
            // Upper left corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxLeft, cyLeft, cwLeft, chLeft, cuLeft, cvLeft, cumaxLeft,
                                   cvmaxLeft, mergedBorderColorKey, m,
                                   _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                   FontEngineBlendMode.BLEND_DIFFUSE);

            // Upper right corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxRight, cyRight, cwRight, chRight, cuRight, cvRight,
                                   cumaxRight, cvmaxRight, mergedBorderColorKey, m,
                                   _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                   FontEngineBlendMode.BLEND_DIFFUSE);
          }
          else
          {
            // Upper left corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxLeft, cyLeft, cwLeft, chLeft, cuLeft, cvLeft, cumaxLeft,
                                   cvmaxLeft, mergedBorderColorKey, m,
                                   cornerTexture.TextureNumber, cuRight, cvRight, cumaxRight, cvmaxRight,
                                   FontEngineBlendMode.BLEND_NONE);

            // Upper right corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxRight, cyRight, cwRight, chRight, cuRight, cvRight,
                                   cumaxRight, cvmaxRight, mergedBorderColorKey, m,
                                   cornerTexture.TextureNumber, cuRight, cvRight, cumaxRight, cvmaxRight,
                                   FontEngineBlendMode.BLEND_NONE);
          }
        }
      }

      // Bottom border rectangle
      // Rotated 180 deg (PI radians)
      bx = tx;
      by = ty + th;
      bw = tw;
      bh = bb;
      if ((bw > 0) && (bh > 0))
      {
        int rotOffset = 0;
        if (_borderTextureRotate)
        {
          zrot = (float)Math.PI;
          rotOffset = 1; // Offset needed due to base of rotation of border edge

          // Translate the border rectangle origin
          // Pixel offset accounts for rotation point being at upper left of the pixel at (bx,by)
          bx = bx + bw - rotOffset;
          by = by + bh - rotOffset;
        }

        // Calculate the texture extent for repeat behaviour while maintaining the textures aspect ratio
        umax = bw / (bh * (textureWidth / textureHeight));
        vmax = 1;

        // Force texture to stretch if repeat not specified.
        if (!_borderTextureRepeat)
        {
          umax = 1;
          vmax = 1;
        }

        // Compute corner coordinates.
        if (_borderHasCorners)
        {
          // Lower left corner position, size, texture coords
          if (_borderTextureRotate)
          {
            cxLeft = bx - bw - bl + rotOffset;
            cyLeft = by - bb + rotOffset;
          }
          else
          {
            cxLeft = bx - bl;
            cyLeft = by;
          }
          cwLeft = bl;
          chLeft = bb;

          cuLeft = 0;
          cvLeft = 0;
          cumaxLeft = 1;
          cvmaxLeft = 1;

          // Lower right corner position, size, texture coords
          if (_borderTextureRotate)
          {
            cxRight = bx + rotOffset;
            cyRight = by - bb + rotOffset;
          }
          else
          {
            cxRight = bx + bw;
            cyRight = by;
          }
          cwRight = br;
          chRight = bb;

          cuRight = 0;
          cvRight = 0;
          cumaxRight = 1;
          cvmaxRight = 1;

          if (_borderCornerTextureRotate)
          {
            // Flip texture vertically
            cuLeft = 0;
            cvLeft = 1;
            cumaxLeft = 1;
            cvmaxLeft = 0;

            // Flip texture horizontally and vertically
            cuRight = 1;
            cvRight = 1;
            cumaxRight = 0;
            cvmaxRight = 0;
          }
        }

        if (flip)
        {
          // Compute new texture coordinates for flipped top side border.
          float temp;
          if (_flipX)
          {
            temp = uoff;
            uoff = umax + temp;
            umax = temp;
          }

          if (_flipY)
          {
            temp = voff;
            voff = vmax + temp;
            vmax = temp;
          }

          // Draw bottom border flipped.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax,
                       mergedBorderColorKey,
                       _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                       FontEngineBlendMode.BLEND_NONE);
        }
        else
        {
          // Draw bottom border.
          texture.Draw(bx, by, bw, bh, zrot, uoff, voff, umax, vmax, mergedBorderColorKey);
        }

        if (_borderHasCorners)
        {
          // Using FontEngineDrawTexture2 for its ability to flip the corner texture.
          float[,] m = GUIGraphicsContext.GetFinalMatrix();

          if (flip)
          {
            // Lower left corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxLeft, cyLeft, cwLeft, chLeft, cuLeft, cvLeft, cumaxLeft,
                                   cvmaxLeft, mergedBorderColorKey, m,
                                   _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                   FontEngineBlendMode.BLEND_DIFFUSE);

            // Lower right corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxRight, cyRight, cwRight, chRight, cuRight, cvRight,
                                   cumaxRight, cvmaxRight, mergedBorderColorKey, m,
                                   _packedBlendableTextureNo, _blendabletexUoffCalc, _blendabletexVoffCalc, _blendabletexUmaxCalc, _blendabletexVmaxCalc,
                                   FontEngineBlendMode.BLEND_DIFFUSE);
          }
          else
          {
            // Lower left corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxLeft, cyLeft, cwLeft, chLeft, cuLeft, cvLeft, cumaxLeft,
                                   cvmaxLeft, mergedBorderColorKey, m,
                                   cornerTexture.TextureNumber, cuRight, cvRight, cumaxRight, cvmaxRight,
                                   FontEngineBlendMode.BLEND_NONE);

            // Lower right corner
            FontEngineDrawTexture2(cornerTexture.TextureNumber, cxRight, cyRight, cwRight, chRight, cuRight, cvRight,
                                   cumaxRight, cvmaxRight, mergedBorderColorKey, m,
                                   cornerTexture.TextureNumber, cuRight, cvRight, cumaxRight, cvmaxRight,
                                   FontEngineBlendMode.BLEND_NONE);
          }
        }
      }
      texture = null;
    }

    /// <summary>
    /// Set the filename of the texture and re-allocates the DirectX resources for this GUIImage.
    /// </summary>
    /// <param name="strFileName"></param>
    public void SetFileName(string fileName)
    {
      if (fileName == null)
      {
        return;
      }
      if (_textureFileNameTag == fileName)
      {
        return; // same file, no need to do anything
      }

      if (logtextures)
      {
        Log.Debug("GUIImage:SetFileName() {0}", fileName);
      }
      _textureFileNameTag = fileName;
      if (_textureFileNameTag.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }
      else
      {
        if (_containsProperty) //we had a property before, not anymore, therefor, unsubscribe
          GUIPropertyManager.OnPropertyChanged -=
            new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
        _containsProperty = false;
      }
      //reallocate & load then new image
      _allocated = false;
      Cleanup();

      AllocResources();
      if (_containsProperty)
      {
        _lockingObject = new object();
      }
      else
      {
        _lockingObject = GUITextureManager.GetCachedTexture(_textureFileNameTag);
      }

      if (_lockingObject == null)
      {
        _lockingObject = new object();
      }
    }

    /// <summary>
    /// Gets the rectangle in which this GUIImage is rendered.
    /// </summary>
    public Rectangle rect
    {
      get { return m_destRect; }
    }

    /// <summary>
    /// Property to enable/disable filtering
    /// </summary>
    public bool Filtering
    {
      get { return _filterImage; }
      set
      {
        if (_filterImage != value)
        {
          _filterImage = value; /*CreateStateBlock();*/
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should be centered in the
    /// given rectangle of the control
    /// </summary>
    [Obsolete("ImageAlignment/ImageVAlignment should be used. (<align>/<valign> tag)")]
    [XMLSkinElement("centered")]
    public bool Centered
    {
      get { return _imageAlignment == Alignment.ALIGN_CENTER && _imageVAlignment == VAlignment.ALIGN_MIDDLE; }
      set
      {
        if (value && (_imageAlignment != Alignment.ALIGN_CENTER || _imageVAlignment != VAlignment.ALIGN_MIDDLE))
        {
          _imageAlignment = Alignment.ALIGN_CENTER;
          _imageVAlignment = VAlignment.ALIGN_MIDDLE;
          _reCalculate = true;
        }
        else if (!value && _imageAlignment == Alignment.ALIGN_CENTER && _imageVAlignment == VAlignment.ALIGN_MIDDLE)
        {
          _imageAlignment = Alignment.ALIGN_LEFT;
          _imageVAlignment = VAlignment.ALIGN_TOP;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image alignment in the
    /// given rectangle of the control
    /// </summary>
    public Alignment ImageAlignment
    {
      get { return _imageAlignment; }
      set
      {
        if (_imageAlignment != value)
        {
          _imageAlignment = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image alignment in the
    /// given rectangle of the control
    /// </summary>
    public VAlignment ImageVAlignment
    {
      get { return _imageVAlignment; }
      set
      {
        if (_imageVAlignment != value)
        {
          _imageVAlignment = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should be zoomed in the
    /// given rectangle of the control
    /// </summary>
    public bool Zoom
    {
      get { return _zoomIn; }
      set
      {
        if (_zoomIn != value)
        {
          _zoomIn = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should retain its height 
    /// after it has been zoomed or aspectratio adjusted
    /// </summary>
    public bool FixedHeight
    {
      get { return _isFixedHeight; }
      set
      {
        if (_isFixedHeight != value)
        {
          _isFixedHeight = value;
          _reCalculate = true;
        }
      }
    }

    /// <summary>
    /// Property which indicates if the image should be zoomed into the
    /// given rectangle of the control. Zoom with fixed top, center width
    /// </summary>
    public bool ZoomFromTop
    {
      get { return _zoomFromTop; }
      set
      {
        if (_zoomFromTop != value)
        {
          _zoomFromTop = value;
          _reCalculate = true;
        }
      }
    }

    // recalculate the image dimensions & position
    public void Refresh()
    {
      Calculate();
    }

    /// <summary>
    /// property which returns true when this instance has a valid image
    /// </summary>
    public bool Allocated
    {
      get
      {
        if (FileName.Length == 0)
        {
          return false;
        }
        if (FileName.Equals("-"))
        {
          return false;
        }
        return true;
      }
    }

    public override int Width
    {
      get { return base.Width; }
      set
      {
        if (base.Width != value)
        {
          base.Width = value;
          _reCalculate = true;
        }
      }
    }

    public override int Height
    {
      get { return base.Height; }
      set
      {
        if (base.Height != value)
        {
          base.Height = value;
          _reCalculate = true;
        }
      }
    }

    public override long ColourDiffuse
    {
      get { return base.ColourDiffuse; }
      set
      {
        if (base.ColourDiffuse != value)
        {
          base.ColourDiffuse = value;
        }
      }
    }

    public override void SetPosition(int dwPosX, int dwPosY)
    {
      if (_positionX == dwPosX && _positionY == dwPosY)
      {
        return;
      }
      _positionX = dwPosX;
      _positionY = dwPosY;
      _reCalculate = true;
    }

    public void SetBorder(string border, BorderPosition position, bool textureRepeat, bool textureRotate,
                          string textureFilename, long colorKey, bool hasCorners, bool cornerTextureRotate)
    {
      _strBorder = border;
      _borderPosition = position;
      _borderTextureRepeat = textureRepeat;
      _borderTextureRotate = textureRotate;
      _borderTextureFileName = textureFilename;
      _borderColorKey = colorKey;
      _borderHasCorners = hasCorners;
      _borderCornerTextureRotate = cornerTextureRotate;

      FinalizeBorder();
    }

    public override void Animate(float timePassed, Animator animator)
    {
      base.Animate(timePassed, animator);
      _reCalculate = true;
    }

    protected override void Update()
    {
      _reCalculate = true;
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
      set
      {
        _diffuseFileName = value;
        _blendableFileName = _diffuseFileName;
        _blendMode = FontEngineBlendMode.BLEND_DIFFUSE;
      }
    }

    public string MaskFileName
    {
      get { return _maskFileName; }
      set
      {
        if (_maskFileName == value)
        {
          return;
        }
        _packedMaskTextureNo = -1; // Force the texture to reload.
        _maskFileName = value;
      }
    }

    public string OverlayFileName
    {
      get { return _overlayFileName; }
      set
      {
        _overlayFileName = value;
        _blendableFileName = _overlayFileName;
        _blendMode = FontEngineBlendMode.BLEND_OVERLAY;
      }
    }

    /// <summary>
    /// Get/set the filename of the border texture.
    /// </summary>
    public string BorderFileName
    {
      get { return _borderTextureFileName; }
      set
      {
        if (_borderTextureFileName == value)
        {
          return;
        }
        _borderTextureFileName = value;
      }
    }

    public RepeatBehavior RepeatBehavior
    {
      get { return _repeatBehavior; }
      set { _repeatBehavior = value; }
    }

    public void SetMemoryImageSize(int width, int height)
    {
      _memoryImageWidth = width;
      _memoryImageHeight = height;
    }

    public bool LockMemoryImageTexture(out Bitmap bitmap)
    {
      int pitch;
      if (_memoryImageTexture == null)
      {
        bitmap = null;
        return false;
      }
      using (GraphicsStream gs = _memoryImageTexture.LockRectangle(0, LockFlags.Discard, out pitch))
      {
        bitmap = new Bitmap(_memoryImageWidth, _memoryImageHeight, pitch, PixelFormat.Format32bppArgb, gs.InternalData);
      }
      return true;
    }

    public void UnLockMemoryImageTexture()
    {
      _memoryImageTexture.UnlockRectangle(0);
    }

    public void RemoveMemoryImageTexture()
    {
      _memoryImageTexture = null;
      GUITextureManager.ReleaseTexture(_textureFileNameTag);
      _textureFileNameTag = String.Empty;
    }

    protected void LoadAnimation(ref string textureFiles)
    {
      string fileName = GUIGraphicsContext.GetThemedSkinFile("\\" + textureFiles);
      try
      {
        XmlTextReader reader = new XmlTextReader(fileName);
        reader.WhitespaceHandling = WhitespaceHandling.None;
        // Parse the file and display each of the nodes.
        while (reader.Read())
        {
          if (reader.NodeType == XmlNodeType.Element)
          {
            switch (reader.Name)
            {
              case "textures":
                {
                  while (reader.Read())
                  {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                      break;
                    }
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                      textureFiles = reader.Value;
                    }
                  }
                  break;
                }
              case "RepeatBehavior":
                {
                  while (reader.Read())
                  {
                    if (reader.NodeType == XmlNodeType.EndElement)
                    {
                      break;
                    }
                    if (reader.NodeType == XmlNodeType.Text)
                    {
                      if (reader.Value.CompareTo("Forever") == 0)
                      {
                        _repeatBehavior = RepeatBehavior.Forever;
                      }
                      else
                      {
                        _repeatBehavior = new RepeatBehavior(double.Parse(reader.Value));
                      }
                    }
                  }
                  break;
                }
            }
          }
        }
      }
      catch (FileNotFoundException)
      {
        //ignore
        return;
      }
    }

    public bool TileFill
    {
      get { return _tileFill; }
      set { _tileFill = value; }
    }
  }
}