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
    class TagDAL
    {
        #region Sql Commands

        private class Sql
        {
            public static readonly string AddNewTag = "UPDATE tags SET tag_name = @TagName, tag_free = 0 WHERE tag_id = @TagID";
            public static readonly string InsertTag = "INSERT INTO tags VALUES (null, null, @TagName, 1)";
            public static readonly string FreeTag = "UPDATE tags SET tag_name = null, tag_free = 1 WHERE tag_name = @TagName";
            public static readonly string RenameTag = "UPDATE tags SET tag_name = @NewTagName WHERE tag_name = '@OldTagName'";
            public static readonly string GetTags = "SELECT tag_name, tag_col FROM tags WHERE tag_free == 0";
            public static readonly string GetMaxTagID = "SELECT max(tag_id) FROM tags";
            public static readonly string GetFreeTagID = "SELECT tag_id FROM tags WHERE tag_free = 1 LIMIT 1";
        }
        
        #endregion

        #region Scalar Method

        #region public static int GetFreeTagID(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetFreeTagID(IDbCommand Command)
        {
            Command.CommandText = Sql.GetFreeTagID;
            return DbProvider.ExecuteScalar(Command);
        }
        #endregion

        #region public static int GetMaxTagID(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetMaxTagID(IDbCommand Command)
        {
            Command.CommandText = Sql.GetMaxTagID;
            return DbProvider.ExecuteScalar(Command);
        }
        #endregion
        
        #endregion

        #region NonQuery Method

        #region public static bool AddNewTag(IDbCommand Command, int TagID, string TagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagID"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static bool AddNewTag(IDbCommand Command, int TagID, string TagName)
        {
            Command.CommandText = Sql.AddNewTag;
            DbProvider.AddParameter(Command, "@TagID", TagID);
            DbProvider.AddParameter(Command, "@TagName", TagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool InsertTag(IDbCommand Command, string TagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static bool InsertTag(IDbCommand Command, string TagName)
        {
            Command.CommandText = Sql.InsertTag;
            DbProvider.AddParameter(Command, "@TagName", TagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool RenameTag(IDbCommand Command, string OldTagName, string NewTagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="OldTagName"></param>
        /// <param name="NewTagName"></param>
        /// <returns></returns>
        public static bool RenameTag(IDbCommand Command, string OldTagName, string NewTagName)
        {

            Command.CommandText = Sql.RenameTag;
            DbProvider.AddParameter(Command, "@OldTagName", OldTagName);
            DbProvider.AddParameter(Command, "@NewTagName", NewTagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool FreeTag(IDbCommand Command, string TagName)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <returns></returns>
        public static bool FreeTag(IDbCommand Command, string TagName)
        {
            Command.CommandText = Sql.FreeTag;
            DbProvider.AddParameter(Command, "@TagName", TagName);
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion
        
        #endregion

        #region DataReader Methods

        #region public static IMeedioItem GetTags(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static IMeedioItem GetTags(IDbCommand Command)
        {
            Command.CommandText = Sql.GetTags;
            return FillTags(DbProvider.ExecuteDataReader(Command));
        }
        #endregion

        #endregion

        #region Fill Methods

        #region private static IMeedioItem FillTags(IDataReader Reader)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static IMeedioItem FillTags(IDataReader Reader)
        {
            IMeedioItem Tags = new MeedioItem();
            try
            {
                while (Reader.Read())
                {   //Tags[tag_name] = tag_col
                    Tags[Reader.GetString(0)] = Reader.GetString(1);
                }
                return Tags;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }
            return null;
        }
        #endregion
        
        #endregion
    }
}
