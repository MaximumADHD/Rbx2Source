# ![rbx2source](Resources/Images/smallRbx2SourceLogo.png)
### Port Roblox models to Source Engine Games
##
[![Hits](https://hits.seeyoufarm.com/api/count/incr/badge.svg?url=https%3A%2F%2Fgithub.com%2FStarLandRBLX%2FRbx2Source&count_bg=%2379C83D&title_bg=%23555555&icon=&icon_color=%23E7E7E7&title=hits&edge_flat=false)](https://hits.seeyoufarm.com)[![Repo Size](https://img.shields.io/github/repo-size/LockpickInteractive/Rbx2Source)](https://hits.seeyoufarm.com)[![Build Rbx2Source](https://github.com/LockpickInteractive/Rbx2Source/actions/workflows/build.yml/badge.svg)](https://github.com/LockpickInteractive/Rbx2Source/actions/workflows/build.yml)![CodeQL](https://github.com/LockpickInteractive/Rbx2Source/actions/workflows/codeql.yml/badge.svg)![Dependabot](https://img.shields.io/badge/dependabot-025E8C?style=flat&logo=dependabot&logoColor=white)![.Net](https://img.shields.io/badge/.NET-5C2D91?style=flat&logo=.net&logoColor=white)![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=flat&logo=c-sharp&logoColor=white)![Windows](https://img.shields.io/badge/Windows-0078D6?style=flat&logo=windows&logoColor=white)[<img src="https://discordapp.com/api/guilds/787797824557154344/widget.png?style=shield">](https://discord.gg/b9MUKXF88p)
                       
Originally developed by [MaximumADHD](https://github.com/MaximumADHD), This project serves as a continuation of the original [Rbx2Source](https://github.com/MaximumADHD/Rbx2Source) Project.

# Setup
- Download [.NET 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) if you don't have it already
- Download the latest [Rbx2Source.exe](https://github.com/LockpickInteractive/Rbx2Source/raw/main/Rbx2Source.exe) file from the github page. 
- Alternatively, download the latest build artifact from the last commit.      
- If you're having issues, try running the [Cache Clearer](https://github.com/LockpickInteractive/Rbx2Source/raw/main/Clear%20Cache.bat).  If that doesn't fix anything, create a github issue.

# Building Instructions
- Clone the github repository.
- Clone [the Dependency repository](https://github.com/MaximumADHD/Roblox-File-Format/) as well, in the same directory that you cloned Rbx2Source in
- Restore NuGet Packages for Both repositories
- Build within Visual Studio 2022 (Debug / Release)

# Known Issues
- Accessories and Gear currently error out when compiling. see [Issue #17](https://github.com/LockpickInteractive/Rbx2Source/issues/17)
- Having layered clothing on your avatar will cause the compilier to error out. Layered clothing is *not* a planned feature at the moment.

# Contributors
<a href="https://github.com/LockpickInteractive/Rbx2Source/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=LockpickInteractive/Rbx2Source" />
</a>
