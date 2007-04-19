using System;
using MeediOS;
using MeediOS.Library.Database;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Collections;

namespace MsSqlLibrary
{
    internal class ListTagsDAL
    {
        #region Sql Commands

        private class Sql
        {
            //public static readonly string GetTagValues = "SELECT tag_values FROM list_tags WHERE tag_name = @TagName GROUP BY tag_values";
            //public static readonly string CreateListTagsIndex = "CREATE INDEX list_item_id ON list_tags (item_id)";
            public static readonly string InsertListTags = "INSERT INTO list_tags VALUES (@ItemID, @TagValue)";
            public static readonly string DeleteListTags = "DELETE FROM list_tags";
            public static readonly string DeleteListTagsByID = "DELETE FROM list_tags WHERE item_id = @ItemID";
            public static readonly string DeleteListTagsByTag = "DELETE FROM list_tags WHERE tag_name = @TagName";
            public static readonly string CreateListTagsTable = "CREATE TEMP TABLE list_tags ( item_id integer, tag_values char UNIQUE ON CONFLICT IGNORE, primary key ( item_id , tag_values ) );";
        }
        
        #endregion

        #region NonQuery Methods

        public static bool CreateListTagsTable(IDbCommand Command)
        {
            Command.CommandText = Sql.CreateListTagsTable;
            return DbProvider.ExecuteNonQuery(Command); ;
        }

        #region public static bool InsertListTags(IDbCommand Command, string TagCol, IMeedioItem Item)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagCol"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static bool InsertListTags(IDbCommand Command, IMeedioItem Item)
        {
            Command.CommandText = Sql.InsertListTags;
            IDbDataParameter IDParam = DbProvider.CreateParameter(Command, "@ItemID", "");
            IDbDataParameter ValueParam = DbProvider.CreateParameter(Command, "@TagValue", "");
            ArrayList keys = Item.Keys as ArrayList;
            for (int i = 0; i < Item.Count; i++)
            {
                IDParam.Value = Item[i];
                ValueParam.Value = Convert.ToString(keys[i]);
                DbProvider.ExecuteNonQuery(Command, false);
            }
            return true;
        }
        #endregion

        #region public static bool DeleteListTags(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static bool DeleteListTags(IDbCommand Command)
        {
            Command.CommandText = Sql.DeleteListTags;
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #endregion
    }
}
