using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using MediaLibrary;

namespace PictureTagImporter
{
  public class PictureTagImporter : IMLImportPlugin
  {
    String[] folderlistRootFolders;
    String stringImageFileMasks;
    String stringFileMasksToInclude;
    String stringFileMasksToExclude;
    bool boolIncludeSystemAndHiddenFiles;
    int intExcludeFilesSmallerThan;


    public bool GetProperties(IMLPluginProperties Properties)
    {
      IMLPluginProperty Prop = Properties.AddNew("folderlistRootFolders");
      {
        Prop.CanTypeChoices = false;
        Prop.Caption = "Root folders";
        Prop.DataType = "folderlist";
        Prop.HelpText = "Select any number of root folders from which to import files.";
        Prop.IsMandatory = true;
      }
      Prop = Properties.AddNew("stringFileMasksToInclude");
      {
        Prop.CanTypeChoices = true;
        Prop.Caption = "File masks to include";
        Prop.DataType = "string";
        Prop.DefaultValue = "*.jpg,*.jpeg,*.bmp,*.gif,*.png";
        Prop.HelpText = "Enter any number of file masks separated by commas, for example: \"*.avi,*.bmp\" (without the quotes). Leave this blank to include all files.";
        Prop.IsMandatory = false;
      }
      Prop = Properties.AddNew("stringFileMasksToExclude");
      {
        Prop.CanTypeChoices = true;
        Prop.Caption = "File masks to exclude";
        Prop.DataType = "string";
        Prop.DefaultValue = "folder.*";
        Prop.HelpText = "Enter any number of file masks separated by commas, for example: \"*.avi,*.bmp\" (without the quotes). Leave this blank to exclude no files.";
        Prop.IsMandatory = false;
      }
      Prop = Properties.AddNew("boolIncludeSystemAndHiddenFiles");
      {
        Prop.CanTypeChoices = false;
        Prop.Caption = "Include system and hidden files";
        Prop.DataType = "bool";
        Prop.DefaultValue = true;
        Prop.HelpText = "Set to true if you wish to include system and hidden files.";
        Prop.IsMandatory = false;
      }
      Prop = Properties.AddNew("intExcludeFilesSmallerThan");
      {
        Prop.CanTypeChoices = true;
        Prop.Caption = "Exclude files smaller than this value (in KB)";
        Prop.DataType = "int";
        Prop.DefaultValue = 0;
        Prop.HelpText = "Enter a KB size to exclude any files smaller than that. Leave as zero to ignore file size.\r\n\r\nWARNING: Enabling this option will negatively impact performance.";
        Prop.IsMandatory = false;
      }
      return true;
    }
    public bool SetProperties(IMLHashItem Properties, out string ErrorText)
    {
      ErrorText = "";
                  ErrorText = "";
                  try
                  {
                    if (Properties["folderlistRootFolders"] != null)
                    {
                      folderlistRootFolders = (String[])Properties["folderlistRootFolders"];
                    }
                    if (Properties["stringImageFileMasks"] != null)
                    {
                      stringImageFileMasks = (String)Properties["stringImageFileMasks"];
                    }
                    if (Properties["stringFileMasksToInclude"] != null)
                    {
                      stringFileMasksToInclude = (String)Properties["stringFileMasksToInclude"];
                    }
                    if (Properties["stringFileMasksToExclude"] != null)
                    {
                      stringFileMasksToExclude = (String)Properties["stringFileMasksToExclude"];
                    }
                    if (Properties["boolIncludeSystemAndHiddenFiles"] != null)
                    {
                      boolIncludeSystemAndHiddenFiles = (bool)Properties["boolIncludeSystemAndHiddenFiles"];
                    }
                    if (Properties["intExcludeFilesSmallerThan"] != null)
                    {
                      intExcludeFilesSmallerThan = (int)Properties["intExcludeFilesSmallerThan"];
                    }
                  }
                  catch (Exception exception)
                  {
                    ErrorText = exception.Message;
                    return false;
                  }
                  return true;
    }

    public bool ValidateProperties(IMLPluginProperties Properties, IMLHashItem PropertyValues)
    {
      return true;
    }

    public bool EditCustomProperty(IntPtr Window, string PropertyName, ref string Value)
    {
      return true;
    }

    public bool Import(IMLSection Section, IMLImportProgress Progress)
    {
      Section.BeginUpdate();

      int added = 0;
      int updated = 0;
      int skipped = 0;
      int deleted = 0;

      if (!(Progress.Progress(0, "Building file list...")))
      {
        Section.CancelUpdate();
        Progress.Progress(0, "Import cancelled.");
        return false;
      }

      FileInfo[] globalFileList = FindFiles(folderlistRootFolders, stringFileMasksToInclude, stringFileMasksToExclude, intExcludeFilesSmallerThan, boolIncludeSystemAndHiddenFiles);

      foreach (FileInfo file in globalFileList)
      {
        // Import tag information
        if (!(Progress.Progress((int)(((float)(added + updated + skipped) / (float)globalFileList.Length) * (float)100), file.Name)))
        {
          Section.CancelUpdate();
          Progress.Progress(0, "Import cancelled.");
          return false;
        }
        
        bool boolAdded = false;
        IMLItem item = Section.FindItemByLocation(file.FullName);
        if (item == null)
        {
          item = Section.AddNewItem(file.Name.Substring(0, file.Name.LastIndexOf(".")), file.FullName);
          added++;
          boolAdded = true;
        }

        item.SaveTags();
      }

      Section.EndUpdate();
      return true;
    }
    #region getingfilelisthelper
    private Regex WildStringToRegex(String WildString)
    {
      String tempString = WildString;
      if (tempString.Contains(":"))
      {
        tempString = tempString.Substring(tempString.LastIndexOf(":") + 1);
      }
      if (tempString.Contains("\\"))
      {
        tempString = tempString.Substring(tempString.LastIndexOf("\\") + 1);
      }
      if (tempString.Contains("/"))
      {
        tempString = tempString.Substring(tempString.LastIndexOf("/") + 1);
      }
      tempString = tempString.Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "");
      tempString = "^" + Regex.Escape(tempString).Replace("\\*", ".*").Replace("\\?", ".") + "$";
      return new Regex(tempString, RegexOptions.IgnoreCase);
    }

    private FileInfo[] RecurseFolders(String folder)
    {
      ArrayList returnvalue = new ArrayList();
      FileInfo[] tempfiles = null;
      try
      {
        tempfiles = new DirectoryInfo(folder).GetFiles("*", SearchOption.TopDirectoryOnly);
        if (tempfiles != null)
        {
          returnvalue.AddRange(tempfiles);
        }
      }
      catch
      {
        if (returnvalue.Count == 0)
        {
          return null;
        }
        else
        {
          return (FileInfo[])returnvalue.ToArray(typeof(FileInfo));
        }
      }
      try
      {
        tempfiles = null;
        foreach (String directory in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
        {
          tempfiles = RecurseFolders(directory);
          if (tempfiles != null)
          {
            returnvalue.AddRange(tempfiles);
          }
        }
      }
      catch
      {
        if (returnvalue.Count == 0)
        {
          return null;
        }
        else
        {
          return (FileInfo[])returnvalue.ToArray(typeof(FileInfo));
        }
      }
      return (FileInfo[])returnvalue.ToArray(typeof(FileInfo));
    }

    private FileInfo[] FindFiles(String[] folders, String includemask, String excludemask, long minimumsize, bool includesystemhidden)
    {
      ArrayList returnvalue = new ArrayList();
      foreach (String folder in folders)
      {
        foreach (FileInfo file in RecurseFolders(folder))
        {
          bool include = false;
          if (includemask.Length > 0)
          {
            foreach (String mask in includemask.Split(new char[] { ',' }))
            {
              if (WildStringToRegex(mask).IsMatch(file.Name))
              {
                include = true;
              }
            }
          }
          bool exclude = false;
          if (excludemask.Length > 0)
          {
            foreach (String mask in excludemask.Split(new char[] { ',' }))
            {
              if (WildStringToRegex(mask).IsMatch(file.Name))
              {
                exclude = true;
              }
            }
          }
          if (!includesystemhidden)
          {
            if (include && !exclude)
            {
              if (((file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden) || ((file.Attributes & FileAttributes.System) == FileAttributes.System))
              {
                exclude = true;
              }
            }
          }
          if (include && !exclude)
          {
            if (minimumsize != 0)
            {
              if (file.Length > minimumsize)
              {
                returnvalue.Add(file);
              }
            }
            else
            {
              returnvalue.Add(file);
            }
          }
        }
      }
      return (FileInfo[])returnvalue.ToArray(typeof(FileInfo));
    }
    #endregion
  }
}
