using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace MediaPortal.GUI.Video
{
	/// <summary>
	/// Zusammenfassung für tvDotComParser.
	/// </summary>
	public class tvDotComParser : ISetupForm
	{
		public tvDotComParser()
		{

		}

		string folderToSave = "Episode Guides/";
		////////#####################
		static System.IO.StreamWriter writer = new System.IO.StreamWriter("log/tvcomLog.txt", false);

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

		#endregion


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
		public bool downloadGuides(string subURL, int season, string showTitle, bool redownload)
		{
			if (!System.IO.Directory.Exists(folderToSave + showTitle))
				System.IO.Directory.CreateDirectory(folderToSave + showTitle);

			string saveGuide = folderToSave + showTitle + "/" + showTitle + "_episodeGuide.htm";
			string saveList = folderToSave + showTitle + "/" + showTitle + "_episodeList_Season_" + season.ToString() + ".htm";
			string saveSummary = folderToSave + showTitle + "/" + showTitle + "_Summary.htm";

			System.Net.WebClient Client = new System.Net.WebClient();
			bool freshlyDownloaded = false;
			try
			{

				if (!System.IO.File.Exists(saveGuide) || redownload)
				{
					Client.DownloadFile("http://www.tv.com/" + subURL + "/episode_guide.html&printable=1", saveGuide);
					freshlyDownloaded = true;
				}
				else
					tvComLogWriteline(saveGuide + " already exists, skipping download");
				// **********
				if (!System.IO.File.Exists(saveList) || redownload)
				{
					Client.DownloadFile("http://www.tv.com/" + subURL + "/episode_listings.html&season=" + season.ToString(), saveList);
					// if we downloaded a new episode list (normally at a new season) we also redownload the summary for updated info
					Client.DownloadFile("http://www.tv.com/" + subURL + "/summary.html&full_summary=1", saveSummary);
				}
				else
					tvComLogWriteline(saveList + " already exists, skipping download");
			}
			catch { return false; };
			return freshlyDownloaded;
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
				while(line.IndexOf("Results for \"") == -1 && line.IndexOf("Result for \"") == -1)
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
			System.IO.StreamReader seasonEpisodeListStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_episodeList_Season_" + seasonNumber.ToString() + ".htm");
			// the seasons ep list that we need to calculate the global ep number
			System.IO.StreamReader showPrintableStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_episodeGuide.htm");
			// the summary guide with general show info
			System.IO.StreamReader showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");

			episode_info episodeInfo = new episode_info();

			episodeInfo.seasonNumber = seasonNumber;
			episodeInfo.episodeNumber = episodeNumber;
			string line;

			try
			{
				// PART 1: Getting the global episode number that we need later to get the info
				// we go through down the episode list stream until the episodes begin
				// edit: we also need to get the prod # here since its not in the print guide

				line = jumpStreamUntil(ref seasonEpisodeListStream, "Reviews / Score");
				// we are now here
				//  Reviews / Score
				//  </th>
				//  </tr>
				// 
				//         
				//  <tr class="tr-alt">
				//  <td class="p-5" style="border-bottom:1px solid #CDD5E0;">
				//  <a href="http://www.tv.com/stargate-sg-1/children-of-the-gods-1/episode/7319/summary.html">
				//
				//
				//                                                    1:                        
				//                            
				//               
				// Children of the Gods (1)&nbsp;</strong>
				// </td>
				// we only need to get the global show number (1:) from this file
				int epCount = 0;
				while (epCount++ < episodeNumber)
				{
					line = jumpStreamUntil(ref seasonEpisodeListStream, ": ");
				}
				// no we are in the correct line;
				line = line.Replace(" ", "").Replace(":", "");
				// line holds the show global episode number
				try
				{
					episodeInfo.globalNumber = Convert.ToInt32(line);
				}
				catch (Exception e)
				{
					// apparently we werent in the correct line
					// we cant continue without this number
					tvComLogWriteline("Could not locate this episode!");
					tvComLogWriteline("Are you sure this episode exists?");
					throw e;
				}

				// now prod # which is 9 lines down
				for (int i = 0; i < 8; i++)
					seasonEpisodeListStream.ReadLine();
				line = seasonEpisodeListStream.ReadLine();
				episodeInfo.prodNumber = line.Replace("</td>", "").Trim();

				// PART 2: Getting the actual episode information

				// needed for some conversions
				System.Globalization.CultureInfo usCulture = new System.Globalization.CultureInfo("en-US");
				//************** TITLE
				line = jumpStreamUntil(ref showPrintableStream, "   " + episodeInfo.globalNumber.ToString());
				showPrintableStream.ReadLine();
				line = showPrintableStream.ReadLine().Replace("  ", ""); ;
				// now we are in the correct part of the file (episode title)
				episodeInfo.title = line;
				//************** AirDate
				line = jumpStreamUntil(ref showPrintableStream, ">First aired:</span>");
				// we use regular expressions to get the date in this format:
				// "07/27/1997"
				line = System.Text.RegularExpressions.Regex.Match(line, "[0-9]{1,2}/[0-9]{1,2}/[0-9]{4}").ToString();
				// we now hold the airdate
				try
				{
					if (line.Length > 1)
					{
						episodeInfo.firstAired = DateTime.Parse(line, usCulture.DateTimeFormat);
					}
				}
				catch (Exception e)
				{
					// apparently we werent in the correct line
					throw e;
				}
				//************** Writer
				// we continue in the same manner
				line = jumpStreamUntil(ref showPrintableStream, ">Writer:</span>");
				line = showPrintableStream.ReadLine();

				string[] split = System.Text.RegularExpressions.Regex.Split(line, "html\">|</a>");
				foreach (string s in split)
				{
					if (s.IndexOf("<") == -1)
						episodeInfo.writer += s.Trim() + ", ";
				}
				if (episodeInfo.writer.Length > 0)
					episodeInfo.writer = episodeInfo.writer.Remove(episodeInfo.writer.LastIndexOf(","), episodeInfo.writer.Length - episodeInfo.writer.LastIndexOf(","));

				//************** Director

				line = jumpStreamUntil(ref showPrintableStream, ">Director:</span>");
				line = showPrintableStream.ReadLine();

				split = System.Text.RegularExpressions.Regex.Split(line, "html\">|</a>");
				foreach (string s in split)
				{
					if (s.IndexOf("<") == -1)
						episodeInfo.director += s.Trim() + ", ";
				}
				if (episodeInfo.director.Length > 0)
					episodeInfo.director = episodeInfo.director.Remove(episodeInfo.director.LastIndexOf(","), episodeInfo.director.Length - episodeInfo.director.LastIndexOf(","));

				//************** Guest Stars

				line = jumpStreamUntil(ref showPrintableStream, ">Guest star:</span>");
				line = showPrintableStream.ReadLine();


				split = System.Text.RegularExpressions.Regex.Split(line.Replace("</a>",""), "<|>");
				string tmp;
				foreach (string s in split)
				{
					tmp = s.Trim();
					if (tmp.Length > 0 && tmp.IndexOf("href") == -1 && tmp.IndexOf("br /") == -1)
					{
						string[] split2 = Regex.Split(tmp,"\\(");
						try
						{
							episodeInfo.guestStarsCharacters.Add(split2[1].Replace("),", "").Replace(")","").Trim());
							episodeInfo.guestStars.Add(split2[0].Trim());
						}
						catch (Exception)
						{
							tvComLogWriteline("Error Parsing Guest Stars");
						}
					}
				}

				//************** Rating


				line = jumpStreamUntil(ref showPrintableStream, ">Global rating:</span>");
				line = showPrintableStream.ReadLine();

				line = System.Text.RegularExpressions.Regex.Match(line, "[0-9]+\\.[0-9]*").ToString();


				try
				{
					if (line.Length > 1)
					{
						episodeInfo.rating = Double.Parse(line, usCulture.NumberFormat);
					}
				}
				catch (Exception e)
				{
					throw e;
				}

				//************** Description

				line = jumpStreamUntil(ref showPrintableStream, "<p>") + "\n";
				while (line.IndexOf("</div>") == -1)
					line += showPrintableStream.ReadLine() + "\n";

				episodeInfo.description = line.Replace("</div>","").Replace("<b>", "").Replace("</b>", "").Replace("<p>", "").Replace("</p>", "\n\n").Replace("<i>", "").Replace("</i>", "").Replace("<br />", "").Trim();


				// PART 3: General Show info

				line = jumpStreamUntil(ref showSummaryStream, "thumb");
				if (line != "eRRoR")
				{
					//tvComLogWriteline(line);
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
					if(line.ToLower() == "error")
					{
						// probably reads originally instead of airs
						showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");
						if((line = jumpStreamUntil(ref showSummaryStream, "Originally").ToLower()) == "error")
						{
							// again an error here is not good
							// we try to open the stream again to be on the top again, but the rest probably wont work either
							showSummaryStream = new System.IO.StreamReader(folderToSave + showTitle + "/" + showTitle + "_Summary.htm");

						}
					}
					tvComLogWriteline("Error at Parsing Currently airs");
					tvComLogWriteline(e2.Message);
				}

				// ********** Network:
				try
				{
					line = jumpStreamUntil(ref showSummaryStream, "on <span class=\"f-bold\">");
					episodeInfo.network = Regex.Split(line, ">")[1].Trim();

				}
				catch (Exception e3)
				{
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
					tvComLogWriteline("Error at Parsing series premiere");
					tvComLogWriteline(e6.Message);
				}

				// ********** Genre:
				try
				{
					line = jumpStreamUntil(ref showSummaryStream, "/genre/");
					episodeInfo.genre = Regex.Split(line, "<|>")[2].Trim();

				}
				catch (Exception e7)
				{
					tvComLogWriteline("Error at Parsing genre");
					tvComLogWriteline(e7.Message);
				}
				// ********** Series Descrp
				try
				{
					jumpStreamUntil(ref showSummaryStream, "<p class=");
					showSummaryStream.ReadLine();
					line = "";
					while (line.IndexOf("</p") == -1)
						line += showSummaryStream.ReadLine() + "\n";

					episodeInfo.seriesDescription = line.Replace("<p>", "").Replace("</p>", "").Replace("<i>", "").Replace("</i>", "").Replace("<br />", "").Trim();

				}
				catch (Exception e8)
				{
					tvComLogWriteline("Error at Parsing general series desc");
					tvComLogWriteline(e8.Message);
				}

				// *********** Regular Cast

				jumpStreamUntil(ref showSummaryStream, "<h3>Cast and Crew</h3>");

				try
				{
					while ((line = jumpStreamUntil(ref showSummaryStream, "summary.html")).IndexOf("person") != -1)
					{
						split = Regex.Split(line, "\">|>");
						episodeInfo.stars.Add(split[1].Replace("</a","").Trim());
						line = jumpStreamUntil(ref showSummaryStream, "<br/>");
						split = Regex.Split(line, ">");
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
				seasonEpisodeListStream.Close();
				showPrintableStream.Close();
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

		private string cleanString(string s)
		{
			return s.Replace(": ", " ").Replace(":", "").Replace("/", "").Replace("[","").Replace("]","");
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

		#region ISetupForm Members
 
		// Returns the name of the plugin which is shown in the plugin menu
		public string PluginName()
		{
			return "TV.com Parser";
		}
 
		// Returns the description of the plugin is shown in the plugin menu
		public string Description()
		{
			return "Downloads Episode Information and displays it inside MediaPortal.";
		}
 
		// Returns the author of the plugin which is shown in the plugin menu
		public string Author()      
		{
			return "Inker";
		}	
		
		// show the setup dialog
		public void   ShowPlugin()  
		{
			MessageBox.Show("Nothing to configure, this is just an example");
		}	
 
		// Indicates whether plugin can be enabled/disabled
		public bool   CanEnable()   
		{
			return false;
		}	
 
		// get ID of windowplugin belonging to this setup
		public int    GetWindowId() 
		{
			return 5678;
		}	
		
		// Indicates if plugin is enabled by default;
		public bool   DefaultEnabled()
		{
			return true;
		}	
		
 
 
		// indicates if a plugin has its own setup screen
		public bool   HasSetup()    
		{
			return true;
		}    
	
		/// <summary>
		/// If the plugin should have its own button on the main menu of Media Portal then it
		/// should return true to this method, otherwise if it should not be on home
		/// it should return false
		/// </summary>
		/// <param name="strButtonText">text the button should have</param>
		/// <param name="strButtonImage">image for the button, or empty for default</param>
		/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
		/// <param name="strPictureImage">subpicture for the button or empty for none</param>
		/// <returns>true  : plugin needs its own button on home
		///          false : plugin does not need its own button on home</returns>
		public bool   GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) 
		{
			strButtonText=String.Empty;
			strButtonImage=String.Empty;
			strButtonImageFocus=String.Empty;
			strPictureImage=String.Empty;
			return false;
		}
		#endregion
	}
}
