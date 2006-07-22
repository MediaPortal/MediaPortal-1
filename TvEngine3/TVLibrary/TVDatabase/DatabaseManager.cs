using System;
using System.Collections.Generic;
using System.Text;

using IdeaBlade.Persistence;
using IdeaBlade.Rdb;
using IdeaBlade.Persistence.Rdb;
using IdeaBlade.Util;
namespace TvDatabase
{
  public class DatabaseManager
  {
    static bool _clearAfterChange = false;
    static PersistenceManager _manager = null;

    static public void New()
    {
      _manager = null;
    }
    static public bool ClearAfterChange
    {
      get
      {
        return _clearAfterChange;
      }
      set
      {
        _clearAfterChange = value;
      }
    }
    static public void SaveChanges()
    {
      DatabaseManager.Instance.SaveChanges();
      if (_clearAfterChange)
      {
        DatabaseManager.Instance.ClearQueryCache();
      }
    }
    static public PersistenceManager Instance
    {
      get
      {
        if (_manager == null)
        {
          _manager = new PersistenceManager();
          _manager.DefaultQueryStrategy = QueryStrategy.Normal;
          DatabaseManager.FillCache();
        }
        return _manager;
      }
    }
    static public void FillCache()
    {
      DatabaseManager.Instance.ClearQueryCache();
      EntityList<Setting> sett = DatabaseManager.Instance.GetEntities<Setting>();
      EntityList<GroupMap> grpmaps = DatabaseManager.Instance.GetEntities<GroupMap>();
      EntityList<Channel> maps = DatabaseManager.Instance.GetEntities<Channel>();
      EntityList<ChannelMap> chmaps = DatabaseManager.Instance.GetEntities<ChannelMap>();
      EntityList<GroupMap> grps = DatabaseManager.Instance.GetEntities<GroupMap>();
      EntityList<Card> cards = DatabaseManager.Instance.GetEntities<Card>();
      EntityList<TuningDetail> details = DatabaseManager.Instance.GetEntities<TuningDetail>();
    }
  }
}
