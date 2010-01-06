#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Text;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Text : FixedValue
  {
    public Text() {}

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
        text = text.Replace("@" + builder2.ToString(), Encoding.ASCII.GetString(new byte[] {num3}));
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
        text = text.Replace("@" + builder.ToString(), Encoding.ASCII.GetString(new byte[] {num2}));
      }
      return text;
    }
  }
}