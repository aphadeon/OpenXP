[![license](https://img.shields.io/github/license/aphadeon/OpenXP.svg)]()
[![GitHub issues](https://img.shields.io/github/issues/aphadeon/OpenXP.svg)]()
[![GitHub last commit](https://img.shields.io/github/last-commit/aphadeon/OpenXP.svg)]()

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
Encrypted RPG Maker archives (.rgssad) will not be supported.  While the format is publicly known 
at this point, the copyright holders for RPG Maker XP has directly expressed that it is 
against their wishes for the encryption details to be investigated or made public.  New (better) 
forms of encryption will be made available for this editor at a later time.

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
MIT License
Copyright 2017 aphadeon & Glitchfinder

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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