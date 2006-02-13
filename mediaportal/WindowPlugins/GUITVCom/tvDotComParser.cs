#region Copyright (C) 2005-2006 Team MediaPortal

/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
  /// <summary>
  /// This class implements all the necessary methods to search, download and interpret
  /// episode information from tv.com.
  /// Note that GUIVideo files implements necessary methods which then call this class.
  /// </summary>
  public class tvDotComParser : ISetupForm
  {
    static tvDotComParser()
    {
      try /// PLEASE DON'T DO THAT IN THE CONSTRUCTOR, IT WILL BE CALLED FROM CONFIG!!! (crashes while MP is running!)
      {
        if (System.IO.File.Exists("log/tvcomLog.txt"))
          System.IO.File.Copy("log/tvcomLog.txt", "log/tvcomLog.bak", true);
        writer = new System.IO.StreamWriter("log/tvcomLog.txt", false);
      }
      catch (System.IO.IOException ex)
      {
        Log.Write(ex);
      }
    }

    string folderToSave = "Episode Guides/";
    ////////#####################
    static System.IO.StreamWriter writer;

    public bool getSeasonEpisode(string filename, out int season, out int ep, out string showname)
    {
      if (!getSeasonEpisodeXFormat(filename, out season, out ep, out showname))
        if (!getSeasonEpisodeSEFormat(filename, out season, out ep, out showname))
          if (!getSeasonEpisodeAllNumbersFormat(filename, out season, out ep, out showname))
            return false;
      showname = cleanString(showname.Replace(".", " ").Replace("-", " "));
      return true;
    }


    #region getting Information from Filename

    private bool getSeasonEpisodeAllNumbersFormat(string filename, out int season, out int episode, out string showname)
    {

      // Note this format is the hardest to read out, since all numbers could mean many things in a filename
      // for instance years are often in filenames
      // therefor we check alot more things here than in the other formats

      string pattern3numbers = "[0-9][0-9][0-9]";
      string pattern4numbers = "[0-9][0-9][0-9][0-9]";
      string patternTooLong = "[0-9][0-9][0-9][0-9][0-9]"; // 5 numbers after another dont make sense, but we need to check
      bool secondHit = false;
      string[] numbers = new string[] { "", "", "" }; // first 3 hits are checked, more is really not needed
      string workingString = "";

      season = -1;
      episode = -1;
      showname = "";

      if (Regex.IsMatch(filename, pattern3numbers) && !Regex.IsMatch(filename, patternTooLong))
      {
        try
        {
          workingString = filename;
          for (int i = 0; i < 3; i++)
          {
            numbers[i] = Regex.Match(workingString, pattern4numbers).ToString();
            if (numbers[i].Length < 1)
              numbers[i] = Regex.Match(workingString, pattern3numbers).ToString();
            if (numbers[i].Length > 1)
              workingString = workingString.Replace(numbers[i], "");
          }
          foreach (string number in numbers)
          {
            if (number.Length > 1) // just too see if its empty
            {
              // we have a 4 or 3 number match in the filename
              // we should check a few thigns........the last two digits (episdoes) need to be under lets say 30 (im not aware of any show with more than 30 eps per season)
              // also, if we dont allow more than 18 seasons, all the 19hundred and 2thousand year informations in the title can be eliminated while we still get good coverage for most shows
              // this should largely eliminte problems with years in filenames
              // of course if the user comes across a file where this is the case, he will still get the keyboard and be able to input an x between season and episode and we will recognize it

              int tmpseason = Convert.ToInt32(number.Remove(number.Length - 2, 2));
              int tmpep = Convert.ToInt32(number.Remove(0, number.Length - 2));

              if (tmpseason > 18 || tmpep > 30)
              {
                // means ep>30 or season>18 (which we dont allow in this format
                // also means that previously no good match was found
                // reset them here to what you have inited them
                if (season < 1)
                {
                  season = -1;
                  episode = -1;
                }
              }
              else if (!secondHit)
              {
                if (episode + 1 != tmpep)
                { // if the second hits episode is exactly 1 greater than the first, we dont do anythign
                  // usefull for eg. Friends 923-924.avi (double episode in one file)
                  // in this case we return the first episode's info (Season 9, Episode 23)
                  if (season > 0) // we already have a good match
                    secondHit = true;
                  // we cant allow more than two number with 3or4 digits in a name either, since we wouldnt know which one actually includes the information
                  // example: Sg1 302 - 1969.avi - we wouldnt know if  302 or 1969 was the info
                  // now in the above example...we should be able to check each of the matching numbers and test them all agains the limits we have, and if only one good remains we can take that
                  season = tmpseason;
                  episode = tmpep;
                }
              }
            }
          }
        }
        catch { }
        if (secondHit)
        {
          // we reset season ep to indicate it wasnt succestful
          season = -1;
          episode = -1;
        }


      }
      if (season == -1)
        return false;
      else
      {
        string[] split = Regex.Split(filename, season.ToString() + "[0-9]{0,1}" + episode.ToString());
        showname = split[0];
      }
      return true;
    }


    private bool getSeasonEpisodeXFormat(string filename, out int season, out int episode, out string showname)
    {
      filename = filename.ToLower();

      string pattern = "[0-9]{1,2}x[0-9]{1,2}";
      string match = "";
      if ((match = Regex.Match(filename, pattern).ToString()).Length > 0)
      {
        // we have a match
        string[] split = Regex.Split(match, "x");
        season = Convert.ToInt32(split[0]);
        episode = Convert.ToInt32(split[1]);

        split = Regex.Split(filename, pattern);
        showname = split[0];

        return true;

      }
      else
      {
        season = -1;
        episode = -1;
        showname = "";
        return false;
      }
    }


    private bool getSeasonEpisodeSEFormat(string filename, out int season, out int episode, out string showname)
    {
      filename = filename.ToLower();

      string pattern = "s[0-9]{1,2}[^0-9^a-z]?ep?[0-9]{1,2}";
      string match = "";
      if ((match = Regex.Match(filename, pattern).ToString()).Length > 0)
      {
        // we have a match
        match = match.Replace("s", "");
        string[] split = Regex.Split(match, "[^0-9^a-z]?ep?");
        season = Convert.ToInt32(split[0]);
        episode = Convert.ToInt32(split[1]);

        split = Regex.Split(filename, pattern);
        showname = split[0];
        return true;

      }
      else
      {
        season = -1;
        episode = -1;
        showname = "";
        return false;
      }
    }


    public bool getShownameEpisodeTitleOnly(string filename, out string showname, out string episodeTitle)
    {
      filename = System.IO.Path.GetFileNameWithoutExtension(filename);
      showname = string.Empty;
      episodeTitle = string.Empty;

      // we first check if its a format like recorded by mediaportal
      // if so we will show all episodes of a show and let the user pick
      // examples: 28 FX_Rescue Me_200508310003p25
      // 9 WDRB_Seinfeld_200508141830p2915
      tvComLogWriteline(filename);
      if (Regex.IsMatch(filename, "[0-9]{1,3}\\s[A-Z0-9]{2,}_"))
      {
        tvComLogWriteline("I think this is a MP Naming convention...");
        tvComLogWriteline("Operation will show all Episodes of this show for manual selection!");

        try
        {
          showname = cleanString(Regex.Split(filename, "_")[1]).Trim();
          episodeTitle = string.Empty;
          return true;
        }
        catch
        {
          tvComLogWriteline("Could not get showname from filename");
          return false;
        }


      }

      filename = filename.ToLower();

      if (filename.IndexOf("_") != -1)
      {
        try
        {
          showname = cleanString(Regex.Split(filename, "_")[0]).Trim();
          episodeTitle = cleanString(Regex.Split(filename, "_")[1]);
        }
        catch
        {
          tvComLogWriteline("Could not get EpisodeTitle from filename (tried \"_\")");
          return false;
        }
      }
      else if (filename.IndexOf("-") != -1)
      {
        try
        {
          showname = cleanString(Regex.Split(filename, "-")[0]).Trim();
          episodeTitle = cleanString(Regex.Split(filename, "-")[1]);
        }
        catch
        {
          tvComLogWriteline("Could not get EpisodeTitle from filename (tried \"-\")");
          return false;
        }
      }
      else
      {
        tvComLogWriteline("Could not get EpisodeTitle from filename");
        showname = filename;
        episodeTitle = String.Empty;

      }
      episodeTitle = episodeTitle.Replace("hdtv", " ")
        .Replace("pdtv", " ")
        .Replace("ws", " ")
        .Replace("pdtv", " ")
        .Replace("xvid", " ")
        .Replace("divx", " ")
        .Replace("dsr", " ")
        .Replace("dvdrip", " ")
        .Replace("ac3", " ")
        .Replace("proper", " ")
        .Replace("_", " ")
        .Replace(".", " ");
      episodeTitle = Regex.Replace(episodeTitle, "[a-z]{2,4}\\[.{2,}", " ");
      episodeTitle = Regex.Replace(episodeTitle, "[a-z]{2,4}-.{2,}", " ").Trim();
      return true;
    }


    #endregion

    public bool searchEpisodebyTitle(string showTitle, string subURL, string episodetitle, out System.Collections.SortedList possibleMatches, bool getAll)
    {
      if (!System.IO.Directory.Exists(folderToSave + showTitle))
        System.IO.Directory.CreateDirectory(folderToSave + showTitle);

      possibleMatches = new System.Collections.SortedList();
      string saveListAllSeasons = folderToSave + showTitle + "/" + showTitle + "_episodeList_All_Seasons.htm";
      bool redownload = false;
      System.Net.WebClient Client = new System.Net.WebClient();

      // we first get the guide of season1
      if (!System.IO.File.Exists(saveListAllSeasons) || redownload)
      {
        Client.DownloadFile("http://www.tv.com/" + subURL + "/episode_listings.html&season=0", saveListAllSeasons);
      }
      System.IO.StreamReader r = new System.IO.StreamReader(saveListAllSeasons);
      string line;
      string correctEpisodeTitle;
      int season, ep;
      bool exactMatch = false;

      // we first try if we can get an exact match
      line = jumpStreamUntil(ref r, episodetitle.ToLower().Replace(' ', '-').Trim());
      if (line != "eRRoR" && !getAll)
      {
        // looks like we have a exact match
        try
        {
          line = Regex.Split(line, "/")[4].Replace('-', ' ');
          correctEpisodeTitle = jumpStreamUntil(ref r, "&nbsp;</strong>").Replace("&nbsp;</strong>", "").Trim();
          line = correctEpisodeTitle.ToLower().Replace("the ", "")
            .Replace("a ", "")
            .Replace("an ", "")
            .Replace("to ", "");

          /*
           * 
           * "the" && word != "a" && word != "an" && word != "to" && word != "the" && word != "to"*/
          if (line == episodetitle.ToLower().Replace("the ", "")
            .Replace("a ", "")
            .Replace("an ", "")
            .Replace("to ", ""))
          {
            line = jumpStreamUntil(ref r, " - ");
            line = Regex.Match(line, "\">.+<").ToString().Replace("\">", "").Replace("<", "");
            season = Convert.ToInt32(Regex.Split(line, " - ")[0]);
            ep = Convert.ToInt32(Regex.Split(line, " - ")[1]);

            possibleMatches.Add(1, correctEpisodeTitle + "|" + season.ToString() + "|" + ep.ToString());
            exactMatch = true;
          }

        }
        catch
        {
          season = -1;
          ep = -1;
          tvComLogWriteline("Error in exact match finding....trying partial match");
          //return false;
        }
      }
      if (!exactMatch)
      {

        // we match differently here (dont use the helper method)
        // the reason is we want to be more forgiving wiht partial matches
        // so for instance we want "male unbonding (2)" to match the episode "male unbonding"
        // you never know what people write into their filenames, and we want to try at least

        // we need to reopen the stream
        r = new System.IO.StreamReader(saveListAllSeasons);

        // we get each of the episodetitles listed, and see if we can match it up
        string[] titleSplit = Regex.Split(episodetitle, " ");
        string[] split2;

        int possibleMatch = 0; // we sort them by the number of words matched (and the lenght of the word)

        while (r.Peek() >= 0)
        {
          possibleMatch = 0;
          line = r.ReadLine();
          if ((line = jumpStreamUntil(ref r, "/episode/")) != "eRRoR")
          {
            line = Regex.Split(line, "/")[4].Replace('-', ' ');

            if (!getAll)
            {
              // version 1: string in guessed title is actually substring in real title
              foreach (string word in titleSplit)
              {
                if (word != "the" && word != "a" && word != "an" && word != "to")
                {
                  if (Regex.IsMatch(line, word))
                    possibleMatch += word.Length;
                }
              }

              // version 2: string in actual title is substring in guessed title
              split2 = Regex.Split(line, " ");
              foreach (string word in split2)
              {
                if (word != "the" && word != "a" && word != "an" && word != "to")
                {
                  if (Regex.IsMatch(episodetitle, word))
                    possibleMatch += word.Length;
                }
              }
            }

            if (possibleMatch > 0 || getAll)
            {
              // means we could match it
              correctEpisodeTitle = jumpStreamUntil(ref r, "&nbsp;</strong>").Replace("&nbsp;</strong>", "").Trim();
              line = jumpStreamUntil(ref r, " - ");
              if (line.ToLower() != "error")
              {
                line = Regex.Match(line, "\">.+<").ToString().Replace("\">", "").Replace("<", "");

                try
                {
                  season = Convert.ToInt32(Regex.Split(line, " - ")[0]);
                  ep = Convert.ToInt32(Regex.Split(line, " - ")[1]);
                  if (!getAll)
                    possibleMatches.Add(possibleMatch * 10000 + (100 - season) * 100 + 100 - ep, correctEpisodeTitle + "|" + season.ToString() + "|" + ep.ToString());
                  else
                    possibleMatches.Add(season * 100 + ep, correctEpisodeTitle + "|" + season.ToString() + "|" + ep.ToString());
                }
                catch
                {
                  tvComLogWriteline("Error getting interpreting List of Episodes, probably this show's episodes aren't properly organized into Seasons/Episodes!");
                  return false;
                }

              }
              else
                tvComLogWriteline("End of List reached... - Note this probably means that the List was not downloaded completely, please manually delete the file to force a redownload!");
            }
          }
          else
            break;
        }
      }

      return true;


    }


    /// <summary>
    /// Downloads the Printable EpisodeGuide (all episodes) and the EpisodeList of a particular Season from TV.com and saves them locally for later use
    /// (Only if the files don't already exist locally)
    /// TODO: Edit correct Save Paths!
    /// </summary>
    /// <param name="subURL">string, as received from the searchResults</param>
    /// <param name="season">needed to download the correct epList</param>
    /// <param name="showTitle">Only needed to Save, make sure no special characters</param>
    /// <param name="redownload">To tell the system to redownload - used in case episode was not found in this guide</param>
    /// <returns>true if succesfull</returns>
    public bool downloadGuides(string subURL, int season, int ep, string showTitle, bool redownload)
    {
      if (!System.IO.Directory.Exists(folderToSave + showTitle))
        System.IO.Directory.CreateDirectory(folderToSave + showTitle);

      string saveGuide = folderToSave + showTitle + "/" + showTitle + "_episodeGuide_Season_" + season.ToString() + "_Page_" + getGuidePageFromEpisodeNo(ep).ToString() + ".htm";
      //string saveList = folderToSave + showTitle + "/" + showTitle + "_episodeList_Season_" + season.ToString() + ".htm";
      string saveSummary = folderToSave + showTitle + "/" + showTitle + "_Summary.htm";

      System.Net.WebClient Client = new System.Net.WebClient();
      bool freshlyDownloaded = false;
      try
      {

        if (!System.IO.File.Exists(saveGuide) || redownload)
        {
          Client.DownloadFile("http://www.tv.com/" + subURL + "/episode_guide.html&season=" + season.ToString() + "&pg_episodes=" + getGuidePageFromEpisodeNo(ep).ToString(), saveGuide);
          freshlyDownloaded = true;
        }
        else
          tvComLogWriteline(saveGuide + " already exists, skipping download");
        // **********
        if (!System.IO.File.Exists(saveSummary))// || redownload)
        {
          //Client.DownloadFile("http://www.tv.com/" + subURL + "/episode_listings.html&season=" + season.ToString(), saveList);
          // if we downloaded a new episode list (normally at a new season) we also redownload the summary for updated info
          Client.DownloadFile("http://www.tv.com/" + subURL + "/summary.html&full_summary=1", saveSummary);
        }
        else
          tvComLogWriteline(saveSummary + " already exists, skipping download");
      }
      catch { return false; };
      return freshlyDownloaded;
    }


    private int getGuidePageFromEpisodeNo(int ep)
    {
      return (int)ep / 25 + 1;
    }


    /// <summary>
    /// This method looks at the contents of the file "offsets" to see if any offsets where specified for the
    /// current show and season.
    /// This is useful if specials are listed as normal episodes on tv.com (eg. Lost - Season 1 Episode 1)
    /// </summary>
    /// <param name="showName">Name of the Show</param>
    /// <param name="season">Current Season</param>
    /// <param name="ep">Number of Episode</param>
    /// <returns></returns>
    private int getOffset(string showName, int season, int ep)
    {

      int offSet = 0;
      if (System.IO.File.Exists(folderToSave + "offsets"))
      {
        System.IO.StreamReader or = new System.IO.StreamReader(folderToSave + "offsets");

        string line, all = "";
        while ((line = or.ReadLine()) != null)
          all += line;

        or.Close();

        string[] lines = Regex.Split(all, "\n");


        tvComLogWriteline("Offset file found, searching for matches...");

        for (int i = 0; i < lines.Length; i++)
        {
          string[] elems = Regex.Split(lines[i], ";");
          if (elems[0].ToLower().Equals(showName.ToLower()) && elems[1].Equals(season.ToString()))
          {
            if (ep >= Convert.ToInt32(elems[2]))
            {
              tvComLogWriteline("Found offset...");
              if (elems.Length < 4)
              {
                offSet++;
              }
              else
              {
                offSet += Convert.ToInt32(elems[3]);
              }
            }
          }
        }
        if (offSet > 0)
          tvComLogWriteline("Total Offset: " + offSet.ToString());
        else
          tvComLogWriteline("No Offsets found...");
      }


      return offSet;
    }


    /// <summary>
    /// Downloads the first Image for the Show from TV.com and places it into the thumbs/videos folder
    /// this one is called directly from the getEpisodeInfo method and is thus private
    /// </summary>
    /// <param name="thumbURL">url of the thumb image on the summary page</param>
    /// <param name="showTitle">name of the show</param>
    /// <returns>true if downloaded succesfully or already existant, otherwise false</returns>
    private bool downloadPicture(string thumbURL, string showTitle)
    {
      // Picture

      string saveAs = "thumbs/videos/title/" + showTitle + ".jpg";

      if (!System.IO.File.Exists(saveAs))
      {
        try
        {
          System.Net.WebClient Client = new System.Net.WebClient();
          tvComLogWriteline("Trying to download Image");
          Client.DownloadFile(thumbURL, saveAs);
          Client.DownloadFile(thumbURL.Replace("thumb", "photo_viewer"), saveAs.Replace(".jpg", "L.jpg"));
          tvComLogWriteline("Downloaded Image sucessfully!");

        }
        catch
        {
          tvComLogWriteline("Error downloading Image (parsed wrong URL?)");
          tvComLogWriteline("The URL was: " + thumbURL);
          return false;
        }

        return true;
      }
      else
      {
        tvComLogWriteline("Image exists, no need to redownload!");
        return true;
      }
    }


    /// <summary>
    /// Searches TV.com for TVShows matching the paramter and returns the results (showtitles and subURLs) as a string[]
    /// </summary>
    /// <param name="title"></param>
    /// <returns>SearchResults as a string[] where the ShowTitle is followed by the subURL for that Show</returns>
    public string[] getSearchResultsFromTitle(string title)
    {
      string[] searchResults;
      System.Collections.ArrayList results = new System.Collections.ArrayList();
      string line = "";
      int numberResults = 0;
      title = title.Replace(" ", "%20"); // spaces naturally dont work in URLs
      // we get the stream from the website
      System.Net.WebClient Client = new System.Net.WebClient();
      System.IO.Stream strm = Client.OpenRead("http://www.tv.com/search.php?type=11&stype=program&qs=" + title);
      tvComLogWriteline("Downloaded results, now parsing...");
      System.IO.StreamReader sr = new System.IO.StreamReader(strm);

      try
      {
        while (line.IndexOf("Results for \"") == -1 && line.IndexOf("Result for \"") == -1)
        {
          line = sr.ReadLine();
        }
      }
      catch (Exception)
      {
        return new string[0];
      }

      line = line.Remove(0, line.IndexOf(">") + 1);
      line = line.Remove(line.IndexOf(" "), line.Length - line.IndexOf(" "));
      if (line.Length > 0)
      {
        numberResults = Int32.Parse(line); // ok we have the number of results
      }

      jumpStreamUntil(ref sr, "<div class=\"divider\"></div>");

      int count = 0;

      // we go through each result, get its title and its subURL and add it to the resultset
      while (++count <= numberResults)
      {
        line = jumpStreamUntil(ref sr, "<a href=");
        if (line.IndexOf("<img") == -1)
        {
          string[] split = System.Text.RegularExpressions.Regex.Split(line, "<|>");
          if (split.Length > 4 && split[4].Length > 0)
          {
            results.Add(cleanString(split[4]));
            results.Add(Regex.Split(split[1].Replace("a href=\"http://www.tv.com/", ""), "/sum")[0]);
          }
        }
        else
          count--;

      }
      sr.Close();

      searchResults = new string[results.Count];
      for (int i = 0; i < results.Count; i++)
      {
        searchResults[i] = (string)results[i];
        //tvComLogWriteline(searchResults[i]);
      }

      tvComLogWriteline("Parsing of SearchResults complete!");
      return searchResults;
    }


    /// <summary>
    /// Parses Episode Information from the seasons EpList and the epGuide and
    /// returns an episode_info object containing all the info
    /// </summary>
    /// <param name="showTitle">The exact name of the show as handed back from the Searchresults</param>
    /// <param name="seasonNumber"></param>
    /// <param name="episodeNumber"></param>
    /// <returns>an episode_info object containing the parsed information</returns>
    public episode_info getEpisodeInfo(string showTitle, int seasonNumber, int episodeNumber)
    {

      //we open the streams

      // the big guide with all the episode infos
      System.IO.StreamReader seasonEpisodeGuideStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_episodeGuide_Season_" + seasonNumber.ToString() + "_Page_" + getGuidePageFromEpisodeNo(episodeNumber).ToString() + ".htm");
      // the seasons ep list that we need to calculate the global ep number
      //System.IO.StreamReader showPrintableStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_episodeGuide.htm");
      // the summary guide with general show info
      System.IO.StreamReader showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");

      episode_info episodeInfo = new episode_info();

      episodeInfo.seasonNumber = seasonNumber;
      episodeInfo.episodeNumber = episodeNumber;
      string line = "";

      try
      {
        // PART 1: Episode Specific Information

        // how many eps down is it on this page of the guide                
        int epCount = (getGuidePageFromEpisodeNo(episodeNumber) - 1) * 24;

        // offset
        epCount -= getOffset(showTitle, seasonNumber, episodeNumber);

        //************** Episode Title

        try
        {
          while (epCount++ < episodeNumber)
          {
            line = jumpStreamUntil(ref seasonEpisodeGuideStream, "class=\"f-big\"");
          }
          if (line.ToLower() == "error")
            throw new Exception("Could not locate Episode!");
          line = jumpStreamUntil(ref seasonEpisodeGuideStream, "</a>");
          // now we are in the correct line;
          episodeInfo.title = Regex.Split(line, "</a>")[0].Trim();

        }
        catch
        {
          // apparently we werent in the correct line
          // we cant continue
          tvComLogWriteline("Could not locate this episode! (Or a parsing Error Occured)");
          tvComLogWriteline("Are you sure this episode exists?");
          throw;
        }

        // needed for some conversions
        System.Globalization.CultureInfo usCulture = new System.Globalization.CultureInfo("en-US");


        //************** AirDate
        try
        {
          line = jumpStreamUntil(ref seasonEpisodeGuideStream, "First aired:");
          // we use regular expressions to get the date in this format:
          // "07/27/1997"
          line = System.Text.RegularExpressions.Regex.Match(line, "[0-9]{1,2}/[0-9]{1,2}/[0-9]{4}").ToString();
          episodeInfo.firstAired = DateTime.Parse(line.Trim(), usCulture);
        }
        catch
        {
          tvComLogWriteline("Error Parsing First Aired Info....Skipping");
        }

        //************** Writer
        jumpStreamUntil(ref seasonEpisodeGuideStream, "Writer:");
        // next line hold the writers name
        line = seasonEpisodeGuideStream.ReadLine();
        if (line.IndexOf("/person/") != -1)
        {
          // otherwise there is no info on the writer
          try
          {
            episodeInfo.writer = Regex.Split(line, "<|>")[2].Trim();
          }
          catch
          {
            tvComLogWriteline("Error Parsing Writer, Skipping.");
          }
        }

        //************** Director
        jumpStreamUntil(ref seasonEpisodeGuideStream, "Director:");
        // next line hold the writers name
        line = seasonEpisodeGuideStream.ReadLine();
        if (line.IndexOf("/person/") != -1)
        {
          // otherwise there is no info on the writer
          try
          {
            episodeInfo.director = Regex.Split(line, "<|>")[2].Trim();
          }
          catch
          {
            tvComLogWriteline("Error Parsing Director, Skipping.");
          }
        }

        //************** Guest Stars

        jumpStreamUntil(ref seasonEpisodeGuideStream, ">Guest star:</span>");
        line = seasonEpisodeGuideStream.ReadLine();

        string[] split = Regex.Split(line.Replace("</a>", ""), "<|>");
        string tmp;
        foreach (string s in split)
        {
          tmp = s.Trim();
          if (tmp.Length > 0 && tmp.IndexOf("href") == -1 && tmp.IndexOf("br /") == -1)
          {
            string[] split2 = Regex.Split(tmp, "\\(");
            try
            {
              //tvComLogWriteline(tmp);
              episodeInfo.guestStarsCharacters.Add(split2[1].Replace("),", "").Trim());
              episodeInfo.guestStars.Add(split2[0].Trim());
            }
            catch (Exception)
            {
              tvComLogWriteline("Error Parsing at least one of the Guest Stars, Skipping.");
            }
          }
        }
        // removing the ")" from the last gs Character
        if (episodeInfo.guestStarsCharacters.Count > 0)
        {
          line = (string)episodeInfo.guestStarsCharacters[episodeInfo.guestStarsCharacters.Count - 1];
          line = line.TrimEnd(')');
          episodeInfo.guestStarsCharacters[episodeInfo.guestStarsCharacters.Count - 1] = line;
        }

        // ************* Episode Plot

        // one line down from guest stars
        line = seasonEpisodeGuideStream.ReadLine();

        do
        {
          line = line.Replace("<BR />", "\n").Replace("<br />", "\n");
          episodeInfo.description += Regex.Replace(line, @"<(.|\n)*?>", "").Trim();
          if (line.IndexOf("</p>") != -1)  // we loop until the paragraph ends
          {
            break;
          }
          episodeInfo.description += "\n";
          line = seasonEpisodeGuideStream.ReadLine();
        } while (true);


        //************** Rating


        line = jumpStreamUntil(ref seasonEpisodeGuideStream, "com_score");
        seasonEpisodeGuideStream.ReadLine();
        line = seasonEpisodeGuideStream.ReadLine();

        line = System.Text.RegularExpressions.Regex.Match(line, "[0-9]+\\.[0-9]*").ToString();


        try
        {
          if (line.Length > 1)
          {
            episodeInfo.rating = Double.Parse(line, usCulture.NumberFormat);

            // *** we also get the number of ratings
            line = jumpStreamUntil(ref seasonEpisodeGuideStream, "Ratings");
            line = Regex.Match(line, "[0-9]+").ToString();
            if (line.Length > 1)
            {
              episodeInfo.numberOfRatings = Int32.Parse(line);
            }

          }
        }
        catch
        {
          tvComLogWriteline("Error Parsing the Rating or number of Ratings, Skipping.");
        }


        // PART 2: General Show info

        line = jumpStreamUntil(ref showSummaryStream, "thumb");
        if (line != "eRRoR")
        {
          try
          {
            if (!downloadPicture(Regex.Split(line, "<img src=\"|\" alt")[1].Trim(), showTitle))
            {
              // no image found, we reopen the stream so it can still get the rest of the info
              showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
            }
          }
          catch (Exception e1)
          {
            tvComLogWriteline("Error at Parsing ImageURL");
            tvComLogWriteline(e1.Message);

          }
        }
        else
        {
          // no image found, we reopen the stream so it can still get the rest of the info
          showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
        }

        // ********** Currently Airs (or originally):
        try
        {
          line = jumpStreamUntil(ref showSummaryStream, "<span class=\"f-bold\">Airs:");
          episodeInfo.airtime = Regex.Split(line, "<|>")[4].Trim();

        }
        catch (Exception e2)
        {
          if (line.ToLower() == "error")
          {
            // probably reads originally instead of airs
            showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
            if ((line = jumpStreamUntil(ref showSummaryStream, "Originally").ToLower()) == "error")
            {
              // again an error here is not good
              // we try to open the stream again to be on the top again, but the rest probably wont work either
              showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
              tvComLogWriteline("Error at Parsing Originally");
              tvComLogWriteline(e2.Message);

            }
          }
          else
          {
            try
            {
              // airtime one line lower
              line = showSummaryStream.ReadLine();
              episodeInfo.airtime = Regex.Split(line, "<|>")[2].Trim();
            }
            catch (Exception e22)
            {
              showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
              tvComLogWriteline("Error at Parsing Currently airs");
              tvComLogWriteline(e22.Message);
            }
          }


        }

        // ********** Network:
        try
        {
          line = jumpStreamUntil(ref showSummaryStream, "on <span class=\"f-bold\">");
          episodeInfo.network = Regex.Split(line, ">")[1].Trim();

        }
        catch (Exception e3)
        {
          showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
          tvComLogWriteline("Error at Parsing Network");
          tvComLogWriteline(e3.Message);
        }
        // ********** runtime:
        line = jumpStreamUntil(ref showSummaryStream, " mins)");
        try
        {
          episodeInfo.runtime = Convert.ToInt32(Regex.Split(line, "\\(| mins\\)")[1]);
        }
        catch (Exception e4)
        {
          showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
          tvComLogWriteline("Error at Parsing runtime");
          tvComLogWriteline(e4.Message);
        }

        // ********** status:
        try
        {
          line = jumpStreamUntil(ref showSummaryStream, "Status: ");
          episodeInfo.status = Regex.Split(line, "Status: ")[1].Trim();

        }
        catch (Exception e5)
        {
          showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
          tvComLogWriteline("Error at Parsing status");
          tvComLogWriteline(e5.Message);
        }

        // ********** series premiere:
        line = jumpStreamUntil(ref showSummaryStream, "Premiered ");
        try
        {
          episodeInfo.seriesPremiere = DateTime.Parse(Regex.Split(line, "Premiered ")[1].Trim());
        }
        catch (Exception e6)
        {
          showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
          tvComLogWriteline("Error at Parsing series premiere");
          tvComLogWriteline(e6.Message);
        }

        // ********** Genre:
        try
        {
          line = jumpStreamUntil(ref showSummaryStream, "/genre/");
          do
          {
            if (line.IndexOf("/genre/") != -1)
              episodeInfo.genre += "/" + Regex.Split(line, "<|>")[2].Trim();
          } while ((line = showSummaryStream.ReadLine()).IndexOf("<br") == -1);
          episodeInfo.genre = episodeInfo.genre.Substring(1);
          try
          {
            // ******** Show Description
            jumpStreamUntil(ref showSummaryStream, "<p class=");
            showSummaryStream.ReadLine();
            line = "";
            while (line.IndexOf("</p") == -1 && showSummaryStream.Peek() >= 0)
              line += showSummaryStream.ReadLine() + "\n";

            line = Regex.Replace(line.Replace("<br />", "\n").Replace("<BR />", "\n"), @"<(.|\n)*?>", "");
            episodeInfo.seriesDescription = line.Trim();

          }
          catch (Exception e8)
          {
            showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
            tvComLogWriteline("Error at Parsing general series desc");
            tvComLogWriteline(e8.Message);
          }

        }
        catch (Exception e7)
        {
          showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
          tvComLogWriteline("Error at Parsing genre, have to skip series description");
          tvComLogWriteline(e7.Message);
        }


        // *********** Regular Cast

        jumpStreamUntil(ref showSummaryStream, "<h3>Cast and Crew</h3>");

        try
        {
          while ((line = jumpStreamUntil(ref showSummaryStream, "summary.html")).IndexOf("person") != -1)
          {
            split = Regex.Split(line, "\">|>");
            episodeInfo.stars.Add(split[1].Replace("</a", "").Trim());
            line = jumpStreamUntil(ref showSummaryStream, "<br/>");
            split = split = Regex.Split(line, ">");
            episodeInfo.starsCharacters.Add(split[1].Trim());
          }
        }
        catch (Exception e9)
        {
          tvComLogWriteline("Error at Parsing regular cast");
          tvComLogWriteline(e9.Message);
        }

      }
      catch (Exception ex)
      {

        tvComLogWriteline("There was an error Parsing the information");
        tvComLogWriteline(ex.Message);
        throw ex;
      }
      finally
      {
        seasonEpisodeGuideStream.Close();
        //showPrintableStream.Close();
        showSummaryStream.Close();
      }
      return episodeInfo;
    }


    /// <summary>
    /// Helper method to quickly jump down in open Streams
    /// </summary>
    /// <param name="reader">which stream to work with</param>
    /// <param name="until">where to stop?</param>
    /// <returns>string with the line at which "until" is found</returns>		
    private string jumpStreamUntil(ref System.IO.StreamReader reader, string until)
    {
      string line;
      try
      {
        while ((line = reader.ReadLine()).IndexOf(until) == -1) ;
      }
      catch (Exception)
      {
        return "eRRoR";
      }
      return line;
    }


    public string cleanString(string s)
    {
      foreach (char c in System.IO.Path.GetInvalidPathChars())
        s = s.Replace(c, ' ');
      return s.Replace('.', ' ')
        .Replace(": ", " ")
        .Replace("/", " ")
        .Replace("-", " ")
        .Replace("_", " ")
        .Replace("[", " ")
        .Replace("]", " ")
        .Replace("(", " ")
        .Replace(")", " ")
        .Replace("?", " ")
        .Replace("!", " ")
        .Replace("\\", " ")
        .Replace(";", " ")
        .Replace("   ", " ")
        .Replace("  ", " ")
        .Trim();
    }


    public string[] searchMapping(string shownameGuess)
    {
      try
      {
        string line;
        System.IO.StreamReader mappingsReader = new System.IO.StreamReader(folderToSave + "mappings.csv");
        do
        {
          if ((line = jumpStreamUntil(ref mappingsReader, shownameGuess)) == "eRRoR")
          {
            mappingsReader.Close();
            return new string[] { "-1" };
          }
          else
          {
            string[] split = Regex.Split(line, ";");
            if (split[0] == shownameGuess)
            {
              mappingsReader.Close();
              return new string[] { split[1], split[2] };
            }
          }
        } while (true);

      }
      catch { return new string[] { "-1" }; }

    }


    public void writeMapping(string shownameGuess, string realShowname, string subURL)
    {
      System.IO.StreamWriter mappingsWriter = new System.IO.StreamWriter(folderToSave + "mappings.csv", true);
      mappingsWriter.WriteLine(shownameGuess + ";" + realShowname + ";" + subURL);
      tvComLogWriteline("Writing new mapping: " + shownameGuess + ";" + realShowname + ";" + subURL);
      mappingsWriter.Close();
    }


    public static void tvComLogWritelineStatic(string line)
    {

      writer.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + " - " + line);
      writer.Flush();

    }


    public void tvComLogWriteline(string line)
    {

      writer.WriteLine(System.DateTime.Now.TimeOfDay.ToString() + " - " + line);
      writer.Flush();

    }

    public string getFilennameFriendlyString(string input)
    {
      foreach (char c in System.IO.Path.GetInvalidPathChars())
      {
        input = input.Replace(c, ' ');
      }
      input = input.Replace("  ", " ").Trim();
      return input;
    }
    #region ISetupForm Members

    // Returns the name of the plugin which is shown in the plugin menu
    public string PluginName()
    {
      return "TV.com Parser";
    }

    // Returns the description of the plugin is shown in the plugin menu
    public string Description()
    {
      return "Downloads and displays episode information for recorded TV in MediaPortal";
    }

    // Returns the author of the plugin which is shown in the plugin menu
    public string Author()
    {
      return "Inker";
    }

    // show the setup dialog
    public void ShowPlugin()
    {
      tvComSetupForm setupForm = new tvComSetupForm();
      setupForm.ShowDialog();
    }

    // Indicates whether plugin can be enabled/disabled
    public bool CanEnable()
    {
      return false;
    }

    // get ID of windowplugin belonging to this setup
    public int GetWindowId()
    {
      return 5678;
    }

    // Indicates if plugin is enabled by default;
    public bool DefaultEnabled()
    {
      return true;
    }



    // indicates if a plugin has its own setup screen
    public bool HasSetup()
    {
      return true;
    }

    /// <summary>
    /// If the plugin should have its own button on the main menu of MediaPortal then it
    /// should return true to this method, otherwise if it should not be on home
    /// it should return false
    /// </summary>
    /// <param name="strButtonText">text the button should have</param>
    /// <param name="strButtonImage">image for the button, or empty for default</param>
    /// <param name="strButtonImageFocus">image for the button, or empty for default</param>
    /// <param name="strPictureImage">subpicture for the button or empty for none</param>
    /// <returns>true  : plugin needs its own button on home
    ///          false : plugin does not need its own button on home</returns>
    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
    {
      strButtonText = String.Empty;
      strButtonImage = String.Empty;
      strButtonImageFocus = String.Empty;
      strPictureImage = String.Empty;
      return false;
    }
    #endregion
  }
}
