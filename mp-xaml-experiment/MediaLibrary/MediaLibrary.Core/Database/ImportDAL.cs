using System;
using MediaLibrary;
using MediaLibrary.Database;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Collections;

namespace MediaLibrary.Database
{
    class ImportDAL
    {
        #region Sql Commands

        private class Sql
        {
            public static readonly string GetImports = "SELECT import_id, name, section, update_mode, plugin_id, schedule_freq, schedule_interval, schedule_time, last_run_dtm, wake_up, run_missed FROM imports";
            public static readonly string GetImportPropreties = "SELECT prop_name, prop_value FROM import_props WHERE import_id = @ImportID";
            public static readonly string DeleteImport = "DELETE FROM imports WHERE import_id = @ImportID";
            public static readonly string UpdateImport = "UPDATE imports SET name = @Name, section = @Section, update_mode = @Mode, "
                + "plugin_id = @PluginID, schedule_freq = @ScheduleFrequency, schedule_interval = @ScheduleInterval, "
                + "schedule_time = @ScheduleTime, last_run_dtm = @LastRun, wake_up = @WakeUp, run_missed = @RunMissed "
                + "WHERE import_id = @ImportID";
            public static readonly string InsertImport = "INSERT INTO imports (name, section, update_mode, plugin_id, schedule_freq, "
                + "schedule_interval, schedule_time, last_run_dtm, wake_up, run_missed) VALUES (@Name, @Section, @Mode, @PluginID, "
                + "@ScheduleFrequency, @ScheduleInterval, @ScheduleTime, @LastRun, @WakeUp, @RunMissed)";
            public static readonly string DeleteImportProperties = "DELETE FROM import_props WHERE import_id = @ImportID";
            public static readonly string InsertImportProperties = "INSERT INTO import_props VALUES (@ImportID, @PropertyName, @PropertyValue)";
            public static readonly string GetMaxImportID = "SELECT max(import_id) FROM imports";
        } 

        #endregion

        #region Scalar Methods

        #region public static int GetMaxImportID(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetMaxImportID(IDbCommand Command)
        {
            Command.CommandText = Sql.GetMaxImportID;
            return DbProvider.ExecuteScalar(Command);
        }
        #endregion
        
        #endregion

        #region NonQuery Methods

        #region public static bool InsertImport(IDbCommand Command, IMLImport Import)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Import"></param>
        /// <returns></returns>
        public static bool InsertImport(IDbCommand Command, IMLImport Import)
        {
            Command.CommandText = Sql.InsertImport;
            DbProvider.AddParameter(Command, "@Name", Import.Name);
            DbProvider.AddParameter(Command, "@Section", Import.SectionName);
            DbProvider.AddParameter(Command, "@Mode", Import.Mode);
            DbProvider.AddParameter(Command, "@PluginID", Import.PluginID);
            DbProvider.AddParameter(Command, "@ScheduleFrequency", Import.ScheduleFrequency);
            DbProvider.AddParameter(Command, "@ScheduleInterval", Import.ScheduleInterval);
            DbProvider.AddParameter(Command, "@ScheduleTime", Import.ScheduleTime.ToString("HHmm"));
            DbProvider.AddParameter(Command, "@LastRun", Import.LastRun.ToOADate());
            DbProvider.AddParameter(Command, "@WakeUp", Convert.ToInt32(Import.WakeUp));
            DbProvider.AddParameter(Command, "@RunMissed", Convert.ToInt32(Import.RunMissed));
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool UpdateImport(IDbCommand Command, IMLImport Import)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Import"></param>
        /// <returns></returns>
        public static bool UpdateImport(IDbCommand Command, IMLImport Import)
        {
            Command.CommandText = Sql.UpdateImport;
            DbProvider.AddParameter(Command, "@ImportID", Import.ID);
            DbProvider.AddParameter(Command, "@Name", Import.Name);
            DbProvider.AddParameter(Command, "@Section", Import.SectionName);
            DbProvider.AddParameter(Command, "@Mode", Import.Mode);
            DbProvider.AddParameter(Command, "@PluginID", Import.PluginID);
            DbProvider.AddParameter(Command, "@ScheduleFrequency", Import.ScheduleFrequency);
            DbProvider.AddParameter(Command, "@ScheduleInterval", Import.ScheduleInterval);
            DbProvider.AddParameter(Command, "@ScheduleTime", Import.ScheduleTime.ToString("HHmm"));
            DbProvider.AddParameter(Command, "@LastRun", Import.LastRun.ToOADate());
            DbProvider.AddParameter(Command, "@WakeUp", Convert.ToInt32(Import.WakeUp));
            DbProvider.AddParameter(Command, "@RunMissed", Convert.ToInt32(Import.RunMissed));
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool DeleteImport(IDbCommand Command, IMLImport Import)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Import"></param>
        /// <returns></returns>
        public static bool DeleteImport(IDbCommand Command, IMLImport Import)
        {
            Command.CommandText = Sql.DeleteImport;
            DbProvider.AddParameter(Command, "@ImportID", Import.ID);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool DeleteImportProperties(IDbCommand Command, IMLImport Import)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Import"></param>
        /// <returns></returns>
        public static bool DeleteImportProperties(IDbCommand Command, IMLImport Import)
        {
            Command.CommandText = Sql.DeleteImportProperties;
            DbProvider.AddParameter(Command, "@ImportID", Import.ID);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool InsertImportProperties(IDbCommand Command, IMLImport Import)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Import"></param>
        /// <returns></returns>
        public static bool InsertImportProperties(IDbCommand Command, IMLImport Import)
        {
            Command.CommandText = Sql.InsertImportProperties;
            IDbDataParameter IdParam = DbProvider.AddParameter(Command, "@ImportID", Import.ID);
            IDbDataParameter NameParam = DbProvider.AddParameter(Command, "@PropertyName", "");
            IDbDataParameter ValueParam = DbProvider.AddParameter(Command, "@PropertyValue", "");
            ArrayList keys = Import.PluginProperties.Keys as ArrayList;
            for (int i = 0; i < Import.PluginProperties.Count; i++)
            {
                NameParam.Value = Convert.ToString(keys[i]);
                ValueParam.Value = Convert.ToString(Import.PluginProperties[i]);
                DbProvider.ExecuteNonQuery(Command,false);
            }
            return true;
        }
        #endregion

        #endregion

        #region DataReader Methods

        #region public static IMLImports GetImports(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static IMLImports GetImports(IDbCommand Command)
        {
            Command.CommandText = Sql.GetImports;
            return FillImports(DbProvider.ExecuteDataReader(Command));
        }
                #endregion

        #region public static IMLHashItem GetImportProperties(IDbCommand Command, int ImportID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ImportID"></param>
        /// <returns></returns>
        public static IMLHashItem GetImportProperties(IDbCommand Command, int ImportID)
        {
            Command.CommandText = Sql.GetImportPropreties;
            DbProvider.AddParameter(Command, "@ImportID", ImportID);
            return FillImportProperties(DbProvider.ExecuteDataReader(Command));
        }
        #endregion

        #endregion

        #region Fill Methods
        
        #region private static IMLImports FillImports(IDataReader Reader)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static IMLImports FillImports(IDataReader Reader)
        {
            MLImports Imports = new MLImports();
            try
            {
                while (Reader.Read())
                {
                    MLImport Item = new MLImport();
                    Item.ID = Reader.GetInt32(0);
                    Item.Name = Reader.GetString(1);
                    Item.SectionName = Reader.GetString(2);
                    Item.Mode = Reader.GetString(3);
                    Item.PluginID = Reader.GetString(4);
                    Item.ScheduleFrequency = Reader.GetString(5);
                    Item.ScheduleInterval = Reader.GetInt32(6);
                    Item.ScheduleTime = DateTime.ParseExact(Reader.GetString(7), "HHmm", null);
                    Item.LastRun = DateTime.FromOADate(Convert.ToDouble(Reader.GetValue(8)));
                    Item.WakeUp = Convert.ToBoolean(Reader.GetInt32(9));
                    Item.RunMissed = Convert.ToBoolean(Reader.GetInt32(10));

                    Imports._Imports.Add(Item);
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return Imports as IMLImports;
        }
        #endregion

        #region private static IMLHashItem FillImportProperties(IDataReader Reader)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static IMLHashItem FillImportProperties(IDataReader Reader)
        {
            IMLHashItem Properties = new MLHashItem() as IMLHashItem;
            try
            {
                while (Reader.Read())
                {
                    Properties[Reader.GetString(0)] = Reader.GetString(1);
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return Properties;
        }
        
        #endregion

        #endregion
    }
}
