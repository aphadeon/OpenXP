# ![logo](../Tools/logo.png) OpenXP Runtime
New Game.exe features:
 * Only Debug builds will enable runtime $DEBUG flag.  Release builds ignore the related 
commandline parameter.
 * 'debug' or 'test' as a commandline parameter will enable debug features, including the 
$DEBUG and $TEST flags, but only in Debug builds of the runtime.
 * "-game test" as a commandline parameter would look for INI and all subsequent data under 
the specified folder.

New Game.ini features (all new features are optional and will be defaulted if not present):
 * Game.ini can be named either Game.ini, or the same name as the executable. In other words, if 
the executable is renamed to MyGame.exe, it will look for MyGame.ini .  
 * Comments are supported and preserved. You can begin a comment with ";", "#", or "//". All 
characters beyond this comment marker on a line are ignored.  
 * The file is now writeable by the runtime itself.  Settings can be changed at runtime, and the 
changes will be saved to file.   
 * Several new settings are supported.  
 * [OXP] UseAppData=true will tell the engine to look for the INI under %AppData%/{Exe Name}/.  
 * [OXP] AppDataFolder=Test will override the AppData subdirectory, i.e. %AppData%/Test/.  
 * UseAppData will also use this as the base location for all file writes from script, e.g. save 
data.  It will also be used as the first place to look for loading, before falling back to the 
application directory, and finally falling through to the RTPs, in order from 1 to 3.  
 * [Logging] LoggingEnabled=true will allow the game to save log files.  
 * [Logging] LogLevel=0 will specify how much information to save. 0: Verbose, 1: Info, 2: 
Warning, 3: Error, 4: Fatal.  It will log the specified level and above.  
 * [Logging] LogCount=3 will specify how many log files to save. 3 would save log0.txt (latest), 
log1.txt, and log2.txt .  Logs older than log2.txt would be deleted in this scenario.  
 * [Logging] LogPath=Logs would specify to store log files in the Logs directory.  
 * [Graphics] Width=640 specifies the window width in clientarea pixels at creation.  
 * [Graphics] Height=480 specifies the window height in clientarea pixels at creation.