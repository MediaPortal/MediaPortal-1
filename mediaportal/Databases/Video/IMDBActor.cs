#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Collections;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// Summary description for IMDBActor.
  /// </summary>
  public class IMDBActor
  {
    public class IMDBActorMovie
    {
      public string MovieTitle;
      public string Role;
      public int Year;
    }

    private string _Name = string.Empty;
    private string _thumbnailUrl = string.Empty;
    private string _placeOfBirth = string.Empty;
    private string _dateOfBirth = string.Empty;
    private string _miniBiography = string.Empty;
    private string _biography = string.Empty;
    private ArrayList movies = new ArrayList();

    public IMDBActor() {}

    public string Name
    {
      get { return _Name; }
      set { _Name = value; }
    }

    public string ThumbnailUrl
    {
      get { return _thumbnailUrl; }
      set { _thumbnailUrl = value; }
    }

    public string DateOfBirth
    {
      get { return _dateOfBirth; }
      set { _dateOfBirth = value; }
    }

    public string PlaceOfBirth
    {
      get { return _placeOfBirth; }
      set { _placeOfBirth = value; }
    }

    public string MiniBiography
    {
      get { return _miniBiography; }
      set { _miniBiography = value; }
    }

    public string Biography
    {
      get { return _biography; }
      set { _biography = value; }
    }

    public int Count
    {
      get { return movies.Count; }
    }

    public IMDBActorMovie this[int index]
    {
      get { return (IMDBActorMovie)movies[index]; }
      set { movies[index] = value; }
    }

    public void Add(IMDBActorMovie movie)
    {
      movies.Add(movie);
    }
  }
}