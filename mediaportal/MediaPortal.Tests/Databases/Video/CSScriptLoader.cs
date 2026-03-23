using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.CSharp;

namespace MediaPortal.Tests.Databases.Video
{
  /// <summary>
  /// Helper class that compiles .csscript files at runtime using CSharpCodeProvider.
  /// Parses //css_reference directives and resolves assembly references relative to the
  /// calling assembly's output directory.
  /// </summary>
  public class CSScriptLoader
  {
    /// <summary>
    /// Compiles a .csscript file and returns the resulting assembly.
    /// </summary>
    /// <param name="scriptPath">Full path to the .csscript file.</param>
    /// <returns>The compiled <see cref="Assembly"/>.</returns>
    public static Assembly LoadScript(string scriptPath)
    {
      if (!File.Exists(scriptPath))
      {
        throw new FileNotFoundException("Script file not found: " + scriptPath);
      }

      string source = File.ReadAllText(scriptPath);

      // Parse //css_reference directives
      List<string> cssReferences = new List<string>();
      Regex rxRef = new Regex(@"^//css_reference\s+""?([^"";\s]+)""?\s*;?\s*$", RegexOptions.Multiline);
      foreach (Match m in rxRef.Matches(source))
      {
        cssReferences.Add(m.Groups[1].Value);
      }

      // Strip //css_reference lines (not valid C#)
      source = Regex.Replace(source, @"^//css_reference\s+.*$", string.Empty, RegexOptions.Multiline);

      // Resolve assembly references
      string outputDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      List<string> resolvedReferences = new List<string>();

      // Always include these framework assemblies
      resolvedReferences.Add("System.dll");
      resolvedReferences.Add("System.Core.dll");
      resolvedReferences.Add("System.Data.dll");
      resolvedReferences.Add("System.Web.dll");
      resolvedReferences.Add("System.Xml.dll");
      resolvedReferences.Add("System.Windows.Forms.dll"); // TODO: remove this because a script to get data from the internet should not use Windows Forms
      resolvedReferences.Add("System.Drawing.dll"); // TODO: remove this because a script to get data from the internet should not draw anything

      foreach (string cssRef in cssReferences)
      {
        string resolved = ResolveAssembly(cssRef, outputDir);
        if (!string.IsNullOrEmpty(resolved) && !resolvedReferences.Contains(resolved))
        {
          resolvedReferences.Add(resolved);
        }
      }

      // Compile
      CSharpCodeProvider provider = new CSharpCodeProvider();
      CompilerParameters parameters = new CompilerParameters();
      parameters.GenerateInMemory = true;
      parameters.GenerateExecutable = false;

      foreach (string refPath in resolvedReferences)
      {
        parameters.ReferencedAssemblies.Add(refPath);
      }

      CompilerResults results = provider.CompileAssemblyFromSource(parameters, source);

      if (results.Errors.HasErrors)
      {
        string errors = string.Empty;
        foreach (CompilerError error in results.Errors)
        {
          if (!error.IsWarning)
          {
            errors += string.Format("Line {0}: {1} - {2}\n", error.Line, error.ErrorNumber, error.ErrorText);
          }
        }
        throw new InvalidOperationException("Script compilation failed:\n" + errors);
      }

      return results.CompiledAssembly;
    }

    /// <summary>
    /// Creates an instance of the specified type from a compiled script assembly.
    /// </summary>
    public static object CreateObject(Assembly assembly, string typeName)
    {
      Type type = assembly.GetType(typeName, false);
      if (type == null)
      {
        // Try searching all types (the class might be internal)
        foreach (Type t in assembly.GetTypes())
        {
          if (t.Name == typeName)
          {
            type = t;
            break;
          }
        }
      }

      if (type == null)
      {
        throw new TypeLoadException("Type '" + typeName + "' not found in compiled script.");
      }

      return Activator.CreateInstance(type);
    }

    /// <summary>
    /// Locates the scripts directory by searching up from the test assembly output directory
    /// to find the MediaPortal.Base\scripts\MovieInfo folder.
    /// </summary>
    public static string FindScriptsDirectory(string subFolder)
    {
      string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

      // Walk up to find the mediaportal solution directory
      // Test output is typically: mediaportal\MediaPortal.Tests\bin\Debug\
      // Scripts are at: mediaportal\MediaPortal.Base\scripts\MovieInfo\
      for (int i = 0; i < 10; i++)
      {
        string candidate = Path.Combine(dir, "MediaPortal.Base", "scripts", subFolder);
        if (Directory.Exists(candidate))
        {
          return candidate;
        }
        dir = Path.GetDirectoryName(dir);
        if (dir == null) break;
      }

      throw new DirectoryNotFoundException(
        "Could not find MediaPortal.Base\\scripts\\" + subFolder + " directory. " +
        "Searched upward from: " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
    }

    private static string ResolveAssembly(string name, string outputDir)
    {
      // Skip framework assemblies that are already added
      if (name.Equals("System.Core", StringComparison.OrdinalIgnoreCase) ||
          name.Equals("System.Web", StringComparison.OrdinalIgnoreCase))
      {
        return null;
      }

      // Ensure .dll extension
      string dllName = name;
      if (!dllName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
      {
        dllName += ".dll";
      }

      // Look in the output directory first
      string path = Path.Combine(outputDir, dllName);
      if (File.Exists(path))
      {
        return path;
      }

      // Try case-insensitive match in the output directory
      foreach (string file in Directory.GetFiles(outputDir, "*.dll"))
      {
        if (Path.GetFileName(file).Equals(dllName, StringComparison.OrdinalIgnoreCase))
        {
          return file;
        }
      }

      // As a last resort, try framework resolution (for things like "System.Core")
      return dllName;
    }
  }
}
