#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Xml;
using System.Runtime.InteropServices;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// The class responsible for keeping track of the used fonts.
  /// </summary>
  public class GUIFontManager
  {
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEnginePresentTextures();


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineSetDevice(void* device);

    static protected List<GUIFont> _listFonts = new List<GUIFont>();
    static private Microsoft.DirectX.Direct3D.Sprite _d3dxSprite;
    static private bool _d3dxSpriteUsed;
    
    private struct FontManagerDrawText
    {
      public Microsoft.DirectX.Direct3D.Font fnt;
      public float xpos;
      public float ypos;
      public int color;
      public string text;
      public float[,] matrix;
      public Microsoft.DirectX.Direct3D.Viewport viewport;
    };
    static private List<FontManagerDrawText> _listDrawText = new List<FontManagerDrawText>();    

    // singleton. Dont allow any instance of this class
    private GUIFontManager()
    {
    }

    static GUIFontManager()
    {
    }

    public static int Count
    {
      get { return _listFonts.Count; }
    }
    /// <summary>
    /// Loads the fonts from a file.
    /// </summary>
    /// <param name="strFilename">The filename from where the fonts are loaded.</param>
    /// <returns>true if loaded else false</returns>
    public static bool LoadFonts(string strFilename)
    {
      // Clear current set of fonts
      Dispose();
      int counter = 0;
      Log.Info("  Load fonts from {0}", strFilename);
      _listFonts.Clear();

      // Load the debug font
      GUIFont fontDebug = new GUIFont("debug", "Arial", 12);
      fontDebug.ID = counter++;
      fontDebug.Load();
      _listFonts.Add(fontDebug);

      try
      {
        // Load the XML document
        XmlDocument doc = new XmlDocument();
        doc.Load(strFilename);
        // Check the root element
        if (doc.DocumentElement == null)
          return false;
        string strRoot = doc.DocumentElement.Name;
        if (strRoot != "fonts")
          return false;
        // Select the list of fonts
        XmlNodeList list = doc.DocumentElement.SelectNodes("/fonts/font");
        foreach (XmlNode node in list)
        {
          XmlNode nodeStart = node.SelectSingleNode("start");
          XmlNode nodeEnd = node.SelectSingleNode("end");
          XmlNode nodeName = node.SelectSingleNode("name");
          XmlNode nodeFileName = node.SelectSingleNode("filename");
          XmlNode nodeHeight = node.SelectSingleNode("height");
          XmlNode nodeBold = node.SelectSingleNode("bold");
          XmlNode nodeItalics = node.SelectSingleNode("italic");
          if (nodeHeight != null && nodeName != null && nodeFileName != null)
          {
            bool bold = false;
            bool italic = false;
            if (nodeBold != null && nodeBold.InnerText != null && nodeBold.InnerText.Equals("yes"))
              bold = true;
            if (nodeItalics != null && nodeItalics.InnerText != null && nodeItalics.InnerText.Equals("yes"))
              italic = true;
            string strName = nodeName.InnerText;
            string strFileName = nodeFileName.InnerText;
            int iHeight = Int32.Parse(nodeHeight.InnerText);

            // height is based on 720x576
            float fPercent = ((float)GUIGraphicsContext.Height) / 576.0f;
            fPercent *= iHeight;
            iHeight = (int)fPercent;
            System.Drawing.FontStyle style = new System.Drawing.FontStyle();
            style = System.Drawing.FontStyle.Regular;
            if (bold)
              style |= System.Drawing.FontStyle.Bold;
            if (italic)
              style |= System.Drawing.FontStyle.Italic;
            GUIFont font = new GUIFont(strName, strFileName, iHeight, style);
            font.ID = counter++;
            if (nodeStart != null && nodeStart.InnerText != "" && nodeEnd != null && nodeEnd.InnerText != "")
            {
              int start = Int32.Parse(nodeStart.InnerText);
              int end = Int32.Parse(nodeEnd.InnerText);
              font.SetRange(start, end);
            }
            else
            {
              font.SetRange(0, GUIGraphicsContext.CharsInCharacterSet);
            }

            font.Load();
            _listFonts.Add(font);
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Log.Info("exception loading fonts {0} err:{1} stack:{2}", strFilename, ex.Message, ex.StackTrace);
      }

      return false;
    }

    /// <summary>
    /// Gets a GUIFont.
    /// </summary>
    /// <param name="iFont">The font number</param>
    /// <returns>A GUIFont instance representing the fontnumber or a default GUIFont if the number does not exists.</returns>
    public static GUIFont GetFont(int iFont)
    {
      if (iFont >= 0 && iFont < _listFonts.Count)
        return _listFonts[iFont];
      return GetFont("debug");
    }

    /// <summary>
    /// Gets a GUIFont.
    /// </summary>
    /// <param name="strFontName">The name of the font</param>
    /// <returns>A GUIFont instance representing the strFontName or a default GUIFont if the strFontName does not exists.</returns>
    public static GUIFont GetFont(string strFontName)
    {
      for (int i = 0; i < _listFonts.Count; ++i)
      {
        GUIFont font = _listFonts[i];
        if (font.FontName == strFontName)
          return font;
      }

      // just return a font
      return GetFont("debug");
    }

    static public void MeasureText(Microsoft.DirectX.Direct3D.Font fnt, string text, ref float textwidth, ref float textheight)
    {
      if (text[0] == ' ') // anti-trim
        text = "_" + text.Substring(1);
      if (text[text.Length - 1] == ' ')
        text = text.Substring(0, text.Length - 1) + '_';
      if (_d3dxSprite == null)
        _d3dxSprite = new Microsoft.DirectX.Direct3D.Sprite(GUIGraphicsContext.DX9Device);
      System.Drawing.Rectangle rect = fnt.MeasureString(_d3dxSprite, text, Microsoft.DirectX.Direct3D.DrawTextFormat.NoClip, System.Drawing.Color.Black);
      textwidth = rect.Width;
      textheight = rect.Height;
      return;
    }

    static public void DrawText(Microsoft.DirectX.Direct3D.Font fnt, float xpos, float ypos, System.Drawing.Color color, string text, int maxWidth)
    {
      FontManagerDrawText draw;
      draw.fnt = fnt;
      draw.xpos = xpos;
      draw.ypos = ypos;
      draw.color = color.ToArgb();
      draw.text = text;
      draw.matrix = (float[,])GUIGraphicsContext.GetFinalMatrix().Clone();
      draw.viewport = GUIGraphicsContext.DX9Device.Viewport;
      if (maxWidth >= 0)
        draw.viewport.Width = ((int)xpos) + maxWidth - draw.viewport.X;
      _listDrawText.Add(draw);
      _d3dxSpriteUsed = true;
    }

    public static void Present()
    {

      FontEnginePresentTextures();
      for (int i = 0; i < _listFonts.Count; ++i)
      {
        GUIFont font = _listFonts[i];
        font.Present();
      }

      if (_d3dxSpriteUsed)
      {
        if (_d3dxSprite == null)
          _d3dxSprite = new Microsoft.DirectX.Direct3D.Sprite(GUIGraphicsContext.DX9Device);
        _d3dxSprite.Begin(Microsoft.DirectX.Direct3D.SpriteFlags.AlphaBlend | Microsoft.DirectX.Direct3D.SpriteFlags.SortTexture);
        Microsoft.DirectX.Direct3D.Viewport orgView = GUIGraphicsContext.DX9Device.Viewport;
        Microsoft.DirectX.Matrix orgProj = GUIGraphicsContext.DX9Device.Transform.View;
        Microsoft.DirectX.Matrix projm = orgProj;
        Microsoft.DirectX.Matrix finalm;
        foreach (FontManagerDrawText draw in _listDrawText)
        {
          finalm.M11 = draw.matrix[0, 0]; finalm.M12 = draw.matrix[0, 1]; finalm.M13 = draw.matrix[0, 2]; finalm.M14 = draw.matrix[0, 3];
          finalm.M21 = draw.matrix[1, 0]; finalm.M22 = draw.matrix[1, 1]; finalm.M23 = draw.matrix[1, 2]; finalm.M24 = draw.matrix[1, 3];
          finalm.M31 = draw.matrix[2, 0]; finalm.M32 = draw.matrix[2, 1]; finalm.M33 = draw.matrix[2, 2]; finalm.M34 = draw.matrix[2, 3];
          finalm.M41 = 0; finalm.M42 = 0; finalm.M43 = 0; finalm.M44 = 1.0f;
          _d3dxSprite.Transform = finalm;
          GUIGraphicsContext.DX9Device.Viewport = draw.viewport;
          float wfactor = ((float)orgView.Width) / (float)draw.viewport.Width;
          float hfactor = ((float)orgView.Height) / (float)draw.viewport.Height;
          float xoffset = (float)(orgView.X - draw.viewport.X);
          float yoffset = (float)(orgView.Y - draw.viewport.Y);
          projm.M11 = (orgProj.M11 + orgProj.M14 * xoffset) * wfactor;
          projm.M21 = (orgProj.M21 + orgProj.M24 * xoffset) * wfactor;
          projm.M31 = (orgProj.M31 + orgProj.M34 * xoffset) * wfactor;
          projm.M41 = (orgProj.M41 + orgProj.M44 * xoffset) * wfactor;
          projm.M12 = (orgProj.M12 + orgProj.M14 * yoffset) * hfactor;
          projm.M22 = (orgProj.M22 + orgProj.M24 * yoffset) * hfactor;
          projm.M32 = (orgProj.M32 + orgProj.M34 * yoffset) * hfactor;
          projm.M42 = (orgProj.M42 + orgProj.M44 * yoffset) * hfactor;
          GUIGraphicsContext.DX9Device.Transform.View = projm;
          draw.fnt.DrawText(_d3dxSprite, draw.text, new System.Drawing.Rectangle((int)draw.xpos, (int)draw.ypos, 0, 0), Microsoft.DirectX.Direct3D.DrawTextFormat.NoClip, draw.color);
          _d3dxSprite.Flush();
        }
        GUIGraphicsContext.DX9Device.Viewport = orgView;
        GUIGraphicsContext.DX9Device.Transform.View = orgProj;
        _d3dxSprite.End();
        _listDrawText = new List<FontManagerDrawText>();
        _d3dxSpriteUsed = false;
      }
    }
    
    /// <summary>
    /// Disposes all GUIFonts.
    /// </summary>
    public static void Dispose()
    {
      Log.Info("  fonts.Dispose()");
      foreach (GUIFont font in _listFonts)
      {
        font.Dispose(null, null);
      }

      if (_d3dxSprite != null)
      {
        _d3dxSprite.Dispose();
        _d3dxSprite = null;
        _d3dxSpriteUsed = false;
      }

    }

    /// <summary>
    /// Sets the device and the FVF.
    /// </summary>
    public static void SetDevice()
    {
      Log.Info("  fonts.SetDevice()");
      IntPtr upDevice = DShowNET.Helper.DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      unsafe
      {
        FontEngineSetDevice(upDevice.ToPointer());
      }
    }

    /// <summary>
    /// Initializes the device objects of the GUIFonts.
    /// </summary>
    public static void InitializeDeviceObjects()
    {
      Log.Info("  fonts.InitializeDeviceObjects()");
      IntPtr upDevice = DShowNET.Helper.DirectShowUtil.GetUnmanagedDevice(GUIGraphicsContext.DX9Device);

      unsafe
      {
        FontEngineSetDevice(upDevice.ToPointer());
      }
      foreach (GUIFont font in _listFonts)
      {
        font.InitializeDeviceObjects();
      }
    }
  }
}