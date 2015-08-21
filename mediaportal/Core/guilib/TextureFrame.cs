using System;
using DShowNET.Helper;
using MediaPortal.ExtensionMethods;
using MediaPortal.GUI.Library;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.guilib
{
  internal class TextureFrame : IDisposable
  {
    public readonly bool UseNewTextureEngine = true;
    
    private bool _disposed = false;                       // track whether Dispose has been called.
    private int _textureNumber = -1;                      // handle to unmanaged texture
    private Texture _image;                               // texture of current frame
    private int _duration;                                // duration of current frame
    private string _imageName = string.Empty;

    public event EventHandler Disposed;

    public TextureFrame(string name, Texture image, int duration)
    {
      _imageName = name;
      _image = image;
      _duration = duration;

      if (image != null)
      {
        unsafe
        {
          _image.Disposing -= new EventHandler(D3DTexture_Disposing);
          _image.Disposing += new EventHandler(D3DTexture_Disposing);

          IntPtr ptr = DirectShowUtil.GetUnmanagedTexture(_image);
          _textureNumber = DXNative.FontEngineAddTextureSync(ptr.ToInt32(), true, (void*)ptr.ToPointer());
        }
      }
    }

    ~TextureFrame()
    {
      DisposeInternal();
    }

    public string ImageName
    {
      get { return _imageName; }
      set { _imageName = value; }
    }

    /// <summary>
    /// property to get/set the duration for this frame
    /// (only useful if the frame belongs to an animation, like an animated gif)
    /// </summary>
    public int Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    public int TextureNumber
    {
      get { return _textureNumber; }
    }

    public Texture Image
    {
      get { return _image; }
    }

    /// <summary>
    /// Releases the resources used by the texture.
    /// </summary>
    public void Dispose()
    {
      DisposeInternal();
      // This object will be cleaned up by the Dispose method.
      // Therefore, calling GC.SupressFinalize to take this object off
      // the finalization queue and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize(this);
    }

    private void DisposeInternal()
    {
      if (_disposed)
      {
        return;
      }

      DisposeUnmanagedResources();

      _disposed = true;

      if (Disposed != null)
      {
        Disposed(this, new EventArgs());
        
        if (Disposed != null)
        {
          foreach (EventHandler eventDelegate in Disposed.GetInvocationList())
          {
            Disposed -= eventDelegate;
          }
        }
      }
    }

    private void DisposeUnmanagedResources()
    {
      if (_textureNumber >= 0)
      {
        DXNative.FontEngineRemoveTextureSync(_textureNumber);
        _textureNumber = -1;
      }

      if (_image != null && !_image.Disposed)
      {
        _image.Disposing -= new EventHandler(D3DTexture_Disposing);
        _image.SafeDispose();
      }

      _image = null;
    }

    private void D3DTexture_Disposing(object sender, EventArgs e)
    {
      // D3D has disposed of this texture! notify so that things are kept up to date
      if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
      {
        Dispose();
      }
    }

    /// <summary>
    /// Draw a texture.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nw"></param>
    /// <param name="nh"></param>
    /// <param name="uoff"></param>
    /// <param name="voff"></param>
    /// <param name="umax"></param>
    /// <param name="vmax"></param>
    /// <param name="color"></param>
    public void Draw(float x, float y, float nw, float nh, float uoff, 
                     float voff, float umax, float vmax, uint color)
    {
      if (_textureNumber >= 0)
      {
        float[,] matrix = GUIGraphicsContext.GetFinalMatrix();
        DXNative.FontEngineDrawTextureSync(_textureNumber, x, y, nw, nh, uoff, voff, umax, vmax, color, matrix);
      }
    }

    /// <summary>
    /// Draw a texture rotated around (x,y).
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nw"></param>
    /// <param name="nh"></param>
    /// <param name="zrot"></param>
    /// <param name="uoff"></param>
    /// <param name="voff"></param>
    /// <param name="umax"></param>
    /// <param name="vmax"></param>
    /// <param name="color"></param>
    public void Draw(float x, float y, float nw, float nh, float zrot, float uoff, 
                     float voff, float umax, float vmax, uint color)
    {
      if (_textureNumber >= 0)
      {
        // Rotate around the x,y point of the specified rectangle; maintain aspect ratio (1.0f)
        TransformMatrix localTransform = new TransformMatrix();
        localTransform.SetZRotation(zrot, x, y, 1.0f);
        TransformMatrix finalTransform = GUIGraphicsContext.GetFinalTransform();
        localTransform = finalTransform.multiply(localTransform);

        DXNative.FontEngineDrawTextureSync(_textureNumber, x, y, nw, nh, uoff, voff, umax, vmax, color,
                                           localTransform.Matrix);

      }
    }

    /// <summary>
    /// Draw a texture rotated around (x,y) blended with a diffuse texture.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nw"></param>
    /// <param name="nh"></param>
    /// <param name="zrot"></param>
    /// <param name="uoff"></param>
    /// <param name="voff"></param>
    /// <param name="umax"></param>
    /// <param name="vmax"></param>
    /// <param name="color"></param>
    /// <param name="diffuseTextureNo"></param>
    /// <param name="uoffd"></param>
    /// <param name="voffd"></param>
    /// <param name="umaxd"></param>
    /// <param name="vmaxd"></param>
    public void Draw(float x, float y, float nw, float nh, float zrot, float uoff, float voff, 
                     float umax, float vmax, uint color, int blendableTextureNo, float uoffd, 
                     float voffd, float umaxd, float vmaxd, FontEngineBlendMode blendMode)
    {
      if (_textureNumber >= 0)
      {
        // Rotate around the x,y point of the specified rectangle; maintain aspect ratio (1.0f)
        TransformMatrix localTransform = new TransformMatrix();
        localTransform.SetZRotation(zrot, x, y, 1.0f);
        TransformMatrix finalTransform = GUIGraphicsContext.GetFinalTransform();
        localTransform = finalTransform.multiply(localTransform);

        DXNative.FontEngineDrawTexture2(_textureNumber, x, y, nw, nh, uoff, voff, umax, vmax,
                               color, localTransform.Matrix,
                               blendableTextureNo, uoffd, voffd, umaxd, vmaxd,
                               blendMode);
      }
    }

    /// <summary>
    /// Draw a masked texture.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="nw"></param>
    /// <param name="nh"></param>
    /// <param name="uoff"></param>
    /// <param name="voff"></param>
    /// <param name="umax"></param>
    /// <param name="vmax"></param>
    /// <param name="color"></param>
    /// <param name="maskTextureNo"></param>
    /// <param name="uoffm"></param>
    /// <param name="voffm"></param>
    /// <param name="umaxm"></param>
    /// <param name="vmaxm"></param>
    public void DrawMasked(float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax,
                           uint color, int maskTextureNo, float uoffm, float voffm, float umaxm, float vmaxm)
    {
      if (_textureNumber >= 0)
      {
        float[,] matrix = GUIGraphicsContext.GetFinalMatrix();
        DXNative.FontEngineDrawMaskedTexture(_textureNumber, x, y, nw, nh, uoff, voff, umax, vmax,
                                             color, matrix, maskTextureNo, uoffm, voffm, umaxm, vmaxm);
      }
    }
  }

}
