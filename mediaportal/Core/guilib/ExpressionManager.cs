#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MediaPortal.GUI.Library
{
  public class GUIExpressionManager
  {
    #region enums

    // A bitmap of available options for ExpressionManager.
    public enum ExpressionOptions
    {
      NONE = 0,
      EVALUATE_ALWAYS = 1   // Evaluate the expression always (even if it's valid)
    }

    #endregion

    #region classes

    protected class FunctionDefinition
    {
      private readonly string _name;
      private List<MethodInfo> _methods;

      public FunctionDefinition(string name)
      {
        _name = name;
        _methods = new List<MethodInfo>();
      }

      public string Name
      {
        get { return _name; }
      }

      public object Invoke(object[] parameters)
      {
        object state;
        Binder defaultBinder = Type.DefaultBinder;
        MethodInfo selectedMethod =
          (MethodInfo)
          defaultBinder.BindToMethod(BindingFlags.Static | BindingFlags.Public | BindingFlags.OptionalParamBinding,
                                     _methods.ToArray(), ref parameters, null,
                                     null, null, out state);
        if (selectedMethod == null)
        {
          throw new MissingMethodException("", _name);
        }
        return selectedMethod.Invoke(null, BindingFlags.Default, defaultBinder, parameters, null);
      }

      public void AddMethod(MethodInfo method)
      {
        _methods.Add(method);
      }
    }

    protected abstract class Expression
    {
      protected List<Expression> DependantExpressions = new List<Expression>();
      protected bool IsValid;

      public void AddDependency(Expression dependantExpression)
      {
        DependantExpressions.Add(dependantExpression);
      }

      public void RemoveDependency(Expression dependantExpression)
      {
        if (DependantExpressions.Contains(dependantExpression))
        {
          DependantExpressions.Remove(dependantExpression);
        }
      }

      public abstract object Evaluate(ExpressionOptions options);

      public virtual void Invalidate()
      {
        if (!IsValid)
        {
          return;
        }

        IsValid = false;
        foreach (Expression exp in DependantExpressions)
        {
          exp.Invalidate();
        }
      }
    }

    protected class LiteralExpression : Expression
    {
      private readonly object _value;

      public LiteralExpression(object value)
      {
        _value = value;
      }

      public override object Evaluate(ExpressionOptions options)
      {
        // Options not used for literal expressions.
        return _value;
      }
    }

    protected class PropertyExpression : Expression
    {
      private readonly string _propertyName;
      private string _value;

      public PropertyExpression(string propertyName)
      {
        _propertyName = propertyName;
        _value = string.Empty;
      }

      public override object Evaluate(ExpressionOptions options)
      {
        // Evaluate if required or if the expression is invalid
        if ((options & ExpressionOptions.EVALUATE_ALWAYS) > 0 || !IsValid)
        {
          _value = GUIPropertyManager.GetProperty(_propertyName);
          IsValid = true;
        }
        return _value;
      }
    }

    protected class FunctionExpression : Expression
    {
      private readonly FunctionDefinition _func;
      private readonly Expression[] _parameters;
      private object _value;

      public FunctionExpression(FunctionDefinition func, Expression[] parameters)
      {
        _func = func;
        _parameters = parameters;
        foreach (Expression param in _parameters)
        {
          param.AddDependency(this);
        }
      }

      public override object Evaluate(ExpressionOptions options)
      {
        // Evaluate if required or if the expression is invalid
        if ((options & ExpressionOptions.EVALUATE_ALWAYS) > 0 || !IsValid)
        {
          int paramCount = _parameters.Length;
          object[] paramValues = new object[paramCount];
          for (int i = 0; i < paramCount; i++)
          {
            paramValues[i] = _parameters[i].Evaluate(options);
          }

          // We may have to convert the parameters to the requested type.
          if ("iif".Equals(_func.Name))
          {
            // The first parameter must be a boolean; attempt coersion if it is a string.
            if (paramValues[0].GetType() != typeof(bool))
            {
              try
              {
                paramValues[0] = bool.Parse((string)paramValues[0]);
              }
              catch (Exception)
              {
                Log.Debug("Condition for iff() function is not a boolean; param={0}, value={1}", _parameters[0], paramValues[0]);
              }
            }
          }

          _value = _func.Invoke(paramValues);
          IsValid = true;
        }
        return _value;
      }
    }

    #endregion

    #region member variables

    //private static Regex _funcExpr = new Regex(@"#[a-z0-9\._]+\(([^()']+|\'([^']|\\\')*\'|\((?<p>)|\)(?<-p>))*(?(p)(?!))\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    //private static Regex _funcExpr = new Regex(@"#(?<function>[a-z0-9\._]+)" +    // function name
    //                                            @"\(" +                           // opening parenthesis
    //                                              @"([^()']+" +           // anything but a parenthesis or quote
    //                                               @"|\'([^']|\\\')*\'" + // a quoted string
    //                                               @"|\((?<p>)" +         // opening parenthesis - increment depth
    //                                               @"|\)(?<-p>)" +        // closing parenthesis - decrement depth
    //                                              @")*" +                 // a sequence of any number of any of the above
    //                                              @"(?(p)(?!))" +         // require depth to be 0
    //                                            @"\)",                    // final closing parenthesis
    //                                            RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private const string ParameterExpr = @"([^()'\,]+" + // anything but a parenthesis or quote
                                         @"|\'([^']|\\\')*\'" + // a quoted string
                                         @"|\((?<p>)" + // opening parenthesis - increment depth
                                         @"|\)(?<-p>)" + // closing parenthesis - decrement depth
                                         @"|(?(p)\,)" + // a comma but only within parenthesis
                                         @")*" + // a sequence of any number of any of the above
                                         @"(?(p)(?!))"; // require depth to be 0

    private const string FunctionExpr = @"(?<function>[a-z][a-z0-9\._]*)" + // function name
                                        @"\s*\(" + // opening parenthesis
                                        @"((?(param),)(?<param>" + ParameterExpr + @"))*" + // parameters
                                        @"\)"; // final closing parenthesis

    private const string ExpressionExpr = @"\s*" +
                                          @"((?<property>#[a-z0-9\._]+)" + // a property name
                                          @"|(?<int>[+-]?\d+)" + // an integer
                                          @"|(?<float>[+-]?\d+\.\d*)" + // a float
                                          @"|\'(?<string>([^']|\\\')*)\'" + // a quoted string
                                          @"|" + FunctionExpr + // a function call
                                          @")\s*"; // require depth to be 0

    //private const string ExprTriggerExpr = @"(?:#\(\s*)" + FunctionExpr + @"(?:\s*\))";
    private const string ExprTriggerExpr = @"(?:#\(\s*)" + ExpressionExpr + @"(?:\s*\))";

    //private static Regex _functionRegEx = new Regex(FunctionExpr, RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static Regex _expressionRegEx = new Regex("^" + ExpressionExpr + "$",
                                                      RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static Regex _exprTriggerRegEx = new Regex(ExprTriggerExpr, RegexOptions.IgnoreCase | RegexOptions.Compiled);


    private static Dictionary<string, FunctionDefinition> _registeredFunctions;
    private static Dictionary<string, Expression> _expressions = new Dictionary<string, Expression>();

    #endregion

    #region ctor

    static GUIExpressionManager()
    {
      _registeredFunctions = new Dictionary<string, FunctionDefinition>();
      RegisterFunctions(typeof (GUIFunctions));
    }

    #endregion

    public static void RegisterFunctions(Type type)
    {
      MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
      foreach (MethodInfo method in staticMethods)
      {
        if (method.IsDefined(typeof (XMLSkinFunctionAttribute), false))
        {
          XMLSkinFunctionAttribute attrib =
            (XMLSkinFunctionAttribute)method.GetCustomAttributes(typeof (XMLSkinFunctionAttribute), false)[0];
          FunctionDefinition func;
          string functionName = attrib.FunctionName ?? (method.DeclaringType.Name + "." + method.Name);
          if (!_registeredFunctions.TryGetValue(functionName, out func))
          {
            func = new FunctionDefinition(functionName);
            _registeredFunctions.Add(functionName, func);
          }
          func.AddMethod(method);
        }
      }
    }

    public static void UnregisterFunction(string functionName)
    {
      if (_registeredFunctions.ContainsKey(functionName))
      {
        _registeredFunctions.Remove(functionName);
      }
    }

    public static void UnregisterFunctions(Type type)
    {
      MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public);
      foreach (MethodInfo method in staticMethods)
      {
        if (method.IsDefined(typeof (XMLSkinFunctionAttribute), false))
        {
          XMLSkinFunctionAttribute attrib =
            (XMLSkinFunctionAttribute)method.GetCustomAttributes(typeof (XMLSkinFunctionAttribute), false)[0];
          string functionName = attrib.FunctionName ?? (method.DeclaringType.Name + "." + method.Name);
          if (_registeredFunctions.ContainsKey(functionName))
          {
            _registeredFunctions.Remove(functionName);
          }
        }
      }
    }

    public static void ClearExpressionCache()
    {
      lock (_expressions)
      {
        _expressions.Clear();
      }
    }
    public static string Parse(string line)
    {
      return Parse(line, ExpressionOptions.NONE);
    }

    public static string Parse(string line, ExpressionOptions options)
    {
      if (line.IndexOf("#(") > -1)
      {
        MatchCollection matches = _exprTriggerRegEx.Matches(line);
        int offset = 0;
        foreach (Match match in matches)
        {
          string result;
          try
          {
            result = ParseExpression(match).Evaluate(options).ToString();
          }
          catch (TargetInvocationException ex)
          {
            result = ex.InnerException.ToString().Replace('\n', ' ').Replace('\r', ' ');
          }
          catch (Exception ex)
          {
            result = ex.ToString().Replace('\n', ' ').Replace('\r', ' ');
          }
          //line = line.Replace(match.Value, result);
          line = line.Remove(match.Index + offset, match.Length).Insert(match.Index + offset, result);
          offset += result.Length - match.Length;
        }
      }
      return line;
    }

    public static object ParseExpression(string expressionText)
    {
      return ParseExpression(expressionText, ExpressionOptions.NONE);
    }

    public static object ParseExpression(string expressionText, ExpressionOptions options)
    {
      //Match match = _functionRegEx.Match(expressionText);
      Match match = _expressionRegEx.Match(expressionText);
      if (match != null)
      {
        return ParseExpression(match).Evaluate(options);
      }
      return null;
    }

    private static Expression ParseExpression(Match match)
    {
      string expressionText = match.Value.Trim();
      Expression expression;
      lock (_expressions)
      {
        if (_expressions.TryGetValue(expressionText, out expression))
        {
          return expression;
        }
        // temporarily add a dummy expression to prevent other threads from parsing the same expression again
        Log.Debug("Cacheing expression: {0}", expressionText);
        _expressions.Add(expressionText, new LiteralExpression(expressionText));
      }

      if (match.Groups["string"].Success)
      {
        expression = new LiteralExpression(match.Groups["string"].Value);
      }
      else if (match.Groups["int"].Success)
      {
        expression = new LiteralExpression(int.Parse(match.Groups["int"].Value));
      }
      else if (match.Groups["float"].Success)
      {
        expression = new LiteralExpression(float.Parse(match.Groups["float"].Value, CultureInfo.InvariantCulture));
      }
      else if (match.Groups["property"].Success)
      {
        expression = new PropertyExpression(match.Groups["property"].Value);
      }
      else if (match.Groups["function"].Success)
      {
        string functionName = match.Groups["function"].Value;
        FunctionDefinition function;
        if (!_registeredFunctions.TryGetValue(functionName, out function))
        {
          Log.Error("Undefined function '{0}' in expression '{1}'", functionName, match.Value);
          expression = new LiteralExpression(match.Value);
        }
        else
        {
          int paramCount = match.Groups["param"].Captures.Count;
          Expression[] parameters = new Expression[paramCount];

          for (int i = 0; i < paramCount; i++)
          {
            string paramText = match.Groups["param"].Captures[i].Value;
            Match paramMatch = _expressionRegEx.Match(paramText);
            parameters[i] = ParseExpression(paramMatch);
          }

          expression = new FunctionExpression(_registeredFunctions[functionName], parameters);
        }
      }
      lock (_expressions)
      {
        // now replace with the real expression
        _expressions[expressionText] = expression;
      }
      return expression;
    }

    public static void InvalidateExpression(string expressionText)
    {
      Expression expression;
      lock (_expressions)
      {
        if (!_expressions.TryGetValue(expressionText, out expression))
        {
          return;
        }
      }
      expression.Invalidate();
    }
  }
}