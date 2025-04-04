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

//#define DO_RESAMPLE
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.guilib;
using MediaPortal.Util;

using SharpDX.Direct3D9;
//using InvalidDataException = SharpDX.Direct3D.InvalidDataException;

namespace MediaPortal.GUI.Library
{
  public class GUITextureManager
  {
    private static readonly ConcurrentDictionary<string, CachedTexture> _cacheTextures =
      new ConcurrentDictionary<string, CachedTexture>();

    private static readonly ConcurrentDictionary<string, bool> _persistentTextures =
      new ConcurrentDictionary<string, bool>();

    private static readonly ConcurrentDictionary<string, DownloadedImage> _cacheDownload =
      new ConcurrentDictionary<string, DownloadedImage>();

    private static TexturePacker _packer = new TexturePacker();

    private GUITextureManager() { }

    ~GUITextureManager()
    {
      DisposeInternal();
    }

    public static void Dispose()
    {
      DisposeInternal();
    }

    private static void DisposeInternal()
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        Log.Debug("TextureManager: Dispose()");
        _packer.SafeDispose();
        ClearTextureCache();
        ClearDownloadCache();

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
            catch (Exception ex)
            {
              Log.Error("GUITextureManager DisposeInternal: " + ex.Message);
            }
          }
        }
      }
    }

    internal static CachedTexture GetCachedTexture(string filename)
    {
      string cacheKey = filename.ToLowerInvariant();
      CachedTexture texture;
      if (_cacheTextures.TryGetValue(cacheKey, out texture))
      {
        return texture;
      }
      return null;
    }

    public static Texture GetTexture(string strName, int iFrameNr, out int iDuration)
    {
      string strCacheKey = strName.ToLowerInvariant();
      iDuration = -1;
      CachedTexture texture;
      if (_cacheTextures.TryGetValue(strCacheKey, out texture) && iFrameNr >= 0 && iFrameNr < texture.Frames)
      {
        TextureFrame frame = texture[iFrameNr];
        iDuration = frame.Duration;
        return frame.Image;
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
      try
      {
        if (fileName.Length == 0)
        {
          return string.Empty;
        }
        if (fileName == "-")
        {
          return string.Empty;
        }
        string lowerFileName = fileName.ToLowerInvariant().Trim();
        if (lowerFileName.IndexOf(@"http:", StringComparison.Ordinal) >= 0)
        {
          DownloadedImage image;
          if (!_cacheDownload.TryGetValue(lowerFileName, out image))
          {
            image = new DownloadedImage(fileName);
            _cacheDownload[lowerFileName] = image;
          }

          if (image.ShouldDownLoad)
          {
            image.Download();
          }

          return image.FileName;
        }

        if (!MediaPortal.Util.Utils.FileExistsInCache(fileName))
        {
          if (!Path.IsPathRooted(fileName))
          {
            return GUIGraphicsContext.GetThemedSkinFile(@"\media\" + fileName);
          }
        }
        return fileName;
      }
      catch (Exception ex)
      {
        Log.Error("GUITextureManager GetFileName: '" + fileName+"' "+ex.Message);
        // ignored
        return string.Empty;
      }
    }

    public static int Load(string fileNameOrg, long lColorKey, int iMaxWidth, int iMaxHeight)
    {
      return Load(fileNameOrg, lColorKey, 0, iMaxWidth, iMaxHeight, false);
    }

    public static int Load(string fileNameOrg, long lColorKey, int iMaxWidth, int iMaxHeight, bool persistent)
    {
      return Load(fileNameOrg, lColorKey, 0, iMaxWidth, iMaxHeight, persistent);
    }

    public static int Load(string fileNameOrg, long lColorKey, int iRotation, int iMaxWidth, int iMaxHeight)
    {
      return Load(fileNameOrg, lColorKey, iRotation, iMaxWidth, iMaxHeight, false);
    }

    public static int Load(string fileNameOrg, long lColorKey, int iRotation, int iMaxWidth, int iMaxHeight, bool persistent)
    {
      return Load(fileNameOrg, lColorKey, iRotation, iMaxWidth, iMaxHeight, persistent, out _);
    }
    public static int Load(string fileNameOrg, long lColorKey, int iRotation, int iMaxWidth, int iMaxHeight, bool persistent, out int iId)
    {
      iId = -1;
      string fileName = GetFileName(fileNameOrg);
      string cacheKey = fileName.ToLowerInvariant();
      if (String.IsNullOrEmpty(fileName))
      {
        return 0;
      }

      CachedTexture cached;
      if (_cacheTextures.TryGetValue(cacheKey, out cached))
      {
        System.Threading.Interlocked.Increment(ref cached.InstanceCounter);
        iId = cached.ID;
        return cached.Frames;
      }

      string extension = Path.GetExtension(fileName).ToLowerInvariant();
      if (extension == ".gif")
      {
        Image theImage = null;
        try
        {
          try
          {
            theImage = ImageFast.FromFile(fileName);
          }
          catch (FileNotFoundException ex)
          {
            Log.Warn("TextureManager: Gif texture: {0} does not exist {1}", fileName, ex.Message);
            return 0;
          }
          catch (Exception ex)
          {
            Log.Warn("TextureManager: Gif Fast loading texture {0} failed using safer fallback {1}", fileName, ex.Message);
            theImage = Image.FromFile(fileName);
          }
          if (theImage != null)
          {
            CachedTexture newCache = new CachedTexture();
            iId = newCache.ID;
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
            catch (Exception ex)
            {
              Log.Error("GUITextureManager Gif Load: " + ex.Message);
            }

            for (int i = 0; i < newCache.Frames; ++i)
            {
              theImage.SelectActiveFrame(oDimension, i);

              //load gif into texture
              using (MemoryStream stream = new MemoryStream())
              {
                theImage.Save(stream, ImageFormat.Png);
                ImageInformation info2;
                stream.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                Texture texture = Texture.FromStream(
                  GUIGraphicsContext.DX9Device,
                  stream,
                  0, // size
                  0, 0, // width/height
                  1,    // mipslevels
                  Usage.None, //0   // Usage.Dynamic,
                  Format.A8R8G8B8,
                  GUIGraphicsContext.GetTexturePoolType(),
                  Filter.None,
                  Filter.None,
                  (int)lColorKey,
                  out info2);
                newCache.Width = info2.Width;
                newCache.Height = info2.Height;
                newCache[i] = new TextureFrame(fileName, texture, (frameDelay[i] / 5) * 50);
              }
            }

            theImage.SafeDispose();
            theImage = null;
            newCache.Disposed += new EventHandler(cachedTexture_Disposed);
            if (persistent && !_persistentTextures.ContainsKey(cacheKey))
            {
              _persistentTextures[cacheKey] = true;
            }

            _cacheTextures[cacheKey] = newCache;

            //Log.Info("  TextureManager:added:" + fileName + " total:" + _cache.Count + " mem left:" + GUIGraphicsContext.DX9Device.AvailableTextureMemory.ToString());
            return newCache.Frames;
          }
        }
        catch (Exception ex)
        {
          Log.Error("TextureManager: Gif exception loading texture {0}", fileName);
          Log.Error(ex);
        }
        return 0;
      }

      try
      {
        if (MediaPortal.Util.Utils.FileExistsInCache(fileName))
        {
          int width = 0;
          int height = 0;
          Texture dxtexture = null;
          if (iRotation != 0)
          {
            dxtexture = LoadGraphic(fileName, lColorKey, iMaxWidth, iMaxHeight, iRotation, out width, out height);
          }
          if (dxtexture == null)
          {
            dxtexture = LoadGraphic(fileName, lColorKey, iMaxWidth, iMaxHeight, out width, out height);
          }
          if (dxtexture != null)
          {
            CachedTexture newCache = new CachedTexture();
            newCache.Name = fileName;
            newCache.Frames = 1;
            newCache.Width = width;
            newCache.Height = height;
            newCache.Texture = new TextureFrame(fileName, dxtexture, 0);
            newCache.Disposed += new EventHandler(cachedTexture_Disposed);
            iId = newCache.ID;

            if (persistent && !_persistentTextures.ContainsKey(cacheKey))
            {
              _persistentTextures[cacheKey] = true;
            }

            _cacheTextures[cacheKey] = newCache;
            return 1;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUITextureManager Load2: {0} for {1}", ex.Message, fileName);
        return 0;
      }
      return 0;
    }

    public static int LoadFromMemory(Image memoryImage, string name, long lColorKey, int iMaxWidth, int iMaxHeight)
    {
      bool bDebugLog = !name.StartsWith("[NoLog:");
      if (bDebugLog)
      {
        Log.Debug("TextureManager: load from memory: {0}", name);
      }
      string cacheName = name;
      string cacheKey = name.ToLowerInvariant();

      CachedTexture cached;
      if (_cacheTextures.TryGetValue(cacheKey, out cached))
      {
        System.Threading.Interlocked.Increment(ref cached.InstanceCounter);
        return cached.Frames;
      }

      if (memoryImage == null)
      {
        return 0;
      }
      if (memoryImage.FrameDimensionsList == null)
      {
        return 0;
      }
      if (memoryImage.FrameDimensionsList.Length == 0)
      {
        return 0;
      }

      try
      {
        CachedTexture newCache = new CachedTexture();

        newCache.Name = cacheName;
        FrameDimension oDimension = new FrameDimension(memoryImage.FrameDimensionsList[0]);
        newCache.Frames = memoryImage.GetFrameCount(oDimension);
        if (newCache.Frames != 1)
        {
          return 0;
        }
        //load gif into texture
        using (MemoryStream stream = new MemoryStream())
        {
          memoryImage.Save(stream, ImageFormat.Png);
          ImageInformation info2;
          stream.Flush();
          stream.Seek(0, SeekOrigin.Begin);
          Texture texture = Texture.FromStream(
            GUIGraphicsContext.DX9Device,
            stream,
            0, // size
            0, 0, //width/height
            1, //mipslevels
            Usage.None, //Usage.Dynamic,
            Format.A8R8G8B8,
            GUIGraphicsContext.GetTexturePoolType(),
            Filter.None,
            Filter.None,
            (int)lColorKey,
            out info2);
          newCache.Width = info2.Width;
          newCache.Height = info2.Height;
          newCache.Texture = new TextureFrame(cacheName, texture, 0);
        }
        memoryImage.SafeDispose();
        memoryImage = null;
        newCache.Disposed += new EventHandler(cachedTexture_Disposed);

        _cacheTextures[cacheKey] = newCache;

        if (bDebugLog)
        {
          Log.Debug("TextureManager: added: memoryImage  " + " total count: " + _cacheTextures.Count + ", mem left (MB): " +
                    ((uint)GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576));
        }
        return newCache.Frames;
      }
      catch (Exception ex)
      {
        Log.Error("TextureManager: exception loading texture memoryImage");
        Log.Error(ex);
      }
      return 0;
    }

    public static int LoadFromMemoryEx(Image memoryImage, string name, long lColorKey, out Texture texture)
    {
      return LoadFromMemoryEx(memoryImage, name, lColorKey, out texture, out _);
    }
    public static int LoadFromMemoryEx(Image memoryImage, string name, long lColorKey, out Texture texture, out int iId)
    {
      iId = -1;
      bool bDebugLog = !name.StartsWith("[NoLog:");
      if (bDebugLog)
      {
        Log.Debug("TextureManagerEx: load from memory: {0}", name);
      }
      string cacheName = name;
      string cacheKey = cacheName.ToLowerInvariant();

      texture = null;
      CachedTexture cached;
      if (_cacheTextures.TryGetValue(cacheKey, out cached))
      {
        System.Threading.Interlocked.Increment(ref cached.InstanceCounter);
        iId = cached.ID;
        return cached.Frames;
      }

      if (memoryImage == null)
      {
        return 0;
      }
      try
      {
        CachedTexture newCache = new CachedTexture();

        newCache.Name = cacheName;
        newCache.Frames = 1;

        iId = newCache.ID;

        //load gif into texture
        using (MemoryStream stream = new MemoryStream())
        {
          memoryImage.Save(stream, ImageFormat.Png);
          ImageInformation info2;
          stream.Flush();
          stream.Seek(0, SeekOrigin.Begin);
          texture = Texture.FromStream(
            GUIGraphicsContext.DX9Device,
            stream,
            0, //size
            0, 0, //width/height
            1, //mipslevels
            Usage.Dynamic, //Usage.Dynamic,
            Format.A8R8G8B8,
            Pool.Default,
            Filter.None,
            Filter.None,
            (int)lColorKey,
            out info2);
          newCache.Width = info2.Width;
          newCache.Height = info2.Height;
          newCache.Texture = new TextureFrame(cacheName, texture, 0);
        }

        newCache.Disposed += new EventHandler(cachedTexture_Disposed);

        _cacheTextures[cacheKey] = newCache;

        if (bDebugLog)
        {
          Log.Debug("TextureManager: added: memoryImage  " + " total count: " + _cacheTextures.Count + ", mem left (MB): " +
                    ((uint)GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576));
        }
        return newCache.Frames;
      }
      catch (Exception ex)
      {
        Log.Error("TextureManager: exception loading texture memoryImage");
        Log.Error(ex);
      }
      return 0;
    }

    public static int LoadFromMemoryEx(Image[] memoryImages, int[] durations, string strName, long lColorKey)
    {
      bool bDebugLog = !strName.StartsWith("[NoLog:");
      if (bDebugLog)
      {
        Log.Debug("TextureManagerEx: load from memory: {0}", strName);
      }
      string strCacheName = strName;
      string strCacheKey = strCacheName.ToLowerInvariant();

      CachedTexture cached;
      if (_cacheTextures.TryGetValue(strCacheKey, out cached))
      {
        System.Threading.Interlocked.Increment(ref cached.InstanceCounter);
        return cached.Frames;
      }

      if (memoryImages == null || durations == null || memoryImages.Length == 0 || durations.Length != memoryImages.Length)
        return 0;

      try
      {
        Texture texture;
        CachedTexture newCache = new CachedTexture();

        newCache.Name = strName;
        newCache.Frames = memoryImages.Length;

        for (int iIdx = 0; iIdx < memoryImages.Length; iIdx++)
        {
          using (MemoryStream stream = new MemoryStream())
          {
            memoryImages[iIdx].Save(stream, ImageFormat.Png);
            ImageInformation info2;
            stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            texture = Texture.FromStream(
              GUIGraphicsContext.DX9Device,
              stream,
              0, //size
              0, 0, //width/height
              1, //mipslevels
              Usage.Dynamic,
              Format.A8R8G8B8,
              Pool.Default,
              Filter.None,
              Filter.None,
              (int)lColorKey,
              out info2);

            newCache.Width = info2.Width;
            newCache.Height = info2.Height;
            newCache[iIdx] = new TextureFrame(strCacheName, texture, Math.Max(10, durations[iIdx]));
          }
        }

        newCache.Disposed += new EventHandler(cachedTexture_Disposed);

        _cacheTextures[strCacheKey] = newCache;

        if (bDebugLog)
        {
          Log.Debug("TextureManager: added: memoryImage  " + " total count: " + _cacheTextures.Count + ", mem left (MB): " +
                    ((uint)GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576));
        }

        return newCache.Frames;
      }
      catch (Exception ex)
      {
        Log.Error("TextureManager: exception loading texture memoryImage");
        Log.Error(ex);
      }

      return 0;
    }

    private static void cachedTexture_Disposed(object sender, EventArgs e)
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        CachedTexture cached = sender as CachedTexture;
        if (cached != null)
        {
          string cacheKey = cached.Name.ToLowerInvariant();

          cached.Disposed -= new EventHandler(cachedTexture_Disposed);

          bool removed;
          _persistentTextures.TryRemove(cacheKey, out removed);

          CachedTexture removedItem;
          _cacheTextures.TryRemove(cacheKey, out removedItem);
        }
      }
    }

    private static Texture LoadGraphic(string fileName, long lColorKey, int iMaxWidth, int iMaxHeight, 
                                       out int width, out int height)
    {
      width = 0;
      height = 0;
#if DO_RESAMPLE
      Image imgSrc = null;
#endif
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
					imgSrc.SafeDispose();
					imgSrc=imgResampled;
					imgResampled=null;
				}
#endif

        Format fmt = Format.A8R8G8B8;

        ImageInformation info2;

        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          //Check for WebP
          if (fs.Length >= 16)
          {
            byte[] header = new byte[16];
            fs.Read(header, 0, 16);
            fs.Position = 0;
            if (System.Text.Encoding.ASCII.GetString(header, 8, 7).Equals("WEBPVP8"))
            {
              Log.Debug("TextureManager: LoadGraphic - WebP detected");
              byte[] data = new byte[fs.Length];
              fs.Read(data, 0, data.Length);
              using (Bitmap bmp = Imaging.WebP.Decode(data))
              {
                using (MemoryStream ms = new MemoryStream())
                {
                  bmp.Save(ms, ImageFormat.Bmp);
                  ms.Position = 0;
                  texture = Texture.FromStream(GUIGraphicsContext.DX9Device,
                                           ms,
                                           0, //size
                                           0, 0, //width/height
                                           1, //mipslevels
                                           Usage.None, //Usage.Dynamic,
                                           Format.A8R8G8B8,
                                           GUIGraphicsContext.GetTexturePoolType(),
                                           Filter.None,
                                           Filter.None,
                                           (int)lColorKey,
                                           out info2);

                }
              }
              width = info2.Width;
              height = info2.Height;
            }
          }
        }

        if (texture == null)
        {
          texture = Texture.FromFile(GUIGraphicsContext.DX9Device,
                                         fileName,
                                         0, 0, //width/height
                                         1, //mipslevels
                                         0, //Usage.Dynamic,
                                         fmt,
                                         GUIGraphicsContext.GetTexturePoolType(),
                                         Filter.None,
                                         Filter.None,
                                         (int)lColorKey,
                                         out info2);
          width = info2.Width;
          height = info2.Height;
        }
      }
      catch (InvalidDataException e1) // weird : should have been FileNotFoundException when file is missing ??
      {
        Log.Debug("TextureManager: LoadGraphic - {0} error {1}, trying copy to Image first", fileName, e1.Message);
        using (Stream str = new MemoryStream())
        {
          using (Image image = ImageFast.FromFile(fileName))
          using (Bitmap result = new Bitmap(image.Width, image.Height))
          using (Graphics g = Graphics.FromImage(result))
          {
            g.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            result.Save(str, ImageFormat.Png);
          }
          str.Position = 0;
          try
          {
            ImageInformation info2;
            texture = Texture.FromStream(GUIGraphicsContext.DX9Device,
                                     str,
                                     0, //size
                                     0, 0, //width/height
                                     1, //mipslevels
                                     Usage.None, //Usage.Dynamic,
                                     Format.A8R8G8B8,
                                     GUIGraphicsContext.GetTexturePoolType(),
                                     Filter.None,
                                     Filter.None,
                                     (int)lColorKey,
                                     out info2);
            width = info2.Width;
            height = info2.Height;
          }
          catch (Exception e2)
          {
            //we need to catch this on higer level.         
            throw e2;
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TextureManager: LoadGraphic - invalid thumb({0}) {1}", fileName, ex.Message);
        Format fmt = Format.A8R8G8B8;
        string fallback = GUIGraphicsContext.GetThemedSkinFile(@"\media\" + "black.png");

        ImageInformation info2;
        texture = Texture.FromFile(GUIGraphicsContext.DX9Device,
                                         fallback,
                                         0, 0, //width/height
                                         1, //mipslevels
                                         0, //Usage.Dynamic,
                                         fmt,
                                         GUIGraphicsContext.GetTexturePoolType(),
                                         Filter.None,
                                         Filter.None,
                                         (int)lColorKey,
                                         out info2);
        width = info2.Width;
        height = info2.Height;
      }
#if DO_RESAMPLE
      finally
      {
        if (imgSrc != null)
        {
          imgSrc.SafeDispose();
        }
      }
#endif
      return texture;
    }

    private static Texture LoadGraphic(string fileName, long lColorKey, int iMaxWidth, int iMaxHeight, int iRotation,
                                       out int width, out int height)
    {
      width = 0;
      height = 0;
      if (string.IsNullOrEmpty(fileName))
      {
        return null;
      }

      Texture texture = null;
      try
      {
        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          Image theImage = null;

          //Check for WebP
          if (fs.Length >= 16)
          {
            byte[] header = new byte[16];
            fs.Read(header, 0, 16);
            fs.Position = 0;
            if (System.Text.Encoding.ASCII.GetString(header, 8, 7).Equals("WEBPVP8"))
            {
              Log.Debug("TextureManager: LoadGraphic - WebP detected");
              byte[] data = new byte[fs.Length];
              fs.Read(data, 0, data.Length);
              theImage = Imaging.WebP.Decode(data);
              if (theImage == null)
                throw new Exception("Failed to load WebP image.");
            }
          }

          if (theImage == null)
            theImage = Image.FromStream(fs, true, false);

          using (theImage)
          {
            if (theImage == null)
            {
              return null;
            }
            Log.Debug("TextureManager: Fast loaded texture {0}|{1}", iRotation, fileName);

            if (iRotation > 0)
            {
              RotateFlipType fliptype;
              switch (iRotation)
              {
                case 1:
                  fliptype = RotateFlipType.Rotate90FlipNone;
                  theImage.RotateFlip(fliptype);
                  break;
                case 2:
                  fliptype = RotateFlipType.Rotate180FlipNone;
                  theImage.RotateFlip(fliptype);
                  break;
                case 3:
                  fliptype = RotateFlipType.Rotate270FlipNone;
                  theImage.RotateFlip(fliptype);
                  break;
                default:
                  fliptype = RotateFlipType.RotateNoneFlipNone;
                  break;
              }
            }
            width = theImage.Size.Width;
            height = theImage.Size.Height;

            texture = Picture.ConvertImageToTexture((Bitmap)theImage, lColorKey, Format.A8R8G8B8, out width, out height);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Error("TextureManager: LoadGraphic: exception loading {0} {1}", fileName, ex.Message);
      }
      return texture;
    }

    internal static TextureFrame GetTexture(string fileNameOrg, int iImage,
                                            out int iTextureWidth, out int iTextureHeight)
    {
      iTextureWidth = 0;
      iTextureHeight = 0;
      string fileName = string.Empty;
      if (!fileNameOrg.StartsWith("["))
      {
        fileName = GetFileName(fileNameOrg);
        if (fileName == string.Empty)
        {
          return null;
        }
      }
      else
      {
        fileName = fileNameOrg;
      }

      CachedTexture cached;
      string cacheKey = fileName.ToLowerInvariant();
      if (_cacheTextures.TryGetValue(cacheKey, out cached))
      {
        iTextureWidth = cached.Width;
        iTextureHeight = cached.Height;
        return cached[iImage];
      }

      return null;
    }

    public static void ReleaseTexture(string fileName)
    {
      ReleaseTexture(fileName, true, -1);
    }
    public static void ReleaseTexture(string fileName, bool bForce)
    {
      ReleaseTexture(fileName, bForce, -1);
    }
    public static void ReleaseTexture(string fileName, bool bForce, int iId)
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        if (string.IsNullOrEmpty(fileName))
        {
          return;
        }

        string cacheKey = fileName.ToLowerInvariant();

        //dont dispose radio/tv logo's since they are used by the overlay windows
        if (cacheKey.IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos")) >= 0)
        {
          return;
        }
        if (cacheKey.IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, "radio")) >= 0)
        {
          return;
        }

        if (_cacheTextures.TryGetValue(cacheKey, out CachedTexture oldImage))
        {
          try
          {
            if (iId >= 0 && iId != oldImage.ID)
              return; //attempt to release old image

            if (!bForce && System.Threading.Interlocked.Decrement(ref oldImage.InstanceCounter) != 0)
              return; // image is still in use

            Log.Debug("TextureManager: Dispose:{0} Frames:{1} Total:{2} Mem left:{3}, Counter:{4}, Force:{5}, ID:{6}, IDreq:{7}",
              oldImage.Name, oldImage.Frames, _cacheTextures.Count,
              GUIGraphicsContext.DX9Device?.IsDisposed ?? true ? -1 : (GUIGraphicsContext.DX9Device.AvailableTextureMemory / 1048576),
              oldImage.InstanceCounter, bForce, oldImage.ID, iId);

            _cacheTextures.TryRemove(cacheKey, out _);
            oldImage.SafeDispose();
          }
          catch (Exception ex)
          {
            Log.Error("TextureManager: Error in ReleaseTexture({0})", fileName);
            Log.Error(ex);
          }
        }
      }
    }

    public static void PreLoad(string fileName)
    {
      //TODO
    }

    public static void CleanupThumbs()
    {
      lock (GUIGraphicsContext.RenderLock)
      {
        Log.Debug("TextureManager: CleanupThumbs()");

        CachedTexture[] textures =
          _cacheTextures.Values.Where(t => t.Name != null && IsTemporary(t.Name)).ToArray();

        foreach (CachedTexture texture in textures)
        {
          string cacheKey = texture.Name.ToLowerInvariant();
          CachedTexture removedItem;
          _cacheTextures.TryRemove(cacheKey, out removedItem);
          texture.SafeDispose();
        }
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

      string fullFileName = fileName.ToLowerInvariant();
      if (fullFileName.IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos").ToLowerInvariant()) >= 0)
      {
        return false;
      }
      if (fullFileName.IndexOf(Config.GetSubFolder(Config.Dir.Thumbs, "radio").ToLowerInvariant()) >= 0)
      {
        return false;
      }

      // check if this texture was loaded to be persistent
      if (_persistentTextures.ContainsKey(fullFileName))
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
      if (!MediaPortal.Util.Utils.FileExistsInCache(fileName))
      {
        if (!Path.IsPathRooted(fileName))
        {
          fullFileName = GUIGraphicsContext.GetThemedSkinFile(@"\media\" + fileName);
        }
      }

      fullFileName = fullFileName.ToLowerInvariant();

      // Check if skin file
      if (fullFileName.IndexOf(@"skin\") >= 0)
      {
        if (fullFileName.IndexOf(@"media\animations\") >= 0)
        {
          return true;
        }
        if (fullFileName.IndexOf(@"media\tetris\") >= 0)
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
      _packer.SafeDispose();
      _packer = new TexturePacker();
      _packer.PackSkinGraphics(GUIGraphicsContext.Skin);

      ClearTextureCache();
      ClearDownloadCache();
    }

    private static void ClearTextureCache()
    {
      CachedTexture[] textures = _cacheTextures.Values.ToArray();

      _cacheTextures.Clear();

      foreach (CachedTexture texture in textures)
      {
        texture.SafeDispose();
      }
    }

    private static void ClearDownloadCache()
    {
      DownloadedImage[] images = _cacheDownload.Values.ToArray();

      _cacheDownload.Clear();

      foreach (DownloadedImage image in images)
      {
        image.SafeDispose();
      }
    }
  }
}