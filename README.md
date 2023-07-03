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
- Treat it as an experimental release and use this tool at your own risk.

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
<https://drive.google.com/file/d/1V39J5RNcZLID6WNnw7ZB98tMwoksHjGS/view?usp=sharing>

Known issues/limitations:
PSXPrev only find files conformant to the file formats it's looking for. PSXPrev can't scan any compressed or proprietary format.
