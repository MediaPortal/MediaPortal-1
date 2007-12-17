using System;
using MediaLibrary;
using MediaLibrary.Database;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Collections;
using System.Text;


namespace SQLiteLibrary
{
    public class MLItemDataSource : IMLItemDataSource
    {
        #region Members

        private IMLHashItem Tags;
        private DbProvider sqlProvider;

        #endregion

        #region Constructors

        public MLItemDataSource(DbProvider sqlProvider)
        {
            this.sqlProvider = sqlProvider;
            LoadTags();
        }

        #endregion

        #region View Functions

        public bool AddNewView(IMLView View)
        {
            bool rtn = false;
            int Order = ViewDAL.GetMaxViewOrder(sqlProvider.CreateCommand(false));
            if (Order > 0 && ViewDAL.InsertView(sqlProvider.CreateCommand(false), View.Name, Order))
            {
                ((MLView)View).ID = ViewDAL.GetMaxViewID(sqlProvider.CreateCommand());
                rtn = (View.ID > 0);
            }
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool UpdateView(string TagName, string TagValue, IMLView View)
        {
            bool rtn = ViewDAL.UpdateView(sqlProvider.CreateCommand(), TagName, TagValue, View.ID);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteView(IMLView View)
        {
            bool rtn = ViewDAL.DeleteView(sqlProvider.CreateCommand(), View.ID);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteAllViews()
        {
            bool rtn = ViewDAL.DeleteAllViews(sqlProvider.CreateCommand());
            sqlProvider.CloseConnection();
            return rtn;
        }

        public IMLView[] GetViews()
        {
            IMLView[] Views = ViewDAL.GetAllViews(sqlProvider.CreateCommand());
            foreach (IMLView View in Views)
                ((MLView)View)._Steps.AddRange(ViewStepDAL.GetStepsByViewID(sqlProvider.CreateCommand(), View.ID));
            sqlProvider.CloseConnection();
            return Views;
        }

        public bool MoveUp(IMLView View)
        {
            throw new Exception("This method has not been implemented");
        }
        
        public bool MoveDown(IMLView View)
        {
            throw new Exception("This method has not been implemented");
        }

        #endregion

        #region ViewStep Functions

        public bool AddNewStep(IMLViewStep Step)
        {
            bool rtn = false;
            if (ViewStepDAL.InsertStep(sqlProvider.CreateCommand(), Step.ViewID, Step.GroupTag))
            {
                ((MLViewStep)Step).ViewStepID = ViewStepDAL.GetMaxStepID(sqlProvider.CreateCommand());
                rtn = (Step.ViewStepID > 0);
            }
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool UpdateStep(string TagName, string TagValue, IMLViewStep Step)
        {
            bool rtn = ViewStepDAL.UpdateStep(sqlProvider.CreateCommand(), TagName, TagValue, Step.ViewID,Step.ViewStepID);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteStep(IMLViewStep Step)
        {
            bool rtn = ViewStepDAL.DeleteStep(sqlProvider.CreateCommand(),Step.ViewID, Step.ViewStepID);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteSteps(IMLView View)
        {
            bool rtn = ViewStepDAL.DeleteStepsByViewID(sqlProvider.CreateCommand(), View.ID);
            //Don't close connection since it's only called from GetViews and that closes the connection
            //sqlProvider.CloseConnection();
            return rtn;
        }

        public bool MoveUp(IMLViewStep Step)
        {
            throw new Exception("This method has not been implemented");
        }
       
        public bool MoveDown(IMLViewStep Step)
        {
            throw new Exception("This method has not been implemented");
        }

        #endregion

        #region Tags Functions

        private void LoadTags()
        {
            Tags = TagDAL.GetTags(sqlProvider.CreateCommand());
        }

        public string[] GetTagNames()
        {
            LoadTags();
            if(Tags != null && Tags.Count > 0)
                return (string[])(Tags.Keys as ArrayList).ToArray(typeof(string));
            sqlProvider.CloseConnection();
            return null;
        }

        public string[] GetTagValues(string TagName)
        {
            IMLHashItem Values = ItemDAL.GetGroupByItems(sqlProvider.CreateCommand(true),Tags, "", Convert.ToString(Tags[TagName]), false);
            if (Tags != null && Tags.Count > 0)
                return (string[])(Tags.Keys as ArrayList).ToArray(typeof(string));
            sqlProvider.CloseConnection();
            return null;
        }

        public bool AddNewTag(string TagName)
        {
            //check if tag exists
            if (Tags.Contains(TagName))
                return false;
            //get the id of first free tag
            int tagID = TagDAL.GetFreeTagID(sqlProvider.CreateCommand());
            if (tagID <= 0) //create new free tags if non exist
                tagID = CreateNewTags();
            //mar the free tag as used by TagName
            if(TagDAL.AddNewTag(sqlProvider.CreateCommand(), tagID, TagName))
            {
                LoadTags();
                return true;
            }
            return false;
        }

        private int CreateNewTags()
        {
            int max = TagDAL.GetMaxTagID(sqlProvider.CreateCommand());
            if(max >= 0)
            {
                //Insert 3 new tags 
                bool EndTransact = BeginUpdate();
                for (int i = (max + 1); i < (max + 4); i++)
                {	//Add  free tag to tags table
                    string CurrentTag = "tag_" + i;
                    TagDAL.InsertTag(sqlProvider.CreateCommand(),CurrentTag);
                    //Add col to items table
                    ItemDAL.AddTagCol(sqlProvider.CreateCommand(), CurrentTag);
                }
                if (EndTransact)
                    EndUpdate();
            }
            return max + 1;
        }

        public bool RenameTag(string OldTagName, string NewTagName)
        {
            bool rtn = TagDAL.RenameTag(sqlProvider.CreateCommand(), OldTagName, NewTagName);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteTag(string TagName)
        {
                //free the tag so it is unused
           bool rtn = (TagDAL.FreeTag(sqlProvider.CreateCommand(), TagName))
                //Clear the column in the items table
                && (ItemDAL.ClearTagCol(sqlProvider.CreateCommand(), TagName));
           sqlProvider.CloseConnection();
           return rtn;
        }

        #endregion

        #region Item Functions

        public bool AddNewItem(IMLItem Item)
        {
            if (ItemDAL.AddOrUpdateItem(sqlProvider.CreateCommand(InsertItemSql(Item)), Item))
                ((MLItem)Item).ID = ItemDAL.GetMaxItemID(sqlProvider.CreateCommand());
            sqlProvider.CloseConnection();
            return Item.ID > 0;
        }

        public bool UpdateItem(IMLItem Item)
        {
            bool rtn = ItemDAL.AddOrUpdateItem(sqlProvider.CreateCommand(UpdateItemSql(Item)), Item);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteItem(IMLItem Item)
        {
            bool rtn = ItemDAL.DeleteItem(sqlProvider.CreateCommand(), Item.ID);
            sqlProvider.CloseConnection();
            return rtn;
        }

        public bool DeleteAllItems()
        {
            bool rtn =  ItemDAL.DeleteAllItems(sqlProvider.CreateCommand());
            sqlProvider.CloseConnection();
            return rtn;
        }

        public IMLItemList GetAllItems()
        {
            IDbCommand Command = sqlProvider.CreateCommand();
            IMLItemList Items = ItemDAL.GetAllItems(Command, Tags);
            sqlProvider.CloseConnection(Command);

            return Items;
        }

        public int ItemCount()
        {
            int rtn = ItemDAL.GetItemCount(sqlProvider.CreateCommand());
            sqlProvider.CloseConnection();
            return rtn;
        }

        public int[] GetAllItemIDs()
        {
            IDbCommand Command = sqlProvider.CreateCommand(false);
            ArrayList IDs = ItemDAL.GetAllItemIDs(Command);
            sqlProvider.CloseConnection(Command);
            if (IDs == null)
                return null;
            else
                return (int[])IDs.ToArray(typeof(int));
        }

        public IMLItem FindItem(string Tag, string Value)
        {
            IMLItem Item = null;
            IDbCommand Command = sqlProvider.CreateCommand(true);
            if (Tag == "item_id")
                Item = ItemDAL.FindItemByID(Command, Tags, Value);
            else if (Tag == "item_location")
                Item = ItemDAL.FindItemByLocation(Command, Tags, Value);
            else if (Tag == "item_ext_id")
                Item = ItemDAL.FindItemByExtID(Command, Tags, Value);
            sqlProvider.CloseConnection(Command);
            return Item;
        }

        public IMLItemList Search(string TagName, string SearchString)
        {
            IMLItemList Items = null;
            IDbCommand Command = sqlProvider.CreateCommand(true);
            if (TagName == null)
                Items = ItemDAL.SearchAll(Command, Tags, SearchString);
            else if (TagName == "item_image")
                Items = ItemDAL.SearchByImage(Command, Tags, SearchString);
            else if (TagName == "item_location")
                Items = ItemDAL.SearchByLocation(Command, Tags, SearchString);
            else if (TagName == "item_name")
                Items = ItemDAL.SearchByTitle(Command, Tags, SearchString);
            else
                Items = ItemDAL.SearchByTag(Command, Tags, TagName, SearchString);
            sqlProvider.CloseConnection(Command);
            return Items;
        }

        public IMLItemList CustomSearch(string Filter, string GroupBy, string GroupFunc, string OrderBy, string OrderType, bool Asc)
        {
            string GroupByCol = SqlHelper.FilterString(GroupBy, "",Tags);
            string OrderByCol = SqlHelper.FilterString(OrderBy, "",Tags);
            bool UseListTags = !SqlHelper.IsNOE(GroupBy) && GroupByCol != "item_name";
            if (UseListTags)
            {
                IMLHashItem ListTags = ItemDAL.GetGroupByItems(sqlProvider.CreateCommand(true),Tags, Filter, GroupByCol, GroupFunc == "index");
                ListTagsDAL.CreateListTagsTable(sqlProvider.CreateCommand(true));
                ListTagsDAL.InsertListTags(sqlProvider.CreateCommand(true), ListTags);
            }
            //Test code to check contents of in-memorty temporary table
            //IMLHashItem ListTagsTable = ListTagsDAL.GetListTagsTable(sqlProvider.CreateCommand());
            IDbCommand Command = sqlProvider.CreateCommand(true);
            Command.CommandText = CustomSearchSql(Filter, GroupByCol, OrderByCol, OrderType, Asc);
            IMLItemList Items = ItemDAL.CustomSearch(Command, Tags);
            //In memory table should be dropped after connection is closed
            //if (UseListTags)
            //{
            //    ListTagsDAL.DeleteListTags(sqlProvider.CreateCommand(true));
            //}
            sqlProvider.CloseConnection(Command);
            return Items;
        }

        #region Sql Builder functions

        private string InsertItemSql(IMLItem Item)
        {
            ArrayList Keys = Item.Tags.Keys as ArrayList;
            StringBuilder tags = new StringBuilder("INSERT INTO items (item_name, item_location, item_ext_id, item_date, item_image");
            StringBuilder values = new StringBuilder(") VALUES (@Name, @Location, @ExtID, @Date, @Image");
            for (int i = 0; i < Item.Tags.Count; i++)
            {
                AddNewTag(Convert.ToString(Keys[i]));
                //TODO add validation to throw errors on invalid names
                tags.Append(", ");
                tags.Append(Tags[Convert.ToString(Keys[i])]);
                values.Append(", @param");
                values.Append(i);
            }
            return tags.Append(values.ToString() + ")").ToString();
        }

        private string UpdateItemSql(IMLItem Item)
        {
            ArrayList Keys = Item.Tags.Keys as ArrayList;
            StringBuilder sql = new StringBuilder("UPDATE items SET item_name = @Name, item_location = @Location, ");
            sql.Append("item_ext_id = @ExtID, item_date = @Date, item_image = @Image");
            for (int i = 0; i < Item.Tags.Count; i++)
            {
                AddNewTag(Convert.ToString(Keys[i]));
                //TODO add validation to throw errors on invalid names
                sql.Append(", ");
                sql.Append(Tags[Convert.ToString(Keys[i])]);
                sql.Append(" = @param");
                sql.Append(i);
            }
            sql.Append(" WHERE item_id = ");
            sql.Append(Item.ID);
            return sql.ToString();
        }

        private string CustomSearchSql(string Filter, string GroupByCol, string OrderByCol, string OrderType, bool Asc)
        {
            bool GroupSelect = !SqlHelper.IsNOE(GroupByCol) && GroupByCol != "item_name";
            bool OrderSelect = !SqlHelper.IsNOE(OrderByCol) && OrderByCol != "item_name" && OrderByCol != GroupByCol;
            string Select = "SELECT " + SqlHelper.GetAllTags(Tags);
            string From = " FROM items JOIN item_dates ON item_dates.item_id = items.item_id ";
            string Where = "";
            string GroupBy = "";
            string OrderBy = "";

            if (GroupSelect)
            {
                Select += ", list_tags.tag_values as caption ";
                From += " JOIN list_tags ON list_tags.item_id = items.item_id ";
            }
            Filter = SqlHelper.FilterString(Filter, "items",Tags);
            if (!SqlHelper.IsNOE(Filter))
                Where = " WHERE " + Filter;

            if (OrderByCol == "item_name")
                OrderBy = " ORDER BY items.item_name";
            else if (OrderByCol == GroupByCol)
                OrderBy = " ORDER BY caption";
            else
                OrderBy = " ORDER BY SortTag(" + OrderByCol + ")";

            if (OrderType == "int")
                OrderBy += " COLLATE BINARY";
            else if (OrderType == "date")
                OrderBy += " COLLATE BINARY";
            else
                OrderBy += " COLLATE NOCASE";

            if (Asc)
                OrderBy += " ASC";
            else
                OrderBy += " DESC";
            //return the created query
            return Select + From + Where + GroupBy + OrderBy;
        }

        #endregion

        #endregion

        #region DataSet Functions

        public IMLDataSet GetDataSet()
        {
            IDbDataAdapter da = sqlProvider.NewDataAdapter();
            da.SelectCommand = sqlProvider.CreateCommand();
            MLDataSet dataSet = new MLDataSet();
            dataSet.Dataset = ItemDAL.GetDataSet(da, Tags);
            sqlProvider.CloseConnection();
            return dataSet;
        }

        public void ReloadDataSet(IMLDataSet dataSet)
        {
            IDbDataAdapter da = sqlProvider.NewDataAdapter();
            da.SelectCommand = sqlProvider.CreateCommand();
            ((MLDataSet)dataSet).Dataset = ItemDAL.GetDataSet(da, Tags);
            sqlProvider.CloseConnection();
        }

        public bool UpdateDataSet(DataSet ds)
        {
            IDbDataAdapter da = sqlProvider.NewDataAdapter();
            da.SelectCommand = sqlProvider.CreateCommand();
            DbCommandBuilder CommandBld = sqlProvider.NewCommandBuilder();
            //commit transaction
            bool Transact = sqlProvider.BeginUpdate(true);
            if (ItemDAL.UpdateDataSet(da, CommandBld, ds, Tags) && Transact)
            {
                EndUpdate();
                return true;
            }
            CancelUpdate();
            sqlProvider.CloseConnection();
            return false;
        }

        #endregion

        #region Transaction Functions

        public bool BeginUpdate()
        {
            return sqlProvider.BeginUpdate();
        }

        public void CancelUpdate()
        {
            sqlProvider.CancelUpdate();
        }

        public void EndUpdate()
        {
            sqlProvider.EndUpdate();
        }

        //this function is not included in the interface
        //public void CustomUpdate(string Filter, IMLHashItem Item)
        //{
        //    string SetAll = "";
        //    ArrayList keys = Item.Keys as ArrayList;
        //    for (int i = 0; i < Item.Count; i++)
        //    {
        //        AddNewTag(keys[i].ToString());
        //        if (SetAll == "")
        //            SetAll = "SET " + SQLStatements.FixSQL(Tags[keys[i].ToString()]) + " = '"
        //                + SQLStatements.FixSQL(Item[i]) + "'";
        //        else
        //            SetAll += ", " + SQLStatements.FixSQL(Tags[keys[i].ToString()]) + " = '"
        //                + SQLStatements.FixSQL(Item[i]) + "'";
        //    }
        //    if (Filter != null)
        //        Filter = " WHERE " + FilterString(Filter, "");
        //    RunTransaction("UPDATE items " + SetAll + Filter);
        //}

        #endregion

        #region Functions to Delete

        #region In-memory-database functions - Status: 100%
        /// <summary>
        /// All objects cached in memory should re-sync with the database
        /// </summary>
        public void Refresh()
        {
            //if (ROConnection != null)
            //    ROConnection.Close();
            //else
            //{
            //    ROConnection = new SQLiteConnection();
            //    ROConnection.ConnectionString = SQLStatements.ROConnectionString;
            //}

            //ROConnection.Open();

            //Tags = (new MLHashItem()) as IMLHashItem;
            //LoadTags();
            //CopyToMem();
        }

        //private bool GetTableList(ref IMLHashItemList TableList)
        //{
        //    TableList = QueryTable(SQLStatements.SelectTables);
        //    return (TableList != null);
        //}

        //private bool GetIndexList(ref IMLHashItemList IndexList)
        //{
        //    IndexList = QueryTable(SQLStatements.SelectIndex);
        //    return (IndexList != null);
        //}

        private void CopyToMem()
        {
            //SQLiteCommand Command = null;
            //IMLHashItemList TableList = null;
            //IMLHashItemList IndexList = null;
            //SQLiteTransaction Transact = null;
            //try
            //{
            //    Command = GetCommand(true);
            //    if (GetTableList(ref TableList))
            //    {
            //        Transact = (SQLiteTransaction)ROConnection.BeginTransaction();
            //        for (int i = 0; i < TableList.Count; i++)
            //        {
            //            Command.CommandText = Convert.ToString(TableList[i]["sql"]);
            //            Command.ExecuteNonQuery();
            //        }
            //        if (GetIndexList(ref IndexList))
            //        {

            //            for (int i = 0; i < IndexList.Count; i++)
            //            {
            //                Command.CommandText = Convert.ToString(IndexList[i]["sql"]);
            //                Command.ExecuteNonQuery();
            //            }
            //        }
            //        Transact.Commit();
            //        Command.CommandText = SQLStatements.AttachDB(this.Section.FileName);
            //        Command.ExecuteNonQuery();
            //        for (int i = 0; i < TableList.Count; i++)
            //        {
            //            string CurrentTable = Convert.ToString(TableList[i]["name"]);
            //            Command.CommandText = SQLStatements.CreateTable(CurrentTable);
            //            Command.ExecuteNonQuery();
            //        }
            //        Command.CommandText = SQLStatements.DetachDB;
            //        Command.ExecuteNonQuery();
            //    }
            //}
            //catch (Exception e) { Console.Write("Exception: " + e.Message); }
            //finally
            //{
            //    if (Command != null) Command.Dispose();
            //    if (Transact != null) Transact.Dispose();
            //}
        }

        #endregion

        #endregion
    }   
}
