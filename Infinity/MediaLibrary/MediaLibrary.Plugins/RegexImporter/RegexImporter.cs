using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MediaLibrary;
using MSjogren.Samples.ShellLink;

namespace RegexImporter
{
    public class RegexImporter : IMLImportPlugin
    {
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
            Prop = Properties.AddNew("stringlistRegularExpressions");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Regular expressions";
                Prop.DataType = "stringlist";
                Prop.HelpText = "Enter one or more regular expressions such as (?<artist>[^\\W\\w]+?)\\(?<album>[\\W\\w]+?)\\(?<track>[\\W\\w]+?)-(?<name>[\\W\\w]+?).([\\W\\w]+?) The \"name\" tag is important because it will be used as the item's name";
                Prop.IsMandatory = true;
            }
            Prop = Properties.AddNew("stringImageFileMasks");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Image file masks";
                Prop.DataType = "string";
                Prop.DefaultValue = "*.jpg,*.jpeg,*.bmp,*.gif,*.png";
                Prop.HelpText = "Enter any number of file masks separated by commas to be used as images, for example : \"folder.jpg,*.bmp\" (without the quotes). If left blank, no images will be found.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringFileMasksToInclude");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "File masks to include";
                Prop.DataType = "string";
                Prop.DefaultValue = "";
                Prop.HelpText = "Enter any number of file masks separated by commas, for example: \"*.avi,*.bmp\" (without the quotes). Leave this blank to include all files.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringFileMasksToExclude");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "File masks to exclude";
                Prop.DataType = "string";
                Prop.DefaultValue = "";
                Prop.HelpText = "Enter any number of file masks separated by commas, for example: \"*.avi,*.bmp\" (without the quotes). Leave this blank to exclude no files.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolIncludeSystemAndHiddenFiles");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Include system and hidden files";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Set to true if you wih to include system and hidden files.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("intExcludeFilesSmallerThanThisValueInKb");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Exclude files smaller than this value (in KB)";
                Prop.DataType = "int";
                Prop.DefaultValue = 0;
                Prop.HelpText = "Enter a KB size to exclude any files smaller than that. Leave as 0 to ignore file size.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolUseSameImageForAllFilesInDirectory");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Use same image for all files in directory";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Do you want to use one image for all files in the same directory? This is generally useful for music albums. If you leave it unchecked, it will look for an image that has the same name as the actual media file\r\nFor example, if left unchecked, for movies it will look to see if there is a image with the same name as the video file that is being added. If one is found it will use that image for that movie.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolGroupChainFilesInEachDirectoryTogether");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Group/chain files in each directory together?";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Do you want files in each directory to be grouped together to play back-to-back? This is generally useful for movies when you have different parts of a movie in the same directory and you want to add them all to the video player to play them all in order back-to-back.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolFollowShortcuts");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Follow shortcuts";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "If checked, the importer will follow shortcuts to files and use the target file to extract tag information.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolImportTheFileDateAsATag");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Import the file date as a tag";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Check this property if you would like to import the file's date as a tag";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolConvertUnderscoresToSpaces");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Convert underscores to spaces";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Check this property if you would like to replace underscores (_s) in the imported data to spaces ( s) (N.b. this will only replace underscores in files whose names contain no spaces)";
                Prop.IsMandatory = false;
            }
            return true;
        }

        public bool SetProperties(IMLHashItem Properties, out String ErrorText)
        {
            ErrorText = String.Empty;
            try
            {
                if (Properties["folderlistRootFolders"] != null)
                {
                    folderlistRootFolders = (String[])Properties["folderlistRootFolders"];
                }
                if (Properties["stringlistRegularExpressions"] != null)
                {
                    stringlistRegularExpressions = (String[])Properties["stringlistRegularExpressions"];
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
                    boolIncludeSystemAndHiddenFiles = (Boolean)Properties["boolIncludeSystemAndHiddenFiles"];
                }
                if (Properties["intExcludeFilesSmallerThanThisValueInKb"] != null)
                {
                    intExcludeFilesSmallerThanThisValueInKb = (Int32)Properties["intExcludeFilesSmallerThanThisValueInKb"];
                }
                if (Properties["boolUseSameImageForAllFilesInDirectory"] != null)
                {
                    boolUseSameImageForAllFilesInDirectory = (Boolean)Properties["boolUseSameImageForAllFilesInDirectory"];
                }
                if (Properties["boolGroupChainFilesInEachDirectoryTogether"] != null)
                {
                    boolGroupChainFilesInEachDirectoryTogether = (Boolean)Properties["boolGroupChainFilesInEachDirectoryTogether"];
                }
                if (Properties["boolFollowShortcuts"] != null)
                {
                    boolFollowShortcuts = (Boolean)Properties["boolFollowShortcuts"];
                }
                if (Properties["boolImportTheFileDateAsATag"] != null)
                {
                    boolImportTheFileDateAsATag = (Boolean)Properties["boolImportTheFileDateAsATag"];
                }
                if (Properties["boolConvertUnderscoresToSpaces"] != null)
                {
                    boolConvertUnderscoresToSpaces = (Boolean)Properties["boolConvertUnderscoresToSpaces"];
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
            if (!(Progress.Progress(0, "Scanning " + folderlistRootFolders.Length + " folder root(s) for " + stringlistRegularExpressions.Length + " regular expression(s).")))
            {
                Progress.Progress(0, "Import cancelled by user.");
                Section.CancelUpdate();
                return false;
            }

            List<FileInfo> fileInfo = new List<FileInfo>();
            try
            {
                fileInfo.AddRange(FindFiles(folderlistRootFolders, stringFileMasksToInclude, stringFileMasksToExclude, intExcludeFilesSmallerThanThisValueInKb, boolIncludeSystemAndHiddenFiles, true, boolFollowShortcuts));
            }
            catch (Exception exception)
            {
                Progress.Progress(100, "An error occurred while polling the specified root folder(s). (" + exception.Message + ")");
                return true;
            }

            if (fileInfo.Count > 0)
            {
                List<RegexGroup> regexGroups = new List<RegexGroup>();
                foreach (String folder in folderlistRootFolders)
                {
                    foreach (String mask in stringlistRegularExpressions)
                    {
                        Regex includeRegex = new Regex("^" + Regex.Escape(folder) + "\\??" + mask + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                        Regex shortcutRegex = new Regex("^([^\\\\]*?\\\\)*" + mask + "$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
                        for (Int32 counter = (fileInfo.Count - 1); counter >= 0; counter--)
                        {
                            if (includeRegex.IsMatch(fileInfo[counter].FullName))
                            {
                                FileInfo tempFileInfo = fileInfo[counter];
                                Boolean shortcut = false;
                                RegexGroup tempRegexGroup = new RegexGroup();
                                if (boolFollowShortcuts)
                                {
                                    if (tempFileInfo.Name.ToLower().EndsWith(".lnk"))
                                    {
                                        try
                                        {
                                            shortcut = true;
                                            ShellShortcut shellShortcut = new ShellShortcut(tempFileInfo.FullName);
                                            tempFileInfo = new FileInfo(shellShortcut.Path);
                                        }
                                        catch
                                        {
                                            tempFileInfo = fileInfo[counter];
                                        }
                                    }
                                }
                                if (shortcut)
                                {
                                    tempRegexGroup.Regex = shortcutRegex;
                                    tempRegexGroup.Groups = shortcutRegex.Match(tempFileInfo.FullName).Groups;
                                }
                                else
                                {
                                    tempRegexGroup.Regex = includeRegex;
                                    tempRegexGroup.Groups = includeRegex.Match(tempFileInfo.FullName).Groups;
                                }
                                tempRegexGroup.FullName = tempFileInfo.FullName;
                                tempRegexGroup.DirectoryName = tempRegexGroup.FullName.Substring(0, tempRegexGroup.FullName.LastIndexOf("\\") + 1);
                                tempRegexGroup.Name = tempRegexGroup.FullName.Substring(tempRegexGroup.DirectoryName.Length);
                                regexGroups.Add(tempRegexGroup);
                                fileInfo.RemoveAt(counter);
                            }
                        }
                    }
                }

                if (regexGroups.Count > 0)
                {
                    regexGroups.Sort(new Comparison<RegexGroup>(CompareRegexGroup));

                    Section.BeginUpdate();

                    Int32 itemsAdded = 0;
                    Int32 itemsUpdated = 0;
                    Int32 itemsSkipped = 0;
                    Int32 itemsFound = regexGroups.Count;
                    Boolean newItem = false;

                    while (regexGroups.Count > 0)
                    {
                        RegexGroup match = regexGroups[0];
                        RegexGroup[] matches = new RegexGroup[] { match };
                        String Location = match.FullName;

                        if (!(Progress.Progress((Int32)((((Double)itemsFound - (Double)regexGroups.Count) / (Double)itemsFound) * (Double)100), Location)))
                        {
                            Progress.Progress(0, "Import cancelled by user.");
                            Section.CancelUpdate();
                            return false;
                        }

                        if (boolGroupChainFilesInEachDirectoryTogether)
                        {
                            try
                            {
                                Int32 itemsProcessed = 0;
                                matches = FindByDirectoryName(regexGroups.ToArray(), match.DirectoryName);
                                Location = String.Empty;
                                foreach (RegexGroup regexGroup in matches)
                                {
                                    if (!(Progress.Progress((Int32)((((Double)itemsFound - ((Double)regexGroups.Count - (Double)itemsProcessed)) / (Double)itemsFound) * (Double)100), regexGroup.FullName)))
                                    {
                                        Progress.Progress(0, "Import cancelled by user.");
                                        Section.CancelUpdate();
                                        return false;
                                    }
                                    Location += ((Location == String.Empty) ? "" : "|") + regexGroup.FullName;
                                    itemsProcessed++;
                                }
                                if (Location.Contains("|"))
                                {
                                    Location = "|" + Location + "|";
                                }
                            }
                            catch
                            {
                                match = regexGroups[0];
                                matches = new RegexGroup[] { match };
                                Location = match.FullName;
                            }
                        }

                        IMLItem item = Section.FindItemByLocation(Location);

                        if (item == null)
                        {
                            item = Section.AddNewItem(String.Empty, Location);
                            newItem = true;
                        }
                        else
                        {
                            newItem = false;
                        }

                        if (newItem || (item.DateChanged < new FileInfo(match.FullName).LastWriteTime))
                        {
                            if (newItem)
                            {
                                itemsAdded++;
                            }
                            else
                            {
                                itemsUpdated++;
                            }
                            for (Int32 counter = 1; counter < match.Groups.Count; counter++)
                            {
                                String tagName = match.Regex.GroupNameFromNumber(counter);
                                String tagValue = match.Groups[counter].Value;
                                if (tagValue != null)
                                {
                                    if (tagName.ToLower() == "name")
                                    {
                                        item.Name = ((boolConvertUnderscoresToSpaces) ? ((tagValue.Contains(" ")) ? tagValue : tagValue.Replace("_", " ")) : tagValue);
                                    }
                                    else
                                    {
                                        item.Tags[tagName] = ((boolConvertUnderscoresToSpaces) ? ((tagValue.Contains(" ")) ? tagValue : tagValue.Replace("_", " ")) : tagValue);
                                    }
                                }
                            }
                            if ((item.ExternalID == null) || (item.ExternalID.Length <= 0))
                            {
                                if ((item.Location != null) && (item.Location.Length > 0))
                                {
                                    item.ExternalID = item.Location.ToLower();
                                }
                                else
                                {
                                    item.ExternalID = Location.ToLower();
                                }
                            }
                            if (boolUseSameImageForAllFilesInDirectory)
                            {
                                FileInfo[] images = FindFiles(new String[] { match.DirectoryName }, stringImageFileMasks, String.Empty, 0, boolIncludeSystemAndHiddenFiles, false, false);
                                if (images.Length > 0)
                                {
                                    item.ImageFile = images[0].FullName;
                                }
                            }
                            else
                            {
                                String imageMask = String.Empty;
                                foreach (String extension in imageExtensions)
                                {
                                    imageMask += ((imageMask == String.Empty) ? "" : ",") + match.Name.Substring(0, match.Name.LastIndexOf(".")).Replace(",", "?") + extension;
                                }
                                FileInfo[] images = FindFiles(new String[] { match.DirectoryName }, imageMask, String.Empty, 0, boolIncludeSystemAndHiddenFiles, false, false);
                                if (images.Length > 0)
                                {
                                    item.ImageFile = images[0].FullName;
                                }
                            }
                            if (boolImportTheFileDateAsATag)
                            {
                                item.Tags["date"] = new FileInfo(match.FullName).LastWriteTime.ToShortDateString();
                            }
                        }
                        else
                        {
                            itemsSkipped++;
                        }
                        item.SaveTags();
                        foreach (RegexGroup regexGroup in matches)
                        {
                            regexGroups.Remove(regexGroup);
                        }
                    }

                    Progress.Progress(100, (itemsFound - regexGroups.Count) + " of " + (itemsFound + fileInfo.Count) + " file(s) processed: " + itemsAdded + " item(s) added, " + itemsUpdated + " item(s) updated, " + itemsSkipped + " item(s) skipped.");

                    Section.EndUpdate();
                }
                else
                {
                    Progress.Progress(100, "No files matched the specified regular expression(s).");
                }
            }
            else
            {
                Progress.Progress(100, "No files in the specified root folder(s) matched the specified include mask(s).");
            }

            return true;
        }

        String[] folderlistRootFolders = new String[] { String.Empty };
        String[] stringlistRegularExpressions = new String[] { String.Empty };
        String stringImageFileMasks = String.Empty;
        String stringFileMasksToInclude = String.Empty;
        String stringFileMasksToExclude = String.Empty;
        Boolean boolIncludeSystemAndHiddenFiles = false;
        Int32 intExcludeFilesSmallerThanThisValueInKb = 0;
        Boolean boolUseSameImageForAllFilesInDirectory = false;
        Boolean boolGroupChainFilesInEachDirectoryTogether = false;
        Boolean boolFollowShortcuts = false;
        Boolean boolImportTheFileDateAsATag = false;
        Boolean boolConvertUnderscoresToSpaces = false;

        String[] imageExtensions = new String[] { ".bmp", ".gif", ".jpeg", ".jpg", ".png", ".tif" };

        private Int32 CompareRegexGroup(RegexGroup val1, RegexGroup val2)
        {
            return val1.Groups[0].Value.CompareTo(val2.Groups[0].Value);
        }

        private RegexGroup[] FindByDirectoryName(RegexGroup[] RegexGroups, String DirectoryName)
        {
            List<RegexGroup> returnvalue = new List<RegexGroup>();
            foreach (RegexGroup regexGroup in RegexGroups)
            {
                if (regexGroup.DirectoryName == DirectoryName)
                {
                    returnvalue.Add(regexGroup);
                }
            }
            return returnvalue.ToArray();
        }

        private struct RegexGroup
        {
            public Regex Regex;
            public GroupCollection Groups;
            public String DirectoryName;
            public String FullName;
            public String Name;
        }

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

        private FileInfo[] RecurseFolders(String folder, Boolean recurse)
        {
            List<FileInfo> returnvalue = new List<FileInfo>();
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
                    return returnvalue.ToArray();
                }
            }
            if (recurse)
            {
                try
                {
                    tempfiles = null;
                    foreach (String directory in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
                    {
                        tempfiles = RecurseFolders(directory, recurse);
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
                        return returnvalue.ToArray();
                    }
                }
            }
            return returnvalue.ToArray();
        }

        private FileInfo[] FindFiles(String[] folders, String includemask, String excludemask, Int32 minimumsize, Boolean includesystemhidden, Boolean recurse, Boolean followshortcuts)
        {
            List<FileInfo> returnvalue = new List<FileInfo>();
            foreach (String folder in folders)
            {
                if (Directory.Exists(folder))
                {
                    foreach (FileInfo fileInfo in RecurseFolders(folder, recurse))
                    {
                        FileInfo file = fileInfo;
                        if (followshortcuts)
                        {
                            if (file.Name.ToLower().EndsWith(".lnk"))
                            {
                                try
                                {
                                    ShellShortcut shellShortcut = new ShellShortcut(file.FullName);
                                    file = new FileInfo(shellShortcut.Path);
                                }
                                catch
                                {
                                    file = fileInfo;
                                }
                            }
                        }
                        Boolean include = false;
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
                        else
                        {
                            include = true;
                        }
                        Boolean exclude = false;
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
                                if (Int32.Parse(file.Length.ToString()) > minimumsize)
                                {
                                    returnvalue.Add(fileInfo);
                                }
                            }
                            else
                            {
                                returnvalue.Add(fileInfo);
                            }
                        }
                    }
                }
            }
            return returnvalue.ToArray();
        }
    }
}
