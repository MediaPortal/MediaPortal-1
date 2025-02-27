#region Copyright (C) 2005-2024 Team MediaPortal

// Copyright (C) 2005-2024 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.OleDb;

namespace TvMovieDbReader
{
  class Program
  {

    static void Main(string[] args)
    {
      OleDbConnection dbConn = null;
      try
      {
        string strDbPath = null;

        if (args.Length >= 1)
          strDbPath = args[0].Trim('\"');

        if (!File.Exists(strDbPath))
        {
          Console.WriteLine("ERROR: Database file invalid: " + strDbPath);
          return;
        }

        //Open the database
        //The x64 Microsoft.Jet.OLEDB.4.0 driver is not available and ACE driver is unable to open old Jet database. We need to open the db as 32bit process
        //and pass the result to the connected client.
        dbConn = new OleDbConnection(
          string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source={0};Mode=Share Deny None;Jet OLEDB:Engine Type=5;Jet OLEDB:Database Locking Mode=1;", strDbPath));
        dbConn.Open();

        StringBuilder sb = new StringBuilder(1024);
        const char DELIMITER = '\0';

        //Response to the client
        Console.WriteLine("OPEN");

        while (true)
        {
          string strLine = Console.ReadLine();
          if (!string.IsNullOrWhiteSpace(strLine))
          {
            string[] arg = strLine.Split(' ');
            switch (arg[0])
            {
              case "Close":
                //Response to the client
                Console.WriteLine("CLOSE");
                return;

              case "GetChannels":

                #region GetChannels
                string sqlSelect = "SELECT * FROM Sender WHERE (Favorit = true) AND (GueltigBis >=Now()) ORDER BY Bezeichnung ASC;";
                DataSet tvMovieTable = new DataSet("Sender");
                try
                {
                  using (OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, dbConn))
                  {
                    using (OleDbDataAdapter databaseAdapter = new OleDbDataAdapter(databaseCommand))
                    {
                      try
                      {
                        databaseAdapter.FillSchema(tvMovieTable, SchemaType.Source, "Sender");
                        databaseAdapter.Fill(tvMovieTable);
                      }
                      catch (Exception dsex)
                      {
                        //Response to the client
                        Console.WriteLine("ERROR: Exception filling Sender DataSet - " + dsex.Message);
                        return;
                      }
                    }
                  }
                }
                catch (OleDbException ex)
                {
                  //Response to the client
                  Console.WriteLine("ERROR: Accessing TV Movie Clickfinder database while reading stations - " + ex.Message);
                  return;
                }
                catch (Exception ex2)
                {
                  //Response to the client
                  Console.WriteLine("ERROR: Exception - " + ex2.Message);
                  return;
                }

                try
                {
                  foreach (DataRow sender in tvMovieTable.Tables["Table"].Rows)
                  {
                    string senderId = sender["ID"].ToString();
                    string senderKennung = sender["SenderKennung"].ToString();
                    string senderBez = sender["Bezeichnung"].ToString();
                    // these are non-vital for now.
                    string senderUrl = String.Empty;
                    string senderSort = "-1";
                    string senderZeichen = @"tvmovie_senderlogoplatzhalter.gif";
                    // Somehow TV Movie's db does not necessarily contain these columns...
                    try
                    {
                      senderUrl = sender["Webseite"].ToString();
                    }
                    catch (Exception) { }
                    try
                    {
                      senderSort = sender["SortNrTVMovie"].ToString();
                    }
                    catch (Exception) { }
                    try
                    {
                      senderZeichen = sender["Zeichen"].ToString();
                    }
                    catch (Exception) { }

                    sb.Clear();
                    sb.Append(senderId);
                    sb.Append(DELIMITER);
                    sb.Append(senderKennung);
                    sb.Append(DELIMITER);
                    sb.Append(senderBez);
                    sb.Append(DELIMITER);
                    sb.Append(senderUrl);
                    sb.Append(DELIMITER);
                    sb.Append(senderSort);
                    sb.Append(DELIMITER);
                    sb.Append(senderZeichen);

                    //Response to the client
                    Console.WriteLine("ROW: " + Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())));
                  }

                  //Response to the client
                  Console.WriteLine("ROWS_END");
                }
                catch (Exception ex)
                {
                  //Response to the client
                  Console.WriteLine("ERROR: Exception - " + ex.Message);
                  return;
                }
                #endregion

                break;

              case "GetChannelData":

                #region GetChannelData
                if (arg.Length == 2)
                {
                  sb.Clear();
                  string stationName = arg[1].Trim('\"');

                  // UNUSED: F16zu9 , live , untertitel , Dauer , Wiederholung
                  //sqlb.Append("SELECT * "); // need for saver schema filling
                  sb.Append(
                    "SELECT TVDaten.SenderKennung, TVDaten.Beginn, TVDaten.Ende, TVDaten.Sendung, TVDaten.Genre, TVDaten.Kurzkritik, TVDaten.KurzBeschreibung, TVDaten.Beschreibung");
                  sb.Append(
                    ", TVDaten.Audiodescription, TVDaten.DolbySuround, TVDaten.Stereo, TVDaten.DolbyDigital, TVDaten.Dolby, TVDaten.Zweikanalton");
                  sb.Append(", TVDaten.FSK, TVDaten.Herstellungsjahr, TVDaten.Originaltitel, TVDaten.Regie, TVDaten.Darsteller");
                  sb.Append(", TVDaten.Interessant, TVDaten.Bewertungen");
                  sb.Append(", TVDaten.live, TVDaten.Dauer, TVDaten.Herstellungsland,TVDaten.Wiederholung");
                  sb.Append(
                    " FROM TVDaten WHERE (((TVDaten.SenderKennung)=\"{0}\") AND ((TVDaten.Ende)>= #{1}#)) ORDER BY TVDaten.Beginn;");

                  DateTime importTime = DateTime.Now.Subtract(TimeSpan.FromHours(4));
                  sqlSelect = string.Format(sb.ToString(), stationName, importTime.ToString("yyyy-MM-dd HH:mm:ss"));
                  //("dd-MM-yyyy HH:mm:ss", System.Globalization.CultureInfo.CurrentCulture));
                  OleDbTransaction databaseTransaction = null;
                  using (OleDbCommand databaseCommand = new OleDbCommand(sqlSelect, dbConn))
                  {
                    try
                    {
                      // The main app might change epg details while importing
                      databaseTransaction = dbConn.BeginTransaction(IsolationLevel.ReadCommitted);
                      databaseCommand.Transaction = databaseTransaction;
                      using (OleDbDataReader reader = databaseCommand.ExecuteReader(CommandBehavior.SequentialAccess))
                      {
                        while (reader.Read())
                        {
                          sb.Clear();
                          for (int i = 0; i < 25; i++)
                          {
                            sb.Append(reader[i]);
                            sb.Append(DELIMITER);
                          }

                          sb.Length--; //remove last delimiter

                          //Response to the client
                          Console.WriteLine("ROW: " + Convert.ToBase64String(Encoding.UTF8.GetBytes(sb.ToString())));
                        }

                        //Response to the client
                        Console.WriteLine("ROWS_END");

                        databaseTransaction.Commit();
                        reader.Close();
                      }
                    }
                    catch (OleDbException ex)
                    {
                      databaseTransaction.Rollback();

                      //Response to the client
                      Console.WriteLine("ERROR: Exception: " + ex.Message);
                      return;
                    }
                    catch (Exception ex1)
                    {
                      try
                      {
                        databaseTransaction.Rollback();
                      }
                      catch (Exception) { }

                      //Response to the client
                      Console.WriteLine("ERROR: Exception: " + ex1.Message);
                      return;
                    }
                  }
                }
                else
                {
                  //Response to the client
                  Console.WriteLine("INVALID_ARGUMENTS");
                  break;
                }

                #endregion

                break;

              default:
                //Response to the client
                Console.WriteLine("INVALID_COMMAND");
                break;
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.Write("ERROR: " + ex.Message);
      }
      finally
      {
        if (dbConn != null)
          dbConn.Close();
      }
    }
  }
}
