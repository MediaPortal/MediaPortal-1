using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;

namespace DreamBox
{
    public class Data
    {
        private string _Url = "";
        private string _UserName = "";
        private string _Password = "";
        private string _Command = "/cgi-bin/";

        public Data(string url, string username, string password)
        {
            _Url = url;
            _UserName = username;
            _Password = password;
        }

        public DataSet AllBouquets
        {
            get
            {
                Request request = new Request(_Url, _UserName, _Password);
                string sreturn = request.PostData(_Command + "getServices?ref=0");
                string[] allBouquets = sreturn.Split('\n');

                // convert to dataset
                DataSet ds = new DataSet();
                DataTable table = new DataTable("Bouquets");
                DataRow row = null;

                try
                {

                    table.Columns.Add("Ref", Type.GetType("System.String"));
                    table.Columns.Add("Name", Type.GetType("System.String"));

                    foreach (string bouquets in allBouquets)
                    {
                        if (bouquets.IndexOf(';') > -1)
                        {
                            string[] bouquet = bouquets.Split(';');
                            row = table.NewRow();
                            row["Ref"] = bouquet[0].ToString();
                            row["Name"] = bouquet[1].ToString();
                            table.Rows.Add(row);
                        }

                    }

                    ds.Tables.Add(table);

                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return ds;
            }
        }

        public DataSet UserTVBouquets
        {
            get
            {
                string userbouquests = "Bouquets (TV)".ToLower();
                string temp = "";
                Request request = new Request(_Url, _UserName, _Password);
                string sreturn = request.PostData(_Command + "getServices?ref=0");
                string[] allBouquets = sreturn.Split('\n');

                // convert to dataset
                DataSet ds = new DataSet();
                DataTable table = new DataTable("Bouquets");
                DataRow row = null;

                try
                {

                    table.Columns.Add("Ref", Type.GetType("System.String"));
                    table.Columns.Add("Name", Type.GetType("System.String"));

                    foreach (string s in allBouquets)
                    {
                        if (s.ToLower().Contains(userbouquests))
                        {
                            temp = s.Split(';')[0];
                            break;
                        }
                    }

                    sreturn = request.PostData(_Command + "getServices?ref=" + temp);
                    allBouquets = sreturn.Split('\n');

                    foreach (string bouquets in allBouquets)
                    {
                        if (bouquets.IndexOf(';') > -1 && bouquets.Length > 0) // HOTFIX!!!!! 28-10-2006
                        {
                            string[] bouquet = bouquets.Split(';');
                            row = table.NewRow();
                            row["Ref"] = bouquet[0].ToString();
                            row["Name"] = bouquet[1].ToString();
                            table.Rows.Add(row);
                        }

                    }

                    ds.Tables.Add(table);

                }
                catch (Exception ex)
                {
                    //throw ex;
                }

                return ds;
            }
        }

        public DataSet UserRadioBouquets
        {
            get
            {
                string userbouquests = "bouquets (radio)";
                string temp = "";
                Request request = new Request(_Url, _UserName, _Password);
                string sreturn = request.PostData(_Command + "getServices?ref=0");
                string[] allBouquets = sreturn.Split('\n');

                // convert to dataset
                DataSet ds = new DataSet();
                DataTable table = new DataTable("Bouquets");
                DataRow row = null;

                try
                {

                    table.Columns.Add("Ref", Type.GetType("System.String"));
                    table.Columns.Add("Name", Type.GetType("System.String"));

                    foreach (string s in allBouquets)
                    {
                        if (s.ToLower().Contains(userbouquests))  // HOTFIX!!!!! 28-10-2006
                        {
                            temp = s.Split(';')[0];
                            break;
                        }
                    }

                    sreturn = request.PostData(_Command + "getServices?ref=" + temp);
                    allBouquets = sreturn.Split('\n');

                    foreach (string bouquets in allBouquets)
                    {
                        if (bouquets.IndexOf(';') > -1)
                        {
                            string[] bouquet = bouquets.Split(';');
                            row = table.NewRow();
                            row["Ref"] = bouquet[0].ToString();
                            row["Name"] = bouquet[1].ToString();
                            table.Rows.Add(row);
                        }

                    }

                    ds.Tables.Add(table);

                }
                catch (Exception ex)
                {
                    throw ex;
                }

                return ds;
            }
        }

        public DataSet Channels(string reference)
        {
            Request request = new Request(_Url, _UserName, _Password);
            string sreturn = request.PostData(_Command + "getServices?ref=" + reference);
            string[] allBouquets = sreturn.Split('\n');

            // convert to dataset
            DataSet ds = new DataSet();
            DataTable table = new DataTable("Bouquets");
            DataRow row = null;

            try
            {

                table.Columns.Add("Ref", Type.GetType("System.String"));
                table.Columns.Add("Name", Type.GetType("System.String"));

                foreach (string bouquets in allBouquets)
                {
                    if (bouquets.IndexOf(';') > -1)
                    {
                        string[] bouquet = bouquets.Split(';');
                        row = table.NewRow();
                        row["Ref"] = bouquet[0].ToString();
                        row["Name"] = bouquet[1].ToString();
                        table.Rows.Add(row);
                    }

                }
                table.DefaultView.Sort = "Name";
                ds.Tables.Add(table);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ds;
        }

        public DataSet Recordings
        {
            get
            {
                Request request = new Request(_Url, _UserName, _Password);
                string sreturn = request.PostData("/body?mode=zap&zapmode=3&zapsubmode=1");
                string result = "";
                Regex objAlphaPattern = new Regex(@"channels\[0\].*?;");
                MatchCollection col = objAlphaPattern.Matches(sreturn);
                for (int i = 0; i < col.Count; i++)
                {
                    result = col[i].Value.ToString();
                }
                result = result.Replace(@"channels[0] = new Array(", "");
                string[] temparr = result.Split('\"');
                int amount = 0;
                for (int i = 0; i < temparr.GetUpperBound(0); i++)
                {
                    if (temparr[i].ToString().Trim() != "," && temparr[i].ToString().Trim() != "")
                    {
                        amount += 1;
                    }
                }
                string[] names = new string[amount];
                amount = 0;
                for (int i = 0; i < temparr.GetUpperBound(0); i++)
                {
                    if (temparr[i].ToString().Trim() != "," && temparr[i].ToString().Trim() != "")
                    {
                        names[amount] = temparr[i];
                        amount += 1;
                    }
                }
                // remove ending ','
                for (int i = 0; i < names.GetUpperBound(0); i++)
                {
                    if (names[i].EndsWith(","))
                        names[i] = names[i].Substring(0, names[i].Length - 1);
                }

                // get refs
                objAlphaPattern = new Regex(@"channelRefs\[0\].*?;");
                col = objAlphaPattern.Matches(sreturn);
                for (int i = 0; i < col.Count; i++)
                {
                    result = col[i].Value.ToString();
                }
                result = result.Replace(@"channelRefs[0] = new Array(", "");
                temparr = result.Split('\"');

                string[] refs = new string[names.GetUpperBound(0) + 1];
                amount = 0;
                for (int i = 0; i < temparr.GetUpperBound(0); i++)
                {
                    if (temparr[i].ToString().Trim() != "," && temparr[i].ToString().Trim() != "")
                    {
                        refs[amount] = temparr[i];
                        amount += 1;
                    }
                }



                // convert to dataset
                DataSet ds = new DataSet();
                DataTable table = new DataTable("Recordings");
                DataRow row = null;

                try
                {

                    table.Columns.Add("Ref", Type.GetType("System.String"));
                    table.Columns.Add("Name", Type.GetType("System.String"));

                    for (int i = 0; i < names.GetUpperBound(0) + 1; i++)
                    {
                        {
                            row = table.NewRow();
                            row["Ref"] = refs[i].ToString();
                            row["Name"] = names[i].ToString();
                            table.Rows.Add(row);
                        }
                    }
                    table.DefaultView.Sort = "Name";
                    ds.Tables.Add(table);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return ds;
            }
        }

        #region EPG
        public DataSet CurrentEPG
        {
            get
            {
                Request request = new Request(_Url, _UserName, _Password);
                string sreturn = request.PostData(_Command + "getcurrentepg");
                string result = "";
                Regex objAlphaPattern = new Regex("middle.*?", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                MatchCollection col = objAlphaPattern.Matches(sreturn);
                for (int i = 0; i < col.Count; i++)
                {
                    result = col[i].Value.ToString();
                }
                return null;
            }
        }
        #endregion
    }

}
