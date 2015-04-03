// Author: Prasanna V. Loganathar
// Created: 2:10 AM 27-11-2014
// Project: LiquidState
// License: http://www.apache.org/licenses/LICENSE-2.0

using System.Reflection;
using System.Resources;
using LiquidState.Properties;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.

[assembly: AssemblyTitle("LiquidState")]
[assembly: AssemblyDescription("Efficient state machines for .NET with both synchronous and asynchronous support")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("LiquidState")]
[assembly: AssemblyCopyright("Copyright ©  2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: NeutralResourcesLanguage("en")]

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

[assembly: AssemblyVersion(AssemblyConstants.Version)]
[assembly: AssemblyFileVersion(AssemblyConstants.Version)]
[assembly: AssemblyInformationalVersion(AssemblyConstants.InformationalVersion)]

namespace LiquidState.Properties
{
    public static class AssemblyConstants
    {
        public const string Version = "4.1";
        public const string InformationalVersion = "4.1";
    }
}