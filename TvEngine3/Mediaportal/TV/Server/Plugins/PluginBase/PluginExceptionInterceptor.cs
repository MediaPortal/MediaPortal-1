using System;
using Castle.DynamicProxy;
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
        this.LogError("PluginExceptionInterceptor.Intercept - caught exception: {0}", ex);
        throw;
      }                                                            
    }
  }
}
