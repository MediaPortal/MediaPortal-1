using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Collections;

namespace MediaLibrary.Database.Providers
{
    public class SQLiteDbProvider : DbProvider
    {
        #region Sql Commands
        private class Sql
        {
            public static readonly string GetROConnectionString = "Data Source=:memory:;Version=3;New=True;Compress=False;Synchronous=Off";

            public static string GetConnectionString(string DataSource)
            {
                if (System.IO.File.Exists(DataSource))
                    return "Data Source=" + DataSource + ";Version=3;New=False;Compress=False;Synchronous=Off;";
                else
                    return "Data Source=" + DataSource + ";Version=3;New=True;Compress=False;Synchronous=Off;";
            }


            //public static string LoadFromAttached(string CurrentTable)
            //{
            //    return "INSERT INTO " + FixSQL(CurrentTable) + " SELECT * FROM loadfrom." + FixSQL(CurrentTable);
            //}
            //public static string AttachDB(string FileName)
            //{
            //    return "ATTACH DATABASE '" + FixSQL(FileName) + "' AS loadfrom";
            //}
            //public static readonly string SelectTables = "SELECT name,sql FROM sqlite_master WHERE type = 'table'";
            //public static readonly string SelectIndex = "SELECT sql FROM sqlite_master WHERE type = 'index'";
            //public static readonly string DetachDB = "DETACH DATABASE loadfrom";

            #region Create Items Tables
            public static string[] CreateItems = 
             { "BEGIN TRANSACTION; "
             , " create table group_items ( group_id    integer , item_id     integer , item_order  integer );"
             , " create table groups ( group_id    integer primary key , group_name  char );"
             , " create table item_dates ( item_id       integer primary key, created       integer, modified      integer );"
             , " create table items ( item_id       integer primary key , item_name     char , item_location char , item_ext_id   char , item_date     integer, item_image    char, tag_1         char, tag_2         char, tag_3         char );"
             , " create table parameters(param_name    char primary key ,param_value   char);"
             , " INSERT INTO parameters VALUES('version',1);"
             , " INSERT INTO parameters VALUES('data-version',1);"
             , " create table tags ( tag_id      integer primary key, tag_name    char, tag_col     char, tag_free    integer );"
             , " INSERT INTO tags VALUES(1,NULL,'tag_1',1);"
             , " INSERT INTO tags VALUES(2,NULL,'tag_2',1);"
             , " INSERT INTO tags VALUES(3,NULL,'tag_3',1);"
             , " create table view_steps(view_step_id   integer primary key,view_id        integer , group_tag      char , group_func     char, mode           char,sort_tag       char, sort_asc       char, sort_type      char );"
             , " create table views ( view_id        integer primary key , view_name      char, view_order     integer, view_filter    char, custom_1       char, custom_2       char);"
                //create defualt indecies
             , " create index item_tag_1 on items ( tag_1 );"
             , " create index item_tag_2 on items ( tag_2 );"
             , " create index item_tag_3 on items ( tag_3 );"
             , " create index items_ext_id_idx on items (item_ext_id );"
             , " create index items_location_idx on items ( item_location );"
                 //create triggers on db
             , " create trigger groups_delete before delete on groups "
                 + "begin " 
                 + "delete from group_items where group_id = old.group_id; "
                 + "end;"
             , " create trigger item_dates_delete after delete on items " 
                 + "begin " 
                 + "delete from item_dates where item_id = old.item_id; " 
                 + "update parameters set param_value = param_value + 1 where param_name = \"data-version\"; " 
                 + "end;"
             , " create trigger item_dates_insert after insert on items "
                 + "begin " 
                 + "insert or replace into item_dates ( item_id , created , modified ) values ( new.item_id , idt_now() , idt_now() ); " 
                 + "update parameters set param_value = param_value + 1 where param_name = \"data-version\"; " 
                 + "end;"
             , " create trigger item_dates_update after update on items " 
                 + "begin " 
                 + "update item_dates set modified = idt_now() where item_id = new.item_id; " 
                 + "update parameters set param_value = param_value + 1 where param_name = \"data-version\"; " 
                 + "end;"
             , " create trigger items_delete before delete on items " 
                 + "begin " 
                 + "delete from group_items where item_id = old.item_id; " 
                 + "end;"
             , " create trigger views_delete before delete on views "
                 + "begin " 
                 + "delete from view_steps where view_id = old.view_id; " 
                 + "end;"
                 //Add default pragmas
             , " PRAGMA default_temp_store = 2;"
             , " COMMIT;"};
            #endregion

            #region Create Imports Tables
            public static string[] CreateImports = 
            {" BEGIN TRANSACTION;"
            ," create table import_props(import_id         integer, prop_name         char, prop_value        char);"
            ," create table imports(import_id         integer primary key, name              char, section           char, update_mode       char, plugin_id         char, schedule_freq     char,schedule_interval integer, schedule_time     char, last_run_dtm      integer, wake_up           integer, run_missed        integer);"
            ," create table parameters(param_name    char primary key , param_value   char);"
            ," INSERT INTO parameters VALUES('version',1);"
            ," create trigger imports_delete before delete on imports "
                + "begin " 
                + "delete from import_props where import_id = old.import_id; " 
                + "end;"
            ," COMMIT;"};
            #endregion
        }
        #endregion

        #region Constructors

        public SQLiteDbProvider() { }

        /// <summary>
        /// Constructor used when opening or creating a section
        /// </summary>
        /// <param name="DataSource">file path</param>
        /// <param name="Properties"></param>
        public SQLiteDbProvider(string DataSource, IMLHashItem Properties)
        {
            if (!string.IsNullOrEmpty(DataSource))
            {
                this._ConnectionString = Sql.GetConnectionString(DataSource);
                CreateTables(DataSource);
                this._ConnectionString = Sql.GetConnectionString(DataSource);
                this._UseInMemDb = false;
                this._InProgress = false;
            }
        }

        #endregion

        #region Implemented Abstract Methods

        public override IDbConnection NewConnection()
        {
            return new SQLiteConnection();
        }

        public override IDbCommand NewCommand()
        {
            return new SQLiteCommand();
        }

        public override IDbDataAdapter NewDataAdapter()
        {
            return new SQLiteDataAdapter();
        }

        public override DbCommandBuilder NewCommandBuilder()
        {
            return new SQLiteCommandBuilder();
        }

        internal override ArrayList GetSections(IMLHashItem Properties)
        {
            string path = Convert.ToString(Properties["Path"]);
            try
            {
                ArrayList sections = new ArrayList();
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
                foreach (System.IO.FileInfo file in dir.GetFiles("*.mlf", System.IO.SearchOption.TopDirectoryOnly))
                {
                    string name = file.Name;
                    name = name.Substring(0, name.LastIndexOf(".mlf"));
                    sections.Add(name);
                }
                return sections;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return null;
        }

        internal override bool DeleteSection(string SectionName, IMLHashItem Properties)
        {
            try
            {
                string path = Convert.ToString(Properties["Path"]);
                if (!path.EndsWith("\\"))
                    path = path + "\\";
                System.IO.File.Delete(path + SectionName + ".mlf");
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            return false;
        }

        protected override void SetMemTable(IDbCommand Command)
        {
            Command.CommandText = "PRAGMA temp_store = MEMORY";
            DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region Private Methods

        private void CreateTables(string DataSource)
        {
            if (!System.IO.File.Exists(DataSource))
            {
                SQLiteConnection Connection = new SQLiteConnection();
                Connection.ConnectionString = this.ConnectionString;
                SQLiteCommand Command = null;
                try
                {
                    Connection.Open();
                    Command = Connection.CreateCommand();
                    string[] CreateCommands = null;
                    if (DataSource.EndsWith(".imports"))
                        CreateCommands = Sql.CreateImports;
                    else if (DataSource.EndsWith(".mlf"))
                        CreateCommands = Sql.CreateItems;
                    for (int i = 0; i < CreateCommands.Length; i++)
                    {
                        Command.CommandText = CreateCommands[i];
                        Command.ExecuteNonQuery();
                    }
                }
                catch (Exception e) { Console.Write("Exception: " + e.Message); }
                finally
                {
                    if (Command != null) Command.Dispose();
                    if (Connection != null)
                    {
                        Connection.Close();
                        Connection.Dispose();
                    }
                }
            }
        }

        #endregion

        #region User Defined Functions

        [SQLiteFunction(Name = "SortTag", Arguments = 1, FuncType = FunctionType.Scalar)]
        class SortTagFunc : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                if (args[0].GetType() != typeof(string))
                    return args[0];
                string str = (string)args[0];
                if (str == null || str == string.Empty)
                    return args[0];
                if (str.StartsWith("|"))
                    return str.Split('|')[1];
                else
                    return args[0];
            }
        }

        [SQLiteFunction(Name = "idt_now", Arguments = 0, FuncType = FunctionType.Scalar)]
        class NowDateFunc : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                return DateTime.Now.ToOADate();
            }
        }

        [SQLiteFunction(Name = "SearchTag", Arguments = -1, FuncType = FunctionType.Scalar)]
        class SearchTagFunc : SQLiteFunction
        {
            public override object Invoke(object[] args)
            {
                string cmp = (string)args[0];
                if (cmp == null || cmp == string.Empty)
                    return false;
                bool start = cmp.StartsWith("*");
                bool end = cmp.EndsWith("*");
                string cmp1 = cmp.Replace("*", "");
                string cmp2 = cmp.Replace("*", "|");

                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i].GetType() == typeof(string))
                    {
                        string str = (string)args[i];
                        if (str != null)
                        {
                            if (start && end)
                            {
                                if (str == cmp1 || str == cmp2)
                                    return true;
                            }
                            else if (!start && end)
                            {
                                if (str.EndsWith(cmp1) || str.EndsWith(cmp2))
                                    return true;
                            }
                            else if (start && !end)
                            {
                                if (str.StartsWith(cmp1) || str.StartsWith(cmp2))
                                    return true;
                            }
                            else if (!start && !end)
                            {
                                if (str.Contains(cmp1) || str.Contains(cmp2))
                                    return true;
                            }

                        }
                    }
                }
                return false;
            }
        }

        #endregion
    }
}