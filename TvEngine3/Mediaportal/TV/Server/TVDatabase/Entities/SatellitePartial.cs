using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class Satellite : IComparable<Satellite>
  {
    public String DisplayName
    {
      get { return System.IO.Path.GetFileNameWithoutExtension(this._localTransponderFile); }
    }

    public override string ToString()
    {
      return this.Name;
    }

    public int CompareTo(Satellite other)
    {
      return Name.CompareTo(other.Name);
    }

    #region IComparable<Satellite> Members

    int IComparable<Satellite>.CompareTo(Satellite other)
    {
      return Name.CompareTo(other.Name);
    }

    #endregion
  }
}
