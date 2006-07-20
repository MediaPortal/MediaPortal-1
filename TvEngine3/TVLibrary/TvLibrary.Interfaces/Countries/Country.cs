
using System;

namespace TvLibrary
{
  /// <summary>
  /// Class which holds the details about a country
  /// </summary>
  [Serializable]
  public class Country
  {
    int _id;
    string _name;
    string _code;

    public Country(int id, string country, string code)
    {
      _id = id;
      _name = country;
      _code = code;
    }

    public override string ToString()
    {
      return _name;
    }

    /// <summary>
    /// get/sets  the country id
    /// </summary>
    public int Id
    {
      get
      {
        return _id;
      }
    }

    /// <summary>
    /// gets/sets the country name
    /// </summary>
    public string Name
    {
      get
      {
        return _name;
      }
    }
    /// <summary>
    /// gets/sets the country code
    /// </summary>
    public string Code
    {
      get
      {
        return _code;
      }
    }
  }

}