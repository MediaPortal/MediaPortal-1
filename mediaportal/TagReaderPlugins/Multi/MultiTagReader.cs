/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections;
namespace MediaPortal.TagReader.MultiTagReader
{
  /// <summary>
  /// Summary description for Class1.
  /// </summary>
  public class MultiTagReader: ITagReader
  {
    /// <summary>Song Info Charater separator</summary>
    public const char SI_SEPARATOR = ':';
    /// <summary>Song Info Format</summary>
    public const string SI_FORMAT = "Format";
    /// <summary>Song Info Details</summary>
    public const string SI_DETAILS = "Details";
    /// <summary>Song Info Details for playtime</summary>
    public const string SI_DETAILS_PLAYTIME = "playtime";
    /// <summary>Song Info Tag</summary>
    public const string SI_TAG = "Tag";
    /// <summary>Song Detail Charater separator</summary>
    public const char SD_SEPARATOR = '=';
    /// <summary>Song Detail Title</summary>
    public const string SD_TITLE = "Title";
    /// <summary>Song Detail Artist</summary>
    public const string SD_ARTIST = "Artist";
    /// <summary>Song Detail Album</summary>
    public const string SD_ALBUM = "Album";
    /// <summary>Song Detail Track</summary>
    public const string SD_TRACK = "Track";
    /// <summary>Song Detail Genre</summary>
    public const string SD_GENRE = "Genre";
    /// <summary>Song Detail Year</summary>
    public const string SD_YEAR = "Year";
    /// <summary>Song Detail Comment</summary>
    public const string SD_COMMENT= "Comment";
    //protected Hashtable m_metadata = new Hashtable();
    MusicTag m_tag=new MusicTag();
    protected string m_filename = null;
    protected string m_quotedFilename = null;

		public MultiTagReader()
		{
		}

    /// <summary>standard constructor</summary>
    /// <param name="arg">File to process (i.e. read and find tags).
    /// </param>
    public MultiTagReader(string filename)
    {
      setFilename(filename);
    }


    private void setFilename(string filename)
    {
      m_filename = filename.Trim();
      if(!m_filename.StartsWith("\""))
      {
        m_quotedFilename = "\"" + m_filename + "\"";	// quote the filename in case of spaces...
      }
      else
      {
        m_quotedFilename = m_filename;
        m_filename = m_filename.Replace("\"", "");
      }
    }

    /// <summary>
    /// This method returns if this tag reader can read the given filename
    /// </summary>
    /// <param name="strFileName">The filename to check if this reader can read the tags</param>
    /// <returns>boolean that specifies if this tag reader can read the given file</returns>
    public override bool SupportsFile(string strFileName)    
    {
      if (System.IO.Path.GetExtension(strFileName).ToLower()==".ogg" ||         
          System.IO.Path.GetExtension(strFileName).ToLower()==".flac" ||        
					System.IO.Path.GetExtension(strFileName).ToLower()==".mpc" ||         
					System.IO.Path.GetExtension(strFileName).ToLower()==".wv" ||        
          System.IO.Path.GetExtension(strFileName).ToLower()==".ape")         
        return true;      
      return false;    
    }
    
    /// <summary>
    /// returns the MusicTag of the file that was read.  This method should be called
    /// after ReadTag.  This makes this class not thread safe.
    /// </summary>
    public override MusicTag Tag    
    {      
      get { 
        return m_tag;
      }    
    }
    
    /// <summary>
    /// Reads a specific filename to extract the tags
    /// </summary>
    /// <param name="filename">the filename to get the tags from</param>
    /// <returns></returns>
    public override bool ReadTag(String filename)
    {
      if(!SupportsFile(filename))
        return false;
      setFilename(filename);
      string[] output = RunProgram("tag.exe", "--simple " + m_quotedFilename, 20);	// 20 seconds to respond
      // skip until the filename
      int i = 0;
      bool found = false;
      for(i = 0; i < output.Length; i++)
      {
        if(output[i] == m_filename)
        {
          found = true;
          break;
        }
      }
      if(found)
      {
        char[] seps = new char[]{SI_SEPARATOR, SD_SEPARATOR};
        for(int j = i+1; j < output.Length; j++)
        {
          string[] parts = output[j].Split(seps,2);
          if(parts.Length == 2)
          {
            switch(parts[0].Trim())
            {
							case SD_TITLE:
								m_tag.Title = parts[1].Trim();
								break;

              case SD_ALBUM:
                m_tag.Album = parts[1].Trim();
                break;
              case SD_ARTIST:
                m_tag.Artist = parts[1].Trim();
                break;
              case SD_COMMENT:
                m_tag.Comment = parts[1].Trim();
                break;
              case SI_DETAILS:
                {
                  string details = parts[1].Trim();
                  int index = details.IndexOf(SI_DETAILS_PLAYTIME);
                  if(index != -1)
                  {
                    index += SI_DETAILS_PLAYTIME.Length + 1;
                    string duration = details.Substring(index);
                    if(duration.IndexOf(":") == duration.LastIndexOf(":")) 
                      duration = "00:" + duration;	// no hours, so add 0 hours to the duration.
									  try									
									  {										
									    TimeSpan intervalVal = TimeSpan.Parse(duration);										
									    m_tag.Duration = (int)intervalVal.TotalSeconds;
									  }catch(Exception){}                
							    }              
							  }
                break;
              case SD_GENRE:
                m_tag.Genre = parts[1].Trim();
                break;
              case SD_TRACK:
								try
								{
									m_tag.Track = Convert.ToInt32(parts[1].Trim());
								}
								catch
								{
									string track = parts[1].Trim();
                  int k = 0;
                  int l = 0;
                  char[] trackChar = track.ToCharArray();
                  for(l = 0; l < trackChar.Length; l++)
                  {
                    if(Char.IsDigit(trackChar[l])) break;
                  }

                  for(k = l; k < trackChar.Length; k++)
                  {
                    if(!Char.IsDigit(trackChar[k])) break;
                  }
                  if(l < k)
                  {
                    if(k == track.Length)
                      m_tag.Track = Convert.ToInt32(track.Substring(l));
                    else
                      m_tag.Track = Convert.ToInt32(track.Substring(l, k-l));
                  }
								}
                break;
              case SD_YEAR:
                try
                {
                  m_tag.Year = Convert.ToInt32(parts[1].Trim());
                }
                catch
                {
                  string year = parts[1].Trim();
                  int k = 0;
                  int l = 0;
                  char[] yearChar = year.ToCharArray();
                  for(l = 0; l < yearChar.Length; l++)
                  {
                    if(Char.IsDigit(yearChar[l])) break;
                  }

                  for(k = l; k < yearChar.Length; k++)
                  {
                    if(!Char.IsDigit(yearChar[k])) break;
                  }
                  if(l < k)
                  {
                    if(k == year.Length)
                      m_tag.Year = Convert.ToInt32(year.Substring(l));
                    else
                      m_tag.Year = Convert.ToInt32(year.Substring(l, k-l));
                  }
                }
                break;
            }
          }
        }
        //System.Collections.IDictionaryEnumerator myEnumerator = m_metadata.GetEnumerator();
        //Console.WriteLine( "\t-KEY-\t-VALUE-" );
        //while ( myEnumerator.MoveNext() )
        //  Console.WriteLine("\t{0}:\t{1}", myEnumerator.Key, myEnumerator.Value);
        //Console.WriteLine();
        return true;
      }
      return false;
    }

    private string[] RunProgram(string exeName, string argsLine, int timeoutSeconds)
    {
      StreamReader outputStream = StreamReader.Null;
      ProcessStartInfo psI = new ProcessStartInfo(exeName, argsLine);
      Process newProcess = new Process();

      string[] output = null;
      bool success = false;

      try
      {
				
        newProcess.StartInfo.FileName = exeName;
        newProcess.StartInfo.Arguments = argsLine;
        newProcess.StartInfo.UseShellExecute = false;
        newProcess.StartInfo.CreateNoWindow = true;
        newProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        //newProcess.StartInfo.RedirectStandardOutput = true;
        newProcess.StartInfo.RedirectStandardError = true;	// for some reason tag.exe uses the standard error rather than standard output
        newProcess.Start();
				
        /*
        psI.UseShellExecute = false;
        psI.RedirectStandardInput = false;
        psI.RedirectStandardOutput = true;
        psI.RedirectStandardError = false;
        psI.CreateNoWindow = true;
        newProcess.StartInfo = psI;
        newProcess.Start();
        */

        if (0 == timeoutSeconds)
        {
          outputStream = newProcess.StandardOutput;
          //output = outputStream.ReadToEnd();
          ArrayList buffer = new ArrayList();
          string line = null;
          do
          {
            line = outputStream.ReadLine();
            buffer.Add(line);
          } while(line != null);

          output = (string[])buffer.ToArray(typeof(string));

          newProcess.WaitForExit();
        }
        else
        {
          success = newProcess.WaitForExit(timeoutSeconds * 1000);
	
          if (success)
          {
            //outputStream = newProcess.StandardOutput;
            outputStream = newProcess.StandardError;
            ArrayList buffer = new ArrayList();
            string line = null;
            do
            {
              line = outputStream.ReadLine();
              if(line != null)
                buffer.Add(line);
            } while(line != null);
            //output = outputStream.ReadToEnd();
            output = (string[])buffer.ToArray(typeof(string));
          }
          else
          {
            string msg = "Timed out at " + timeoutSeconds + " seconds waiting for " + exeName + " to exit.";
            throw (new Exception(msg));
          }
        }
      }
      catch(Exception e)
      {
        //throw (new Exception("An error occurred running " + exeName + ".",e));
        throw e;
      }
      finally
      {
        outputStream.Close();
        newProcess.Close();
      }
      return output;
    }
  }
}
