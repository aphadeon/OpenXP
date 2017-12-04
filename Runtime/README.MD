# ![logo](../Tools/logo.png) OpenXP Runtime
New Game.exe features:
 * OpenXP Runtime requires, and makes use of, OpenGL 4.0.  
 * Only Debug builds will enable runtime $DEBUG flag.  Release builds ignore the related 
commandline parameter.
 * 'debug' or 'test' as a commandline parameter will enable debug features, including the 
$DEBUG and $TEST flags, but only in Debug builds of the runtime.
 * "-game test" as a commandline parameter would look for INI and all subsequent data under 
the specified **test** folder.

New Game.ini features (all new features are optional and will be defaulted if not present):
 * Game.ini can be named either Game.ini, or the same name as the executable. In other words, if 
the executable is renamed to MyGame.exe, it will look for MyGame.ini .  
 * Comments are supported and preserved. You can begin a comment with ";", "#", or "//". All 
characters beyond this comment marker on a line are ignored.  
 * The file is now writeable by the runtime itself.  Settings can be changed at runtime, and the 
changes will be saved to file.   
 * Several new settings are supported.  
 * [OXP] UseAppData=true will tell the engine to look for the INI under %AppData%/{Exe Name}/. 
Recommended for production to avoid issues with unwriteable game directories, caused by UAC or 
similar.  
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
 * [Graphics] KeepAspectRatio=true would cause black bars to be added when the window is resized 
or you switch to fullscreen, rather than stretching to fill the clientarea.  
 * [Graphics] Fullscreen=true would start the game in fullscreen. This setting is saved based on 
fullscreen status at last exit.  
 * [Graphics] BorderlessFullscreen=true would tell the game to run in borderless fullscreen mode, 
as opposed to temporarily changing the display's resolution to match the game's.  
 * [Graphics] Borderless=true would tell the game to run without a border in windowed mode. Not 
recommended unless you have implemented your own window management.  
 * [Graphics] UpdateRate=60 would tell the game to update up to 60 times per second by default.  
 * [Graphics] FrameRate=60 would tell the game to refresh the display up to 60 times per second.  
 * [Graphics] BaseScale=2 would tell the game to draw the client at double resolution.  
 * [Graphics] AllowResize=true would tell the game to allow resizing the client window.  
 * [Graphics] LocationX=0 stores the last X location of the window. Defaults to centered. Used to 
restore location on startup.  
 * [Graphics] LocationY=0 stores the last Y location of the window. Defaults to centered. Used to 
restore location on startup.  