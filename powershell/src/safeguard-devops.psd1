#
# Module manifest for module 'safeguard-devops'
#
# Generated by: petrsnd
#
# Generated on: 7/14/2020
#

@{

# Script module or binary module file associated with this manifest.
RootModule = 'safeguard-devops.psm1'

# Version number of this module.
ModuleVersion = '0.1.99999'

# Supported PSEditions
# CompatiblePSEditions = @()

# ID used to uniquely identify this module
GUID = '989cd395-44ed-41b7-96a3-b44bcec9e3d0'

# Author of this module
Author = 'petrsnd'

# Company or vendor of this module
CompanyName = 'One Identity LLC'

# Copyright statement for this module
Copyright = '(c) 2020 One Identity LLC. All rights reserved.'

# Description of the functionality provided by this module
Description = 'Scripting tools for interacting with the One Identity Safeguard DevOps Service.'

# Minimum version of the Windows PowerShell engine required by this module
PowerShellVersion = '5.0'

# Name of the Windows PowerShell host required by this module
# PowerShellHostName = ''

# Minimum version of the Windows PowerShell host required by this module
# PowerShellHostVersion = ''

# Minimum version of Microsoft .NET Framework required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# DotNetFrameworkVersion = ''

# Minimum version of the common language runtime (CLR) required by this module. This prerequisite is valid for the PowerShell Desktop edition only.
# CLRVersion = ''

# Processor architecture (None, X86, Amd64) required by this module
# ProcessorArchitecture = ''

# Modules that must be imported into the global environment prior to importing this module
# RequiredModules = @()

# Assemblies that must be loaded prior to importing this module
# RequiredAssemblies = @()

# Script files (.ps1) that are run in the caller's environment prior to importing this module.
# ScriptsToProcess = @()

# Type files (.ps1xml) to be loaded when importing this module
# TypesToProcess = @()

# Format files (.ps1xml) to be loaded when importing this module
# FormatsToProcess = @()

# Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
NestedModules = @(
     # Main = safeguard-devops.psm1
     # Internal Only
     'sslhandling.psm1',
     'ps-utilities.psm1',
     # Public Modules
     'init.psm1',
     'certificates.psm1',
     'configuration.psm1',
     'plugins.psm1'
)

# Functions to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no functions to export.
FunctionsToExport = @(
     # sslhandling.psm1 is just a helper module -- no functions exported
     # ps-utilities.psm1 is just a helper module -- no functions exported

     # safeguard-devops.psm1 -- handles login/logout and calling generic methods
     'Get-SgDevOpsApplianceStatus', 'Initialize-SgDevOpsAppliance', 'Clear-SgDevOpsAppliance',
     'Connect-SgDevOps', 'Disconnect-SgDevOps', # <-- session init / deinit
     'Invoke-SgDevOpsMethod', # <-- this is the function used to implement most of the others

     # init.psm1 -- handles full service initialization walkthrough UX
     'Get-SgDevOpsStatus', 'Initialize-SgDevOps',

     # certificates.psm1 -- handles web server and client certificate setup
     'Get-SgDevOpsSslCertificate', 'Install-SgDevOpsSslCertificate', 'Clear-SgDevOpsSslCertificate',
     'Get-SgDevOpsClientCertificate', 'Install-SgDevOpsClientCertificate', 'Clear-SgDevOpsClientCertificate',
     'New-SgDevOpsCsr',
     'Get-SgDevOpsTrustedCertificate', 'Install-SgDevOpsTrustedCertificate', 'Remove-SgDevOpsTrustedCertificate',
     'Sync-SgDevOpsTrustedCertificate',

     # configuration.psm1
     'Get-SgDevOpsConfiguration', 'Initialize-SgDevOpsConfiguration', 'Clear-SgDevOpsConfiguration',
     'Get-SgDevOpsAvailableAssetAccount','Get-SgDevOpsRegisteredAssetAccount',
     'Register-SgDevOpsAssetAccount','Unregister-SgDevOpsAssetAccount',

     # plugins.psm1
     'Get-SgDevOpsPlugin', 'Install-SgDevOpsPlugin', 'Remove-SgDevOpsPlugin',
     'Get-SgDevOpsPluginVaultAccount', 'Set-SgDevOpsPluginVaultAccount',
     'Get-SgDevOpsPluginSetting', 'Set-SgDevOpsPluginSetting'
)

# Cmdlets to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no cmdlets to export.
CmdletsToExport = @()

# Variables to export from this module
VariablesToExport = '*'

# Aliases to export from this module, for best performance, do not use wildcards and do not delete the entry, use an empty array if there are no aliases to export.
AliasesToExport = @()

# DSC resources to export from this module
# DscResourcesToExport = @()

# List of all modules packaged with this module
# ModuleList = @()

# List of all files packaged with this module
# FileList = @()

# Private data to pass to the module specified in RootModule/ModuleToProcess. This may also contain a PSData hashtable with additional module metadata used by PowerShell.
PrivateData = @{

    PSData = @{

         # Tags applied to this module. These help with module discovery in online galleries.
         Tags = @('OneIdentity', 'Safeguard', 'Powershell', 'CLI', 'DevOps')

         # A URL to the license for this module.
         LicenseUri = 'https://github.com/OneIdentity/SafeguardDevOpsService/blob/master/LICENSE'

         # A URL to the main website for this project.
         ProjectUri = 'https://github.com/OneIdentity/SafeguardDevOpsService'

         # A URL to an icon representing this module.
         IconUri = 'https://raw.githubusercontent.com/OneIdentity/SafeguardDevOpsService/master/SafeguardLogo.ico'

         # List of external modules that this module is dependent upon.
         ExternalModuleDependencies = $('safeguard-ps')

         # Pre-release tag
         Prerelease = '-pre'

         # ReleaseNotes of this module
         ReleaseNotes = @"
safeguard-devops 0.1 Release Notes:

 - initial beta version
"@

     } # End of PSData hashtable

} # End of PrivateData hashtable

# HelpInfo URI of this module
# HelpInfoURI = ''

# Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix.
# DefaultCommandPrefix = ''

}
