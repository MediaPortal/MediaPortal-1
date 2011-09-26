using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvLibrary.Interfaces
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
