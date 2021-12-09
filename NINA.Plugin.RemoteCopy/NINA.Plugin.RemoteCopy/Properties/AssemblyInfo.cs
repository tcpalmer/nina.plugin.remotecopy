using System.Reflection;
using System.Runtime.InteropServices;

[assembly: Guid("F35D0A07-E238-436C-A4BD-BAA4AA2A6255")]

[assembly: AssemblyTitle("Remote Copy")]
[assembly: AssemblyDescription("Copy acquired files to another location")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Tom Palmer")]
[assembly: AssemblyProduct("RemoteCopy.NINAPlugin")]
[assembly: AssemblyCopyright("Copyright © 2021")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: AssemblyVersion("0.0.0.1")]
[assembly: AssemblyFileVersion("0.0.0.1")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "2.0.0.2001")]

[assembly: AssemblyMetadata("License", "MPL-2.0")]
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
[assembly: AssemblyMetadata("Repository", "https://github.com/tcpalmer/nina.plugin.remotecopy/")]
[assembly: AssemblyMetadata("FeaturedImageURL", "https://raw.githubusercontent.com/tcpalmer/nina.plugin.remotecopy/main/NINA.Plugin.RemoteCopy/assets/remote-copy-logo.png?raw=true")]

[assembly: AssemblyMetadata("LongDescription", @"Remote Copy supports replication of your image files to another location or server while your sequence is running.  This can be useful to automatically copy the images to the server used for postprocessing, to support live stacking, or for long term archiving.

# Robocopy #
Robocopy is a file and directory replication tool provided with all Windows installations.  Robocopy can be configured (as it is here) to continuously monitor a source folder for changes and then replicate automatically.  Through the use of the 'Robocopy Start' and 'Robocopy Stop' sequence instructions, you can easily replicate your image files to a another location on the same system or a mapped drive on another server.

See the Windows [robocopy documentation](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy) for more information.

## Robocopy Instructions ##
The plugin provides two instructions for robocopy, both in the 'Remote Copy' group:
* Robocopy Start.  You provide the source and destination folders and when executed, this instruction will start a robocopy process in the background to replicate files from the source to the destination.  Via the options, you can choose to display the command window (where you can watch progress) or not.  If the window isn't shown, log output will be written to robocopy-yyyyMMdd-HHmmss.log in the same location as the main NINA log.  You can also override the default options passed to robocopy.  *Consult the documentation and be sure you know what you're doing before changing this.*

* Robocopy Stop.  Insert this instruction at the end of your sequence to stop the background robocopy process.  You can set a wait time before the process will be stopped.  Since robocopy can only check for changes once per minute, it is important to give the last image file(s) time to replicate - especially if your destination is non-local.  Ideally, this instruction would be included in the same parallel instruction set containing your other shutdown instructions like park scope and warm camera.

If you're replicating to a remote server, successful operation of course depends on having a decent network connection.  The default options will retry a failed copy up to two times but this plugin does not guarantee replication: at the end of your session, you are encouraged to verify that you have all files where you want them.

Other things to be aware of:
* The plugin watches the background robocopy process and if it detects that it has stopped, it will restart it.
* If you execute another Robocopy Start instruction before stopping an existing one, it will stop the current one before starting another.
* The plugin will only manage robocopy processes that it started so if you run robocopy for other needs, it shouldn't interfere.  However, it would probably be bad to have overlapping source/destination folders.
* If you forget to include the Robocopy Stop instruction, the plugin will attempt to stop the robocopy process when NINA ends - but it's best to explicitly stop it with the instruction.
* Robocopy has been tested with remote destinations like OneDrive and does work - just be sure to set a sufficiently large delay in your Robocopy Stop instruction to give it time to replicate your final image(s).
* If you see problems, it's likely because robocopy isn't happy for some reason.  Please test the robocopy command by itself outside of NINA before blaming the plugin.

# SCP #
Support for SCP (secure copy) might be added if there's sufficient interest.  This would enable replication to arbitrary remote systems (e.g. cloud storage) without requiring the destination to be a mounted file system.

# Getting Help #
* Ask for help in the #plugin-discussions channel on the NINA project [Discord server](https://discord.com/invite/rWRbVbw).
* [Source code](https://github.com/tcpalmer/nina.plugin.remotecopy)
* [Change log](https://github.com/tcpalmer/nina.plugin.remotecopy/blob/main/CHANGELOG.md)

Remote Copy is provided 'as is' under the terms of the [Mozilla Public License 2.0](https://github.com/tcpalmer/nina.plugin.sessionmetadata/blob/main/LICENSE.txt)
")]

[assembly: ComVisible(false)]
