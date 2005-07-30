/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D=Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A datastructure for caching textures.
  /// This is used by the GUITextureManager which keeps a cache of all textures in use
  /// </summary>
  public class CachedTexture 
  {
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineRemoveTexture(int textureNo);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern int  FontEngineAddTexture(int hasCode,bool useAlphaBlend,void* fontTexture);
		
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern int  FontEngineAddSurface(int hasCode,bool useAlphaBlend,void* fontTexture);
		
		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEngineDrawTexture(int textureNo,float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color);

		[DllImport("fontEngine.dll", ExactSpelling=true, CharSet=CharSet.Auto, SetLastError=true)]
		unsafe private static extern void FontEnginePresentTextures();

		/// <summary>
		/// Class which contains a single frame
		/// A cached texture can contain more then 1 frames for example when its an animated gif
		/// </summary>
    public class Frame 
    {
      Texture								 _Image;			//texture of current frame
      int										 _Duration;	//duration of current frame
			int										 _TextureNo;
			public readonly bool    UseNewTextureEngine=true;
			string									imageName=String.Empty;
			static private bool logTextures=false;
      public Frame(string name,Texture image, int duration)
      {
				imageName=name;
        _Image = image;
        _Duration = duration;
				if (image!=null)
				{
					unsafe
					{
						IntPtr ptr=DShowNET.DsUtils.GetUnmanagedTexture(_Image);
						_TextureNo=FontEngineAddTexture(ptr.ToInt32(),true,(void*) ptr.ToPointer());
						if (logTextures) Log.Write("Frame:ctor() fontengine: added texture:{0} {1}",_TextureNo.ToString(),imageName);
					}
				}
      }
			public string ImageName
			{
				get { return imageName;}
				set { imageName=value;}
			}

			/// <summary>
			/// Property to get/set the texture
			/// </summary>
      public Texture Image
      {
        get { return _Image;}
        set {
          if (_Image!=null) 
          {
						if (logTextures) Log.Write("Frame:Image fontengine: remove texture:{0} {1}",_TextureNo.ToString(),imageName);
						FontEngineRemoveTexture(_TextureNo);
            if (!_Image.Disposed) 
              _Image.Dispose();
          }
          _Image=value;
					
					if (_Image!=null)
					{
						unsafe
						{
							IntPtr ptr=DShowNET.DsUtils.GetUnmanagedTexture(_Image);
							_TextureNo=FontEngineAddTexture(ptr.ToInt32(),true,(void*) ptr.ToPointer());
							if (logTextures) Log.Write("Frame:Image fontengine: added texture:{0} {1}",_TextureNo.ToString(),imageName);
						}
					}
        }
      }

			/// <summary>
			/// property to get/set the duration for this frame
			/// (only usefull if the frame belongs to an animation, like an animated gif)
			/// </summary>
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
					if (logTextures) Log.Write("Frame: dispose() fontengine: remove texture:"+_TextureNo.ToString());
					FontEngineRemoveTexture(_TextureNo);
					if (!_Image.Disposed)
          {
            _Image.Dispose();
          }
          _Image=null;
        }
      }

      #endregion

			public void Draw(float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color)
			{
				//string logline=String.Format("draw:#{0} {1} {2} {3} {4}",_TextureNo,x,y,nw,nh);
				//Trace.WriteLine(logline);
				if (_TextureNo>=0)
				{
					FontEngineDrawTexture(_TextureNo,x, y, nw, nh, uoff, voff, umax, vmax, color);
				}
				else
				{
					if (logTextures) Log.Write("fontengine:Draw() ERROR. Texture is disposed:{0} {1}",_TextureNo.ToString(),imageName);
				}
			}
    }

    string    m_strName="";								// filename of the texture
    ArrayList m_Frames=new ArrayList();	  // array to hold all frames
    int       m_iWidth=0;									// width of the texture
    int       m_iHeight=0;								// height of the texture
    int       m_iFrames=0;								// number of frames in the animation
    Image     m_Image=null;								// GDI image of the texture

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
		/// Get/set the DirectX texture for the 1st frame
		/// </summary>
    public Frame texture
    {
      get 
			{ 
				if (m_Frames.Count==0) return null;
				return (Frame )m_Frames[0];
			}
      set 
			{ 
          Dispose();      // cleanup..
          m_Frames.Clear();
          m_Frames.Add(value);
      }
    }

		/// <summary>
		/// Get/set the GDI Image 
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
		/// indexer to get a Frame or to set a Frame
		/// </summary>
    public Frame this [int index]
    {
      get 
      {
				if (index <0 || index >= m_Frames.Count) return null;
        return (Frame)m_Frames[index];
      }
      set 
      {
				if (index <0) return;

        if (m_Frames.Count <= index)
          m_Frames.Add(value);
        else
        {
          Frame frame=(Frame)m_Frames[index];
          if (frame!=value)
          {
            frame.Dispose();
            m_Frames[index]=value;
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

      foreach (Frame tex in m_Frames)
      {
				if (tex!=null)
				{
					tex.Dispose();
				}
      }
      m_Frames.Clear();
      if (m_Image!=null)
      {
        m_Image.Dispose();
        m_Image=null;
      }
    }
    #endregion
  }
}
