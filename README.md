
# MapsetChecksCatch
MapsetChecksCatch is like [MapsetChecks](https://github.com/Naxesss/MapsetChecks) a plugin for [MapsetVerifier](https://github.com/Naxesss/MapsetVerifier). This plugin is used to add extra checks for the gamemode osu!catch. The ObjectMapper was taken and enhanced from [CatchCheck](https://github.com/rorre/CatchCheck) so big props to [-Keitaro](https://osu.ppy.sh/users/3378391) for that!

> **Note:** It is still in heavy development so [false positives and false negatives](https://en.wikipedia.org/wiki/False_positives_and_false_negatives) may occur. If you found something that isn't right feel free to open an issue, pm me on [osu!](https://osu.ppy.sh/users/2369776) or dm me on discord.

## Available checks
**Unrankable checks:**
 - Spinner gap check
 - Edge dashes
 - Consecutive hyperdashes
 - Luminosity of combo colour

**Miner check:**
 - Max combo guideline
 - Spinner present

## Planned checks

 - Hyperdashes in Cups/Salads
 - Dashes in Cups
 - Consecutive edge dash usage in Rains/Overdoses
 - Hyperdash of different snap usage on Platters
 - Unallowed snaps on Salad/Platter/Rain
 - Difficulty guideline settings

## How to install

- Download the latest release of `MapsetChecksCatch.dll` (which can be found at the [release tab](https://github.com/Darius-Wattimena/MapsetChecksCatch/releases))
- Open your *Roaming* folder, you can find it with either:
	- Typing `%APPDATA%` in your file explorer's address bar.
	- Navigate to the folder yourself `C:/Users/<YOURNAME>/AppData/Roaming`.
- Open the `Mapset Verifier Externals` folder.
- Open the `checks` folder.
- Place the `MapsetChecksCatch.dll` file in this folder.
> **Note:** Do not get confused with the checks folder of Mapset Verifier itself. That folder is only used for official plugins not for thirdparty plugins like this one.
