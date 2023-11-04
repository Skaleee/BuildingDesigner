# BuildingDesigner
![Startmenu](https://github.com/Skaleee/BuildingDesigner/assets/78816681/9deb7041-571a-4397-935a-9abf0d53b05d)

![Bearbeitungsansicht_options](https://github.com/Skaleee/BuildingDesigner/assets/78816681/5abdcc99-f57a-421c-8903-fd12c9d2999c)

This is a free building-design application implemented in Unity. 

The application works with german development plans and shows the user, if the current design complies with the restrictions of the plan. Not all restrictions are supported by the application.
The designed scene can be exported as a project file for later editing or as FBX-file for use in other software.

The application was tested on Windows but might also compile for other platforms, if they are supported by the used libraries.

# How to build:
1. Clone this project.
2. Open the project folder with the Unity-Editor. The project was made in Editor-Version __2022.3.0f1__. Newer versions might also work.
3. In the Editor, go to File->Build Settings... and then build the application for the chosen platform.

# Libraries used:
[ASCII FBX Exporter for Unity](https://github.com/KellanHiggins/AsciiFBXExporterForUnity) 

[Ear-Clipping-Triangulation](https://github.com/SebLague/Ear-Clipping-Triangulation)

[Unity Standalone File Browser](https://github.com/gkngkc/UnityStandaloneFileBrowser)
