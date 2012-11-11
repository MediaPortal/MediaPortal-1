using System.Reflection;
using System.Runtime.InteropServices;
using MediaPortal.Common.Utils;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("KNC")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Team MediaPortal")]
[assembly: AssemblyProduct("KNC")]
[assembly: AssemblyCopyright("Copyright © Team MediaPortal 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("9e4133f3-78dd-457d-aa12-a570b2b655fc")]

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