# MapsetChecksCatch

MapsetChecksCatch is a plugin for [MapsetVerifier](https://github.com/Naxesss/MapsetVerifier). This plugin is used to add extra checks for the gamemode osu!catch. The initial code for the BeatmapDistanceCalculator was taken from [CatchCheck](https://github.com/rorre/CatchCheck) so big props to [-Keitaro](https://osu.ppy.sh/users/3378391) for making this!

This adds support for all osu!catch rules specified on the [Ranking Criteria page](https://osu.ppy.sh/wiki/en/Ranking_criteria/osu!catch).

> [!WARNING]
> This plugin is still in development, so [false positives and false negatives](https://en.wikipedia.org/wiki/False_positives_and_false_negatives) may occur. If you find something that isn't right, feel free to open an issue, pm me on [osu!](https://osu.ppy.sh/users/2369776) or dm me on Discord.

## Installing

1. Download the latest release of `MapsetChecksCatch.dll` (which can be found at the [release tab](https://github.com/Darius-Wattimena/MapsetChecksCatch/releases))
2. Open MV and click the settings icon (top right)
3. Scroll down to the `Shortcuts` section
4. Click the `Open externals folder` icon
5. Open the `checks` folder
6. Place the `MapsetChecksCatch.dll` file in this folder.

## Known issues

- Checks where we do something with dashes as detection gets funky with very low and very high BPM.

## How to run locally

### Requirements

- Have [.NET 8 or higher](https://dotnet.microsoft.com/en-us/download/dotnet) installed.
- Have [MapsetVerifier 1.9.0 or later](https://github.com/Naxesss/MapsetVerifier) installed.

### First set up

1. Clone this repostiory 
2. Clone [MapsetVerifier](https://github.com/Naxesss/MapsetVerifier) and make sure the two projects are in the same root.
   For example a layout like this:
   ```
   C:/Coding
   │	
   └───OsuProjects
       └───MapsetVerifier
       └───MapsetChecksCatch
   ```
3. Open the MapsetVerifier project and build it once. This can be done by for example executing `dotnet build src` in the terminal. This is needed as we need a `MapsetVerifier.dll` in order to build the MapsetChecksCatch project.
4. Go back to the MapsetChecksCatch project and execute the `./local-build.bat` script. This takes care of building the project and moving the file to the `\Mapset Verifier Externals\checks` folder in AppData.
	- Alternatively you can run `dotnet build ./MapsetChecksCatch.sln` and move the result that is in `\bin\Debug\net8.0\MapsetChecksCatch.dll` to the external checks folder by hand.

### Making changes

1. Do your code changes
2. Close MV
3. Build plugin using `./local-build.bat`
4. Re-open MV and test out your changes.
