/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.GUIScript
{
	public class ExpressionEval
	{

		internal class BinaryOp
		{
			private string  _strOp;
			private int     _nPrecedence;

			public string Op            { get { return _strOp; } } 
			public int    Precedence    { get { return _nPrecedence; } }

			public BinaryOp(string strOp)
			{ _strOp = strOp; _nPrecedence = ExpressionEval.OperatorPrecedence(strOp); }

			public override string ToString()
			{ return Op; }
		}

		internal class BinaryOpQueue
		{
			private ArrayList _oplist       = new ArrayList();

			public BinaryOpQueue(ArrayList expressionlist)
			{
				foreach (object item in expressionlist)
					if (item is BinaryOp)
						Enqueue((BinaryOp)item);
			}

			public void Enqueue(BinaryOp op)
			{
				bool bQueued = false;
				for (int x = 0; x < _oplist.Count && !bQueued; x++)
				{
					if (((BinaryOp)_oplist[x]).Precedence > op.Precedence)
					{
						_oplist.Insert(x, op);
						bQueued = true;
					}
				}
				if (!bQueued)
					_oplist.Add(op);
			}

			public BinaryOp Dequeue()
			{
				if (_oplist.Count == 0)
					return null;
				BinaryOp ret = (BinaryOp)_oplist[0];
				_oplist.RemoveAt(0);
				return ret;
			}
		}

		internal class UnaryOp
		{
			private string  _strOp;

			public string Op            { get { return _strOp; } } 

			public UnaryOp(string strOp)
			{ _strOp = strOp; }

			public override string ToString()
			{ return Op; }

		}
       
		ArrayList       _expressionlist = new ArrayList();
		string          _strExpression  = "";
		bool            _bParsed        = false;

		public ExpressionEval() {}

		/// <summary>
		/// Constructor with string
		/// </summary>
		public ExpressionEval(string strExpression)
		{ Expression = strExpression; }

		/// <summary>
		/// Gets or sets the expression to be evaluated.
		/// </summary>
		public string Expression
		{
			get { return _strExpression; } 
			set 
			{ 
				_strExpression = value.Trim(); 
				_bParsed = false;
				_expressionlist.Clear();
			} 
		}
       
		/// <summary>
		/// Evaluates the expression
		/// </summary>
		public object Evaluate()
		{
			if (Expression == null || Expression == "")
				return 0;

			return ExecuteEvaluation();
		}

		public bool EvaluateBool()
		{ return Convert.ToBoolean(Evaluate()); }

		public int EvaluateInt()
		{ return Convert.ToInt32(Evaluate()); }

		public double EvaluateDouble()
		{ return Convert.ToDouble(Evaluate()); }

		public long EvaluateLong()
		{ return Convert.ToInt64(Evaluate()); }

		public static object Evaluate(string strExpression)
		{
			ExpressionEval expr = new ExpressionEval(strExpression);
			return expr.Evaluate();
		}

		public static object Evaluate(string strExpression, FunctionHandler handler)
		{
			ExpressionEval expr = new ExpressionEval(strExpression);
			expr.FunctionHandler += handler;
			return expr.Evaluate();
		}

		private object ExecuteEvaluation()
		{
			//Break Expression Apart into List
			if (!_bParsed)
				for (int x = 0; x < Expression.Length; x = NextToken(x));
			_bParsed = true;

			//Perform Operations
			return EvaluateList();
		}

		private int NextToken(int nIdx)
		{
			Match   mRet = null;
			int     nRet = nIdx + 1;
			object  val  = null;
			Match m = DefinedRegex.Parenthesis.Match(Expression, nIdx);
			if (m.Success)
			{ mRet = m; }

			m = DefinedRegex.Function.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{ mRet = m; }

			m = DefinedRegex.UnaryOp.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{ mRet = m; val = new UnaryOp(m.Value); }

			m = DefinedRegex.Hexadecimal.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{ mRet = m; val = Convert.ToInt32(m.Value, 16); }

			m = DefinedRegex.Boolean.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{ mRet = m; val = bool.Parse(m.Value); }

			m = DefinedRegex.Numeric.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{
				while (m.Success && m.Value == "")
					m = m.NextMatch();
				if (m.Success)
				{
					mRet = m;
					val = double.Parse(m.Value);
				}
			}

			m = DefinedRegex.String.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{ mRet = m; val = m.Groups["String"].Value.Replace("\\\"", "\""); }

			m = DefinedRegex.BinaryOp.Match(Expression, nIdx);
			if (m.Success && (mRet == null || m.Index < mRet.Index))
			{ mRet = m; val = new BinaryOp(m.Value); }

			if (mRet.Value == "(" || mRet.Value.StartsWith("$"))
			{
				nRet = (mRet.Value == "(") ? nRet+1 : mRet.Index + mRet.Length;
				int nDepth = 1;
				bool bInQuotes = false;
				while (nDepth > 0)
				{
					if (nRet >= Expression.Length)
						throw new Exception("Missing " + (bInQuotes ? "\"" : ")") + " in Expression");
					if (!bInQuotes && Expression[nRet] == ')')
						nDepth--;
					if (!bInQuotes && Expression[nRet] == '(')
						nDepth++;
                    
					if (Expression[nRet] == '"' && (nRet == 0 || Expression[nRet - 1] != '\\'))
						bInQuotes = !bInQuotes;
                    
					nRet++;
				}
				if (mRet.Value == "(")
				{
					ExpressionEval expr = new ExpressionEval(
						Expression.Substring(mRet.Index + 1, nRet - mRet.Index - 2)
						);
					if (this.FunctionHandler != null)
						expr.FunctionHandler += this.FunctionHandler;
					_expressionlist.Add(expr);
				}
				else
				{
					FunctionEval func = new FunctionEval(
						Expression.Substring(mRet.Index, (nRet) - mRet.Index)
						);
					if (this.FunctionHandler != null)
						func.FunctionHandler += this.FunctionHandler;
					_expressionlist.Add(func);
				}
			}
			else
			{
				nRet = mRet.Index + mRet.Length;
				_expressionlist.Add(val);
			}

			return nRet;
		}

		private object EvaluateList()
		{
			ArrayList list = (ArrayList)_expressionlist.Clone();

			//Do the unary operators first
			for (int x = 0; x < list.Count; x++)
			{
				if (list[x] is UnaryOp)
				{
					list[x] = PerformUnaryOp(
						(UnaryOp)list[x],
						list[x + 1]
						);
					list.RemoveAt(x + 1);
				}
			}

			//Do the queued binary operations
			BinaryOpQueue opqueue = new BinaryOpQueue(list);
			BinaryOp op = opqueue.Dequeue();
			while (op != null)
			{
				int nIdx = list.IndexOf(op);
				list[nIdx - 1] = PerformBinaryOp(
					(BinaryOp)list[nIdx],
					list[nIdx - 1],
					list[nIdx + 1]
					);
				list.RemoveAt(nIdx);
				list.RemoveAt(nIdx);
				op = opqueue.Dequeue();
			}

			object ret = null;
			if (list[0] is ExpressionEval)
				ret = ((ExpressionEval)list[0]).Evaluate();
			else if (list[0] is FunctionEval)
				ret = ((FunctionEval)list[0]).Evaluate();
			else 
				ret = list[0];

			return ret;
		}

		private static int OperatorPrecedence(string strOp)
		{
			switch (strOp)
			{
				case "*":
				case "/":
				case "%":  return 0;
				case "+":
				case "-":  return 1;
				case "<":
				case "<=":
				case ">":
				case ">=": return 2;
				case "==":
				case "!=": return 3;
				case "&":  return 4;
				case "^":  return 5;
				case "|":  return 6;
				case "&&": return 7;
				case "||": return 8;
			}
			throw new Exception("Operator " + strOp + "not defined.");
		}

		private static object PerformBinaryOp(BinaryOp op, object v1, object v2)
		{
			if (v1 is ExpressionEval)
				v1 = ((ExpressionEval)v1).Evaluate();
			else if (v1 is FunctionEval)
				v1 = ((FunctionEval)v1).Evaluate();
			if (v2 is ExpressionEval)
				v2 = ((ExpressionEval)v2).Evaluate();
			else if (v2 is FunctionEval)
				v2 = ((FunctionEval)v2).Evaluate();

			switch (op.Op)
			{
				case "*":  return (Convert.ToDouble(v1) *  Convert.ToDouble(v2));
				case "/":  return (Convert.ToDouble(v1) /  Convert.ToDouble(v2));
				case "%":  return (Convert.ToInt64(v1)  %  Convert.ToInt64(v2));
				case "+":  
				case "-":  
				case "<":  
				case "<=": 
				case ">":  
				case ">=": 
				case "==": 
				case "!=": return DoSpecialOperator(op, v1, v2);
				case "&":  return (Convert.ToUInt64(v1)  &  Convert.ToUInt64(v2));
				case "^":  return (Convert.ToUInt64(v1)  ^  Convert.ToUInt64(v2));
				case "|":  return (Convert.ToUInt64(v1)  |  Convert.ToUInt64(v2));
				case "&&": return (Convert.ToBoolean(v1) && Convert.ToBoolean(v2));
				case "||": return (Convert.ToBoolean(v1) || Convert.ToBoolean(v2));
			}
			throw new Exception("Binary Operator " + op.Op + "not defined.");
		}

		private static object DoSpecialOperator(BinaryOp op, object v1, object v2)
		{
			if (v1 is string || v2 is string)
			{
				string  str1 = "" + v1, 
					str2 = "" + v2;

				switch (op.Op)
				{
					case "+":   return str1 + str2;
					case "-":   throw new Exception("operator '-' invalid for strings");
					case "<":   return str1.CompareTo(str2) < 0;
					case "<=":  return str1.CompareTo(str2) < 0 || str1 == str2;
					case ">":   return str1.CompareTo(str2) > 0;
					case ">=":  return str1.CompareTo(str2) > 0 || str1 == str2;;
					case "==":  return str1 == str2;
					case "!=":  return str1 != str2;
				}
			}
			if (v1 is DateTime || v2 is DateTime)
			{
				DateTime d1 = (DateTime)v1, d2 = Convert.ToDateTime(v2);
				switch (op.Op)
				{
					case "+":   throw new Exception("operator '+' invalid for dates");
					case "-":   return d1 -  d2;
					case "<":   return d1 <  d2; 
					case "<=":  return d1 <= d2;
					case ">":   return d1 >  d2;
					case ">=":  return d1 >= d2;
					case "==":  return d1 == d2;
					case "!=":  return d1 != d2;
				}
			}

			double f1 = Convert.ToDouble(v1), f2 = Convert.ToDouble(v2);
			switch (op.Op)
			{
				case "+":   return f1 +  f2;
				case "-":   return f1 -  f2;
				case "<":   return f1 <  f2;
				case "<=":  return f1 <= f2;
				case ">":   return f1 >  f2;
				case ">=":  return f1 >= f2;
				case "==":  return f1 == f2;
				case "!=":  return f1 != f2;
			}

			throw new Exception("operator '" + op.Op + "' not specified");
		}

		private static object PerformUnaryOp(UnaryOp op, object v)
		{
			if (v is ExpressionEval)
				v = ((ExpressionEval)v).Evaluate();
			else if (v is FunctionEval)
				v = ((FunctionEval)v).Evaluate();
            
			switch (op.Op)
			{
				case "+": return (Convert.ToDouble(v));
				case "-": return (-Convert.ToDouble(v));
			}
			throw new Exception("Unary Operator " + op.Op + "not defined.");
		}

		public event FunctionHandler FunctionHandler;

	}
}
