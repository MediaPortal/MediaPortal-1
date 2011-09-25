using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using TvLibrary.Interfaces;

namespace TvLibrary.Interfaces
{
  public static class DynamicLinqBuilder
  {
    /*public static IQueryable ApplyNotContainsFilter(IQueryable source, string propertyName, object propertyValue) { }
    public static IQueryable ApplyContainsFilter(IQueryable source, string propertyName, object propertyValue) { }
    public static IQueryable ApplyStartsWithFilter(IQueryable source, string propertyName, object propertyValue) { }   
    */
     
    public static IQueryable<ProgramDTO> ApplyFilter<T>(IQueryable source, string propertyName, T propertyValue, ConditionOperator conditionOperator)
    {     
      LambdaExpression lambdaExpression = null;
      string propertyValueString = propertyValue as string;
      switch (conditionOperator)
      {
        case ConditionOperator.Equals:                   
          Expression expression;
          ParameterExpression parameterExpression = GetParameterExpression(source, propertyName, propertyValue, out expression);
     
          var parameterExp = new ParameterExpression[1] 
                                        {
                                          parameterExpression
                                        };
          if (propertyValueString != null)
          {
            propertyValueString = propertyValueString.ToUpperInvariant();
     
            MethodInfo methodToUpperInvariant = typeof(string).GetMethod("ToUpperInvariant");
            expression = Expression.Call(expression, methodToUpperInvariant);
            lambdaExpression = Expression.Lambda(Expression.Equal(expression, Expression.Constant(propertyValueString)), parameterExp);
          }
          else
          {
            lambdaExpression = Expression.Lambda(Expression.Equal(expression, Expression.Constant(propertyValue)), parameterExp);
          }
          break;
     
        case ConditionOperator.Contains:
          if (propertyValueString != null)
          {
            lambdaExpression = GetContainsExpression<ProgramDTO>(propertyName, propertyValueString);
          }
          else
          {
            throw new InvalidOperationException("ConditionOperator.Contains only supports strings");
          }
          break;       
        case ConditionOperator.NotContains:
          if (propertyValueString != null)
          {
            lambdaExpression = GetNotContainsExpression<ProgramDTO>(propertyName, propertyValueString);
          }
          else
          {
            throw new InvalidOperationException("ConditionOperator.NotContains only supports strings");
          }
          break;
        case ConditionOperator.StartsWith:
          if (propertyValueString != null)
          {
            lambdaExpression = GetStartsWithExpression<ProgramDTO>(propertyName, propertyValueString);
          }
          else
          {
            throw new InvalidOperationException("ConditionOperator.StartsWith only supports strings");
          }
          break;
      }
         
      MethodCallExpression methodCallExpression = Expression.Call(typeof (Queryable), "Where", new Type[1]
      {
        source.ElementType
      }, new Expression[2]
      {
        source.Expression,
        Expression.Quote(lambdaExpression)
      });
      return source.Provider.CreateQuery<ProgramDTO>(methodCallExpression);
    }

    private static Expression<Func<T, bool>> GetContainsExpression<T>(string propertyName, string propertyValue)
    {
      propertyValue = propertyValue.ToUpperInvariant();
      var parameterExp = Expression.Parameter(typeof(T), "type");
      var propertyExp = Expression.Property(parameterExp, propertyName);
      MethodInfo methodToUpperInvariant = typeof(string).GetMethod("ToUpperInvariant");
      var toUpperInvariantMethodExp = Expression.Call(propertyExp, methodToUpperInvariant);
      MethodInfo methodContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
      var propertyValueExp = Expression.Constant(propertyValue, typeof(string));
      var containsMethodExp = Expression.Call(toUpperInvariantMethodExp, methodContains, propertyValueExp);

      Expression<Func<T, bool>> containsExpression = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
      return containsExpression;
    }

    private static LambdaExpression GetStartsWithExpression<T>(string propertyName, string propertyValue)
    {            
      var parameterExp = Expression.Parameter(typeof(T), "type");
      var propertyExp = Expression.Property(parameterExp, propertyName);

      MethodInfo methodStartsWith = typeof(string).GetMethod("StartsWith", new[] { typeof(string), typeof(bool), typeof(CultureInfo) });
      var propertyValueExp = Expression.Constant(propertyValue);
      var ignoreCaseExp = Expression.Constant(true);
      var cultureInfoExp = Expression.Constant(CultureInfo.InvariantCulture);
      var startsWithMethodExp = Expression.Call(propertyExp, methodStartsWith, new[] { propertyValueExp, ignoreCaseExp, cultureInfoExp });

      /*Expression toUpperInvariantMethodExp = Expression.Call(
        propertyExp,
        typeof(T).GetMethod("StartsWith", new[] { typeof(string), typeof(bool), typeof(CultureInfo) }),
        propertyValueExp,
        ignoreCaseExp, 
        cultureInfoExp
        );
      */
      Expression<Func<T, bool>> containsExpression = Expression.Lambda<Func<T,  bool>>(startsWithMethodExp, parameterExp);
      return containsExpression;
    }

    private static LambdaExpression GetNotContainsExpression<T>(string propertyName, string propertyValue)
    {
      propertyValue = propertyValue.ToUpperInvariant();
      var parameterExp = Expression.Parameter(typeof(T), "type");
      var propertyExp = Expression.Property(parameterExp, propertyName);
      MethodInfo methodToUpperInvariant = typeof(string).GetMethod("ToUpperInvariant");
      var toUpperInvariantMethodExp = Expression.Call(propertyExp, methodToUpperInvariant);
      MethodInfo methodContains = typeof(string).GetMethod("Contains", new[] { typeof(string) });
      var propertyValueExp = Expression.Constant(propertyValue, typeof(string));
      var containsMethodExp = Expression.Not(Expression.Call(toUpperInvariantMethodExp, methodContains, propertyValueExp));


      Expression<Func<T, bool>> containsExpression = Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
      return containsExpression;
    }

    private static ParameterExpression GetParameterExpression<T>(IQueryable source, string propertyName, T value,
                                                                  out Expression expression)
    {
      ParameterExpression parameterExpression = Expression.Parameter(source.ElementType, string.Empty);
      expression = CreatePropertyExpression(parameterExpression, propertyName);
      if (Nullable.GetUnderlyingType(expression.Type) != null && value != null)
      {
        expression = Expression.Convert(expression, RemoveNullableFromType(expression.Type));
      }
      return parameterExpression;
    }
     
    private static Expression CreatePropertyExpression(Expression parameterExpression, string propertyName)
    {
      string str = propertyName;
      var chArray = new char[1]
      {
        '.'
      };
      return str.Split(chArray).Aggregate<string, Expression>(null, (current, propertyOrFieldName) => current != null ? Expression.PropertyOrField(current, propertyOrFieldName) : Expression.PropertyOrField(parameterExpression, propertyOrFieldName));
    }
     
    /*public static T ChangeType<T>(object propertyValue)
    {
      return (T) ChangeType(propertyValue, typeof (T));
    }
    public static object ChangeType(object propertyValue, Type type)
    {
      if (type == null)
        throw new ArgumentNullException("type");
      if (propertyValue == null)
      {
        if (TypeAllowsNull(type))
          return (object) null;
        return Convert.ChangeType(propertyValue, type, CultureInfo.CurrentCulture);
      }
      type = RemoveNullableFromType(type);
      if (propertyValue.GetType() == type)
      {
        return propertyValue;
      }
      TypeConverter converter1 = TypeDescriptor.GetConverter(type);
      if (converter1.CanConvertFrom(propertyValue.GetType()))
      {
        return converter1.ConvertFrom(propertyValue);
      }
      TypeConverter converter2 = TypeDescriptor.GetConverter(propertyValue.GetType());
      if (converter2.CanConvertTo(type))
        return converter2.ConvertTo(propertyValue, type);
      throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "CannotConvertType", new object[2]
                                                                                                            {
                                                                                                              propertyValue.GetType(),
                                                                                                              type
                                                                                                            }));
    }
       
    internal static bool TypeAllowsNull(Type type)
    {
      if (!(Nullable.GetUnderlyingType(type) != null))
        return !type.IsValueType;
      return true;
    }
*/
    public static Type RemoveNullableFromType(Type type)
    {
      return Nullable.GetUnderlyingType(type) ?? type;
    }
  }
} 


