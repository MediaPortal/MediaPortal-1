using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace VersionUpdater
{
  public class VersionResult
  {
    public Version Current;
    public Version UpgradableFrom;
    public string VersionTitle;
    public bool Valid;
  }

  public class VersionUpdater
  {
    public string SolutionInfoAssemblyVersion;
    public string SolutionInfoAssemblyFileVersion;
    public string MPConfigAppConfig { get; set; }
    public string MPAppConfig { get; set; }
    public string InstallerScript { get; set; }
    public string DeployToolMaxPackageVersion { get; set; }
    public string DeployToolCurrentPackageVersion { get; set; }
    public string DeployToolCurrentDisplayVersion { get; set; }

    private string fSourceCodeFolder;

    public VersionUpdater(string sourceCodeFolder)
    {
      fSourceCodeFolder = sourceCodeFolder;
      ParseSolutionInfo(getAbsPath(@"Common-MP-TVE3\SolutionInfo.cs"));
      MPAppConfig = GetFromAppConfig(getAbsPath(@"mediaportal\MediaPortal.Application\App.config"));
      MPConfigAppConfig = GetFromAppConfig(getAbsPath(@"mediaportal\Configuration\App.config"));
      InstallerScript = GetFromInstallerScript(getAbsPath(@"Tools\InstallationScripts\include\MediaPortalCurrentVersion.nsh"));
      ParseUtils(getAbsPath(@"Tools\MediaPortal.DeployTool\Utils.cs"));
    }
    private string getAbsPath(string relPath)
    {
      return Path.Combine(fSourceCodeFolder, relPath);
    }
    public void Save(Version current, Version upgradableFrom, string description)
    {
      SaveSolutionInfo(getAbsPath(@"Common-MP-TVE3\SolutionInfo.cs"), String.Format("{0}.{1}.{2}.0", current.Major, current.Minor, current.Build));

      string appConfigVersion = current.Build == 0 ?
        String.Format("{0}.{1}-{2}", current.Major, current.Minor, description) :
        String.Format("{0}.{1}.{2}-{3}", current.Major, current.Minor, current.Build, description);
      SetAppConfig(getAbsPath(@"mediaportal\MediaPortal.Application\App.config"), appConfigVersion);
      SetAppConfig(getAbsPath(@"mediaportal\Configuration\App.config"), appConfigVersion);
      SetInstallerScript(getAbsPath(@"Tools\InstallationScripts\include\MediaPortalCurrentVersion.nsh"), current.Major, current.Minor, current.Build);
      SetUtils(getAbsPath(@"Tools\MediaPortal.DeployTool\Utils.cs"), current, upgradableFrom, description);
    }

    public VersionResult Validate()
    {
      VersionResult res = new VersionResult
      {
        Valid = true
      };
      if (!Version.TryParse(SolutionInfoAssemblyVersion, out res.Current))
      {
        res.Valid = false;
      }
      res.Valid = res.Valid && check(res.Current, SolutionInfoAssemblyFileVersion)
        && check(res.Current, InstallerScript)
        && check(res.Current, DeployToolCurrentPackageVersion) && MPConfigAppConfig == MPAppConfig;

      Match m = Regex.Match(MPAppConfig, @"^(?<major>\d+)\.(?<minor>\d+)(?:\.(?<revision>\d+))?\-(?<description>.*)");
      if (m.Success)
      {
        res.Valid = res.Valid && (m.Groups["major"].Value == res.Current.Major.ToString())
          && (m.Groups["minor"].Value == res.Current.Minor.ToString())
          && (!m.Groups["revision"].Success || (m.Groups["revision"].Value == res.Current.Build.ToString()));

        res.VersionTitle = m.Groups["description"].Value;
      }
      else
        res.Valid = false;

      m = Regex.Match(DeployToolCurrentDisplayVersion, @"^(?<major>\d+)\.(?<minor>\d+)(?:\.(?<revision>\d+))?\s(?<description>.*)");
      if (m.Success)
      {
        res.Valid = res.Valid && (m.Groups["major"].Value == res.Current.Major.ToString())
          && (m.Groups["minor"].Value == res.Current.Minor.ToString())
          && (!m.Groups["revision"].Success || (m.Groups["revision"].Value == res.Current.Build.ToString()))
          && m.Groups["description"].Value == res.VersionTitle;
      }
      else
        res.Valid = false;

      if (!Version.TryParse(DeployToolMaxPackageVersion, out res.UpgradableFrom))
      {
        res.Valid = false;
      };
      if (res.Valid)
        res.Current = new Version(res.Current.Major, res.Current.Minor, res.Current.Build);

      return res;
    }

    private bool check(Version ver, string versionToTest)
    {
      Version tmp;
      if (!Version.TryParse(versionToTest, out tmp))
        return false;
      else
      {
        if (tmp.Revision == -1)
          tmp = new Version(tmp.Major, tmp.Minor, tmp.Build, 0);
        return ver.Equals(tmp);
      }
    }

    private void ParseUtils(string fileName)
    {
      var lines = File.ReadAllLines(fileName);
      for (int i = 0; i < lines.Length; i++)
      {
        if (lines[i].Contains(@"case ""max"":"))
        {
          string maj = null, min = null, rev = null;
          getValueFromLine(lines[i + 1], @"major\s*=\s*(?<version>\d+)", ref maj);
          getValueFromLine(lines[i + 2], @"minor\s*=\s*(?<version>\d+)", ref min);
          getValueFromLine(lines[i + 3], @"revision\s*=\s*(?<version>\d+)", ref rev);
          if (maj != null && min != null && rev != null)
            DeployToolMaxPackageVersion = String.Format("{0}.{1}.{2}", maj, min, rev);

        }
        if (lines[i].Contains("public static Version GetCurrentPackageVersion()"))
        {
          string maj = null, min = null, rev = null;
          getValueFromLine(lines[i + 2], @"major\s*=\s*(?<version>\d+)", ref maj);
          getValueFromLine(lines[i + 3], @"minor\s*=\s*(?<version>\d+)", ref min);
          getValueFromLine(lines[i + 4], @"revision\s*=\s*(?<version>\d+)", ref rev);
          if (maj != null && min != null && rev != null)
            DeployToolCurrentPackageVersion = String.Format("{0}.{1}.{2}", maj, min, rev);
        }

        if (lines[i].Contains("public static string GetDisplayVersion()"))
        {
          string version = null;
          getValueFromLine(lines[i + 2], @"return\s""(?<version>[^""]*)"";", ref version);
          DeployToolCurrentDisplayVersion = version;
        }
      }
    }

    private void SetUtils(string fileName, Version current, Version upgradableFrom, string description)
    {
      var lines = File.ReadAllLines(fileName);
      for (int i = 0; i < lines.Length; i++)
      {
        if (lines[i].Contains(@"case ""max"":"))
        {
          setValueFromLine(ref lines[i + 1], @"major\s*=\s*(?<version>\d+)", upgradableFrom.Major.ToString());
          setValueFromLine(ref lines[i + 2], @"minor\s*=\s*(?<version>\d+)", upgradableFrom.Minor.ToString());
          setValueFromLine(ref lines[i + 3], @"revision\s*=\s*(?<version>\d+)", upgradableFrom.Build.ToString());
        }
        if (lines[i].Contains("public static Version GetCurrentPackageVersion()"))
        {
          setValueFromLine(ref lines[i + 2], @"major\s*=\s*(?<version>\d+)", current.Major.ToString());
          setValueFromLine(ref lines[i + 3], @"minor\s*=\s*(?<version>\d+)", current.Minor.ToString());
          setValueFromLine(ref lines[i + 4], @"revision\s*=\s*(?<version>\d+)", current.Build.ToString("D3"));
        }

        if (lines[i].Contains("public static string GetDisplayVersion()"))
        {
          string version = current.Build == 0 ?
            String.Format("{0}.{1} {2}", current.Major, current.Minor, description) :
            String.Format("{0}.{1}.{2} {3}", current.Major, current.Minor, current.Build, description);
          setValueFromLine(ref lines[i + 2], @"return\s""(?<version>[^""]*)"";", version);
        }
      }
      File.WriteAllLines(fileName, lines);
    }

    private string GetFromInstallerScript(string fileName)
    {
      string maj = null, min = null, rev = null;
      var lines = File.ReadAllLines(fileName);
      foreach (var line in lines)
      {
        getValueFromLine(line, @"!define\sVER_MAJOR\s+(?<version>\d+)", ref maj);
        getValueFromLine(line, @"!define\sVER_MINOR\s+(?<version>\d+)", ref min);
        getValueFromLine(line, @"!define\sVER_REVISION\s+(?<version>\d+)", ref rev);
      }
      if (maj == null || min == null || rev == null)
        return null;
      return String.Format("{0}.{1}.{2}", maj, min, rev);
    }

    private void SetInstallerScript(string fileName, int maj, int min, int rev)
    {
      var lines = File.ReadAllLines(fileName);
      for (int i = 0; i < lines.Length; i++)
      {
        setValueFromLine(ref lines[i], @"!define\sVER_MAJOR\s+(?<version>\d+)", maj.ToString());
        setValueFromLine(ref lines[i], @"!define\sVER_MINOR\s+(?<version>\d+)", min.ToString());
        setValueFromLine(ref lines[i], @"!define\sVER_REVISION\s+(?<version>\d+)", rev.ToString("D3"));
      }
      File.WriteAllLines(fileName, lines);
    }

    private string GetFromAppConfig(string fileName)
    {
      var lines = File.ReadAllLines(fileName);
      string result = null;
      foreach (var line in lines)
      {
        getValueFromLine(line, @"<add\skey=""version""\svalue=""(?<version>[^""]*)""\s*/>", ref result);
        if (result != null)
          return result;
      }
      return null;
    }

    private void SetAppConfig(string fileName, string value)
    {
      var lines = File.ReadAllLines(fileName);
      for (int i = 0; i < lines.Length; i++)
      {
        setValueFromLine(ref lines[i], @"<add\skey=""version""\svalue=""(?<version>[^""]*)""\s*/>", value);
      }
      File.WriteAllLines(fileName, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private void ParseSolutionInfo(string fileName)
    {
      var lines = File.ReadAllLines(fileName);
      foreach (var line in lines)
      {
        getValueFromLine(line, @"\[assembly:\sAssemblyVersion\(""(?<version>[^""]*)""\)]", ref SolutionInfoAssemblyVersion);
        getValueFromLine(line, @"\[assembly:\sAssemblyFileVersion\(""(?<version>[^""]*)""\)]", ref SolutionInfoAssemblyFileVersion);
      }
    }

    private void SaveSolutionInfo(string fileName, string newValue)
    {
      var lines = File.ReadAllLines(fileName);
      for (int i = 0; i < lines.Length; i++)
      {
        setValueFromLine(ref lines[i], @"\[assembly:\sAssemblyVersion\(""(?<version>[^""]*)""\)]", newValue);
        setValueFromLine(ref lines[i], @"\[assembly:\sAssemblyFileVersion\(""(?<version>[^""]*)""\)]", newValue);
      }
      File.WriteAllLines(fileName, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    private void getValueFromLine(string line, string regex, ref string value)
    {
      Match m = Regex.Match(line, regex);
      if (m.Success) value = m.Groups["version"].Value;
    }
    private void setValueFromLine(ref string line, string regex, string value)
    {
      Regex r = new Regex(regex);
      line = r.Replace(line, match => match.Value.Replace(match.Groups["version"].Value, value));
    }
  }
}
