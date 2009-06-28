using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using MediaPortal.Profile;
using MediaPortal.Configuration;

namespace MediaPortal.Player.Subtitles
{
  
  [StructLayout(LayoutKind.Sequential)]
  public struct SubtitleStyle
  {
    [MarshalAs(UnmanagedType.LPWStr)]
    public string fontName;
    public int fontColor;
    [MarshalAs(UnmanagedType.Bool)]
    public bool fontIsBold;
    public int fontSize;
    public int fontCharset;
    public int shadow;
    public int borderWidth;
    [MarshalAs(UnmanagedType.Bool)]
    public bool isBorderOutline;

    public void Load(Settings xmlreader)
    {
      fontName = xmlreader.GetValueAsString("subtitles", "fontface", "Arial");
      fontSize = xmlreader.GetValueAsInt("subtitles", "fontsize", 18);
      fontIsBold = xmlreader.GetValueAsBool("subtitles", "bold", true);
      fontCharset = xmlreader.GetValueAsInt("subtitles", "charset", 1); //default charset

      string strColor = xmlreader.GetValueAsString("subtitles", "color", "ffffff");
      int argb = Int32.Parse(strColor, System.Globalization.NumberStyles.HexNumber);
      //convert ARGB to BGR (COLORREF)
      fontColor = (int)((argb & 0x000000FF) << 16) |
                    (int)(argb & 0x0000FF00) |
                    (int)((argb & 0x00FF0000) >> 16);
      shadow = xmlreader.GetValueAsInt("subtitles", "shadow", 3);
      borderWidth = xmlreader.GetValueAsInt("subtitles", "borderWidth", 2);
      isBorderOutline = xmlreader.GetValueAsBool("subtitles", "borderOutline", true);
    }
  }

  public enum AutoSaveTypeEnum
  {
    NEVER,
    ASK,
    ALWAYS
  };
  
  public class SubSettings
  {
    protected SubtitleStyle defStyle;
    protected int delayInterval;
    protected AutoSaveTypeEnum autoSaveType = AutoSaveTypeEnum.NEVER;
    protected bool posRelativeToFrame = false;
    protected bool overrideASSStyle;

    public void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        defStyle = new SubtitleStyle();
        defStyle.Load(xmlreader);
        delayInterval = xmlreader.GetValueAsInt("subtitles", "delayInterval", 250);

        bool save = xmlreader.GetValueAsBool("subtitles", "saveNever", true);
        if (save)
        {
          autoSaveType = AutoSaveTypeEnum.NEVER;
        }
        else
        {
          save = xmlreader.GetValueAsBool("subtitles", "saveAsk", false);
          autoSaveType = (save ? AutoSaveTypeEnum.ASK : AutoSaveTypeEnum.ALWAYS);
        }

        posRelativeToFrame = xmlreader.GetValueAsBool("subtitles", "subPosRelative", false);
        overrideASSStyle = xmlreader.GetValueAsBool("subtitles", "subStyleOverride", false);
        LoadAdvancedSettings(xmlreader);
      }
    }

    protected virtual void LoadAdvancedSettings(Settings xmlreader)
    {
    }
  }
}
