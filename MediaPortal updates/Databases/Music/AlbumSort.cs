using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Music.Database
{
  public class AlbumSort : IComparer<MusicAlbumInfo> 
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

    public int Compare(MusicAlbumInfo info1, MusicAlbumInfo info2)
    {
      int fitness1 = GetFitness(info1.Title,info1.Artist, info1.Title2);
      int fitness2 = GetFitness(info2.Title, info2.Artist, info2.Title2);
      if (fitness1 > fitness2) return -1;
      if (fitness1 < fitness2) return 1;
      return String.Compare(info1.Title2,info2.Title2);
    }

    int GetFitness(string albumName, string artistName, string year)
    {
      int fitness=0;
      if (_year > 0 && year.IndexOf(_year.ToString()) >= 0)
      {
        fitness += 4;
      }

      if (_artistName != String.Empty)
      {
        string[] parts = _artistName.Split(new char[] { ' ' });
        int[] offsets = new int[parts.Length];
        for (int i = 0; i < parts.Length; ++i)
        {
          offsets[i] = -1;
          int pos = artistName.ToLower().IndexOf(parts[i].ToLower());
          if (pos >= 0) 
          {
            if (i > 0)
            {
              if (pos > offsets[i-1]) fitness+=2;
              else  fitness++;
            }
            else
            {
            fitness += 2;
            }
            offsets[i]=pos;
          }
        }
      }

      if (_album != String.Empty)
      {
        string[] parts = _album.Split(new char[] { ' ' });
        int[] offsets = new int[parts.Length];
        for (int i=0; i < parts.Length;++i)
        {
          offsets[i] = -1;
          int pos = albumName.ToLower().IndexOf(parts[i].ToLower());
          if (pos >= 0)
          {
            if (i > 0)
            {
              if (pos > offsets[i - 1]) fitness += 2;
              else fitness++;
            }
            else
            {
              fitness += 2;
            }
            offsets[i] = pos;
          }
        }
      }
      return fitness;
    }
  }
}
