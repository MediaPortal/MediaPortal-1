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
