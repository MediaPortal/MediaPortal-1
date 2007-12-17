using System;
using System.Data;
using System.Data.Common;
using System.Collections;

namespace MediaLibrary.Database
{
    /// <summary>
    /// The DbProvider class lists all the abstract methods that each data 
    /// access layer provider (SQLite, SQL Server, OleDb, etc.) must implement.
    /// </summary>
    public abstract class DbProvider
    {
        // Private Members

        protected string _ConnectionString;
        private IDbConnection _Connection;
        private IDbTransaction _Transaction;
        protected bool _InProgress;
        protected bool _UseInMemDb;
       

        #region Properties

        public bool InProgress
        {
            get { return _InProgress; }
        }
        public bool UseInMemDb
        {
            get { return _UseInMemDb; }
        }
        public string ConnectionString
        {
            get
            {
                if (_ConnectionString == string.Empty || _ConnectionString.Length == 0)
                    throw new ArgumentException("Invalid database connection string.");
                return _ConnectionString;
            }
        } 

        #endregion

        #region Constructors

        protected DbProvider() { }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Data provider specific implementation for accessing relational databases.
        /// </summary>
        public abstract IDbConnection NewConnection();
        /// <summary>
        /// Data provider specific implementation for executing SQL statement while connected to a data source.
        /// </summary>
        public abstract IDbCommand NewCommand();
        /// <summary>
        /// Data provider specific implementation for updating the DataSet.
        /// </summary>
        public abstract IDbDataAdapter NewDataAdapter();
        /// <summary>
        /// Data provider specific implementation for updating the DataSet.
        /// </summary>
        public abstract DbCommandBuilder NewCommandBuilder();
        /// <summary>
        /// Method to return a string of all sections in the database
        /// </summary>
        internal abstract ArrayList GetSections(IMLHashItem Properties);
        /// <summary>
        /// Deletes the currently opened database
        /// </summary>
        /// <returns></returns>
        internal abstract bool DeleteSection(string Name, IMLHashItem Properties);

        protected abstract void SetMemTable(IDbCommand Command);
        #endregion

        public IDbCommand CreateCommand()
        {
            return CreateCommand(false, "");
        }

        public IDbCommand CreateCommand(bool ReadOnly)
        {
            return CreateCommand(ReadOnly, "");
        }

        public IDbCommand CreateCommand(string Sql)
        {
            return CreateCommand(false, Sql);
        }

        public IDbCommand CreateCommand(bool ReadOnly, string Sql)
        {
            IDbCommand cmd = null;
            if (ReadOnly && UseInMemDb)
            {
                //cmd = ROConnection.CreateCommand();
            }
            else if (InProgress || (_Connection != null && _Connection.State == ConnectionState.Open))
            {
                if (ReadOnly)
                    SetMemTable(_Connection.CreateCommand());
                cmd = _Connection.CreateCommand();
            }
            else
            {
                _Connection = this.NewConnection();
                _Connection.ConnectionString = ConnectionString;
                _Connection.Open();
                if (ReadOnly)
                    SetMemTable(_Connection.CreateCommand());
                cmd = _Connection.CreateCommand();
            }
            cmd.CommandText = Sql;
            return cmd;
        }

        public void CloseConnection()
        {
            if (!InProgress && _Connection != null && _Connection.State != ConnectionState.Closed)
            {
                _Connection.Close();
            }
        }

        public void CloseConnection(IDbCommand Command)
        {
            if (Command != null)
                Command.Dispose();
            CloseConnection();
        }

        #region Database Transaction

        public bool BeginUpdate()
        {
            return BeginUpdate(false);
        }
        public bool BeginUpdate(bool Stackable)
        {
            if (_InProgress && !Stackable)
                return false;
            _Transaction = null;
            try
            {
                if (_Connection.State == ConnectionState.Closed)
                    _Connection.Open();
                _Transaction = _Connection.BeginTransaction();
                _InProgress = true;
            }
            catch (Exception e)
            {
                Console.Write("Exception: " + e.Message);
                _InProgress = false;
                return false;
            }
            return true;
        }

        public void CancelUpdate()
        {
            try
            {
                _InProgress = false;
                _Transaction.Rollback();
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally
            {
                if (_Transaction != null)
                    _Transaction.Dispose();
                CloseConnection();
            }
        }

        public void EndUpdate()
        {
            try
            {
                _InProgress = false;
                _Transaction.Commit();
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally
            {
                if (_Transaction != null)
                    _Transaction.Dispose();
                CloseConnection();
            }
        }

        #endregion

        #region Static Helper Functions

        public static IDbDataParameter CreateParameter(IDbCommand Command, string Key, string Value)
        {
            IDbDataParameter Parameter = Command.CreateParameter();
            Parameter.ParameterName = Key;
            Parameter.Value = Value;
            return Parameter;
        }
        public static IDbDataParameter CreateParameter(IDbCommand Command, string Key, int Value)
        {
            IDbDataParameter Parameter = Command.CreateParameter();
            Parameter.ParameterName = Key;
            Parameter.Value = Value;
            return Parameter;
        }
        public static IDbDataParameter CreateParameter(IDbCommand Command, string Key, double Value)
        {
            IDbDataParameter Parameter = Command.CreateParameter();
            Parameter.ParameterName = Key;
            Parameter.Value = Value;
            return Parameter;
        }

        public static IDbDataParameter AddParameter(IDbCommand Command, string Key, string Value)
        {
            IDbDataParameter Parameter = CreateParameter(Command, Key, Value);
            Command.Parameters.Add(Parameter);
            return Parameter;
        }
        public static IDbDataParameter AddParameter(IDbCommand Command, string Key, int Value)
        {
            IDbDataParameter Parameter = CreateParameter(Command, Key, Value);
            Command.Parameters.Add(Parameter);
            return Parameter;
        }
        public static IDbDataParameter AddParameter(IDbCommand Command, string Key, double Value)
        {
            IDbDataParameter Parameter = CreateParameter(Command, Key, Value);
            Command.Parameters.Add(Parameter);
            return Parameter;
        }

        public static int ExecuteScalar(IDbCommand Command)
        {
            try
            {
                object obj = Command.ExecuteScalar();
                if (Convert.IsDBNull(obj) || obj == null)
                    return 0;
                else
                    return Convert.ToInt32(obj);
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null)    Command.Dispose(); }
            return -1;
        }

        public static bool ExecuteNonQuery(IDbCommand Command)
        {
            return ExecuteNonQuery(Command, true);
        }

        public static bool ExecuteNonQuery(IDbCommand Command, bool DisposeCommand)
        {
            try
            {
                Command.ExecuteNonQuery();
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            finally { if (Command != null && DisposeCommand)    Command.Dispose(); }
            return false;
        }

        public static IDataReader ExecuteDataReader(IDbCommand Command)
        {
            IDataReader Reader = null;
            try
            {
                Reader = Command.ExecuteReader() as IDataReader;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            return Reader;
        }

        public static bool UpdateDataAdapter(IDataAdapter Adapter, DataSet dataSet)
        {
            try
            {
                Adapter.Update(dataSet);
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            return false;
        }

        public static bool FillDataAdapter(IDataAdapter Adapter, DataSet dataSet)
        {
            try
            {
                Adapter.Fill(dataSet);
                return true;
            }
            catch (Exception e) { Console.Write("Exception: " + e.Message); }
            return false;
        }

        #endregion

    }
}
