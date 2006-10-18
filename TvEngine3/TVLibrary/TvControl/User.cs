using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace TvControl
{
  /// <summary>
  /// Class holding user credentials
  /// </summary>
  [Serializable]
  public class User
  {
    string _hostName;
    bool _isAdmin;

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    public User()
    {
      _hostName = Dns.GetHostName();
      _isAdmin = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    /// <param name="isAdmin">if set to <c>true</c> [is admin].</param>
    public User(string name, bool isAdmin)
    {
      _hostName = name;
      _isAdmin = isAdmin;
    }

    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get
      {
        return _hostName;
      }
      set
      {
        _hostName = value;
      }
    }
    /// <summary>
    /// Gets or sets a value indicating whether this instance is admin.
    /// </summary>
    /// <value><c>true</c> if this instance is admin; otherwise, <c>false</c>.</value>
    public bool IsAdmin
    {
      get
      {
        return _isAdmin;
      }
      set
      {
        _isAdmin = value;
      }
    }
  }
}
