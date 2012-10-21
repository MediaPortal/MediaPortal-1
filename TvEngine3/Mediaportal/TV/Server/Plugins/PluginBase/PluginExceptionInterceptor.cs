using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using MediaPortal.Common.Utils;

namespace Mediaportal.TV.Server.Plugins.Base
{
  public class PluginExceptionInterceptor : IInterceptor
  {
    #region logging

    private static ILogManager Log
    {
        get { return LogHelper.GetLogger(typeof(PluginExceptionInterceptor)); }
    }

    #endregion

    public void Intercept(IInvocation invocation)
    {
      try 
      {
        invocation.Proceed();
      } 
      catch (Exception ex)
      {
        Log.ErrorFormat("PluginExceptionInterceptor.Intercept - caught exception: {0}", ex);
        throw;
      }                                                            
    }
  }
}
