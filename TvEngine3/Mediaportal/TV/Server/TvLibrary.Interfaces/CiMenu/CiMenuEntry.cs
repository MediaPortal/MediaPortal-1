using System;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.CiMenu
{
  /// <summary>
  /// CiMenuEntry class to store a single entry
  /// </summary>
  [Serializable]
  public class CiMenuEntry
  {
    private readonly Int32 _index;
    private readonly String _message;

    /// <summary>
    /// Index of menu entry
    /// </summary>
    public int Index
    {
      get { return _index; }
    }

    /// <summary>
    /// Message of menu entry
    /// </summary>
    public String Message
    {
      get { return _message; }
    }

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="index">Index of entry</param>
    /// <param name="message">Message</param>
    public CiMenuEntry(Int32 index, String message)
    {
      _index = index;
      _message = message;
    }

    /// <summary>
    /// Formatted choice text
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("{0}) {1}", _index, _message);
    }
  }
}