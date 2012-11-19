using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.Entities.Cache
{
  public class EntityCache<T, K> where T : class
  {
    private readonly Dictionary<K, T> _cache = new Dictionary<K, T>();
    private readonly object _cacheLock = new object();
    private bool _dataFetchedAtleastOnce;

    public int CacheCount()
    {
      bool dataFetchedAtleastOnce;
      return CacheCount(out dataFetchedAtleastOnce);
    }

    public int CacheCount (out bool dataFetchedAtleastOnce)
    {
      dataFetchedAtleastOnce = _dataFetchedAtleastOnce;
      lock (_cacheLock)
      {
        return _cache.Count;
      }      
    }

    public T GetOrUpdateFromCache (K key, Func<K, T> fetchFunc)
    {
      T type;
      bool hasValue;      
      lock (_cacheLock)
      {
        T typeValue;
        hasValue = _cache.TryGetValue(key, out type);
      }

      if (!hasValue)
      {
        type = fetchFunc(key);
        _dataFetchedAtleastOnce = true;
        if (type != null)
        {
          lock (_cacheLock)
          {
            _cache[key] = type;
          }
        }                
      }
      return type;
    }
  }
}
