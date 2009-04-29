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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using MediaPortal.GUI.Library;
using MediaPortal.Util;
using TvDatabase;

namespace PersonalTVGuide
{
  /// <summary>
  ///  This is a MediaPortal Class.
  /// </summary>
  public class TVServerWishList : List<IWishItem>, IWishList
  {
    #region Enums
    #endregion

    #region Events
    #endregion

    #region Variables
    // Private Variables
    // Protected Variables
    protected int _selectedItem = 0;
    // Public Variables
    #endregion

    #region Constructors/Destructors

    #endregion

    #region Properties
    // Public Properties
    public int SelectedItem
    {
      get { return _selectedItem; }
      set
      {
        _selectedItem = value;
        if (_selectedItem >= Count) _selectedItem = Count - 1;
      }
    }
 
    #endregion

    #region Public Methods
    public void UpDate()
    {
      Clear();
      IList keywordList = Keyword.ListAll();
      foreach (Keyword keyword in keywordList)
      {
        Add(new TVServerWishItem(keyword));
      }
    }

    public void InsertTVProgs(ref GUIListControl lcProgramList, DateTime start, DateTime stop)
    {
      IList list = PersonalTVGuideMap.ListAll();
      foreach (PersonalTVGuideMap map in list)
      {
        Program prog = Program.Retrieve(map.IdProgram);
        if ((prog.StartTime >= start) && (prog.StartTime < stop))
        {
          GUIListItem item = new GUIListItem();
          item.Label = prog.Title;
          if (prog.EpisodeNum != String.Empty) item.Label += "\n" + prog.EpisodeNum;
          item.Label2 = String.Format("{0} {1} - {2}",
                                      Utils.GetShortDayString(prog.StartTime),
                                      prog.StartTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat),
                                      prog.EndTime.ToString("t", CultureInfo.CurrentCulture.DateTimeFormat));
          string strLogo = Utils.GetCoverArt(Thumbs.TVChannel, prog.ReferencedChannel().Name);
          if (!System.IO.File.Exists(strLogo)) strLogo = "defaultVideoBig.png";
          item.ThumbnailImage = strLogo;
          item.IconImage = strLogo;
          item.IconImageBig = strLogo;
          //item.PinImage = RecordingIconStr(prog);
          item.DVDLabel = prog.Description;
          item.TVTag = prog;
          item.MusicTag = map;
          //item.Rating = Ranking;
          //item.OnItemSelected += new MediaPortal.GUI.Library.GUIListItem.ItemSelectedHandler(OnProgItemSelected);
          lcProgramList.Add(item);
        }
      }
    }

    #endregion

    #region Private Methods
    #endregion

    #region <Base class> Overloads
    public override string ToString()
    {
      return string.Format("TVServerWishList - Count = {0}", Count);
    }
    
    #endregion

    #region <Interface> Implementations
    // region for each interface
    #endregion
  }
}
