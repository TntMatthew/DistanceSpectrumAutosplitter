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
        bool started = false;
        bool justFinished = false;

        Socket livesplitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public void Initialize(IManager manager)
        {
            try
            {
                livesplitSocket.Connect("localhost", 16834);
                livesplitSocket.Send(Encoding.UTF8.GetBytes($"initgametime\r\n"));
                Console.WriteLine("Connected successfully.");
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Failed to connect to the LiveSplit server: {e.Message}");
            }

            Race.Loaded += (sender, args) =>
            {
                if (Game.CurrentMode == Spectrum.Interop.Game.GameMode.Adventure && !started && Game.LevelName == "Broken Symmetry")
                {
                    if (!livesplitSocket.Connected)
                    {
                        try
                        {
                            livesplitSocket.Connect("localhost", 16834);
                            livesplitSocket.Send(Encoding.UTF8.GetBytes($"initgametime\r\n"));
                            Console.WriteLine("Connected successfully.");
                        }
                        catch (SocketException e)
                        {
                            Console.WriteLine($"Failed to connect to the LiveSplit server: {e.Message}");
                        }
                    }
                }
            };

            LocalVehicle.Finished += (sender, args) =>
            {
                if (Game.CurrentMode == Spectrum.Interop.Game.GameMode.Adventure && started)
                {
                    totalElapsedTime += Race.ElapsedTime;
                    SendData($"setgametime {totalElapsedTime.TotalSeconds}");
                    SendData("split");
                    SendData("pausegametime\r\n");
                    countingTime = false;
                    if (Game.LevelName == "Credits")
                    {
                        totalElapsedTime = new TimeSpan();
                        started = false;
                        justFinished = true;
                    }
                }
            };

            Race.Started += (sender, args) =>
            {
                if (Game.CurrentMode == Spectrum.Interop.Game.GameMode.Adventure)
                {
                    if (Game.LevelName == "Broken Symmetry")
                    {
                        SendData("starttimer");
                        started = true;
                    }
                    else if (started)
                    {
                        SendData("unpausegametime");
                    }
                }
                countingTime = true;
            };

            MainMenu.Loaded += (sender, args) =>
            {
                if (started && !justFinished)
                {
                    totalElapsedTime = new TimeSpan();
                    started = false;
                    countingTime = false;
                    SendData("reset");
                }
                else
                {
                    justFinished = false;
                }
            };
        }

        public void Update()
        {
            if (countingTime && livesplitSocket.Connected)
            {
                TimeSpan countingTime = totalElapsedTime + Race.ElapsedTime;
                SendData($"setgametime {countingTime.TotalSeconds}");
            }
        }

        public void Shutdown()
        {

        }
   
        void SendData(string command)
        {
            if (livesplitSocket.Connected)
            {
                try
                {
                    livesplitSocket.Send(Encoding.UTF8.GetBytes($"{command}\r\n"));
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Failed to send data to LiveSplit server: {e.Message}");
                    Console.WriteLine("Disconnecting...");
                    livesplitSocket.Disconnect(false);
                    livesplitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
            }
        }
    }
}
