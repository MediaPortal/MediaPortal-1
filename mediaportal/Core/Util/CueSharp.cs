/*
Title:    CueSharp
Version:  0.5
Released: March 24, 2007

Author:   Wyatt O'Day
Website:  wyday.com/cuesharp
*/
using System;
using System.Text;
using System.IO;
using TagLib;

namespace CueSharp
{
  /// <summary>
  /// A CueSheet class used to create, open, edit, and save cuesheets.
  /// </summary>
  public class CueSheet
  {
    #region Private Variables
    string[] cueLines;

    private string m_FilePath = "";
    private string m_Catalog = "";
    private string m_CDTextFile = "";
    private string[] m_Comments = new string[0];
    // strings that don't belong or were mistyped in the global part of the cue
    private string[] m_Garbage = new string[0];
    private string m_Performer = "";
    private string m_Songwriter= "";
    private string m_Title="";
    private Track[] m_Tracks = new Track[0];
    #endregion

    #region Properties


    /// <summary>
    /// Returns/Sets track in this cuefile.
    /// </summary>
    /// <param name="tracknumber">The track in this cuefile.</param>
    /// <returns>Track at the tracknumber.</returns>
    public Track this[int tracknumber]
    {
      get { return m_Tracks[tracknumber]; }
      set { m_Tracks[tracknumber] = value; }
    }

    /// <summary>
    /// The full path to the cuesheet.
    /// </summary>
    public string FilePath
    {
      get { return m_FilePath; }
      set { m_FilePath = value; }
    }

    /// <summary>
    /// The catalog number must be 13 digits long and is encoded according to UPC/EAN rules.
    /// Example: CATALOG 1234567890123
    /// </summary>
    public string Catalog
    {
      get { return m_Catalog; }
      set { m_Catalog = value; }
    }

    /// <summary>
    /// This command is used to specify the name of the file that contains the encoded CD-TEXT information for the disc. This command is only used with files that were either created with the graphical CD-TEXT editor or generated automatically by the software when copying a CD-TEXT enhanced disc.
    /// </summary>
    public string CDTextFile
    {
      get { return m_CDTextFile; }
      set { m_CDTextFile = value; }
    }

    /// <summary>
    /// This command is used to put comments in your CUE SHEET file.
    /// </summary>
    public string[] Comments
    {
      get { return m_Comments; }
      set { m_Comments = value; }
    }

    /// <summary>
    /// Lines in the cue file that don't belong or have other general syntax errors.
    /// </summary>
    public string[] Garbage
    {
      get { return m_Garbage; }
    }

    /// <summary>
    /// This command is used to specify the name of a perfomer for a CD-TEXT enhanced disc.
    /// </summary>
    public string Performer
    {
      get { return m_Performer; }
      set { m_Performer = value; }
    }

    /// <summary>
    /// This command is used to specify the name of a songwriter for a CD-TEXT enhanced disc.
    /// </summary>
    public string Songwriter
    {
      get { return m_Songwriter; }
      set { m_Songwriter = value; }
    }

    /// <summary>
    /// The title of the entire disc as a whole.
    /// </summary>
    public string Title
    {
      get { return m_Title; }
      set { m_Title = value; }
    }

    /// <summary>
    /// The array of tracks on the cuesheet.
    /// </summary>
    public Track[] Tracks
    {
      get { return m_Tracks; }
      set { m_Tracks = value; }
    }

    /// <summary>
    /// Get/set the genre/date/comment/discID tags of the track
    /// </summary>
    public string GenreTag
    {
      get { return GetTagInComment("GENRE"); }
      set { SetTagInComment("GENRE", value); }
    }
    public string DateTag
    {
      get { return GetTagInComment("DATE"); }
      set { SetTagInComment("DATE", value); }
    }
    public string CommentTag
    {
      get { return GetTagInComment("COMMENT"); }
      set { SetTagInComment("COMMENT", value); }
    }
    public int DiscIDTag
    {
      get
      {
        string strDiscID = GetTagInComment("DISCID");
        try
        { return (int)Convert.ToUInt32(strDiscID,16); }
        catch (Exception)
        { }
        return 0;
      }
      set { SetTagInComment("DISCID", String.Format("{0:x8}", value)); }
    }
    public int DiscNumberTag
    {
      get
      {
        string strDiscNumber = GetTagInComment("DISCNUMBER");
        try
        { return Convert.ToInt32(strDiscNumber); }
        catch (Exception)
        { }
        return 0;
      }
      set { SetTagInComment("DISCNUMBER", value.ToString()); }
    }
    public string CompilationTag
    {
      get { return GetTagInComment("COMPILATION"); }
      set { SetTagInComment("COMPILATION", value); }
    }
    public string ISRCTag
    {
      get { return GetTagInComment("ISRC"); }
      set { SetTagInComment("ISRC", value); }
    }

    /// <summary>
    /// The array of datafiles in the cuesheet.
    /// </summary>
    public string[] DataFiles
    {
      get
      {
        int nb_datafiles = 0;
        for (int t = 0; t < Tracks.Length; ++t)
        {
          if (Tracks[t].DataFile.Filename != null)
            nb_datafiles++;
        }
        string[] audiofiles = new string[nb_datafiles];
        for (int t = 0, i = 0; t < Tracks.Length; ++t)
        {
          if (Tracks[t].DataFile.Filename != null)
            audiofiles[i++] = Tracks[t].DataFile.Filename;
        }
        return audiofiles;
      }
    }
    #endregion

    #region Constructors

    /// <summary>
    /// Create a cue sheet from scratch.
    /// </summary>
    public CueSheet()
    { }

    /// <summary>
    /// Parse a cue sheet string.
    /// </summary>
    /// <param name="cueString">A string containing the cue sheet data.</param>
    /// <param name="lineDelims">Line delimeters; set to "(char[])null" for default delimeters.</param>
    public CueSheet(string cueString, char[] lineDelims)
    {
      if (lineDelims == null)
        lineDelims = new char[] { '\n' };

      cueLines = cueString.Split(lineDelims);
      RemoveEmptyLines(ref cueLines);
      ParseCue(cueLines);
    }

    /// <summary>
    /// Parses a cue sheet file.
    /// </summary>
    /// <param name="cuefilename">The filename for the cue sheet to open.</param>
    public CueSheet(string cuefilename)
    {
      ReadCueSheet(cuefilename, Encoding.Default);
    }

    /// <summary>
    /// Parses a cue sheet file.
    /// </summary>
    /// <param name="cuefilename">The filename for the cue sheet to open.</param>
    /// <param name="encoding">The encoding used to open the file.</param>
    public CueSheet(string cuefilename, Encoding encoding)
    {
      ReadCueSheet(cuefilename, encoding);
    }

    private void ReadCueSheet(string filename, Encoding encoding)
    {
      FilePath = filename;
      // array of delimiters to split the sentence with
      char[] delimiters = new char[] { '\n' };

      // read in the full cue file
      TextReader tr = new StreamReader(filename, encoding);
      //read in file
      cueLines = tr.ReadToEnd().Split(delimiters);

      // close the stream
      tr.Close();

      RemoveEmptyLines(ref cueLines);

      ParseCue(cueLines);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Removes any empty lines, elimating possible trouble.
    /// </summary>
    /// <param name="file"></param>
    private void RemoveEmptyLines(ref string[] file)
    {
      int itemsRemoved = 0;

      for (int i = 0; i < file.Length; i++)
      {
        if (file[i].Trim() != "")
          file[i - itemsRemoved] = file[i];
        else if (file[i].Trim() == "")
          itemsRemoved++;
      }

      if (itemsRemoved > 0)
        file = (string[])ResizeArray(file, file.Length - itemsRemoved);
    }

    private void ParseCue(string[] file)
    {
      //-1 means still global, 
      //all others are track specific
      int trackOn = -1;
      AudioFile currentFile = new AudioFile();

      for (int i = 0; i < file.Length; i++)
      {
        file[i] = file[i].Trim();

        switch (file[i].Substring(0, file[i].IndexOf(' ')).ToUpper())
        {
          case "CATALOG":
            ParseString(file[i], trackOn);
            break;
          case "CDTEXTFILE":
            ParseString(file[i], trackOn);
            break;
          case "FILE":
            currentFile = ParseFile(file[i], trackOn);
            break;
          case "FLAGS":
            ParseFlags(file[i], trackOn);
            break;
          case "INDEX":
            ParseIndex(file[i], trackOn);
            break;
          case "ISRC":
            ParseString(file[i], trackOn);
            break;
          case "PERFORMER":
            ParseString(file[i], trackOn);
            break;
          case "POSTGAP":
            ParseIndex(file[i], trackOn);
            break;
          case "PREGAP":
            ParseIndex(file[i], trackOn);
            break;
          case "REM":
            ParseComment(file[i], trackOn);
            break;
          case "SONGWRITER":
            ParseString(file[i], trackOn);
            break;
          case "TITLE":
            ParseString(file[i], trackOn);
            break;
          case "TRACK":
            trackOn++;
            ParseTrack(file[i], trackOn);
            if (currentFile.Filename != "") //if there's a file
            {
              m_Tracks[trackOn].DataFile = currentFile;
              currentFile = new AudioFile();
            }
            break;
          default:
            ParseGarbage(file[i], trackOn);
            //save discarded junk and place string[] with track it was found in
            break;
        }
      }
    }

    private void ParseComment(string line, int trackOn)
    {
      //remove "REM" (we know the line has already been .Trim()'ed)
      line = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

      if (trackOn == -1)
      {
        if (line.Trim() != "")
        {
          m_Comments = (string[])ResizeArray(m_Comments, m_Comments.Length + 1);
          m_Comments[m_Comments.Length - 1] = line;
        }
      }
      else
      {
        m_Tracks[trackOn].AddComment(line);
      }
    }

    private int GetCommentTag(string comment_name)
    {
      for (int i = 0; i < Comments.Length; i++)
      {
        if ( Comments[i].Trim().IndexOf(comment_name) == 0 )
          return i;
      }
      return -1;
    }

    private string GetTagInComment(string comment_name)
    {
      int i = GetCommentTag(comment_name);

      if ( i < 0 )
        return string.Empty;
      else
        return Comments[i].Substring(comment_name.Length);
    }

    private void SetTagInComment(string comment_name, string value)
    {
      int i = GetCommentTag(comment_name);

      if ( i < 0 )
        AddComment(comment_name.Trim() + " " + value.Trim());
      else
        Comments[i] = comment_name.Trim() + " " + value.Trim();
    }

    private AudioFile ParseFile(string line, int trackOn)
    {
      string fileType;

      line = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

      fileType = line.Substring(line.LastIndexOf(' '), line.Length - line.LastIndexOf(' ')).Trim();

      line = line.Substring(0, line.LastIndexOf(' ')).Trim();

      //if quotes around it, remove them.
      if (line[0] == '"')
      {
        line = line.Substring(1, line.LastIndexOf('"') - 1);
      }

      return new AudioFile(line, fileType);
    }

    private void ParseFlags(string line, int trackOn)
    {
      string temp;

      if (trackOn != -1)
      {
        line = line.Trim();
        if (line != "")
        {
          try
          {
            temp = line.Substring(0, line.IndexOf(' ')).ToUpper();
          }
          catch (Exception)
          {
            temp = line.ToUpper();
          }

          switch (temp)
          {
            case "FLAGS":
              m_Tracks[trackOn].AddFlag(temp);
              break;
            case "DATA":
              m_Tracks[trackOn].AddFlag(temp);
              break;
            case "DCP":
              m_Tracks[trackOn].AddFlag(temp);
              break;
            case "4CH":
              m_Tracks[trackOn].AddFlag(temp);
              break;
            case "PRE":
              m_Tracks[trackOn].AddFlag(temp);
              break;
            case "SCMS":
              m_Tracks[trackOn].AddFlag(temp);
              break;
            default:
              break;
          }

          //processing for a case when there isn't any more spaces
          //i.e. avoiding the "index cannot be less than zero" error
          //when calling line.IndexOf(' ')
          try
          {
            temp = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' '));
          }
          catch (Exception)
          {
            temp = line.Substring(0, line.Length);
          }

          //if the flag hasn't already been processed
          if (temp.ToUpper().Trim() != line.ToUpper().Trim())
          {
            ParseFlags(temp, trackOn);
          }
        }
      }
    }

    private void ParseGarbage(string line, int trackOn)
    {
      if (trackOn == -1)
      {
        if (line.Trim() != "")
        {
          m_Garbage = (string[])ResizeArray(m_Garbage, m_Garbage.Length + 1);
          m_Garbage[m_Garbage.Length - 1] = line;
        }
      }
      else
      {
        m_Tracks[trackOn].AddGarbage(line);
      }
    }

    private void ParseIndex(string line, int trackOn)
    {
      string indexType;
      string tempString;

      int number = 0;
      int minutes;
      int seconds;
      int frames;

      indexType = line.Substring(0, line.IndexOf(' ')).ToUpper();

      tempString = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

      if (indexType == "INDEX")
      {
        //read the index number
        number = Convert.ToInt32(tempString.Substring(0, tempString.IndexOf(' ')));
        tempString = tempString.Substring(tempString.IndexOf(' '), tempString.Length - tempString.IndexOf(' ')).Trim();
      }

      //extract the minutes, seconds, and frames
      minutes = Convert.ToInt32(tempString.Substring(0, tempString.IndexOf(':')));
      seconds = Convert.ToInt32(tempString.Substring(tempString.IndexOf(':') + 1, tempString.LastIndexOf(':') - tempString.IndexOf(':') - 1));
      frames = Convert.ToInt32(tempString.Substring(tempString.LastIndexOf(':') + 1, tempString.Length - tempString.LastIndexOf(':') - 1));

      if (indexType == "INDEX")
        m_Tracks[trackOn].AddIndex(number, minutes, seconds, frames);
      else if (indexType == "PREGAP")
        m_Tracks[trackOn].PreGap = new FramePosition(minutes, seconds, frames);
      else if (indexType == "POSTGAP")
        m_Tracks[trackOn].PostGap = new FramePosition(minutes, seconds, frames);
    }

    private void ParseString(string line, int trackOn)
    {
      string category = line.Substring(0, line.IndexOf(' ')).ToUpper();

      line = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

      //get rid of the quotes
      if (line[0] == '"')
        line = line.Substring(1, line.LastIndexOf('"') - 1);

      switch (category)
      {
        case "CATALOG":
          if (trackOn == -1)
            this.m_Catalog = line;
          break;
        case "CDTEXTFILE":
          if (trackOn == -1)
            this.m_CDTextFile = line;
          break;
        case "ISRC":
          if (trackOn != -1)
            m_Tracks[trackOn].ISRC = line;
          break;
        case "PERFORMER":
          if (trackOn == -1)
            this.m_Performer = line;
          else
            m_Tracks[trackOn].Performer = line;
          break;
        case "SONGWRITER":
          if (trackOn == -1)
            this.m_Songwriter = line;
          else
            m_Tracks[trackOn].Songwriter = line;
          break;
        case "TITLE":
          if (trackOn == -1)
            this.m_Title = line;
          else
            m_Tracks[trackOn].Title = line;
          break;
        default:
          break;
      }
    }

    /// <summary>
    /// Parses the TRACK command. 
    /// </summary>
    /// <param name="line">The line in the cue file that contains the TRACK command.</param>
    /// <param name="trackOn">The track currently processing.</param>
    private void ParseTrack(string line, int trackOn)
    {
      string tempString;
      int trackNumber;

      tempString = line.Substring(line.IndexOf(' '), line.Length - line.IndexOf(' ')).Trim();

      try
      { trackNumber = Convert.ToInt32(tempString.Substring(0, tempString.IndexOf(' '))); }
      catch (Exception)
      { throw; }

      //find the data type.
      tempString = tempString.Substring(tempString.IndexOf(' '), tempString.Length - tempString.IndexOf(' ')).Trim();

      AddTrack(trackNumber, tempString);
    }

    /// <summary>
    /// Reallocates an array with a new size, and copies the contents
    /// of the old array to the new array.
    /// </summary>
    /// <param name="oldArray">The old array, to be reallocated.</param>
    /// <param name="newSize">The new array size.</param>
    /// <returns>A new array with the same contents.</returns>
    /// <remarks >Useage: int[] a = {1,2,3}; a = (int[])ResizeArray(a,5);</remarks>
    public static System.Array ResizeArray(System.Array oldArray, int newSize)
    {
      int oldSize = oldArray.Length;
      System.Type elementType = oldArray.GetType().GetElementType();
      System.Array newArray = System.Array.CreateInstance(elementType, newSize);
      int preserveLength = System.Math.Min(oldSize, newSize);
      if (preserveLength > 0)
        System.Array.Copy(oldArray, newArray, preserveLength);
      return newArray;
    }

    /// <summary>
    /// Add a track to the current cuesheet.
    /// </summary>
    /// <param name="tracknumber">The number of the said track.</param>
    /// <param name="datatype">The datatype of the track.</param>
    private void AddTrack(int tracknumber, string datatype)
    {
      Track track = new Track(tracknumber, datatype);

      AddTrack(track);
    }

    /// <summary>
    /// Add a track to the current cuesheet
    /// </summary>
    /// <param name="title">The title of the track.</param>
    /// <param name="performer">The performer of this track.</param>
    public void AddTrack(string title, string performer)
    {
      Track track = new Track(m_Tracks.Length, "");
      track.Performer = performer;
      track.Title     = title;

      AddTrack(track);
    }


    /// <summary>
    /// Add a track to the current cuesheet
    /// </summary>
    /// <param name="title">The title of the track.</param>
    /// <param name="performer">The performer of this track.</param>
    /// <param name="filename">The name of the file associated to this track</param>
    /// <param name="fType">The datatype for the track (typically DataType.Audio)</param>
    public void AddTrack(string title, string performer, string filename, FileType fType)
    {
      Track track = new Track(m_Tracks.Length, "");
      track.Performer = performer;
      track.Title     = title;
      track.DataFile  = new AudioFile(filename, fType);

      AddTrack(track);
    }

    /// <summary>
    /// Add a track to the current cuesheet
    /// </summary>
    /// <param name="title">The title of the track.</param>
    /// <param name="performer">The performer of this track.</param>
    /// <param name="datatype">The datatype for the track (typically DataType.Audio)</param>
    public void AddTrack(string title, string performer, DataType datatype)
    {
      Track track = new Track(m_Tracks.Length, datatype);
      track.Performer = performer;
      track.Title     = title;

      AddTrack(track);
    }

    /// <summary>
    /// Add a track to the current cuesheet
    /// </summary>
    /// <param name="track">Track object to add to the cuesheet.</param>
    public void AddTrack(Track track)
    {
      track.Sheet = this;
      track.Rank  = m_Tracks.Length;

      m_Tracks = (Track[])ResizeArray(m_Tracks, m_Tracks.Length + 1);
      m_Tracks[track.Rank] = track;
    }

    /// <summary>
    /// Remove a track from the cuesheet.
    /// </summary>
    /// <param name="trackIndex">The index of the track you wish to remove.</param>
    public void RemoveTrack(int trackIndex)
    {
      if ((trackIndex >=0 ) && (trackIndex < m_Tracks.Length))
      {
        Track track = m_Tracks[trackIndex];
        track.Sheet = null;
        track.Rank  = -1;
        for (int i = trackIndex; i < m_Tracks.Length - 1; i++)
          m_Tracks[i] = m_Tracks[i + 1];
        m_Tracks = (Track[])ResizeArray(m_Tracks, m_Tracks.Length - 1);
      }
    }

    /// <summary>
    /// Add index information to an existing track.
    /// </summary>
    /// <param name="trackIndex">The array index number of track to be modified</param>
    /// <param name="indexNum">The index number of the new index</param>
    /// <param name="minutes">The minute value of the new index</param>
    /// <param name="seconds">The seconds value of the new index</param>
    /// <param name="frames">The frames value of the new index</param>
    public void AddIndex(int trackIndex, int indexNum, int minutes, int seconds, int frames)
    {
      m_Tracks[trackIndex].AddIndex(indexNum, minutes, seconds, frames);
    }

    /// <summary>
    /// Remove an index from a track.
    /// </summary>
    /// <param name="trackIndex">The array-index of the track.</param>
    /// <param name="indexIndex">The index of the Index you wish to remove.</param>
    public void RemoveIndex(int trackIndex, int indexIndex)
    {
      //Note it is the index of the Index you want to delete, 
      //which may or may not correspond to the number of the index.
      m_Tracks[trackIndex].RemoveIndex(indexIndex);
    }

    /// <summary>
    /// Add a comment to the current cuesheet.
    /// </summary>
    /// <param name="comment">The text of the comment to add.</param>
    public void AddComment(string comment)
    {
      if (comment.Trim().Length > 0)
      {
        m_Comments = (string[])ResizeArray(m_Comments, m_Comments.Length + 1);
        m_Comments[m_Comments.Length - 1] = comment.Trim();
      }
    }

    /// <summary>
    /// Save the cue sheet file to specified location.
    /// </summary>
    /// <param name="filename">Filename of destination cue sheet file.</param>
    public void SaveCue(string filename)
    {
      SaveCue(filename, Encoding.Default);
    }

    /// <summary>
    /// Save the cue sheet file to specified location.
    /// </summary>
    /// <param name="filename">Filename of destination cue sheet file.</param>
    /// <param name="encoding">The encoding used to save the file.</param>
    public void SaveCue(string filename, Encoding encoding)
    {
      TextWriter tw = new StreamWriter(filename, false, encoding);

      tw.WriteLine(this.ToString());

      //close the writer stream
      tw.Close();
    }

    /// <summary>
    /// Method to output the cuesheet into a single formatted string.
    /// </summary>
    /// <returns>The entire cuesheet formatted to specification.</returns>
    public override string ToString()
    {
      StringBuilder output = new StringBuilder();

      foreach (string comment in m_Comments)
      {
        output.Append("REM " + comment + Environment.NewLine);
      }

      if (m_Catalog.Trim() != "")
      {
        output.Append("CATALOG " + m_Catalog + Environment.NewLine);
      }

      if (m_Performer.Trim() != "")
      {
        output.Append("PERFORMER \"" + m_Performer + "\"" + Environment.NewLine);
      }

      if (m_Songwriter.Trim() != "")
      {
        output.Append("SONGWRITER \"" + m_Songwriter + "\"" + Environment.NewLine);
      }

      if (m_Title.Trim() != "")
      {
        output.Append("TITLE \"" + m_Title + "\"" + Environment.NewLine);
      }

      if (m_CDTextFile.Trim() != "")
      {
        output.Append("CDTEXTFILE \"" + m_CDTextFile.Trim() + "\"" + Environment.NewLine);
      }

      for (int i = 0; i < m_Tracks.Length; i++)
      {
        output.Append(m_Tracks[i].ToString());

        if (i != m_Tracks.Length - 1)
        {
          //add line break for each track except last
          output.Append(Environment.NewLine);
        }
      }

      return output.ToString();
    }

    #endregion

    //TODO: Fix calculation bugs; currently generates erroneous IDs.
    #region CalculateDiscIDs
    //For complete CDDB/freedb discID calculation, see:
    //http://www.freedb.org/modules.php?name=Sections&sop=viewarticle&artid=6

    /// <summary>
    /// Method to compute the CDDB disc ID and output it into a single formatted string.
    /// </summary>
    /// <returns>The CDDB disc ID.</returns>
    public string CalculateCDDBdiscID()
    {
      int i, t = 0, n = 0;

      /* For backward compatibility this algorithm must not change */

      i = 0;

      while (i < m_Tracks.Length)
      {
        n = n + cddb_sum((lastTrackIndex(m_Tracks[i]).Position.Minutes * 60) + lastTrackIndex(m_Tracks[i]).Position.Seconds);
        i++;
      }

      Console.WriteLine(n.ToString());

      t = ((lastTrackIndex(m_Tracks[m_Tracks.Length - 1]).Position.Minutes * 60) + lastTrackIndex(m_Tracks[m_Tracks.Length - 1]).Position.Seconds) -
              ((lastTrackIndex(m_Tracks[0]).Position.Minutes * 60) + lastTrackIndex(m_Tracks[0]).Position.Seconds);

      ulong lDiscId = (((uint)n % 0xff) << 24 | (uint)t << 8 | (uint)m_Tracks.Length);
      return String.Format("{0:x8}", lDiscId);
    }

    private Index lastTrackIndex(Track track)
    {
      return track.Indices[track.Indices.Length - 1];
    }

    private int cddb_sum(int n)
    {
      int ret;

      /* For backward compatibility this algorithm must not change */

      ret = 0;

      while (n > 0)
      {
        ret = ret + (n % 10);
        n = n / 10;
      }

      return (ret);
    }

    #endregion CalculateDiscIDS

  }

  /// <summary>
  ///DCP - Digital copy permitted
  ///4CH - Four channel audio
  ///PRE - Pre-emphasis enabled (audio tracks only)
  ///SCMS - Serial copy management system (not supported by all recorders)
  ///There is a fourth subcode flag called "DATA" which is set for all non-audio tracks. This flag is set automatically based on the datatype of the track.
  /// </summary>
  public enum Flags
  {
    DCP, CH4, PRE, SCMS, DATA, NONE
  }

  /// <summary>
  /// BINARY - Intel binary file (least significant byte first)
  /// MOTOROLA - Motorola binary file (most significant byte first)
  /// AIFF - Audio AIFF file
  /// WAVE - Audio WAVE file
  /// MP3 - Audio MP3 file
  /// </summary>
  public enum FileType
  {
    BINARY, MOTOROLA, AIFF, WAVE, MP3
  }

  /// <summary>
  /// <list>
  /// <item>AUDIO - Audio/Music (2352)</item>
  /// <item>CDG - Karaoke CD+G (2448)</item>
  /// <item>MODE1/2048 - CDROM Mode1 Data (cooked)</item>
  /// <item>MODE1/2352 - CDROM Mode1 Data (raw)</item>
  /// <item>MODE2/2336 - CDROM-XA Mode2 Data</item>
  /// <item>MODE2/2352 - CDROM-XA Mode2 Data</item>
  /// <item>CDI/2336 - CDI Mode2 Data</item>
  /// <item>CDI/2352 - CDI Mode2 Data</item>
  /// </list>
  /// </summary>
  public enum DataType
  {
    AUDIO, CDG, MODE1_2048, MODE1_2352, MODE2_2336, MODE2_2352, CDI_2336, CDI_2352
  }

  /// <summary>
  /// This class is used to manipulate frame position in +/-:minutes:seconds:frames,
  /// but also durations in same units.
  /// </summary>
  public class FramePosition
  {
    #region Private Variables
    private int m_sign    = 1; // Is the frame position a negative/positive value
    private int m_minutes = 0;
    private int m_seconds = 0; //   0..59
    private int m_frames  = 0; //   0..FRAMES_PER_SECOND-1
    #endregion

    #region Properties
    public const int FRAMES_PER_SECOND = 75;

    /// <summary>
    /// Get/set sign only
    /// </summary>
    public int Sign
    {
      get { return m_sign; }
      set { m_sign = (value < 0) ? -1 : +1; }
    }

    /// <summary>
    /// Get/set minutes only
    /// </summary>
    public int Minutes
    {
      get { return m_minutes; }
      set { m_minutes  = Math.Abs(value); }
    }

    /// <summary>
    /// Get/set seconds only
    /// </summary>
    public int Seconds
    {
      get { return m_seconds; }
      set
      {
        if (value >= 60)
          m_seconds = 59;
        else if (value < 0)
          m_seconds = 0;
        else
          m_seconds = value;
      }
    }

    /// <summary>
    /// Get/set frames only
    /// </summary>
    public int Frames
    {
      get { return m_frames; }
      set
      {
        if (value >= FRAMES_PER_SECOND)
          m_frames = FRAMES_PER_SECOND-1;
        else if (value < 0)
          m_frames = 0;
        else
          m_frames = value;
      }
    }

    /// <summary>
    /// Get/set this object in number of ms
    /// </summary>
    public int InMiliSeconds
    {
      get { return (InFrames*1000)/FRAMES_PER_SECOND; }
      set { InFrames = (value * FRAMES_PER_SECOND) / 1000; }
    }

    /// <summary>
    /// Get/set this object in number of frames
    /// </summary>
    public int InFrames
    {
      get { return Sign*(Frames + 75*(Seconds + 60*Minutes)); }
      set
      {
        int x = Math.Abs(value);
        Sign = value;
        Frames = x % FRAMES_PER_SECOND;
        x /= FRAMES_PER_SECOND;
        Seconds = x % 60;
        x /= 60;
        Minutes = x;
      }
    }

    /// <summary>
    /// Property to get/set the index in seconds
    /// </summary>
    public int InSeconds
    {
      get { return Sign*(Seconds + 60*Minutes); }
      set
      {
        int x = Math.Abs(value);
        Sign = value;
        Frames = 0;
        Seconds = x % 60;
        x /= 60;
        Minutes = x;
      }
    }

    /// <summary>
    /// Property to get/set the index in minutes
    /// </summary>
    public int InMinutes
    {
      get { return Sign*Minutes; }
      set
      {
        int x = Math.Abs(value);
        Sign = value;
        Frames = 0;
        Seconds = 0;
        Minutes = x;
      }
    }
    #endregion

    #region Operators
    public static FramePosition operator+(FramePosition pos1, FramePosition pos2)
    {
      FramePosition pos = new FramePosition(pos1);
      pos.InFrames += pos2.InFrames;
      return pos;
    }

    public static FramePosition operator-(FramePosition pos1, FramePosition pos2)
    {
      FramePosition pos = new FramePosition(pos1);
      pos.InFrames -= pos2.InFrames;
      return pos;
    }
    #endregion

    #region Contructors
    public FramePosition()
    {}
    public FramePosition(FramePosition pos)
    {
      m_sign    = pos.m_sign;
      m_minutes = pos.m_minutes;
      m_seconds = pos.m_seconds;
      m_frames  = pos.m_frames;
    }
    public FramePosition(int minutes, int seconds, int frames)
    {
      Sign    = minutes;
      Minutes = minutes;
      Seconds = seconds;
      Frames  = frames;
    }
    #endregion
  }

  /// <summary>
  /// This command is used to specify indexes (or subindexes) within a track.
  /// Syntax:
  ///  INDEX [number] [mm:ss:ff]
  /// </summary>
  public class Index
  {
    #region Private Variables
    int m_number;
    FramePosition m_frame_position;
    #endregion

    #region Properties
    /// <summary>
    /// Get/set the index number
    /// </summary>
    public int Number
    {
      get { return m_number; }
      set
      {
        if (value > 99)
          m_number = 99;
        else if (value < 0)
          m_number = 0;
        else
          m_number = value;
      }
    }

    /// <summary>
    /// Get/set the index position
    /// </summary>
    public FramePosition Position
    {
      get { return m_frame_position; }
      set { m_frame_position = value; }
    }
    #endregion

    #region Contructors
    /// <summary>
    /// The Index of a track.
    /// </summary>
    /// <param name="number">Index number 0-99</param>
    /// <param name="minutes">Minutes (0-99)</param>
    /// <param name="seconds">Seconds (0-59)</param>
    /// <param name="frames">Frames (0-74)</param>
    public Index(int number, int minutes, int seconds, int frames)
    {
      m_number = number;
      m_frame_position = new FramePosition(minutes, seconds, frames);
    }
    public Index(int number)
    {
      m_number = number;
      m_frame_position = new FramePosition();
    }
    private Index()
    {
      m_number = -1;
      m_frame_position = null;
    }
    #endregion
  }

  /// <summary>
  /// Track that contains either data or audio. It can contain Indices and comment information.
  /// </summary>
  public class Track
  {
    #region Private Variables
    private string[] m_Comments;
    // strings that don't belong or were mistyped in the global part of the cue
    private AudioFile m_DataFile;
    private string[] m_Garbage;
    private Index[] m_Indices;
    private string m_ISRC;

    private string m_Performer;
    private FramePosition m_PostGap;
    private FramePosition m_PreGap;
    private string m_Songwriter;
    private string m_Title;
    private Flags[] m_TrackFlags;
    private DataType m_TrackDataType;
    private int m_TrackNumber;
    private CueSheet m_CueSheet = null; // Owner of this track
    private int m_TrackRank = -1; // What is the pos of the track into the track array
    #endregion

    #region Properties
    /// <summary>
    /// Return the cuesheet associated to this track
    /// </summary>
    public CueSheet Sheet
    {
      get { return m_CueSheet; }
      internal set { m_CueSheet = value; }
    }

    /// <summary>
    /// Returns/Sets Index in this track.
    /// </summary>
    /// <param name="indexnumber">Index in the track.</param>
    /// <returns>Index at indexnumber.</returns>
    public Index this[int indexnumber]
    {
      get { return m_Indices[indexnumber]; }
      set { m_Indices[indexnumber] = value; }
    }

    public string[] Comments
    {
      get { return m_Comments; }
      set { m_Comments = value; }
    }


    public AudioFile DataFile
    {
      get { return m_DataFile; }
      set { m_DataFile = value; }
    }

    /// <summary>
    /// The data file this track is mapped onto
    /// </summary>
    public AudioFile UsedDataFile
    {
      get
      {
        AudioFile f = new AudioFile();
        for (int i = m_TrackRank; i >= 0; i--)
        {
          f = Sheet.Tracks[i].DataFile;
          if (f.Filename != null)
            break;
        }
        return f;
      }
    }

    /// <summary>
    /// Lines in the cue file that don't belong or have other general syntax errors.
    /// </summary>
    public string[] Garbage
    {
      get { return m_Garbage; }
      set { m_Garbage = value; }
    }

    public Index[] Indices
    {
      get { return m_Indices; }
      set { m_Indices = value; }
    }

    public string ISRC
    {
      get { return m_ISRC; }
      set { m_ISRC = value; }
    }

    public string Performer
    {
      get { return m_Performer; }
      set { m_Performer = value; }
    }

    public FramePosition PostGap
    {
      get { return m_PostGap; }
      set { m_PostGap = value; }
    }

    public FramePosition PreGap
    {
      get { return m_PreGap; }
      set { m_PreGap = value; }
    }

    public string Songwriter
    {
      get { return m_Songwriter; }
      set { m_Songwriter = value; }
    }

    /// <summary>
    /// If the TITLE command appears before any TRACK commands, then the string will be encoded as the title of the entire disc.
    /// </summary>
    public string Title
    {
      get { return m_Title; }
      set { m_Title = value; }
    }

    public DataType TrackDataType
    {
      get { return m_TrackDataType; }
      set { m_TrackDataType = value; }
    }

    public Flags[] TrackFlags
    {
      get { return m_TrackFlags; }
      set { m_TrackFlags = value; }
    }

    public int TrackNumber
    {
      get { return m_TrackNumber; }
      set { m_TrackNumber = value; }
    }

    public int Rank
    {
      get { return m_TrackRank; }
      internal set { m_TrackRank = value; }
    }

    /// <summary>
    /// Get the track preceding the current one in cuesheet order
    /// </summary>
    public Track PrevTrack
    {
      get
      {
        if (m_TrackRank > 0)
          return Sheet.Tracks[m_TrackRank-1];
        else
          return null;
      }
    }

    /// <summary>
    /// Get the track following the current one in cuesheet order
    /// </summary>
    public Track NextTrack
    {
      get
      {
        if (m_TrackRank < Sheet.Tracks.Length-1)
          return Sheet.Tracks[m_TrackRank+1];
        else
          return null;
      }
    }

    /// <summary>
    /// Get the position of the start frame of the track from the beginning of the data file
    /// </summary>
    public FramePosition DataFileRelativeStartFramePosition
    {
      get
      {
        // INDEX 0 is not always present but INDEX 1 is
        Index idx = GetIndex(0);
        if (idx == null)
          idx = GetIndex(1);

        return idx.Position;
      }
    }

    /// <summary>
    /// Get the position of the end frame of the track from the beginning of the data file
    /// </summary>
    public FramePosition DataFileRelativeEndFramePosition
    {
      get
      {
        string dataFilename = UsedDataFile.Filename;
        // The end of the track is the beginning of the next one
        if (   (NextTrack == null)                                // If last track, we are at the end of the data file used by this track
            || (NextTrack.UsedDataFile.Filename != dataFilename)) // If next track has not the same data file, ditto
        {
          FramePosition pos = new FramePosition();

          try
          {
            // Get data file full path
            string dataFilePath;
            if (Path.IsPathRooted(dataFilename))
              dataFilePath = dataFilename;
            else
              dataFilePath = Path.Combine(Path.GetDirectoryName(Sheet.FilePath), dataFilename);

            // Gets informations about the data file
            TagLib.ByteVector.UseBrokenLatin1Behavior = true;
            TagLib.File tag = TagLib.File.Create(dataFilePath);

            // Convert the nb of ms into a frame position
            pos.InFrames = (int)((tag.Properties.Duration.TotalMilliseconds/1000)*FramePosition.FRAMES_PER_SECOND);
          }
          catch (Exception ex)
          {
          }

          return pos;
        }
        else                                                               // Next track exits and is using the same data file
          return NextTrack.DataFileRelativeStartFramePosition;
      }
    }

    /// <summary>
    /// Get the duration of the track
    /// </summary>
    public FramePosition Duration
    {
      get { return DataFileRelativeEndFramePosition - DataFileRelativeStartFramePosition; }
    }

    /// <summary>
    /// Get/set the genre/date/comment tags of the track
    /// </summary>
    public string GenreTag
    {
      get { return GetTagInComment("GENRE"); }
      set { SetTagInComment("GENRE", value); }
    }
    public string DateTag
    {
      get { return GetTagInComment("DATE"); }
      set { SetTagInComment("DATE", value); }
    }
    public string CommentTag
    {
      get { return GetTagInComment("COMMENT"); }
      set { SetTagInComment("COMMENT", value); }
    }

    #endregion

    #region Contructors

    public Track(int tracknumber, string datatype)
    {
      m_TrackNumber = tracknumber;

      switch (datatype.Trim().ToUpper())
      {
        case "AUDIO":
          m_TrackDataType = DataType.AUDIO;
          break;
        case "CDG":
          m_TrackDataType = DataType.CDG;
          break;
        case "MODE1/2048":
          m_TrackDataType = DataType.MODE1_2048;
          break;
        case "MODE1/2352":
          m_TrackDataType = DataType.MODE1_2352;
          break;
        case "MODE2/2336":
          m_TrackDataType = DataType.MODE2_2336;
          break;
        case "MODE2/2352":
          m_TrackDataType = DataType.MODE2_2352;
          break;
        case "CDI/2336":
          m_TrackDataType = DataType.CDI_2336;
          break;
        case "CDI/2352":
          m_TrackDataType = DataType.CDI_2352;
          break;
        default:
          m_TrackDataType = DataType.AUDIO;
          break;
      }

      m_TrackFlags = new Flags[0];
      m_Songwriter = "";
      m_Title = "";
      m_ISRC = "";
      m_Performer = "";
      m_Indices = new Index[0];
      m_Garbage = new string[0];
      m_Comments = new string[0];
      m_PreGap = null;
      m_PostGap = null;
      m_DataFile = new AudioFile();
    }

    public Track(int tracknumber, DataType datatype)
    {
      m_TrackNumber = tracknumber;
      m_TrackDataType = datatype;

      m_TrackFlags = new Flags[0];
      m_Songwriter = "";
      m_Title = "";
      m_ISRC = "";
      m_Performer = "";
      m_Indices = new Index[0];
      m_Garbage = new string[0];
      m_Comments = new string[0];
      m_PreGap = null;
      m_PostGap = null;
      m_DataFile = new AudioFile();
    }

    #endregion

    #region Methods
    public void AddFlag(Flags flag)
    {
      //if it's not a none tag
      //and if the tags hasn't already been added
      if (flag != Flags.NONE && NewFlag(flag) == true)
      {
        m_TrackFlags = (Flags[])CueSheet.ResizeArray(m_TrackFlags, m_TrackFlags.Length + 1);
        m_TrackFlags[m_TrackFlags.Length - 1] = flag;
      }
    }

    public void AddFlag(string flag)
    {
      switch (flag.Trim().ToUpper())
      {
        case "DATA":
          AddFlag(Flags.DATA);
          break;
        case "DCP":
          AddFlag(Flags.DCP);
          break;
        case "4CH":
          AddFlag(Flags.CH4);
          break;
        case "PRE":
          AddFlag(Flags.PRE);
          break;
        case "SCMS":
          AddFlag(Flags.SCMS);
          break;
        default:
          return;
      }
    }

    public void AddGarbage(string garbage)
    {
      if (garbage.Trim() != "")
      {
        m_Garbage = (string[])CueSheet.ResizeArray(m_Garbage, m_Garbage.Length + 1);
        m_Garbage[m_Garbage.Length - 1] = garbage;
      }
    }

    public void AddComment(string comment)
    {
      if (comment.Trim() != "")
      {
        m_Comments = (string[])CueSheet.ResizeArray(m_Comments, m_Comments.Length + 1);
        m_Comments[m_Comments.Length - 1] = comment;
      }
    }

    private int GetCommentTag(string comment_name)
    {
      for (int i = 0; i < Comments.Length; i++)
      {
        if ( Comments[i].Trim().IndexOf(comment_name) == 0 )
          return i;
      }
      return -1;
    }

    private string GetTagInComment(string comment_name)
    {
      int i = GetCommentTag(comment_name);

      if ( i < 0 )
        return string.Empty;
      else
        return Comments[i].Substring(comment_name.Length);
    }

    private void SetTagInComment(string comment_name, string value)
    {
      int i = GetCommentTag(comment_name);

      if ( i < 0 )
        AddComment(comment_name.Trim() + " " + value.Trim());
      else
        Comments[i] = comment_name.Trim() + " " + value.Trim();
    }


    public void AddIndex(int number, int minutes, int seconds, int frames)
    {
      m_Indices = (Index[])CueSheet.ResizeArray(m_Indices, m_Indices.Length + 1);

      m_Indices[m_Indices.Length - 1] = new Index(number, minutes, seconds, frames);
    }

    public Index GetIndex(int number)
    {
      for (int i = 0; i < m_Indices.Length; i++)
      {
        if (this[i].Number == number)
          return this[i];
      }
      return null;
    }

    public void RemoveIndex(int indexIndex)
    {
      for (int i = indexIndex; i < m_Indices.Length - 1; i++)
      {
        m_Indices[i] = m_Indices[i + 1];
      }
      m_Indices = (Index[])CueSheet.ResizeArray(m_Indices, m_Indices.Length - 1);
    }

    /// <summary>
    /// Checks if the flag is indeed new in this track.
    /// </summary>
    /// <param name="flag">The new flag to be added to the track.</param>
    /// <returns>True if this flag doesn't already exist.</returns>
    private bool NewFlag(Flags new_flag)
    {
      foreach (Flags flag in m_TrackFlags)
      {
        if (flag == new_flag)
        {
          return false;
        }
      }
      return true;
    }

    public override string ToString()
    {
      StringBuilder output = new StringBuilder();

      //write file
      if (m_DataFile.Filename != null && m_DataFile.Filename.Trim() != "")
      {
        output.Append("FILE \"" + m_DataFile.Filename.Trim() + "\" " + m_DataFile.Filetype.ToString() + Environment.NewLine);
      }

      output.Append("  TRACK " + m_TrackNumber.ToString().PadLeft(2, '0') + " " + m_TrackDataType.ToString().Replace('_', '/'));

      //write comments
      foreach (string comment in m_Comments)
      {
        output.Append(Environment.NewLine + "    REM " + comment);
      }

      if (m_Performer.Trim() != "")
      {
        output.Append(Environment.NewLine + "    PERFORMER \"" + m_Performer + "\"");
      }

      if (m_Songwriter.Trim() != "")
      {
        output.Append(Environment.NewLine + "    SONGWRITER \"" + m_Songwriter + "\"");
      }

      if (m_Title.Trim() != "")
      {
        output.Append(Environment.NewLine + "    TITLE \"" + m_Title + "\"");
      }

      //write flags
      if (m_TrackFlags.Length > 0)
      {
        output.Append(Environment.NewLine + "    FLAGS");
      }

      foreach (Flags flag in m_TrackFlags)
      {
        output.Append(" " + flag.ToString().Replace("CH4", "4CH"));
      }

      //write isrc
      if (m_ISRC.Trim() != "")
      {
        output.Append(Environment.NewLine + "    ISRC " + m_ISRC.Trim());
      }

      //write pregap
      if (m_PreGap != null)
      {
        output.Append(Environment.NewLine + "    PREGAP " + m_PreGap.Minutes.ToString().PadLeft(2, '0') + ":" + m_PreGap.Seconds.ToString().PadLeft(2, '0') + ":" + m_PreGap.Frames.ToString().PadLeft(2, '0'));
      }

      //write Indices
      for (int j = 0; j < m_Indices.Length; j++)
      {
        output.Append(Environment.NewLine + "    INDEX " + this[j].Number.ToString().PadLeft(2, '0') + " " + this[j].Position.Minutes.ToString().PadLeft(2, '0') + ":" + this[j].Position.Seconds.ToString().PadLeft(2, '0') + ":" + this[j].Position.Frames.ToString().PadLeft(2, '0'));
      }

      //write postgap
      if (m_PostGap != null)
      {
        output.Append(Environment.NewLine + "    POSTGAP " + m_PostGap.Minutes.ToString().PadLeft(2, '0') + ":" + m_PostGap.Seconds.ToString().PadLeft(2, '0') + ":" + m_PostGap.Frames.ToString().PadLeft(2, '0'));
      }

      return output.ToString();
    }

    #endregion Methods
  }

  /// <summary>
  /// This command is used to specify a data/audio file that will be written to the recorder.
  /// </summary>
  public struct AudioFile
  {
    #region Private Variables
    private string m_Filename;
    private FileType m_Filetype;
    #endregion

    #region Properties
    public string Filename
    {
      get { return m_Filename; }
      set { m_Filename = value; }
    }

    /// <summary>
    /// BINARY - Intel binary file (least significant byte first)
    /// MOTOROLA - Motorola binary file (most significant byte first)
    /// AIFF - Audio AIFF file
    /// WAVE - Audio WAVE file
    /// MP3 - Audio MP3 file
    /// </summary>
    public FileType Filetype
    {
      get { return m_Filetype; }
      set { m_Filetype = value; }
    }
    #endregion

    #region Constructors
    public AudioFile(string filename, string filetype)
    {
      m_Filename = filename;

      switch (filetype.Trim().ToUpper())
      {
        case "BINARY":
          m_Filetype = FileType.BINARY;
          break;
        case "MOTOROLA":
          m_Filetype = FileType.MOTOROLA;
          break;
        case "AIFF":
          m_Filetype = FileType.AIFF;
          break;
        case "WAVE":
          m_Filetype = FileType.WAVE;
          break;
        case "MP3":
          m_Filetype = FileType.MP3;
          break;
        default:
          m_Filetype = FileType.BINARY;
          break;
      }
    }

    public AudioFile(string filename, FileType filetype)
    {
      m_Filename = filename;
      m_Filetype = filetype;
    }
    #endregion

    #region Methods
    #endregion
  }
}
