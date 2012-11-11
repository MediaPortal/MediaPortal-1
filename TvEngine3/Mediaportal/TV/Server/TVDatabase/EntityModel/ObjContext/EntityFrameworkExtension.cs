using System;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mediaportal.TV.Server.TVDatabase.EntityModel.ObjContext
{
    /// <summary>
    /// Adds entension methods for <see cref="IQueryable"/>
    /// </summary>
    public static class IQueryableExtension
    {
        public static IQueryable<TSource> Include<TSource>(this IQueryable<TSource> source, string path)
        {
            var objectQuery = source as ObjectQuery<TSource>;
            if (objectQuery != null)
            {
                return objectQuery.Include(path);
            }
            return source;
        }

        public static IQueryable<T> Include<T>(this IQueryable<T> mainQuery, Expression<Func<T, object>> subSelector)
        {
            return mainQuery.Include(((subSelector.Body as MemberExpression).Member as PropertyInfo).Name);
        }
    }
}
