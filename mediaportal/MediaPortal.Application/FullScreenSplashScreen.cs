#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
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
      if (strVersion.Length > 1)
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
      bool needInvalidate = false;

      Log.Debug("FullScreenSplash: Splashscreen.xml found: {0}", skinFilePath);

      var doc = new XmlDocument();
      doc.Load(skinFilePath);
      if (doc.DocumentElement != null)
      {
        XmlNodeList controlsList = doc.DocumentElement.SelectNodes("/window/controls/control");
        if (controlsList != null)
          foreach (XmlNode control in controlsList)
          {
            XmlNode selectSingleNode = control.SelectSingleNode("type/text()");
            XmlNode singleNode = control.SelectSingleNode("id/text()");
            // if the background image control is found
            if (singleNode != null && (selectSingleNode != null && (selectSingleNode.Value.ToLowerInvariant() == "image" && singleNode.Value == "1"))) 
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
            XmlNode node = control.SelectSingleNode("type/text()");
            XmlNode selectSingleNode1 = control.SelectSingleNode("id/text()");
            // if the center label control is found
            if (selectSingleNode1 != null && (node != null && (node.Value.ToLowerInvariant() == "label" && selectSingleNode1.Value == "2")))
            {
              if (control.SelectSingleNode("textsize") != null)
              {
                XmlNode xmlNode = control.SelectSingleNode("textsize/text()");
                if (xmlNode != null)
                {
                  float textSize = float.Parse(xmlNode.Value);
                  Log.Debug("FullScreenSplash: Textsize value found: {0}", textSize);
                  lblMain.Font = new Font(lblMain.Font.FontFamily, textSize, lblMain.Font.Style);
                  Log.Debug("FullScreenSplash: Textsize successfully set: {0}", textSize);
                }
              }
              if (control.SelectSingleNode("textcolor") != null)
              {
                XmlNode xmlNode = control.SelectSingleNode("textcolor/text()");
                if (xmlNode != null)
                {
                  Color textColor = ColorTranslator.FromHtml(xmlNode.Value);
                  Log.Debug("FullScreenSplash: TextColor value found: {0}", textColor);
                  lblMain.ForeColor = textColor;
                  lblVersion.ForeColor = textColor;
                  lblCVS.ForeColor = textColor;
                  Log.Debug("FullScreenSplash: TextColor successfully set: {0}", textColor);
                }
              }
              if (control.SelectSingleNode("posX") != null)
              {
                XmlNode xmlNode = control.SelectSingleNode("posX/text()");
                if (xmlNode != null)
                {
                  var _value = xmlNode.Value;
                  Log.Debug("FullScreenSplash: Text PosX value found: {0}", _value);
                  int _number = 0;
                  if (Int32.TryParse(_value, out _number))
                  {
                    lblMain.Dock = System.Windows.Forms.DockStyle.None;
                    int newNumber = ScaleHorizontal(_number);
                    lblMain.Left = newNumber;
                    Log.Debug("FullScreenSplash: Main Label PosX successfully set: {0}/{1}", _number, newNumber);
                    needInvalidate = true;
                  }
                }
              }
              if (control.SelectSingleNode("posY") != null)
              {
                XmlNode xmlNode = control.SelectSingleNode("posY/text()");
                if (xmlNode != null)
                {
                  var _value = xmlNode.Value;
                  Log.Debug("FullScreenSplash: Text PosY value found: {0}", _value);
                  int _number = 0;
                  if (Int32.TryParse(_value, out _number))
                  {
                    lblMain.Dock = System.Windows.Forms.DockStyle.None;
                    int newNumber = ScaleVertical(_number);
                    lblMain.Top = newNumber;
                    Log.Debug("FullScreenSplash: Main Label PosY successfully set: {0}/{1}", _number, newNumber);
                    needInvalidate = true;
                  }
                }
              }
              if (control.SelectSingleNode("width") != null)
              {
                XmlNode xmlNode = control.SelectSingleNode("width/text()");
                if (xmlNode != null)
                {
                  var _value = xmlNode.Value;
                  Log.Debug("FullScreenSplash: Text Width value found: {0}", _value);
                  int _number = 0;
                  if (Int32.TryParse(_value, out _number))
                  {
                    lblMain.Dock = System.Windows.Forms.DockStyle.None;
                    int newNumber = ScaleHorizontal(_number);
                    lblMain.Width = newNumber;
                    Log.Debug("FullScreenSplash: Main Label Width successfully set: {0}/{1}", _number, newNumber);
                    needInvalidate = true;
                  }
                }
              }
              if (control.SelectSingleNode("height") != null)
              {
                XmlNode xmlNode = control.SelectSingleNode("height/text()");
                if (xmlNode != null)
                {
                  var _value = xmlNode.Value;
                  Log.Debug("FullScreenSplash: Text Height value found: {0}", _value);
                  int _number = 0;
                  if (Int32.TryParse(_value, out _number))
                  {
                    lblMain.Dock = System.Windows.Forms.DockStyle.None;
                    int newNumber = ScaleVertical(_number);
                    lblMain.Height = newNumber;
                    Log.Debug("FullScreenSplash: Main Label Height successfully set: {0}/{1}", _number, newNumber);
                    needInvalidate = true;
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
                    Log.Debug("FullScreenSplash: Text Align value found: {0}", _value);
                    _value = _value.Trim().ToLower();

                    if (_value == "bottomcenter")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
                    }
                    if (_value == "bottomleft")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
                    }
                    if (_value == "bottomright")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.BottomRight;
                    }
                    if (_value == "middlecenter")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    }
                    if (_value == "middleleft")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    }
                    if (_value == "middleright")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                    }
                    if (_value == "topcenter")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.TopCenter;
                    }
                    if (_value == "topleft")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.TopLeft;
                    }
                    if (_value == "topright")
                    {
                      lblMain.TextAlign = System.Drawing.ContentAlignment.TopRight;
                    }
                    Log.Debug("FullScreenSplash: Main Label Alignment successfully set: {0}", _value);
                  }
                }
              }
            }
          }
      }
      if (needInvalidate)
      {
        this.Invalidate(true);
      }
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
          catch (FormatException) // Size values were invalid.
          {  }
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

  }
}