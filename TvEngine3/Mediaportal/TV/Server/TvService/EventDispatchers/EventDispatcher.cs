using System;
using System.Collections.Generic;

namespace Mediaportal.TV.Server.TVService.EventDispatchers
{
  public abstract class EventDispatcher
  {
    protected readonly object _usersLock = new object();
    protected readonly IDictionary<string, DateTime> _users = new Dictionary<string, DateTime>();

    ~EventDispatcher()
    {
      Stop();
    }    

    public void Register(string username)
    {
      lock (_usersLock)
      {
        if (!_users.ContainsKey(username))
        {
          _users.Add(username, DateTime.Now);
        }
      }
    }

    public void UnRegister(string username)
    {
      lock (_usersLock)
      {
        if (_users.ContainsKey(username))
        {
          _users.Remove(username);
        }
      }
    }

    public abstract void Start();
    public abstract void Stop();

    protected IDictionary<string, DateTime> GetUsersCopy()
    {
      IDictionary<string, DateTime> usersCopy;
      lock (_usersLock)
      {
        usersCopy = new Dictionary<string, DateTime>(_users);
      }
      return usersCopy;
    }

  }
}
