using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mediaportal.TV.Server.TVLibrary.Interfaces;
using Microsoft.Win32;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer
{
  public static class Common
  {
    public static bool IsFolderValid(string recordingFolder)
    {
      bool hasFolder = !string.IsNullOrEmpty(recordingFolder);
      if (hasFolder)
      {
        try
        {
          Path.GetFullPath(recordingFolder);
        }
        catch (Exception)
        {
          hasFolder = false;
        }
      }
      return hasFolder;
    }

    public static string GetDefaultTimeshiftingFolder()
    {
      return Path.Combine(PathManager.GetDataPath, "timeshiftbuffer");
    }

    public static string GetDefaultRecordingFolder()
    {
      string recPath = ReturnRegistryValue(@"SOFTWARE\Microsoft\Windows\CurrentVersion\MediaCenter\Service\Recording",
                                  "RecordPath");
      if (String.IsNullOrEmpty(recPath))
      {
        recPath = OSInfo.OSInfo.VistaOrLater()
          //Windows Vista and up
                    ? Environment.GetEnvironmentVariable("PUBLIC") + "\\Recorded TV"
          //Windows XP
                    : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) +
                      "\\Shared Documents\\Recorded TV";
      }
      return recPath;
    }

    /// <summary>
    /// Checks whether a given regPath+regValue exist
    /// </summary>
    /// <param name="regPath">The main path to search for (e.g. "SOFTWARE\Microsoft\Windows\CurrentVersion\MediaCenter\Service\Recording")</param>
    /// <param name="regValue">The key to get the value of (e.g. "RecordPath")</param>
    /// <returns>null is not found, otherwise the value converted to a string</returns>
    public static string ReturnRegistryValue(string regPath, string regValue)
    {
      RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath);
      if (key != null)
      {
        string strKeyValue = key.GetValue(regValue).ToString();
        key.Close();
        return strKeyValue;
      }
      return String.Empty;
    }    
  }
}
