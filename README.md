# CobbleBuild

A C# port tool focused on automating the process of porting over assets and data into minecraft bedrock edition. 

# Build Instructions
1. Ensure that dotnet is installed on your system.
2. Run `dotnet build` in the root of the project.
3. Clone [this repo](https://github.com/Incoherent-Code/Cobblemon-Bedrock) into a nearby folder.
4. Modify the cbconfig.json file. The fields `minecratJavaPath` and `cobblemonPath` need to point to the minecraft java 1.20.1 jar and the cobblemon 1.4.1 source code respectively.
5. Execute the built application in the root of that repo's folder. `dotnet run ./path/to/CobbleBuild/bin/Release/net8.0/CobbleBuild.dll`

# Contributing
- Follow the C# specification, even if I don't half of the time.
- Some of the code in this repo ain't great. I've gotten alot better at C#, but this project started nearly a year before I open sourced the project. Pull requests with constructive critcism are appreciated. 