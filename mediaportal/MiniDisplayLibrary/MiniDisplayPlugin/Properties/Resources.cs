using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Properties
{
  [CompilerGenerated, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"),
   DebuggerNonUserCode]
  internal class Resources
  {
    private static CultureInfo resourceCulture;
    private static ResourceManager resourceMan;

    internal Resources() {}

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static CultureInfo Culture
    {
      get { return resourceCulture; }
      set { resourceCulture = value; }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    internal static ResourceManager ResourceManager
    {
      get
      {
        if (ReferenceEquals(resourceMan, null))
        {
          ResourceManager manager = new ResourceManager("MiniDisplayPlugin.Properties.Resources",
                                                        typeof (Resources).Assembly);
          resourceMan = manager;
        }
        return resourceMan;
      }
    }
  }
}