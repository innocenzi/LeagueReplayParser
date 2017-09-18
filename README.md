# League Replay Parser

This is a library which allows you to easily parse a League of Legend replay file. Those files are default-located in your documents under `League of Legends/Replays` folder and have the `.rofl` (**R**eplay **of** **L**eague) extention.

# Getting started

[![GitHub release](https://img.shields.io/github/release/hawezo/LeagueReplayParser.svg?style=flat-square)](https://github.com/hawezo/LeagueReplayParser)
[![GitHub issues](https://img.shields.io/github/issues/hawezo/LeagueReplayParser.svg?style=flat-square)](https://github.com/hawezo/LeagueReplayParser/issues)
[![Github downloads](https://img.shields.io/github/downloads/hawezo/LeagueReplayParser/total.svg?style=flat-square)](https://github.com/hawezo/LeagueReplayParser)
&nbsp;
[![NuGet](https://img.shields.io/nuget/v/Hawezo.LeagueTools.LeagueReplayParser.svg?style=flat-square)](https://www.nuget.org/packages/Hawezo.LeagueTools.LeagueReplayParser)
[![NuGet downloads](https://img.shields.io/nuget/dt/Hawezo.LeagueTools.LeagueReplayParser.svg?style=flat-square)](https://www.nuget.org/packages/Hawezo.LeagueTools.LeagueReplayParser)

## Installation

You will first need to install the library by either [downloading it](https://github.com/hawezo/LeagueReplayParser/archive/master.zip) or [installing it from NuGet](https://www.nuget.org/packages/Hawezo.LeagueTools.LeagueReplayParser).

## Usage

At first, you will need to deal with the League object. It is the object that will give you informations about the installed League of Legends version and which will allow you to start a replay.

### League

#### Initialization

First, instanciate it: `League league = new League(@"C:\Riot Games");`. It will check if the given repertory exists, and if not, it will throw an `IOException`.

You may overload `League` constructor with the replay directory: `new League("@C:\Riot Games", @"D:\League\Replays");`. By default, your replay directory is in your documents and the library will find it.

Also, `League` contains properties `DefaultRiotDirectory` and `DefaultReplaysDirectory` which contains the default paths for the game and the replays.

#### Loading replays

Once `League` is initialized, you will need to load the replays. It is recommended to load them asynchronously just in case you have a slow machine and the parsing take long enough to freeze an eventual UI.

```csharp
// Async
await league.LoadReplaysAsync();

// Sync 
league.LoadReplays();
``` 

You can now access all available replays by using `league.Replays` property.

#### Starting replays

After getting your `Replay` object, you will be able to start it thanks to your `League` instance.

The following code will start all the playable replays in a row.
```csharp
foreach (Replay replay in league.Replays)
  if (replay.CanBePlayed.GetDefaultOrValue(true))
    await league.StartReplayAsync(replay);
``` 

The *nullable bool* `CanBePlayed` property will return `true` if your League of Legends' version is equal (or lower but this should not happen, right?) to the replay's League version, but will return `false` if not or `null` if you never instanciated any `League` object.

In a simplier way, you can start a replay file by just using `league.StartReplayAsync(@"C:\League\Replays\yourReplay.rofl");`.

### Replay

The `Replay` object contains some data about your replay file, such as informations about the two teams, their champions, their stuff, level, and other stuff.

```csharp
// Display purple team informations
foreach (Player player in replay.PurpleTeam.Players)
{
    Console.WriteLine();
    Console.WriteLine($"› {player.PlayerName} as {player.ChampionName}:");
    Console.WriteLine($"   Level: {player.Level}");
    Console.WriteLine($"   KDA: {player.KDA.Kills}/{player.KDA.Deaths}/{player.KDA.Assistances} ({player.KDA.Ratio})");
    Console.WriteLine($"   Lane: {player.Lane.ToString()}");
    Console.WriteLine($"   Minion score: {player.MinionScore}");
}
``` 

### Example code

As an example, this code will display some data from the first available replay file as well as the stats from the wining team.

```csharp
League league = new League(League.DefaultRiotDirectory);
  await league.LoadReplaysAsync(Encoding.UTF8);

  Console.WriteLine($"League version: {league.Version}");
  Console.WriteLine($"Found {league.Replays.Count} replays.");

  if (league.Replays.Count > 0)
  {
      Replay replay = league.Replays[0];
      Console.WriteLine($"Looking out first replay.");
      Console.WriteLine($"League version: {replay.GameVersion.ToString()}");
      Console.WriteLine($"Game duration: {replay.GameLength.ToString()}");
      Console.WriteLine($"Winning team: {replay.WiningTeam.Side.ToString()}");

      foreach (Player player in replay.WiningTeam.Players)
      {
          Console.WriteLine();
          Console.WriteLine($"+ {player.PlayerName} as {player.ChampionName}:");
          Console.WriteLine($"   Level: {player.Level}");
          Console.WriteLine($"   KDA: {player.KDA.Kills}/{player.KDA.Deaths}/{player.KDA.Assistances} ({player.KDA.Ratio})");
          Console.WriteLine($"   Lane: {player.Lane}");
          Console.WriteLine($"   Minion score: {player.MinionScore}");
      }

  }
```

## To-do

- [ ] Instead of having only one replay directory, add multiple ones.
- [ ] Get more stats and data from the replay file (I'll do that if you ask me).
