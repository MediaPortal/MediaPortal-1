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

using System.IO;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using Microsoft.Office.Interop.Outlook;
using Exception=System.Exception;

namespace ProcessPlugins.CallerId
{
  /// <summary>
  /// Summary description for OutlookHelper.
  /// </summary>
  public class OutlookHelper
  {
    public struct Caller
    {
      public string Name;
      public string Type;
      public bool HasPicture;
    }

    public static Caller OutlookLookup(string outlookQuery)
    {
      Caller found = new Caller();

      try
      {
        Application olApplication = new Application();
        NameSpace olNamespace = olApplication.GetNamespace("mapi");
        MAPIFolder olContacts = olNamespace.GetDefaultFolder(OlDefaultFolders.olFolderContacts);
        Items olItems = (Items) olContacts.Items;

        string sFilter
          = "[AssistantTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[Business2TelephoneNumber] = '" + outlookQuery + "' Or "
            + "[BusinessFaxNumber] = '" + outlookQuery + "' Or "
            + "[BusinessTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[CallbackTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[CarTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[CompanyMainTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[Home2TelephoneNumber] = '" + outlookQuery + "' Or "
            + "[HomeFaxNumber] = '" + outlookQuery + "' Or "
            + "[HomeTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[ISDNNumber] = '" + outlookQuery + "' Or "
            + "[MobileTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[OtherFaxNumber] = '" + outlookQuery + "' Or "
            + "[OtherTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[PrimaryTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[RadioTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[TelexNumber] = '" + outlookQuery + "' Or "
            + "[HomeTelephoneNumber] = '" + outlookQuery + "' Or "
            + "[TTYTDDTelephoneNumber] = '" + outlookQuery + "'";

        ContactItem olContactItem = (ContactItem) olItems.Find(sFilter);

        if (olContactItem != null)
        {
          if (olContactItem.AssistantTelephoneNumber == outlookQuery)
          {
            found.Type = "Assistant Telephone";
          }
          else if (olContactItem.Business2TelephoneNumber == outlookQuery)
          {
            found.Type = "Business 2 Telephone";
          }
          else if (olContactItem.BusinessFaxNumber == outlookQuery)
          {
            found.Type = "Business Fax";
          }
          else if (olContactItem.BusinessTelephoneNumber == outlookQuery)
          {
            found.Type = "Business Telephone";
          }
          else if (olContactItem.CallbackTelephoneNumber == outlookQuery)
          {
            found.Type = "Callback Telephone";
          }
          else if (olContactItem.CarTelephoneNumber == outlookQuery)
          {
            found.Type = "Car Telephone";
          }
          else if (olContactItem.CompanyMainTelephoneNumber == outlookQuery)
          {
            found.Type = "Company Main Telephone";
          }
          else if (olContactItem.Home2TelephoneNumber == outlookQuery)
          {
            found.Type = "Home 2 Telephone";
          }
          else if (olContactItem.HomeFaxNumber == outlookQuery)
          {
            found.Type = "Home Fax";
          }
          else if (olContactItem.HomeTelephoneNumber == outlookQuery)
          {
            found.Type = "Home Telephone";
          }
          else if (olContactItem.ISDNNumber == outlookQuery)
          {
            found.Type = "ISDN";
          }
          else if (olContactItem.MobileTelephoneNumber == outlookQuery)
          {
            found.Type = "Mobile Telephone";
          }
          else if (olContactItem.OtherFaxNumber == outlookQuery)
          {
            found.Type = "Other Fax";
          }
          else if (olContactItem.OtherTelephoneNumber == outlookQuery)
          {
            found.Type = "Other Telephone";
          }
          else if (olContactItem.PrimaryTelephoneNumber == outlookQuery)
          {
            found.Type = "Primary Telephone";
          }
          else if (olContactItem.RadioTelephoneNumber == outlookQuery)
          {
            found.Type = "Radio Telephone";
          }
          else if (olContactItem.TelexNumber == outlookQuery)
          {
            found.Type = "Telex";
          }
          else if (olContactItem.TTYTDDTelephoneNumber == outlookQuery)
          {
            found.Type = "TTYTDD Telephone";
          }

          found.Name = olContactItem.FullName;

          string applicationPath = System.Windows.Forms.Application.ExecutablePath;
          applicationPath = Path.GetFullPath(applicationPath);
          applicationPath = Path.GetDirectoryName(applicationPath);
          if (olContactItem.Attachments[1] != null)
          {
            Log.Debug(Thumbs.Yac + @"\" + olContactItem.Attachments[1].FileName);
            olContactItem.Attachments[1].SaveAsFile(Thumbs.Yac + @"\" + olContactItem.Attachments[1].FileName);
            found.HasPicture = true;
          }
        }
        olContactItem = null;
        olItems = null;
        olContacts = null;
        olNamespace = null;
        olApplication = null;
      }
      catch (Exception ex)
      {
        Log.Info("Outlook exception: {0}", ex.Message);
      }

      return found;
    }
  }
}