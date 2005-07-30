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

// Code based on source from "The Code Project"
// by railerb 

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.GUIScript
{

	public delegate object FunctionHandler(string strName, object[] a_params);
	public class FunctionEval
	{
		private string      _strExpression = "";
		private string      _strFunc       = "";
		private bool        _bParsed       = false;
		private object []   _params        = null;

		public string Expression 
		{ 
			get { return _strExpression; } 
			set 
			{ 
				_strExpression = value.Trim();
				_strFunc = "";
				_bParsed = false;
				_params = null;
			} 
		}

		public FunctionEval() {}

		public FunctionEval(string strExpression)
		{ Expression = strExpression; }

		public object Evaluate()
		{
			object ret = null;
			if (!_bParsed)
			{
				StringBuilder   strbRet = new StringBuilder(Expression);
				string          strNext = strbRet.ToString();
				Match           m       = DefinedRegex.Function.Match(strNext);

				if (m.Success)
				{
					_params = GetParameters(m);
					_strFunc = m.Groups["Function"].Value;
				}
				_bParsed = true;
			}
			ret = ExecuteFunction(_strFunc, _params);
			return ret;
		}

		/// <summary>
		/// Evaluates a string expression of a function
		/// </summary>
		public static object Evaluate(string strExpression)
		{
			FunctionEval expr = new FunctionEval(strExpression);
			return expr.Evaluate();
		}


		public static string Replace(string strInput, FunctionHandler handler)
		{
			FunctionEval expr = new FunctionEval(strInput);
			if (handler != null)
				expr.FunctionHandler += handler;
			return expr.Replace();
		}

		public static object Evaluate(string strExpression, FunctionHandler handler)
		{
			FunctionEval expr = new FunctionEval(strExpression);
			if (handler != null)
				expr.FunctionHandler += handler;
			return expr.Evaluate();
		}

		public static string Replace(string strInput)
		{
			FunctionEval expr = new FunctionEval(strInput);
			return expr.Replace();
		}

		public string Replace()
		{
			StringBuilder strbRet = new StringBuilder(Expression);
			Match m = DefinedRegex.Function.Match(Expression);

			while (m.Success)
			{
				int nDepth = 1;
				int nIdx   = m.Index + m.Length;
				//Get the parameter string
				while (nDepth > 0)
				{
					if (nIdx >= strbRet.Length)
						throw new Exception("Missing ')' in Expression");
					if (strbRet[nIdx] == ')')
						nDepth--;
					if (strbRet[nIdx] == '(')
						nDepth++;
					nIdx++;
				}
				string strExpression = strbRet.ToString(m.Index, nIdx - m.Index);
				strbRet.Replace(strExpression, "" + Evaluate(strExpression, FunctionHandler));
				m = DefinedRegex.Function.Match(strbRet.ToString());
			}
			return strbRet.ToString();
		}

		private object [] GetParameters(Match m)
		{
			string  strParams   = "";
			int     nIdx        = m.Index + m.Length;
			int     nDepth      = 1;
			int     nLast       = 0;
			bool    bInQuotes   = false;
			ArrayList ret       = new ArrayList();

			//Get the parameter string
			while (nDepth > 0)
			{
				if (nIdx >= Expression.Length)
					throw new Exception("Missing ')' in Expression");
                
				if (!bInQuotes && Expression[nIdx] == ')')
					nDepth--;
				if (!bInQuotes && Expression[nIdx] == '(')
					nDepth++;

				if (Expression[nIdx] == '"' && (nIdx == 0 || Expression[nIdx-1] != '\\'))
					bInQuotes = !bInQuotes;

				if (nDepth > 0)
					nIdx++;
			}
			strParams = Expression.Substring(m.Index + m.Length, nIdx - (m.Index + m.Length));

			if (strParams == "")
				return null;
            
			bInQuotes = false;
			for (nIdx = 0; nIdx < strParams.Length; nIdx++)
			{
				if (!bInQuotes && strParams[nIdx] == ')')
					nDepth--;
				if (!bInQuotes && strParams[nIdx] == '(')
					nDepth++;

				if (strParams[nIdx] == '"' && (nIdx == 0 || strParams[nIdx - 1] != '\\'))
					bInQuotes = !bInQuotes;

				if (!bInQuotes && nDepth == 0 && strParams[nIdx] == ',')
				{
					ret.Add(strParams.Substring(nLast, nIdx - nLast));
					nIdx++;
					nLast = nIdx;
				}
			}
			ret.Add(strParams.Substring(nLast, nIdx - nLast));

			for (nIdx = 0; nIdx < ret.Count; nIdx++)
				try     { ret[nIdx] = new ExpressionEval(ret[nIdx].ToString()); } 
				catch   { ret[nIdx] = ((string)ret[nIdx]).Trim(); }
            
			return ret.ToArray();
		}

		/// <summary>
		/// executes functions
		/// </summary>
		private object ExecuteFunction(string strName, object[] p)
		{
			object[] a_params = null;
			if (p != null)
			{
				a_params = (object[])p.Clone();
				for (int x = 0; x < a_params.Length; x++)
					if (a_params[x] is ExpressionEval)
						a_params[x] = ((ExpressionEval)a_params[x]).Evaluate();
			}
			switch (strName.ToLower())
			{
					// Math functions
				case "sin":     return Math.Sin(Convert.ToDouble(a_params[0]));
				case "cos":     return Math.Cos(Convert.ToDouble(a_params[0]));
				case "tan":     return Math.Tan(Convert.ToDouble(a_params[0]));
				case "asin":    return Math.Asin(Convert.ToDouble(a_params[0]));
				case "acos":    return Math.Acos(Convert.ToDouble(a_params[0]));
				case "atan":    return Math.Atan(Convert.ToDouble(a_params[0]));
				case "sinh":    return Math.Sinh(Convert.ToDouble(a_params[0]));
				case "cosh":    return Math.Cosh(Convert.ToDouble(a_params[0]));
				case "tanh":    return Math.Tanh(Convert.ToDouble(a_params[0]));
				case "abs":     return Math.Abs(Convert.ToDouble(a_params[0]));
				case "sqrt":    return Math.Sqrt(Convert.ToDouble(a_params[0]));
				case "log":     return (a_params.Length > 1) ? 
												Math.Log(Convert.ToDouble(a_params[0]), Convert.ToDouble(a_params[1])) :
												Math.Log(Convert.ToDouble(a_params[0]));
				case "log10":   return Math.Log10(Convert.ToDouble(a_params[0]));
				case "ciel":    return Math.Ceiling(Convert.ToDouble(a_params[0]));
				case "floor":   return Math.Floor(Convert.ToDouble(a_params[0]));
				case "exp":     return Math.Exp(Convert.ToDouble(a_params[0]));
				case "max":     return Math.Max(Convert.ToDouble(a_params[0]), Convert.ToDouble(a_params[1]));
				case "min":     return Math.Min(Convert.ToDouble(a_params[0]), Convert.ToDouble(a_params[1]));
				case "pow":     return Math.Pow(Convert.ToDouble(a_params[0]), Convert.ToDouble(a_params[1]));
				case "round":   return (a_params.Length > 1) ?
												Math.Round(Convert.ToDouble(a_params[0]), Convert.ToInt32(a_params[1])) :
												Math.Round(Convert.ToDouble(a_params[0]));
				case "rnd":     Random r = new Random();
												return r.Next();
 				case "random":  r = new Random();
												return r.Next(Convert.ToInt32(a_params[0]), Convert.ToInt32(a_params[1]));
				case "now":     return DateTime.Now;
				case "today":   return DateTime.Today;
				case "e":       return Math.E;
				case "pi":      return Math.PI;
				case "calc":    return ExpressionEval.Evaluate("" + a_params[0]);
				default:        return FunctionHelper(strName, a_params);
			}
		}

		protected object FunctionHelper(string strName, object[] a_params)
		{
			if (FunctionHandler != null)
				return FunctionHandler(strName, a_params);
			return null;
		}

		public event FunctionHandler FunctionHandler;
	}
}
