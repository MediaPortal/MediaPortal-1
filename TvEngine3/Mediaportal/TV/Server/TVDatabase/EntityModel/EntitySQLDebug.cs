using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Mediaportal.TV.Server.TVDatabase.Entities;
using Mediaportal.TV.Server.TVDatabase.Entities.Enums;
using Mediaportal.TV.Server.TVDatabase.EntityModel.Interfaces;
using System.Reflection;
using System.Data.EntityClient;
using System.Data.Common;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel
{
  public static class EntitySqlDebug
  {
    static Assembly efAssembly = typeof(EntityCommand).Assembly;

    public static string ToTraceString(this IQueryable query)
    {
      MethodInfo method = query.GetType().
      GetMethod("ToTraceString");

      if (method != null)
        return method.Invoke(query, null).ToString();
      else return "";
    }

    public static string ToTraceString(this ObjectContext context)
    {
      string intern = "System.Data.Mapping.Update.Internal";

      Type dynUpdate = efAssembly.GetType(intern + ".DynamicUpdateCommand");
      Type updTranslate = efAssembly.GetType(intern + ".UpdateTranslator");
      Type funcUpdate = efAssembly.GetType(intern + ".FunctionUpdateCommand");

      EntityConnection conn = context.Connection as EntityConnection;
      object[] parameter = new object[]
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
          object[] mParams = new object[] { updTranslator, new Dictionary<long, object>() };
          dbCommands.Add((DbCommand)createCommand.Invoke(obj, mParams));
        }
        else
          throw new NotImplementedException("Unknown update command type");
      }

      StringBuilder ts = new StringBuilder();
      foreach (DbCommand cmd in dbCommands)
      {
        ts.AppendLine("\r\n****** Command Begin ******");
        ts.AppendLine(cmd.CommandText);
        ts.AppendLine("*** Parameterwerte ***:");
        foreach (DbParameter p in cmd.Parameters)
          ts.AppendFormat("{0} = {1}\r\n", p.ParameterName, p.Value);
        ts.AppendLine("****** Command End *********\r\n");
      }
      
      return ts.ToString();
    }
  }
}
