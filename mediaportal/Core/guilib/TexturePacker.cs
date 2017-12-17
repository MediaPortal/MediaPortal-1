#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DShowNET.Helper;
using MediaPortal.Configuration;
using MediaPortal.ExtensionMethods;
using MediaPortal.Profile;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;
using Filter = Microsoft.DirectX.Direct3D.Filter;

// ReSharper disable CheckNamespace
namespace MediaPortal.GUI.Library
// ReSharper restore CheckNamespace
{
  /// <summary>
  /// Summary description for TexturePacker.
  /// </summary>
  public class TexturePacker : IDisposable
  {
    public delegate void DisposeEventHandler(object sender, int texutureNumber);

    public event DisposeEventHandler Disposing;

    #region PackedTexture Class

    [Serializable]
    // ReSharper disable InconsistentNaming
    public class PackedTexture
    {

      public PackedTextureNode root;
      public int textureNo;
      [NonSerialized] public Texture texture;
    };
    // ReSharper restore InconsistentNaming

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
        return string.IsNullOrEmpty(fileName) ? null : GetInternal(fileName);
      }

      private PackedTextureNode GetInternal(string fileName)
      {
        if (FileName == fileName)
        {
          return this;
        }
        if (ChildLeft != null)
        {
          PackedTextureNode node = ChildLeft.GetInternal(fileName);
          if (node != null)
          {
            return node;
          }
        }
        return ChildRight != null ? ChildRight.GetInternal(fileName) : null;
      }

      public PackedTextureNode Insert(string fileName, Image img, Image rootImage)
      {
        if (ChildLeft != null && ChildRight != null)
        {
          PackedTextureNode node = ChildLeft.Insert(fileName, img, rootImage);
          return node ?? ChildRight.Insert(fileName, img, rootImage);
        }
        //(if there's already a lightmap here, return)
        if (!string.IsNullOrEmpty(FileName))
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
        return newNode ?? ChildRight.Insert(fileName, img, rootImage);
      }
    }

    #endregion

    #region variables

    private List<PackedTexture> _packedTextures;   // A list of packed textures
    private List<string> _texturesNotPacked;       // A list of textures that could not be packed
    // ReSharper disable InconsistentNaming
    private const int MAXTEXTUREDIMENSION = 2048;
    // ReSharper restore InconsistentNaming
    private int _maxTextureWidth;
    private int _maxTextureHeight;

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
        Log.Info(
          "TexturePacker: Cache from {0} is outdated and will be recreated including current tv ({1}) and radio ({2}) thumbs",
          cacheCreationTime.ToShortDateString(), tvLogoChangeTime.ToShortDateString(),
          radioLogoChangeTime.ToShortDateString());
      }

      using (Settings xmlreader = new MPSettings())
      {
        // Intended for developers that rebuild mp often.
        if (xmlreader.GetValueAsBool("debug", "should_never_remove_packed_skin", false))
        {
          return;
        }
        Log.Info("TexturePacker: Removing packed skin");

        try
        {
          string[] cacheFiles = Directory.GetFiles(GUIGraphicsContext.SkinCacheFolder, "packedgfx*",
                                                   SearchOption.TopDirectoryOnly);
          foreach (string file in cacheFiles)
          {
            File.Delete(file);
          }
        }
        catch (Exception) {}
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

    private void SavePackedSkin()
    {
      // Save the packed texture files.
      string packedXml = string.Format(@"{0}\packedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);

      using (var fileStream = new FileStream(packedXml, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        var packedTextures = new ArrayList();
        foreach (PackedTexture packed in _packedTextures)
        {
          packedTextures.Add(packed);
        }
        var formatter = new BinaryFormatter();
        formatter.Serialize(fileStream, packedTextures);
        fileStream.Close();
      }

      // Save a list of the images that were not able to be packed.
      string notPackedXml = string.Format(@"{0}\notpackedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);

      using (var fileStream = new FileStream(notPackedXml, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        var notPackedTextures = new ArrayList();
        foreach (string notPacked in _texturesNotPacked)
        {
          notPackedTextures.Add(notPacked);
        }
        var formatter = new BinaryFormatter();
        formatter.Serialize(fileStream, notPackedTextures);
        fileStream.Close();
      }
    }

    private bool LoadPackedSkin()
    {
      bool loadedPacked = false;
      Log.Debug("Skin Folder : {0}", GUIGraphicsContext.Skin);
      Log.Debug("Cache Folder: {0}", GUIGraphicsContext.SkinCacheFolder);
      string notPackedXml = string.Format(@"{0}\notpackedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);
      string packedXml = string.Format(@"{0}\packedgfx2.bxml", GUIGraphicsContext.SkinCacheFolder);

      using (Settings xmlreader = new MPSettings())
      {
        if (xmlreader.GetValueAsBool("debug", "skincaching", true) &&
            MediaPortal.Util.Utils.FileExistsInCache(packedXml))
        {
          // Load textures that were packed.
          using (var fileStream = new FileStream(packedXml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            try
            {
              _packedTextures = new List<PackedTexture>();
              var formatter = new BinaryFormatter();
              var packedTextures = (ArrayList)formatter.Deserialize(fileStream);
              foreach (PackedTexture packed in packedTextures)
              {
                _packedTextures.Add(packed);
              }
              fileStream.Close();
            }
            catch {}
          }

          // Load the list of textures that were not able to be packed.
          using (var fileStream = new FileStream(notPackedXml, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            try
            {
              _texturesNotPacked = new List<string>();
              var formatter = new BinaryFormatter();
              var notPackedTextures = (ArrayList)formatter.Deserialize(fileStream);
              foreach (string notPacked in notPackedTextures)
              {
                _texturesNotPacked.Add(notPacked);
              }
              fileStream.Close();
            }
            catch {}
          }
          loadedPacked = true;
        }
      }
      return loadedPacked;
    }

    public void PackSkinGraphics(string skinName)
    {
      // for debugging we can comment out this line to avoid rebuild of skin cache on MP start
      Cleanup();
      if (LoadPackedSkin())
      {
        return;
      }

      // Create the list of textures to pack.
      _packedTextures = new List<PackedTexture>();
      _texturesNotPacked = new List<string>();
      var files = new List<string>();

      string[] skinFiles = Directory.GetFiles(String.Format(@"{0}\media", skinName), "*.png");
      files.AddRange(skinFiles);

      try
      {
        string[] themeFiles = Directory.GetFiles(String.Format(@"{0}\themes", skinName), "*.png", SearchOption.AllDirectories);
        files.AddRange(themeFiles);
      }
      catch (DirectoryNotFoundException)
      {
        // The themes directory is not required to exist.
      }

      string[] tvLogos = Directory.GetFiles(Config.GetSubFolder(Config.Dir.Thumbs, @"tv\logos"), "*.png", SearchOption.AllDirectories);
      files.AddRange(tvLogos);

      string[] radioLogos = Directory.GetFiles(Config.GetSubFolder(Config.Dir.Thumbs, "Radio"), "*.png", SearchOption.AllDirectories);
      files.AddRange(radioLogos);

      string[] weatherFiles = Directory.GetFiles(String.Format(@"{0}\media\weather", skinName), "*.png");
      files.AddRange(weatherFiles);

      string[] tetrisFiles = Directory.GetFiles(String.Format(@"{0}\media\tetris", skinName), "*.png");
      files.AddRange(tetrisFiles);

      // Determine maximum texture dimensions
      try
      {
        Caps capabilities = GUIGraphicsContext.DX9Device.DeviceCaps;
        _maxTextureWidth = capabilities.MaxTextureWidth;
        _maxTextureHeight = capabilities.MaxTextureHeight;
        Log.Info("TexturePacker: D3D device does support {0}x{1} textures", _maxTextureWidth, _maxTextureHeight);
      }
      catch (Exception)
      {
        _maxTextureWidth = MAXTEXTUREDIMENSION;
        _maxTextureHeight = MAXTEXTUREDIMENSION;
      }

      if (_maxTextureWidth > MAXTEXTUREDIMENSION)
      {
        _maxTextureWidth = MAXTEXTUREDIMENSION;
      }
      if (_maxTextureHeight > MAXTEXTUREDIMENSION)
      {
        _maxTextureHeight = MAXTEXTUREDIMENSION;
      }

      Log.Info("TexturePacker: using {0}x{1} as packed textures limit", _maxTextureWidth, _maxTextureHeight);
      Log.Info("TexturePacker: using {0}x{1} as single texture limit", _maxTextureWidth / 2, _maxTextureHeight / 2);

      while (true)
      {
        bool imagesLeft = false;

        var bigOne = new PackedTexture {root = new PackedTextureNode(), texture = null, textureNo = -1};
        using (var rootImage = new Bitmap(_maxTextureWidth, _maxTextureHeight))
        {
          bigOne.root.Rect = new Rectangle(0, 0, _maxTextureWidth, _maxTextureHeight);
          for (int i = 0; i < files.Count; ++i)
          {
            if (files[i] == null)
            {
              continue;
            }
            files[i] = files[i].ToLowerInvariant();
            if (files[i] != string.Empty)
            {
              // Ignore files not needed for MP
              if (files[i].IndexOf("preview.", StringComparison.Ordinal) >= 0)
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
                  // Keep track of textures that cannot be packed.
                  _texturesNotPacked.Add(files[i]);
                  files[i] = string.Empty;
                }
                else
                {
                  imagesLeft = true;
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
        if (!imagesLeft)
        {
          break;
        }
      }
      SavePackedSkin();
    }

    private void LoadPackedGraphics(int index)
    {
      //	return ;
      PackedTexture bigOne = _packedTextures[index];
      const Format useFormat = Format.A8R8G8B8;
      //if (IsCompressedTextureFormatOk(Format.Dxt5))
      //{
      //    Log.Debug("TexturePacker: Using DXT5 texture format");
      //    useFormat = Format.Dxt5;
      //}
      if (bigOne.texture == null)
      {
        bigOne.textureNo = -1;

        string fileName = String.Format(@"{0}\packedgfx2{1}.png", GUIGraphicsContext.SkinCacheFolder, index);

        var info2 = new ImageInformation();
        Texture tex = TextureLoader.FromFile(GUIGraphicsContext.DX9Device,
                                             fileName,
                                             0, 0, //width/height
                                             1, //mipslevels
                                             0, //Usage.Dynamic,
                                             useFormat,
                                             GUIGraphicsContext.GetTexturePoolType(),
                                             Filter.None,
                                             Filter.None,
                                             0,
                                             ref info2);
        bigOne.texture = tex;
        bigOne.texture.Disposing -= TextureDisposing;
        bigOne.texture.Disposing += TextureDisposing;

        Log.Info("TexturePacker: Loaded {0} texture:{1}x{2} miplevels:{3}", fileName, info2.Width, info2.Height,
                 tex.LevelCount);
      }
    }

    private void TextureDisposing(object sender, EventArgs e)
    {
      if ((sender as Texture) == null)
      {
        return;
      }
      foreach (PackedTexture bigOne in _packedTextures)
      {
        if (bigOne.texture == (Texture)sender)
        {
          if (bigOne.textureNo >= 0)
          {
            Log.Info("TexturePacker: disposing texture:{0}", bigOne.textureNo);
            DXNative.FontEngineRemoveTextureSync(bigOne.textureNo);
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
      dontAdd = false;
      Image bmp;

      try
      {
        bmp = ImageFast.FromFile(file);
      }
      catch (Exception)
      {
        Log.Warn("TexturePacker: Fast loading of texture {0} failed - trying safe fallback now", file);
        bmp = Image.FromFile(file);
      }

      if (bmp.Width > (_maxTextureHeight / 2) || bmp.Height > (_maxTextureWidth / 2))
      {
        Log.Warn("TexturePacker: Texture {0} is too large to be cached. Texture {1}x{2} - limit {3}x{4}",
                 file, bmp.Width, bmp.Height, _maxTextureHeight / 2, _maxTextureWidth / 2);
        dontAdd = true;
        return false;
      }

      string skinName = String.Format(@"{0}\media", GUIGraphicsContext.Skin).ToLowerInvariant();
      int pos = file.IndexOf(skinName, StringComparison.Ordinal);
      if (pos >= 0)
      {
        file = file.Remove(pos, skinName.Length);
      }

      string themeName = String.Format(@"{0}\themes", GUIGraphicsContext.Skin).ToLowerInvariant();
      pos = file.IndexOf(themeName, StringComparison.Ordinal);
      if (pos >= 0)
      {
        file = file.Remove(pos, themeName.Length);
      }

      if (file.StartsWith(@"\"))
      {
        file = file.Remove(0, 1);
      }
      bool result = Add(root, bmp, rootImage, file);
      bmp.SafeDispose();
      bmp = null;

      return result;
    }

    public bool Get(string fileName, out float uoffs, out float voffs, out float umax, out float vmax, out int iWidth,
                    out int iHeight, out Texture tex, out int textureNo)
    {
      uoffs = voffs = umax = vmax = 0.0f;
      iWidth = iHeight = 0;
      textureNo = -1;
      tex = null;
      if (_packedTextures == null)
      {
        return false;
      }

      if (fileName.StartsWith(@"\"))
      {
        fileName = fileName.Remove(0, 1);
      }
      fileName = fileName.ToLowerInvariant();
      if (fileName == string.Empty)
      {
        return false;
      }

      int index = 0;
      PackedTexture bigOne = null;
      PackedTextureNode foundNode = null;

      // Look for textures first in the current theme location.  Theme textures override default textures.
      // If the default theme is selected then avoid looking through the theme.
      if (!GUIThemeManager.CurrentThemeIsDefault)
      {
        string skinThemeTexturePath = GUIThemeManager.CurrentTheme + @"\media\";
        skinThemeTexturePath = skinThemeTexturePath.ToLowerInvariant();

        // If a theme texture exists but was not able to be packed then avoid trying to unpack the texture all together.  This prevents
        // a base skin texture from being unpacked and returned when the base texture could be packed but the overriding theme texture
        // could not be packed.
        if (!IsTexturePacked(skinThemeTexturePath + fileName))
        {
          return false;
        }

        foreach (PackedTexture texture in _packedTextures)
        {
          if ((foundNode = texture.root.Get(skinThemeTexturePath + fileName)) != null)
          {
            bigOne = texture;
            break;
          }
          index++;
        }
      }

      // No theme texture was found.  Check the default skin location.
      if (foundNode == null)
      {
        index = 0;
        foreach (PackedTexture texture in _packedTextures)
        {
          if ((foundNode = texture.root.Get(fileName)) != null)
          {
            bigOne = texture;
            break;
          }
          index++;
        }
      }

      if (foundNode != null)
      {
        uoffs = (float)(foundNode.Rect.Left + 1) / bigOne.root.Rect.Width;
        voffs = (float)(foundNode.Rect.Top + 1) / bigOne.root.Rect.Height;
        umax = (float)(foundNode.Rect.Width - 2) / bigOne.root.Rect.Width;
        vmax = (float)(foundNode.Rect.Height - 2) / bigOne.root.Rect.Height;
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
            bigOne.textureNo = DXNative.FontEngineAddTextureSync(ptr.ToInt32(), true, ptr.ToPointer());
            Log.Info("TexturePacker: fontengine add texure:{0}", bigOne.textureNo);
          }
        }
        textureNo = bigOne.textureNo;
        return true;
      }
      return false;
    }

    private bool IsTexturePacked(string textureFile)
    {
      // Look through the textures that wee not packed (likely a shorter list).
      return _texturesNotPacked.All(texture => texture.IndexOf(textureFile, StringComparison.Ordinal) <= 0);
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
            DXNative.FontEngineRemoveTextureSync(bigOne.textureNo);
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
                bigOne.texture.Disposing -= TextureDisposing;
                bigOne.texture.SafeDispose();
              }
              catch (Exception) {}
            }
            bigOne.texture = null;
          }
        }
      }
    }
  }
}