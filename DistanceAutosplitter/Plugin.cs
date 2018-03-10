﻿//using Spectrum;
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
        bool inLoad = false;
        bool paused = false;
        bool started = false;
        bool justFinished = false;

        string category = "Adventure";

        Socket livesplitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        string firstLevel;
        string lastLevel;
        string[] noReset;

        public void Initialize(IManager manager)
        {
            if (category == "Adventure")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "Credits";
                noReset = new string[0];
            }
            else if (category == "Sprint SS")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "The Manor";
                noReset = new string[0];
            }
            else if (category == "Challenge SS")
            {
                firstLevel = "Dodge";
                lastLevel = "Elevation";
                noReset = new string[0];
            }
            else if (category == "All Arcade Levels")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "Elevation";
                noReset = new string[]{ "The Manor" };
            }
            else if (category == "All Levels")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "Elevation";
                noReset = new string[]{ "Credits", "The Manor" };
            }

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
                if (!started && Game.LevelName == firstLevel)
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
                if (started)
                {
                    totalElapsedTime += Race.ElapsedTime;
                    SendData($"setgametime {totalElapsedTime.TotalSeconds}");
                    if (args.Type == RaceEndType.Finished)
                    {
                        SendData("split");
                    }
                    SendData("pausegametime");
                    inLoad = true;
                    if (Game.LevelName == lastLevel)
                    {
                        totalElapsedTime = new TimeSpan();
                        started = false;
                        justFinished = true;
                    }
                    else if (Array.Exists(noReset, levelName => levelName == Game.LevelName))
                    {
                        justFinished = true;
                    }
                }
            };

            Race.Started += (sender, args) =>
            {
                if (Game.LevelName == firstLevel)
                {
                    SendData("starttimer");
                    started = true;
                }
                else if (started)
                {
                    SendData("unpausegametime");
                }
                inLoad = false;
            };

            MainMenu.Loaded += (sender, args) =>
            {
                if (started && !justFinished)
                {
                    totalElapsedTime = new TimeSpan();
                    started = false;
                    inLoad = false;
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
            if (!inLoad && livesplitSocket.Connected)
            {
                TimeSpan elapsedTime = totalElapsedTime + Race.ElapsedTime;

                if (G.Sys.GameManager_.PauseMenuOpen_)
                {
                    if (!paused)
                    {
                        SendData($"setgametime {elapsedTime.TotalSeconds}");
                        SendData("pausegametime");
                        paused = true;
                    }
                }
                else
                {
                    if (paused)
                    {
                        SendData("unpausegametime");
                        paused = false;
                    }
                }
                
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
