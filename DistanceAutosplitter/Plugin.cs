//using Spectrum;
using Spectrum.API;
using Spectrum.Interop.Game;
using Spectrum.Interop.Game.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using System;
using System.Net.Sockets;
using System.Text;

namespace DistanceAutosplitter
{
    public class Entry : IPlugin, IUpdatable
    {
        public string FriendlyName => "Distance Autosplitter";
        public string Author => "Matthew Tavendale";
        public string Contact => "@TntMatthew#3201 on Discord";
        public APILevel CompatibleAPILevel => APILevel.XRay;

        TimeSpan totalElapsedTime = new TimeSpan();
        bool countingTime = false;

        Socket livesplitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Initialize(IManager manager)
        {
            livesplitSocket.Connect("localhost", 16834);
            livesplitSocket.Send(Encoding.UTF8.GetBytes($"initgametime\r\n"));

            LocalVehicle.Finished += (sender, args) =>
            {
                if (Game.CurrentMode == Spectrum.Interop.Game.GameMode.Adventure)
                {
                    totalElapsedTime += Race.ElapsedTime;
                    livesplitSocket.Send(Encoding.UTF8.GetBytes($"setgametime {totalElapsedTime.TotalSeconds}\r\n"));
                    livesplitSocket.Send(Encoding.UTF8.GetBytes("split\r\n"));
                    livesplitSocket.Send(Encoding.UTF8.GetBytes("pausegametime\r\n"));
                    countingTime = false;
                    if (Game.LevelName == "Credits")
                    {
                        totalElapsedTime = new TimeSpan();
                    }
                }
            };

            Race.Started += (sender, args) =>
            {
                if (Game.CurrentMode == Spectrum.Interop.Game.GameMode.Adventure)
                {
                    Console.WriteLine("started");
                    Console.WriteLine(Game.LevelName);
                    if (Game.LevelName == "Broken Symmetry")
                    {
                        livesplitSocket.Send(Encoding.UTF8.GetBytes("starttimer\r\n"));
                    }
                    else
                    {
                        livesplitSocket.Send(Encoding.UTF8.GetBytes("unpausegametime\r\n"));
                    }
                }
                countingTime = true;
            };

        }

        public void Update()
        {
            if (countingTime)
            {
                TimeSpan countingTime = totalElapsedTime + Race.ElapsedTime;
                livesplitSocket.Send(Encoding.UTF8.GetBytes($"setgametime {countingTime.TotalSeconds}\r\n"));
            }
        }

        public void Shutdown()
        {

        }
    }
}
