# PSXPrev

PSXPREV - Playstation (PSX) Files Previewer/Extractor<br><br>
![Sample](https://i.snipboard.io/hLlNy5.jpg)<br><br>
This tool uses:<br>
- OpenTK (https://github.com/opentk/opentk)<br>
- DiscUtils (https://github.com/DiscUtils/DiscUtils)<br>
- Daniele de Santis Playstation Icons (http://www.danieledesantis.net/)<br><br>
To run PSXPrev, you need:<br>
- An OpenGL 3.0 compatible video card<br>
- .NET Framework 4.8<br>
- Treat it as an experimental release and use this tool at your own risk.<br><br>
Scanned file formats: TMD, PMD, TIM, HMD, TOD, VDF, and more.<br>
Warning: PSXPrev uses a lot of memory to scan files, so your system might run out of resources while scanning.<br><br>
Usage:<br>
A launcher interface will be displayed when running the application without passing any parameters.<br>
When passing parameters, PSXPrev will start scanning for files using the given parameters.<br><br>
After the scan has been completed, a new window will be displayed, containing the following tabs:<br>
- Models:  Models found while scanning files.<br>
- Textures: Textures found while scanning files.<br>
- VRAM Preview: This is where you have a replica of PSX Video RAM, which consist of 32 256x256 textures.<br>(You can use these pages to compose the final textures applied to the models)<br>
- Animations: Animations found while scanning files.<br><br>
Video tutorial:<br>
<https://www.youtube.com/watch?v=hPDa8l3ZE6U&feature=youtu.be><br><br>
Compatibility list:<br>
<https://drive.google.com/file/d/1V39J5RNcZLID6WNnw7ZB98tMwoksHjGS/view?usp=sharing><br><br>
Known issues/limitations:<br>
PSXPrev only find files conformant to the file formats it's looking for. PSXPrev can't scan any compressed or proprietary format.<br><br>
