using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EntityClient;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel
{
  public static class EntitySqlDebug
  {
    private static readonly Assembly EfAssembly = typeof(EntityCommand).Assembly;

    public static string ToTraceString(this IQueryable query)
    {
      MethodInfo method = query.GetType().GetMethod("ToTraceString");
      if (method != null)
      {
        return method.Invoke(query, null).ToString();
      }
      return "";
    }

    public static string ToTraceString(this ObjectContext context)
    {
      const string intern = "System.Data.Mapping.Update.Internal";

      Type dynUpdate = EfAssembly.GetType(intern + ".DynamicUpdateCommand");
      Type updTranslate = EfAssembly.GetType(intern + ".UpdateTranslator");
      Type funcUpdate = EfAssembly.GetType(intern + ".FunctionUpdateCommand");

      var conn = context.Connection as EntityConnection;
      var parameter = new object[]
                {
                context.ObjectStateManager,
                conn.GetMetadataWorkspace(),
                conn, context.CommandTimeout
                };

      BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
      object updTranslator = Activator.CreateInstance(updTranslate, flags, null, parameter, null);
      MethodInfo prodCommands = updTranslate.GetMethod("ProduceCommands", flags);
      object updateCommands = prodCommands.Invoke(updTranslator, null);
      List<DbCommand> dbCommands = new List<DbCommand>();

      foreach (object obj in (IEnumerable)updateCommands)
      {
        if (funcUpdate.IsInstanceOfType(obj))
        {
          FieldInfo dbCommand = funcUpdate.
          GetField("m_dbCommand", flags);
          dbCommands.Add((DbCommand)dbCommand.GetValue(obj));
        }
        else if (dynUpdate.IsInstanceOfType(obj))
        {
          MethodInfo createCommand = dynUpdate.
          GetMethod("CreateCommand", flags);
          object[] mParams = new object[] { updTranslator, new Dictionary<int, object>() };
          dbCommands.Add((DbCommand)createCommand.Invoke(obj, mParams));
        }
        else
        {
          throw new NotImplementedException("Unknown update command type");
        }
      }

      var traceString = new StringBuilder();
      foreach (DbCommand command in dbCommands)
      {
        traceString.AppendLine("--=============== BEGIN COMMAND ===============");
        traceString.AppendLine();

        foreach (DbParameter param in command.Parameters)
        {
          traceString.AppendFormat("declare {0} {1} set {0} = '{2}'", param.ParameterName, GetSqlDbType(param), param.Value);
          traceString.AppendLine();
        }
        traceString.AppendLine();
        traceString.AppendLine(command.CommandText);

        traceString.AppendLine();
        traceString.AppendLine("go");
        traceString.AppendLine();
        traceString.AppendLine("--=============== END COMMAND ===============");
      }

      return traceString.ToString();
    }

    private static string GetSqlDbType(IDataParameter param)
    {
      string result;
      var parm = new SqlParameter();
      try
      {
        parm.DbType = param.DbType;
        result = parm.SqlDbType.ToString();
      }
      catch (Exception)
      {
        result = param.DbType.ToString();
      }
      return result;
    }
  }
}
