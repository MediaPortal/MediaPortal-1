#region Copyright (C) 2017 Team MediaPortal

// Copyright (C) 2017 Team MediaPortal
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

using MediaPortal.GUI.Library;
using MediaPortal.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MediaPortal.GUI.Library
{
  public class GUIOverlayImage
  {
    private int _x = 0;
    private int _y = 0;
    private int _w = 0;
    private int _h = 0;
    private string _fileName = string.Empty;

    /// <summary>
    /// empty constructor
    /// </summary>
    public GUIOverlayImage()
    {
    }

    public GUIOverlayImage(int x, int y, int w, int h, string fileName)
    {
      _fileName = fileName;
      _x = x;
      _y = y;
      _w = w;
      _h = h;
    }

    public string FileName
    {
      get 
      { 
        string _resultFileName = _fileName; 
        if (!string.IsNullOrEmpty(_resultFileName) && _resultFileName.IndexOf("#", StringComparison.Ordinal) >= 0)
        {
          _resultFileName = GUIPropertyManager.Parse(_resultFileName);
        }
        return _resultFileName; 
      }
      set { _fileName = value; }
    }

    public string ThemedFileName
    {
      get 
      { 
        string _fileName = FileName;
        if (File.Exists(_fileName))
        {
          return _fileName;
        }
        _fileName = @"\media\" + _fileName;
        return GUIGraphicsContext.GetThemedSkinFile(_fileName);
      }
    }

    public int posX
    { 
      get { return _x; }
      set { _x = value; }
    }

    public int posY
    { 
      get { return _y; }
      set { _y = value; }
    }

    public int Width
    { 
      get { return _w; }
      set { _w = value; }
    }

    public int Height
    { 
      get { return _h; }
      set { _h = value; }
    }
  }

  public class GUIImageAllocator
  {
    private static List<String> _cachedAllocatorImages = new List<string>();
    private static bool _preserveAspectRatio = false;

    static GUIImageAllocator() { }

    public static bool PreserveAspectRatio
    {
      get { return _preserveAspectRatio; }
      set { _preserveAspectRatio = value; }
    }

    private static void Flush(string sTextureName)
    {
      Log.Debug("GUIImageAllocator: Flush {0}", sTextureName);
      GUITextureManager.ReleaseTexture(sTextureName);
    }

    public static void ClearCachedAllocatorImages()
    {
      if (_cachedAllocatorImages != null)
      {
        foreach (String sTextureName in _cachedAllocatorImages)
        {
          Flush(sTextureName);
        }
        _cachedAllocatorImages.Clear();
      }
      _cachedAllocatorImages = null;
      _cachedAllocatorImages = new List<string>();
    }

    public static string BuildConcatImage(string Prefix, string originalImage, int width, int height, List<GUIOverlayImage> listOverlayImages)
    {
      if (listOverlayImages == null || listOverlayImages.Count == 0)
      {
        return originalImage;
      }

      try
      {
        if (!string.IsNullOrEmpty(originalImage))
        {
          GUIOverlayImage _overlayImage = new GUIOverlayImage(0, 0, width, height, originalImage);
          listOverlayImages.Insert(0, _overlayImage);
        }

        string inMemoryFileID = string.Empty;
        foreach (GUIOverlayImage logo in listOverlayImages)
        {
          inMemoryFileID += Path.GetFileNameWithoutExtension(logo.FileName);
        }
        inMemoryFileID = inMemoryFileID.Replace(";", "-").Replace("{", "").Replace("}", "").Replace(" ", ""); //  + ".png"
        inMemoryFileID = "[" + Prefix + ":" + inMemoryFileID + "]";

        return BuildImages(width, height, listOverlayImages, inMemoryFileID);
      }
      catch (Exception ex)
      {
        Log.Error("GUIImageAllocator: BuildConcatImage: The Image Building Engine generated an error: " + ex.Message);
      }
      return string.Empty;
    }

    internal static string BuildImages(int width, int height, List<GUIOverlayImage> listOverlayImages, string inMemoryFileID)
    {
      if (GUITextureManager.GetCachedTexture(inMemoryFileID) != null)
      {
        return inMemoryFileID;
      }

      List<Image> imgs = new List<Image>();
      List<GUIOverlayImage> ovrls = new List<GUIOverlayImage>();

      // step one: Resize all Images to fit in Width, Height
      for (int i = 0; i < listOverlayImages.Count; i++)
      {
        if (listOverlayImages[i].Width == 0 || listOverlayImages[i].Height == 0)
        {
          continue;
        }

        Image single = null;
        try
        {
          single = LoadImageFastFromFile(listOverlayImages[i].ThemedFileName);
          if (single == null)
          {
            continue;
          }
          if (single.Width == 0 || single.Height == 0)
          {
            continue;
          }
        }
        catch (Exception)
        {
          Log.Debug("GUIImageAllocator: Skip. Could not load Image file... " + listOverlayImages[i].FileName);
          continue;
        }

        if (listOverlayImages[i].Width != single.Width || listOverlayImages[i].Height != single.Height)
        {
          single = MediaPortal.Util.Utils.ResizeImage(single, new Size(listOverlayImages[i].Width, listOverlayImages[i].Height), _preserveAspectRatio);
        }

        imgs.Add(single);
        ovrls.Add(listOverlayImages[i]);
      }
      if (imgs.Count == 0)
      {
        return string.Empty;
      }

      // step two: finally draw all images
      Bitmap bmp = new Bitmap(width, height);
      Image img = bmp;
      Graphics g = Graphics.FromImage(img);
      try
      {
        for (int i = 0; i < imgs.Count; i++)
        {
          g.DrawImage(imgs[i], ovrls[i].posX, ovrls[i].posY);
        }
      }
      finally
      {
        g.Dispose();
      }

      // step three: build final image in memory
      try
      {                
        // we don't have to try first, if name already exists mp will not do anything with the image
        GUITextureManager.LoadFromMemory(bmp, inMemoryFileID, 0, width, height);

        if (!string.IsNullOrEmpty(inMemoryFileID) && !_cachedAllocatorImages.Contains(inMemoryFileID))
        {
          _cachedAllocatorImages.Add(inMemoryFileID);
        }
      }
      catch (Exception)
      {
        Log.Error("GUIImageAllocator: BuildImages: Unable to add to MP's Graphics memory: " + inMemoryFileID);
        return string.Empty;
      }
      return inMemoryFileID;
    }

    /// <summary>
    /// Loads an Image from a File by invoking GDI Plus instead of using build-in .NET methods, or falls back to Image.FromFile
    /// Can perform up to 10x faster
    /// </summary>
    /// <param name="filename">The filename to load</param>
    /// <returns>A .NET Image object</returns>
    internal static Image LoadImageFastFromFile(string filename)
    {
      filename = Path.GetFullPath(filename);
      if (!File.Exists(filename))
      {
        return null;
      }

      Image imageFile = null;
      try
      {
        try
        {
          imageFile = ImageFast.FromFile(filename);
        }
        catch (Exception)
        {
          Log.Debug("GUIImageAllocator: Reverting to slow ImageLoading for: " + filename);
          imageFile = Image.FromFile(filename);
        }
      }
      catch (FileNotFoundException fe)
      {
        Log.Debug("GUIImageAllocator: Image does not exist: " + filename + " - " + fe.Message);
        return null;
      }
      catch (Exception e)
      {
        // this probably means the image is bad
        Log.Debug("GUIImageAllocator: Unable to load Imagefile (corrupt?): " + filename + " - " + e.Message);
        return null;
      }
      return imageFile;
    }

  }
}