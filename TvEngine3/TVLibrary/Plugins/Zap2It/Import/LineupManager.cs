/* 
 *	Copyright (C) 2006-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace Zap2it
{
  public class LineupManager
  {
    const string LOGIN_URL = "http://labs.zap2it.com/ztvws/ztvws_login/1,1059,TMS01-1,00.html";
    const string HOME_URL = "http://labs.zap2it.com/ztvws/ztvws_homepage/1,1052,TMS01-1,00.html";

    private string _username;
    private string _password;

    private Dictionary<WebEntities.WebLineup, Uri> _postUris = new Dictionary<Zap2it.WebEntities.WebLineup, Uri>();
    private CookieContainer _cookies = new CookieContainer();

    /// <summary>
    /// Initializes a new instance of the <see cref="T:LineupManager"/> class.
    /// </summary>
    /// <param name="username">The Zap2it account username.</param>
    /// <param name="password">The Zap2it account password.</param>
    public LineupManager(string username, string password)
    {
      this._username = username;
      this._password = password;
    }

    /// <summary>
    /// Logins to data direct.
    /// </summary>
    /// <returns></returns>
    public WebEntities.WebLineupCollection Login()
    {
      HttpWebRequest webRequest = WebRequest.Create(LOGIN_URL) as HttpWebRequest;
      NameValueCollection values = new NameValueCollection();
      values.Add("username", this._username);
      values.Add("password", this._password);
      values.Add("action", "Login");
      webRequest.CookieContainer = this._cookies;
      string html = PostFormData(webRequest, values);
      return PopulateLineupCollection(html);
    }

    /// <summary>
    /// Retreives the lineups.
    /// </summary>
    /// <returns></returns>
    public WebEntities.WebLineupCollection RetrieveLineups()
    {
      if (CheckSessionIsValid() == false)
        return this.Login();
      else
      {
        string html;
        HttpWebRequest webRequest = WebRequest.Create(HOME_URL) as HttpWebRequest;
        webRequest.CookieContainer = this._cookies;
        using (StreamReader reader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
        {
          html = reader.ReadToEnd();
        }
        return PopulateLineupCollection(html);
      }
    }

    /// <summary>
    /// Retrieves the channels for lineup.
    /// </summary>
    /// <param name="lineup">The lineup.</param>
    /// <returns></returns>
    public WebEntities.WebChannelCollection RetrieveChannelsForLineup(WebEntities.WebLineup lineup)
    {
      if (CheckSessionIsValid() == false) Login();
      HttpWebRequest webRequest = WebRequest.Create(lineup.FormAction) as HttpWebRequest;
      NameValueCollection values = new NameValueCollection();
      values.Add("udl_id", lineup.Udl_Id);
      values.Add("zipcode", lineup.ZipCode);
      values.Add("lineup_id", lineup.LineupId);
      values.Add("submit", "Modify");
      webRequest.CookieContainer = this._cookies;
      string html = PostFormData(webRequest, values);
      this._postUris[lineup] = webRequest.GetResponse().ResponseUri;
      return PopulateChannelCollection(html);
    }

    /// <summary>
    /// Sets the channels for lineup.
    /// </summary>
    /// <param name="lineup">The lineup.</param>
    /// <param name="channels">The channels.</param>
    public bool SetChannelsForLineup(WebEntities.WebLineup lineup, WebEntities.WebChannelCollection channelCollection)
    {
      if (CheckSessionIsValid() == false) Login();
      HttpWebRequest webRequest = WebRequest.Create(lineup.FormAction) as HttpWebRequest;
      NameValueCollection values = new NameValueCollection();
      values.Add("udl_id", lineup.Udl_Id);
      values.Add("zipcode", lineup.ZipCode);
      values.Add("lineup_id", lineup.LineupId);
      values.Add("submit", "Modify");
      webRequest.CookieContainer = this._cookies;
      PostFormData(webRequest, values);
      Uri submitUri = webRequest.GetResponse().ResponseUri;

      webRequest = WebRequest.Create(submitUri) as HttpWebRequest;
      values.Clear();
      foreach (WebEntities.WebChannel channel in channelCollection)
      {
        if (channel.Enabled)
        {
          values.Add(channel.Id, "1");
        }
      }
      values.Add("action", "Update");
      webRequest.CookieContainer = this._cookies;
      PostFormData(webRequest, values);
      HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
      return (response.StatusCode == HttpStatusCode.OK);
    }

    /// <summary>
    /// Populates the lineup collection.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns></returns>
    private WebEntities.WebLineupCollection PopulateLineupCollection(string html)
    {
      WebEntities.WebLineupCollection lineupCollection = new WebEntities.WebLineupCollection();
      Regex regex = new Regex(
          @"<TD BGCOLOR=""#FFFFFF"">(?<name>[^<]+)</TD>[^<]*<TD BGCOLOR=""#FFFFFF"">(?<type>[^<]+)</TD>[^<]*" +
          @"<FORM ACTION=""(?<formAction>[^""]+)"" METHOD=""post"">[^<]*<TD BGCOLOR=""#FFFFFF"" ALIGN=""center"">[^<]*" +
          @"<INPUT TYPE=""hidden"" NAME=""udl_id"" VALUE=""(?<udl_id>[^""]+)"">[^<]*" +
          @"<INPUT TYPE=""hidden"" NAME=""zipcode"" VALUE=""(?<zipcode>[^""]+)"">[^<]*" +
          @"<INPUT TYPE=""hidden"" NAME=""lineup_id"" VALUE=""(?<lineup_id>[^""]+)"">", RegexOptions.Singleline);

      foreach (Match match in regex.Matches(html))
      {
        WebEntities.WebLineup lineup = new Zap2it.WebEntities.WebLineup();
        lineup.Name = match.Groups["name"].Value.Trim();
        lineup.Type = match.Groups["type"].Value.Trim();
        lineup.Udl_Id = match.Groups["udl_id"].Value.Trim();
        lineup.ZipCode = match.Groups["zipcode"].Value.Trim();
        lineup.LineupId = match.Groups["lineup_id"].Value.Trim();
        lineup.FormAction = new Uri(new Uri(LOGIN_URL), match.Groups["formAction"].Value.Trim());
        lineupCollection.Add(lineup);
      }
      return lineupCollection;
    }

    /// <summary>
    /// Populates the channel collection.
    /// </summary>
    /// <param name="html">The HTML.</param>
    /// <returns></returns>
    private WebEntities.WebChannelCollection PopulateChannelCollection(string html)
    {
      WebEntities.WebChannelCollection channelCollection = new WebEntities.WebChannelCollection();
      Regex regex = new Regex(
          @"<INPUT TYPE=""checkbox"" (?<checked>CHECKED\s*)?NAME=""[0-9]+"" ID=""(?<id>[0-9]+)"" VALUE=1>[^<]*</TD>[^<]*" +
          @"<LABEL[^<]*<TD>(?<channel>[^<]+)</TD>[^<]*<TD>&nbsp;(?<station>[^<]+)</TD>", RegexOptions.Singleline);

      foreach (Match match in regex.Matches(html))
      {
        WebEntities.WebChannel channel = new Zap2it.WebEntities.WebChannel();
        channel.Id = match.Groups["id"].Value.Trim();
        channel.Station = match.Groups["station"].Value.Trim();
        channel.ChannelNum = match.Groups["channel"].Value.Trim();
        channel.Enabled = match.Groups["checked"].Success;
        channelCollection.Add(channel);
      }
      return channelCollection;
    }

    /// <summary>
    /// Checks the session is valid.
    /// </summary>
    /// <returns></returns>
    public bool CheckSessionIsValid()
    {
      // Check for cookies
      if (_cookies.Count == 0) return false;

      // Send a HEAD request with cookies, check for an OK response for valid session cookies
      HttpWebRequest webRequest = WebRequest.Create(HOME_URL) as HttpWebRequest;
      webRequest.CookieContainer = this._cookies;
      webRequest.AllowAutoRedirect = false;
      webRequest.Method = "HEAD";
      HttpWebResponse response = webRequest.GetResponse() as HttpWebResponse;
      return response.StatusCode == HttpStatusCode.OK;
    }

    /// <summary>
    /// Posts the form data.
    /// </summary>
    /// <param name="webRequest">The web request.</param>
    /// <param name="values">The values.</param>
    /// <returns></returns>
    private string PostFormData(WebRequest webRequest, NameValueCollection values)
    {
      StringBuilder parameters = new StringBuilder();
      string response;
      for (int i = 0; i < values.Count; i++)
      {
        if (parameters.Length != 0)
        {
          parameters.Append("&");
        }
        parameters.Append(values.GetKey(i)).Append('=').Append(System.Web.HttpUtility.UrlEncode(values[i]));
      }
      webRequest.Timeout = 45000; //Changed to 45 secs from 20 seconds
      webRequest.Method = "POST";
      webRequest.ContentType = "application/x-www-form-urlencoded";
      UTF8Encoding encoding = new UTF8Encoding();
      byte[] bytes = encoding.GetBytes(parameters.ToString());
      webRequest.ContentLength = bytes.Length;
      using (Stream writeStream = webRequest.GetRequestStream())
      {
        writeStream.Write(bytes, 0, bytes.Length);
      }
      using (StreamReader reader = new StreamReader(webRequest.GetResponse().GetResponseStream()))
      {
        response = reader.ReadToEnd();
      }
      return response;
    }
  }
}