using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using MediaPortal.Common.Utils;
using Mediaportal.TV.Server.Plugins.Base.Interfaces;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.Plugins.Base
{
  public class PluginExceptionInterceptor : IInterceptor
  {
    public void Intercept(IInvocation invocation)
    {
      try 
      {
        invocation.Proceed();
      } 
      catch (Exception ex)
      {
        Log.Error("PluginExceptionInterceptor.Intercept - caught exception: {0}", ex);
        throw;
      }                                                            
    }
  }
}
