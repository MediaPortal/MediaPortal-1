#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Globalization;
using System.IO;
using System.Net;
using System.Collections;
using System.Threading;

namespace MediaPortal.Util
{

  /* This is a port of Vilistextum from C to C# which is released under the GPL
   * All conversion work is by Andy Qua (including any bugs introduced :).
   *
   * Apologies for the style - its been pretty much a straight forward port 
   * with as few modifications as possible so some of the method names are non-standard
   * and even still slightly in their original german :)
   *
   * The usage is quite simple, create an instance of the class passing in the HTML text
   * as a parameter then call the ToString method to call the convertor and
   * get the converted text.
   */
  public class HtmlToText
  {
    String html;
    char ch;
    int pos;
    bool convert_characters = true;

    int LEFT = 1;
    int CENTER = 2;
    int RIGHT = 3;

    int spreads = 50;
    int hr_spreads = 50;
    int paragraph = 0;

    int tab = 4;         // tabulator
    int spaces = 0;      // spaces at beginning of line
    bool nooutput = false;    // for SCRIPT, etc: no output
    int orderedlist = 0; // OL

    int div_test = 0;

    string word = "";

    string line = "";
    int line_len = 0;       // apparent length of the line
    int line_len_old = 0;
    int line_pos = 0;       // true length of line
    int word_len = 0;         // apparent length of the word
    int word_pos = 0;         // true length of word
    int nrBlankLines = 0; // how many line were blank
    int noleadingblanks = 0;  // remove blanks lines at the start of the ouput

    bool convert_tags = false;
    int errorlevel = 1;
    int shrink_lines = 0;
    bool option_links = false;
    bool option_title = false;
    bool remove_empty_alt = true;   /* dont show [] for <IMG ALT="">  */
    bool option_no_image = true;    /* don't show [Image] */
    bool option_no_alt = true;    /* don't show [alt text] for <IMG ALT="alt text" */
    bool stripMultipleBlankLines = true; // Strip multiple blank lines
    bool removeLinesWithOnlyLinks = true;

    // Dynamic align added by autophile@starband.net 29 Mar 2002
    Stack align = new Stack();

    int references_count = 0;
    string references;
    string[] schemes = { "ftp://", "file://", "http://", "gopher://", "mailto:", "news:", "nntp://", "telnet://", "wais://", "prospero://" };

    char bullet_style = ' ';

    int definition_list = 0;

    string default_image = "Image";

    bool pre = false; /* for PRE-Tag */

    /**
     * rename this method to test the convertor - should be able to be compiled stand-alone
     */
    public static void Main(string[] args)
    {
      Thread.CurrentThread.Name = "HtmlToText";
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(args[0]);

      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      Stream stream = response.GetResponseStream();
      StreamReader r = new StreamReader(stream);
      string data = r.ReadToEnd();


      HtmlToText app = new HtmlToText(data);
      string text = app.ToString();
      Console.WriteLine(text);

    }

    public HtmlToText(string text)
    {
      html = text;

      reset();
    }

    public void reset()
    {
      pos = 0;
    }

    private int nextChar()
    {
      if (pos + 1 <= html.Length)
        return html[pos++];
      else
        return -1;
    }

    String text = "";
    public override string ToString()
    {
      int c;
      while ((c = nextChar()) != -1)
      {
        ch = (char)c;
        switch (c)
        {
          case '<':
            readTag();
            break;

          case '&':
            readEntity();
            break;

          case 173: // Soft hyphen ignore it
            break;

          case 9: // tab
            if (pre)
              word_plus_ch(0x09);
            else
              word_end();
            break;

          case 13: // CR
          case 10:
            word_end();
            if (pre)
              line_break();
            break;

          // Microsoft ...
          case 0x80:
          case 0x81:
          case 0x82:
          case 0x83:
          case 0x84:
          case 0x85:
          case 0x86:
          case 0x87:
          case 0x88:
          case 0x89:
          case 0x8a:
          case 0x8b:
          case 0x8c:
          case 0x8d:
          case 0x8e:
          case 0x8f:
          case 0x90:
          case 0x91:
          case 0x92:
          case 0x93:
          case 0x94:
          case 0x95:
          case 0x96:
          case 0x97:
          case 0x98:
          case 0x99:
          case 0x9a:
          case 0x9b:
          case 0x9c:
          case 0x9d:
          case 0x9e:
          case 0x9f:
            {
              if (convert_characters)
                microsoft_character(ch);
              else
                word_plus_ch(ch);
              break;
            }
          default:
            if (!pre)
            {
              if (ch == ' ')
                word_end();
              else
                word_plus_ch(ch);
            }
            else
              word_plus_ch(ch);
            break;

        }
      }

      return text;
    }

    // ------------------------------------------------

    // get the next attribute and writes it to attr_name and attr_ctnt.
    // attr_name is converted to uppercase.
    char get_attr(out string attr_name, out string attr_ctnt)
    {
      string temp;

      attr_name = "";
      attr_ctnt = "";

      // skip whitespace
      do
      {
        ch = (char)nextChar();
      } while (Char.IsWhiteSpace(ch) && ch != '>');

      if (ch == '>')
        return '>';

      do
      {
        attr_name += ch.ToString();
        ch = (char)nextChar();
      } while (ch != '=' && ch != '>');

      if (ch == '>')
        return '>';

      // content of attribute
      ch = (char)nextChar();

      // skip white_space
      while (Char.IsWhiteSpace(ch) && ch != '>')
        ch = (char)nextChar();

      temp = "";

      // if quoted
      if (ch == '"' || ch == '\'')
      {
        // attribute looks like alt="bla" or alt='bla'.
        // we'll have to remember what the quote was.
        char quote = ch;
        ch = (char)nextChar();
        while (quote != ch)
        {
          temp += ch.ToString();
          ch = (char)nextChar();
        }
        ch = (char)nextChar();
      }
      else
      {
        // attribute looks like alt=bla
        temp = ch.ToString();
        while (Char.IsWhiteSpace(ch) && ch != '>')
        {
          ch = (char)nextChar();
          temp += ch.ToString();
        }
      }

      attr_name = attr_name.ToUpper();
      if (CMP("ALT", attr_name))
        parse_entities(ref temp);

      attr_ctnt = temp;

      return ch;
    } // end get_attr

    // ------------------------------------------------


    private void readTag()
    {
      string str = "";

      ch = ((char)nextChar());
      ch = Char.ToUpper(ch);

      // letter -> normal tag
      // '!' -> CDATA section or comment
      // '/' -> end tag
      // '?' -> XML processing instruction
      if (!Char.IsLetter(ch) && ch != '/' && ch != '!' && ch != '?')
      {
        word_plus_ch('<');
        pos--;
        return;
      }

      // read html tag
      while (ch != '>' && ch != ' ' && ch != 13 && ch != 10)
      {
        str += ch.ToString();
        ch = ((char)nextChar());
        ch = Char.ToUpper(ch);
      }

      // first all tags, that affect if there is any output at all
      if (CMP("SCRIPT", str)) { start_nooutput(); }
      else if (CMP("/SCRIPT", str)) { end_nooutput(); }
      else if (CMP("STYLE", str)) { start_nooutput(); }
      else if (CMP("/STYLE", str)) { end_nooutput(); }
      else if (CMP("TITLE", str))
      {
        if (option_title)
        {
          push_align(LEFT);
          neuer_paragraph();
        }
        else
        {
          word_end();
          print_line();
          nooutput = true;
        }
      }
      else if (CMP("/TITLE", str))
      {
        if (option_title)
        {
          paragraphen_end();
          print_line();
        }
        else
        {
          word_end();
          clear_line();
          print_line();
          nooutput = false;
        }
      }

      if (!nooutput)
      {
        if (CMP("/HTML", str)) { } //Environment.Exit( 1 ); }
        else if (CMP("!DOCTYPE", str)) { while ((ch = (char)nextChar()) != '>'); }
        //			else if ( CMP("META", str) )      { find_encoding(); }
        else if (CMP("?XML", str))
        {
          // xml default charset is utf-8
          //				set_iconv_charset("utf-8");
          //				find_encoding();
        }
        else if (CMP("A", str)) { href(); word_plus_ch('['); } //if ( !option_links) start_nooutput();}
        else if (CMP("/A", str))
        {
          if (word.Length > 0 && word[word.Length - 1] != '[')
            word_plus_ch(']');
          else if (word.Length > 0)
          {
            word = word.Remove(word.Length - 1, 1);
            word_len--;
            word_pos--;
          }
        }
        // Linebreak
        else if (CMP("BR", str)) { line_break(); }
        else if (CMP("BR/", str)) { line_break(); } // xhtml

        else if (CMP("P", str)) { start_p(); }
        else if (CMP("/P", str)) { paragraphen_end(); }
        else if (CMP("BLOCKQUOTE", str)) { start_p(); }
        else if (CMP("/BLOCKQUOTE", str)) { paragraphen_end(); }
        else if (CMP("Q", str)) { word_plus_ch('"'); }
        else if (CMP("/Q", str)) { word_plus_ch('"'); }

          // Convert these Tags
        else if (CMP("B", str)) { if (convert_tags) { word_plus_ch('*'); } }
        else if (CMP("/B", str)) { if (convert_tags) { word_plus_ch('*'); } }
        else if (CMP("I", str)) { if (convert_tags) { word_plus_ch('/'); } }
        else if (CMP("/I", str)) { if (convert_tags) { word_plus_ch('/'); } }
        else if (CMP("U", str)) { if (convert_tags) { word_plus_ch('_'); } } // deprecated
        else if (CMP("/U", str)) { if (convert_tags) { word_plus_ch('_'); } } // deprecated
        else if (CMP("STRONG", str)) { if (convert_tags) { word_plus_ch('*'); } }
        else if (CMP("/STRONG", str)) { if (convert_tags) { word_plus_ch('*'); } }
        else if (CMP("EM", str)) { if (convert_tags) { word_plus_ch('/'); } }
        else if (CMP("/EM", str)) { if (convert_tags) { word_plus_ch('/'); } }
        // headings
        else if (CMP("H1", str)) { start_p(); }
        else if (CMP("/H1", str)) { paragraphen_end(); }
        else if (CMP("H2", str)) { start_p(); }
        else if (CMP("/H2", str)) { paragraphen_end(); }
        else if (CMP("H3", str)) { start_p(); }
        else if (CMP("/H3", str)) { paragraphen_end(); }
        else if (CMP("H4", str)) { start_p(); }
        else if (CMP("/H4", str)) { paragraphen_end(); }
        else if (CMP("H5", str)) { start_p(); }
        else if (CMP("/H5", str)) { paragraphen_end(); }
        else if (CMP("H6", str)) { start_p(); }
        else if (CMP("/H6", str)) { paragraphen_end(); }

        else if (CMP("HR", str)) { hr(); }
        else if (CMP("HR/", str)) { hr(); } // xhtml

        else if (CMP("LI", str)) { start_lis(); }
        else if (CMP("/LI", str)) { end_lis(); }
        else if (CMP("UL", str)) { start_uls(); }
        else if (CMP("/UL", str)) { end_uls(); return; }
        else if (CMP("DIR", str)) { start_uls(); }       // deprecated
        else if (CMP("/DIR", str)) { end_uls(); return; } // deprecated
        else if (CMP("MENU", str)) { start_uls(); }       // deprecated
        else if (CMP("/MENU", str)) { end_uls(); return; } // deprecated
        else if (CMP("OL", str)) { start_ols(); }
        else if (CMP("/OL", str)) { end_ols(); }

        else if (CMP("DIV", str)) { start_div(0); }
        else if (CMP("/DIV", str)) { end_div(); }
        else if (CMP("CENTER", str)) { start_div(CENTER); } // deprecated
        else if (CMP("/CENTER", str)) { end_div(); }         // deprecated
        else if (CMP("RIGHT", str)) { start_div(RIGHT); }
        else if (CMP("/RIGHT", str)) { end_div(); }

        // tags with alt attribute
        else if (CMP("IMG", str)) { image(default_image, 1); }
        else if (CMP("APPLET", str)) { image("Applet", 1); } // deprecated
        else if (CMP("AREA", str)) { image("Area", 0); }
        else if (CMP("INPUT", str)) { image("Input", 0); }

         // table
        else if (CMP("TABLE", str)) { push_align(LEFT); neuer_paragraph(); }
        else if (CMP("/TABLE", str)) { paragraphen_end(); }
        else if (CMP("TD", str)) { word_plus_ch(' '); }
        else if (CMP("/TD", str)) { }
        else if (CMP("TH", str)) { }
        else if (CMP("/TH", str)) { }
        else if (CMP("TR", str)) { line_break(); }
        else if (CMP("/TR", str)) { }
        else if (CMP("CAPTION", str)) { }
        else if (CMP("/CAPTION", str)) { }

        else if (CMP("PRE", str)) { start_p(); pre = true; }
        else if (CMP("/PRE", str)) { paragraphen_end(); pre = false; }

        else if (CMP("DL", str)) { start_dl(); } // Definition List
        else if (CMP("/DL", str)) { end_dl(); }
        else if (CMP("DT", str)) { start_dt(); } // Definition Title
        else if (CMP("/DT", str)) { end_dt(); }
        else if (CMP("DD", str)) { start_dd(); } // Definition Description
        else if (CMP("/DD", str)) { end_dd(); }

          // tags for forms
        else if (CMP("FORM", str)) { }
        else if (CMP("/FORM", str)) { }
        else if (CMP("BUTTON", str)) { } // TODO: extract name?
        else if (CMP("/BUTTON", str)) { }
        else if (CMP("FIELDSET", str)) { }
        else if (CMP("/FIELDSET", str)) { }
        else if (CMP("TEXTAREA", str)) { }
        else if (CMP("/TEXTAREA", str)) { }
        else if (CMP("LEGEND", str)) { }
        else if (CMP("/LEGEND", str)) { }
        else if (CMP("LABEL", str)) { }
        else if (CMP("/LABEL", str)) { }

       // tags that have no visible effect
        else if (CMP("SAMP", str)) { }
        else if (CMP("/SAMP", str)) { }
        else if (CMP("CODE", str)) { }
        else if (CMP("/CODE", str)) { }
        else if (CMP("ABBR", str)) { }
        else if (CMP("/ABBR", str)) { }
        else if (CMP("ACRONYM", str)) { }
        else if (CMP("/ACRONYM", str)) { }
        else if (CMP("BIG", str)) { }
        else if (CMP("/BIG", str)) { }
        else if (CMP("VAR", str)) { }
        else if (CMP("/VAR", str)) { }
        else if (CMP("KBD", str)) { }
        else if (CMP("/KBD", str)) { }

      // tags that should have some visible effect
        else if (CMP("BDO", str)) { }
        else if (CMP("/BDO", str)) { }
        else if (CMP("INS", str)) { }
        else if (CMP("/INS", str)) { }
        else if (CMP("DEL", str)) { }
        else if (CMP("/DEL", str)) { }
        else if (CMP("S", str)) { } // deprecated
        else if (CMP("/S", str)) { } // deprecated
        else if (CMP("STRIKE", str)) { } // deprecated
        else if (CMP("/STRIKE", str)) { } // deprecated

        // those tags are ignored
        else if (CMP("HTML", str)) { }
        else if (CMP("BASE", str)) { }
        else if (CMP("LINK", str)) { }
        else if (CMP("BASEFONT", str)) { } // deprecated

        else if (CMP("HEAD", str)) { }
        else if (CMP("/HEAD", str)) { }
        else if (CMP("BODY", str)) { }
        else if (CMP("/BODY", str)) { }
        else if (CMP("FONT", str)) { } // deprecated
        else if (CMP("/FONT", str)) { } // deprecated
        else if (CMP("MAP", str)) { }
        else if (CMP("/MAP", str)) { }
        else if (CMP("SUP", str)) { }
        else if (CMP("/SUP", str)) { }
        else if (CMP("ADDRESS", str)) { }
        else if (CMP("/ADDRESS", str)) { }
        else if (CMP("TT", str)) { }
        else if (CMP("/TT", str)) { }
        else if (CMP("SUB", str)) { }
        else if (CMP("/SUB", str)) { }
        else if (CMP("NOSCRIPT", str)) { }
        else if (CMP("/NOSCRIPT", str)) { }
        else if (CMP("SMALL", str)) { }
        else if (CMP("/SMALL", str)) { }
        else if (CMP("SPAN", str)) { }
        else if (CMP("/SPAN", str)) { }
        else if (CMP("DFN", str)) { }
        else if (CMP("/DFN", str)) { }
        else if (CMP("BLINK", str)) { }
        else if (CMP("/BLINK", str)) { }
        else if (CMP("CITE", str)) { }
        else if (CMP("/CITE", str)) { }

        else if (CMP("NOBR", str)) { }
        else if (CMP("/NOBR", str)) { }
        else if (CMP("SELECT", str)) { }
        else if (CMP("/SELECT", str)) { }
        else if (CMP("OPTION", str)) { }

        else if (CMP("FRAME", str)) { }
        else if (CMP("/FRAME", str)) { }
        else if (CMP("FRAMESET", str)) { }
        else if (CMP("/FRAMESET", str)) { }
        else if (CMP("NOFRAMES", str)) { }
        else if (CMP("/NOFRAMES", str)) { }
        else if (CMP("IFRAME", str)) { }
        else if (CMP("/IFRAME", str)) { }
        else if (CMP("LAYER", str)) { }
        else if (CMP("/LAYER", str)) { }
        else if (CMP("ILAYER", str)) { }
        else if (CMP("/ILAYER", str)) { }
        else if (CMP("NOLAYER", str)) { }
        else if (CMP("/NOLAYER", str)) { }

        else if (CMP("COL", str)) { }
        else if (CMP("COLGROUP", str)) { }
        else if (CMP("/COLGROUP", str)) { }
        else if (CMP("ISINDEX", str)) { } // deprecated
        else if (CMP("THEAD", str)) { }
        else if (CMP("/THEAD", str)) { }
        else if (CMP("TFOOT", str)) { }
        else if (CMP("/TFOOT", str)) { }
        else if (CMP("TBODY", str)) { }
        else if (CMP("/TBODY", str)) { }
        else if (CMP("PARAM", str)) { }
        else if (CMP("/PARAM", str)) { }
        else if (CMP("OBJECT", str)) { }
        else if (CMP("/OBJECT", str)) { }
        else if (CMP("OPTGROUP", str)) { }
        else if (CMP("/OPTGROUP", str)) { }

        else if (CMP("/AREA", str)) { }

        else if (str.StartsWith("!--"))
        {
          pos--;
          pos--;
          pos--;
          ch = kill_comment();
        } // Comment

          // these have to be ignored, to avoid the following error to show up
        else if (CMP("SCRIPT", str)) { }
        else if (CMP("/SCRIPT", str)) { }
        else if (CMP("STYLE", str)) { }
        else if (CMP("/STYLE", str)) { }
        else { if (errorlevel >= 2) { print_error("tag ignored: ", str); } }
      }

      // Skip attributes
      while (ch != '>')
      {
        string attr;
        string attr_cont;

        ch = get_attr(out attr, out attr_cont);
      }
    }


    private void readEntity()
    {
      string str = "&";
      do
      {
        ch = (char)nextChar();
        str += ch.ToString();
      } while (Char.IsLetterOrDigit(ch) || ch == '#');

      // if last char is no ';', then the string is no valid entity.
      // maybe it is something like &nbsp or even '& '
      if (ch != ';')
      {
        // save last char
        pos--;
        str = str.Remove(str.Length - 1, 1);
      }
      else
      {
        // valid entity
      }

      string tmp = parseEntity(str);

      // str contains the converted entity or the original string
      word_plus_string(tmp);
    }

    /* parses entities in string */
    void parse_entities(ref string s)
    {
      int i = 0;
      string entity = "";
      int len = s.Length;
      string result = "";

      while (i < len)
      {
        while (i < len && s[i] != '&')
          result += s[i++].ToString();

        if (i < len && s[i] == '&')
        {
          while (i < len && s[i] != ';' && !Char.IsWhiteSpace(s[i]))
            entity += s[i++].ToString();
          parseEntity(entity);

          result += entity;
        }
        i++;
      }

      s = result;
    } /* end parse_entities */

    private string parseEntity(String entity)
    {
      string conv = "";
      if (entity_number(entity, out conv) ||
          html_entity(entity, out conv) ||
          latin1(entity, out conv) ||
          microsoft_entities(entity, out conv) ||
          unicode_entity(entity, out conv))
      {
        return conv;
      }
      else
        return "";
    }

    private bool CMP(string s1, string s2)
    {
      return s1.Equals(s2);
    }

    private bool set_char(string s1, out string s2, int rep)
    {
      s2 = ((char)rep).ToString();
      return true;
    }

    private int extract_entity_number(string s)
    {
      int number;

      // Numeric entity
      if (s.Length > 2 && (s[0] == '&') && (s[1] == '#'))
      {
        // Hex entity
        if (s[0] == 'x' || s[0] == 'X')
        {
          s = s.Substring(3);
          number = int.Parse(s, NumberStyles.AllowHexSpecifier);
        }
        // Decimal entity
        else
        {
          s = s.Substring(2);
          if (s.EndsWith(";"))
            s = s.Remove(s.Length - 1, 1);
          number = int.Parse(s);
        }
        return number;
      }
      else
      {
        return -1;
      }
    }

    private bool entity_number(string str, out string conv)
    {
      int number;

      number = extract_entity_number(str);
      // printf("entity_number: %d\n", number);

      // no numeric entity
      if (number == -1)
      {
        conv = "";
        return false;
      }
      else
      {
        // ascii printable character 32-127
        if ((number >= 32) && (number <= 127))
        {
          set_char(str, out conv, number);
          return true;
        }
        // ansi printable character 160-255
        else if ((number >= 160) && (number <= 255))
        {
          // latin1 soft hyphen, just swallow it and return empty string
          if (number == 173)
          {
            conv = "";
            return true;
          }
          set_char(str, out conv, number);
          return true;
        }
        // ascii control character -> return empty string
        else if ((number >= 0) && (number < 32))
        {
          conv = "";
          return true;
        }
      }
      conv = "";
      return false;
    }

    bool html_entity(string str, out string conv)
    {
      if (CMP("&quot;", str)) { return (set_char(str, out conv, '"')); }
      else if (CMP("&;", str)) { return (set_char(str, out conv, '&')); } // for those brain damaged ones
      else if (CMP("&amp;", str)) { return (set_char(str, out conv, '&')); }
      else if (CMP("&gt;", str)) { return (set_char(str, out conv, '>')); }
      else if (CMP("&lt;", str)) { return (set_char(str, out conv, '<')); }
      else if (CMP("&apos;", str)) { return (set_char(str, out conv, '\'')); }
      else
      {
        conv = "";
        // found no html entity
        return false;
      }
    }

    private bool latin1(string str, out string conv)
    {
      if (CMP("&nbsp;", str)) { return (set_char(str, out conv, 160)); } // no-break space
      else if (CMP("&iexcl;", str)) { return (set_char(str, out conv, 161)); } // inverted exclamation mark
      else if (CMP("&cent;", str)) { return (set_char(str, out conv, 162)); } // cent sign
      else if (CMP("&pound;", str)) { return (set_char(str, out conv, 163)); } // pound sterling sign
      else if (CMP("&curren;", str)) { return (set_char(str, out conv, 164)); } // general currency sign
      else if (CMP("&yen;", str)) { return (set_char(str, out conv, 165)); } // yen sign
      else if (CMP("&brvbar;", str)) { return (set_char(str, out conv, 166)); } // broken (vertical) bar
      else if (CMP("&sect;", str)) { return (set_char(str, out conv, 167)); } // section sign
      else if (CMP("&uml;", str)) { return (set_char(str, out conv, 168)); } // umlaut (dieresis)
      else if (CMP("&copy;", str)) { return (set_char(str, out conv, 169)); } // copyright sign
      else if (CMP("&ordf;", str)) { return (set_char(str, out conv, 170)); } // ordinal indicator, feminine
      else if (CMP("&laquo;", str)) { return (set_char(str, out conv, 171)); } // angle quotation mark, left
      else if (CMP("&not;", str)) { return (set_char(str, out conv, 172)); } // not sign
      else if (CMP("&shy;", str)) { return (set_char(str, out conv, '\0')); } // soft hyphen, just swallow it
      else if (CMP("&reg;", str)) { return (set_char(str, out conv, 174)); } // registered sign
      else if (CMP("&macr;", str)) { return (set_char(str, out conv, 175)); } // macron
      else if (CMP("&deg;", str)) { return (set_char(str, out conv, 176)); } // degree sign
      else if (CMP("&plusmn;", str)) { return (set_char(str, out conv, 177)); } // plus-or-minus sign
      else if (CMP("&sup2;", str)) { return (set_char(str, out conv, 178)); } // superscript two
      else if (CMP("&sup3;", str)) { return (set_char(str, out conv, 179)); } // superscript three
      else if (CMP("&acute;", str)) { return (set_char(str, out conv, 180)); } // acute accent
      else if (CMP("&micro;", str)) { return (set_char(str, out conv, 181)); } // micro sign
      else if (CMP("&para;", str)) { return (set_char(str, out conv, 182)); } // pilcrow (paragraph sign)
      else if (CMP("&middot;", str)) { return (set_char(str, out conv, 183)); } // middle dot
      else if (CMP("&cedil;", str)) { return (set_char(str, out conv, 184)); } // cedilla
      else if (CMP("&sup1;", str)) { return (set_char(str, out conv, 185)); } // superscript one
      else if (CMP("&ordm;", str)) { return (set_char(str, out conv, 186)); } // ordinal indicator, masculine
      else if (CMP("&raquo;", str)) { return (set_char(str, out conv, 187)); } // angle quotation mark, right
      else if (CMP("&frac14;", str)) { return (set_char(str, out conv, 188)); } // fraction one-quarter
      else if (CMP("&frac12;", str)) { return (set_char(str, out conv, 189)); } // fraction one-half
      else if (CMP("&frac34;", str)) { return (set_char(str, out conv, 190)); } // fraction three-quarters
      else if (CMP("&iquest;", str)) { return (set_char(str, out conv, 191)); } // inverted question mark
      else if (CMP("&Agrave;", str)) { return (set_char(str, out conv, 192)); } // capital A, grave accent
      else if (CMP("&Aacute;", str)) { return (set_char(str, out conv, 193)); } // capital A, acute accent
      else if (CMP("&Acirc;", str)) { return (set_char(str, out conv, 194)); } // capital A, circumflex accent
      else if (CMP("&Atilde;", str)) { return (set_char(str, out conv, 195)); } // capital A, tilde
      else if (CMP("&Auml;", str)) { return (set_char(str, out conv, 196)); } // capital A, dieresis or umlaut mark
      else if (CMP("&Aring;", str)) { return (set_char(str, out conv, 197)); } // capital A, ring
      else if (CMP("&AElig;", str)) { return (set_char(str, out conv, 198)); } // capital AE diphthong (ligature)
      else if (CMP("&Ccedil;", str)) { return (set_char(str, out conv, 199)); } // capital C, cedilla
      else if (CMP("&Egrave;", str)) { return (set_char(str, out conv, 200)); } // capital E, grave accent
      else if (CMP("&Eacute;", str)) { return (set_char(str, out conv, 201)); } // capital E, acute accent
      else if (CMP("&Ecirc;", str)) { return (set_char(str, out conv, 202)); } // capital E, circumflex accent
      else if (CMP("&Euml;", str)) { return (set_char(str, out conv, 203)); } // capital E, dieresis or umlaut mark
      else if (CMP("&Igrave;", str)) { return (set_char(str, out conv, 204)); } // capital I, grave accent
      else if (CMP("&Iacute;", str)) { return (set_char(str, out conv, 205)); } // capital I, acute accent
      else if (CMP("&Icirc;", str)) { return (set_char(str, out conv, 206)); } // capital I, circumflex accent
      else if (CMP("&Iuml;", str)) { return (set_char(str, out conv, 207)); } // capital I, dieresis or umlaut mark
      else if (CMP("&ETH;", str)) { return (set_char(str, out conv, 208)); } // capital Eth, Icelandic
      else if (CMP("&Ntilde;", str)) { return (set_char(str, out conv, 209)); } // capital N, tilde
      else if (CMP("&Ograve;", str)) { return (set_char(str, out conv, 210)); } // capital O, grave accent
      else if (CMP("&Oacute;", str)) { return (set_char(str, out conv, 211)); } // capital O, acute accent
      else if (CMP("&Ocirc;", str)) { return (set_char(str, out conv, 212)); } // capital O, circumflex accent
      else if (CMP("&Otilde;", str)) { return (set_char(str, out conv, 213)); } // capital O, tilde
      else if (CMP("&Ouml;", str)) { return (set_char(str, out conv, 214)); } // capital O, dieresis or umlaut mark
      else if (CMP("&times;", str)) { return (set_char(str, out conv, 215)); } // multiply sign
      else if (CMP("&Oslash;", str)) { return (set_char(str, out conv, 216)); } // capital O, slash
      else if (CMP("&Ugrave;", str)) { return (set_char(str, out conv, 217)); } // capital U, grave accent
      else if (CMP("&Uacute;", str)) { return (set_char(str, out conv, 218)); } // capital U, acute accent
      else if (CMP("&Ucirc;", str)) { return (set_char(str, out conv, 219)); } // capital U, circumflex accent
      else if (CMP("&Uuml;", str)) { return (set_char(str, out conv, 220)); } // capital U, dieresis or umlaut mark
      else if (CMP("&Yacute;", str)) { return (set_char(str, out conv, 221)); } // capital Y, acute accent
      else if (CMP("&THORN;", str)) { return (set_char(str, out conv, 222)); } // capital THORN, Icelandic
      else if (CMP("&szlig;", str)) { return (set_char(str, out conv, 223)); } // small sharp s, German (sz ligature)
      else if (CMP("&agrave;", str)) { return (set_char(str, out conv, 224)); } // small a, grave accent
      else if (CMP("&aacute;", str)) { return (set_char(str, out conv, 225)); } // small a, acute accent
      else if (CMP("&acirc;", str)) { return (set_char(str, out conv, 226)); } // small a, circumflex accent
      else if (CMP("&atilde;", str)) { return (set_char(str, out conv, 227)); } // small a, tilde
      else if (CMP("&auml;", str)) { return (set_char(str, out conv, 228)); } // small a, dieresis or umlaut mark
      else if (CMP("&aring;", str)) { return (set_char(str, out conv, 229)); } // small a, ring
      else if (CMP("&aelig;", str)) { return (set_char(str, out conv, 230)); } // small ae diphthong (ligature)
      else if (CMP("&ccedil;", str)) { return (set_char(str, out conv, 231)); } // small c, cedilla
      else if (CMP("&egrave;", str)) { return (set_char(str, out conv, 232)); } // small e, grave accent
      else if (CMP("&eacute;", str)) { return (set_char(str, out conv, 233)); } // small e, acute accent
      else if (CMP("&ecirc;", str)) { return (set_char(str, out conv, 234)); } // small e, circumflex accent
      else if (CMP("&euml;", str)) { return (set_char(str, out conv, 235)); } // small e, dieresis or umlaut mark
      else if (CMP("&igrave;", str)) { return (set_char(str, out conv, 236)); } // small i, grave accent
      else if (CMP("&iacute;", str)) { return (set_char(str, out conv, 237)); } // small i, acute accent
      else if (CMP("&icirc;", str)) { return (set_char(str, out conv, 238)); } // small i, circumflex accent
      else if (CMP("&iuml;", str)) { return (set_char(str, out conv, 239)); } // small i, dieresis or umlaut mark
      else if (CMP("&eth;", str)) { return (set_char(str, out conv, 240)); } // small eth, Icelandic
      else if (CMP("&ntilde;", str)) { return (set_char(str, out conv, 241)); } // small n, tilde
      else if (CMP("&ograve;", str)) { return (set_char(str, out conv, 242)); } // small o, grave accent
      else if (CMP("&oacute;", str)) { return (set_char(str, out conv, 243)); } // small o, acute accent
      else if (CMP("&ocirc;", str)) { return (set_char(str, out conv, 244)); } // small o, circumflex accent
      else if (CMP("&otilde;", str)) { return (set_char(str, out conv, 245)); } // small o, tilde
      else if (CMP("&ouml;", str)) { return (set_char(str, out conv, 246)); } // small o, dieresis or umlaut mark
      else if (CMP("&divide;", str)) { return (set_char(str, out conv, 247)); } // divide sign
      else if (CMP("&oslash;", str)) { return (set_char(str, out conv, 248)); } // small o, slash
      else if (CMP("&ugrave;", str)) { return (set_char(str, out conv, 249)); } // small u, grave accent
      else if (CMP("&uacute;", str)) { return (set_char(str, out conv, 250)); } // small u, acute accent
      else if (CMP("&ucirc;", str)) { return (set_char(str, out conv, 251)); } // small u, circumflex accent
      else if (CMP("&uuml;", str)) { return (set_char(str, out conv, 252)); } // small u, dieresis or umlaut mark
      else if (CMP("&yacute;", str)) { return (set_char(str, out conv, 253)); } // small y, acute accent
      else if (CMP("&thorn;", str)) { return (set_char(str, out conv, 254)); } // small thorn, Icelandic
      else if (CMP("&yuml;", str)) { return (set_char(str, out conv, 255)); } // small y, dieresis or umlaut mark

      conv = "";
      return false;
    }

    private bool microsoft_entities(string str, out string conv)
    {
      int number = extract_entity_number(str);

      if (!convert_characters) { conv = ""; return false; }
      // Euro
      else if (number == 128) { conv = "EUR"; }
      else if (CMP("&euro;", str)) { conv = "EUR"; }
      else if (number == 8364) { conv = "EUR"; }

       // Single Low-9 Quotation Mark
      else if (number == 130) { set_char(str, out conv, ','); }
      else if (CMP("&sbquo;", str)) { set_char(str, out conv, ','); }
      else if (number == 8218) { set_char(str, out conv, ','); }

      else if (number == 131) { set_char(str, out conv, 'f'); } // Latin Small Letter F With Hook
      else if (CMP("&fnof;", str)) { set_char(str, out conv, 'f'); } // Latin Small Letter F With Hook
      else if (number == 402) { set_char(str, out conv, 'f'); } // Latin Small Letter F With Hook

      // Double Low-9 Quotation Mark
      else if (number == 132) { conv = "\""; }
      else if (CMP("&bdquo;", str)) { conv = "\""; }
      else if (number == 8222) { conv = "\""; }

      else if (number == 133) { conv = "..."; } // Horizontal Ellipsis
      else if (CMP("&hellip;", str)) { conv = "..."; } // Horizontal Ellipsis
      else if (number == 8230) { conv = "..."; } // Horizontal Ellipsis

       // Dagger
      else if (number == 134) { conv = "/-"; }
      else if (CMP("&dagger;", str)) { conv = "/-"; }
      else if (number == 8224) { conv = "/-"; }

       // Double Dagger
      else if (number == 135) { conv = "/="; }
      else if (CMP("&Dagger;", str)) { conv = "/="; }
      else if (number == 8225) { conv = "/="; }

       // Modifier Letter Circumflex Accent
      else if (number == 136) { set_char(str, out conv, '^'); }
      else if (CMP("&circ;", str)) { set_char(str, out conv, '^'); }
      else if (number == 710) { set_char(str, out conv, '^'); }

      // Per Mille Sign
      else if (number == 137) { conv = "0/00"; }
      else if (CMP("&permil;", str)) { conv = "0/00"; }
      else if (number == 8240) { conv = "0/00"; }

       // Latin Capital Letter S With Caron
      else if (number == 138) { set_char(str, out conv, 'S'); }
      else if (CMP("&Scaron;", str)) { set_char(str, out conv, 'S'); }
      else if (number == 352) { set_char(str, out conv, 'S'); }

      // Single Left-Pointing Angle Quotation Mark
      else if (number == 139) { set_char(str, out conv, '<'); }
      else if (CMP("&lsaquo;", str)) { set_char(str, out conv, '<'); }
      else if (number == 8249) { set_char(str, out conv, '<'); }

       // Latin Capital Ligature OE
      else if (number == 140) { conv = "OE"; }
      else if (CMP("&OElig;", str)) { conv = "OE"; }
      else if (number == 338) { conv = "OE"; }

      // Z\/
      else if (number == 142) { set_char(str, out conv, 'Z'); }
      else if (number == 381) { set_char(str, out conv, 'Z'); }

      // Left Single Quotation Mark
      else if (number == 145) { set_char(str, out conv, '`'); }
      else if (CMP("&lsquo;", str)) { set_char(str, out conv, '`'); }
      else if (number == 8216) { set_char(str, out conv, '`'); }

       // Right Single Quotation Mark
      else if (number == 146) { set_char(str, out conv, '\''); }
      else if (CMP("&rsquo;", str)) { set_char(str, out conv, '\''); }
      else if (number == 8217) { set_char(str, out conv, '\''); }

       // Left Double Quotation Mark
      else if (number == 147) { set_char(str, out conv, '"'); }
      else if (CMP("&ldquo;", str)) { set_char(str, out conv, '"'); }
      else if (number == 8220) { set_char(str, out conv, '"'); }

       // Right Double Quotation Mark
      else if (number == 148) { set_char(str, out conv, '"'); }
      else if (CMP("&rdquo;", str)) { set_char(str, out conv, '"'); }
      else if (number == 8221) { set_char(str, out conv, '"'); }

       // Bullet
      else if (number == 149) { set_char(str, out conv, '*'); }
      else if (CMP("&bull;", str)) { set_char(str, out conv, '*'); }
      else if (number == 8226) { set_char(str, out conv, '*'); }

       // En Dash
      else if (number == 150) { set_char(str, out conv, '-'); }
      else if (CMP("&ndash;", str)) { set_char(str, out conv, '-'); }
      else if (number == 8211) { set_char(str, out conv, '-'); }

       // Em Dash
      else if (number == 151) { conv = "--"; }
      else if (CMP("&mdash;", str)) { conv = "--"; }
      else if (number == 8212) { conv = "--"; }

       // Small Tilde
      else if (number == 152) { set_char(str, out conv, '~'); }
      else if (CMP("&tilde;", str)) { set_char(str, out conv, '~'); }
      else if (number == 732) { set_char(str, out conv, '~'); }

      // Trade Mark Sign
      else if (number == 153) { conv = "[tm]"; }
      else if (CMP("&trade;", str)) { conv = "[tm]"; }
      else if (number == 8482) { conv = "[tm]"; }

       // Latin Small Letter S With Caron
      else if (number == 154) { set_char(str, out conv, 's'); }
      else if (CMP("&scaron;", str)) { set_char(str, out conv, 's'); }
      else if (number == 353) { set_char(str, out conv, 's'); }

      // Single Right-Pointing Angle Quotation Mark
      else if (number == 155) { set_char(str, out conv, '>'); }
      else if (CMP("&rsaquo;", str)) { set_char(str, out conv, '>'); }
      else if (number == 8250) { set_char(str, out conv, '>'); }

       // Latin Small Ligature OE
      else if (number == 156) { conv = "oe"; }
      else if (CMP("&oelig;", str)) { conv = "oe"; }
      else if (number == 339) { conv = "oe"; }

      // z\/
      else if (number == 158) { set_char(str, out conv, 'z'); }
      else if (number == 382) { set_char(str, out conv, 'z'); }

      // Latin Capital Letter Y With Diaeresis
      else if (number == 159) { set_char(str, out conv, 'Y'); }
      else if (CMP("&Yuml;", str)) { set_char(str, out conv, 'Y'); }
      else if (number == 376) { set_char(str, out conv, 'Y'); }

      else
      {
        conv = "";
        return false;
      }

      return true; // Microsoft entity found
    }

    /* ------------------------------------------------ */

    void microsoft_character(int c)
    {
      switch (c)
      {
        /* Microsoft... */
        case 0x80: /* MICROSOFT EURO */
          word_plus_string("EUR"); break;

        case 0x82: /* SINGLE LOW-9 QUOTATION MARK */
          word_plus_ch(','); break;
        case 0x83: /* Latin Small Letter F With Hook */
          word_plus_ch('f'); break;
        case 0x84: /* Double Low-9 Quotation Mark */
          word_plus_string("\""); break;
        case 0x85: /* HORIZONTAL ELLIPSIS */
          word_plus_string("..."); break;
        case 0x86: /* Dagger */
          word_plus_string("/-"); break;
        case 0x87: /* Double Dagger */
          word_plus_string("/="); break;
        case 0x88: /* Modifier Letter Circumflex Accent */
          word_plus_ch('^'); break;
        case 0x89: /* Per Mille Sign */
          word_plus_string("0/00"); break;
        case 0x8a: /* Latin Capital Letter S With Caron */
          word_plus_ch('S'); break;
        case 0x8b: /*  Single Left-Pointing Angle Quotation Mark */
          word_plus_ch('<'); break;
        case 0x8c: /* Latin Capital Ligature OE */
          word_plus_string("OE"); break;

        case 0x8e: /* Z\/ */
          word_plus_ch('Z'); break;

        case 0x91: /* LEFT SINGLE QUOTATION MARK */
          word_plus_ch('`'); break;
        case 0x92: /* RIGHT SINGLE QUOTATION MARK */
          word_plus_ch('\''); break;
        case 0x93: /* LEFT DOUBLE QUOTATION MARK */
          word_plus_ch('\"'); break;
        case 0x94: /* RIGHT DOUBLE QUOTATION MARK */
          word_plus_ch('\"'); break;
        case 0x95: /* BULLET */
          word_plus_ch('*'); break;
        case 0x96: /* EN DASH */
          word_plus_ch('-'); break;
        case 0x97: /* EM DASH */
          word_plus_string("--"); break;
        case 0x98: /* SMALL TILDE */
          word_plus_ch('~'); break;
        case 0x99: /* TRADE MARK SIGN */
          word_plus_string("[tm]"); break;
        case 0x9a: /* LATIN SMALL LETTER S WITH CARON */
          word_plus_ch('s'); break;
        case 0x9b: /* SINGLE RIGHT-POINTING ANGLE QUOTATION MARK */
          word_plus_ch('>'); break;
        case 0x9c: /* LATIN SMALL LIGATURE OE */
          word_plus_string("oe"); break;

        case 0x9e: /* z\/ */
          word_plus_ch('z'); break;
        case 0x9f: /* LATIN CAPITAL LETTER Y WITH DIAERESIS */
          word_plus_ch('Y'); break;
      }
    } /* end microsoft_character */
    private bool unicode_entity(string str, out string conv)
    {
      conv = "";
      return false;
    }


    int get_align()
    {
      if (align.Count == 0)
        align.Push(1);

      return ((int)align.Peek());
    } // end get_align

    // ------------------------------------------------

    void push_align(int a)
    {
      align.Push(a);
    }

    void pop_align()
    {
      align.Pop();
    }

    // ------------------------------------------------

    void center_line()
    {
      /*
            int i,j;

              if (line_len!=0)
              {
                j=(spreads-line_len)/2;

                for (i=line_pos+j+2; i>=0; i--)
                {
                  line[i+j]=line[i];
                }
                for (i=0; i<j; i++) { line[i]=' '; }
              }

      */
    } // end center_line

    // ------------------------------------------------

    void right_line()
    {
      /*
              int i,j;

              if (line_len!=0)
              {
                j=spreads-line_len;


                for (i=line_pos+j+2; i>=0; i--)
                {

              line[i+j]=line[i];
                }
                for (i=0; i<j; i++) { line[i]=' '; }

              }
      */
    } // end right_line

    // ------------------------------------------------

    // return true, if z is all spaces or nonbreakable space
    bool only_spaces(string z)
    {
      int len = z.Length;
      int i;
      bool ret = true;
      char j;

      for (i = 0; i < len; i++)
      {
        j = z[i];
        ret = (ret && (j == ' ' || j == 160));
      }
      return (ret);
    } // end only_spaces

    // ------------------------------------------------

    void clear_line()
    {
      line = "";
      line_len = 0; line_pos = 0;
    }

    // ------------------------------------------------


    // print line
    void print_line()
    {
      bool printline;


      if ((shrink_lines > 0) && only_spaces(line))
      {
        clear_line();
        nrBlankLines++;
      }
      else
      {
        nrBlankLines = 0;
      }

      // Don't allow leading blank lines.
      // That means the first line of the output is never an empty line
      if (noleadingblanks == 0) { noleadingblanks = (only_spaces(line) ? 0 : 1); }

      if (shrink_lines == 0)
      {
        printline = (!((line_len == 0) && (line_len_old == 0)));
      }
      else
      {
        printline = (!((nrBlankLines > shrink_lines) || (noleadingblanks == 0)));
      }

      // fprintf(stderr, "nrBlankLines %d shrink_lines %d line_len %d line_len_old %d noleadingblanks %d nooutput %d printline %d line %ls \n", nrBlankLines, shrink_lines, line_len, line_len_old, noleadingblanks, nooutput, printline, line);

      // Check whether to remove lines with just links in
      if (!nooutput && removeLinesWithOnlyLinks)
        printline = !doesLineContainOnlyLinks(line);

      if (printline)
      {
        if (get_align() == LEFT) { }
        if (get_align() == CENTER) { center_line(); }
        if (get_align() == RIGHT) { right_line(); }

        if (!nooutput) { output_string(line); }
      }
      line_len_old = line_len;
      clear_line();

    } // end print_line

    // ------------------------------------------------

    string nonchars = "|/*#^%$£\"!()-";
    bool doesLineContainOnlyLinks(string line)
    {
      bool inLink = false;
      bool linksOnly = true;
      int open = 0;
      int close = 0;

      if (line.Trim().Equals(""))
        return false;

      // Check to see if we have a mismatch of []
      foreach (char c in line)
      {
        if (c == '[')
        {
          inLink = true;
          open++;
        }
        else if (c == ']')
        {
          inLink = false;
          close++;
        }
        else if (!inLink)
        {
          if (!Char.IsWhiteSpace(c) && nonchars.IndexOf(c) == -1)
          {
            linksOnly = false;
          }
        }
      }

      // If we had didn't have only links or we had a mismatch of square brackets
      // return false (line doesn't contain links only
      if (linksOnly && (open == close))
        return true;
      else
        return false;
    }

    // ------------------------------------------------

    bool is_line_empty()
    {
      return line.Equals("");
    } // end is_line_empty

    // ------------------------------------------------

    void status()
    {
      //	printf(" paragraph: %d; div_test: %d; align[align_nr]: %d; z_o: %d\n",paragraph, div_test, get_align(), line_len_old);
    }

    // ------------------------------------------------

    void line_plus_word(string s, int wl, int wp)
    {
      int i = line_pos,
          j = 0;

      while (i < line_pos + wp) { line += s[j]; j++; i++; }

      line_len += wl; line_pos += wp;
    } // end line_plus_word

    // ------------------------------------------------

    void word_plus_string_nocount(string s)
    {
      int len = s.Length,
          i = word_pos,
          j = 0;

      while (i < word_pos + len) { word += s[j]; j++; i++; }
      word_pos += len;


    } // end word_plus_string_nocount

    // ------------------------------------------------

    void word_plus_string(string s)
    {
      int len = s.Length,
          i = word_pos,
          j = 0;

      while (i < word_pos + len) { word += s[j]; j++; i++; }
      word_pos += len; word_len += len;


    } // end word_plus_string

    // ------------------------------------------------

    void word_plus_ch(int c)
    {
      word += (char)c;
      word_pos++;
      word_len++;
    } // end word_plus

    // ------------------------------------------------

    void word_end()
    {
      int i = 0;

      if (word_len > 0)
      {
        if (line_len + word_len + 1 > spreads)
        {

          print_line();
          i = 0;
          while (i < spaces) { line_plus_word(" ", 1, 1); i++; }
          if (orderedlist > 0) { line_plus_word(" ", 1, 1); }
          line_plus_word(word, word_len, word_pos);
        }
        else if (line_len != 0)
        {

          // add space + word
          line_plus_word(" ", 1, 1); line_plus_word(word, word_len, word_pos);
        }
        else // line_len==0 => new beginning of a paragraph
        {

          i = 0;
          while (i < spaces) { line_plus_word(" ", 1, 1); i++; }
          if (orderedlist > 0) { line_plus_word(" ", 1, 1); }
          line_plus_word(word, word_len, word_pos);
        }
        word_pos = 0;
        word_len = 0;
        word = "";
      }


    } // end word_end

    // ------------------------------------------------

    void line_break()
    {
      word_end();
      print_line();
    } // end line_break

    // ------------------------------------------------

    void paragraphen_end()
    {
      if (paragraph != 0)
      {

        line_break();
        print_line();
        paragraph--;

        pop_align();
      }

    } // end paragraphen_end

    // ------------------------------------------------

    void neuer_paragraph()
    {

      //fprintf(stderr, "paragraph %d\n", paragraph);
      if (paragraph != 0) { paragraphen_end(); }
      line_break();
      print_line();
      paragraph++;

    } // end neuer_paragraph

    // ------------------------------------------------

    void hr()
    {
      //
      int i, hr_width = hr_spreads - 4, hr_align = CENTER;

      string attr_name;
      string attr_ctnt;
      while (ch != '>')
      {
        ch = get_attr(out attr_name, out attr_ctnt);

        if (CMP("ALIGN", attr_name))
        {
          attr_ctnt = attr_ctnt.ToUpper();
          if (CMP("LEFT", attr_ctnt)) { hr_align = LEFT; }
          else if (CMP("CENTER", attr_ctnt)) { hr_align = CENTER; }
          else if (CMP("RIGHT", attr_ctnt)) { hr_align = RIGHT; }
          else if (CMP("JUSTIFY", attr_ctnt)) { hr_align = LEFT; }
          else { if (errorlevel >= 2) { print_error("Error ", "No LEFT|CENTER|RIGHT found!\n"); } }
        }
        else if (CMP("WIDTH", attr_name))
        {
          i = attr_ctnt.Length;
          if (attr_ctnt[i - 1] == '%')
          {
            attr_ctnt = attr_ctnt.Substring(0, i - 1); ;
            hr_width = int.Parse(attr_ctnt);
            if (hr_width == 100) { hr_width = hr_spreads - 4; }
            else { hr_width = hr_spreads * hr_width / 100; }
          }
          else
          {
            hr_width = int.Parse(attr_ctnt) / 8;
            if (hr_width > hr_spreads - 4) { hr_width = hr_spreads - 4; }
          }
        }
      }

      neuer_paragraph();
      push_align(hr_align);
      for (i = 0; i < hr_width; i++) { word_plus_ch('-'); }
      paragraphen_end();

    } // end hr

    void output_string(string str)
    {
      if (stripMultipleBlankLines)
      {
        str = str.Trim();
        if (str.Equals("\n") || str.Equals("") && text.EndsWith("\n\n\n"))
          return;
      }
      text += str + "\n";
      //		Console.WriteLine( str );
    }

    void start_nooutput()
    {
      word_end();
      print_line();
      nooutput = true;

      string attr_name;
      string attr_ctnt;
      while (ch != '>')
      {
        ch = get_attr(out attr_name, out attr_ctnt);
        // printf("attr_name: %ls\nattr_ctnt: %ls\n", attr_name, attr_ctnt);
        if (CMP("/", attr_name))
        {
          //				printf("Empty tag\n");
          nooutput = false;
        }
      }
    } // end start_nooutput

    void end_nooutput()
    {
      word_end();
      print_line();
      nooutput = false;
    } // end end_nooutput

    /* ------------------------------------------------ */

    void start_p()
    {
      push_align(LEFT);
      neuer_paragraph();
      check_for_center();
    } /* end start_p */

    /* ------------------------------------------------ */

    void href()
    {
      string tmp = "";

      if (!option_links)
        return;

      string attr_name;
      string attr_ctnt;
      while (ch != '>')
      {
        ch = get_attr(out attr_name, out attr_ctnt);

        if (CMP("HREF", attr_name))
        {
          //			if ((STRSTR(attr_ctnt, "://")!=NULL) || (STRNCMP("mailto:", attr_ctnt, 7)==0) || (STRNCMP("news:", attr_ctnt, 5)==0)) { 
          if (attr_ctnt.Length > 5 &&
              (attr_ctnt.IndexOf("://") != -1 ||
              attr_ctnt.Substring(0, 7).Equals("mailto:") ||
              attr_ctnt.Substring(0, 5).Equals("news:")))
          {
            references_count++;

            parse_entities(ref attr_ctnt);
            print_footnote_number(ref tmp, references_count);
            word_plus_string(tmp);

            construct_footnote(ref tmp, references_count, attr_ctnt);
            references += tmp;
          }
        }
      }
    } /* end href */



    /* ------------------------------------------------ */
    /*
        int check_style()
        {
          while (ch!='>')
          {
            ch=get_attr();
            if CMP("TYPE", attr_name)
            {
              if CMP("disc", attr_ctnt)   { return '*'; }
              if CMP("square", attr_ctnt) { return '+'; }
              if CMP("circle", attr_ctnt) { return 'o'; }
            }
          }
          return 0;
        }  //end check_style
    */
    /* ------------------------------------------------ */

    void start_uls()
    {
      /*int ret; */
      line_break();

      push_align(LEFT);

      /* * o + # @ - = ~ $ % */
      if (bullet_style == ' ') { bullet_style = '*'; }
      else if (bullet_style == '*') { bullet_style = 'o'; }
      else if (bullet_style == 'o') { bullet_style = '+'; }
      else if (bullet_style == '+') { bullet_style = '#'; }
      else if (bullet_style == '#') { bullet_style = '@'; }
      else if (bullet_style == '@') { bullet_style = '-'; }
      else if (bullet_style == '-') { bullet_style = '='; }
      else if (bullet_style == '=') { bullet_style = '~'; }
      else if (bullet_style == '~') { bullet_style = '$'; }
      else if (bullet_style == '$') { bullet_style = '%'; }

      spaces += tab;
    } /* end start_uls */

    void end_uls()
    {
      spaces -= tab;
      line_break();

      if (bullet_style == '%') { bullet_style = '$'; }
      else if (bullet_style == '$') { bullet_style = '~'; }
      else if (bullet_style == '~') { bullet_style = '='; }
      else if (bullet_style == '=') { bullet_style = '-'; }
      else if (bullet_style == '-') { bullet_style = '@'; }

      else if (bullet_style == '@') { bullet_style = '#'; }
      else if (bullet_style == '#') { bullet_style = '+'; }
      else if (bullet_style == '+') { bullet_style = 'o'; }
      else if (bullet_style == 'o') { bullet_style = '*'; }
      else if (bullet_style == '*') { bullet_style = ' '; }

      pop_align();
    } /* end end_uls */

    /* ------------------------------------------------ */

    void start_ols()
    {
      start_uls();
    } /* end start_ols */

    /* ------------------------------------------------ */

    void end_ols()
    {
      end_uls();
    } /* end end_ols */

    /* ------------------------------------------------ */

    void start_lis()
    {
      spaces -= 2;

      /* don't output line break, if this list item is immediately
      after a start or end list tag. start_uls and end_uls have
      already take care of the line break */
      if (!is_line_empty())
        line_break();

      word_plus_ch(bullet_style);

      word_end();
      spaces += 2;
    } /* end start_lis */

    /* ------------------------------------------------ */

    void end_lis()
    {
    }

    /* ------------------------------------------------ */

    /* Definition List */
    void start_dl()
    {
      end_dd();
      start_p();
    } /* end start_dl */

    void end_dl()
    {
      paragraphen_ende();

      end_dd();
    } /* end_dl */

    /* Definition Title */
    void start_dt()
    {
      end_dd();

      line_break();
    } /* end start_dt */

    void end_dt()
    {
    } /* end_dt */

    /* Definition Description */
    void start_dd()
    {
      end_dd();

      line_break();
      spaces += tab;

      definition_list = 1;
    } /* end  */

    void end_dd()
    {
      if (definition_list == 1)
      {
        spaces -= tab;
        definition_list = 0;
      }
    } /* end_dd */


    /* ------------------------------------------------ */

    void start_div(int a)
    {
      line_break();

      if (a != 0)
      {
        div_test = a;
        push_align(div_test);
      }
      else
      {
        check_for_center();
      }
    } /* end start_div */

    /* ------------------------------------------------ */

    void end_div()
    {
      word_end();

      if (paragraph != 0)
        paragraphen_ende();
      else
        print_line();

      pop_align(); /* einer für start_div */
      div_test = 0;
    } /* end_div */

    /* ------------------------------------------------ */

    /* find alt attribute in current tag */
    void image(string alt_text, int show_alt)
    {
      int found_alt = 0;
      string attr_name;
      string attr_ctnt;
      while (ch != '>')
      {
        ch = get_attr(out attr_name, out attr_ctnt);
        if (CMP("ALT", attr_name))
        {
          if (!(remove_empty_alt && CMP("", attr_ctnt)))
          {
            if (!option_no_alt)
            {
              word_plus_ch('[');
              word_plus_string(attr_ctnt);
              word_plus_ch(']');
            }
          }
          found_alt = 1;
        }
      }
      if ((found_alt == 0) && (show_alt != 0))
      {
        if (!option_no_image)
        {
          word_plus_ch('[');
          word_plus_string(alt_text);
          word_plus_ch(']');
        }
      }
    } /* end image */

    /* simple finite state machine to eat up complete comment '!--' */
    char kill_comment()
    {
      char c = ' ';
      bool dontquit = true;
      while (dontquit)
      {
        c = (char)nextChar();
        if (c == '-')
        {
          c = (char)nextChar();
          while (c == '-')
          {
            c = (char)nextChar();
            if (c == '>')
              dontquit = false;
          }
        }
      }

      return c;
    } /* end kill_comment */

    void print_error(string error, string text)
    {
      Console.WriteLine("{0}{0}", error, text);
    }

    /* ------------------------------------------------ */

    /* used when there's only the align-attribut to be checked  */
    void check_for_center()
    {
      int found = 0;
      string attr_name;
      string attr_ctnt;
      while (ch != '>')
      {
        ch = get_attr(out attr_name, out attr_ctnt);
        if (CMP("ALIGN", attr_name))
        {
          found = 1;
          attr_ctnt = attr_ctnt.ToUpper();
          if (CMP("LEFT", attr_ctnt)) { push_align(LEFT); }
          else if (CMP("CENTER", attr_ctnt)) { push_align(CENTER); }
          else if (CMP("RIGHT", attr_ctnt)) { push_align(RIGHT); }
          else if (CMP("JUSTIFY", attr_ctnt)) { push_align(LEFT); }
          else { if (errorlevel >= 2) { print_error("error", "No LEFT|CENTER|RIGHT found!\n"); push_align(LEFT); } }
        }
      }
      /* found no ALIGN  */
      if (found == 0)
        push_align(LEFT);

    } /* end check_for_center */

    void print_footnote_number(ref string temp, int number)
    {
      temp = String.Format("[{0}]", number);
    }

    void construct_footnote(ref string temp, int number, string link)
    {
      temp = String.Format("  {0} {1}\n", number, link);
    } /* end construct_footnote */

    void paragraphen_ende()
    {
      if (paragraph != 0)
      {
        line_break();
        print_line();
        paragraph--;

        pop_align();
      }
    } /* end paragraphen_ende */
  }
}