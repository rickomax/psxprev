# PSXPrev

PSXPREV - Playstation (PSX) Files Previewer/Extractor

![Sample](https://i.snipboard.io/hLlNy5.jpg)

PSXPrev uses:
- OpenTK (https://github.com/opentk/opentk)
- DiscUtils (https://github.com/DiscUtils/DiscUtils)
- Daniele de Santis Playstation Icons (http://www.danieledesantis.net/)

To run PSXPrev, you need:
- An OpenGL 3.0 compatible video card
- .NET Framework 4.8

Treat it as an experimental release and use this tool at your own risk.

Scanned file formats: TMD, PMD, TIM, HMD, TOD, VDF, and more.
Warning: PSXPrev uses a lot of memory to scan files, so your system might run out of resources while scanning.

Usage:
A launcher interface will be displayed when running the application without passing any parameters.
When passing parameters, PSXPrev will start scanning for files using the given parameters.
After the scan has been completed, a new window will be displayed, containing the following tabs:
- Models:  Models found while scanning files.
- Textures: Textures found while scanning files.
- VRAM Preview: This is where you have a replica of PSX Video RAM, which consist of 32 256x256 textures.(You can use these pages to compose the final textures applied to the models)
- Animations: Animations found while scanning files.

Video tutorial:
<https://www.youtube.com/watch?v=hPDa8l3ZE6U&feature=youtu.be>

Compatibility list:
<https://docs.google.com/spreadsheets/d/155pUzwl7CC14ssT0PJkaEA53CS1ijpOV04VitQCVBC4>

Known issues/limitations:
PSXPrev only find files conformant to the file formats it's looking for. PSXPrev can't scan any compressed or proprietary format.

Command line usage:
```
usage: PSXPrev <PATH> [FILTER="*.*"] [-help] [-an] [-bff] [-croc]
               [-hmd] [-pmd] [-psx] [-tim] [-tmd] [-tod] [-vdf]
               [-ignoretmdversion] [-log] [-noverbose] [-debug]
               [-selectmodel] [-drawvram] [-attachlimbs]

arguments:
  PATH   : folder or file path to scan
  FILTER : wildcard filter for files to include (default: "*.*")

scanner options: (default: all formats)
  -an    : scan for AN animations
  -bff   : scan for BFF models
  -croc  : scan for CROC models
  -hmd   : scan for HMD models, textures, and animations
  -pmd   : scan for PMD models
  -psx   : scan for PSX models
  -tim   : scan for TIM textures
  -tmd   : scan for TMD models
  -tod   : scan for TOD animations
  -vdf   : scan for VDF animations
  -ignoretmdversion : reduce strictness when scanning TMD models

log options:
  -log       : write output to log file
  -noverbose : reduce output to console and file
  -debug     : output file format details and other information

program options:
  -selectmodel : select and display the first-loaded model
  -drawvram    : draw all loaded textures to VRAM (not advised when scanning a lot of files)
  -attachlimbs : enable Auto Attach Limbs by default
```
