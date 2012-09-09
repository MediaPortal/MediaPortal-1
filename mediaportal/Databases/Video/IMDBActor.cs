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

using System;
using System.Collections;
using System.Globalization;
using MediaPortal.GUI.Library;

namespace MediaPortal.Video.Database
{
  /// <summary>
  /// Summary description for IMDBActor.
  /// </summary>
  public class IMDBActor
  {
    public class IMDBActorMovie : IComparable
    {
      public string MovieTitle = string.Empty;
      public string Role = string.Empty;
      public int Year = 1900;
      //Added
      public int ActorID = -1;
      public string MovieImdbID = string.Empty;
      public string MoviePlot = string.Empty;
      public string MovieCover = string.Empty;
      public string MovieCast = string.Empty;
      public string MovieGenre = string.Empty;
      public string MovieCredits = string.Empty;
      public string MovieMpaaRating = string.Empty;
      public int MovieRuntime = 0;

      public int CompareTo(object movie)
      {
        if (!(movie is IMDBActorMovie))
          throw new InvalidCastException();
        
        IMDBActorMovie sortByYear = (IMDBActorMovie)movie;
        return sortByYear.Year.CompareTo(Year);
       }
    }

    
    //Changed - _thumbnailURL added - IMDBActorID added
    private int _id = -1;
    private string _name = string.Empty;
    private string _imdbActorID = string.Empty; // New
    private string _thumbnailUrl = string.Empty; // New
    private string _placeOfBirth = string.Empty;
    private string _dateOfBirth = string.Empty;
    private string _placeOfDeath = string.Empty; // New
    private string _dateOfDeath = string.Empty; // New
    private string _miniBiography = string.Empty;
    private string _biography = string.Empty;
    private string _lastUpdate = string.Empty;
    private ArrayList _movies = new ArrayList();
    
    public IMDBActor() {}

    //Added
    public int ID
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

    public string DateOfDeath
    {
      get { return _dateOfDeath; }
      set { _dateOfDeath = value; }
    }

    public string PlaceOfDeath
    {
      get { return _placeOfDeath; }
      set { _placeOfDeath = value; }
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

    public string LastUpdate
    {
      get { return _lastUpdate; }
      set { _lastUpdate = value; }
    }

    public int Count
    {
      get { return _movies.Count; }
    }

    public void SortActorMoviesByYear()
    {
      _movies.Sort();
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

    public void Reset()
    {
      _name = string.Empty;
      _imdbActorID = string.Empty; // New
      _thumbnailUrl = string.Empty; // New
      _placeOfBirth = string.Empty;
      _dateOfBirth = string.Empty;
      _placeOfDeath = string.Empty; // New
      _dateOfDeath = string.Empty; // New
      _miniBiography = string.Empty;
      _biography = string.Empty;
      _lastUpdate = string.Empty;
    }

    public void SetProperties()
    {
      GUIPropertyManager.SetProperty("#Actor.Name", Name);
      GUIPropertyManager.SetProperty("#Actor.DateOfBirth", DateOfBirth);
      GUIPropertyManager.SetProperty("#Actor.PlaceOfBirth", PlaceOfBirth);
      DateTime lastUpdate;
      DateTime.TryParseExact(LastUpdate, "yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out lastUpdate);
      string x = lastUpdate.ToShortDateString();
      GUIPropertyManager.SetProperty("#Actor.LastUpdate", lastUpdate.ToShortDateString());

      if (DateOfDeath != Strings.Unknown)
      {
        GUIPropertyManager.SetProperty("#Actor.DateOfDeath", DateOfDeath);
      }
      else
      {
        GUIPropertyManager.SetProperty("#Actor.DateOfDeath", string.Empty);
      }

      if (PlaceOfDeath != Strings.Unknown)
      {
        GUIPropertyManager.SetProperty("#Actor.PlaceOfDeath", PlaceOfDeath);
      }
      else
      {
        GUIPropertyManager.SetProperty("#Actor.PlaceOfDeath", string.Empty);
      }

      string biography = Biography;

      if (biography == string.Empty || biography == Strings.Unknown)
      {
        biography = MiniBiography;
        if (biography == string.Empty || biography == Strings.Unknown)
        {
          biography = string.Empty;
        }
      }
      GUIPropertyManager.SetProperty("#Actor.Biography", biography);

      if (ID == -1)
      {
        GUIPropertyManager.SetProperty("#hideActorinfo", "true");
      }
      else
      {
        GUIPropertyManager.SetProperty("#hideActorinfo", "false");
      }
    }

    public void ResetProperties()
    {
      GUIPropertyManager.SetProperty("#Actor.Name", string.Empty);
      GUIPropertyManager.SetProperty("#Actor.DateOfBirth", string.Empty);
      GUIPropertyManager.SetProperty("#Actor.PlaceOfBirth", string.Empty);
      GUIPropertyManager.SetProperty("#Actor.LastUpdate", string.Empty);
      GUIPropertyManager.SetProperty("#Actor.DateOfDeath", string.Empty);
      GUIPropertyManager.SetProperty("#Actor.PlaceOfDeath", string.Empty);
      GUIPropertyManager.SetProperty("#Actor.Biography", string.Empty);
      GUIPropertyManager.SetProperty("#hideActorinfo", "true");
    }
  }
}