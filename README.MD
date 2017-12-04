# ![logo](Tools/logo.png) OpenXP
An open-source 2D top-down RPG editor and engine, which aims to support RPG Maker XP projects. 
It uses Ruby for scripting, and will make attempts to maintain compatibility with existing third 
party RPG Maker XP scripts.

While not an exact clone, it should be close enough to be comfortable, and more importantly, to 
*just work*.  There will be additional enhancements and features beyond the original capabilities 
of the RPG Maker XP engine.

**OpenXP is currently in early development.  It is not recommended for production use at this time.**


Status
--------
At this point we are focusing on interoperability and compatibility with RPG Maker XP projects, both 
via the editor and a runtime implementation.

The runtime is partially derived from the previous project OpenGame.exe, but limited to RPG Maker XP 
support, and focuses on enabling enhancements moreso than absolute perfect emulation (but existing 
third-party script compatibility will still be taken into consideration when reasonable).

More enhanced features will be added in the future.


Cross-Platform
--------------
OpenXP's runtime intends to officially support Windows, Linux, and Mac.  It is not yet ready for use 
on non-Windows platforms.  The editor may be made cross platform, this is yet to be determined.


Changes from Vanilla
--------------------
Editor:  
No RGSSAD Encryption support.  This is by design. We will eventually offer a new encryption method.  
More coming.


Limitations
-----------
You must own RPG Maker XP in order to use the OpenXP editor, in accordance with the details 
of this project's license (see License below for more information).  I strongly respect the intellectual 
property rights of others, and while we are using a clean-room approach to implementation, I have no 
desire to compromise the value or integrity of their product.

Encrypted RPG Maker archives (.rgssad) will not be supported.  While the format is publicly known 
at this point, the copyright holders for RPG Maker XP has directly expressed that it is 
against their wishes for the encryption details to be investigated or made public.  New forms of 
encryption will be made available for this editor at a later time.

Win32API is not supported.  A plugin system is planned that will allow you to extend many facets 
of both the editor and the runtime, and additionally you have access to the source code to tailor 
the engine to your needs.  A cross-platform runtime is a high priority with this project, and most 
existing Win32API-dependent scripts will likely not work as intended due to a different backend.


Building from source
--------------------
We also use NuGet to manage dependencies- as long as you have package resolution enabled, 
this process should run automatically and install the required libraries.

Some dependencies are bundled in the repository, where custom built options were preferable. 


Contributions
------------
Please see the [Contribution Guidelines and Credits List](CONTRIBUTING.MD).  :eyes:

Thanks to the [hbgames.org community](http://www.hbgames.org/) for helping me learn the RPG Maker 
series over the years.  

Thanks to vgvgf and the HBGames community for demonstrating how to pack and unpack Table, Color, 
and Tone classes.  

This project is based on [OpenGame.exe](https://github.com/aphadeon/OpenGame.exe). 
Special thanks to all the contributors of OpenGame.exe for their help.

License
-------
```
OpenXP License
Copyright 2017 aphadeon & Glitchfinder

You must own RPG Maker XP to use the OpenXP editor in binary form to produce software. With this 
condition met, you may freely use, copy, modify, publish, merge, distribute, sell, or sublicense 
copies of the software for any purpose, subject to the conditions below.

You must give appropriate credit to all contributors (see CONTRIBUTING.MD), indicate if changes 
were made, and provide a copy of or link to this license with all substantial portions of the 
software. You may do so in any reasonable manner which does not suggest the licensor(s) endorses 
you or your use.

All software is provided "as is", without warranty of any kind, express or implied, including but 
not limited to the warranties of merchantability, fitness for a particular purpose and 
noninfringement. In no event shall the authors, contributors, or copyright holders be liable for 
any claim, damages, or other liability, whether in an action of contract, tort or otherwise, 
arising from, out of or in connection with the software or the use or other dealings in the
software.
```
The *Dependencies* subdirectory is exempted from this license entirely, dependencies are subject 
to their own license, as indicated below.

The *Documentation* subdirectory is exempted from this license entirely. The license terms of the 
template apply as usual, and any contents are to be considered public domain.

Dependency License Information:
-------------------------------

[IronRuby](http://ironruby.net/) 
is licensed under [Apache License v2](http://www.apache.org/licenses/LICENSE-2.0).  

[Ionic.Zlib](https://github.com/jstedfast/Ionic.Zlib) 
is licensed under [ZLIB License](https://github.com/jstedfast/Ionic.Zlib/blob/master/License.zlib.txt).  

[ScintillaNET](https://github.com/jacobslusser/ScintillaNET) 
is licensed under [MIT License](https://github.com/jacobslusser/ScintillaNET/blob/master/LICENSE).  

[Scintilla](http://www.scintilla.org/) 
is licensed under [MIT License](http://www.scintilla.org/License.txt).  