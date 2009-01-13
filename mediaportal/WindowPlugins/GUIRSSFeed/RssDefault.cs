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

/* RssDefault.cs
 * =============
 * 
 * RSS.NET (http://rss-net.sf.net/)
 * Copyright © 2002, 2003 George Tsiokos. All Rights Reserved.
 * 
 * RSS 2.0 (http://blogs.law.harvard.edu/tech/rss)
 * RSS 2.0 is offered by the Berkman Center for Internet & Society at 
 * Harvard Law School under the terms of the Attribution/Share Alike 
 * Creative Commons license.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining 
 * a copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
*/
using System;

namespace Rss
{
  /// <summary>Contains default values and methods for maintaining data consistency</summary>
  [@Serializable()]
  public class RssDefault
  {
    /// <summary>Default value for a string in all RSS classes</summary>
    /// <value>empty string</value>
    /// <remarks>If an element in the RSS class library has the value of RssDefault.String, consider the element as "not entered", "null", or empty.</remarks>
    public const string String = "";

    /// <summary>Default value for an int in all RSS classes</summary>
    /// <value>-1</value>
    /// <remarks>If an element in the RSS class library has the value of RssDefault.Int, consider the element as "not entered", "null", or empty.</remarks>
    public const int Int = -1;

    /// <summary>Default value for a DateTime in all RSS classes</summary>
    /// <value>DateTime.MinValue</value>
    /// <remarks>If an element in the RSS class library has the value of RssDefault.DateTime, consider the element as "not entered", "null", or empty.</remarks>
    public static readonly DateTime DateTime = DateTime.MinValue;

    /// <summary>Default value for a Uri in all RSS classes</summary>
    /// <value>gopher://rss-net.sf.net</value>
    /// <remarks>If an element in the RSS class library has the value of RssDefault.Uri, consider the element as "not entered", "null", or empty.</remarks>
    public static readonly Uri Uri = new Uri("gopher://rss-net.sf.net");

    /// <summary>Verifies the string passed is not null</summary>
    /// <param name="input">string to verify</param>
    /// <returns>RssDefault.String if input is null, otherwise input</returns>
    /// <remarks>Method is used in properties to prevent a null value</remarks>
    public static string Check(string input)
    {
      return input == null ? String : input;
    }

    /// <summary>Verifies the int passed is greater than or equal to -1</summary>
    /// <param name="input">int to verify</param>
    /// <returns>RssDefault.Int if int is less than -1, else input</returns>
    /// <remarks>Method is used in properties to prevent values less than -1</remarks>
    public static int Check(int input)
    {
      return input < -1 ? Int : input;
    }

    /// <summary>Verifies the Uri passed is not null</summary>
    /// <param name="input">Uri to verify</param>
    /// <returns>RssDefault.Uri if input is null, otherwise input</returns>
    /// <remarks>Method is used in all properties to prevent a null value</remarks>
    public static Uri Check(Uri input)
    {
      return input == null ? Uri : input;
    }
  }
}