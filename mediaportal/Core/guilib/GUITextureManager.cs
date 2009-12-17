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

//#define DO_RESAMPLE
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.CompilerServices;
using MediaPortal.Configuration;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  public class GUITextureManager
  {
    private const int MAX_THUMB_WIDTH = 512;
    private const int MAX_THUMB_HEIGHT = 512;
    private static List<CachedTexture> _cache = new List<CachedTexture>();
    private static Dictionary<string, CachedTexture> _textureCacheLookup = new Dictionary<string, CachedTexture>();
    private static Dictionary<string, bool> _persistentTextures = new Dictionary<string, bool>();
    private static List<DownloadedImage> _cacheDownload = new List<DownloadedImage>();
    private static TexturePacker _packer = new TexturePacker();

    // singleton. Dont allow any instance of this class
    private GUITextureManager()
    {
    }

    static GUITextureManager()
    {
    }

    ~GUITextureManager()
    {
      dispose(false);
    }

    public static void Dispose()
    {
      dispose(true);
    }

    private static void dispose(bool disposing)
    {
      Log.Debug("TextureManager: Dispose()");
      _packer.Dispose();
      if (disposing)
      {
        foreach (CachedTexture cached in _cache)
        {
          cached.Disposed -= new EventHandler(cachedTexture_Disposed);
          cached.Dispose();
        }
        _cache.Clear();
      }
      _cacheDownload.Clear();

      string[] files = null;

      try
      {
        files = Directory.GetFiles(Config.GetFolder(Config.Dir.Thumbs), "MPTemp*.*");
      }
      catch { }

      if (files != null)
      {
        foreach (string file in files)
        {
          try
          {
            File.Delete(file);
          }
          catch (Exception) { }
        }
      }
    }

    public static CachedTexture GetCachedTexture(string filename)
    {
      if (_textureCacheLookup.ContainsKey(filename))
      {
        return _textureCacheLookup[filename];
      }
      return null;
    }

    public static void StartPreLoad()
    {
      //TODO
    }

    public static void EndPreLoad()
    {
      //TODO
    }

    public static Image Resample(Image imgSrc, int iMaxWidth, int iMaxHeight)
    {
      int width = imgSrc.Width;
      int height = imgSrc.Height;
      while (width < iMaxWidth || height < iMaxHeight)
      {
        width *= 2;
        height *= 2;
      }
      float fAspect = ((float)width) / ((float)height);

      if (width > iMaxWidth)
      {
        width = iMaxWidth;
        height = (int)Math.Round(((float)width) / fAspect);
      }

      if (height > (int)iMaxHeight)
      {
        height = iMaxHeight;
        width = (int)Math.Round(fAspect * ((float)height));
      }

      Bitmap result = new Bitmap(width, height);
      using (Graphics g = Graphics.FromImage(result))
      {
        g.CompositingQuality = Thumbs.Compositing;
        g.InterpolationMode = Thumbs.Interpolation;
        g.SmoothingMode = Thumbs.Smoothing;
        g.DrawImage(imgSrc, new Rectangle(0, 0, width, height));
      }
      return result;
    }

    private static string GetFileName(string fileName)
    {
      if (fileName.Length == 0)
      {
        return "";
      }
      if (fileName == "-")
      {
        return "";
      }
      string lowerFileName = fileName.ToLower().Trim();
      if (lowerFileName.IndexOf(@"http:") >= 0)
      {
        foreach (DownloadedImage image in _cacheDownload)
        {
          if (String.Compare(image.URL, fileName, true) == 0)
          {
            if (image.ShouldDownLoad)
            {
              image.Download();
            }
            return image.FileName;
          }
        }
        DownloadedImage newimage = new DownloadedImage(fileName);
        newimage.Download();
        _cacheDownload.Add(newimage);
        return newimage.FileName;
      }

      if (!File.Exists(fileName))
      {
        if (!Path.IsPathRooted(fileName))
        {
          return GUIGraphicsContext.Skin + @"\media\" + fileName;
        }
      }
      return fileName;
    }

    public static int Load(string fileNameOrg, long lColorKey, int iMaxWidth, int iMaxHeight)
    {
      return Load(fileNameOrg, lColorKey, iMaxWidth, iMaxHeight, false);
    }

    public static int Load(string fileNameOrg, long lColorKey, int iMaxWidth, int iMaxHeight, bool persistent)
    {
      string fileName = GetFileName(fileNameOrg);
      if (String.IsNullOrEmpty(fileName))
      {
        return 0;
      }

      for (int i = 0; i < _cache.Count; ++i)
      {
        CachedTexture cached = (CachedTexture)_cache[i];
        if (String.Compare(cached.Name, fileName, true) == 0)
        {
          return cached.Frames;
        }
      }

      string extension = Path.GetExtension(fileName).ToLower();
      if (extension == ".gif")
      {
        if (!File.Exists(fileName))
        {
          Log.Warn("TextureManager: texture: {0} does not exist", fileName);
          return 0;
        }

        Image theImage = null;
        try
        {
          try
          {
            theImage = ImageFast.FromFile(fileName);
          }
          catch (ArgumentException)
          {
            Log.Warn("TextureManager: Fast loading texture {0} failed using safer fallback", fileName);
            theImage = Image.FromFile(fileName);
          }
          if (theImage != null)
          {
            CachedTexture newCache = new CachedTexture();

            newCache.Name = fileName;
            FrameDimension oDimension = new FrameDimension(theImage.FrameDimensionsList[0]);
            newCache.Frames = theImage.GetFrameCount(oDimension);
            int[] frameDelay = new int[newCache.Frames];
            for (int num2 = 0; (num2 < newCache.Frames); ++num2)
            {
              frameDelay[num2] = 0;
            }

            // Getting Frame duration of an animated Gif image            
            try
            {
              int num1 = 20736;
              PropertyItem item1 = theImage.GetPropertyItem(num1);
              if (item1 != null)
              {
                byte[] buffer1 = item1.Value;
                for (int num2 = 0; (num2 < newCache.Frames); ++num2)
                {
                  frameDelay[num2] = (((buffer1[(num2 * 4)] + (256 * buffer1[((num2 * 4) + 1)])) +
                                       (65536 * buffer1[((num2 * 4) + 2)])) + (16777216 * buffer1[((num2 * 4) + 3)]));
                }
              }
            }
            catch (Exception) { }

            for (int i = 0; i < newCache.Frames; ++i)
            {
              theImage.SelectActiveFrame(oDimension, i);

              //load gif into texture
              using (MemoryStream stream = new MemoryStream())
              {
                theImage.Save(stream, ImageFormat.Png);
                ImageInformation info2 = new ImageInformation();
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                Texture texture = TextureLoader.FromStream(
                  GUIGraphicsContext.DX9Device,
                  stream,
                  0, 0, //width/height
                  1, //mipslevels
                  0, //Usage.Dynamic,
                  Format.A8R8G8B8,
                  GUIGraphicsContext.GetTexturePoolType(),
                  Filter.None,
                  Filter.None,
                  (int)lColorKey,
                  ref info2);
                newCache.Width = info2.Width;
                newCache.Height = info2.Height;
                newCache[i] = new CachedTexture.Frame(fileName, texture, (frameDelay[i] / 5) * 50);
              }
            }

            theImage.Dispose();
            theImage = null;
            newCache.Disposed += new EventHandler(cachedTexture_Disposed);
            if (persistent && !_persistentTextures.ContainsKey(newCache.Name))
            {
              _persistentTextures.Add(newCache.Name, true);
              _cache.Add(newCache);

            }
            _cache.Add(newCache);
            _textureCacheLookup[fileName] = newCache;

            //Log.Info("  TextureManager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
            return newCache.Frames;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TextureManager: exception loading texture {0} - {1}", fileName, ex.Message);
        }
        return 0;
      }

      if (File.Exists(fileName))
      {
        int width, height;
        Texture dxtexture = LoadGraphic(fileName, lColorKey, iMaxWidth, iMaxHeight, out width, out height);
        if (dxtexture != null)
        {
          CachedTexture newCache = new CachedTexture();
          newCache.Name = fileName;
          newCache.Frames = 1;
          newCache.Width = width;
          newCache.Height = height;
          newCache.texture = new CachedTexture.Frame(fileName, dxtexture, 0);
          //Log.Info("  texturemanager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
          newCache.Disposed += new EventHandler(cachedTexture_Disposed);
          if (persistent && !_persistentTextures.ContainsKey(newCache.Name))
          {
            _persistentTextures.Add(newCache.Name, true);
          }
          _cache.Add(newCache);
          _textureCacheLookup[fileName] = newCache;
          return 1;
        }
      }
      return 0;
    }

    public static int LoadFromMemory(string name, long lColorKey, int iMaxWidth, int iMaxHeight, out Texture texture)
    {
      Log.Debug("TextureManager: load from memory: {0} {1} {2}", name, iMaxWidth, iMaxHeight);
      string cacheName = name;

      texture = null;
      try
      {
        CachedTexture newCache = new CachedTexture();

        Bitmap bitmap = new Bitmap(iMaxWidth, iMaxHeight, PixelFormat.Format32bppArgb);
        Image memoryImage = bitmap;

        newCache.Name = cacheName;
        newCache.Frames = 1;

        //load gif into texture
        using (MemoryStream stream = new MemoryStream())
        {
          memoryImage.Save(stream, ImageFormat.Png);
          ImageInformation info2 = new ImageInformation();
          stream.Flush();
          stream.Seek(0, SeekOrigin.Begin);
          texture = TextureLoader.FromStream(
            GUIGraphicsContext.DX9Device,
            stream,
            0, 0, //width/height
            1, //mipslevels
            Usage.Dynamic, //Usage.Dynamic,
            Format.A8R8G8B8,
            Pool.Default,
            Filter.None,
            Filter.None,
            (int)lColorKey,
            ref info2);
          newCache.Width = info2.Width;
          newCache.Height = info2.Height;
          newCache.texture = new CachedTexture.Frame(cacheName, texture, 0);
        }
        //memoryImage.Dispose();
        //memoryImage = null;
        newCache.Disposed += new EventHandler(cachedTexture_Disposed);
        _cache.Add(newCache);
        _textureCacheLookup[cacheName] = newCache;

        Log.Debug("TextureManager: added: memoryImage  " + " total: " + _cache.Count + " mem left: " +
                 GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
        return newCache.Frames;
      }
      catch (Exception ex)
      {
        Log.Error("TextureManager: exception loading texture memoryImage");
        Log.Error(ex);
      }
      return 0;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private static void cachedTexture_Disposed(object sender, EventArgs e)
    {
      // a texture in the cache has been disposed of! remove from the cache
      for (int i = 0; i < _cache.Count; ++i)
      {
        if (_cache[i] == sender)
        {
          //Log.Debug("TextureManager: Already disposed texture - cleaning up the cache...");
          _cache[i].Disposed -= new EventHandler(cachedTexture_Disposed);
          if (_persistentTextures.ContainsKey(_cache[i].Name))
            _persistentTextures.Remove(_cache[i].Name);
          _cache.Remove(_cache[i]);
        }
      }
    }

    private static Texture LoadGraphic(string fileName, long lColorKey, int iMaxWidth, int iMaxHeight, out int width, out int height)
    {
      width = 0;
      height = 0;
      Image imgSrc = null;
      Texture texture = null;
      try
      {
#if DO_RESAMPLE
        imgSrc=Image.FromFile(fileName);   
        if (imgSrc==null) return null;
				//Direct3D prefers textures which height/width are a power of 2
				//doing this will increases performance
				//So the following core resamples all textures to
				//make sure all are 2x2, 4x4, 8x8, 16x16, 32x32, 64x64, 128x128, 256x256, 512x512
				int w=-1,h=-1;
				if (imgSrc.Width >2   && imgSrc.Width < 4)  w=2;
				if (imgSrc.Width >4   && imgSrc.Width < 8)  w=4;
				if (imgSrc.Width >8   && imgSrc.Width < 16) w=8;
				if (imgSrc.Width >16  && imgSrc.Width < 32) w=16;
				if (imgSrc.Width >32  && imgSrc.Width < 64) w=32;
				if (imgSrc.Width >64  && imgSrc.Width <128) w=64;
				if (imgSrc.Width >128 && imgSrc.Width <256) w=128;
				if (imgSrc.Width >256 && imgSrc.Width <512) w=256;
				if (imgSrc.Width >512 && imgSrc.Width <1024) w=512;


				if (imgSrc.Height >2   && imgSrc.Height < 4)  h=2;
				if (imgSrc.Height >4   && imgSrc.Height < 8)  h=4;
				if (imgSrc.Height >8   && imgSrc.Height < 16) h=8;				
				if (imgSrc.Height >16  && imgSrc.Height < 32) h=16;
				if (imgSrc.Height >32  && imgSrc.Height < 64) h=32;
				if (imgSrc.Height >64  && imgSrc.Height <128) h=64;
				if (imgSrc.Height >128 && imgSrc.Height <256) h=128;
				if (imgSrc.Height >256 && imgSrc.Height <512) h=256;
				if (imgSrc.Height >512 && imgSrc.Height <1024) h=512;
				if (w>0 || h>0)
				{
					if (h > w) w=h;
					Log.Info("TextureManager: resample {0}x{1} -> {2}x{3} {4}",
												imgSrc.Width,imgSrc.Height, w,w,fileName);

					Image imgResampled=Resample(imgSrc,w, h);
					imgSrc.Dispose();
					imgSrc=imgResampled;
					imgResampled=null;
				}
#endif

        Format fmt = Format.A8R8G8B8;

        ImageInformation info2 = new ImageInformation();
        texture = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                         fileName,
                                         0, 0, //width/height
                                         1, //mipslevels
                                         0, //Usage.Dynamic,
                                         fmt,
                                         GUIGraphicsContext.GetTexturePoolType(),
                                         Filter.None,
                                         Filter.None,
                                         (int)lColorKey,
                                         ref info2);
        width = info2.Width;
        height = info2.Height;
      }
      catch (Exception)
      {
        Log.Error("TextureManager: LoadGraphic - invalid thumb({0})", fileName);
        Format fmt = Format.A8R8G8B8;
        string fallback = GUIGraphicsContext.Skin + @"\media\" + "black.png";

        ImageInformation info2 = new ImageInformation();
        texture = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                         fallback,
                                         0, 0, //width/height
                                         1, //mipslevels
                                         0, //Usage.Dynamic,
                                         fmt,
                                         GUIGraphicsContext.GetTexturePoolType(),
                                         Filter.None,
                                         Filter.None,
                                         (int)lColorKey,
                                         ref info2);
        width = info2.Width;
        height = info2.Height;
      }
      finally
      {
        if (imgSrc != null)
        {
          imgSrc.Dispose();
        }
      }
      return texture;
    }

    //public static Image GetImage(string fileNameOrg)
    //{
    //  string fileName = GetFileName(fileNameOrg);
    //  if (fileNameOrg.StartsWith("["))
    //    fileName = fileNameOrg;
    //  if (fileName == "") return null;

    //  for (int i = 0; i < _cache.Count; ++i)
    //  {
    //    CachedTexture cached = (CachedTexture)_cache[i];
    //    if (String.Compare(cached.Name, fileName, true) == 0)
    //    {
    //      if (cached.image != null)
    //        return cached.image;
    //      else
    //      {

    //        try
    //        {
    //          cached.image = Image.FromFile(fileName);             

    //            using (Graphics g = Graphics.FromImage(cached.image))
    //            {
    //              g.DrawImage(cached.image, 0, 0);
    //            }

    //        }
    //        catch (Exception ex)
    //        {
    //          Log.Error("TextureManage: GetImage({0}) ", fileName);
    //          Log.Error(ex);
    //          return null;
    //        }
    //        return cached.image;
    //      }
    //    }
    //  }

    //  if (!System.IO.File.Exists(fileName)) return null;
    //  Image img = null;
    //  try
    //  {
    //      img = Image.FromFile(fileName);
    //      using (Graphics g = Graphics.FromImage(img))
    //      {
    //        g.DrawImage(img, 0, 0);
    //      }        
    //  }
    //  catch (Exception ex)
    //  {
    //    Log.Error("TextureManage: GetImage({0})", fileName);
    //    Log.Error(ex);
    //    return null;
    //  }

    //  if (img != null)
    //  {
    //    CachedTexture newCache = new CachedTexture();
    //    newCache.Frames = 1;
    //    newCache.Name = fileName;
    //    newCache.Width = img.Width;
    //    newCache.Height = img.Height;
    //    newCache.image = img;
    //    //Log.Info("  texturemanager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
    //    newCache.Disposed += new EventHandler(cachedTexture_Disposed);
    //    _cache.Add(newCache);
    //    return img;
    //  }
    //  return null;
    //}

    public static CachedTexture.Frame GetTexture(string fileNameOrg, int iImage, out int iTextureWidth, out int iTextureHeight)
    {
      iTextureWidth = 0;
      iTextureHeight = 0;
      string fileName = "";
      if (!fileNameOrg.StartsWith("["))
      {
        fileName = GetFileName(fileNameOrg);
        if (fileName == "")
        {
          return null;
        }
      }
      else
      {
        fileName = fileNameOrg;
      }
      for (int i = 0; i < _cache.Count; ++i)
      {
        CachedTexture cached = (CachedTexture)_cache[i];
        if (String.Compare(cached.Name, fileName, true) == 0)
        {
          iTextureWidth = cached.Width;
          iTextureHeight = cached.Height;
          return (CachedTexture.Frame)cached[iImage];
        }
      }
      return null;
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void ReleaseTexture(string fileName)
    {
      if (string.IsNullOrEmpty(fileName))
      {
        return;
      }

      //dont dispose radio/tv logo's since they are used by the overlay windows
      if (fileName.ToLower().IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos")) >= 0)
      {
        return;
      }
      if (fileName.ToLower().IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, "radio")) >= 0)
      {
        return;
      }

      for (int i = 0; i < _cache.Count; i++)
      {
        try
        {
          CachedTexture oldImage = _cache[i];
          if (!string.IsNullOrEmpty(oldImage.Name))
          {
            if (String.Compare(oldImage.Name, fileName, true) == 0)
            {
              lock (oldImage)
              {
                //Log.Debug("TextureManager: Dispose:{0} Frames:{1} Total:{2} Mem left:{3}", oldImage.Name, oldImage.Frames, _cache.Count, Convert.ToString(GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1000));
                _cache.Remove(oldImage);
                _textureCacheLookup.Remove(oldImage.Name);
                oldImage.Disposed -= new EventHandler(cachedTexture_Disposed);
                oldImage.Dispose();
                break;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Log.Error("TextureManager: Error in ReleaseTexture({0}) - {1}", fileName, ex.Message);
        }
      }
    }

    public static void PreLoad(string fileName)
    {
      //TODO
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    public static void CleanupThumbs()
    {
      Log.Debug("TextureManager: CleanupThumbs()");
      try
      {
        List<CachedTexture> newCache = new List<CachedTexture>();
        foreach (CachedTexture cached in _cache)
        {
          if (IsTemporary(cached.Name))
          {
            // Log.Debug("TextureManager: dispose: " + cached.Name + " total: " + _cache.Count + " mem left: " + Convert.ToString(GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1000));
            cached.Disposed -= new EventHandler(cachedTexture_Disposed);
            cached.Dispose();
          }
          else
          {
            newCache.Add(cached);
          }
        }
        _cache.Clear();
        _cache = newCache;
      }
      catch (Exception ex)
      {
        Log.Error("TextureManage: Error cleaning up thumbs - {0}", ex.Message);
      }
    }

    public static bool IsTemporary(string fileName)
    {
      if (String.IsNullOrEmpty(fileName))
      {
        return false;
      }
      if (fileName == "-")
      {
        return false;
      }

      if (fileName.ToLower().IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos")) >= 0)
      {
        return false;
      }
      if (fileName.ToLower().IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, "radio")) >= 0)
      {
        return false;
      }

      // check if this texture was loaded to be persistent
      if (_persistentTextures.ContainsKey(fileName))
      {
        return false;
      }

      /* Temporary: (textures that are disposed)
       * - all not skin images
       * 
       * NOT Temporary: (textures that are kept in cache)
       * - all skin graphics
       * 
       */

      // Get fullpath and file name
      string fullFileName = fileName;
      if (!File.Exists(fileName))
      {
        if (!Path.IsPathRooted(fileName))
        {
          fullFileName = GUIGraphicsContext.Skin + @"\media\" + fileName;
        }
      }

      // Check if skin file
      if (fullFileName.ToLower().IndexOf(@"skin\") >= 0)
      {
        if (fullFileName.ToLower().IndexOf(@"media\animations\") >= 0)
        {
          return true;
        }
        if (fullFileName.ToLower().IndexOf(@"media\Tetris\") >= 0)
        {
          return true;
        }
        return false;
      }
      return true;
    }

    public static void Init()
    {
      _packer.PackSkinGraphics(GUIGraphicsContext.Skin);
    }

    public static bool GetPackedTexture(string fileName, out float uoff, out float voff, out float umax, out float vmax,
                                        out int textureWidth, out int textureHeight, out Texture tex,
                                        out int _packedTextureNo)
    {
      return _packer.Get(fileName, out uoff, out voff, out umax, out vmax, out textureWidth, out textureHeight, out tex,
                         out _packedTextureNo);
    }

    public static void Clear()
    {
      _packer.Dispose();
      _packer = new TexturePacker();
      _packer.PackSkinGraphics(GUIGraphicsContext.Skin);

      _cache.Clear();
      _cacheDownload.Clear();
    }
  }
}
