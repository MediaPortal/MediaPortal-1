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
    class ViewDAL
    {
        #region SQL Commands

        private class Sql
        {
            public static string UpdateView(string CurrentTag)
            {
                return "UPDATE views SET " + CurrentTag + " = @TagValue WHERE view_id = @ViewID";
            }
            public static readonly string DeleteView = "DELETE FROM views WHERE view_id = @ViewID";
            public static readonly string DeleteAllViews = "DELETE FROM views";
            public static readonly string GetAllViews = "SELECT * FROM views ORDER BY view_order ASC";
            public static readonly string GetMaxViewID = "SELECT max(view_id) FROM views";
            public static readonly string GetMaxViewOrder = "SELECT max(view_order) FROM views";
            public static readonly string InsertView = "INSERT INTO views (view_id, view_name, view_order) VALUES (null,@Name,@Order)";
        }

        #endregion

        #region DataReader Methods

        #region public static IMLView[] GetAllViews(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static IMLView[] GetAllViews(IDbCommand Command)
        {
            Command.CommandText = Sql.GetAllViews;
            return FillViews(DbProvider.ExecuteDataReader(Command));
        }
        #endregion

        #endregion

        #region Scalar Methods

        #region public static int GetMaxViewID(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetMaxViewID(IDbCommand Command)
        {
            try
            {
                Command.CommandText = Sql.GetMaxViewID;
                object obj = Command.ExecuteScalar();
                if (Convert.IsDBNull(obj))
                    return 0;
                else
                    return (Convert.ToInt32(obj));
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return -1;
        }
        #endregion

        #region public static int GetMaxViewOrder(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static int GetMaxViewOrder(IDbCommand Command)
        {
            try
            {
                Command.CommandText = Sql.GetMaxViewOrder;
                object obj = Command.ExecuteScalar();
                if (Convert.IsDBNull(obj))
                    return 1;
                else
                    return (Convert.ToInt32(obj) + 1);
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return -1;
        }
        #endregion

        #endregion

        #region NonQuery Methods

        #region public static bool InsertView(IDbCommand Command, string Name, int Order)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="Name"></param>
        /// <param name="Order"></param>
        /// <returns></returns>
        public static bool InsertView(IDbCommand Command, string Name, int Order)
        {
            try
            {
                Command.CommandText = Sql.InsertView;
                DbProvider.AddParameter(Command, "@Name", Name);
                DbProvider.AddParameter(Command, "@Order", Order);
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return false;
        }
        #endregion

        #region public static bool UpdateView(IDbCommand Command, string TagName, string TagValue, int ViewID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="TagName"></param>
        /// <param name="TagValue"></param>
        /// <param name="ViewID"></param>
        /// <returns></returns>
        public static bool UpdateView(IDbCommand Command, string TagName, string TagValue, int ViewID)
        {
            Command.CommandText = Sql.UpdateView(TagName);
            DbProvider.AddParameter(Command, "@TagValue", TagValue);
            DbProvider.AddParameter(Command, "@ViewID", ViewID.ToString());
            return DbProvider.ExecuteNonQuery(Command);
        }
        #endregion

        #region public static bool DeleteView(IDbCommand Command, int ViewID)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="ViewID"></param>
        /// <returns></returns>
        public static bool DeleteView(IDbCommand Command, int ViewID)
        {
            try
            {
                Command.CommandText = Sql.DeleteView;
                DbProvider.AddParameter(Command, "@ViewID", ViewID);
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null) Command.Dispose(); }
            return false;
        }
        #endregion

        #region public static bool DeleteAllViews(IDbCommand Command)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <returns></returns>
        public static bool DeleteAllViews(IDbCommand Command)
        {
            try
            {
                Command.CommandText = Sql.DeleteAllViews;
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

        #region private static IMLView[] FillViews(IDataReader Reader)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Reader"></param>
        /// <returns></returns>
        private static IMLView[] FillViews(IDataReader Reader)
        {
            ArrayList Views = new ArrayList();
            try
            {
                while (Reader.Read())
                {
                    int idx = 0;    //use ordinal position for quicker access
                    MLView View = new MLView();
                    View.ID = Convert.ToInt32(Reader[idx++]);
                    View.Name = Convert.ToString(Reader[idx++]);
                    View.Order = Convert.ToInt32(Reader[idx++]);
                    View.Filter = Convert.ToString(Reader[idx++]);
                    View.Custom1 = Convert.ToString(Reader[idx++]);
                    View.Custom2 = Convert.ToString(Reader[idx++]);
                    Views.Add(View);
                }
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Reader != null) Reader.Close(); }

            return (IMLView[])Views.ToArray(typeof(IMLView));
        }

        #endregion

        #endregion
    }
}
