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
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.Library
{
  class GUIFunctions
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
      string[] formats = multiFormat.Split(new char[]{'|'});
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

    #endregion

    #region Conversions

    [XMLSkinFunction("cint")]
    public static int ConvertToInt(object value)
    {
      return Convert.ToInt32(value);
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
      for(int i = 0; i < args.Length; i+=2)
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
    public static new bool Equals(object arg1, object arg2)
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
      for(int i = 0; i<conditions.Length; i++)
      {
        if(!conditions[i])
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
    [XMLSkinFunction("eval")]
    public static object Evaluate(string expression)
    {
      return GUIExpressionManager.ParseExpression(expression);
    }
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
    public static int Add(params int[] args)
    {
      int sum = 0;
      for (int i = 0; i < args.Length; i++)
      {
        sum += args[i];
      }
      return sum;
    }

    [XMLSkinFunction("add")]
    public static float Add(params float[] args)
    {
      float sum = 0;
      for (int i = 0; i < args.Length; i++)
      {
        sum += args[i];
      }
      return sum;
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
    public static int Multiply(params int[] args)
    {
      int prod = 1;
      for (int i = 0; i < args.Length; i++)
      {
        prod *= args[i];
      }
      return prod;
    }

    [XMLSkinFunction("mul")]
    public static float Multiply(params float[] args)
    {
      float prod = 1;
      for (int i = 0; i < args.Length; i++)
      {
        prod *= args[i];
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

    #endregion

  }
}
