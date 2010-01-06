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

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Util;

namespace ProcessPlugins.CallerId
{
  /// <summary>
  /// Summary description for CallerIdPlugin.
  /// </summary>
  public class CallerIdPlugin : IPlugin, ISetupForm
  {
    private bool stopListeningFlag = false;
    private const string ERR_FAILED_TO_FIND_AREACODE_XML = "xml file couldn't be found";
    private const string SUCCESS_LOADED_AREACODE_XML = "xml file loaded";
    private Hashtable areaCodeLookup;

    public CallerIdPlugin() {}

    private Hashtable AreaCodeToLocationMap
    {
      get
      {
        if (areaCodeLookup == null)
        {
          string areaCodeXMLFile = Config.GetFile(Config.Dir.Config, "yac-area-codes.xml");
          string npa, location;
          Hashtable temp = new Hashtable();

          if (File.Exists(areaCodeXMLFile))
          {
            XmlDocument source = new XmlDocument();
            source.Load(areaCodeXMLFile);
            XmlNodeList areacodeNodes = source.SelectNodes("/areacodes/areacode");


            XmlNode areacodeNode;
            temp.Add("000", SUCCESS_LOADED_AREACODE_XML); // slot 000 reserved for hashtable status

            for (int i = 0; i < areacodeNodes.Count; i++) // Loop through, pulling npa and location.
            {
              areacodeNode = areacodeNodes[i];
              npa = areacodeNode.Attributes["npa"].Value;
              location = areacodeNode.Attributes["location"].Value;
              if (!temp.Contains(npa))
              {
                temp.Add(npa, location);
              }
            }
          }
          else // the file doesn't exist; put something in the lookup table to indicate it wasn't.
          {
            // TODO detect that it wasn't loaded succesfully or other error conditions, if it *was* located.
            temp.Add("000", ERR_FAILED_TO_FIND_AREACODE_XML); // slot 000 reserved for hashtable status
            Log.Error("couldn't load xml from " + areaCodeXMLFile, "error");
          }
          areaCodeLookup = temp;
        }
        return areaCodeLookup;
      }
    }

    protected bool HasImage(String path) //works on http, UNC, or local paths
    {
      bool result = false;
      // TODO: ensure that the chars I'm looking for are appropriate for the file system/protocol
      // TODO: ensure that the sought file name isn't "too long"
      try
      {
        if ((path.StartsWith("http")))
          // based on http://cephas.net/blog/2003/10/25/failsafe_amazon_image_using_java_c_coldfusion.html
        {
          HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(path);
          try
          {
            // Use the current user in case an NTLM Proxy or similar is used.
            // request.Proxy = WebProxy.GetDefaultProxy();
            webreq.Proxy.Credentials = CredentialCache.DefaultCredentials;
          }
          catch (Exception) {}
          WebResponse response = webreq.GetResponse();
          if (response.ContentType.StartsWith("image"))
          {
            result = true;
          }
          response.Close();
        }
        else
        {
          result = File.Exists(path);
        }
      }
      catch {}
      return result;
    }

    #region IPlugin Members

    public void Start()
    {
      string location = (string)AreaCodeToLocationMap["206"];
      //for improved perf, preload the hashtable for areacodes xml file now
      Thread workerThread = new Thread(new ThreadStart(YACListen));
      workerThread.IsBackground = true;
      workerThread.Name = "YAC Listener";
      workerThread.Start();
    }

    public void Stop()
    {
      lock (this)
      {
        stopListeningFlag = true;
      }
    }


    private void YACListen()
    {
      int port = 10629; //YAC's port
      string inboundRawYACData = null;

      try
      {
        object[] oButtons = new Object[1];

        TcpListener server = new TcpListener(IPAddress.Any, port);

        server.Start(); // Start listening for client requests.

        string yaclogofile = String.Format(@"{0}\yac-small.png", Thumbs.Yac);
        if (HasImage(yaclogofile))
        {
          // found the image store
        }

        while (!stopListeningFlag) // main listening loop
        {
          lock (this)
          {
            if (server.Pending()) // AcceptTCPClient is synchronous, so only enter it when we've got a listener pending 
            {
              TcpClient client = server.AcceptTcpClient();
              string currentTime = DateTime.Now.ToString("t");

              inboundRawYACData = null;

              NetworkStream stream = client.GetStream();
              StreamReader sreader = new StreamReader(stream);
              inboundRawYACData = sreader.ReadToEnd();
              client.Close(); // Shutdown and end connection


              if (inboundRawYACData.Length > 513) // chop off more anything beyond a "reasonable" payload
              {
                inboundRawYACData.Remove(512, (inboundRawYACData.Length - 511));
              }

              bool parsedCall = PostYACDialog(inboundRawYACData, currentTime);

              if (parsedCall == false)
              {
                Log.Info("failed to parse call");
              }
            }
            Thread.Sleep(100); // breathe for a moment, so we don't peg the CPU in this tight loop
          }
        }
      }
      catch (Exception e)
      {
        // TODO log the exception text, but not until I'm sure I'm not going to throw while logging.

        string eExceptionStr = "Exception: " + e.Message;
        string eTargetSiteStr = "TargetSite: " + e.TargetSite;
        Log.Error(String.Format("{0} {1}", eExceptionStr, eTargetSiteStr));
      }
    }


    protected bool PostYACDialog(string inboundRawYACData, string currentTime)
    {
      string imagepathStr = null;
      string titlebarStr = null;
      string bodyStr = null;
      string targetPicturePath = null;

      try
      {
        //   YAC phone call format is: @CALLYAC Test Call~(425) 555-1212 -- note that this is the *usual* number format, but not the only one
        // YAC text message format is: text of message

        if (inboundRawYACData.StartsWith("@CALL")) // it's a call
        {
          inboundRawYACData = inboundRawYACData.Remove(0, 5); // get rid of the @CALL label

          // Split the name & phone number at the tilde
          char[] delimiter = {'~'};
          string[] split = null;
          split = inboundRawYACData.Split(delimiter, 2);
          string callernameStr = split[0].Trim();
          string phonenumStr = split[1].Trim();

          // TODO: ensure that my payloads are of reasonable size

          switch (phonenumStr.ToLower())
          {
            case "":
              {
                // no phonenum string payload, use default image
                break;
              }
            case "out of area":
              {
                targetPicturePath = String.Format(@"{0}\out-of-area.jpg", Thumbs.Yac);
                break;
              }
            case "private number":
              {
                targetPicturePath = String.Format(@"{0}\private-number.jpg", Thumbs.Yac);
                break;
              }
            case "wireless caller":
              {
                targetPicturePath = String.Format(@"{0}\wireless-caller.jpg", Thumbs.Yac);
                break;
              }
            default: // see if there's numeric-only content and act on what I find, plus other scrutiny
              {
                string numericOnlyPhoneNumber = phonenumStr;
                Regex numericOnlyRegex = new Regex(@"[^\d]");
                numericOnlyPhoneNumber = numericOnlyRegex.Replace(numericOnlyPhoneNumber, "");

                if (numericOnlyPhoneNumber.Length == 0)
                {
                  // only try perfect match for whatever's in phonenumStr when numericOnlyPhoneNumber is blank
                  //  for perf reasons, since I expect to see it less often
                  targetPicturePath = String.Format(@"{0}\{1}", Thumbs.Yac, phonenumStr.ToLower());
                  if (HasImage(targetPicturePath))
                  {
                    imagepathStr = targetPicturePath;
                    // TODO: I shouldn't do any more image-looking; I'm done.
                  }
                  else
                  {
                    // there is no image available for phonenumStr
                  }
                }
                else // we've got content for numericOnlyPhoneNumber
                {
                  // see if I've got a perfect match for numericOnlyPhoneNumber, whatever the length
                  targetPicturePath = String.Format(@"{0}\{1}.jpg", Thumbs.Yac, numericOnlyPhoneNumber);
                  if (HasImage(targetPicturePath))
                  {
                    imagepathStr = targetPicturePath;
                    // TODO: I shouldn't do any more image-looking; I'm done.
                  }
                  else // no perfect match for number
                  {
                    // perfect match for caller string?
                    targetPicturePath = String.Format(@"{0}\{1}.jpg", Thumbs.Yac, callernameStr);
                    if (HasImage(targetPicturePath))
                    {
                      imagepathStr = targetPicturePath;
                      // TODO: I shouldn't do any more image-looking; I'm done.
                    }

                    else // no perfect match on caller name
                    {
                      switch (numericOnlyPhoneNumber.Length)
                        //   finally, try to chop bits of the number off & look for them in order.
                      {
                          // case 7: 
                          //   create a 10-digit number from the area code that *may* be present in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Telephony\Locations\Location1\AreaCode
                          //   extra credit: extract location (area code/state) from MC PostalCode
                          // goto case 10;

                        case 11: // remove first digit if it's a 1, and then use 10-digit logic 
                          // assume that the 11 digits include a bogus first digit (that it's really a 3+7 US-style number)
                          numericOnlyPhoneNumber = numericOnlyPhoneNumber.Substring(1, 10);
                          goto case 10;
                          // msdn says "Unlike the C++ switch statement, C# does not support an explicit fall through from one case label to another. If you want, you can use goto a switch-case, or goto default."

                        case 10: // default logic for original version
                          {
                            string targetareacodeStr = numericOnlyPhoneNumber.Substring(0, 3);
                            targetPicturePath = String.Format(@"{0}\{1}.jpg", Thumbs.Yac, targetareacodeStr);

                            if (HasImage(targetPicturePath))
                            {
                              imagepathStr = targetPicturePath;
                            }
                            else
                            {
                              // try to find a picture for the areacode's location (state, province, etc.)
                              string location = (string)AreaCodeToLocationMap[targetareacodeStr];

                              if (location == null)
                              {
                                // leave imagepath set to null; no picture for caller, areacode or location
                              }
                              else
                              {
                                location = location.ToLower();
                                targetPicturePath = String.Format(@"{0}\{1}.jpg", Thumbs.Yac, location);

                                // add location string to caller name if we've got a location, but no better picture

                                if (HasImage(targetPicturePath))
                                {
                                  imagepathStr = targetPicturePath;
                                }
                                callernameStr = callernameStr + " (" + location + ")";
                              }
                            }
                            break;
                          } // end 10-digit case

                        default: // there's a number, but I don't know how to process that length
                          {
                            // a picture could go here if you wanted, but a better place is at the default in the switch for all calls
                            break;
                          }
                      } // end switch (numericOnlyPhoneNumber.Length)
                    } // end else // no perfect match on caller name
                  } // end no perfect match for number
                } // end content in numericOnlyPhoneNumber

                // TODO figure out why this doesn't give an image for an otherwise unknown wireless caller
                if ((callernameStr.ToLower() == "wireless caller") & (imagepathStr == null))
                  // use this graphic whenever I don't have a better picture for a wireless caller
                {
                  targetPicturePath = String.Format(@"{0}\wireless-caller.jpg", Thumbs.Yac);
                }

                // a default picture assignment would go here if you wanted to have one for every call
                break;
              } // end default handler for switch (phonenumStr.ToLower)						
          } // end switch (phonenumStr.ToLower)

          titlebarStr = callernameStr;
          bodyStr = "Call from " + phonenumStr + " at " + currentTime.ToLower();
        } // end if (inboundRawYACData.StartsWith("@CALL"))  // it's a call

        else // it's a text message
        {
          titlebarStr = "Text Message";
          bodyStr = inboundRawYACData + " at " + currentTime.ToLower();

          targetPicturePath = String.Format(@"{0}\{1}.jpg", Thumbs.Yac, inboundRawYACData.Trim());
          // a picture for the text message payload?
          if (HasImage(targetPicturePath))
          {
            imagepathStr = targetPicturePath;
          }
          else
          {
            targetPicturePath = String.Format(@"{0}\text-message.jpg", Thumbs.Yac);
          }
        }

        if (HasImage(targetPicturePath))
        {
          imagepathStr = targetPicturePath;
        }
        else
        {
          imagepathStr = "";
          // ensure imagepath is set to null; no picture for the error case
        }

        Log.Info(@"***** INCOMING CALL: " + DateTime.Now.ToString("F") + @" *****");
        Log.Info("   Name: " + titlebarStr);
        Log.Info("   Number: " + bodyStr);
        Log.Info("   Image: " + imagepathStr);


        //show message...
        GUIMessage msg = new GUIMessage(GUIMessage.MessageType.GUI_MSG_NOTIFY, 0, 0, 0, 0, 0, 0);
        msg.Label = titlebarStr;
        msg.Label2 = bodyStr;
        msg.Label3 = imagepathStr;
        GUIGraphicsContext.SendMessage(msg);
        //				mcHost.HostControl.Dialog(bodyStr, titlebarStr, oButtons, 20, false, imagepathStr, new OnDialogCloseDelegate(OnButtonPress));
      } // end try


      catch (Exception e)
      {
        // TODO log the exception text, but not until I'm sure I'm not going to throw while logging.

        string eExceptionStr = "Exception: " + e.Message;
        string eTargetSiteStr = "TargetSite: " + e.TargetSite;
        Log.Info(String.Format("{0} {1}", eExceptionStr, eTargetSiteStr));
        return false;
      }
      return true;
    }

    #endregion

    #region ISetupForm Members

    public bool CanEnable()
    {
      return true;
    }

    public string Description()
    {
      return "Displays analog caller-ID information";
    }

    public bool DefaultEnabled()
    {
      return false;
    }

    public int GetWindowId()
    {
      // TODO:  Add CallerIdPlugin.GetWindowId implementation
      return -1;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      // TODO:  Add CallerIdPlugin.GetHome implementation
      strButtonText = null;
      strButtonImage = null;
      strButtonImageFocus = null;
      strPictureImage = null;
      return false;
    }

    public string Author()
    {
      return "Frodo";
    }

    public string PluginName()
    {
      return "YAC Caller-ID";
    }

    public bool HasSetup()
    {
      // TODO:  Add CallerIdPlugin.HasSetup implementation
      return false;
    }

    public void ShowPlugin()
    {
      // TODO:  Add CallerIdPlugin.ShowPlugin implementation
    }

    #endregion
  }
}