#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System.Collections.Generic;
using TvDatabase;

namespace PersonalTVGuide
{
  /// <summary>
  ///  WishItem Class for the TVServer Database
  /// </summary>
  public class TVServerWishItem : IWishItem 
  {
    #region Enum
    #endregion

    #region Delegates
    #endregion

    #region Events
    #endregion


    #region Variables
    // Private Variables
    // Protected Variables
    protected Keyword _keyWord = new Keyword("", 0, false, Keyword.SearchInType.Title);
    protected bool _autoRecord;
    protected int _rating;
    protected bool _searchIn;
    protected List<ChannelGroup> _channelGroupList = new List<ChannelGroup>();       // holds all valid groups (with TVChannels in it) for this Item
    protected List<Timespan> _timeSpanList = new List<Timespan>();                   // for the seven days of a week
    protected List<Program> _programList = new List<Program>();                      // store the found TVPrograms 
    // Public Variables
    #endregion

    #region Constructors/Destructors
    public TVServerWishItem(string keyword, int rating, bool autoRecord, Keyword.SearchInType searchIn)
    {
      Name = keyword;
      Rating = rating;
      AutoRecord = autoRecord;
      SearchIn = searchIn;
    }
    public TVServerWishItem(Keyword keyWord)
    {
      _keyWord = keyWord;
    }
      
    #endregion

    #region Properties
    // Public Properties
    public string Name
    {
      get { return _keyWord.Name;  }
      set { _keyWord.Name = value; }
    }
    public bool AutoRecord 
    {
      get { return _keyWord.AutoRecord; }
      set { _keyWord.AutoRecord = value; }
    }
    public int Rating
    {
      get { return _keyWord.Rating; }
      set { _keyWord.Rating = value; }
    }
    public Keyword.SearchInType SearchIn
    {
      get { return _keyWord.SearchIn; }
      set { _keyWord.SearchIn = value; }
    }
    public bool SearchInDescription
    {
      get { return _keyWord.SearchInDescription; }
      set { _keyWord.SearchInDescription = value; }
    }
    public bool SearchInGenre
    {
      get { return _keyWord.SearchInGenre; }
      set { _keyWord.SearchInGenre = value; }
    }
    public bool SearchInTitle
    {
      get { return _keyWord.SearchInTitle; }
      set { _keyWord.SearchInTitle = value; }
    }
    #endregion

    #region Public Methods
    public void LoadData()
    {
      if (Name.Length < 1) return;
      LoadChannelGroups();
      LoadTimeSpans();
      LoadPrograms();
    }

    public void SaveData()
    {
    }
    #endregion

    #region Protected Methods
    protected void LoadChannelGroups()
    {
      _channelGroupList = KeywordMap.RetrieveChannelGroups(_keyWord.IdKeyword);
    }
    protected void LoadTimeSpans()
    {
      _timeSpanList = Timespan.RetrieveTimeSpanList(_keyWord.IdKeyword);
    }
    protected void LoadPrograms()
    {
      _programList = PersonalTVGuideMap.RetrieveProgramList(_keyWord.IdKeyword);
    }
    #endregion

    #region Private Methods
    #endregion

    #region <Interface> Implementations
    // region for each interface
    #endregion
  }
}
