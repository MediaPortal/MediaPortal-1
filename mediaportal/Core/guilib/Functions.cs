#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.Library
{
  internal class GUIFunctions
  {
    #region  String functions

    [XMLSkinFunction("string.format")]
    public static string FormatString(string format, object arg0)
    {
      return string.Format(format, arg0);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(string format, object arg0, object arg1)
    {
      return string.Format(format, arg0, arg1);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(string format, object arg0, object arg1, object arg2)
    {
      return string.Format(format, arg0, arg1, arg2);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(string format, params object[] args)
    {
      return string.Format(format, args);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(int formatId, object arg0)
    {
      return string.Format(GUILocalizeStrings.Get(formatId), arg0);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(int formatId, object arg0, object arg1)
    {
      return string.Format(GUILocalizeStrings.Get(formatId), arg0, arg1);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(int formatId, object arg0, object arg1, object arg2)
    {
      return string.Format(GUILocalizeStrings.Get(formatId), arg0, arg1, arg2);
    }

    [XMLSkinFunction("string.format")]
    public static string FormatString(int formatId, params object[] args)
    {
      return string.Format(GUILocalizeStrings.Get(formatId), args);
    }

    [XMLSkinFunction("string.formatcount")]
    public static string FormatStringSingularPlural(int value, string multiFormat)
    {
      string[] formats = multiFormat.Split(new char[] {'|'});
      if (formats.Length != 3)
      {
        throw new ArgumentException("The value provided is not a 3-part format string", "multiFormat");
      }

      string format = (value == 0 ? formats[0] : (value == 1 ? formats[1] : formats[2]));
      return string.Format(format, value);
    }

    [XMLSkinFunction("string.formatcount")]
    public static string FormatStringSingularPlural(int value, int multiFormatId)
    {
      return FormatStringSingularPlural(value, GUILocalizeStrings.Get(multiFormatId));
    }

    [XMLSkinFunction("string.formatcount")]
    public static string FormatStringSingularPlural(string value, string multiFormat)
    {
      int intValue = int.Parse(value);
      return FormatStringSingularPlural(intValue, multiFormat);
    }

    [XMLSkinFunction("string.formatcount")]
    public static string FormatStringSingularPlural(string value, int multiFormatId)
    {
      int intValue = int.Parse(value);
      return FormatStringSingularPlural(intValue, GUILocalizeStrings.Get(multiFormatId));
    }

    [XMLSkinFunction("L")]
    public static string LocalizeString(int Id)
    {
      return GUILocalizeStrings.Get(Id);
    }

    [XMLSkinFunction("L")]
    public static string LocalizeString(string Id)
    {
      return GUILocalizeStrings.Get(int.Parse(Id));
    }

    [XMLSkinFunction("string.trim")]
    public static string TrimString(string text)
    {
      return text.Trim();
    }

    [XMLSkinFunction("string.trim")]
    public static string TrimString(string text, string charsToTrim)
    {
      return text.Trim(charsToTrim.ToCharArray());
    }

    [XMLSkinFunction("string.rtrim")]
    public static string RightTrimString(string text)
    {
      return text.TrimEnd();
    }

    [XMLSkinFunction("string.rtrim")]
    public static string RightTrimString(string text, string charsToTrim)
    {
      return text.TrimEnd(charsToTrim.ToCharArray());
    }

    [XMLSkinFunction("string.ltrim")]
    public static string LeftTrimString(string text)
    {
      return text.TrimStart();
    }

    [XMLSkinFunction("string.ltrim")]
    public static string LeftTrimString(string text, string charsToTrim)
    {
      return text.TrimStart(charsToTrim.ToCharArray());
    }

    #endregion

    #region Conversions

    [XMLSkinFunction("cint")]
    public static int ConvertToInt(object value)
    {
      return Convert.ToInt32(value);
    }

    [XMLSkinFunction("cflt")]
    public static float ConvertToFloat(object value)
    {
      return Convert.ToSingle(value);
    }

    [XMLSkinFunction("cdate")]
    public static DateTime ConvertToDate(object value)
    {
      return Convert.ToDateTime(value);
    }

    #endregion

    #region Conditionals

    [XMLSkinFunction("iif")]
    public static object Iif(bool condition, object truePart, object falsePart)
    {
      return condition ? truePart : falsePart;
    }

    [XMLSkinFunction("choose")]
    public static object Choose(int index, params object[] values)
    {
      return values[index];
    }

    [XMLSkinFunction("switch")]
    public static object Switch(params object[] args)
    {
      for (int i = 0; i < args.Length; i += 2)
      {
        if ((bool)args[i])
        {
          return args[i + 1];
        }
      }
      if ((args.Length & 1) != 0)
      {
        return args[args.Length - 1];
      }
      return null;
    }

    #endregion

    #region Comparisons

    //[XMLSkinFunction("eq")]
    //public static bool Equals(int arg1, int arg2)
    //{
    //  return arg1 == arg2;
    //}

    //[XMLSkinFunction("eq")]
    //public static bool Equals(float arg1, float arg2)
    //{
    //  return arg1 == arg2;
    //}

    //[XMLSkinFunction("eq")]
    //public static bool Equals(string arg1, string arg2)
    //{
    //  return arg1 == arg2;
    //}

    [XMLSkinFunction("eq")]
    public new static bool Equals(object arg1, object arg2)
    {
      return object.Equals(arg1, arg2);
    }

    [XMLSkinFunction("neq")]
    public static bool NotEquals(object arg1, object arg2)
    {
      return !object.Equals(arg1, arg2);
    }

    [XMLSkinFunction("gt")]
    public static bool GreaterThan(object arg1, object arg2)
    {
      IComparable op1 = arg1 as IComparable;
      IComparable op2 = arg2 as IComparable;

      if (op1 != null)
      {
        return op1.CompareTo(arg2) > 0;
      }
      if (op2 != null)
      {
        return op2.CompareTo(op1) < 0;
      }
      return false; // both are null or not comparable
    }

    [XMLSkinFunction("gte")]
    public static bool GreaterThanOrEqual(object arg1, object arg2)
    {
      IComparable op1 = arg1 as IComparable;
      IComparable op2 = arg2 as IComparable;

      if (op1 != null)
      {
        return op1.CompareTo(arg2) >= 0;
      }
      if (op2 != null)
      {
        return op2.CompareTo(op1) <= 0;
      }
      return true; // both are null or not comparable
    }

    [XMLSkinFunction("lt")]
    public static bool LessThan(object arg1, object arg2)
    {
      IComparable op1 = arg1 as IComparable;
      IComparable op2 = arg2 as IComparable;

      if (op1 != null)
      {
        return op1.CompareTo(arg2) < 0;
      }
      if (op2 != null)
      {
        return op2.CompareTo(op1) > 0;
      }
      return false; // both are null or not comparable
    }

    [XMLSkinFunction("lte")]
    public static bool LessThanOrEqual(object arg1, object arg2)
    {
      IComparable op1 = arg1 as IComparable;
      IComparable op2 = arg2 as IComparable;

      if (op1 != null)
      {
        return op1.CompareTo(arg2) <= 0;
      }
      if (op2 != null)
      {
        return op2.CompareTo(op1) >= 0;
      }
      return true; // both are null or not comparable
    }

    #endregion

    #region Boolean logic

    [XMLSkinFunction("not")]
    public static bool Not(bool condition)
    {
      return !condition;
    }

    [XMLSkinFunction("and")]
    public static bool And(params bool[] conditions)
    {
      for (int i = 0; i < conditions.Length; i++)
      {
        if (!conditions[i])
        {
          return false;
        }
      }
      return true;
    }

    [XMLSkinFunction("or")]
    public static bool Or(params bool[] conditions)
    {
      for (int i = 0; i < conditions.Length; i++)
      {
        if (conditions[i])
        {
          return true;
        }
      }
      return false;
    }

    #endregion

    #region Other functions

    // Other
    //[XMLSkinFunction("eval")]
    //public static object Evaluate(string expression)
    //{
    //  return GUIExpressionManager.ParseExpression(expression);
    //}

    #endregion

    #region Math related

    [XMLSkinFunction("neg")]
    public static int Negate(int arg)
    {
      return -arg;
    }

    [XMLSkinFunction("neg")]
    public static float Negate(float arg)
    {
      return -arg;
    }

    [XMLSkinFunction("add")]
    public static int Add(int arg0, int arg1)
    {
      return arg0 + arg1;
    }

    private static int Add(int arg0, params object[] args)
    {
      int sum = arg0;
      for (int i = 0; i < args.Length; i++)
      {
        sum += Convert.ToInt32(args[i]);
      }
      return sum;
    }

    [XMLSkinFunction("add")]
    public static float Add(float arg0, float arg1)
    {
      return arg0 + arg1;
    }

    private static float Add(float arg0, params object[] args)
    {
      float sum = arg0;
      for (int i = 0; i < args.Length; i++)
      {
        sum += Convert.ToSingle(args[i]);
      }
      return sum;
    }

    [XMLSkinFunction("add")]
    public static object Add(object arg0, params object[] args)
    {
      if (arg0 is int)
      {
        return Add((int)arg0, args);
      }
      return Add((float)arg0, args);
    }

    [XMLSkinFunction("sub")]
    public static int Subtract(int arg1, int arg2)
    {
      return arg1 - arg2;
    }

    [XMLSkinFunction("sub")]
    public static float Subtract(float arg1, float arg2)
    {
      return arg1 - arg2;
    }

    [XMLSkinFunction("mul")]
    public static int Multiply(int arg0, int arg1)
    {
      return arg0 * arg1;
    }

    [XMLSkinFunction("mul")]
    public static float Multiply(float arg0, float arg1)
    {
      return arg0 * arg1;
    }

    [XMLSkinFunction("mul")]
    public static object Multiply(object arg0, params object[] args)
    {
      if (arg0 is int)
      {
        return Multiply((int)arg0, args);
      }
      return Multiply((float)arg0, args);
    }

    private static int Multiply(int arg0, params object[] args)
    {
      int prod = arg0;
      for (int i = 0; i < args.Length; i++)
      {
        prod *= Convert.ToInt32(args[i]);
      }
      return prod;
    }

    private static float Multiply(float arg0, params object[] args)
    {
      float prod = arg0;
      for (int i = 0; i < args.Length; i++)
      {
        prod *= Convert.ToSingle(args[i]);
      }
      return prod;
    }

    [XMLSkinFunction("div")]
    public static int Divide(int arg1, int arg2)
    {
      return arg1 / arg2;
    }

    [XMLSkinFunction("div")]
    public static float Divide(float arg1, float arg2)
    {
      return arg1 / arg2;
    }

    [XMLSkinFunction("math.round")]
    public static float Round(float arg)
    {
      return (float)Math.Round(arg);
    }

    [XMLSkinFunction("math.round")]
    public static float Round(float arg, int decimals)
    {
      return (float)Math.Round(arg, decimals);
    }

    [XMLSkinFunction("math.ceil")]
    public static float Ceiling(float arg)
    {
      return (float)Math.Ceiling(arg);
    }

    [XMLSkinFunction("math.ceil")]
    public static float Ceiling(float arg, int decimals)
    {
      double scale = Math.Pow(10, decimals);
      return (float)(Math.Ceiling(arg * scale) / scale);
    }

    [XMLSkinFunction("math.floor")]
    public static float Floor(float arg)
    {
      return (float)Math.Floor(arg);
    }

    [XMLSkinFunction("math.floor")]
    public static float Floor(float arg, int decimals)
    {
      double scale = Math.Pow(10, decimals);
      return (float)(Math.Floor(arg * scale) / scale);
    }

    #endregion

    #region Date functions

    [XMLSkinFunction("date.add")]
    public static DateTime DateAdd(DateTime date, TimeSpan timeSpan)
    {
      return date.Add(timeSpan);
    }

    [XMLSkinFunction("date.add")]
    public static DateTime DateAdd(string interval, float number, DateTime date)
    {
      switch (interval.ToLowerInvariant())
      {
        case "d":
        case "dd":
        case "y":
        case "dy":
        case "w":
        case "dw":
          return date.AddDays(number);
        case "ww":
        case "wk":
          return date.AddDays(7 * number);
        case "m":
        case "mm":
          return date.AddMonths((int)number);
        case "q":
        case "qq":
          return date.AddMonths(3 * (int)number);
        case "yy":
        case "yyyy":
          return date.AddYears((int)number);
        case "h":
        case "hh":
          return date.AddHours(number);
        case "n":
        case "nn":
          return date.AddMinutes(number);
        case "s":
        case "ss":
          return date.AddSeconds(number);
        case "ms":
          return date.AddMilliseconds(number);
        default:
          throw new ArgumentException("Invalid parameter value", "interval");
      }
    }

    [XMLSkinFunction("date.sub")]
    public static TimeSpan DateSub(DateTime date1, DateTime date2)
    {
      return date1.Subtract(date2);
    }

    [XMLSkinFunction("date.sub")]
    public static DateTime DateSub(DateTime date, TimeSpan timeSpan)
    {
      return date.Subtract(timeSpan);
    }

    [XMLSkinFunction("date.extract")]
    public static int DateExtract(string interval, DateTime date)
    {
      switch (interval.ToLowerInvariant())
      {
        case "d":
        case "dd":
          return date.Day;
        case "y":
        case "dy":
          return date.DayOfYear;
        case "w":
        case "dw":
          return (int)date.DayOfWeek;
        case "ww":
        case "wk":
          return date.DayOfYear / 7;
        case "m":
        case "mm":
          return date.Month;
        case "q":
        case "qq":
          return (date.Month - 1) / 3 + 1;
        case "yy":
        case "yyyy":
          return date.Year;
        case "h":
        case "hh":
          return date.Hour;
        case "n":
        case "nn":
          return date.Minute;
        case "s":
        case "ss":
          return date.Second;
        case "ms":
          return date.Millisecond;
        default:
          throw new ArgumentException("Invalid parameter value", "interval");
      }
    }

    [XMLSkinFunction("date.extract")]
    public static float DateExtract(string interval, TimeSpan timeSpan)
    {
      switch (interval.ToLowerInvariant())
      {
        case "d":
        case "dd":
          return timeSpan.Days;
        case "ww":
        case "wk":
          return (float)timeSpan.Days / 7;
        case "h":
        case "hh":
          return timeSpan.Hours;
        case "n":
        case "nn":
          return timeSpan.Minutes;
        case "s":
        case "ss":
          return timeSpan.Seconds;
        case "ms":
          return timeSpan.Milliseconds;
        default:
          throw new ArgumentException("Invalid parameter value", "interval");
      }
    }

    #endregion

    #region Skin setting functions

    [XMLSkinFunction("skin.hassetting")]
    public static object SkinHasSetting(string setting)
    {
      int condition = GUIInfoManager.TranslateString("skin.hassetting(" + setting + ")");
      return GUIInfoManager.GetBool(condition, 0);
    }

    [XMLSkinFunction("skin.togglesetting")]
    public static object SkinToggleSetting(string setting)
    {
      // Toggle the boolean setting to the opposite value.
      int condition = GUIInfoManager.TranslateSingleString("skin.togglesetting(" + setting + ")");
      bool newValue = !GUIInfoManager.GetBool(condition, 0);
      GUIInfoManager.SetBool(condition, newValue, 0);
      return newValue;
    }

    [XMLSkinFunction("skin.setstring")]
    public static object SkinSetString(params object[] args)
    {
      // args[0] - setting name
      // args[1] - (optional) new value
      // args[2] - (optional) keyboard prompt
      string newValue = "";
 
      // Set the setting to the specified string.  If no value is specified then present the keyboard to input the value.
      if (args.Length == 2)
      {
        int condition = GUIInfoManager.TranslateSingleString("skin.setstring(" + args[0] + ")");
        newValue = args[1].ToString();
        GUIInfoManager.SetString(condition, newValue, 0);
      }
      else
      {
        // No value was provided for the skin setting.  Display a keyboard and ask for a value.
        string prompt = "";
        if (args.Length >= 3)
        {
          newValue = args[1].ToString();
          prompt = args[2].ToString();
          GUILocalizeStrings.LocalizeLabel(ref prompt);
        }

        // Get the current value to initialize the keyboard.
        int condition = GUIInfoManager.TranslateSingleString("skin.setstring(" + args[0] + "," + newValue + "," + prompt + ")");
        string userInput = GUIInfoManager.GetString(condition, 0);

        if (GetUserInputString(ref userInput, prompt))
        {
          GUIInfoManager.SetString(condition, userInput, 0);
        }
        else
        {
          // Keyboard cancelled; no value supplied and no input was entered into the keyboard.
        }
      }
      return newValue;
    }

    private static bool GetUserInputString(ref string sString, string label)
    {
      IStandardKeyboard keyboard = (IStandardKeyboard)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_VIRTUAL_KEYBOARD);
      if (null == keyboard)
      {
        return false;
      }
      keyboard.IsSearchKeyboard = true;
      keyboard.Reset();
      keyboard.Text = sString;
      keyboard.Label = label;
      keyboard.DoModal(GUIWindowManager.ActiveWindowEx);
      if (keyboard.IsConfirmed)
      {
        sString = keyboard.Text;
      }
      return keyboard.IsConfirmed;
    }

    [XMLSkinFunction("skin.setbool")]
    public static object SkinSetBool(params object[] args)
    {
      // args[0] - setting name
      // args[1] - (optional) new value
      bool newValue = true;

      if (args.Length == 2)
      {
        newValue = bool.Parse(args[1].ToString());
      }
      int condition = GUIInfoManager.TranslateSingleString("skin.setbool(" + args[0] + ")");
      GUIInfoManager.SetBool(condition, newValue, 0);
      return newValue;
    }

    [XMLSkinFunction("skin.reset")]
    public static object SkinReset(string setting)
    {
      // Resets the specifed setting.  Booleans are set false, strings are set to empty string.
      SkinSettings.ResetSkinBool(setting);
      SkinSettings.ResetSkinString(setting);
      SkinSettings.Save();
      return true;
    }

    [XMLSkinFunction("skin.resetsettings")]
    public static object SkinResetSettings()
    {
      // Resets all settings.  Booleans are set false, strings are set to empty string.
      SkinSettings.ResetAllSkinBool();
      SkinSettings.ResetAllSkinString();
      SkinSettings.Save();
      return true;
    }

    [XMLSkinFunction("skin.hastheme")]
    public static object SkinHasTheme(string theme)
    {
      int condition = GUIInfoManager.TranslateSingleString("skin.hastheme(" + theme + ")");
      return GUIInfoManager.GetBool(condition, 0);
    }

    [XMLSkinFunction("skin.theme")]
    public static object SkinTheme(params object[] args)
    {
      // args[0] - theme navigation direction; 1 moves to next, -1 moves to previous
      // args[1] - (optional) the control id to focus on after the theme has been changed
      int direction = 1;
      int focusControlId = 0;
      if (args.Length > 0)
      {
        direction = (int)args[0];

        if (args.Length > 1)
        {
          focusControlId = (int)args[1];
        }
      }

      return GUIThemeManager.ActivateThemeNext(direction, focusControlId);
    }

    [XMLSkinFunction("skin.settheme")]
    public static object SkinSetTheme(params object[] args)
    {
      // args[0] - new skin theme name
      // args[1] - (optional) the control id to focus on after the theme has been changed
      int focusControlId = 0;
      if (args.Length > 1)
      {
        focusControlId = (int)args[1];
      }

      return GUIThemeManager.ActivateThemeByName(args[0].ToString(), focusControlId);
    }

    #endregion

  }
}