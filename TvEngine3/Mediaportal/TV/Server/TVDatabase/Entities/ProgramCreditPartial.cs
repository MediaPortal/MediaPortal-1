namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  public partial class ProgramCredit
  {
    public override string ToString()
    {
      return ("[" + Role + "] = [" + Person + "]");
    }
  }
}
