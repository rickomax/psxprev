# psxprev
PSXPREV - Playstation (PSX) File Previewer/Extractor<br><br>
PsxPrev, a TMD/PMD/TIM PlayStation file scanner.<br><br>
This tool uses:<br>
GLMNet - https://github.com/dwmkerr/glmnet<br>
SharpGL - https://github.com/dwmkerr/sharpgl<br>
Daniele de Santis Playstation Icons - http://www.danieledesantis.net/<br><br>
You'll need to run it:<br>
An OpenGL 2.0 compatible video card<br>
.NET Framework 4.5<br>
Treat it as an experimental release and use this tool as your own risk!<br><br>
The tool can use a big ammount of memory to scan the files, so, be careful.<br>

Usage:<br>
PSXPrev folder [filter] [-tmd] [-pmd] [-tim] [-retim] [-tod] [-hmdmodels] [-log] [-noverbose]<br>
<ul>
<li>folder - folder where are the files to be scanned (recursively scan)</li>
<li>filter - use this parameter to specify a filter of files to be scanned (eg: *.tod)</li>
<li>tmd - add this parameter to scan for tmd models</li>
<li>pmd - add this parameter to scan for pmd models</li>
<li>tim - add this parameter to scan for tim textures</li>
<li>retim - add this parameter to scan for resident evil specific tim textures</li>
<li>tod - add this paramete rto scan for tod animations</li>
<li>hmdmodels - add this parameter to scan for hmd models (wip)</li>
<li>log - add this parameter to generate a scanning log text file</li>
<li>noverbose - add this parameter to dont show log on the console window</li>
</ul>

A console window will appear, the application will scan each byte of the files inside the folder/filter you specified trying to find the files types you specified, so, it can take a bit of time for this process to end.<br><br>
After the scan has been completed, a new window will open, with the tabs:<br>
Models: This is where you see the models that has been found on the scan<br>
Textures: This is where you see the textures that has been found on the scan<br>
VRAM Preview: This is where you have a replica of PSX Video RAM, which consist of 32 256x256 textures. You can use these pages to compose the final textures applied to the models.<br>
Animations: This is a working in progress panel where you have animation list and controls, and a field to assign a loded entity to the animation preview.<br><br>
Usage tips:<br>
To Apply a Texture to a Model, first you have to look at the VRAM Page property of the Model you want to apply the Texture.After that have to draw the textures (at the VRAM Tab, called Draw to VRAM) on the desired VRAM Page (which is pre-defined in the Texture Properties area). After that, if you come back to the Model Tab, you may see your Model textured, if it has Uvs property enabled (True).<br>
To Export a Model or Multiple Models,  tick their checkboxes at the Model Tab, click at Export Selected button, select the desired Output Format and the Output Folder. (Textures and materials will be automatically exported)<br>
To Export a Texture or Multiple Textures, select them at the Texture Tab, click at Export Selected button and select the desired Output Folder.<br><br>
Known issues/limitations:<br>
The two 3D exportable file formats has disadvantages, .OBJ files cannot have vertex color information, and .PLY files will group all the sub-models in one single model. I'm looking for a better format for the exporter. An experimental .OBJ exporter option with Vertex Color is available.<br>
The tool will only find files that are explicitly conformant to <br>
the file formats it's looking for. Any compressed file cannot be scanned.
