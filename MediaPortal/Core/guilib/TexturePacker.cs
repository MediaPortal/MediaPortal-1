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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Filter = Microsoft.DirectX.Direct3D.Filter;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// Summary description for TexturePacker.
  /// </summary>
  public class TexturePacker
  {
    public delegate void DisposeEventHandler(object sender, int texutureNumber);

    public event DisposeEventHandler Disposing;

    #region imports

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineRemoveTexture(int textureNo);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe int FontEngineAddTexture(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe int FontEngineAddSurface(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawTexture(int textureNo, float x, float y, float nw, float nh,
                                                            float uoff, float voff, float umax, float vmax, int color);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEnginePresentTextures();

    #endregion

    #region PackedTexture Class

    [Serializable]
    public class PackedTexture
    {
      public PackedTextureNode root;
      public int textureNo;
      [NonSerialized]
      public Texture texture;
    } ;

    #endregion

    #region PackedTextureNode class

    [Serializable]
    public class PackedTextureNode
    {
      public PackedTextureNode ChildLeft;
      public PackedTextureNode ChildRight;
      public Rectangle Rect;
      public string FileName;

      public PackedTextureNode Get(string fileName)
      {
        if (fileName == null)
        {
          return null;
        }
        if (fileName.Length == 0)
        {
          return null;
        }
        if (FileName != null)
        {
          if (FileName == fileName)
          {
            return this;
          }
        }
        if (ChildLeft != null)
        {
          PackedTextureNode node = ChildLeft.Get(fileName);
          if (node != null)
          {
            return node;
          }
        }
        if (ChildRight != null)
        {
          return ChildRight.Get(fileName);
        }
        return null;
      }

      public PackedTextureNode Insert(string fileName, Image img, Image rootImage)
      {
        //Log.Info("rect:({0},{1}) {2}x{3} img:{4}x{5} filename:{6} left:{7} right:{8}",
        //				Rect.Left,Rect.Top,Rect.Width,Rect.Height,img.Width,img.Height,FileName, ChildLeft,ChildRight);
        if (ChildLeft != null && ChildRight != null)
        {
          PackedTextureNode node = ChildLeft.Insert(fileName, img, rootImage);
          if (node != null)
          {
            return node;
          }
          return ChildRight.Insert(fileName, img, rootImage);
        }
        //(if there's already a lightmap here, return)
        if (FileName != null && FileName.Length > 0)
        {
          return null;
        }

        //(if we're too small, return)
        if ((img.Width + 2) > Rect.Width || (img.Height + 2) > Rect.Height)
        {
          return null;
        }
        //(if we're just right, accept)
        if ((img.Width + 2) == Rect.Width && (img.Height + 2) == Rect.Height)
        {
          using (Graphics g = Graphics.FromImage(rootImage))
          {
            FileName = fileName;
            g.CompositingQuality = CompositingQuality.HighQuality; // Thumbs.Compositing;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic; // Thumbs.Interpolation;
            g.SmoothingMode = SmoothingMode.HighQuality; // Thumbs.Smoothing;
            // draw oversized image first
            g.DrawImage(img, Rect.Left, Rect.Top, Rect.Width, Rect.Height);
            // draw original image ontop of oversized imagealt
            g.DrawImage(img, Rect.Left + 1, Rect.Top + 1, Rect.Width - 2, Rect.Height - 2);
          }
          return this;
        }

        if (Rect.Width <= 2 || Rect.Height <= 2)
        {
          return null;
        }
        //(otherwise, gotta split this node and create some kids)
        ChildLeft = new PackedTextureNode();
        ChildRight = new PackedTextureNode();

        //(decide which way to split)
        int dw = Rect.Width - (img.Width + 2);
        int dh = Rect.Height - (img.Height + 2);

        if (dw > dh)
        {
          ChildLeft.Rect = new Rectangle(Rect.Left, Rect.Top, (img.Width + 2), Rect.Height);
          ChildRight.Rect = new Rectangle(Rect.Left + (img.Width + 2), Rect.Top, Rect.Width - (img.Width + 2),
                                          Rect.Height);
        }
        else
        {
          ChildLeft.Rect = new Rectangle(Rect.Left, Rect.Top, Rect.Width, (img.Height + 2));
          ChildRight.Rect = new Rectangle(Rect.Left, Rect.Top + (img.Height + 2), Rect.Width,
                                          Rect.Height - (img.Height + 2));
        }
        //(insert into first child we created)
        PackedTextureNode newNode = ChildLeft.Insert(fileName, img, rootImage);
        if (newNode != null)
        {
          return newNode;
        }
        return ChildRight.Insert(fileName, img, rootImage);
      }
    }

    #endregion

    #region variables

    private List<PackedTexture> _packedTextures;
    private const int MAXTEXTURESIZE = 2048; // the maximum width and height which fits into the texture cache - depends on drivers / gfx hardware

    #endregion

    #region ctor/dtor

    public TexturePacker()
    {
    }

    #endregion

    public static void Cleanup()
    {
      // Check if mediaportal is newer than packedgfx, then delete packedgfx (will be recreated at next run)
      // purpose is to have fresh packedgfx containing _all_ media.
      string cacheFile = string.Format(@"{0}\packedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);
      string mpFile = Config.GetFile(Config.Dir.Base, "MediaPortal.exe");
      DateTime cacheCreationTime = File.GetLastWriteTime(cacheFile);
      DateTime mpCreationTime = File.GetLastWriteTime(mpFile);
      DateTime tvLogoChangeTime = Directory.GetLastWriteTime(Thumbs.TVChannel);
      DateTime radioLogoChangeTime = Directory.GetLastWriteTime(Thumbs.Radio);

      if (cacheCreationTime > mpCreationTime)
      {
        // Cache was created after MP got updated - check whether logos have changed
        // People might startup MP after install _before_ they are have finished to fetch logos for everything.
        if ((cacheCreationTime > tvLogoChangeTime) && (cacheCreationTime > radioLogoChangeTime))
        {
          return;
        }
        else
        {
          Log.Info("TexturePacker: Cache from {0} is outdated and will be recreated including current tv ({1}) and radio ({2}) thumbs",
            cacheCreationTime.ToShortDateString(), tvLogoChangeTime.ToShortDateString(), radioLogoChangeTime.ToShortDateString());
        }
      }

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        // Intended for developers that rebuild mp often.
        if (xmlreader.GetValueAsBool("debug", "should_never_remove_packed_skin", false))
        {
          return;
        }
        Log.Info("TexturePacker: Removing packed skin");

        try
        {
          string[] cacheFiles = Directory.GetFiles(GUIGraphicsContext.SkinCacheFolder, "packedgfx*", SearchOption.TopDirectoryOnly);
          for (int i = 0; i < cacheFiles.Length; i++)
          {
            File.Delete(cacheFiles[i]);
          }
        }
        catch (Exception) { }
      }
    }

    private bool Add(PackedTextureNode root, Image img, Image rootImage, string fileName)
    {
      PackedTextureNode node = root.Insert(fileName, img, rootImage);
      if (node != null)
      {
        // Log.Debug("*** TexturePacker: Added {0} at ({1},{2}) {3}x{4}", fileName, node.Rect.X, node.Rect.Y, node.Rect.Width, node.Rect.Height);
        node.FileName = fileName;
        return true;
      }
      // Log.Debug("*** TexturePacker: No room anymore to add: {0}", fileName);
      return false;
    }

    private void SavePackedSkin(string skinName)
    {
      string packedXml = string.Format(@"{0}\packedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);

      using (FileStream fileStream = new FileStream(packedXml, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        ArrayList packedTextures = new ArrayList();
        foreach (PackedTexture packed in _packedTextures)
        {
          packedTextures.Add(packed);
        }
        BinaryFormatter formatter = new BinaryFormatter();
        formatter.Serialize(fileStream, packedTextures);
        fileStream.Close();
      }
    }

    private bool LoadPackedSkin(string skinName)
    {
      Log.Debug("Skin Folder : {0}", GUIGraphicsContext.Skin);
      Log.Debug("Cache Folder: {0}", GUIGraphicsContext.SkinCacheFolder);
      string packedXml = string.Format(@"{0}\packedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);

      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (xmlreader.GetValueAsBool("debug", "skincaching", true) && File.Exists(packedXml))
        {
          using (FileStream fileStream = new FileStream(packedXml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            try
            {
              _packedTextures = new List<PackedTexture>();
              ArrayList packedTextures = new ArrayList();
              BinaryFormatter formatter = new BinaryFormatter();
              packedTextures = (ArrayList)formatter.Deserialize(fileStream);
              foreach (PackedTexture packed in packedTextures)
              {
                _packedTextures.Add(packed);
              }
              fileStream.Close();
              return true;
            }
            catch { }
          }
        }
      }
      return false;
    }

    public void PackSkinGraphics(string skinName)
    {
      Cleanup();
      if (LoadPackedSkin(skinName))
      {
        return;
      }

      _packedTextures = new List<PackedTexture>();
      List<string> files = new List<string>();
      using (Settings xmlreader = new Settings(Config.GetFile(Config.Dir.Config, "MediaPortal.xml")))
      {
        if (xmlreader.GetValueAsBool("debug", "packSkinGfx", true))
        {
          string[] skinFiles = Directory.GetFiles(String.Format(@"{0}\media", skinName), "*.png");
          files.AddRange(skinFiles);
        }
        // workaround for uncommon rendering implementation of volume osd - it won't show up without packedtextures
        else
        {
          string[] skinFiles = Directory.GetFiles(String.Format(@"{0}\media", skinName), "volume*.png");
          string[] forcedCacheFiles = Directory.GetFiles(String.Format(@"{0}\media", skinName), "cached*.png");
          files.Add(String.Format(@"{0}\media\Thumb_Mask.png", skinName));
          files.AddRange(skinFiles);
          files.AddRange(forcedCacheFiles);
        }
        if (xmlreader.GetValueAsBool("debug", "packLogoGfx", false))
        {
          string[] logoFiles = Directory.GetFiles(Config.GetFolder(Config.Dir.Thumbs), "*.png", SearchOption.AllDirectories);
          //string[] thumbFiles = Directory.GetFiles(Config.GetFolder(Config.Dir.Thumbs), "*.jpg", SearchOption.AllDirectories);
          files.AddRange(logoFiles);
          //files.AddRange(thumbFiles);
        }
        if (xmlreader.GetValueAsBool("debug", "packPluginGfx", true))
        {
          string[] weatherFiles = Directory.GetFiles(Config.GetFolder(Config.Dir.Weather), "*.png", SearchOption.AllDirectories);
          string[] tetrisFiles = Directory.GetFiles(String.Format(@"{0}\media\tetris", skinName), "*.png");
          files.AddRange(weatherFiles);
          files.AddRange(tetrisFiles);
        }
      }

      //Determine maximum texture dimensions
      //We limit the max resolution to 2048x2048
      int iMaxWidth = MAXTEXTURESIZE;
      int iMaxHeight = MAXTEXTURESIZE;
      try
      {
        Caps d3dcaps = GUIGraphicsContext.DX9Device.DeviceCaps;
        iMaxWidth = d3dcaps.MaxTextureWidth;
        iMaxHeight = d3dcaps.MaxTextureHeight;
        Log.Info("TexturePacker: D3D device does support {0}x{1} textures", iMaxWidth, iMaxHeight);
      }
      catch (Exception) { }

      if (iMaxWidth > MAXTEXTURESIZE)
      {
        iMaxWidth = MAXTEXTURESIZE;
      }
      if (iMaxHeight > MAXTEXTURESIZE)
      {
        iMaxHeight = MAXTEXTURESIZE;
      }

      while (true)
      {
        bool ImagesLeft = false;

        PackedTexture bigOne = new PackedTexture();
        bigOne.root = new PackedTextureNode();
        bigOne.texture = null;
        bigOne.textureNo = -1;
        using (Bitmap rootImage = new Bitmap(iMaxWidth, iMaxHeight))
        {
          bigOne.root.Rect = new Rectangle(0, 0, iMaxWidth, iMaxHeight);
          for (int i = 0; i < files.Count; ++i)
          {
            if (files[i] == null)
            {
              continue;
            }
            files[i] = files[i].ToLower();
            if (files[i] != string.Empty)
            {
              // Ignore files not needed for MP
              if (files[i].IndexOf("preview.") >= 0)
              {
                files[i] = string.Empty;
                continue;
              }

              bool dontAdd;
              if (AddBitmap(bigOne.root, rootImage, files[i], out dontAdd))
              {
                files[i] = string.Empty;
              }
              else
              {
                if (dontAdd)
                {
                  files[i] = string.Empty;
                }
                else
                {
                  ImagesLeft = true;
                }
              }
            }
          }

          if (!Directory.Exists(GUIGraphicsContext.SkinCacheFolder))
          {
            Directory.CreateDirectory(GUIGraphicsContext.SkinCacheFolder);
          }
          string fileName = String.Format(@"{0}\packedgfx2{1}.png", GUIGraphicsContext.SkinCacheFolder,
                                          _packedTextures.Count);

          rootImage.Save(fileName, ImageFormat.Png);
          Log.Debug("TexturePacker: Cache root {0} filled", fileName);
        }

        _packedTextures.Add(bigOne);
        if (!ImagesLeft)
        {
          break;
        }
      }
      SavePackedSkin(skinName);
    }

    private void LoadPackedGraphics(int index)
    {
      //	return ;
      PackedTexture bigOne = _packedTextures[index];
      Format useFormat = Format.A8R8G8B8;
      //if (IsCompressedTextureFormatOk(Format.Dxt5))
      //{
      //  Log.Debug("TexturePacker: Using DXT5 texture format");
      //  useFormat = Format.Dxt5;
      //}
      if (bigOne.texture == null)
      {
        bigOne.textureNo = -1;

        string fileName = String.Format(@"{0}\packedgfx2{1}.png", GUIGraphicsContext.SkinCacheFolder, index);

        ImageInformation info2 = new ImageInformation();
        Texture tex = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                             fileName,
                                             0, 0, //width/height
                                             1, //mipslevels
                                             0, //Usage.Dynamic,
                                             useFormat,
                                             GUIGraphicsContext.GetTexturePoolType(),
                                             Filter.None,
                                             Filter.None,
                                             (int)0,
                                             ref info2);
        bigOne.texture = tex;
        bigOne.texture.Disposing += new EventHandler(texture_Disposing);

        Log.Info("TexturePacker: Loaded {0} texture:{1}x{2} miplevels:{3}", fileName, info2.Width, info2.Height,
                 tex.LevelCount);
      }
    }

    private void texture_Disposing(object sender, EventArgs e)
    {
      if ((sender as Texture) == null)
      {
        return;
      }
      for (int i = 0; i < _packedTextures.Count; ++i)
      {
        PackedTexture bigOne = _packedTextures[i];
        if (bigOne.texture == (Texture)sender)
        {
          if (bigOne.textureNo >= 0)
          {
            Log.Info("TexturePacker: disposing texture:{0}", bigOne.textureNo);
            FontEngineRemoveTexture(bigOne.textureNo);
            if (Disposing != null)
            {
              Disposing(this, bigOne.textureNo);
            }
          }
          bigOne.texture = null;
          bigOne.textureNo = -1;
          return;
        }
      }
    }

    private bool AddBitmap(PackedTextureNode root, Image rootImage, string file, out bool dontAdd)
    {
      bool result = false;
      dontAdd = false;
      Image bmp = null;

      try
      {
        bmp = ImageFast.FromFile(file);
      }
      catch (ArgumentException)
      {
        Log.Warn("TexturePacker: Fast loading of texture {0} failed - trying safe fallback now", file);
        bmp = Image.FromFile(file);
      }

      //if (bmp.Width >= GUIGraphicsContext.Width || bmp.Height >= GUIGraphicsContext.Height)
      if (bmp.Width >= MAXTEXTURESIZE || bmp.Height >= MAXTEXTURESIZE)
      {
        Log.Warn("TexturePacker: Texture {0} is too large to be cached", file);
        dontAdd = true;
        return false;
      }

      string skinName = String.Format(@"{0}\media", GUIGraphicsContext.Skin).ToLower();

      int pos = file.IndexOf(skinName);
      if (pos >= 0)
      {
        file = file.Remove(pos, skinName.Length);
      }
      if (file.StartsWith(@"\"))
      {
        file = file.Remove(0, 1);
      }
      result = Add(root, bmp, rootImage, file);
      bmp.Dispose();
      bmp = null;

      return result;
    }

    public bool Get(string fileName, out float uoffs, out float voffs, out float umax, out float vmax, out int iWidth,
                    out int iHeight, out Texture tex, out int TextureNo)
    {
      uoffs = voffs = umax = vmax = 0.0f;
      iWidth = iHeight = 0;
      TextureNo = -1;
      tex = null;
      if (_packedTextures == null)
      {
        return false;
      }

      if (fileName.StartsWith(@"\"))
      {
        fileName = fileName.Remove(0, 1);
      }
      fileName = fileName.ToLower();
      if (fileName == string.Empty)
      {
        return false;
      }
      int index = 0;
      foreach (PackedTexture bigOne in _packedTextures)
      {
        PackedTextureNode foundNode = bigOne.root.Get(fileName);
        if (foundNode != null)
        {
          uoffs = ((float)foundNode.Rect.Left + 1) / ((float)bigOne.root.Rect.Width);
          voffs = ((float)foundNode.Rect.Top + 1) / ((float)bigOne.root.Rect.Height);
          umax = ((float)foundNode.Rect.Width - 2) / ((float)bigOne.root.Rect.Width);
          vmax = ((float)foundNode.Rect.Height - 2) / ((float)bigOne.root.Rect.Height);
          iWidth = foundNode.Rect.Width - 2;
          iHeight = foundNode.Rect.Height - 2;
          if (bigOne.texture == null)
          {
            LoadPackedGraphics(index);
          }

          tex = bigOne.texture;
          if (bigOne.textureNo == -1)
          {
            unsafe
            {
              IntPtr ptr = DirectShowUtil.GetUnmanagedTexture(bigOne.texture);
              bigOne.textureNo = FontEngineAddTexture(ptr.ToInt32(), true, (void*)ptr.ToPointer());
              Log.Info("TexturePacker: fontengine add texure:{0}", bigOne.textureNo);
            }
          }
          TextureNo = bigOne.textureNo;
          return true;
        }
        index++;
      }
      return false;
    }

    public void Dispose()
    {
      Log.Info("TexturePacker:Dispose()");
      if (_packedTextures != null)
      {
        foreach (PackedTexture bigOne in _packedTextures)
        {
          if (bigOne.textureNo >= 0)
          {
            Log.Info("TexturePacker: remove texture:{0}", bigOne.textureNo);
            FontEngineRemoveTexture(bigOne.textureNo);
            if (Disposing != null)
            {
              Disposing(this, bigOne.textureNo);
            }
          }
          if (bigOne.texture != null)
          {
            if (!bigOne.texture.Disposed)
            {
              try
              {
                bigOne.texture.Disposing -= new EventHandler(texture_Disposing);
                bigOne.texture.Dispose();
              }
              catch (Exception)
              {
              }
            }
            bigOne.texture = null;
          }
        }
      }
    }

    private bool IsCompressedTextureFormatOk(Format textureFormat)
    {
      if (Manager.CheckDeviceFormat(0, DeviceType.Hardware, GUIGraphicsContext.DirectXPresentParameters.BackBufferFormat,
                                    Usage.None, ResourceType.Textures,
                                    textureFormat))
      {
        Log.Info("TexurePacker: Using compressed textures");
        return true;
      }
      return false;
    }
  }
}