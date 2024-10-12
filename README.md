# CobbleBuild

A C# port tool focused on automating the process of porting over assets and data into minecraft bedrock edition. Imports assets to the [main repo](https://github.com/Incoherent-Code/Cobblemon-Bedrock) for this project.

## Build Instructions

### Visual Studio
1. Open the csproj file in Visual Studio
2. Clone [this repo](https://github.com/Incoherent-Code/Cobblemon-Bedrock) into any folder.
3. Go to Debug > CobbleBuild Debug Properties
4. Ensure that the working directory is set as the repo you just cloned.
5. Build and run the project.

### Without Visual Studio
1. Ensure that dotnet is installed on your system.
2. Run `dotnet build` in the root of the project.
3. Clone [this repo](https://github.com/Incoherent-Code/Cobblemon-Bedrock) into a nearby folder.
4. Modify the cbconfig.json file. "minecraftJavaPath" needs to be supplied with a path to the minecraft jar (1.20.1 as of writing). If you have this version installed on windows, it should be found. Otherwise, point it to a valid jar file.
The appropriate version of cobblemon's source code should automatically download.
5. Execute the built application in the root of that repo's folder. `dotnet run ./path/to/CobbleBuild/bin/Release/net8.0/CobbleBuild.dll`

## Contributing
- Follow the C# specification, even if I don't half of the time.
- Some of the code in this repo ain't great. I've gotten alot better at C#, but this project started nearly a year before I open sourced the project. Pull requests with constructive critcism are appreciated. 