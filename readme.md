# Distance Spectrum Autosplitter
This is a Spectrum plugin that provides automatic splits and an IGT timer for Distance speedruns through
LiveSplit Server.

This is very early in development. I haven't actually used this in a full run yet, so I don't know if
everything works properly (it should). There's no option to disable it yet, so until I get that in, I am
currently locking it to only work in Adventure mode. I want to have it automatically reset if you return to
the main menu, but I haven't figured out how to detect that yet, so you'll have to manually reset for now.

**Note**: It is not yet decided whether or not using Spectrum will be allowed for runs, so this might be useless.
We'll see. 

## How to use
First, install the LiveSplit Server component: https://github.com/LiveSplit/LiveSplit.Server/releases

After you've done that, you need to add the LiveSplit Server component to your layout. Then right-click on
LiveSplit, go into the Control submenu, and select Start Sever. Note that you must do this every time you
start LiveSplit, and you should do this before you open the game. The plugin connects to the server as
soon as it's loaded, so if you start the server after you open the game, it will silently fail (there's no
debugging info in -console yet).

Note: If you want to use RTA timing, you will have to start the timer manually; do not use the automatic
start for timing runs in real time, it is not accurate to the timing method actually used. You won't have
to worry about anything if you use IGT, though, apart from the fact there is no game time field on the sr.c
boards yet. So you probably won't want to actually use that part of the autosplitter yet.

You then need to install Distance Autosplitter plugin. Grab the latest .dll from the
[releases page](https://github.com/TntMatthew/DistanceSpectrumAutosplitter/releases), and drop
it in your `Distance_Data/Spectrum/Plugins` directory. And you're done - the timer will
automatically start once IGT begins, and will split after each level is completed.