using System;
using System.Collections.Generic;
using System.Text;

namespace ThumbDBLib
{
  static public class ThumbnailDatabaseCache
  {
    static List<ThumbnailDatabase> _cache = new List<ThumbnailDatabase>();

    static public ThumbnailDatabase Get(string folder)
    {
      lock (_cache)
      {
        for (int i = 0; i < _cache.Count; ++i)
        {
          if (String.Compare(_cache[i].Folder, folder, true) == 0) return _cache[i];
        }

        ThumbnailDatabase dbs = new ThumbnailDatabase(folder);
        _cache.Add(dbs);
        return dbs;
      }
    }
  }
}
