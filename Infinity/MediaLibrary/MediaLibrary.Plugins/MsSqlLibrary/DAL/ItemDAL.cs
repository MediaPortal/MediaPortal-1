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
    /// <summary>
    /// 
    /// </summary>
    /// <remarks></remarks>
    /// <example></example>
    internal class ItemDAL
    {
        #region Sql Commands

        private class Sql
        {
            public static string CreateIndex(string CurrentTag)
            {
                return "create index item_" + CurrentTag + " on items ( " + CurrentTag + " );";
            }
            public static readonly string DeleteAllItems = "DELETE FROM items";
            public static readonly string DeleteItem = "DELETE FROM items WHERE item_id = @ItemID";
            public static readonly string GetAllItemIDs = "SELECT item_id FROM items";
            public static readonly string GetMaxItemID = "SELECT max(item_id) FROM items";
            public static readonly string GetItemCount = "SELECT count(item_id) FROM items";
            public static string GetAllItems(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id ";
            }
            public static string GetAllItemsAs(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetItemTagsAs(Tags) + " FROM items";
            }
            public static string SearchAll(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id WHERE SearchTag( @TagValue, "
                    + SqlHelper.GetAllTags(Tags) + " ) ";

            }
            public static string SearchByImage(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id WHERE items.item_image = @TagValue ";
            }
            public static string SearchByLocation(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id WHERE items.item_location = @TagValue";
            }
            public static string SearchByTag(IMeedioItem Tags, string CurrentTag)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id WHERE SearchTag( @TagValue, " 
                    + CurrentTag + " ) ";

            }
            public static string SearchByTitle(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id  WHERE items.item_name = @TagValue";
            }
            public static string FindItemByID(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON" 
                    + " item_dates.item_id = items.item_id WHERE items.item_id = @TagValue";

            }
            public static string FindItemByLocation(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON"
                    + " item_dates.item_id = items.item_id WHERE items.item_location = @TagValue";
            }
            public static string FindItemByExtID(IMeedioItem Tags)
            {
                return "SELECT " + SqlHelper.GetAllTags(Tags) + " FROM items JOIN item_dates ON"
                    + " item_dates.item_id = items.item_id WHERE items.item_ext_id = @TagValue";
            }
            public static string AddTagCol(string CurrentTag)
            {
                return "ALTER TABLE items ADD " + SqlHelper.FixSQL(CurrentTag) + " char";
            }
            public static string FreeTagCol(string CurrentTag)
            {
                return "UPDATE items SET " + SqlHelper.FixSQL(CurrentTag) + " = ''";
            }
            public static string GetGroupByItems(string Filter, string GroupBy, IMeedioItem Tags)
            {
                string Select = "SELECT item_id, " + GroupBy + " as caption FROM items";
                string Where = "";
                Filter = SqlHelper.FilterString(Filter, "", Tags);
                if (!SqlHelper.IsNOE(Filter))
                    Where = " WHERE " + Filter;
                return Select + Where + " GROUP BY caption";
            }
        }

        #endregion

        #region Scalar Methods

        #region public static int GetItemCount(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetItemCount(IDbCommand Command)
        {
            Command.CommandText = Sql.GetItemCount;
            return DbProvider.ExecuteScalar(Command);
        }
        #endregion

        #region public static int GetMaxItemID(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetMaxItemID(IDbCommand Command)
        {
            Command.CommandText = Sql.GetMaxItemID;
            return DbProvider.ExecuteScalar(Command);
        }
        #endregion

        #endregion

        #region NonQuery Methods

        #region public static bool AddTagCol(IDbCommand Command, string TagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static bool AddTagCol(IDbCommand Command, string TagName)
        {
            Command.CommandText = Sql.AddTagCol(TagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool ClearTagCol(IDbCommand Command, string TagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static bool ClearTagCol(IDbCommand Command, string TagName)
        {
            Command.CommandText = Sql.FreeTagCol(TagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool CreateIndex(IDbCommand Command, string TagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static bool CreateIndex(IDbCommand Command, string TagName)
        {
            Command.CommandText = Sql.CreateIndex(TagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool AddOrUpdateItem(IDbCommand Command, IMLItem Item)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Item"></param>
        /// <returns></returns>
        public static bool AddOrUpdateItem(IDbCommand Command, IMLItem Item)
        {
            DbProvider.AddParameter(Command, "@Name", Item.Name);
            DbProvider.AddParameter(Command, "@Location", Item.Location);
            DbProvider.AddParameter(Command, "@Date", Item.TimeStamp.ToOADate());
            DbProvider.AddParameter(Command, "@ExtID", Item.ExternalID);
            DbProvider.AddParameter(Command, "@Image", Item.ImageFile);
            for (int i = 0; i < Item.Tags.Count; i++)
            {
                DbProvider.AddParameter(Command, "@param" + i, Convert.ToString(Item.Tags[i]));
            }
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool DeleteItem(IDbCommand Command, int ItemID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        public static bool DeleteItem(IDbCommand Command, int ItemID)
        {
            Command.CommandText = Sql.DeleteItem;
            DbProvider.AddParameter(Command, "@ItemID", ItemID);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool DeleteAllItems(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static bool DeleteAllItems(IDbCommand Command)
        {
            Command.CommandText = Sql.DeleteAllItems;
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        public static bool UpdateDataSet(IDbDataAdapter Adapter, DbCommandBuilder Builder,
            DataSet dataSet, IMeedioItem Tags)
        {
            Adapter.SelectCommand.CommandText = Sql.GetAllItemsAs(Tags);
            Builder.DataAdapter = Adapter as DbDataAdapter;
            Adapter.UpdateCommand = Builder.GetUpdateCommand(true);
            Adapter.InsertCommand = Builder.GetInsertCommand(true);
            Adapter.DeleteCommand = Builder.GetDeleteCommand(true);
            return DbProvider.UpdateDataAdapter(Adapter, dataSet);
        }

        public static DataSet GetDataSet(IDbDataAdapter Adapter, IMeedioItem Tags)
        {
            Adapter.SelectCommand.CommandText = Sql.GetAllItemsAs(Tags);
            DataSet dataSet = new DataSet();
            if (DbProvider.FillDataAdapter(Adapter, dataSet))
                return dataSet;
            return null;
        }

        #endregion

        #region DataReader Methods

        #region public static ArrayList GetAllItemIDs(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static ArrayList GetAllItemIDs(IDbCommand Command)
        {
            Command.CommandText = Sql.GetAllItemIDs;
            return FillArrayList(DbProvider.ExecuteDataReader(Command), true);
        }
        #endregion

        #region public static IMLItem FindItemByID(IDbCommand Command, IMeedioItem Tags, string Value)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IMLItem FindItemByID(IDbCommand Command, IMeedioItem Tags, string Value)
        {
            Command.CommandText = Sql.FindItemByID(Tags);
            DbProvider.AddParameter(Command, "@TagValue", Value);
            return FillItem(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItem FindItemByLocation(IDbCommand Command, IMeedioItem Tags, string Value)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IMLItem FindItemByLocation(IDbCommand Command, IMeedioItem Tags, string Value)
        {
            Command.CommandText = Sql.FindItemByLocation(Tags);
            DbProvider.AddParameter(Command, "@TagValue", Value);
            return FillItem(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItem FindItemByExtID(IDbCommand Command, IMeedioItem Tags, string Value)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static IMLItem FindItemByExtID(IDbCommand Command, IMeedioItem Tags, string Value)
        {
            Command.CommandText = Sql.FindItemByExtID(Tags);
            DbProvider.AddParameter(Command, "@TagValue", Value);
            return FillItem(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion 

        #region public static IMLItemList GetAllItems(IDbCommand Command, IMeedioItem Tags)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <returns></returns>
        public static IMLItemList GetAllItems(IDbCommand Command, IMeedioItem Tags)
        {
            Command.CommandText = Sql.GetAllItems(Tags);
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItemList SearchAll(IDbCommand Command, IMeedioItem Tags, string SearchString)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static IMLItemList SearchAll(IDbCommand Command, IMeedioItem Tags, string SearchString)
        {
            Command.CommandText = Sql.SearchAll(Tags);
            DbProvider.AddParameter(Command, "@TagValue", SearchString);
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItemList SearchByImage(IDbCommand Command, IMeedioItem Tags, string SearchString)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static IMLItemList SearchByImage(IDbCommand Command, IMeedioItem Tags, string SearchString)
        {
            Command.CommandText = Sql.SearchByImage(Tags);
            DbProvider.AddParameter(Command, "@TagValue", SearchString);
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItemList SearchByLocation(IDbCommand Command, IMeedioItem Tags, string SearchString)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static IMLItemList SearchByLocation(IDbCommand Command, IMeedioItem Tags, string SearchString)
        {
            Command.CommandText = Sql.SearchAll(Tags);
            DbProvider.AddParameter(Command, "@TagValue", SearchString);
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItemList SearchByTag(IDbCommand Command, IMeedioItem Tags, string TagName, string SearchString)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="TagName"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static IMLItemList SearchByTag(IDbCommand Command, IMeedioItem Tags, string TagName, string SearchString)
        {
            Command.CommandText = Sql.SearchByTag(Tags, TagName);
            DbProvider.AddParameter(Command, "@TagValue", SearchString);
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItemList SearchByTitle(IDbCommand Command, IMeedioItem Tags, string SearchString)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        public static IMLItemList SearchByTitle(IDbCommand Command, IMeedioItem Tags, string SearchString)
        {
            Command.CommandText = Sql.SearchByTitle(Tags);
            DbProvider.AddParameter(Command, "@TagValue", SearchString);
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMLItemList CustomSearch(IDbCommand Command, IMeedioItem Tags)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <returns></returns>
        public static IMLItemList CustomSearch(IDbCommand Command, IMeedioItem Tags)
        {
            return FillItems(DbProvider.ExecuteDataReader(Command), Tags.Keys as ArrayList);
        }
        #endregion

        #region public static IMeedioItem GetGroupByItems(IDbCommand Command, IMeedioItem Tags, string Filter, string GroupByCol, bool Index)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Tags"></param>
        /// <param name="Filter"></param>
        /// <param name="GroupByCol"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        public static IMeedioItem GetGroupByItems(IDbCommand Command, IMeedioItem Tags, string Filter, string GroupByCol, bool Index)
        {
            Command.CommandText = Sql.GetGroupByItems(Filter, GroupByCol, Tags);
            return FillGroupByItems(DbProvider.ExecuteDataReader(Command), Index);
        }
        #endregion

        #endregion

        #region Fill Methods

        #region private static ArrayList FillArrayList(IDataReader Reader)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static ArrayList FillArrayList(IDataReader Reader, bool IsInt)
        {
            ArrayList List = new ArrayList();
            while (Reader.Read())
                if(IsInt)
                    List.Add(Reader.GetInt32(0));
                else
                    List.Add(Reader.GetString(0));
            return List;
        }
        #endregion

        #region private static IMeedioItem FillGroupByItems(IDataReader Reader, bool Index)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <param name="Index"></param>
        /// <returns></returns>
        private static IMeedioItem FillGroupByItems(IDataReader Reader, bool Index)
        {
            IMeedioItem Item = (new MeedioItem()) as IMeedioItem;
            try
            {
                while (Reader.Read())
                {
                    int id = Reader.GetInt32(0);
                    string[] arr = SqlHelper.SplitListTags(Convert.ToString(Reader.GetValue(1)));
                    for (int k = 1; k < arr.Length - 1; k++)
                    {
                        if (Index)
                            Item[arr[k].Substring(0, 1)] = id;
                        else
                            Item[arr[k]] = id;
                    }
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return Item;
        }
        #endregion

        #region private static IMLItemList FillItems(IDataReader Reader, ArrayList Keys)
        /// <summary>
        /// Creates an IMLItemList object populated from the data reader
        /// </summary>
        /// <param name="Reader"></param>
        /// <param name="Keys"></param>
        /// <returns></returns>
        private static IMLItemList FillItems(IDataReader Reader, ArrayList Keys)
        {
            MLItemList ItemList = new MLItemList();
            try
            {
                while (Reader.Read())
                {
                    int i = 0;
                    MLItem Item = new MLItem();
                    Item.ID = Reader.GetInt32(i++);
                    Item.Name = Reader.GetString(i++);
                    Item.Location = Reader.GetString(i++);
                    Item.ExternalID = Reader.GetString(i++);
                    Item.TimeStamp = DateTime.FromOADate(Reader.GetDouble(i++));
                    Item.ImageFile = Reader.GetString(i++);
                    Item.DateCreated = DateTime.FromOADate(Reader.GetDouble(i++));
                    Item.DateChanged = DateTime.FromOADate(Reader.GetDouble(i++));
                    for (; i < Reader.FieldCount; i++)
                    {
                        if (i == Reader.FieldCount && Keys.Count < Reader.FieldCount)
                            Item.Tags["caption"] = Reader.GetValue(i); //get the group_by field
                        //TODO Return sort_by field 
                        else
                            Item.Tags[Keys[i-8].ToString()] = Reader[i];
                    }
                    ItemList.Items.Add(Item);
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return ItemList as IMLItemList;
        }
        #endregion

        #region private static IMLItem FillItem(IDataReader Reader, ArrayList Keys)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static IMLItem FillItem(IDataReader Reader, ArrayList Keys)
        {
            MLItem Item = null;
            try
            {
                if (Reader.Read())
                {
                    int i = 0;
                    Item = new MLItem();
                    Item.ID = Reader.GetInt32(i++);
                    Item.Name = Reader.GetString(i++);
                    Item.Location = Reader.GetString(i++);
                    Item.ExternalID = Reader.GetString(i++);
                    Item.TimeStamp = DateTime.FromOADate(Reader.GetDouble(i++));
                    Item.ImageFile = Reader.GetString(i++);
                    Item.DateCreated = DateTime.FromOADate(Reader.GetDouble(i++));
                    Item.DateChanged = DateTime.FromOADate(Reader.GetDouble(i++));
                    for (i = 0; i < Reader.FieldCount; i++)
                        Item.Tags[Keys[i].ToString()] = Reader[i];
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return Item as IMLItem;
        }
        #endregion
        
        #endregion
    }
}
