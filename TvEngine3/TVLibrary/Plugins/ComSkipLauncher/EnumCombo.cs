using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TvEngine
{
  public static class EnumCombo
  {
    /// <summary>
    /// Populates a combobox with the values from an enum
    /// </summary>
    /// <typeparam name="T">The enum type</typeparam>
    /// <param name="combobox">The combobox to populate</param>
    public static void PopulateCombo<T>(ComboBox combobox)
    {
      if (!typeof(T).IsEnum)
      {
        throw new ArgumentException("T must be an enumerated type");
      }

      List<KeyValuePair<T, string>> options = new List<KeyValuePair<T, string>>();

      foreach (var en in Enum.GetValues(typeof(T))) {
        var s = en.ToString();
        // Insert a space between a lower case and upper case ("BelowNormal" becomes "Below Normal")
        s = Regex.Replace(s, "([a-z])([A-Z])", "$1 $2");
        options.Add(new KeyValuePair<T,string>((T)en, s));
      }

      combobox.DataSource = options;
      combobox.DisplayMember = "Value";
      combobox.ValueMember = "Key"; 
    }
  }
}
