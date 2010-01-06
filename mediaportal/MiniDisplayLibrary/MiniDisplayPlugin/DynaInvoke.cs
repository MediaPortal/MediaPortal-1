using System;
using System.Collections;
using System.Reflection;

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
      foreach (Type type in assembly.GetTypes())
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
      foreach (Type type in Assembly.LoadFrom(AssemblyName).GetTypes())
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
      public Type type;

      public DynaClassInfo() {}

      public DynaClassInfo(Type t, object c)
      {
        this.type = t;
        this.ClassObject = c;
      }
    }
  }
}