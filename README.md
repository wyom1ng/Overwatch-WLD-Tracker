# Overwatch-WLD-Tracker

## Introduction

My win/loss/draw tracker works automatically by utilizing computer vision. It is still a WIP but the core functionality works.

Right now... 

it can:

* automatically determine the outcome of your game and write it to a .txt file
* Format it's output to your desires
* read your in game stats such as
	* Eliminations
	* Deaths
	* Healing
	* etc.

it can't:
* register anything on a resolution different to 1920*1080
* manually adjust the counter in case it messes up

## How to use

![Tray Menu](https://i.imgur.com/GemFCdy.png)

The application opens in tray. There are only a few options, most of which are self explanatory.

![Format Menu](https://i.imgur.com/vubUVP6.png)

The format option allows you to freely design your tracker.

![Tool Tip](https://i.imgur.com/yqKpjjo.png)

You can find a tip as to how to use it by hovering over the "Set Output Format" label.

![Idle](https://i.imgur.com/MTMWhy5.png)
This Icon means that Overwatch isn't running. Start it to continue

![SR](https://i.imgur.com/BLeP1MU.png)
This Icon means that the Tracker doesn't know your SR. Go to the Play menu to continue.

![Ready](https://i.imgur.com/ASFvzR1.png)
This Icon means it found your SR and is ready to go.

![Recording](https://i.imgur.com/FhSFRbT.png)
This icon means that the tracker realized you're in game. For this to work you have to be tabbed in to the game when it starts.

![OBS](https://i.imgur.com/v6uLEaw.png)

In OBS add a Text (GDI+) Source and check "Read from file" in properties to add the counter.

## Credits

On the foundation of Avoid's [OverwatchTracker](https://github.com/MartinNielsenDev/OverwatchTracker)
