/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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

using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Soap;
using MediaPortal.GUI.Library;
using MediaPortal.GUI.View;
using MediaPortal.Database;
using Programs.Utils;
using ProgramsDatabase;
using SQLite.NET;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for ProgramViewHandler.
  /// </summary>
  public class ProgramViewHandler
  {
    ViewDefinition currentView = null;
    int currentLevel = 0;
    ArrayList views = new ArrayList();

    public ProgramViewHandler()
    {
      if (!System.IO.File.Exists("programViews3.xml"))
      {
        FilterDefinition filter1 = null;
        FilterDefinition filter2 = null;
        FilterDefinition filter3 = null;
        FilterDefinition filter4 = null;
        FilterDefinition filter5 = null;
        FilterDefinition filter6 = null;

        //manufacturer
        ViewDefinition viewManufacturer = new ViewDefinition();
        viewManufacturer.Name="Manufacturer";
        filter1 = new FilterDefinition();filter1.Where="manufacturer";;filter1.SortAscending=true;
        filter2 = new FilterDefinition();filter2.Where="filename";;filter2.SortAscending=true;
        viewManufacturer.Filters.Add(filter1);
        viewManufacturer.Filters.Add(filter2);

        //genre
        ViewDefinition viewGenre = new ViewDefinition();
        viewGenre.Name="Genre";
        filter1 = new FilterDefinition();filter1.Where="genre";;filter1.SortAscending=true;
        filter2 = new FilterDefinition();filter2.Where="genre2";;filter2.SortAscending=true;
        filter3 = new FilterDefinition();filter3.Where="genre3";;filter3.SortAscending=true;
        filter4 = new FilterDefinition();filter4.Where="genre4";;filter4.SortAscending=true;
        filter5 = new FilterDefinition();filter5.Where="genre5";;filter5.SortAscending=true;
        filter6 = new FilterDefinition();filter6.Where="filename";;filter6.SortAscending=true;
        viewGenre.Filters.Add(filter1);
        viewGenre.Filters.Add(filter2);
        viewGenre.Filters.Add(filter3);
        viewGenre.Filters.Add(filter4);
        viewGenre.Filters.Add(filter5);
        viewGenre.Filters.Add(filter6);

        //rating
        ViewDefinition viewRating = new ViewDefinition();
        viewRating.Name="Rating";
        filter1 = new FilterDefinition();filter1.Where="rating";;filter1.SortAscending=false;
        filter2 = new FilterDefinition();filter2.Where="filename";;filter2.SortAscending=true;
        viewRating.Filters.Add(filter1);
        viewRating.Filters.Add(filter2);

        //year
        ViewDefinition viewYear = new ViewDefinition();
        viewYear.Name="Year";
        filter1 = new FilterDefinition();filter1.Where="year";;filter1.SortAscending=true;
        filter2 = new FilterDefinition();filter2.Where="filename";;filter2.SortAscending=true;
        viewYear.Filters.Add(filter1);
        viewYear.Filters.Add(filter2);

        //most launched
        ViewDefinition viewMostLaunched = new ViewDefinition();
        viewMostLaunched.Name="Most Launched";
        filter1 = new FilterDefinition();filter1.Where="launchcount";filter1.SqlOperator=">";filter1.Restriction="0";filter1.SortAscending=false;
        viewMostLaunched.Filters.Add(filter1);

        //most recently launched
        ViewDefinition viewMostRecentlyLaunched = new ViewDefinition();
        viewMostRecentlyLaunched.Name="Most Recently Launched";
        filter1 = new FilterDefinition();filter1.Where="lastTimeLaunched";;filter1.SortAscending=false;
        viewMostRecentlyLaunched.Filters.Add(filter1);


        ArrayList listViews = new ArrayList();
        listViews.Add(viewManufacturer);
        listViews.Add(viewGenre);
        listViews.Add(viewRating);
        listViews.Add(viewYear);
        listViews.Add(viewMostLaunched);
        listViews.Add(viewMostRecentlyLaunched);

        using(FileStream fileStream = new FileStream("programViews2.xml", FileMode.Create, FileAccess.Write, FileShare.Read))
        {
          SoapFormatter formatter = new SoapFormatter();
          formatter.Serialize(fileStream, listViews);
          fileStream.Close();
        }
        
      }
      if (System.IO.File.Exists("programViews2.xml"))
      {
        using (FileStream fileStream = new FileStream("programViews2.xml", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        {
          try
          {
            SoapFormatter formatter = new SoapFormatter();
            views = (ArrayList) formatter.Deserialize(fileStream);
            fileStream.Close();
          }
          catch
          {
          }
        }
      }
    }

    public ViewDefinition View
    {
      get { return currentView; }
      set { currentView = value; }
    }


    public ArrayList Views
    {
      get { return views; }
      set { views = value; }
    }

    public string CurrentView
    {
      get
      {
        if (currentView == null)
          return String.Empty;
        return currentView.Name;
      }
      set
      {
        View = null;
        foreach (ViewDefinition definition in views)
        {
          if (definition.Name == value)
          {
            View = definition;
            CurrentLevel = 0;
          }
        }
      }
    }

    public int CurrentLevel
    {
      get { return currentLevel; }
      set
      {
        if (currentView == null)
        {
          currentLevel = 0;
          return;
        }
        if (value < 0 || value >= currentView.Filters.Count) return;
        currentLevel = value;
      }
    }

    public int MaxLevels
    {
      get
      {
        if (currentView == null)
        {
          return 0;
        }
        else
        {
          return currentView.Filters.Count;
        }
      }
    }

    public bool IsFilterQuery
    {
      get
      {
        if (currentView == null)
        {
          return false;
        }
        else
        {
          return (currentLevel < MaxLevels - 1);
        }
      }
    }

    public bool AddFilterItem(ProgramFilterItem filterItem)
    {
      bool res = false;
      FilterDefinition definition = (FilterDefinition) currentView.Filters[CurrentLevel];
      definition.SelectedValue = GetFieldValue(filterItem, definition.Where);
      if (currentLevel + 1 < currentView.Filters.Count)
      {
        currentLevel++;
        res = true;
      }
      return res;
    }

    public bool RemoveFilterItem()
    {
      bool res = false;
      if (currentLevel > 0)
      {
        currentLevel--;
        res = true;
      }
      return res;
    }

/*
 *     public void Select(AppItem app, FileItem file, string pathSubfolders)
    {
      FilterDefinition definition = (FilterDefinition) currentView.Filters[CurrentLevel];
      definition.SelectedValue = GetFieldValue(file, definition.Where);
      if (currentLevel + 1 < currentView.Filters.Count) currentLevel++;
    }
*/

    public string BuildQuery(int appID, string pathSubfolders)
    {
      SQLSelectBuilder sqlSelect = new SQLSelectBuilder();
      // build the SQL query respecting all the filters and let 
      // the query be executed from outside (this is different from MusicViewHandler)
      sqlSelect.AddTable("file");
      sqlSelect.AddWhereCond(String.Format("appid = {0}", appID));
      if (currentView == null)
      {
        // no View / leaf view activated: build standard queries
        //
        // a: "select file.*, '' as title2, '' as fieldtype2 from file where appid = {0} order by isfolder desc, uppertitle"
        // b: "select file.*, '' as title2, '' as fieldtype2 from file where appid = {0} and filepath = '{1}' order by isfolder desc, uppertitle"

        sqlSelect.AddField("file.*");
        sqlSelect.AddField("'' as title2");
        sqlSelect.AddField("'' as fieldtype2");
        if (pathSubfolders != "")
        {
          sqlSelect.AddWhereCond(String.Format("filepath = '{0}'", pathSubfolders));
        }
        sqlSelect.AddOrderField("isfolder desc");
        sqlSelect.AddOrderField("uppertitle");
      }
      else
      {
        // view is enabled
        for (int i = 0; i < CurrentLevel; ++i)
        {
          BuildFilter((FilterDefinition) currentView.Filters[i], sqlSelect);
        }
        BuildFields((FilterDefinition) currentView.Filters[CurrentLevel], sqlSelect);
        BuildWhere((FilterDefinition) currentView.Filters[CurrentLevel], sqlSelect);
        BuildRestriction((FilterDefinition) currentView.Filters[CurrentLevel], sqlSelect);
        BuildOrder((FilterDefinition) currentView.Filters[CurrentLevel], sqlSelect);
      }
      return sqlSelect.AsSQL;
    }


    void BuildFilter(FilterDefinition filter, SQLSelectBuilder sqlSelect)
    {
      sqlSelect.AddWhereCond(String.Format(" {0}='{1}'", GetFieldId(filter.Where), ProgramUtils.Encode(filter.SelectedValue)));
    }

    void BuildRestriction(FilterDefinition filter, SQLSelectBuilder sqlSelect)
    {
      if (filter.SqlOperator != String.Empty && filter.Restriction != String.Empty)
      {
        string whereClause = "";
        string restriction = filter.Restriction;
        restriction = restriction.Replace("*", "%");
        ProgramUtils.RemoveInvalidChars(ref restriction);
        if (filter.SqlOperator == "=")
        {
          bool isascii = false;
          for (int x = 0; x < restriction.Length; ++x)
          {
            if (!Char.IsDigit(restriction[x]))
            {
              isascii = true;
              break;
            }
          }
          if (isascii)
          {
            filter.SqlOperator = "like";
          }
        }
        whereClause = String.Format(" {0} {1} '{2}'", GetFieldName(filter.Where), filter.SqlOperator, restriction);
        sqlSelect.AddWhereCond(whereClause);
      }
    }

    void BuildWhere(FilterDefinition filter, SQLSelectBuilder sqlSelect)
    {
      if (filter.Where == "launchcount")
      {
        sqlSelect.AddWhereCond("launchcount <> ''");
        sqlSelect.AddWhereCond("launchcount IS NOT NULL");
        sqlSelect.AddWhereCond("launchcount > 0");
      }
      if (filter.Where == "lastTimeLaunched")
      {
        sqlSelect.AddWhereCond("lastTimeLaunched <> ''");
        sqlSelect.AddWhereCond("lastTimeLaunched IS NOT NULL");
      }
      if (filter.WhereValue != "*")
      {
        sqlSelect.AddWhereCond(String.Format(" {0}='{1}'", GetField(filter.Where), ProgramUtils.Encode(filter.WhereValue)));
      }
    }

    void BuildFields(FilterDefinition filter, SQLSelectBuilder sqlSelect)
    {
      // build fields to select for intermediate filter
      sqlSelect.Distinct = GetDistinct(filter.Where); // do we need a DISTINCT sql?
      sqlSelect.AddField(GetFieldNameForSelect(filter.Where)); // make sure TITLE field contains the first label-text
      sqlSelect.AddField(GetFieldTypeForSelect(filter.Where)); // datatype of TITLE-field (to avoid f****ing round errors)
      sqlSelect.AddField(GetFieldName2ForSelect(filter.Where)); // label2 where necessary (launchcount / lastlaunched)
      sqlSelect.AddField(GetFieldType2ForSelect(filter.Where)); // datatype of TITLE2-field
      if (CurrentLevel == MaxLevels - 1)
      {
        // build filelist for leaf-level
        sqlSelect.AddOrderField("isfolder desc");
        sqlSelect.AddOrderField("uppertitle");
      }
    }


    void BuildOrder(FilterDefinition filter, SQLSelectBuilder sqlSelect)
    {
      string orderClause = GetField(filter.Where);
      if (orderClause != "")
      {
        if (!filter.SortAscending) orderClause += " desc";
        else orderClause += " asc";
        if (filter.Limit > 0)
        {
          orderClause += String.Format(" Limit {0}", filter.Limit);
        }
        sqlSelect.AddOrderField(orderClause);
      }
    }

    string GetField(string where)
    {
      if (where == "title") return "title";
      if (where == "genre") return "genre";
      if (where == "genre2") return "genre2";
      if (where == "genre3") return "genre3";
      if (where == "genre4") return "genre4";
      if (where == "genre5") return "genre5";
      if (where == "country") return "country";
      if (where == "manufacturer") return "manufacturer";
      if (where == "year") return "year";
      if (where == "rating") return "rating";
      if (where == "launchcount") return "launchcount";
      if (where == "lastTimeLaunched") return "lastTimeLaunched";
      return "";
    }

    string GetFieldId(string where)
    {
      if (where == "title") return "title";
      if (where == "genre") return "genre";
      if (where == "genre2") return "genre2";
      if (where == "genre3") return "genre3";
      if (where == "genre4") return "genre4";
      if (where == "genre5") return "genre5";
      if (where == "country") return "country";
      if (where == "manufacturer") return "manufacturer";
      if (where == "year") return "year";
      if (where == "rating") return "rating";
      if (where == "launchcount") return "launchcount";
      if (where == "lastTimeLaunched") return "lastTimeLaunched";
      return "";
    }

    string GetFieldName(string where)
    {
      // yeah, that's great code :-)
      // maps WHERE-fieldname to a SQL-fieldname
      // and the MAY be different in the future.....
      if (where == "title") return "title";
      else if (where == "genre") return "genre";
      else if (where == "genre2") return "genre2";
      else if (where == "genre3") return "genre3";
      else if (where == "genre4") return "genre4";
      else if (where == "genre5") return "genre5";
      else if (where == "manufacturer") return "manufacturer";
      else if (where == "country") return "country";
      else if (where == "year") return "year";
      else if (where == "rating") return "rating";
      else if (where == "launchcount") return "launchcount";
      else if (where == "lastTimeLaunched") return "lastTimeLaunched";
      else return "";
    }

    string GetFieldNameForSelect(string where)
    {
      string res = "";
      if (where == "launchcount")
      {
        res = "file.*";
      }
      else if (where == "lastTimeLaunched")
      {
        res = "file.*";
      }
      else
      {
        res = GetFieldName(where);
        if (res == "")
        {
          res = "file.*";
        }
        else
        {
          res = res + " as title";
        }
      }
      return res;
    }

    string GetFieldName2ForSelect(string where)
    {
      string res = "";
      if (where == "launchcount")
      {
        res = "launchcount  as title2";
      }
      else if (where == "lastTimeLaunched")
      {
        res = "lastTimeLaunched  as title2";
      }
      else
      {
        res = "'' as title2";
      }
      return res;
    }

    string GetFieldTypeForSelect(string where)
    {
      if (where == "title") return "'STR' as fieldtype"; // watch quotes... these are SQL-strings!
      else if (where == "genre") return "'STR' as fieldtype";
      else if (where == "genre2") return "'STR' as fieldtype";
      else if (where == "genre3") return "'STR' as fieldtype";
      else if (where == "genre4") return "'STR' as fieldtype";
      else if (where == "genre5") return "'STR' as fieldtype";
      else if (where == "manufacturer") return "'STR' as fieldtype";
      else if (where == "country") return "'STR' as fieldtype";
      else if (where == "year") return "'INT' as fieldtype";
      else if (where == "rating") return "'INT' as fieldtype";
        //      else if (where == "launchcount") return "'INT'";
      else if (where == "launchcount") return "'STR' as fieldtype";
      else if (where == "lastTimeLaunched") return "'STR' as fieldtype";
      else return "'STR' as fieldtype";
    }

    string GetFieldType2ForSelect(string where)
    {
      if (where == "launchcount") return "'INT' as fieldtype2";
      else if (where == "lastTimeLaunched") return "'STR' as fieldtype2";
      else return "'' as fieldtype2";
    }

    bool GetDistinct(string where)
    {
      if (where == "title") return false;
      else if (where == "genre") return true;
      else if (where == "genre2") return true;
      else if (where == "genre3") return true;
      else if (where == "genre4") return true;
      else if (where == "genre5") return true;
      else if (where == "manufacturer") return true;
      else if (where == "country") return true;
      else if (where == "year") return true;
      else if (where == "rating") return true;
      else if (where == "launchcount") return false;
      else if (where == "lastTimeLaunched") return false;
      else return false;
    }


    string GetFieldValue(ProgramFilterItem filterItem, string where)
    {
      return filterItem.Title; 
/*
 *       if (where == "title") return filterItem.Title;
      else if (where == "genre") return filterItem.Genre;
      else if (where == "manufacturer") return filterItem.Manufacturer;
      else if (where == "country") return filterItem.Country;
      else if (where == "year") return ((int) filterItem.Year).ToString();
      else if (where == "rating") return ((int) filterItem.Rating).ToString();
      else return "";
*/      
    }


    public void SetLabel(ProgramFilterItem filterItem, ref GUIListItem item)
    {
      if (filterItem == null) return;
      FilterDefinition definition = (FilterDefinition) currentView.Filters[CurrentLevel];
      if ((definition.Where == "genre") || (definition.Where == "genre2") || (definition.Where == "genre3") || (definition.Where == "genre4") || (definition.Where == "genre5"))
      {
        item.Label = filterItem.Genre;
        item.Label2 = String.Empty;
        item.Label3 = String.Empty;
      }
      if (definition.Where == "year")
      {
        item.Label = (filterItem.Year).ToString();
        item.Label2 = String.Empty;
        item.Label3 = String.Empty;
      }
      else
      {
        item.Label = filterItem.Title;
        item.Label2 = String.Empty;
        item.Label3 = String.Empty;
      }

    }


  }
}