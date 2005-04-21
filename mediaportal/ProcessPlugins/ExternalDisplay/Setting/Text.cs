using System.Text;
using MediaPortal.GUI.Library;

namespace ProcessPlugins.ExternalDisplay.Setting
{
  /// <summary>
  /// The Text class represents either a fixed text, or a reference to a translatable resource string.
  /// </summary>
  public class Text : Value
  {
    public Text()
    {
      
    }

    public Text(string _text)
    {
      value = _text;
    }

    public Text(string _text, Condition _condition)
    {
      value=_text;
      Condition=_condition;
    }

    /// <summary>
    /// Evaluates the <see cref="Text"/>
    /// </summary>
    /// <returns>
    /// The fixed text, or in case of a reference to a translatable resource string, the translation.
    /// If the associated <see cref="Condition"/> evaluates to false, the return value is an empty string.
    /// </returns>
    public override string Evaluate()
    {
      string text = value;
      if (Condition==null || Condition.Evaluate())
      {
        int pos = text.IndexOf("#")+1;
        while(pos>0)
        {
          StringBuilder b = new StringBuilder();
          while(pos>=1 && text[pos]>='0' && text[pos]<='9')
          {
            b.Append(text[pos++]);
          }
          string num = b.ToString();
          int val = int.Parse(num);
          text = text.Replace("#"+num,GUILocalizeStrings.Get(val));
          pos = text.IndexOf("#")+1;
        }
        return text;
      }
      return "";
    }
  }
}
