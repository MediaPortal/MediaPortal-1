using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MediaPortal.GUI.Library;


namespace MediaPortal.DomainManager
{
  public class CustomDomainManager : AppDomainManager
  {
    public static bool DoLog = false;
    public override void InitializeNewDomain(AppDomainSetup setup)
    {
      base.InitializeNewDomain(setup);

      AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      var requested = new AssemblyName(args.Name);
      if (DoLog)
        Log.Debug("assemblyresolver called for {0}", args.Name);

      var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == requested.Name);
      if (asm != null)
      {
        if (DoLog)
          Log.Debug("already loaded");
        return asm;
      }


      string[] folders = { ".", @"plugins\ExternalPlayers", @"plugins\process", @"plugins\subtitle", @"plugins\windows" };
      foreach (string folder in folders)
      {
        string assemblyPath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            folder,
            requested.Name + ".dll"
        );
        if (File.Exists(assemblyPath))
        {
          var available = AssemblyName.GetAssemblyName(assemblyPath);

          // Check assembly identity, ignoring version.
          if (available.Name != requested.Name ||
              !string.Equals(
                  available.GetPublicKeyToken()?.ToString(),
                  requested.GetPublicKeyToken()?.ToString(),
                  StringComparison.OrdinalIgnoreCase))
          {
            if (DoLog)
              Log.Debug("wrong identity");
          }
          else
          {
            if (DoLog)
              Log.Debug("found at {0}", assemblyPath);
            return Assembly.LoadFrom(assemblyPath);
          }
        }
      }

      Log.Debug("not found");
      return null;
    }
  }
}