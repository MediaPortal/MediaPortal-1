using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using MediaLibrary;
using TagLib;


namespace AudioTagImporter
{
    public class AudioTagImporter : IMLImportPlugin
    {
        String[] folderlistRootFolders;
        String stringImageFileMasks;
        String stringFileMasksToInclude;
        String stringFileMasksToExclude;
        bool boolIncludeSystemAndHiddenFiles;
        int intExcludeFilesSmallerThan;
        String choiceImageFinding;
        String stringPreferredImageName;
        bool boolLookForEmbeddedCoverArt;
        bool boolLyrics3v200;
        String stringRemoveText;
        bool boolRemoveArtistPrefixes;
        bool boolRemoveAlbumPrefixes;
        bool boolNormalizeArtistNames;
        bool boolNormalizeAlbumNames;
        bool boolNormalizeGenreNames;
        bool boolFixRatings;
        String[] stringlistRating1Values;
        String[] stringlistRating2Values;
        String[] stringlistRating3Values;
        String[] stringlistRating4Values;
        String[] stringlistRating5Values;
        bool boolTitle;
        bool boolArtist;
        bool boolAlbum;
        bool boolTrack;
        bool boolYear;
        bool boolGenre;
        bool boolComment;
        bool boolComposer;
        bool boolOriginalArtist;
        bool boolCopyright;
        bool boolEncoded;
        bool boolAdditionalTags;
        bool boolDebugLog = false;

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
            Prop = Properties.AddNew("stringImageFileMasks");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Image file masks";
                Prop.DataType = "string";
                Prop.DefaultValue = "*.jpg,*.jpeg,*.bmp,*.gif,*.png";
                Prop.HelpText = "Enter any number of file masks separated by commas to be used as images, for example: \"folder.jpg,*.bmp\" (without the quotes). If left blank, no images will be found.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringFileMasksToInclude");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "File masks to include";
                Prop.DataType = "string";
                Prop.DefaultValue = "*.wma,*.mp3,*.mp2,*.wav,*.ape,*.ogg,*.flac,*.mpc";
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
            Prop = Properties.AddNew("choiceImageFinding");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Image finding";
                Prop.Choices = new String[] { "best", "first" };
                Prop.DataType = "string";
                Prop.DefaultValue = "best";
                Prop.HelpText = "Choosing best will use the largest image available but will be slower, choosing first will use the first image found in the folder.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringPreferredImageName");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Preferred image name";
                Prop.DataType = "string";
                Prop.DefaultValue = "";
                Prop.HelpText = "Type the name of the preferred image file here (e.g. front_cover.jpg) If that file exists, Audio Tag Importer will give it precedence over any other image in the folder. Leave this option blank to allow Audio Tag Importer to decide which image to use based on the Image finding setting.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolLookForEmbeddedCoverArt");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Look for embedded cover art";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Uncheck this option if you would like to skip detection of embedded cover art in MP3's and WMA's";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolLyrics3v200");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Look for lyrics in Lyrics3 v2.00 format";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.GroupCaption = "Lyrics3 v2.00";
                Prop.HelpText = "If checked, the import will include lyrics in Lyrics3 v2.00 format.\r\n\r\nWARNING: Enabling this option will negatively impact performance.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringRemoveText");
            {
                Prop.CanTypeChoices = true;
                Prop.Caption = "Remove text";
                Prop.DataType = "string";
                Prop.DefaultValue = "";
                Prop.HelpText = "Enter the text you would like to remove from imported lyrics. Use \\n to seperate lines.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolRemoveArtistPrefixes");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Remove prefixes from Artist name";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.GroupCaption = "Artist/Album names";
                Prop.HelpText = "If checked the Artist name will be in the format \"Doors, The\"";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolRemoveAlbumPrefixes");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Remove prefixes from Album name";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "If checked the Album name will be in the format \"Battle of Los Angeles, The\"";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolNormalizeArtistNames");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Normalize Artist names";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "If checked the Artist names will be verified for internal consistency (e.g. \"prodigy\", \"Prodigy\", and \"The Prodigy\" will all be renamed to \"The Prodigy\")\r\n\r\nWARNING: Enabling this option will negatively impact performance.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolNormalizeAlbumNames");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Normalize Album names";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "If checked the Album names will be verified for internal consistency (e.g. \"fat of the land\", \"Fat of the Land\", and \"The Fat of the Land\" will all be renamed to \"The Fat of the Land\")\r\n\r\nWARNING: Enabling this option will negatively impact performance.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolNormalizeGenreNames");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Normalize Genre names";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "If checked the Genre names will be verified for internal consistency (e.g. \"house\" and \"House\" will all be renamed to \"House\")\r\n\r\nWARNING: Enabling this option will negatively impact performance.";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolFixRatings");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Fix ratings";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.GroupCaption = "Music ratings";
                Prop.HelpText = "Check this option if you would like to unify the ratings from all your music files and map them to the values you enter below";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringlistRating1Values");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Rating 1 values";
                Prop.DataType = "stringlist";
                Prop.DefaultValue = "1,Poor,*";
                Prop.HelpText = "Rate 1 all files with their Rating tag set to this values";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringlistRating2Values");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Rating 2 values";
                Prop.DataType = "stringlist";
                Prop.DefaultValue = "2,Fair,**";
                Prop.HelpText = "Rate 2 all files with their Rating tag set to this values";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringlistRating3Values");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Rating 3 values";
                Prop.DataType = "stringlist";
                Prop.DefaultValue = "3,Good,***";
                Prop.HelpText = "Rate 3 all files with their Rating tag set to this values";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringlistRating4Values");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Rating 4 values";
                Prop.DataType = "stringlist";
                Prop.DefaultValue = "4,Very Good,****";
                Prop.HelpText = "Rate 4 all files with their Rating tag set to this values";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("stringlistRating5Values");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Rating 5 values";
                Prop.DataType = "stringlist";
                Prop.DefaultValue = "5,Excellent,*****";
                Prop.HelpText = "Rate 5 all files with their Rating tag set to this values";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolTitle");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Title";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.GroupCaption = "Tag Settings";
                Prop.HelpText = "Import the title tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolArtist");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Artist";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the artist tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolAlbum");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Album";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the album tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolTrack");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Track";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the track tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolYear");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Year";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the year tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolGenre");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Genre";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the genre tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolComment");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Comment";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the comment tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolComposer");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Composer";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the composer tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolOriginalArtist");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Original Artist";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the original artist tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolCopyright");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Copyright";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the copyright tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolEncoded");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Encoded";
                Prop.DataType = "bool";
                Prop.DefaultValue = true;
                Prop.HelpText = "Import the encoded tag?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolAdditionalTags");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "User Defined Tags";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.HelpText = "Import user-defined tags?";
                Prop.IsMandatory = false;
            }
            Prop = Properties.AddNew("boolDegubLog");
            {
                Prop.CanTypeChoices = false;
                Prop.Caption = "Write debug log";
                Prop.DataType = "bool";
                Prop.DefaultValue = false;
                Prop.GroupCaption = "Debug";
                Prop.HelpText = "Create a log file of caught exceptions.";
                Prop.IsMandatory = false;
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

        public bool SetProperties(IMLHashItem Properties, out String ErrorText)
        {
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
                if (Properties["choiceImageFinding"] != null)
                {
                    choiceImageFinding = (String)Properties["choiceImageFinding"];
                }
                if (Properties["stringPreferredImageName"] != null)
                {
                    stringPreferredImageName = (String)Properties["stringPreferredImageName"];
                }
                if (Properties["boolLookForEmbeddedCoverArt"] != null)
                {
                    boolLookForEmbeddedCoverArt = (bool)Properties["boolLookForEmbeddedCoverArt"];
                }
                if (Properties["boolLyrics3v200"] != null)
                {
                    boolLyrics3v200 = (bool)Properties["boolLyrics3v200"];
                }
                if (Properties["stringRemoveText"] != null)
                {
                    stringRemoveText = (String)Properties["stringRemoveText"];
                }
                if (Properties["boolRemoveArtistPrefixes"] != null)
                {
                    boolRemoveArtistPrefixes = (bool)Properties["boolRemoveArtistPrefixes"];
                }
                if (Properties["boolRemoveAlbumPrefixes"] != null)
                {
                    boolRemoveAlbumPrefixes = (bool)Properties["boolRemoveAlbumPrefixes"];
                }
                if (Properties["boolNormalizeArtistNames"] != null)
                {
                    boolNormalizeArtistNames = (bool)Properties["boolNormalizeArtistNames"];
                }
                if (Properties["boolNormalizeAlbumNames"] != null)
                {
                    boolNormalizeAlbumNames = (bool)Properties["boolNormalizeAlbumNames"];
                }
                if (Properties["boolNormalizeGenreNames"] != null)
                {
                    boolNormalizeGenreNames = (bool)Properties["boolNormalizeGenreNames"];
                }
                if (Properties["boolFixRatings"] != null)
                {
                    boolFixRatings = (bool)Properties["boolFixRatings"];
                }
                if (Properties["stringlistRating1Values"] != null)
                {
                    stringlistRating1Values = (String[])Properties["stringlistRating1Values"];
                }
                if (Properties["stringlistRating2Values"] != null)
                {
                    stringlistRating2Values = (String[])Properties["stringlistRating2Values"];
                }
                if (Properties["stringlistRating3Values"] != null)
                {
                    stringlistRating3Values = (String[])Properties["stringlistRating3Values"];
                }
                if (Properties["stringlistRating4Values"] != null)
                {
                    stringlistRating4Values = (String[])Properties["stringlistRating4Values"];
                }
                if (Properties["stringlistRating5Values"] != null)
                {
                    stringlistRating5Values = (String[])Properties["stringlistRating5Values"];
                }
                if (Properties["boolTitle"] != null)
                {
                    boolTitle = (bool)Properties["boolTitle"];
                }
                if (Properties["boolArtist"] != null)
                {
                    boolArtist = (bool)Properties["boolArtist"];
                }
                if (Properties["boolAlbum"] != null)
                {
                    boolAlbum = (bool)Properties["boolAlbum"];
                }
                if (Properties["boolTrack"] != null)
                {
                    boolTrack = (bool)Properties["boolTrack"];
                }
                if (Properties["boolYear"] != null)
                {
                    boolYear = (bool)Properties["boolYear"];
                }
                if (Properties["boolGenre"] != null)
                {
                    boolGenre = (bool)Properties["boolGenre"];
                }
                if (Properties["boolComment"] != null)
                {
                    boolComment = (bool)Properties["boolComment"];
                }
                if (Properties["boolComposer"] != null)
                {
                    boolComposer = (bool)Properties["boolComposer"];
                }
                if (Properties["boolOriginalArtist"] != null)
                {
                    boolOriginalArtist = (bool)Properties["boolOriginalArtist"];
                }
                if (Properties["boolCopyright"] != null)
                {
                    boolCopyright = (bool)Properties["boolCopyright"];
                }
                if (Properties["boolEncoded"] != null)
                {
                    boolEncoded = (bool)Properties["boolEncoded"];
                }
                if (Properties["boolAdditionalTags"] != null)
                {
                    boolAdditionalTags = (bool)Properties["boolAdditionalTags"];
                }
                if (Properties["boolDegubLog"] != null)
                {
                    boolDebugLog = (bool)Properties["boolDegubLog"];
                }
            }
            catch (Exception exception)
            {
                ErrorText = exception.Message;
                return false;
            }
            return true;
        }

        public bool Import(IMLSection Section, IMLImportProgress Progress)
        {
            DateTime starttime = DateTime.Now;

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

                long LastUpdateTag = 0;
                long LastUpdateFile = file.LastWriteTime.Ticks;

                try
                {
                    LastUpdateTag = Convert.ToInt64(item.Tags.Get("LastUpdate", "0"));
                }
                catch (Exception exception)
                {
                    WriteDebug("Convert.ToInt64(item.Tags.Get(\"LastUpdate\", \"0\")) : ", exception.Message, " : " + item.Tags.Get("LastUpdate", "0").ToString());
                    if (!(Progress.Progress(0, exception.Message)))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }
                }

                TagLib.File tagLib = null;
                if (LastUpdateTag < LastUpdateFile)
                {
                    try
                    {
                        tagLib = TagLib.File.Create(file.FullName);
                    }
                    catch (Exception exception)
                    {
                        WriteDebug("TagLib.File.Create(file.FullName) : ", exception.Message, " : " + file.FullName);
                        if (!(Progress.Progress(0, exception.Message)))
                        {
                            Section.CancelUpdate();
                            Progress.Progress(0, "Import cancelled.");
                            return false;
                        }
                    }
                    if (!boolAdded)
                    {
                        updated++;
                    }
                }
                else
                {
                    skipped++;
                }

                if (tagLib != null)
                {
                    if (boolTitle)
                    {
                        try
                        {
                            if (tagLib.Tag.Title != null)
                            {
                                if (tagLib.Tag.Title.Trim().Length > 0)
                                {
                                    item.Name = tagLib.Tag.Title.Trim();
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Title.Trim() : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolAlbum)
                    {
                        try
                        {
                            if (tagLib.Tag.Album != null)
                            {
                                item.Tags["Album"] = tagLib.Tag.Album;
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Album : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolArtist)
                    {
                        try
                        {
                            if (tagLib.Tag.AlbumArtists != null)
                            {
                                String tempArtist = String.Empty;
                                foreach (String artist in tagLib.Tag.AlbumArtists)
                                {
                                    tempArtist += " & " + artist;
                                }
                                item.Tags["Artist"] = tempArtist.Trim(new char[] { ' ', '&' });
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.AlbumAtrists : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolComment)
                    {
                        try
                        {
                            if (tagLib.Tag.Comment != null)
                            {
                                item.Tags["Comment"] = tagLib.Tag.Comment.Trim();
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Comment : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolComposer)
                    {
                        try
                        {
                            if (tagLib.Tag.Composers != null)
                            {
                                String tempComposer = String.Empty;
                                foreach (String composer in tagLib.Tag.Composers)
                                {
                                    tempComposer += " & " + composer;
                                }
                                item.Tags["Composer"] = tempComposer.Trim(new char[] { ' ', '&' });
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Composer : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolGenre)
                    {
                        try
                        {
                            if (tagLib.Tag.Genres != null)
                            {
                                ArrayList tempGenres = new ArrayList();
                                foreach (String genre in tagLib.Tag.Genres)
                                {
                                    if (!(tempGenres.Contains(genre)))
                                    {
                                        tempGenres.Add(genre);
                                    }
                                }
                                String tempGenre = String.Empty;
                                foreach (Object obj in tempGenres)
                                {
                                    tempGenre += "|" + obj.ToString();
                                }
                                if (tempGenre.Trim(new char[] { ' ', '|' }).Length > 0)
                                {
                                    if (tempGenre.Trim(new char[] { ' ', '|' }).Contains("|"))
                                    {
                                        item.Tags["Genre"] = "|" + tempGenre.Trim(new char[] { ' ', '|' }) + "|";
                                    }
                                    else
                                    {
                                        item.Tags["Genre"] = tempGenre.Trim(new char[] { ' ', '|' });
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Genre : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolTrack)
                    {
                        try
                        {
                            if (tagLib.Tag.Track != (uint)0)
                            {
                                item.Tags["Track"] = tagLib.Tag.Track;
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Track : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                    if (boolYear)
                    {
                        try
                        {
                            if (tagLib.Tag.Year != (uint)0)
                            {
                                item.Tags["Year"] = tagLib.Tag.Year;
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("tagLib.Tag.Year : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }

                    TagLib.Id3v2.Tag id3v2 = null;
                    try
                    {
                        id3v2 = (TagLib.Id3v2.Tag)tagLib.GetTag(TagTypes.Id3v2);
                    }
                    catch (Exception exception)
                    {
                        WriteDebug("tagLib.GetTag(TagTypes.Id3v2) : ", exception.Message, "");
                        if (!(Progress.Progress(0, exception.Message)))
                        {
                            Section.CancelUpdate();
                            Progress.Progress(0, "Import cancelled.");
                            return false;
                        }
                    }

                    if (id3v2 != null)
                    {
                        if (boolCopyright)
                        {
                            try
                            {
                                String tempCopyright = String.Empty;
                                foreach (TagLib.Id3v2.TextIdentificationFrame frame in id3v2.GetFrames((ByteVector)"TCOP"))
                                {
                                    tempCopyright += ", " + frame.FieldList.ToString().Trim();
                                }
                                if (tempCopyright.Trim(new char[] { ' ', ',' }).Length > 0)
                                {
                                    item.Tags["Copyright"] = tempCopyright.Trim(new char[] { ' ', ',' });
                                }
                            }
                            catch (Exception exception)
                            {
                                WriteDebug("id3v2.GetFrames(TCOP) : ", exception.Message, "");
                                if (!(Progress.Progress(0, exception.Message)))
                                {
                                    Section.CancelUpdate();
                                    Progress.Progress(0, "Import cancelled.");
                                    return false;
                                }
                            }
                        }
                        if (boolEncoded)
                        {
                            try
                            {
                                String tempEncoded = String.Empty;
                                foreach (TagLib.Id3v2.TextIdentificationFrame frame in id3v2.GetFrames((ByteVector)"TENC"))
                                {
                                    tempEncoded += ", " + frame.FieldList.ToString();
                                }
                                if (tempEncoded.Trim(new char[] { ' ', ',' }).Length > 0)
                                {
                                    item.Tags["Encoded"] = tempEncoded.Trim(new char[] { ' ', ',' });
                                }
                            }
                            catch (Exception exception)
                            {
                                WriteDebug("id3v2.GetFrames(TENC) : ", exception.Message, "");
                                if (!(Progress.Progress(0, exception.Message)))
                                {
                                    Section.CancelUpdate();
                                    Progress.Progress(0, "Import cancelled.");
                                    return false;
                                }
                            }
                        }
                        if (boolOriginalArtist)
                        {
                            try
                            {
                                String tempOriginalArtist = String.Empty;
                                foreach (TagLib.Id3v2.TextIdentificationFrame frame in id3v2.GetFrames((ByteVector)"TOPE"))
                                {
                                    tempOriginalArtist += ", " + frame.FieldList.ToString();
                                }
                                if (tempOriginalArtist.Trim(new char[] { ' ', ',' }).Length > 0)
                                {
                                    item.Tags["OriginalArtist"] = tempOriginalArtist.Trim(new char[] { ' ', ',' });
                                }
                            }
                            catch (Exception exception)
                            {
                                WriteDebug("id3v2.GetFrames(TOPE) : ", exception.Message, "");
                                if (!(Progress.Progress(0, exception.Message)))
                                {
                                    Section.CancelUpdate();
                                    Progress.Progress(0, "Import cancelled.");
                                    return false;
                                }
                            }
                        }

                        // Import user-defined ID3v2 tags
                        if (boolAdditionalTags)
                        {
                            foreach (TagLib.Id3v2.Frame frame in id3v2.GetFrames((ByteVector)"TXXX"))
                            {
                                String tempTagName = String.Empty;
                                String tempTagText = String.Empty;
                                if (frame.GetType().ToString() == "TagLib.Id3v2.UserTextIdentificationFrame")
                                {
                                    tempTagName = ((TagLib.Id3v2.UserTextIdentificationFrame)frame).Description;
                                    tempTagText = ((TagLib.Id3v2.UserTextIdentificationFrame)frame).FieldList.ToString();
                                }
                                if ((tempTagName != String.Empty) && (tempTagText != String.Empty))
                                {
                                    item.Tags[tempTagName] = tempTagText;
                                    item.SaveTags();
                                }
                            }
                        }

                        if (boolLyrics3v200)
                        {
                            try
                            {
                                TagLib.Id3v2.Frame[] USLT = null;

                                try
                                {
                                    USLT = id3v2.GetFrames((ByteVector)"USLT");
                                }
                                catch (Exception exception)
                                {
                                    WriteDebug("id3v2.GetFrames(USLT) : ", exception.Message, "");
                                }

                                if (USLT.Length > 0)
                                {
                                    if (((TagLib.Id3v2.UnknownFrame)USLT[0]).Data.Count != 0)
                                    {
                                        String tempLyrics = String.Empty;
                                        for (int counter = 0; counter < ((TagLib.Id3v2.UnknownFrame)USLT[0]).Data.Count; counter++)
                                        {
                                            int character = Convert.ToInt32(((TagLib.Id3v2.UnknownFrame)USLT[0]).Data[counter]);
                                            if (((character >= 32) && (character <= 126)) || (character == 9) || (character == 10) || (character == 13))
                                            {
                                                tempLyrics += Convert.ToChar(character).ToString();
                                            }
                                        }
                                        item.Tags["Lyrics"] = tempLyrics.Substring(3);
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                if (!(Progress.Progress(0, exception.Message)))
                                {
                                    WriteDebug("item.Tags[\"Lyrics\"] : ", exception.Message, "");
                                    Section.CancelUpdate();
                                    Progress.Progress(0, "Import cancelled.");
                                    return false;
                                }
                            }
                        }
                    }

                    // Image detection
                    if (boolLookForEmbeddedCoverArt)
                    {
                        if (tagLib.Tag.Pictures != null)
                        {
                            try
                            {
                                id3v2 = (TagLib.Id3v2.Tag)tagLib.GetTag(TagTypes.Id3v2);
                                TagLib.Mpeg4.AppleTag apple = (TagLib.Mpeg4.AppleTag)tagLib.GetTag(TagTypes.Apple);
                                bool apicImage = false;
                                bool covrImage = false;
                                if (id3v2 != null)
                                {
                                    foreach (TagLib.Id3v2.AttachedPictureFrame frame in id3v2.GetFrames("APIC"))
                                    {
                                        apicImage = true;
                                    }
                                }
                                if (apple != null)
                                {
                                    foreach (TagLib.Mpeg4.AppleDataBox box in apple.DataBoxes("covr"))
                                    {
                                        covrImage = true;
                                    }
                                }

                                if (apicImage)
                                {
                                    item.ImageFile = "apic:" + file.FullName;
                                }
                                if (covrImage)
                                {
                                    item.ImageFile = "covr:" + file.FullName;
                                }
                            }
                            catch (Exception exception)
                            {
                                WriteDebug("id3v2.GetFrames(APIC) / apple.DataBoxes(covr) : ", exception.Message, "");
                                if (!(Progress.Progress(0, exception.Message)))
                                {
                                    Section.CancelUpdate();
                                    Progress.Progress(0, "Import cancelled.");
                                    return false;
                                }
                            }
                        }

                        // Import corrected rating information
                        if (boolFixRatings)
                        {
                            try
                            {
                                String tempRating = item.Tags.Get("Rating", String.Empty).ToString();

                                foreach (String stringRatingValue in stringlistRating1Values)
                                {
                                    if (tempRating.Equals(stringRatingValue))
                                    {
                                        tempRating = "1";
                                    }
                                }
                                foreach (String stringRatingValue in stringlistRating2Values)
                                {
                                    if (tempRating.Equals(stringRatingValue))
                                    {
                                        tempRating = "2";
                                    }
                                }
                                foreach (String stringRatingValue in stringlistRating3Values)
                                {
                                    if (tempRating.Equals(stringRatingValue))
                                    {
                                        tempRating = "3";
                                    }
                                }
                                foreach (String stringRatingValue in stringlistRating4Values)
                                {
                                    if (tempRating.Equals(stringRatingValue))
                                    {
                                        tempRating = "4";
                                    }
                                }
                                foreach (String stringRatingValue in stringlistRating5Values)
                                {
                                    if (tempRating.Equals(stringRatingValue))
                                    {
                                        tempRating = "5";
                                    }
                                }

                                item.Tags["Rating"] = tempRating;
                                item.SaveTags();
                            }
                            catch (Exception exception)
                            {
                                WriteDebug("Rating : ", exception.Message, "");
                                if (!(Progress.Progress(0, exception.Message)))
                                {
                                    Section.CancelUpdate();
                                    Progress.Progress(0, "Import cancelled.");
                                    return false;
                                }
                            }
                        }
                    }

                    bool missingImage = false;
                    if (item.ImageFile == null)
                    {
                        missingImage = true;
                    }
                    else
                    {
                        if (item.ImageFile.Trim().Length <= 0)
                        {
                            missingImage = true;
                        }
                    }
                    if (missingImage)
                    {
                        try
                        {
                            ArrayList imageList = new ArrayList();
                            bool foundPreferred = false;
                            foreach (String imageType in stringImageFileMasks.Split(new char[] { ',' }))
                            {
                                foreach (FileInfo fileInfo in file.Directory.GetFiles(imageType, SearchOption.TopDirectoryOnly))
                                {
                                    if (WildStringToRegex(stringPreferredImageName).IsMatch(fileInfo.Name))
                                    {
                                        foundPreferred = true;
                                    }
                                    imageList.Add(fileInfo.FullName);
                                }
                            }
                            if (foundPreferred)
                            {
                                item.ImageFile = file.DirectoryName + "\\" + stringPreferredImageName;
                            }
                            else if (choiceImageFinding == "best")
                            {
                                int tempArea = 0;
                                String tempFilename = String.Empty;
                                foreach (Object fileName in imageList)
                                {
                                    FileInfo fileInfo = new FileInfo(fileName.ToString());
                                    Bitmap tempBitmap = new Bitmap(fileInfo.FullName);
                                    if ((tempBitmap.Height * tempBitmap.Width) > tempArea)
                                    {
                                        tempArea = (tempBitmap.Height * tempBitmap.Width);
                                        tempFilename = fileInfo.FullName;
                                    }
                                }
                                if (tempFilename != string.Empty)
                                {
                                    item.ImageFile = tempFilename;
                                }
                            }
                            else
                            {
                                if (imageList.Count > 0)
                                {
                                    item.ImageFile = imageList[0].ToString();
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            WriteDebug("FindImage : ", exception.Message, "");
                            if (!(Progress.Progress(0, exception.Message)))
                            {
                                Section.CancelUpdate();
                                Progress.Progress(0, "Import cancelled.");
                                return false;
                            }
                        }
                    }
                }

                item.Tags["LastUpdate"] = DateTime.Now.Ticks;
                item.SaveTags();
            }

            IMLItemList items = Section.GetReadOnlyItems();

            if (!(Progress.Progress(0, "Running cleanup operations...")))
            {
                Section.CancelUpdate();
                Progress.Progress(0, "Import cancelled.");
                return false;
            }

            if ((boolNormalizeArtistNames) && ((added + updated) > 0))
            {
                // Normalize artist names (e.g. "The Prodigy", "Prodigy", and "prodigy" all become "The Prodigy")

                ArrayList masterList = new ArrayList();
                ArrayList normalizedMasterList = new ArrayList();

                // Compile all artist and original artist names
                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Compiling artist name lists...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    String artist = item.Tags.Get("Artist", String.Empty).ToString();
                    String originalArtist = item.Tags.Get("OriginalArtist", String.Empty).ToString();

                    if (!(masterList.Contains(artist)))
                    {
                        masterList.Add(artist);
                    }
                    if (!(masterList.Contains(originalArtist)))
                    {
                        masterList.Add(originalArtist);
                    }
                }

                // Sort artist name arrays alphabetically
                masterList.Sort();

                // Normalize artist name arrays
                if (!(Progress.Progress(0, "Normalizing artist names...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                foreach (Object obj in masterList)
                {
                    normalizedMasterList.Add(NormalizeArtist(obj.ToString()));
                }

                // Find and record correct artist names based on normalized name
                if (!(Progress.Progress(0, "Selecting best match for normalized artist names...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                // Correct artist names in library
                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Writing corrected artist names...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    String artist = NormalizeArtist(item.Tags.Get("Artist", String.Empty).ToString());
                    String originalArtist = NormalizeArtist(item.Tags.Get("OriginalArtist", String.Empty).ToString());

                    if (normalizedMasterList.Contains(artist))
                    {
                        if (boolRemoveArtistPrefixes)
                        {
                            item.Tags["Artist"] = ReformatString(masterList[normalizedMasterList.IndexOf(artist)].ToString());
                        }
                        else
                        {
                            item.Tags["Artist"] = masterList[normalizedMasterList.IndexOf(artist)].ToString();
                        }
                    }
                    if (normalizedMasterList.Contains(originalArtist))
                    {
                        if (boolRemoveArtistPrefixes)
                        {
                            item.Tags["OriginalArtist"] = ReformatString(masterList[normalizedMasterList.IndexOf(originalArtist)].ToString());
                        }
                        else
                        {
                            item.Tags["OriginalArtist"] = masterList[normalizedMasterList.IndexOf(originalArtist)].ToString();
                        }
                    }
                    item.SaveTags();
                }
            }
            else if (boolRemoveArtistPrefixes)
            {
                if (!(Progress.Progress(0, "Removing artist name prefixes...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Removing artist name prefixes...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);
                    try
                    {
                        item.Tags["Artist"] = ReformatString(item.Tags.Get("Artist", "").ToString().Trim());
                        item.Tags["OriginalArtist"] = ReformatString(item.Tags.Get("OriginalArtist", "").ToString().Trim());
                        item.SaveTags();
                    }
                    catch (Exception exception)
                    {
                        WriteDebug("ReformatString : ", exception.Message, " : " + item.Tags.Get("Artist", "").ToString() + " / " + item.Tags.Get("OriginalArtist", "").ToString());
                        if (!(Progress.Progress(0, exception.Message)))
                        {
                            Section.CancelUpdate();
                            Progress.Progress(0, "Import cancelled.");
                            return false;
                        }
                    }
                }
            }

            if ((boolNormalizeAlbumNames) && ((added + updated) > 0))
            {
                // Normalize album names (e.g. "The Prodigy", "Prodigy", and "prodigy" all become "The Prodigy")
                ArrayList masterList = new ArrayList();
                ArrayList normalizedMasterList = new ArrayList();

                // Compile all album names
                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Compiling album name lists...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    String album = item.Tags.Get("Album", String.Empty).ToString();

                    if (!(masterList.Contains(album)))
                    {
                        masterList.Add(album);
                    }
                }

                // Sort album name arrays alphabetically
                masterList.Sort();

                // Normalize album name arrays
                if (!(Progress.Progress(0, "Normalizing album names...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                foreach (Object obj in masterList)
                {
                    normalizedMasterList.Add(NormalizeArtist(obj.ToString()));
                }

                // Find and record correct album names based on normalized name
                if (!(Progress.Progress(0, "Selecting best match for normalized album names...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                // Correct album names in library
                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Writing corrected album names...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    String album = NormalizeArtist(item.Tags.Get("Album", String.Empty).ToString());

                    if (normalizedMasterList.Contains(album))
                    {
                        if (boolRemoveAlbumPrefixes)
                        {
                            item.Tags["Album"] = ReformatString(masterList[normalizedMasterList.IndexOf(album)].ToString());
                        }
                        else
                        {
                            item.Tags["Album"] = masterList[normalizedMasterList.IndexOf(album)].ToString();
                        }
                    }
                    item.SaveTags();
                }
            }
            else if (boolRemoveAlbumPrefixes)
            {
                for (int counter = 0; counter < items.Count; counter++)
                {
                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    try
                    {
                        item.Tags["Album"] = ReformatString(item.Tags.Get("Album", "").ToString().Trim());
                        item.SaveTags();
                    }
                    catch (Exception exception)
                    {
                        WriteDebug("ReformatString : ", exception.Message, " : " + item.Tags.Get("Album", "").ToString());
                        if (!(Progress.Progress(0, exception.Message)))
                        {
                            Section.CancelUpdate();
                            Progress.Progress(0, "Import cancelled.");
                            return false;
                        }
                    }
                }
            }

            if ((boolNormalizeGenreNames) && ((added + updated) > 0))
            {
                // Normalize genre names (e.g. "The Prodigy", "Prodigy", and "prodigy" all become "The Prodigy")
                ArrayList masterList = new ArrayList();
                ArrayList normalizedMasterList = new ArrayList();

                // Compile all genre names
                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Compiling genre name lists...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    String[] genres = item.Tags.Get("Genre", String.Empty).ToString().Trim(new char[] { ' ', '|', ',', '/' }).Split(new char[] { '|', ',', '/' });

                    foreach (String genre in genres)
                    {
                        if (!(masterList.Contains(genre)))
                        {
                            masterList.Add(genre);
                        }
                    }
                }

                // Sort genre name arrays alphabetically
                masterList.Sort();

                // Normalize genre name arrays
                if (!(Progress.Progress(0, "Normalizing genre names...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                foreach (Object obj in masterList)
                {
                    normalizedMasterList.Add(NormalizeArtist(obj.ToString()));
                }

                // Find and record correct genre names based on normalized name
                if (!(Progress.Progress(0, "Selecting best match for normalized genre names...")))
                {
                    Section.CancelUpdate();
                    Progress.Progress(0, "Import cancelled.");
                    return false;
                }

                // Correct genre names in library
                for (int counter = 0; counter < items.Count; counter++)
                {
                    if (!(Progress.Progress((int)(((float)counter / (float)items.Count) * (float)100), "Writing corrected genre names...")))
                    {
                        Section.CancelUpdate();
                        Progress.Progress(0, "Import cancelled.");
                        return false;
                    }

                    IMLItem item = Section.FindItemByID(items[counter].ID);

                    String[] genres = item.Tags.Get("Genre", String.Empty).ToString().Trim(new char[] { ' ', '|', ',', '/' }).Split(new char[] { '|', ',', '/' });

                    ArrayList tempGenreList = new ArrayList();
                    foreach (String genre in genres)
                    {
                        String normalGenre = NormalizeArtist(genre);
                        if (normalizedMasterList.Contains(normalGenre))
                        {
                            if (!(tempGenreList.Contains(masterList[normalizedMasterList.IndexOf(normalGenre)].ToString())))
                            {
                                tempGenreList.Add(masterList[normalizedMasterList.IndexOf(normalGenre)].ToString());
                            }
                        }
                    }

                    String tempGenre = String.Empty;
                    foreach (Object obj in tempGenreList)
                    {
                        tempGenre += ((tempGenre == String.Empty) ? "" : "|") + obj.ToString();
                    }

                    item.Tags["Genre"] = ((tempGenre.Contains("|")) ? "|" : "") + tempGenre + ((tempGenre.Contains("|")) ? "|" : "");
                    item.SaveTags();
                }
            }

            if (!(Progress.Progress(0, "Committing changes...")))
            {
                Section.CancelUpdate();
                Progress.Progress(0, "Import cancelled.");
                return false;
            }

            Section.EndUpdate();

            TimeSpan buildtime = new TimeSpan(DateTime.Now.Ticks - starttime.Ticks);

            Progress.Progress(100, (added + updated + skipped) + " files processed: " + added + " added, " + updated + " updated, " + skipped + " skipped. (" + buildtime.Hours.ToString().PadLeft(2, '0') + ":" + buildtime.Minutes.ToString().PadLeft(2, '0') + ":" + buildtime.Seconds.ToString().PadLeft(2, '0') + "." + buildtime.Milliseconds.ToString().PadLeft(3, '0') + ")");

            return true;
        }

        private String SelectArtist(String normalArtist, ArrayList artistList, ArrayList normalList)
        {
            ArrayList artistChoices = new ArrayList();
            ArrayList artistValues = new ArrayList();
            int minValue = 256;
            String returnValue = String.Empty;

            for (int counter = 0; counter < normalList.Count; counter++)
            {
                if (normalArtist == normalList[counter].ToString())
                {
                    artistChoices.Add(artistList[counter]);
                }
            }

            foreach (Object obj in artistChoices)
            {
                Regex nonalphanumeric = new Regex("[^0-9a-zA-Z]", RegexOptions.IgnoreCase);
                String tempArtist = nonalphanumeric.Replace(obj.ToString(), "");
                int tempValue = 0;

                foreach (char character in tempArtist.ToCharArray())
                {
                    tempValue += (int)character;
                }

                tempValue = (int)((float)tempValue / (float)tempArtist.Length);

                minValue = Math.Min(minValue, tempValue);

                artistValues.Add(tempValue);
            }

            try
            {
                returnValue = artistChoices[artistValues.IndexOf(minValue)].ToString();
            }
            catch (Exception exception)
            {
                WriteDebug("SelectArtist() : ", exception.Message, "");
            }

            return returnValue;
        }

        private String NormalizeArtist(String passedArtist)
        {
            String returnValue = passedArtist.ToLower().Replace("&", "and").Replace("+", "and");

            if (returnValue.StartsWith("the "))
            {
                returnValue = returnValue.Substring(4);
            }
            if (returnValue.StartsWith("an "))
            {
                returnValue = returnValue.Substring(3);
            }
            if (returnValue.StartsWith("a "))
            {
                returnValue = returnValue.Substring(2);
            }

            if (returnValue.EndsWith(", the"))
            {
                returnValue.Substring(0, returnValue.Length - 5);
            }
            if (returnValue.EndsWith(", an"))
            {
                returnValue.Substring(0, returnValue.Length - 4);
            }
            if (returnValue.EndsWith(", a"))
            {
                returnValue.Substring(0, returnValue.Length - 3);
            }

            String tempReturnValue = String.Empty;
            String word = String.Empty;
            Regex numeral = new Regex("^[0-9]+$", RegexOptions.IgnoreCase);
            foreach (char character in returnValue.ToCharArray())
            {
                if (numeral.IsMatch(character.ToString()))
                {
                    word += character.ToString();
                }
                else
                {
                    if (word != String.Empty)
                    {
                        if (numeral.IsMatch(word))
                        {
                            while (word.StartsWith("0"))
                            {
                                tempReturnValue += "zero";
                                if (word.Length > 1)
                                {
                                    word = word.Substring(1);
                                }
                                else
                                {
                                    word = String.Empty;
                                }
                            }
                            if (word != String.Empty)
                            {
                                tempReturnValue += NumeralToText(Convert.ToUInt32(word));
                            }
                        }
                        word = String.Empty;
                    }
                    tempReturnValue += character.ToString();
                }
            }
            if (word != String.Empty)
            {
                if (numeral.IsMatch(word))
                {
                    while (word.StartsWith("0"))
                    {
                        tempReturnValue += "zero";
                        if (word.Length > 1)
                        {
                            word = word.Substring(1);
                        }
                        else
                        {
                            word = String.Empty;
                        }
                    }
                    if (word != String.Empty)
                    {
                        tempReturnValue += NumeralToText(Convert.ToUInt32(word));
                    }
                }
                word = String.Empty;
            }

            returnValue = tempReturnValue;

            Regex nonalphanumeric = new Regex("[^0-9a-zA-Z]", RegexOptions.IgnoreCase);
            returnValue = nonalphanumeric.Replace(returnValue, "");

            return returnValue;
        }

        private String ReformatString(String workingString)
        {
            if (workingString.ToLower().StartsWith("a "))
            {
                workingString = workingString.Substring(2) + ", A";
            }
            else if (workingString.ToLower().StartsWith("an "))
            {
                workingString = workingString.Substring(3) + ", An";
            }
            else if (workingString.ToLower().StartsWith("the "))
            {
                workingString = workingString.Substring(4) + ", The";
            }

            return workingString;
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

        private static string NumeralToText(uint Number)
        {
            String numberString = Number.ToString();
            String returnValue = String.Empty;
            int modulous = (int)(numberString.Length % 3);
            if (modulous == 0)
            {
                modulous = 3;
            }
            if ((numberString.Length <= 3) && (numberString.Length > 0))
            {
                returnValue = ConvertNumber(Number);
            }
            else if ((numberString.Length <= 6) && (numberString.Length > 3))
            {
                String thousands = ConvertNumber(Convert.ToUInt32(numberString.Substring(0, modulous)));
                String hundreds = ConvertNumber(Convert.ToUInt32(numberString.Substring(modulous)));
                returnValue = thousands + " thousand " + ((hundreds.Trim().Length == 0) ? "" : hundreds);
            }
            else if ((numberString.Length <= 9) && (numberString.Length > 6))
            {
                String millions = ConvertNumber(Convert.ToUInt32(numberString.Substring(0, modulous)));
                String thousands = ConvertNumber(Convert.ToUInt32(numberString.Substring(modulous, 3)));
                String hundreds = ConvertNumber(Convert.ToUInt32(numberString.Substring(modulous + 3, 3)));
                returnValue = millions + " million " + ((thousands.Trim().Length == 0) ? "" : thousands + " thousand ") + ((hundreds.Trim().Length == 0) ? "" : hundreds);
            }
            else if ((numberString.Length <= 12) && (numberString.Length > 9))
            {
                String billions = ConvertNumber(Convert.ToUInt32(numberString.Substring(0, modulous)));
                String millions = ConvertNumber(Convert.ToUInt32(numberString.Substring(modulous, 3)));
                String thousands = ConvertNumber(Convert.ToUInt32(numberString.Substring(modulous + 3, 3)));
                String hundreds = ConvertNumber(Convert.ToUInt32(numberString.Substring(modulous + 6, 3)));
                returnValue = billions + " billions " + ((millions.Trim().Length == 0) ? "" : millions + " million ") + ((thousands.Trim().Length == 0) ? "" : thousands + " thousand ") + ((hundreds.Trim().Length == 0) ? "" : hundreds);
            }
            return returnValue.Trim().Replace("  ", " ");
        }

        private static string ConvertNumber(uint Number)
        {
            string strhh, strh1, strh2, strh3;
            string str = "";
            uint h1 = Number / 100; //hundreds
            uint h2 = Number % 100;
            uint h3 = h2 / 10; //tens
            uint h4 = h2 % 10; //units

            switch (h1)
            {
                case 1:
                    strh3 = "one hundred";
                    break;
                case 2:
                    strh3 = "two hundred";
                    break;
                case 3:
                    strh3 = "three hundred";
                    break;
                case 4:
                    strh3 = "four hundred";
                    break;
                case 5:
                    strh3 = "five hundred";
                    break;
                case 6:
                    strh3 = "six hundred";
                    break;
                case 7:
                    strh3 = "seven hundred";
                    break;
                case 8:
                    strh3 = "eight hundred";
                    break;
                case 9:
                    strh3 = "nine hundred";
                    break;
                default:
                    strh3 = "";
                    break;
            }
            switch (h3)
            {
                case 1:
                    strh2 = "ten";
                    break;
                case 2:
                    strh2 = "twenty";
                    break;
                case 3:
                    strh2 = "thirty";
                    break;
                case 4:
                    strh2 = "fourty";
                    break;
                case 5:
                    strh2 = "fifty";
                    break;
                case 6:
                    strh2 = "sixty";
                    break;
                case 7:
                    strh2 = "seventy";
                    break;
                case 8:
                    strh2 = "eighty";
                    break;
                case 9:
                    strh2 = "ninety";
                    break;
                default:
                    strh2 = "";
                    break;
            }

            switch (h4)
            {
                case 1:
                    strh1 = "one";
                    break;
                case 2:
                    strh1 = "two";
                    break;
                case 3:
                    strh1 = "three";
                    break;
                case 4:
                    strh1 = "four";
                    break;
                case 5:
                    strh1 = "five";
                    break;
                case 6:
                    strh1 = "six";
                    break;
                case 7:
                    strh1 = "seven";
                    break;
                case 8:
                    strh1 = "eight";
                    break;
                case 9:
                    strh1 = "nine";
                    break;
                default:
                    strh1 = "zero";
                    break;
            }

            //Eleven - Twelve - ... - ninetee
            if (strh2 == "ten" && strh1 == "one")
            {
                strh1 = "";
                strh2 = "eleven";
            }
            else if (strh2 == "ten" && strh1 == "two")
            {
                strh1 = "";
                strh2 = "twelve";
            }
            else if (strh2 == "ten" && strh1 == "three")
            {
                strh1 = "";
                strh2 = "thirteen";
            }
            else if (strh2 == "ten" && strh1 == "four")
            {
                strh1 = "";
                strh2 = "fourteen";
            }
            else if (strh2 == "ten" && strh1 == "five")
            {
                strh1 = "";
                strh2 = "fifteen";
            }
            else if (strh2 == "ten" && strh1 == "six")
            {
                strh1 = "";
                strh2 = "sixteen";
            }
            else if (strh2 == "ten" && strh1 == "seven")
            {
                strh1 = "";
                strh2 = "seventeen";
            }
            else if (strh2 == "ten" && strh1 == "eight")
            {
                strh1 = "";
                strh2 = "eighteen";
            }
            else if (strh2 == "ten" && strh1 == "nine")
            {
                strh1 = "";
                strh2 = "nineteen";
            }

            //special cases
            if (str.Length == 1)
            {
                strhh = strh1;
            }
            else if (strh1 == "zero")
            {
                strhh = strh3 + " " + strh2;
            }
            else if (strh2 == "")
            {
                strhh = strh3 + " " + strh2 + strh1;
            }
            else if (str.Length == 2)
            {
                strhh = strh3 + " " + strh2 + " " + strh1;
            }
            else
            {
                strhh = strh3 + " " + strh2 + " " + strh1;
            }

            return strhh;
        }

        private void WriteDebug(String prefix, String message, String suffix)
        {
            if (boolDebugLog)
            {
                StreamWriter lout = new StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + "\\AudioTagImporter.log", true);
                lout.WriteLine(DateTime.Now + " : " + prefix + message + suffix);
                lout.Close();
            }
        }
    }
}
