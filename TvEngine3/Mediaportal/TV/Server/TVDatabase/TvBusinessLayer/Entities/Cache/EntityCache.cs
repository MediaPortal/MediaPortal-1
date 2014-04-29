using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVDatabase.TVBusinessLayer.Entities.Cache
{
  public class EntityCache<T, K> where T : class
  {
    private readonly Dictionary<K, T> _cache = new Dictionary<K, T>();
    private readonly object _cacheLock = new object();
    private bool _dataFetchedAtleastOnce;

    public EntityCache()
    {
      
    }

    public EntityCache(Dictionary<K, T> cache)
    {
      _cache = cache;
    }

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

    public void AddOrUpdateCache (K key, T value)
    {
      lock (_cacheLock)
      {
       _cache[key] = value;
      }
    }

    public T GetFromCache(K key)
    {
      T type;
      lock (_cacheLock)
      {
        _cache.TryGetValue(key, out type);
      }
      return type;
    }

    public T GetOrUpdateFromCache (K key, Func<K, T> fetchFunc)
    {
      T type;
      bool hasValue;      
      lock (_cacheLock)
      {
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
