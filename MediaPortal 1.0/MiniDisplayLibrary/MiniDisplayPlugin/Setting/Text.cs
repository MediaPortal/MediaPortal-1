using MediaPortal.GUI.Library;
using System;
using System.Text;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Text : FixedValue
  {
    public Text()
    {
    }

    public Text(string _text)
    {
      base.value = _text;
    }

    public Text(string _text, Condition _condition)
    {
      base.value = _text;
      base.Condition = _condition;
    }

    protected override string DoEvaluate()
    {
      int num;
      string text = base.value;
      for (num = text.IndexOf("#") + 1; num > 0; num = text.IndexOf("#") + 1)
      {
        StringBuilder builder = new StringBuilder();
        while (((num >= 1) && (num < text.Length)) && ((text[num] >= '0') && (text[num] <= '9')))
        {
          builder.Append(text[num++]);
        }
        string s = builder.ToString();
        int dwCode = int.Parse(s);
        text = text.Replace("#" + s, GUILocalizeStrings.Get(dwCode));
      }
      for (num = text.IndexOf("@") + 1; num > 0; num = text.IndexOf("@") + 1)
      {
        StringBuilder builder2 = new StringBuilder();
        while (((num >= 1) && (num < text.Length)) && ((text[num] >= '0') && (text[num] <= '9')))
        {
          builder2.Append(text[num++]);
        }
        byte num3 = Convert.ToByte(builder2.ToString(), 0x10);
        text = text.Replace("@" + builder2.ToString(), Encoding.ASCII.GetString(new byte[] { num3 }));
      }
      return this.EvaluateHex(text);
    }

    private string EvaluateHex(string text)
    {
      for (int i = text.IndexOf("@") + 1; i > 0; i = text.IndexOf("@") + 1)
      {
        StringBuilder builder = new StringBuilder();
        while (((i >= 1) && (i < text.Length)) && ((text[i] >= '0') && (text[i] <= '9')))
        {
          builder.Append(text[i++]);
        }
        byte num2 = Convert.ToByte(builder.ToString(), 0x10);
        text = text.Replace("@" + builder.ToString(), Encoding.ASCII.GetString(new byte[] { num2 }));
      }
      return text;
    }
  }
}

