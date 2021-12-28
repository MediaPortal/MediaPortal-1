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

using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Xml;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;
using MediaPortal.Util;

namespace MediaPortal
{
  public partial class FullScreenSplashScreen : MPForm
  {
    public FullScreenSplashScreen()
    {
      InitializeComponent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="version"></param>
    public void SetVersion(string version)
    {
      string[] strVersion = version.Split('-');
      lblVersion.Text = strVersion[0];
      Log.Info("Version: Application {0}", strVersion[0]);
      if (strVersion.Length == 2)
      {
        lblCVS.Text = strVersion[1];
        Log.Info("Edition/Codename: {0}", lblCVS.Text);
      }
      else if (strVersion.Length == 5)
      {
        string day   = strVersion[2].Substring(0, 2);
        string month = strVersion[2].Substring(3, 2);
        string year  = strVersion[2].Substring(6, 4);
        string time  = strVersion[3].Substring(0, 5);
        string build = strVersion[4].Substring(0, 13).Trim();
        lblCVS.Text  = string.Format("{0} {1} ({2}-{3}-{4} / {5} CET)", strVersion[1], build, year, month, day, time);
        Log.Info("Version: {0}", lblCVS.Text);
      }
      else
      {
        lblCVS.Text = string.Empty;
      }
      Update();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="information"></param>
    public void SetInformation(string information)
    {
      lblMain.Text = information;
      Update();
    }

    /// <summary>
    /// 
    /// </summary>
    public void FadeIn()
    {
      while (Opacity <= 0.9)
      {
        Opacity += 0.05;
        Thread.Sleep(15);
      }
      Opacity = 1;
    }

    /// <summary>
    /// Tries to load settings of the splash screen (color, background image...)
    /// </summary>
    public void RetrieveSplashScreenInfo()
    {
      InitSkinSizeFromReferenceXML();
      ReadSplashScreenXML();
      if (pbBackground.Image == null)
      {
        ReadReferenceXML();
      }
    }

    /// <summary>
    /// Tries to read the splashscreen.xml file of the current skin
    /// </summary>
    private void ReadSplashScreenXML()
    {
      string skinFilePath = GUIGraphicsContext.GetThemedSkinFile("\\splashscreen.xml");

      if (!File.Exists(skinFilePath))
      {
        Log.Debug("FullScreenSplash: Splashscreen.xml not found!: {0}", skinFilePath);
        return;
      }

      Log.Debug("FullScreenSplash: Splashscreen.xml found: {0}", skinFilePath);

      var doc = new XmlDocument();
      doc.Load(skinFilePath);
      if (doc.DocumentElement != null)
      {
        XmlNodeList controlsList = doc.DocumentElement.SelectNodes("/window/controls/control");
        if (controlsList != null)
        {
          foreach (XmlNode control in controlsList)
          {
            XmlNode nodeType = control.SelectSingleNode("type/text()");
            XmlNode nodeID = control.SelectSingleNode("id/text()");
            if (nodeType == null || nodeID == null)
            {
              continue;
            }

            // if the background image control is found
            if (nodeType.Value.ToLowerInvariant() == "image" && nodeID.Value == "1") 
            {
              XmlNode xmlNode = control.SelectSingleNode("texture/text()");
              if (xmlNode != null)
              {
                string backgoundImageName = xmlNode.Value;
                string backgroundImagePath = GUIGraphicsContext.GetThemedSkinFile("\\media\\" + backgoundImageName);
                if (File.Exists(backgroundImagePath))
                {
                  Log.Debug("FullScreenSplash: Try to load background image value found: {0}", backgroundImagePath);
                  pbBackground.Image = new Bitmap(backgroundImagePath); // load the image as background
                  Log.Debug("FullScreenSplash: background image successfully loaded: {0}", backgroundImagePath);
                }
              }
              continue;
            }

            #region Main label
            // if the Main label control is found
            if (nodeType.Value.ToLowerInvariant() == "label" && nodeID.Value == "2")
            {
              ReadLabelProperties(lblMain, control, "Main"); 
              continue;
            }
            #endregion

            #region Version label
            // if the Version label control is found
            if (nodeType.Value.ToLowerInvariant() == "label" && nodeID.Value == "3")
            {
              ReadLabelProperties(lblVersion, control, "Version"); 
              continue;
            }
            #endregion

            #region CVS label
            // if the edition/codename/cvs label control is found
            if (nodeType.Value.ToLowerInvariant() == "label" && nodeID.Value == "4")
            {
              ReadLabelProperties(lblCVS, control, "Edition/Codename/CVS"); 
              continue;
            }
            #endregion
          }
        }
      }

      #region Font scale

      lblMain.Font = ScaleFontSize(lblMain.Font);
      lblVersion.Font = ScaleFontSize(lblVersion.Font);
      lblCVS.Font = ScaleFontSize(lblCVS.Font);

      #endregion

      this.Invalidate(true);
    }

    private void InitSkinSizeFromReferenceXML()
    {
      string skinReferenceFilePath = GUIGraphicsContext.GetThemedSkinFile("\\references.xml");

      Log.Debug("FullScreenSplash: Try to init Skin size from reference.xml: {0}", skinReferenceFilePath);

      var doc = new XmlDocument();
      doc.Load(skinReferenceFilePath);
      if (doc.DocumentElement != null)
      {
        XmlNode nodeSkinWidth = doc.DocumentElement.SelectSingleNodeFast("/controls/skin/width/text()");
        XmlNode nodeSkinHeight = doc.DocumentElement.SelectSingleNodeFast("/controls/skin/height/text()");
        if (nodeSkinWidth != null && nodeSkinHeight != null)
        {
          try
          {
            int iWidth = Convert.ToInt16(nodeSkinWidth.Value);
            int iHeight = Convert.ToInt16(nodeSkinHeight.Value);
            Log.Debug("FullScreenSplash: Original skin size: {0}x{1}", iWidth, iHeight);
            GUIGraphicsContext.SkinSize = new Size(iWidth, iHeight);
            return;
          }
          catch (FormatException ex) // Size values were invalid.
          {
            Log.Debug("FullScreenSplash: InitSkinSizeFromReferenceXML: {0}", ex.Message);
          }
        }
      }
      Log.Debug("FullScreenSplash: Fallback to Default skin size: 1920x1080");
      GUIGraphicsContext.SkinSize = new Size(1920, 1080);
    }

    /// <summary>
    /// Tries to find the background of the current skin by analysing the reference.xml
    /// </summary>
    private void ReadReferenceXML()
    {
      string skinReferenceFilePath = GUIGraphicsContext.GetThemedSkinFile("\\references.xml");

      Log.Debug("FullScreenSplash: Try to use the reference.xml: {0}", skinReferenceFilePath);

      var doc = new XmlDocument();
      doc.Load(skinReferenceFilePath);
      if (doc.DocumentElement != null)
      {
        XmlNodeList controlsList = doc.DocumentElement.SelectNodes("/controls/control");

        if (controlsList != null)
          foreach (XmlNode control in controlsList)
          {
            XmlNode selectSingleNode = control.SelectSingleNode("type/text()");
            if (selectSingleNode != null && selectSingleNode.Value.ToLowerInvariant() == "image")
            {
              XmlNode singleNode = control.SelectSingleNode("texture/text()");
              if (singleNode != null)
              {
                string backgoundImageName = singleNode.Value;
                string backgroundImagePath = GUIGraphicsContext.GetThemedSkinFile("\\media\\" + backgoundImageName);
                if (File.Exists(backgroundImagePath))
                {
                  pbBackground.Image = new Bitmap(backgroundImagePath); // load the image as background
                  Log.Debug("FullScreenSplash: Background image value found: {0}", backgroundImagePath);
                }
              }
            }
          }
      }
    }

    /// <summary>
    /// Read Label properties from Skin splashscreen.xml file
    /// </summary>
    private void ReadLabelProperties(System.Windows.Forms.Label label, XmlNode control, string text)
    {
      if (label == null || control == null)
      {
        return;
      }

      if (control.SelectSingleNode("textsize") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("textsize/text()");
        if (xmlNode != null)
        {
          float textSize = float.Parse(xmlNode.Value);
          Log.Debug("FullScreenSplash: {0} label Textsize value found: {1}", text, textSize);
          label.Font = new Font(label.Font.FontFamily, textSize, label.Font.Style);
          Log.Debug("FullScreenSplash: {0} label Textsize successfully set: {1}", text, textSize);
        }
      }
      if (control.SelectSingleNode("fontname") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("fontname/text()");
        if (xmlNode != null)
        {
          string fontName = xmlNode.Value;
          Font testFont = new Font(fontName, label.Font.Size, label.Font.Style);
          Log.Debug("FullScreenSplash: {0} label Fontname value found: {1}", text, fontName);
          if (testFont.Name == fontName)
          {
            label.Font = testFont;
            Log.Debug("FullScreenSplash: {0} label Fontname successfully set: {1}", text, fontName);
          }
          else
          {
            Log.Debug("FullScreenSplash: {0} label Fontname {1} not found.", text, fontName);
          }
        }
      }
      if (control.SelectSingleNode("fontstyle") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("fontstyle/text()");
        if (xmlNode != null)
        {
          var _value = xmlNode.Value;
          if (!string.IsNullOrEmpty(_value))
          {
            Log.Debug("FullScreenSplash: {0} label Font style value found: {1}", text, _value);
            _value = _value.Trim().ToLower();

            if (_value == "regular")
            { 
              label.Font = new Font(label.Font.FontFamily, label.Font.Size, FontStyle.Regular);
            }
            if (_value == "bold")
            {
              label.Font = new Font(label.Font.FontFamily, label.Font.Size, FontStyle.Bold);
            }
            if (_value == "italic")
            {
              label.Font = new Font(label.Font.FontFamily, label.Font.Size, FontStyle.Italic);
            }
            if (_value == "underline")
            {
              label.Font = new Font(label.Font.FontFamily, label.Font.Size, FontStyle.Underline);
            }
            Log.Debug("FullScreenSplash: {0} label Font style successfully set: {1}", text, _value);
          }
        }
      }
      if (control.SelectSingleNode("textcolor") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("textcolor/text()");
        if (xmlNode != null)
        {
          Color textColor = ColorTranslator.FromHtml(xmlNode.Value);
          Log.Debug("FullScreenSplash: {0} label TextColor value found: {1}", text, textColor);
          label.ForeColor = textColor;
          Log.Debug("FullScreenSplash: {0} label TextColor successfully set: {1}", text, textColor);
        }
      }
      if (control.SelectSingleNode("posX") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("posX/text()");
        if (xmlNode != null)
        {
          var _value = xmlNode.Value;
          Log.Debug("FullScreenSplash: {0} label PosX value found: {1}", text, _value);
          int _number = 0;
          if (Int32.TryParse(_value, out _number))
          {
            label.Dock = System.Windows.Forms.DockStyle.None;
            int newNumber = ScaleHorizontal(_number);
            label.Left = newNumber;
            Log.Debug("FullScreenSplash: {0} label PosX successfully set: {1}/{2}", text, _number, newNumber);
          }
        }
      }
      if (control.SelectSingleNode("posY") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("posY/text()");
        if (xmlNode != null)
        {
          var _value = xmlNode.Value;
          Log.Debug("FullScreenSplash: {0} label PosY value found: {1}", text, _value);
          int _number = 0;
          if (Int32.TryParse(_value, out _number))
          {
            label.Dock = System.Windows.Forms.DockStyle.None;
            int newNumber = ScaleVertical(_number);
            label.Top = newNumber;
            Log.Debug("FullScreenSplash: {0} label PosY successfully set: {1}/{2}", text, _number, newNumber);
          }
        }
      }
      if (control.SelectSingleNode("width") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("width/text()");
        if (xmlNode != null)
        {
          var _value = xmlNode.Value;
          Log.Debug("FullScreenSplash: {0} label Width value found: {1}", text, _value);
          int _number = 0;
          if (Int32.TryParse(_value, out _number))
          {
            label.Dock = System.Windows.Forms.DockStyle.None;
            int newNumber = ScaleHorizontal(_number);
            label.Width = newNumber;
            Log.Debug("FullScreenSplash: {0} label Width successfully set: {1}/{2}", text, _number, newNumber);
          }
        }
      }
      if (control.SelectSingleNode("height") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("height/text()");
        if (xmlNode != null)
        {
          var _value = xmlNode.Value;
          Log.Debug("FullScreenSplash: {0} label Height value found: {1}", text, _value);
          int _number = 0;
          if (Int32.TryParse(_value, out _number))
          {
            label.Dock = System.Windows.Forms.DockStyle.None;
            int newNumber = ScaleVertical(_number);
            label.Height = newNumber;
            Log.Debug("FullScreenSplash: {0} label Height successfully set: {1}/{2}", text, _number, newNumber);
          }
        }
      }
      if (control.SelectSingleNode("align") != null)
      {
        XmlNode xmlNode = control.SelectSingleNode("align/text()");
        if (xmlNode != null)
        {
          var _value = xmlNode.Value;
          if (!string.IsNullOrEmpty(_value))
          {
            Log.Debug("FullScreenSplash: {0} label Align value found: {1}", text, _value);
            _value = _value.Trim().ToLower();

            if (_value == "bottomcenter")
            {
              label.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            }
            if (_value == "bottomleft")
            {
              label.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            }
            if (_value == "bottomright")
            {
              label.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            }
            if (_value == "middlecenter")
            {
              label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            }
            if (_value == "middleleft")
            {
              label.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            }
            if (_value == "middleright")
            {
              label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            }
            if (_value == "topcenter")
            {
              label.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            }
            if (_value == "topleft")
            {
              label.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            }
            if (_value == "topright")
            {
              label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            }
            Log.Debug("FullScreenSplash: {0} label Alignment successfully set: {1}", text, _value);
          }
        }
      }
    }

    /// <summary>
    /// Scale x position for current resolution
    /// </summary>
    /// <param name="x">X coordinate to scale.</param>
    private int ScaleHorizontal(int x)
    {
      if (GUIGraphicsContext.SkinSize.Width != 0)
      {
        float percentX = (float)GUIGraphicsContext.currentScreen.Bounds.Width / (float)GUIGraphicsContext.SkinSize.Width;
        x = (int)Math.Round((float)x * percentX);
      }
      return x;
    }

    /// <summary>
    /// Scale y position for current resolution
    /// </summary>
    /// <param name="y">Y coordinate to scale.</param>
    private int ScaleVertical(int y)
    {
      if (GUIGraphicsContext.SkinSize.Height != 0)
      {
        float percentY = (float)GUIGraphicsContext.currentScreen.Bounds.Height / (float)GUIGraphicsContext.SkinSize.Height;
        y = (int)Math.Round((float)y * percentY);
      }
      return y;
    }

    /// <summary>
    /// Scale Font size for current resolution
    /// </summary>
    /// <param name="font">Font to scale.</param>
    private Font ScaleFontSize(Font font)
    {
      int ascent = font.FontFamily.GetCellAscent(font.Style);
      int descent = font.FontFamily.GetCellDescent(font.Style);
      int emHeight = font.FontFamily.GetEmHeight(font.Style);
      int sizeInPixel = (int)Math.Ceiling(font.Size * (float)(descent + ascent) / (float)emHeight);

      return new Font(font.FontFamily, ScaleVertical(sizeInPixel), font.Style, GraphicsUnit.Pixel);
    }
  }
}