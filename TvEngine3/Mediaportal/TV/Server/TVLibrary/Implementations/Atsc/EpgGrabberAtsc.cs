#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Analyzer;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Tuner;

namespace Mediaportal.TV.Server.TVLibrary.Implementations.Atsc
{
  /// <summary>
  /// An implementation of <see cref="IEpgGrabber"/> for electronic programme
  /// guide data formats used by ATSC broadcasters.
  /// </summary>
  internal class EpgGrabberAtsc : IEpgGrabber
  {
    #region constants

    private static readonly IDictionary<byte, string> MAPPING_ATSC_GENRES = new Dictionary<byte, string>(256)
    {
      { 0x20, "Education" },
      { 0x21, "Entertainment" },
      { 0x22, "Movie" },
      { 0x23, "News" },
      { 0x24, "Religious" },
      { 0x25, "Sports" },
      { 0x26, "Other" },
      { 0x27, "Action" },
      { 0x28, "Advertisement" },
      { 0x29, "Animated" },
      { 0x2a, "Anthology" },
      { 0x2b, "Automobile" },
      { 0x2c, "Awards" },
      { 0x2d, "Baseball" },
      { 0x2e, "Basketball" },
      { 0x2f, "Bulletin" },
      { 0x30, "Business" },
      { 0x31, "Classical" },
      { 0x32, "College" },
      { 0x33, "Combat" },
      { 0x34, "Comedy" },
      { 0x35, "Commentary" },
      { 0x36, "Concert" },
      { 0x37, "Consumer" },
      { 0x38, "Contemporary" },
      { 0x39, "Crime" },
      { 0x3a, "Dance" },
      { 0x3b, "Documentary" },
      { 0x3c, "Drama" },
      { 0x3d, "Elementary" },
      { 0x3e, "Erotica" },
      { 0x3f, "Exercise" },
      { 0x40, "Fantasy" },
      { 0x41, "Farm" },
      { 0x42, "Fashion" },
      { 0x43, "Fiction" },
      { 0x44, "Food" },
      { 0x45, "Football" },
      { 0x46, "Foreign" },
      { 0x47, "Fund Raiser" },
      { 0x48, "Game/Quiz" },
      { 0x49, "Garden" },
      { 0x4a, "Golf" },
      { 0x4b, "Government" },
      { 0x4c, "Health" },
      { 0x4d, "High School" },
      { 0x4e, "History" },
      { 0x4f, "Hobby" },
      { 0x50, "Hockey" },
      { 0x51, "Home" },
      { 0x52, "Horror" },
      { 0x53, "Information" },
      { 0x54, "Instruction" },
      { 0x55, "International" },
      { 0x56, "Interview" },
      { 0x57, "Language" },
      { 0x58, "Legal" },
      { 0x59, "Live" },
      { 0x5a, "Local" },
      { 0x5b, "Math" },
      { 0x5c, "Medical" },
      { 0x5d, "Meeting" },
      { 0x5e, "Military" },
      { 0x5f, "Miniseries" },
      { 0x60, "Music" },
      { 0x61, "Mystery" },
      { 0x62, "National" },
      { 0x63, "Nature" },
      { 0x64, "Police" },
      { 0x65, "Politics" },
      { 0x66, "Premier" },
      { 0x67, "Prerecorded" },
      { 0x68, "Product" },
      { 0x69, "Professional" },
      { 0x6a, "Public" },
      { 0x6b, "Racing" },
      { 0x6c, "Reading" },
      { 0x6d, "Repair" },
      { 0x6e, "Repeat" },
      { 0x6f, "Review" },
      { 0x70, "Romance" },
      { 0x71, "Science" },
      { 0x72, "Series" },
      { 0x73, "Service" },
      { 0x74, "Shopping" },
      { 0x75, "Soap Opera" },
      { 0x76, "Special" },
      { 0x77, "Suspense" },
      { 0x78, "Talk" },
      { 0x79, "Technical" },
      { 0x7a, "Tennis" },
      { 0x7b, "Travel" },
      { 0x7c, "Variety" },
      { 0x7d, "Video" },
      { 0x7e, "Weather" },
      { 0x7f, "Western" },
      { 0x80, "Art" },
      { 0x81, "Auto Racing" },
      { 0x82, "Aviation" },
      { 0x83, "Biography" },
      { 0x84, "Boating" },
      { 0x85, "Bowling" },
      { 0x86, "Boxing" },
      { 0x87, "Cartoon" },
      { 0x88, "Children" },
      { 0x89, "Classic Film" },
      { 0x8a, "Community" },
      { 0x8b, "Computers" },
      { 0x8c, "Country Music" },
      { 0x8d, "Court" },
      { 0x8e, "Extreme Sports" },
      { 0x8f, "Family" },
      { 0x90, "Financial" },
      { 0x91, "Gymnastics" },
      { 0x92, "Headlines" },
      { 0x93, "Horse Racing" },
      { 0x94, "Hunting/Fishing/Outdoors" },
      { 0x95, "Independent" },
      { 0x96, "Jazz" },
      { 0x97, "Magazine" },
      { 0x98, "Motorcycle Racing" },
      { 0x99, "Music/Film/Books" },
      { 0x9a, "News-International" },
      { 0x9b, "News-Local" },
      { 0x9c, "News-National" },
      { 0x9d, "News-Regional" },
      { 0x9e, "Olympics" },
      { 0x9f, "Original" },
      { 0xa0, "Performing Arts" },
      { 0xa1, "Pets/Animals" },
      { 0xa2, "Pop" },
      { 0xa3, "Rock & Roll" },
      { 0xa4, "Sci-Fi" },
      { 0xa5, "Self Improvement" },
      { 0xa6, "Sitcom" },
      { 0xa7, "Skating" },
      { 0xa8, "Skiing" },
      { 0xa9, "Soccer" },
      { 0xaa, "Track/Field" },
      { 0xab, "True" },
      { 0xac, "Volleyball" },
      { 0xad, "Wrestling" }
    };

    #endregion

    /// <summary>
    /// Initialise a new instance of the <see cref="EpgGrabberAtsc"/> class.
    /// </summary>
    /// <param name="grabberAtsc">The DVB EPG grabber, if available/supported.</param>
    /// <param name="grabberMhw">The MediaHighway EPG grabber, if available/supported.</param>
    /// <param name="grabberOpenTv">The OpenTV EPG grabber, if available/supported.</param>
    public EpgGrabberAtsc(IGrabberEpgAtsc grabberAtsc, IGrabberEpgScte grabberScte)
    {
      // TODO
    }

    private static string GetAtscGenreDescription(byte genreId)
    {
      string description;
      if (!MAPPING_ATSC_GENRES.TryGetValue(genreId, out description))
      {
        description = string.Format("User Defined {0}", genreId);
      }
      return description;
    }

    #region IEpgGrabber members

    public void ReloadConfiguration()
    {
      throw new System.NotImplementedException();
    }

    public bool IsGrabbing
    {
      get { throw new System.NotImplementedException(); }
    }

    public void GrabEpg(Interfaces.Channel.IChannel tuningDetail, IEpgGrabberCallBack callBack)
    {
      throw new System.NotImplementedException();
    }

    public void AbortGrabbing()
    {
      throw new System.NotImplementedException();
    }

    #endregion

    #region IEpgCallBack member

    public int OnEpgReceived()
    {
      throw new System.NotImplementedException();
    }

    #endregion
  }
}
