using System;

namespace MediaPortal.Database
{
  /// <summary>
  /// Utility class for building dynamic SQL-SELECT Queries
  /// add every single table, wherecondition, groupBy, orderBy condition
  /// without bothering about keywords, etc
  /// To get the resulting SQL-SELECT, use the "AsSQL" property
  /// To get the resulting SQL-SELECT COUNT, use the "AsSQLCount
  /// </summary>
  public class SQLSelectBuilder
  {
    string fields = "";
    string tables = "";
    string where = "";
    string groupBy = "";
    string orderBy = "";
    string count = "count(*)";
    string distinct = "";


    public SQLSelectBuilder()
    {
      Clear();
    }


    public void Clear()
    {
      fields = "";
      tables = "";
      where = "";
      groupBy = "";
      orderBy = "";
      count = "count(*)";
      distinct = "";
    }

    public void AddField(string fieldName)
    {
      if (fields == "")
      {
        fields = fieldName;
      }
      else
      {
        fields = fields + ", " + fieldName;
      }
    }

    public void AddTable(string tableName)
    {
      if (tables == "")
      {
        tables = tableName;
      }
      else
      {
        // avoid having the same table twice....
        if (tables.IndexOf(tableName) == -1)
        {
          tables = tables + ", " + tableName;
        }
      }
    }

    public void AddWhereCond(string whereCond)
    {
      if (where == "")
      {
        where = whereCond;
      }
      else
      {
        // avoid having the same where condition twice....
        if (where.IndexOf(whereCond) == -1)
        {
          where = where + " and " + whereCond; //yes, no OR conditions allowed :)
        }
      }
    }

    public void AddGroupField(string groupField)
    {
      if (groupBy == "")
      {
        groupBy = groupField;
      }
      else
      {
        groupBy = groupBy + ", " + groupField;
      }
    }

    public void AddOrderField(string orderField)
    {
      if (orderBy == "")
      {
        orderBy = orderField;
      }
      else
      {
        orderBy = orderBy + ", " + orderField;
      }
    }

    public string Fields
    {
      get { return fields; }
      set { fields = value; }
    }

    public string FieldsForSQL
    {
      get
      {
        if (fields == "")
        {
          return "*";
        }
        else
        {
          return fields;
        }
      }
    }

    public string Tables
    {
      get { return tables; }
      set { tables = value; }
    }

    public string Where
    {
      get { return where; }
      set { where = value; }
    }

    public string GroupBy
    {
      get { return groupBy; }
      set { groupBy = value; }
    }

    public string OrderBy
    {
      get { return orderBy; }
      set { orderBy = value; }
    }

    public bool Distinct
    {
      get { return (distinct == "DISTINCT"); }
      set
      {
        if (value)
        {
          distinct = "DISTINCT";
        }
        else
        {
          distinct = "";
        }
      }
    }


    public string Count
    {
      get { return count; }
      set { count = value; }
    }

    public string AsSQL
    {
      get { return GetAsSQL(); }
    }

    public string AsSQLCount
    {
      get { return GetAsSQLCount(); }
    }

    string GetAsSQL()
    {
      string res = "";
      res = String.Format("SELECT {0} {1} FROM {2} ", distinct, FieldsForSQL, tables);
      if (where != "")
      {
        res = res + " WHERE " + where;
      }
      if (groupBy != "")
      {
        res = res + " GROUP BY " + groupBy;
      }
      if (orderBy != "")
      {
        res = res + " ORDER BY " + orderBy;
      }
      return res;
    }

    string GetAsSQLCount()
    {
      string res = "";
      res = String.Format("SELECT {0} FROM {1} ", count, tables);
      if (where != "")
      {
        res = res + " WHERE " + where;
      }
      if (groupBy != "")
      {
        res = res + " GROUP BY " + groupBy;
      }
      if (orderBy != "")
      {
        res = res + " ORDER BY " + orderBy;
      }
      return res;
    }
  }
}