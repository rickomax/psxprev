# psxprev - async version
PSXPREV - Playstation (PSX) File Previewer/Extractor<br><br>
PsxPrev, a TMD/PMD/TIM PlayStation file scanner.<br><br>
![Sample](https://ricardoreis.net/wp-content/uploads/2017/05/lod2.png)
This tool uses:<br>
OpenTK - https://github.com/opentk/opentk<br>
DiscUtils - https://github.com/DiscUtils/DiscUtils<br>
Daniele de Santis Playstation Icons - http://www.danieledesantis.net/<br><br>
You'll need to run it:<br>
An OpenGL 3.0 compatible video card<br>
.NET Framework 4.5<br>
Treat it as an experimental release and use this tool as your own risk!<br>
(The tool can use a big ammount of memory to scan the files, so, be careful.)<br>
Usage:<br>
PSXPrev folder/iso [filter] [-tmd] [-tmdAlt] [-pmd] [-tim] [-timAlt] [-tod] [-hmdmodels] [-log] [-noverbose]<br>
<ul>
<li>folder/iso - folder where are the files to be scanned or an ISO file path to scan the ISO contents</li>
<li>filter - use this parameter to specify a filter of files to be scanned (eg: *.tod)</li>
<li>tmd - add this parameter to scan for tmd models</li>
<li>tmdAlt - add this parameter to scan for tmd models using an alternative parser</li>
<li>pmd - add this parameter to scan for pmd models</li>
<li>tim - add this parameter to scan for tim textures</li>
<li>timAlt - add this parameter to scan for tim textures using an alternative parser</li>
<li>tod - add this paramete rto scan for tod animations</li>
<li>hmdmodels - add this parameter to scan for hmd models (wip)</li>
<li>log - add this parameter to generate a scanning log text file</li>
<li>noverbose - add this parameter to dont show log on the console window</li>
</ul>
If you don't pass any parameter, a launcher interface will be displayed.
Otherwise, a console window will appear, the application will scan each byte of the files inside the folder/filter you specified trying to find the files types you specified, so, it can take a bit of time for this process to end.<br><br>
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
The tool will only find files that are explicitly conformant to the file formats it's looking for. Any compressed file cannot be scanned.<br><br>
<b>License (BSD License 2.0):</b><br>
Copyright (c) 2018, Ricardo Reis<br>
All rights reserved.<br><br>
Redistribution and use in source and binary forms, with or without<br>
modification, are permitted provided that the following conditions are met:<br>
    * Redistributions of source code must retain the above copyright<br>
      notice, this list of conditions and the following disclaimer.<br>
    * Redistributions in binary form must reproduce the above copyright<br>
      notice, this list of conditions and the following disclaimer in the<br>
      documentation and/or other materials provided with the distribution.<br>
    * Neither the name of the <organization> nor the<br>
      names of its contributors may be used to endorse or promote products<br>
      derived from this software without specific prior written permission.<br><br>
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND<br>
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED<br>
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE<br>
DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY<br>
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES<br>
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;<br>
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND<br>
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT<br>
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS<br>
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.<br>
