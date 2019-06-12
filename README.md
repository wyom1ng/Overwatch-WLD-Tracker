# Better Overwatch - A competitive match tracker
[![Build status](https://ci.appveyor.com/api/projects/status/94bxngssk039i80q?svg=true)](https://ci.appveyor.com/project/MartinNielsenDev/overwatchtracker)
[![Latest Release](https://img.shields.io/github/release/martinnielsendev/overwatchtracker.svg)](https://github.com/MartinNielsenDev/OverwatchTracker/releases)

Official website at http://betteroverwatch.com/

![better overwatch website](https://betteroverwatch.com/assets/images/website.png)
## What does it do?
My match tracker will seamlessly run as you play competitive Overwatch games and record your stats, such as..
* Current skill rating
* Eliminations
* Objective Eliminations
* Damage done
* Healing done
* Deaths
* Played heroes
* Final score

... and more

## What does it **not** do?
* Store or send any private information
* Store or send any images of your screen (aside from optional playerlist at the start of the game)
* Inject into the game
* Read or write memory to the game
* Send any inputs to the game

## How does it work?
The Tracker works by looking at the game screen and applying computer vision then passing it through trained neural networks to analyze and extract data from small parts of the screen.

All processing is done locally through this application, the server only displays the data that the tracker sends nicely for you.

## Examples
![games](http://betteroverwatch.com/assets/images/games.png)
![detailed-game](http://betteroverwatch.com/assets/images/detailed-game.png)

Live demo on my profile: http://betteroverwatch.com/user/Avoid
