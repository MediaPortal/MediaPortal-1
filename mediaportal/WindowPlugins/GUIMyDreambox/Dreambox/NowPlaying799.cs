using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;


namespace DreamBox
{
    public class NowPlaying799
    {
        

        public NowPlaying799()
        {

        }

        public string NowPlaying()
        {
            string nowPlaying = "";
            Request request = new Request("http://www.club977.com/", null, null);
            nowPlaying = request.PostData("");
            return RunningPart(nowPlaying);
        }

        string RunningPart(String strToCheck)
        {
            Regex objAlphaPattern = new Regex("<td height=\"30\" class=\"last\">?.*<br>");
            Match m = objAlphaPattern.Match(strToCheck);
            string s = m.ToString().Replace("<td height=\"30\" class=\"last\">", "").Replace("<br>", "");

            return s;

        }

    }
}
