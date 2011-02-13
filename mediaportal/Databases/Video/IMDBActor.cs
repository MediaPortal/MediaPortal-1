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
      //Added
      public string imdbID;
    }

    //Changed - _thumbnailURL added - IMDBActorID added
    private int _id;
    private string _name = string.Empty;
    private string _imdbActorID = string.Empty; // New
    private string _thumbnailUrl = string.Empty; // New
    private string _placeOfBirth = string.Empty;
    private string _dateOfBirth = string.Empty;
    private string _miniBiography = string.Empty;
    private string _biography = string.Empty;
    private ArrayList _movies = new ArrayList();

    public IMDBActor() {}

    //Added
    public int id
    {
      get { return _id; }
      set { _id = value; }
    }

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    //Added
    public string IMDBActorID
    {
      get { return _imdbActorID; }
      set { _imdbActorID = value; }
    }

    //Added
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
      get { return _movies.Count; }
    }

    public IMDBActorMovie this[int index]
    {
      get { return (IMDBActorMovie)_movies[index]; }
      set { _movies[index] = value; }
    }

    public void Add(IMDBActorMovie movie)
    {
      _movies.Add(movie);
    }
  }
}