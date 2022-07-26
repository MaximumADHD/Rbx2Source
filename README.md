# ![rbx2source](Resources/Images/smallRbx2SourceLogo.png)
### Port Roblox models to Source Engine Games
##
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FStarLandRBLX%2FRbx2Source&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)    
Originally developed by [MaximumADHD](https://github.com/MaximumADHD), this project serves as a continuation of the original [Rbx2Source](https://github.com/MaximumADHD/Rbx2Source) Project. So far, we've gotten the program to open and it compiles. Model doesn't seem to compile completely as of right now but hopefully it will be fixed soon

# Setup
- Download the latest [Rbx2Source.exe](https://github.com/StarLandRBLX/Rbx2Source/raw/main/Rbx2Source.exe) file from the github page.       
- If your Source Games are on a drive other than C:, you must create a config.txt file next to Rbx2Source.
- [Here's an example of a good config.txt file](https://github.com/StarLandRBLX/Rbx2Source/raw/main/config.example.txt)
- If you're having issues after the first compile, run the [Cache Clearer](https://github.com/StarLandRBLX/Rbx2Source/raw/main/Clear%20Cache.bat).  

# Building Instructions
- Clone the github repository. (With submodules)
- You may have to edit the Roblox-File-Format submodule in Visual Studio to change it's output directory (you'll see the path it's looking for in a build error)
- You may have to restore the NuGet Packages as well
- Build

# Known Bugs
- Not loading thumbnails of users/accessories [issue #1](https://github.com/StarLandRBLX/Rbx2Source/issues/1)
