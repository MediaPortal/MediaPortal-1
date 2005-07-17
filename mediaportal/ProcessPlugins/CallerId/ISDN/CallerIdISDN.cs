using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace ProcessPlugins.CallerId
{
  /// <summary>
  /// Media Portal IPlugin (Process) / by mPod
  /// Caller-ID for ISDN incl. caller-details lookup
  /// 
  /// relies on:
  /// - ISDN CAPI 2.0 (did not test with VoIP CAPI)
  /// - Microsoft Outlook (tested with Outlook 2003)
  /// </summary>
  public class CallerIdISDN : ISetupForm, IPlugin
  {
    const string ERR_FAILED_TO_FIND_AREACODE_XML    = "ISDN: Area code XML file cannot be found";
    const string SUCCESS_LOADED_AREACODE_XML        = "ISDN: Area code XML file loaded";
    const string ERR_FAILED_TO_FIND_COUNTRYCODE_XML = "ISDN: Country code XML file cannot be found";
    const string SUCCESS_LOADED_COUNTRYCODE_XML     = "ISDN: Country code XML file loaded";

    static Hashtable areaCodeLookup;
    static Hashtable countryCodeLookup;
    static Hashtable countryTranslator;
    static string myCountryCode;
    static string myAreaCode;
    bool ISDNdisabled = false;

    ISDNWatch ISDNWatch;

    static private Hashtable AreaCodeLookup
    {
      get
      {
        if (areaCodeLookup == null)
        {
          string areaCodeXMLFile = "ISDNCodes.xml";
          string areaCode, location;
          Hashtable areaTable = new Hashtable();
          areaTable.Add("", Strings.Unknown);

          if (File.Exists(areaCodeXMLFile)) 
          {
            XmlDocument source = new XmlDocument();
            source.Load(areaCodeXMLFile); 
            XmlNodeList areaCodeNodes = source.SelectNodes("/codes/area");
						
            XmlNode areaCodeNode;
            areaTable.Add("000", SUCCESS_LOADED_AREACODE_XML);	// slot 000 reserved for hashtable status

            for (int i=0; i<areaCodeNodes.Count; i++)  // Loop through, pulling areacode and location
            {
              areaCodeNode = areaCodeNodes[i];
              if (areaCodeNode.Attributes["iso"].Value == (string)CountryCodeLookup[myCountryCode])
              {
                areaCode = areaCodeNode.Attributes["areacode"].Value;
                location = areaCodeNode.Attributes["location"].Value;

                if (!areaTable.Contains(areaCode))
                {
                  areaTable.Add(areaCode, location);
                }
              }
            }
          }
          else  // the file doesn't exist; put something in the lookup table to indicate it wasn't.
          {
            // TODO detect that it wasn't loaded succesfully or other error conditions, if it *was* located.
            areaTable.Add("000", ERR_FAILED_TO_FIND_AREACODE_XML);  // slot 000 reserved for hashtable status
            Log.WriteFile(Log.LogType.Log,true,"ISDN: Cannot load area codes from " + areaCodeXMLFile, "error");
          }


          areaCodeLookup = areaTable;
        } 
        return areaCodeLookup;
      }
    }

    static private Hashtable CountryCodeLookup
    {
      get
      {
        if (countryCodeLookup == null)
        {
          string countryCodeXMLFile = "ISDNCodes.xml";
          string countryCode, country;
          Hashtable countryTable = new Hashtable();
          countryTable.Add("+", Strings.Unknown);

          if (File.Exists(countryCodeXMLFile)) 
          {
            XmlDocument source = new XmlDocument();
            source.Load(countryCodeXMLFile); 
            XmlNodeList countryCodeNodes = source.SelectNodes("/codes/country");

            XmlNode countryCodeNode;
            countryTable.Add("000", SUCCESS_LOADED_COUNTRYCODE_XML);  // slot 000 reserved for hashtable status

            for (int i=0; i<countryCodeNodes.Count; i++)  // Loop through, pulling countrycode and country
            {
              countryCodeNode = countryCodeNodes[i];
              countryCode = countryCodeNode.Attributes["code"].Value;
              country = countryCodeNode.Attributes["iso"].Value;

              if (!countryTable.Contains(countryCode))
              {
                countryTable.Add("+" + countryCode, country);
              }
            }
          }
          else  // the file doesn't exist; put something in the lookup table to indicate it wasn't.
          {
            // TODO detect that it wasn't loaded succesfully or other error conditions, if it *was* located.
            countryTable.Add("000", ERR_FAILED_TO_FIND_COUNTRYCODE_XML);  // slot 000 reserved for hashtable status
            Log.WriteFile(Log.LogType.Log,true,"ISDN: Cannot load country codes from " + countryCodeXMLFile, "error");
          }
          countryCodeLookup = countryTable;
        } 
        return countryCodeLookup;
      }
    }

    static private Hashtable CountryTranslator
    {
      get
      {
        if (countryTranslator == null)
        {
          string translatorXMLFile = "ISDNCodes.xml";
          string countryShort, countryLong;
          Hashtable translatorTable = new Hashtable();
          translatorTable.Add(Strings.Unknown, Strings.Unknown);

          if (File.Exists(translatorXMLFile)) 
          {
            XmlDocument source = new XmlDocument();
            source.Load(translatorXMLFile); 
            XmlNodeList translatorNodes = source.SelectNodes("/codes/country");
						
            XmlNode translatorNode;
            translatorTable.Add("000", SUCCESS_LOADED_COUNTRYCODE_XML);	// slot 000 reserved for hashtable status

            for (int i=0; i<translatorNodes.Count; i++)  // Loop through, pulling areacode and location
            {
              translatorNode = translatorNodes[i];
              countryShort = translatorNode.Attributes["iso"].Value;
              countryLong = translatorNode.Attributes["name"].Value;

              if (!translatorTable.Contains(countryShort))
              {
                translatorTable.Add(countryShort, countryLong);
              }
            }
          }
          else  // the file doesn't exist; put something in the lookup table to indicate it wasn't.
          {
            // TODO detect that it wasn't loaded succesfully or other error conditions, if it *was* located.
            translatorTable.Add("000", ERR_FAILED_TO_FIND_COUNTRYCODE_XML);  // slot 000 reserved for hashtable status
            Log.WriteFile(Log.LogType.Log,true,"ISDN: Cannot load translator codes from " + translatorXMLFile, "error");
          }
          countryTranslator = translatorTable;
        } 
        return countryTranslator;
      }
    }


    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "ISDN Caller-ID Plugin";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "mPod";
    }

    public string PluginName()
    {
      return "ISDN Caller-ID";
    }

    public bool HasSetup()
    {
      return false;
    }

    public void ShowPlugin()
    {
    }

    #endregion

    #region IPlugin Members

    public void Start()
    {
      if (ISDNWatch.CapiInstalled)
      {
        OutlookHelper.Caller dummy = OutlookHelper.OutlookLookup("dummy");  // First Outlook-lookup might take some time, so let's do this here

        ISDNWatch.LocationInfo locationInfo = ISDNWatch.GetLocationInfo();
        myCountryCode = "+" + locationInfo.CountryCode;
        string myCountry = (string)CountryCodeLookup[myCountryCode];
        if (myCountry == null)
          myCountry = Strings.Unknown;
        string myCountryLong = (string)CountryTranslator[myCountry];
        myAreaCode = locationInfo.AreaCode;
        string myArea = (string)AreaCodeLookup[myAreaCode];
        if (myArea == null)
          myArea = Strings.Unknown;
        if (myAreaCode != "")
          Log.Write("ISDN: Home location: {0} ({1}), {2} ({3})", myArea, myAreaCode, myCountryLong, myCountryCode);

        ISDNWatch = new ISDNWatch();
        ISDNWatch.Start();
        ISDNWatch.CidReceiver  += new ISDNWatch.EventHandler(ProcessCallerId);
      }
      else
      {
        ISDNdisabled = true;
        Log.Write("ISDN: CAPI error. No ISDN card installed? Caller-ID disabled.");
      }
    }

    public void Stop()
    {
      if (!ISDNdisabled)
        ISDNWatch.Stop();
    }

    #endregion

    void ProcessCallerId(string callerId)
    {
      if (callerId != null)
      {
        string country;
        string countryCode;
        string outlookQuery;

        if (callerId[0] == '+')
        {
          // International caller
          int posCountryCode = 0;
          country = null;
          while (country == null)
          {
            posCountryCode++;
            country = (string)CountryCodeLookup[callerId.Substring(0, posCountryCode)];
          }
          countryCode = callerId.Substring(0, posCountryCode);
          callerId = callerId.Remove(0, posCountryCode);
        }
        else
        {
          // Home country caller
          country = (string)CountryCodeLookup[myCountryCode];
          countryCode = myCountryCode;
        }
        if (country == null)
          country = "";

        // Parse area code
        int posAreaCode = callerId.Length;
        string location = null;
        while (location == null)
        {
          location = (string)AreaCodeLookup[callerId.Substring(0, posAreaCode)];
          if (location == null)
            posAreaCode--;
        }

        string areaCode = callerId.Substring(0, posAreaCode);
        string phoneNumber = callerId.Remove(0, posAreaCode);
        if (location != Strings.Unknown)
          outlookQuery = countryCode + " (" + areaCode + ") " + phoneNumber;
        else
          outlookQuery = countryCode + "  (I) " + phoneNumber;
        OutlookHelper.Caller caller = OutlookHelper.OutlookLookup(outlookQuery);

        if (caller.Name != null)
          Log.Write("ISDN: Incoming call from {0} ({1}, {2} / {3})", caller.Name, location, (string)CountryTranslator[country], outlookQuery);
        else
          Log.Write("ISDN: Incoming call ({0}, {1} / {2})", location, (string)CountryTranslator[country], outlookQuery);


        // Notify window popup
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY, 0, 0, 0, 0, 0, 0);
        msg.Label = "Incoming call";
        if (country != Strings.Unknown)
        {
          msg.Label = msg.Label + " from " + location + ", " + (string)CountryTranslator[country];
          if (caller.Name != null)
          {
            msg.Label2 = caller.Name + "\n\n(" + caller.Type + ")";
            if (caller.HasPicture)
              msg.Label3 = Thumbs.Yac + @"\ContactPicture.jpg";
            else
              msg.Label3 = Thumbs.Yac + @"\text-message.jpg";
          }
          else
          {
            msg.Label2 = outlookQuery;
          }
        }
        else
          msg.Label2 = callerId + "\n\nAn error occurred.\nSee the log file for details.";
        
        GUIGraphicsContext.SendMessage(msg);
      }
    }
  }
}