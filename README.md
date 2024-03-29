# PSXPrev

PSXPREV - PlayStation (PSX) Files Previewer/Extractor

[![Release](https://img.shields.io/github/v/release/rickomax/psxprev
)](https://github.com/rickomax/psxprev/releases/latest)
[![Release Date](https://img.shields.io/github/release-date-pre/rickomax/psxprev)](https://github.com/rickomax/psxprev/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/rickomax/psxprev/total
)](https://github.com/rickomax/psxprev/releases)
[![Discord](https://img.shields.io/discord/1126965151011184660.svg?style=flat&logo=discord&label=Discord&colorB=7389DC&link=https://discord.gg/Haan9wVdKB)](https://discord.gg/Haan9wVdKB)

---
![Program Preview][Preview Action Man Skeleton]

(Models from Action Man 2: Destruction X)

<details><summary>Old preview images</summary>

---
##### Beta 0.9.9: Models from Planet DOB
![Program Preview][Preview Planet DOB DOB]

##### Alpha 0.9.8.4: Models from Planet DOB
![Program Preview][Preview Planet DOB cast]

---
##### Alpha 0.9.7.1: Models from PsyQ demo files
![Program Preview][Preview Space Shuttle]

</details>

<!-- Preview image markdown reference links -->
[Preview Space Shuttle]: <https://github.com/rickomax/psxprev/assets/9752430/20a11609-d75f-49ce-ad6c-84d458802082>
[Preview Planet DOB cast]: <https://github.com/rickomax/psxprev/assets/12863984/3070bf42-33f1-42b1-b09f-48386390f397>
[Preview Planet DOB DOB]: <https://github.com/rickomax/psxprev/assets/9752430/ba2da3bd-cfca-441b-9bcd-dc695a645530>
[Preview Action Man Skeleton]: <https://github.com/rickomax/psxprev/assets/9752430/d87d0160-b6c1-4ff7-9e18-ce403d71e91b>

---

Treat this as an experimental release and use this tool at your own risk. Warning: PSXPrev uses a lot of memory to scan files, so your system might run out of resources while scanning.

**Links:** [Video tutorial](https://www.youtube.com/watch?v=hPDa8l3ZE6U) \| [Compatibility list](https://docs.google.com/spreadsheets/d/155pUzwl7CC14ssT0PJkaEA53CS1ijpOV04VitQCVBC4)

**Credits:**
- 3D rendering with [OpenTK](https://github.com/opentk/opentk)
- ISO reading with [DiscUtils](https://github.com/DiscUtils/DiscUtils)
- JSON reading/writing with [Newtonsoft.Json](https://www.newtonsoft.com/json)
- Ozgur Ozcitak's [ImageListView](https://github.com/oozcitak/imagelistview)
- Daniele de Santis's [Playstation Icons](https://www.behance.net/gallery/26021809/Playstation-icons-%28free-download%29)
- Yusuke Kamiyamane's [Fugue Icons](https://p.yusukekamiyamane.com)

<!-- Daniele de Santis's personal site: <http://www.danieledesantis.net/> -->

**Runtime Requirements:**
- An OpenGL 3.0 compatible video card
- .NET Framework 4.5.1

---

**Supported formats:**
- Models: TMD, PMD, HMD, BFF (Blitz Games), PIL (Blitz Games), MOD (Croc), PSX (Neversoft)
- Textures: TIM, HMD, SPT (Blitz Games), PSX (Neversoft)
- Animations: AN, TOD, VDF, HMD, PIL (Blitz Games), PSX (Neversoft)

**Usage:**
A scanner window will be displayed when running the application without passing any command line arguments.
When passing command line arguments, PSXPrev will start scanning for files using the given parameters.
As the scan runs, a new window will be displayed, containing the following tabs:
- Models: Viewer for models found while scanning files.
- Textures: Viewer for textures found while scanning files.
- VRAM: Viewer for a replica of PSX Video RAM, which consists of 32 256x256 textures. (These pages are used to compose the final textures applied to models)
- Animations: Player for animations found while scanning files.

To scan more files, you can select **File** &gt; **Start Scan** to open the scanner window again.

**Known issues/limitations:**
PSXPrev only finds files conformant to the file formats it's scanning for. PSXPrev can't scan any compressed or proprietary formats.

**Command line usage:**
```
usage: PSXPrev <PATH> [FILTER="*"] [-help] [...options]

arguments:
  PATH   : folder or file path to scan
  FILTER : wildcard filter for files to include (default: "*")

scanner formats: (default: all formats except SPT)
  -an        : scan for AN animations
  -bff       : scan for BFF models and animations (Blitz Games)
  -hmd       : scan for HMD models, textures, and animations
  -mod/-croc : scan for MOD models (Croc)
  -pil       : scan for PIL models and animations (Blitz Games)
  -pmd       : scan for PMD models
  -psx       : scan for PSX models, textures, and animations (Neversoft)
  -spt       : scan for SPT textures (Blitz Games)
  -tim       : scan for TIM textures
  -tmd       : scan for TMD models
  -tod       : scan for TOD animations
  -vdf       : scan for VDF animations

scanner options:
  -ignorehmdversion     : less strict scanning of HMD models
  -ignorepmdversion     : less strict scanning of PMD models
  -ignoretimversion     : less strict scanning of TIM textures
  -ignoretmdversion     : less strict scanning of TMD models
  -align <ALIGN>        : scan offsets at specified increments
  -start <OFFSET>       : scan files starting at offset (hex)
  -stop  <OFFSET>       : scan files up to offset (hex, exclusive)
  -range [START],[STOP] : shorthand for [-start <START>] [-stop <STOP>]
  -startonly  : shorthand for -stop <START+1>
  -nextoffset : continue scan at end of previous match
  -regex      : treat FILTER as Regular Expression
  -depthlast  : scan files at lower folder depths first
  -syncscan   : disable multi-threaded scanning per format
  -scaniso    : scan individual files inside .iso files
  -scanbin    : scan individual files inside raw PS1 .bin files
                not all files may be listed in a .bin file, use -databin as a fallback
  -databin    : scan data contents of raw PS1 .bin files
  -binsector <START>,<SIZE> : change sector reading of .bin files (default: 24,2048)
                              combined values must not exceed 2352

log options:
  -log       : write output to log file
  -debug     : output file format details and other information
  -error     : show error (exception) messages when reading files
  -noverbose/-quiet : don't write output to console

program options:
  -drawvram  : draw all loaded textures to VRAM (not advised when scanning many files)
```
