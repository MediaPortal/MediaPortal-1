using System;
using MediaLibrary;
using MediaLibrary.Database;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Collections;

namespace SQLiteLibrary
{
    internal class ListTagsDAL
    {
        #region Sql Commands

        private class Sql
        {
            public static readonly string GetListTags = "SELECT item_id, tag_values FROM list_tags";
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

        public static IMLHashItem GetListTagsTable(IDbCommand Command)
        {
            Command.CommandText = Sql.GetListTags;
            return FillGroupByItems(DbProvider.ExecuteDataReader(Command));
        }

        #region public static bool InsertListTags(IDbCommand Command, string TagCol, IMLHashItem Item)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagCol"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static bool InsertListTags(IDbCommand Command, IMLHashItem Item)
        {
            Command.CommandText = Sql.InsertListTags;
            IDbDataParameter IDParam = DbProvider.AddParameter(Command, "@ItemID", "");
            IDbDataParameter ValueParam = DbProvider.AddParameter(Command, "@TagValue", "");
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

        #region private static IMLHashItem FillGroupByItems(IDataReader Reader, bool Index)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        private static IMLHashItem FillGroupByItems(IDataReader Reader)
        {
            IMLHashItem Item = (new MLHashItem()) as IMLHashItem;
            try
            {
                while (Reader.Read())
                {
                    Item["item_id"] = Reader.GetInt32(0);
                    Item["tag_value"] = Convert.ToString(Reader.GetValue(1));
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return Item;
        }
        #endregion
    }
}
