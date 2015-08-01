#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupControls.UserInterfaceControls;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Presentation;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.SetupTV
{
  public class Utils
  {
    [DllImport("kernel32.dll")]
    private static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out UInt64 lpFreeBytesAvailable,
                                                  out UInt64 lpTotalNumberOfBytes, out UInt64 lpTotalNumberOfFreeBytes);

    [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GetVolumeInformation(
      string RootPathName,
      StringBuilder VolumeNameBuffer,
      int VolumeNameSize,
      out uint VolumeSerialNumber,
      out uint MaximumComponentLength,
      out uint FileSystemFlags,
      StringBuilder FileSystemNameBuffer,
      int nFileSystemNameSize);

    [DllImport("kernel32.dll")]
    public static extern long GetDriveType(string driveLetter);


    // singleton. Dont allow any instance of this class
    private Utils() {}

    public static int getDriveType(string drive)
    {
      if (drive == null)
        return 2;
      long driveType = GetDriveType(drive);
      if ((driveType & 5) == 5)
        return 5; //cd
      if ((driveType & 3) == 3)
        return 3; //fixed
      if ((driveType & 2) == 2)
        return 2; //removable
      if ((driveType & 4) == 4)
        return 4; //remote disk
      if ((driveType & 6) == 6)
        return 6; //ram disk
      return 0;
    }

    public static string GetSize(long dwFileSize)
    {
      if (dwFileSize < 0)
        return "0";
      string szTemp;
      // file < 1 kbyte?
      if (dwFileSize < 1024)
      {
        //  substract the integer part of the float value
        float fRemainder = (dwFileSize / 1024.0f) - (dwFileSize / 1024.0f);
        float fToAdd = 0.0f;
        if (fRemainder < 0.01f)
          fToAdd = 0.1f;
        szTemp = String.Format("{0:f} KB", (dwFileSize / 1024.0f) + fToAdd);
        return szTemp;
      }
      const long iOneMeg = 1024 * 1024;

      // file < 1 megabyte?
      if (dwFileSize < iOneMeg)
      {
        szTemp = String.Format("{0:f} KB", dwFileSize / 1024.0f);
        return szTemp;
      }

      // file < 1 GByte?
      long iOneGigabyte = iOneMeg;
      iOneGigabyte *= 1000;
      if (dwFileSize < iOneGigabyte)
      {
        szTemp = String.Format("{0:f} MB", dwFileSize / ((float)iOneMeg));
        return szTemp;
      }
      //file > 1 GByte
      int iGigs = 0;
      while (dwFileSize >= iOneGigabyte)
      {
        dwFileSize -= iOneGigabyte;
        iGigs++;
      }
      float fMegs = dwFileSize / ((float)iOneMeg);
      fMegs /= 1000.0f;
      fMegs += iGigs;
      szTemp = String.Format("{0:f} GB", fMegs);
      return szTemp;
    }

    public static string MakeFileName(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }
      foreach (char c in Path.GetInvalidFileNameChars())
      {
        strText = strText.Replace(c, '_');
      }
      return strText;
    }

    public static string MakeDirectoryPath(string strText)
    {
      if (string.IsNullOrEmpty(strText))
      {
        return string.Empty;
      }
      foreach (char c in Path.GetInvalidPathChars())
      {
        strText = strText.Replace(c, '_');
      }
      return strText;
    }

    public static string ReplaceTag(string line, string tag, string value, string empty)
    {
      if (line == null)
        return String.Empty;
      if (line.Length == 0)
        return String.Empty;
      if (tag == null)
        return line;
      if (tag.Length == 0)
        return line;

      Regex r = new Regex(String.Format(@"\[[^%]*{0}[^\]]*[\]]", tag));
      if (value == empty)
      {
        Match match = r.Match(line);
        if (match != null && match.Length > 0)
        {
          line = line.Remove(match.Index, match.Length);
        }
      }
      else
      {
        Match match = r.Match(line);
        if (match != null && match.Length > 0)
        {
          line = line.Remove(match.Index, match.Length);
          string m = match.Value.Substring(1, match.Value.Length - 2);
          line = line.Insert(match.Index, m);
        }
      }
      return line.Replace(tag, value);
    }

    public static ulong GetDiskSpace(string drive)
    {
      if (drive.StartsWith(@"\"))
      {
        return GetShareSpace(drive);
      }
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        drive[0] + @":\",
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return totalNumberOfBytes;
    }

    public static ulong GetFreeDiskSpace(string drive)
    {
      if (drive.StartsWith(@"\"))
      {
        return GetFreeShareSpace(drive);
      }
      ulong freeBytesAvailable;
      ulong totalNumberOfBytes;
      ulong totalNumberOfFreeBytes;

      GetDiskFreeSpaceEx(
        drive[0] + @":\",
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }

    public static ulong GetFreeShareSpace(string UNCPath)
    {
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        System.IO.Path.GetPathRoot(UNCPath),
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return freeBytesAvailable;
    }

    public static ulong GetShareSpace(string UNCPath)
    {
      ulong freeBytesAvailable = 0;
      ulong totalNumberOfBytes = 0;
      ulong totalNumberOfFreeBytes = 0;

      GetDiskFreeSpaceEx(
        System.IO.Path.GetPathRoot(UNCPath),
        out freeBytesAvailable,
        out totalNumberOfBytes,
        out totalNumberOfFreeBytes);
      return totalNumberOfBytes;
    }

    public static void UpdateCardStatus(MPListView mpListView1)
    {
      if (!ServiceHelper.IsAvailable)
      {
        return;
      }
      
      try
      {
        IList<CardPresentation> cards = ServiceAgents.Instance.ControllerServiceAgent.ListAllCards();

        int i = 0;

        while (mpListView1.Items.Count > cards.Count)
        {
          mpListView1.Items.RemoveAt(mpListView1.Items.Count - 1);
        }

        IDictionary<int, int> cardIds = new Dictionary<int, int>();

        foreach (CardPresentation card in cards)
        {
          ListViewItem item;
          if (mpListView1.Items.Count < cards.Count)
          {
            item = mpListView1.Items.Add(""); // 0 card id
            item.SubItems.Add("");//1 card type
            item.SubItems.Add("");//2 state
            item.SubItems.Add("");//3 channelname
            item.SubItems.Add("");//4 scrambled
            item.SubItems.Add("");//5 user
            item.SubItems.Add("");//6 cardname
            item.SubItems.Add("");//7 subchannels 
            item.SubItems.Add("");//7 owner
          }
          else
          {            
            item = mpListView1.Items[i];
          }

          string cardId = "n/a";
          if (card.CardId.HasValue)
          {
            cardId = card.CardId.Value.ToString(CultureInfo.InvariantCulture);
          }

          item.SubItems[0].Text = cardId;
          item.SubItems[0].Tag = cardId;
          item.SubItems[1].Text = card.CardType;
          item.SubItems[2].Text = card.State;
          item.SubItems[3].Text = card.ChannelName;
          item.SubItems[4].Text = card.IsScrambled;
          item.SubItems[5].Text = card.UserName;
          item.SubItems[6].Text = card.CardName;
          item.SubItems[7].Text = card.SubChannels.ToString(CultureInfo.InvariantCulture);
          item.SubItems[8].Text = card.IsOwner;

          /*if (!card.Idle)
          {
            int nrOfusers = 0;
            bool hasCardId = cardIds.TryGetValue(card.CardId.GetValueOrDefault(), out nrOfusers);
            if (hasCardId)
            {
              nrOfusers++;
              cardIds[card.CardId.GetValueOrDefault()] = nrOfusers;
            }
            else
            {
              nrOfusers = 1;
              cardIds.Add(card.CardId.GetValueOrDefault(), nrOfusers);
            }            
          } */         

          if (card.SubChannelsCountOk)
          {
            ColorLine(Color.White, item);               
          }
          else
          {
            ColorLine(Color.Red, item);
          }
          i++;
        }
      }
      catch (Exception ex)
      {
        Log.Error(ex);
      }
    }

    private static void ColorLine(Color lineColor, ListViewItem item)
    {
      item.UseItemStyleForSubItems = false;
      item.BackColor = lineColor;

      foreach (ListViewItem.ListViewSubItem lvi in item.SubItems)
      {
        lvi.BackColor = lineColor;
      }
    }
  }
}