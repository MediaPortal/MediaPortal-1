/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using MediaPortal.Util;
//using MediaPortal.Dialogs;
using System.ComponentModel;
using MediaPortal.Configuration;
using System.Threading;



namespace MediaPortal.GUI.WeatherOverlay
{
  /// <summary>
  /// Summary for WeatherOverlay.
  /// </summary>
  public class MyWeatherOverlay : GUIOverlayWindow, IRenderLayer, ISetupForm
  {
    const char DEGREE_CHARACTER = (char)176;				//the degree 'o' character

    enum SkinControls
    {
      CONTROL_CURRENT_TEMP = 2,
      CONTROL_CURRENT_IMAGE = 3,
      CONTROL_LOCATION = 4,
      CONTROL_WEATHER_DESCRIPTION = 5
    }

    [SkinControlAttribute((int)SkinControls.CONTROL_CURRENT_IMAGE)]    protected GUIImage imgImage = null;

    public MyWeatherOverlay()
    {
      GetID = 7002;
      //
      // TODO: Add the constructor logic here.
      //
    }

    public override bool DoesPostRender()
    {
      if (GUIGraphicsContext.IsFullScreenVideo)
        return false;
      if (!GUIGraphicsContext.Overlay)
        return false;
      return true;
    }

    public override bool Init()
    {
      GUILayerManager.RegisterLayer(this, GUILayerManager.LayerType.WeatherOverlay);
      bool bResult = Load(GUIGraphicsContext.Skin + @"\weatherOverlay.xml");
      GetID = 7002;

      GUIFadeLabel lblCurrentTemp = (GUIFadeLabel)GetControl((int)SkinControls.CONTROL_CURRENT_TEMP);
      GUIFadeLabel lblLocation = (GUIFadeLabel)GetControl((int)SkinControls.CONTROL_LOCATION);
      GUIFadeLabel lblWeatherDescription = (GUIFadeLabel)GetControl((int)SkinControls.CONTROL_WEATHER_DESCRIPTION);
      imgImage = (GUIImage)GetControl((int)SkinControls.CONTROL_CURRENT_IMAGE);
      if (LoadWeather(Config.GetFile(Config.Dir.Weather, "curWeather.xml")))
      {
        GUIControl.SetControlLabel(GetID, (int)SkinControls.CONTROL_CURRENT_TEMP, _nowTemp);
        GUIControl.SetControlLabel(GetID, (int)SkinControls.CONTROL_LOCATION, _nowLocation);
        GUIControl.SetControlLabel(GetID, (int)SkinControls.CONTROL_WEATHER_DESCRIPTION, _nowCond);
        //MessageBox.Show("INIT: read weather info: temp=" + _nowTemp + ",location=" + _nowLocation + ",condition=" + _nowCond);
      }
      else
        bResult = false;
      return bResult;
    }

    public override bool SupportsDelayedLoad
    {
      get { return false; }
    }

    public override void PreInit()
    {
      base.PreInit();
      AllocResources();
    }

    public override void PostRender(float timePassed, int iLayer)
    {
      if (iLayer != 3)
        return;
      if (GUIGraphicsContext.Overlay == false)
        return;

      GUIControl.SetControlLabel(GetID, (int)SkinControls.CONTROL_CURRENT_TEMP, _nowTemp);
      GUIControl.SetControlLabel(GetID, (int)SkinControls.CONTROL_LOCATION, _nowLocation);
      GUIControl.SetControlLabel(GetID, (int)SkinControls.CONTROL_WEATHER_DESCRIPTION, _nowCond);
      // GUIImage imgImage = (GUIImage)GetControl((int)SkinControls.CONTROL_CURRENT_IMAGE); !!!!
      Log.Info("Weather - Now Icon: {0}", _nowIcon);
      if (imgImage != null)
      {
        imgImage.FreeResources();
        imgImage.SetFileName(_nowIcon);
        imgImage.AllocResources();
      }
      //Log.Info("Weather - SetControlLabel: CONTROL_CURRENT_IMAGE {0}", _nowIcon);
      base.Render(timePassed);
    }

    #region IRenderLayer
    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      PostRender(timePassed, 3);
    }
    #endregion

    #region WeatherHelpers
    // variables used to store weather information
    string _nowLocation, _nowIcon, _nowCond, _nowTemp, _nowFeel, _nowHumd, _nowDewp;
    // load weather from xml file
    bool LoadWeather(string weatherFile)
    {
      int tempInteger = 0;
      string tempString = String.Empty;
      string unitTemperature = String.Empty;
      string unitSpeed = String.Empty;
      DateTime time = DateTime.Now;

      // load the xml file
      XmlDocument doc = new XmlDocument();
      doc.Load(weatherFile);

      if (doc.DocumentElement == null)
        return false;

      string root = doc.DocumentElement.Name;
      XmlNode xmlElement = doc.DocumentElement;
      if (root == "error")
      {
        //string szCheckError;

        //GUIDialogOK dialogOk = (GUIDialogOK)GUIWindowManager.GetWindow(7002);

        //GetString(xmlElement, "err", out szCheckError, "Unknown Error");	//grab the error string

        //// show error dialog...
        //dialogOk.SetHeading(412);	//"Unable to get weather data"
        //dialogOk.SetLine(1, szCheckError);
        //dialogOk.SetLine(2, _nowLocation);
        //dialogOk.SetLine(3, String.Empty);
        //dialogOk.DoModal(GetID);
        return true;	//we got a message so do display a second in refreshme()
      }

      // units (C or F and mph or km/h or m/s) 
      unitTemperature = "C";//_temperatureFarenheit;

      //if (_windSpeed[0] == 'M')
      //    unitSpeed = "mph";
      //else if (_windSpeed[0] == 'K')
      //    unitSpeed = "km/h";
      //else
      //    unitSpeed = "m/s";
      unitSpeed = "km/h";

      // location
      XmlNode element = xmlElement.SelectSingleNode("loc");
      if (null != element)
      {
        GetString(element, "dnam", out _nowLocation, String.Empty);
      }

      //current weather
      element = xmlElement.SelectSingleNode("cc");
      if (null != element)
      {
        GetInteger(element, "icon", out tempInteger);
        _nowIcon = Config.GetFile(Config.Dir.Weather, String.Format(@"128x128\{0}.png", tempInteger));

        GetString(element, "t", out _nowCond, String.Empty);			//current condition
        _nowCond = LocalizeOverview(_nowCond);
        SplitLongString(ref _nowCond, 8, 15);				//split to 2 lines if needed

        GetInteger(element, "tmp", out tempInteger);				//current temp
        _nowTemp = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);
        GetInteger(element, "flik", out tempInteger);				//current 'Feels Like'
        _nowFeel = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);

        XmlNode pNestElement = element.SelectSingleNode("wind");	//current wind
        //ParseAndBuildWindString(pNestElement, unitSpeed, out _nowWind);

        GetInteger(element, "hmid", out tempInteger);				//current humidity
        _nowHumd = String.Format("{0}%", tempInteger);

        pNestElement = element.SelectSingleNode("uv");	//current UV index
        if (null != pNestElement)
        {
          GetInteger(pNestElement, "i", out tempInteger);
          GetString(pNestElement, "t", out  tempString, String.Empty);
          //_nowUVId = String.Format("{0} {1}", tempInteger, LocalizeOverview(tempString));
        }

        GetInteger(element, "dewp", out tempInteger);				//current dew point
        _nowDewp = String.Format("{0}{1}{2}", tempInteger, DEGREE_CHARACTER, unitTemperature);

      }
      return true;
    }


    void GetString(XmlNode xmlElement, string tagName, out string stringValue, string defaultValue)
    {
      stringValue = String.Empty;

      XmlNode node = xmlElement.SelectSingleNode(tagName);
      if (node != null)
      {
        if (node.InnerText != null)
        {
          if (node.InnerText != "-")
            stringValue = node.InnerText;
        }
      }
      if (stringValue.Length == 0)
      {
        stringValue = defaultValue;
      }
    }


    void GetInteger(XmlNode xmlElement, string tagName, out int intValue)
    {
      intValue = 0;
      XmlNode node = xmlElement.SelectSingleNode(tagName);
      if (node != null)
      {
        if (node.InnerText != null)
        {
          try
          {
            intValue = Int32.Parse(node.InnerText);
          }
          catch (Exception)
          {
          }
        }
      }
    }


    string LocalizeOverview(string token)
    {
      string localizedLine = String.Empty;

      foreach (string tokenSplit in token.Split(' '))
      {
        string localizedWord = String.Empty;

        if (String.Compare(tokenSplit, "T-Storms", true) == 0 || String.Compare(tokenSplit, "T-Storm", true) == 0)
          localizedWord = GUILocalizeStrings.Get(370);
        else if (String.Compare(tokenSplit, "Partly", true) == 0)
          localizedWord = GUILocalizeStrings.Get(371);
        else if (String.Compare(tokenSplit, "Mostly", true) == 0)
          localizedWord = GUILocalizeStrings.Get(372);
        else if (String.Compare(tokenSplit, "Sunny", true) == 0)
          localizedWord = GUILocalizeStrings.Get(373);
        else if (String.Compare(tokenSplit, "Cloudy", true) == 0 || String.Compare(tokenSplit, "Clouds", true) == 0)
          localizedWord = GUILocalizeStrings.Get(374);
        else if (String.Compare(tokenSplit, "Snow", true) == 0)
          localizedWord = GUILocalizeStrings.Get(375);
        else if (String.Compare(tokenSplit, "Rain", true) == 0)
          localizedWord = GUILocalizeStrings.Get(376);
        else if (String.Compare(tokenSplit, "Light", true) == 0)
          localizedWord = GUILocalizeStrings.Get(377);
        else if (String.Compare(tokenSplit, "AM", true) == 0)
          localizedWord = GUILocalizeStrings.Get(378);
        else if (String.Compare(tokenSplit, "PM", true) == 0)
          localizedWord = GUILocalizeStrings.Get(379);
        else if (String.Compare(tokenSplit, "Showers", true) == 0 || String.Compare(tokenSplit, "Shower", true) == 0 || String.Compare(tokenSplit, "T-Showers", true) == 0)
          localizedWord = GUILocalizeStrings.Get(380);
        else if (String.Compare(tokenSplit, "Few", true) == 0)
          localizedWord = GUILocalizeStrings.Get(381);
        else if (String.Compare(tokenSplit, "Scattered", true) == 0 || String.Compare(tokenSplit, "Isolated", true) == 0)
          localizedWord = GUILocalizeStrings.Get(382);
        else if (String.Compare(tokenSplit, "Wind", true) == 0)
          localizedWord = GUILocalizeStrings.Get(383);
        else if (String.Compare(tokenSplit, "Strong", true) == 0)
          localizedWord = GUILocalizeStrings.Get(384);
        else if (String.Compare(tokenSplit, "Fair", true) == 0)
          localizedWord = GUILocalizeStrings.Get(385);
        else if (String.Compare(tokenSplit, "Clear", true) == 0)
          localizedWord = GUILocalizeStrings.Get(386);
        else if (String.Compare(tokenSplit, "Early", true) == 0)
          localizedWord = GUILocalizeStrings.Get(387);
        else if (String.Compare(tokenSplit, "and", true) == 0)
          localizedWord = GUILocalizeStrings.Get(388);
        else if (String.Compare(tokenSplit, "Fog", true) == 0)
          localizedWord = GUILocalizeStrings.Get(389);
        else if (String.Compare(tokenSplit, "Haze", true) == 0)
          localizedWord = GUILocalizeStrings.Get(390);
        else if (String.Compare(tokenSplit, "Windy", true) == 0)
          localizedWord = GUILocalizeStrings.Get(391);
        else if (String.Compare(tokenSplit, "Drizzle", true) == 0)
          localizedWord = GUILocalizeStrings.Get(392);
        else if (String.Compare(tokenSplit, "Freezing", true) == 0)
          localizedWord = GUILocalizeStrings.Get(393);
        else if (String.Compare(tokenSplit, "N/A", true) == 0)
          localizedWord = GUILocalizeStrings.Get(394);
        else if (String.Compare(tokenSplit, "Mist", true) == 0)
          localizedWord = GUILocalizeStrings.Get(395);
        else if (String.Compare(tokenSplit, "High", true) == 0)
          localizedWord = GUILocalizeStrings.Get(799);
        else if (String.Compare(tokenSplit, "Low", true) == 0)
          localizedWord = GUILocalizeStrings.Get(798);
        else if (String.Compare(tokenSplit, "Moderate", true) == 0)
          localizedWord = GUILocalizeStrings.Get(534);
        else if (String.Compare(tokenSplit, "Late", true) == 0)
          localizedWord = GUILocalizeStrings.Get(553);
        else if (String.Compare(tokenSplit, "Very", true) == 0)
          localizedWord = GUILocalizeStrings.Get(554);
        // wind directions
        else if (String.Compare(tokenSplit, "N", true) == 0)
          localizedWord = GUILocalizeStrings.Get(535);
        else if (String.Compare(tokenSplit, "E", true) == 0)
          localizedWord = GUILocalizeStrings.Get(536);
        else if (String.Compare(tokenSplit, "S", true) == 0)
          localizedWord = GUILocalizeStrings.Get(537);
        else if (String.Compare(tokenSplit, "W", true) == 0)
          localizedWord = GUILocalizeStrings.Get(538);
        else if (String.Compare(tokenSplit, "NE", true) == 0)
          localizedWord = GUILocalizeStrings.Get(539);
        else if (String.Compare(tokenSplit, "SE", true) == 0)
          localizedWord = GUILocalizeStrings.Get(540);
        else if (String.Compare(tokenSplit, "SW", true) == 0)
          localizedWord = GUILocalizeStrings.Get(541);
        else if (String.Compare(tokenSplit, "NW", true) == 0)
          localizedWord = GUILocalizeStrings.Get(542);
        else if (String.Compare(tokenSplit, "Thunder", true) == 0)
          localizedWord = GUILocalizeStrings.Get(543);
        else if (String.Compare(tokenSplit, "NNE", true) == 0)
          localizedWord = GUILocalizeStrings.Get(544);
        else if (String.Compare(tokenSplit, "ENE", true) == 0)
          localizedWord = GUILocalizeStrings.Get(545);
        else if (String.Compare(tokenSplit, "ESE", true) == 0)
          localizedWord = GUILocalizeStrings.Get(546);
        else if (String.Compare(tokenSplit, "SSE", true) == 0)
          localizedWord = GUILocalizeStrings.Get(547);
        else if (String.Compare(tokenSplit, "SSW", true) == 0)
          localizedWord = GUILocalizeStrings.Get(548);
        else if (String.Compare(tokenSplit, "WSW", true) == 0)
          localizedWord = GUILocalizeStrings.Get(549);
        else if (String.Compare(tokenSplit, "WNW", true) == 0)
          localizedWord = GUILocalizeStrings.Get(551);
        else if (String.Compare(tokenSplit, "NNW", true) == 0)
          localizedWord = GUILocalizeStrings.Get(552);
        else if (String.Compare(tokenSplit, "VAR", true) == 0)
          localizedWord = GUILocalizeStrings.Get(556);
        else if (String.Compare(tokenSplit, "CALM", true) == 0)
          localizedWord = GUILocalizeStrings.Get(557);
        else if (String.Compare(tokenSplit, "Storm", true) == 0 || String.Compare(tokenSplit, "Gale", true) == 0 || String.Compare(tokenSplit, "Tempest", true) == 0)
          localizedWord = GUILocalizeStrings.Get(599);
        else if (String.Compare(tokenSplit, "in the Vicinity", true) == 0)
          localizedWord = GUILocalizeStrings.Get(559);
        else if (String.Compare(tokenSplit, "Clearing", true) == 0)
          localizedWord = GUILocalizeStrings.Get(560);

        if (localizedWord == String.Empty)
          localizedWord = tokenSplit;	//if not found, let fallback

        localizedLine = localizedLine + localizedWord;
        localizedLine += " ";
      }

      return localizedLine;

    }


    //splitStart + End are the chars to search between for a space to replace with a \n
    void SplitLongString(ref string lineString, int splitStart, int splitEnd)
    {
      //search chars 10 to 15 for a space
      //if we find one, replace it with a newline
      for (int i = splitStart; i < splitEnd && i < (int)lineString.Length; i++)
      {
        if (lineString[i] == ' ')
        {
          lineString = lineString.Substring(0, i) + "\n" + lineString.Substring(i + 1);
          return;
        }
      }
    }
    #endregion

    #region ISetupForm Members
    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return "MyWeatherOverlay";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "This adds a Weather Overlay for skins";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "oore.mofux.net";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      MessageBox.Show("No Setup required. Skinners use screen ID: 7002");
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return true;
    }

    // get ID of windowplugin belonging to this setup
    public int GetWindowId()
    {
      return 7002;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return false; // Not all skins enable this feature.
    }

    // indicates if a plugin has its own setup screen
    public bool HasSetup()
    {
      return false; //Not yet
    }

    /// <summary>
    /// If the plugin should have its own button on the main menu of Media Portal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true : plugin needs its own button on home
    /// false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = "this should never be seen";
      strButtonImage = "";
      strButtonImageFocus = "";
      strPictureImage = "";
      return false;
    }

    public override int GetID
    {
      get
      {
        return 7002;
      }

      set
      {
      }
    }
    #endregion
  }
}