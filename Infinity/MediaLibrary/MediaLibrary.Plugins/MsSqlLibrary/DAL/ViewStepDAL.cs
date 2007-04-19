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
    class ViewStepDAL
    {
        #region SQL Commands
        private class Sql
        {
            public static readonly string GetStepsByViewID = "SELECT * FROM view_steps WHERE view_id = @ViewID ORDER BY view_step_id ASC";
            public static readonly string DeleteStep = "DELETE FROM view_steps WHERE view_id = @ViewID AND view_step_id = @ViewStepID";
            public static readonly string DeleteStepsByViewID = "DELETE FROM view_steps WHERE view_id = @ViewID";
            public static readonly string GetMaxStepID = "SELECT max(view_step_id) FROM view_steps";
            public static readonly string InsertStep = "INSERT INTO view_steps (view_step_id, view_id, group_tag, sort_asc) VALUES (null, @ViewID, @GroupTag, 'yes')";
            public static string UpdateStep(string CurrentTag)
            {
                return "UPDATE view_steps SET " + SqlHelper.FixSQL(CurrentTag) + " = @TagValue " +
                    "WHERE view_step_id = @ViewStepID AND view_id = @ViewID";
            }
        }

        #endregion

        #region DataReader Methods

        #region public static ArrayList GetStepsByViewID(IDbCommand Command, int ViewID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ViewID"></param>
        /// <returns></returns>
        public static ArrayList GetStepsByViewID(IDbCommand Command, int ViewID)
        {
            Command.CommandText = Sql.GetStepsByViewID;
            DbProvider.AddParameter(Command, "@ViewID", ViewID);
            return FillViewSteps(DbProvider.ExecuteDataReader(Command));
        }
        #endregion 
        
        #endregion

        #region Scalar Methods

        #region public static int GetMaxStepID(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetMaxStepID(IDbCommand Command)
        {
            Command.CommandText = Sql.GetMaxStepID;
            return DbProvider.ExecuteScalar(Command);
        }
        #endregion 

        #endregion

        #region NonQuery Methods

        #region public static bool InsertStep(IDbCommand Command, int ViewID, string GroupTag)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ViewID"></param>
        /// <param name="GroupTag"></param>
        /// <returns></returns>
        public static bool InsertStep(IDbCommand Command, int ViewID, string GroupTag)
        {
            try
            {
                Command.CommandText = Sql.InsertStep;
                DbProvider.AddParameter(Command, "@ViewID", ViewID);
                DbProvider.AddParameter(Command, "@GroupTag", GroupTag);
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return false;
        }
        #endregion

        #region public static bool UpdateStep(IDbCommand Command, string TagName, string TagValue, int ViewID, int ViewStepID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <param name="TagValue"></param>
        /// <param name="ViewID"></param>
        /// <param name="ViewStepID"></param>
        /// <returns></returns>
        public static bool UpdateStep(IDbCommand Command, string TagName, string TagValue, int ViewID, int ViewStepID)
        {
            try
            {
                Command.CommandText = Sql.UpdateStep(TagName);
                DbProvider.AddParameter(Command, "@TagValue", TagValue);
                DbProvider.AddParameter(Command, "@ViewID", ViewID);
                DbProvider.AddParameter(Command, "@ViewStepID", ViewID);
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return false;
        }
        #endregion

        #region public static bool DeleteStep(IDbCommand Command, int ViewID, int ViewStepID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ViewID"></param>
        /// <param name="ViewStepID"></param>
        /// <returns></returns>
        public static bool DeleteStep(IDbCommand Command, int ViewID, int ViewStepID)
        {
            try
            {
                Command.CommandText = Sql.DeleteStep;
                DbProvider.AddParameter(Command, "@ViewID", ViewID);
                DbProvider.AddParameter(Command, "@ViewStepID", ViewID);
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return false;
        }
        #endregion

        #region public static bool DeleteStepsByViewID(IDbCommand Command, int ViewID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ViewID"></param>
        /// <returns></returns>
        public static bool DeleteStepsByViewID(IDbCommand Command, int ViewID)
        {
            try
            {
                Command.CommandText = Sql.DeleteStepsByViewID;
                DbProvider.AddParameter(Command, "@ViewID", ViewID);
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return false;
        }
        #endregion

        #endregion

        #region Fill Methods

        #region private static ArrayList FillViewSteps(IDataReader Reader)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static ArrayList FillViewSteps(IDataReader Reader)
        {
            ArrayList ViewSteps = new ArrayList();
            try
            {
                while (Reader.Read())
                {
                    int idx = 0;    //use ordinal position for quicker access
                    MLViewStep ViewStep = new MLViewStep();
                    ViewStep.ViewStepID = Convert.ToInt32(Reader[idx++]);
                    ViewStep.ViewID = Convert.ToInt32(Reader[idx++]);
                    ViewStep.GroupTag = Convert.ToString(Reader[idx++]);
                    ViewStep.GroupFunction = Convert.ToString(Reader[idx++]);
                    ViewStep.Mode = Convert.ToString(Reader[idx++]);
                    ViewStep.SortTag = Convert.ToString(Reader[idx++]);
                    ViewStep.SortAscending = Convert.ToString(Reader[idx++]) != "no";
                    ViewStep.SortType = Convert.ToString(Reader[idx++]);
                    ViewSteps.Add(ViewStep);
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return ViewSteps;
        }
        #endregion 
        
        #endregion
    }
}
