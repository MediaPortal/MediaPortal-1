#region Copyright (C) 2005-2013 Team MediaPortal

// Copyright (C) 2005-2013 Team MediaPortal
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

using System.Linq;
using System.Xml.Linq;

namespace MediaPortal.LastFM
{
  public class LastFMUser
  {
    public string Username { get; set; }
    public string UserImgURL { get; set; }
    public bool Subscriber { get; set; }
    public int PlayCount { get; set; }

    public LastFMUser(string strUserName, string strImgURL, bool bSubscriber, int iPlaycount)
    {
      Username = strUserName;
      UserImgURL = strImgURL;
      Subscriber = bSubscriber;
      PlayCount = iPlaycount;
    }

    public LastFMUser() { }

    public LastFMUser(XContainer xDoc)
    {
      var user = xDoc.Descendants("user").FirstOrDefault();
      if (user == null) return;

      var userName = (string) user.Element("name");
      var subscriber = ((string) user.Element("subscriber")) == "1";
      int playcount;
      int.TryParse((string) user.Element("playcount"), out playcount);
      var userImgURL = (from img in user.Elements("image")
                        where (string) img.Attribute("size") == "medium"
                        select img.Value).First();

      Username = userName;
      UserImgURL = userImgURL;
      Subscriber = subscriber;
      PlayCount = playcount;
    }

  }
}
