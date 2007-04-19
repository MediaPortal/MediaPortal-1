using System;
using System.Collections.Generic;
using System.Text;
using MediaLibrary.Settings;

namespace MediaLibrary.Configuration
{
    /// <summary>
    /// This is a temporary class untill MLPluginWraper is completed to abstract
    /// out plugin management
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    public static class MLExtentsions
    {
        #region public static string[] Split(string source)
        /// <summary>
        /// Smart method of splitting a string.  Skips quoted elements, removes the quotes.
        /// </summary>
        /// <param name="source">Source string to split apart</param>
        /// <param name="separator">Separator character</param>
        /// <returns>A string array of the split up elements</returns>
        public static string[] Split(string source)
        {
            char[] toks = new char[2] { '\"', ',' };
            char[] quot = new char[1] { '\"' };
            int m = 0;
            int n = 0;
            List<string> ls = new List<string>();
            string s;

            while (source.Length > 0)
            {
                n = source.IndexOfAny(toks, n);
                if (n == -1) break;
                if (source[n] == toks[0])
                {
                    source = source.Remove(n, 1);
                    bool escapeChar = true;
                    while (escapeChar)
                    {
                        n = source.IndexOfAny(quot, n);
                        if (n == -1)
                        {
                            source = "\"" + source;
                            break;
                        }
                        if (escapeChar = (((n + 1) < source.Length) && (source[n + 1] == toks[0])))
                            n++;
                        source = source.Remove(n, 1);
                    }
                    if (n == -1) break;

                }
                else
                {
                    s = source.Substring(0, n).Trim();
                    source = source.Substring(n + 1).Trim();
                    if (s.Length > 0) ls.Add(s);
                    n = 0;
                }
            }
            if (source.Length > 0) ls.Add(source);

            string[] ar = new string[ls.Count];
            ls.CopyTo(ar, 0);

            return ar;
        }
        #endregion

        #region public static string Join(string[] Arr)
        /// <summary>
        /// Smart method of joining a string.  Quotes each line and escapes original quotes
        /// </summary>
        /// <param name="Arr"></param>
        /// <returns></returns>
        public static string Join(string[] Arr)
        {
            string rtn = "";
            if (Arr != null)
            {
                foreach (string str in Arr)
                {
                    if (string.IsNullOrEmpty(rtn))
                        rtn += "\"" + str.Replace("\"", "\"\"") + "\"";
                    else
                        rtn += ",\"" + str.Replace("\"", "\"\"") + "\"";
                }
            }
            return rtn;
        }
        #endregion

        #region public static bool SetProperties(IMLPlugin Plugin, IMLHashItem Properties, out string ErrorText)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Plugin"></param>
        /// <param name="Properties"></param>
        /// <param name="ErrorText"></param>
        /// <returns></returns>
        public static bool SetProperties(IMLPlugin Plugin, IMLHashItem Properties, out string ErrorText)
        {
            ErrorText = "";
            IMLPluginProperties Props = new MLPluginProperties();
            Plugin.GetProperties(Props);
            CastProperties(Props, Properties);
            return Plugin.SetProperties(Properties, out ErrorText);
        }
        #endregion

        #region public static void CastProperties(IMLPluginProperties Properties, IMLHashItem PropValues)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Properties"></param>
        /// <param name="PropValues"></param>
        public static void CastProperties(IMLPluginProperties Properties, IMLHashItem PropValues)
        {
            foreach (IMLPluginProperty Prop in Properties)
            {
                switch (Prop.DataType)
                {
                    case "folderlist":
                    case "stringlist":
                        PropValues[Prop.Name] = MLExtentsions.Split(Convert.ToString(PropValues[Prop.Name]));
                        break;
                    case "int":
                        PropValues[Prop.Name] = Convert.ToInt32(PropValues[Prop.Name]);
                        break;
                    case "float":
                        PropValues[Prop.Name] = Convert.ToDouble(PropValues[Prop.Name]);
                        break;
                    case "bool":
                        PropValues[Prop.Name] = Convert.ToBoolean(PropValues[Prop.Name]);
                        break;
                    case "time":
                    case "date":
                    case "string":
                    case "folder":
                    case "file":
                    case "custom":
                    default:
                        break;
                }
            }
        }
        #endregion
    }
}
