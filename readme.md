# Distance Spectrum Autosplitter
This is a Spectrum plugin that provides automatic splits, resets, and an IGT timer for Distance speedruns
through LiveSplit Server.

This is very early in development. I haven't actually used this in a full run yet, so I don't know if
everything works properly (though it should). This supports quite a few categories. It is set to Adventure by
default. If you want to change the category, you can edit the config file in `Distance_Data/Spectrum/Settings`.

The current available categories are Adventure, Sprint SS, Challenge SS, All Arcade Levels, and All Levels.
If the `category` key in the aformentioned config file is not any one of those, it will use the default
(which is Adventure).

If you want to play the game without having the timer split a bunch on you, just stop the server in LiveSplit.
It should handle it just fine.

**Note**: It is not yet decided whether or not using Spectrum will be allowed for runs, so this might be useless.
We'll see. 

## How to use
First, install the LiveSplit Server component: https://github.com/LiveSplit/LiveSplit.Server/releases

After you've done that, you need to add the LiveSplit Server component to your layout. Then right-click on
LiveSplit, go into the Control submenu, and select Start Sever. Note that you must do this every time you
start LiveSplit. The plugin will attempt to connect to the server immediately once loaded - if it fails,
it will re-attempt just before you start a run.

You then need to install Distance Autosplitter plugin. Grab the latest .dll from the
[releases page](https://github.com/TntMatthew/DistanceSpectrumAutosplitter/releases), and drop
it in your `Distance_Data/Spectrum/Plugins` directory. And you're done. The timer will autostart
once you select the first level from the levelselect, and will split after each level is completed.