/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

// Code based on source from "The Code Project"
// by railerb 

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.GUIScript
{
	internal class DefinedRegex
	{
		private const string c_strNumeric       = @"(?:[0-9]+)?(?:\.[0-9]+)?(?:E-?[0-9]+)?(?=\b)";
		private const string c_strHex           = @"0x([0-9a-fA-F]+)";
		private const string c_strUnaryOp       = @"(?:\+|-|!|~)(?=\w|\()";
		private const string c_strBinaryOp      = @"\+|-|\*|/|%|&&|\|\||&|\||\^|==|!=|>=|<=|=|<|>";
		private const string c_strBool          = @"true|false";
		private const string c_strFunction      = @"\$(?<Function>\w+)\(";
		private const string c_strString        = "\\\"(?<String>.*?[^\\\\])\\\"";

		internal static Regex Numeric = new Regex(
			c_strNumeric,
			RegexOptions.Compiled
			);

		internal static Regex Hexadecimal = new Regex(
			c_strHex,
			RegexOptions.Compiled
			);

		internal static Regex Boolean = new Regex(
			c_strBool,
			RegexOptions.Compiled | RegexOptions.IgnoreCase
			);

		internal static Regex UnaryOp = new Regex(
			@"(?<=(?:" + c_strBinaryOp + @")\s*|\A)(?:" + c_strUnaryOp + @")",
			RegexOptions.Compiled
			);

		internal static Regex BinaryOp = new Regex(
			@"(?<!(?:" + c_strBinaryOp + @")\s*|^\A)(?:" + c_strBinaryOp + @")",
			RegexOptions.Compiled
			);

		internal static Regex Parenthesis = new Regex(
			@"\(",
			RegexOptions.Compiled
			);

		internal static Regex Function = new Regex(
			c_strFunction,
			RegexOptions.Compiled
			);

		internal static Regex String = new Regex(
			c_strString,
			RegexOptions.Compiled
			);

	}
}
