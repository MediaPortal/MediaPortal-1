using System;

namespace Mediaportal.TV.Server.TVDatabase.Entities
{
  [AttributeUsage(AttributeTargets.All)]
  public class ProgramAttribute : Attribute 
  {
    private string _displayName;
    public string DisplayName
    {
      get
      {
        //todo grab displayname by language/resources
        return _displayName;
      }
      private set { _displayName = value; }
    }

    public int LanguageId { get; private set; }

   public ProgramAttribute(string displayName, int languageId)
   {
      DisplayName = displayName;
      LanguageId = languageId;
   }
  }
}
