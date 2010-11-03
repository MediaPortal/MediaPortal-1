using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SetupTv
{
  public static class ExtentionMethods
  {
    public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
    {
      Random rnd = new Random();
      return source.OrderBy<T, int>((item) => rnd.Next());
    }
  }
}
