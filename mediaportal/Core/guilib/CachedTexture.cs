using System;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A datastructure for caching textures.
  /// </summary>
  public class CachedTexture 
  {
    public class Frame 
    {
      Texture _Image;
      int     _Duration;
      
      public Frame(Texture image, int duration)
      {
        _Image = image;
        _Duration = duration;
      }

      public Texture Image
      {
        get { return _Image;}
        set {
          if (_Image!=null) 
          {
            if (!_Image.Disposed) 
              _Image.Dispose();
          }
          _Image=value;
        }
      }

      public int Duration
      {
        get { return _Duration;}
        set { _Duration=value;}
      }
      #region IDisposable Members

      public void Dispose()
      {
        if (_Image!=null)
        {
          if (!_Image.Disposed)
          {
            _Image.Dispose();
          }
          _Image=null;
        }
      }

      #endregion
    }

    string    m_strName="";
    ArrayList m_textures=new ArrayList();
    int       m_iWidth=0;
    int       m_iHeight=0;
    int       m_iFrames=0;
    Image     m_Image=null;

		/// <summary>
		/// The (emtpy) constructor of the CachedTexture class.
		/// </summary>
    public CachedTexture()
    {
    }


		/// <summary>
		/// Get/set the filename/location of the texture.
		/// </summary>
    public string Name
    {
      get { return m_strName;}
      set { m_strName=value;}
    }

		/// <summary>
		/// Get/set the DirectX texture corresponding to the file.
		/// </summary>
    public Frame texture
    {
      get { return (Frame )m_textures[0];}
      set { 
          Dispose();      // cleanup..
          m_textures.Clear();
          m_textures.Add(value);
      }
    }

		/// <summary>
		/// Get/set the Image containing the texture.
		/// </summary>
    public Image image
    {
      get { return m_Image;}
      set 
      {
        if (m_Image!=null)
        {
          m_Image.Dispose();
        }
        m_Image=value;
      }
    }

		/// <summary>
		/// Get/set the width of the texture.
		/// </summary>
    public int Width
    {
      get { return m_iWidth;}
      set { m_iWidth=value;}
    }

		/// <summary>
		/// Get/set the height of the texture.
		/// </summary>
    public int Height
    {
      get { return m_iHeight;}
      set { m_iHeight=value;}
    }

		/// <summary>
		/// Get/set the number of frames out of which the texture exsists.
		/// </summary>
    public int Frames
    {
      get { return m_iFrames;}
      set { m_iFrames=value;}
    }

		/// <summary>
		/// Get/set textures individual textures.
		/// </summary>
    public Frame this [int index]
    {
      get 
      {
        return (Frame)m_textures[index];
      }
      set 
      {
        if (m_textures.Count <= index)
          m_textures.Add(value);
        else
        {
          Frame frame=(Frame)m_textures[index];
          if (frame!=value)
          {
            frame.Dispose();
            m_textures[index]=value;
          }
        }
      }
    }
    #region IDisposable Members
		/// <summary>
		/// Releases the resources used by the texture.
		/// </summary>
    public void Dispose()
    {

      foreach (Frame tex in m_textures)
      {
        tex.Dispose();
      }
      m_textures.Clear();
      if (m_Image!=null)
      {
        m_Image.Dispose();
        m_Image=null;
      }
    }
    #endregion
  }
}
