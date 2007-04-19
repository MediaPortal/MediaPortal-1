using System;
using MediaLibrary;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Data.SQLite;
using System.Collections;

namespace SQLiteLibrary
{
    internal class SqlHelper
    {

        #region Static Methods

        public static string FixSQL(string str)
        {
            if (!IsNOE(str))
                return str.Replace("'", "''");
            return string.Empty;
        }
        
        public static string FixSQL(object obj)
        {
            string str = Convert.ToString(obj);
            return str.Replace("'", "''");
        }
        
        public static bool IsNOE(string str)
        {
            return (str == null || str == string.Empty);
        }
        
        public static bool IsNOE(object obj)
        {
            string str = Convert.ToString(obj);
            return (str == null || str == string.Empty);
        }

        public static string FilterString(string str, string tbl, IMLHashItem Tags)
        {
            if (str == null)
                return string.Empty;
            int num1 = str.IndexOf('{');
            int num2 = str.IndexOf('}');
            if (num1 != -1 && num2 != -1)
            {
                string tag = str.Substring(num1 + 1, num2 - num1 - 1);
                if (!IsNOE(tbl))
                    str = str.Replace("{" + tag + "}", tbl + "." + Convert.ToString(Tags[tag]));
                else
                    str = str.Replace("{" + tag + "}", Convert.ToString(Tags[tag]));
                str = FilterString(str, tbl, Tags);
            }
            return str;
        }

        public static string[] SplitListTags(string str)
        {
            if (str.IndexOf('|') >= 0)
            {
                string[] arr = str.Split('|');
                return arr;
            }
            else
            {
                string[] arr = new string[3];
                arr[1] = str;
                return arr;
            }
        }

        public static string GetAllTags(IMLHashItem Tags)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("items.item_id, ");
            sb.Append("items.item_name, ");
            sb.Append("items.item_location, ");
            sb.Append("items.item_ext_id, ");
            sb.Append("items.item_date, ");
            sb.Append("items.item_image, ");
            sb.Append("item_dates.created, ");
            sb.Append("item_dates.modified");
            for (int i = 0; i < Tags.Count; i++)
            {
                sb.Append(", items.");
                sb.Append(Tags[i]);
            }
            return sb.ToString();
        }
        
        public static string GetAllTagsAs(IMLHashItem Tags)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("items.item_id as ID, ");
            sb.Append("items.item_name as Name, ");
            sb.Append("items.item_location as Location, ");
            sb.Append("items.item_ext_id as ExtID, ");
            sb.Append("items.item_date as Timestamp, ");
            sb.Append("items.item_image as Image, ");
            sb.Append("item_dates.created as Created, ");
            sb.Append("item_dates.modified as Modified");
            ArrayList keys = Tags.Keys as ArrayList;
            for (int i = 0; i < Tags.Count; i++)
            {
                sb.Append(", items.");
                sb.Append(Tags[i]);
                sb.Append(" as ");
                sb.Append(keys[i].ToString());
            }
            return sb.ToString();
        }
        
        public static string GetItemTags(IMLHashItem Tags)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("items.item_id, ");
            sb.Append("items.item_name, ");
            sb.Append("items.item_location, ");
            sb.Append("items.item_ext_id, ");
            sb.Append("items.item_date, ");
            sb.Append("items.item_image ");
            for (int i = 0; i < Tags.Count; i++)
            {
                sb.Append(", items.");
                sb.Append(Tags[i]);
            }
            return sb.ToString();
        }

        public static string GetItemTagsAs(IMLHashItem Tags)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("items.item_id as ID, ");
            sb.Append("items.item_name as Name, ");
            sb.Append("items.item_location as Location, ");
            sb.Append("items.item_ext_id as ExtID, ");
            sb.Append("items.item_date as Timestamp, ");
            sb.Append("items.item_image as Image ");
            ArrayList keys = Tags.Keys as ArrayList;
            for (int i = 0; i < Tags.Count; i++)
            {
                sb.Append(", items.");
                sb.Append(Tags[i]);
                sb.Append(" as ");
                sb.Append(keys[i].ToString());
            }
            return sb.ToString();
        }

        #endregion
    }
}
