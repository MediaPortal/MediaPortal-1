using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Un4seen.Bass;
using DShowNET.AudioMixer;
using Microsoft.Win32;
using MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting;
using MediaPortal.Configuration;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Player;
using MediaPortal.Profile;
using MediaPortal.TV.Recording;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class DynaInvoke
  {
    public static Hashtable AssemblyReferences = new Hashtable();
    public static Hashtable ClassReferences = new Hashtable();

    public static DynaClassInfo GetClassReference(string AssemblyName, string ClassName)
    {
      Assembly assembly;
      if (ClassReferences.ContainsKey(AssemblyName))
      {
        return (DynaClassInfo)ClassReferences[AssemblyName];
      }
      if (!AssemblyReferences.ContainsKey(AssemblyName))
      {
        AssemblyReferences.Add(AssemblyName, assembly = Assembly.LoadFrom(AssemblyName));
      }
      else
      {
        assembly = (Assembly)AssemblyReferences[AssemblyName];
      }
      foreach (System.Type type in assembly.GetTypes())
      {
        if (type.IsClass && type.FullName.EndsWith("." + ClassName))
        {
          DynaClassInfo info = new DynaClassInfo(type, Activator.CreateInstance(type));
          ClassReferences.Add(AssemblyName, info);
          return info;
        }
      }
      throw new Exception("could not instantiate class");
    }

    public static object InvokeMethod(DynaClassInfo ci, string MethodName, object[] args)
    {
      return ci.type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, ci.ClassObject, args);
    }

    public static object InvokeMethod(string AssemblyName, string ClassName, string MethodName, object[] args)
    {
      return InvokeMethod(GetClassReference(AssemblyName, ClassName), MethodName, args);
    }

    public static object InvokeMethodSlow(string AssemblyName, string ClassName, string MethodName, object[] args)
    {
      foreach (System.Type type in Assembly.LoadFrom(AssemblyName).GetTypes())
      {
        if (type.IsClass && type.FullName.EndsWith("." + ClassName))
        {
          object target = Activator.CreateInstance(type);
          return type.InvokeMember(MethodName, BindingFlags.InvokeMethod, null, target, args);
        }
      }
      throw new Exception("could not invoke method");
    }

    public class DynaClassInfo
    {
      public object ClassObject;
      public System.Type type;

      public DynaClassInfo()
      {
      }

      public DynaClassInfo(System.Type t, object c)
      {
        this.type = t;
        this.ClassObject = c;
      }
    }
  }

}

