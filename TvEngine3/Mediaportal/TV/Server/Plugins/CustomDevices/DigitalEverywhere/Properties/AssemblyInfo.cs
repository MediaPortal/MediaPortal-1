using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Digital Everywhere")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Team MediaPortal")]
[assembly: AssemblyProduct("Digital Everywhere")]
[assembly: AssemblyCopyright("Copyright © Team MediaPortal 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("43e86eb0-b12b-4496-91d9-d7bfea022a1f")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// MediaPortal TV Server plugin compatibility.
[assembly: CompatibleVersion("1.2.3.0", "1.2.3.0")]
[assembly: UsesSubsystem("TVE.DirectShow")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice.ConditionalAccess")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice.ConditionalAccessMenu")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice.Diseqc")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice.PidFilter")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice.Power")]
[assembly: UsesSubsystem("TVE.Plugins.CustomDevice.Tuner")]