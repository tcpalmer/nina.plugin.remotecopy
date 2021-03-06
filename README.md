# Remote Copy NINA Plugin

Remote Copy supports replication of your image files to another location or server while your sequence is running.  This can be useful to automatically copy the images to the server used for postprocessing, to support live stacking, or for long term archiving.

# Robocopy #
Robocopy is a file and directory replication tool provided with all Windows installations.  Robocopy can be configured (as it is here) to continuously monitor a source folder for changes and then replicate automatically.  Through the use of the 'Robocopy Start' and 'Robocopy Stop' sequence instructions, you can easily replicate your image files to a another location on the same system or a mapped drive on another server.

See the Windows [robocopy documentation](https://docs.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy) for more information.

## Robocopy Instructions ##
The plugin provides two instructions for robocopy, both in the 'Remote Copy' group:
* Robocopy Start.  You provide the source and destination folders and when executed, this instruction will start a robocopy process in the background to replicate files from the source to the destination.  Via the options, you can choose to display the command window (where you can watch progress) or not.  If the window isn't shown, log output will be written to robocopy-yyyyMMdd-HHmmss.log in the same location as the main NINA log.  You can also override the default options passed to robocopy.  *Consult the documentation and be sure you know what you're doing before changing this.*

* Robocopy Stop.  Insert this instruction at the end of your sequence to stop the background robocopy process.  You can set a wait time before the process will be stopped.  Since robocopy can only check for changes once per minute, it is important to give the last image file(s) time to replicate.  Ideally, this instruction would be included in the same parallel instruction set containing your other shutdown instructions like park scope and warm camera.

If you're replicating to a remote server, successful operation of course depends on having a decent network connection.  The default options will retry a failed copy up to two times but this plugin does not guarantee replication: at the end of your session, you are encouraged to verify that you have all files where you want them.

Other things to be aware of:
* The plugin watches the background robocopy process and if it detects that it has stopped, it will restart it.
* If you execute another Robocopy Start instruction before stopping an existing one, it will stop the current one before starting another.
* The plugin will only manage robocopy processes that it started so if you run robocopy for other needs, it shouldn't interfere.  However, it would probably be bad to have overlapping source/destination folders.
* If you forget to include the Robocopy Stop instruction, the plugin will attempt to stop the robocopy process when NINA ends - but it's best to explicitly stop it with the instruction.

# SCP #
Support for SCP (secure copy) might be added if there's sufficient interest.  This would enable replication to arbitrary remote systems (e.g. cloud storage) without requiring Windows shares.

## Getting Help
Ask for help in the #plugin-discussions channel on the NINA project [Discord server](https://discord.com/invite/rWRbVbw).

