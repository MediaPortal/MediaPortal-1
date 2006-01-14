using System;
using System.Collections;
using System.Text;

namespace MediaPortal.Music.Database
{
  public class AlbumSort : IComparer 
  {
    string _album;
    string _artistName;
    int _year;
    public AlbumSort(string album,string artistName, int year)
    {
      _album = album.ToLower();
      _artistName = artistName.ToLower();
      _year = year;
    }

    public int Compare(object a, object b)
    {
      MusicAlbumInfo info1 = a as MusicAlbumInfo;
      MusicAlbumInfo info2 = b as MusicAlbumInfo;
      int fitness1 = GetFitness(info1.Title2);
      int fitness2 = GetFitness(info2.Title2);
      if (fitness1 > fitness2) return -1;
      if (fitness1 < fitness2) return 1;
      return String.Compare(info1.Title2,info2.Title2);
    }

    int GetFitness(string line)
    {
      string lineLower=line.ToLower();
      int fitness=0;
      if (_year > 0 && lineLower.IndexOf(_year.ToString()) >= 0)
      {
        fitness += 2;
      }

      if (_artistName != String.Empty)
      {
        string[] parts = _artistName.Split(new char[] { ' ' });
        for (int i = 0; i < parts.Length; ++i)
        {
          if (lineLower.IndexOf(parts[i].ToLower()) >= 0) fitness += 2;
        }
      }

      if (_album != String.Empty)
      {
        string[] parts = _album.Split(new char[]{' '});
        for (int i=0; i < parts.Length;++i)
        {
          if (lineLower.IndexOf(parts[i].ToLower()) >= 0) fitness += 2;
        }
      }
      return fitness;
    }
  }
}
