#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

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
    private string fields = "";
    private string tables = "";
    private string where = "";
    private string groupBy = "";
    private string orderBy = "";
    private string count = "count(*)";
    private string distinct = "";


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

    private string GetAsSQL()
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

    private string GetAsSQLCount()
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