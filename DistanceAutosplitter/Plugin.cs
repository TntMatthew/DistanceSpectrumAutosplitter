//using Spectrum;
using Spectrum.API;
using Spectrum.Interop.Game;
using Spectrum.Interop.Game.Vehicle;
using Spectrum.API.Interfaces.Plugins;
using Spectrum.API.Interfaces.Systems;
using Spectrum.API.Configuration;
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
        Settings settings = new Settings(typeof(Entry));

        TimeSpan totalElapsedTime = new TimeSpan();
        bool inLoad = false;
        bool paused = false;
        bool started = false;
        bool justFinished = false;

        string category = "Adventure";

        Socket livesplitSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        string firstLevel = "Broken Symmetry";
        string lastLevel = "Credits";
        string[] requiresMenuing = new string[0];

        public void Initialize(IManager manager)
        {
            if (!settings.ContainsKey("category"))
            {
                settings.Add("category", "Adventure");
                settings.Save();
            }
            else
            {
                category = settings.GetItem<string>("category");
            }

            if (category == "Adventure")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "Credits";
                requiresMenuing = new string[0];
            }
            else if (category == "Sprint SS")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "The Manor";
                requiresMenuing = new string[0];
            }
            else if (category == "Challenge SS")
            {
                firstLevel = "Dodge";
                lastLevel = "Elevation";
                requiresMenuing = new string[0];
            }
            else if (category == "All Arcade Levels")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "Elevation";
                requiresMenuing = new string[]{ "The Manor" };
            }
            else if (category == "All Levels")
            {
                firstLevel = "Broken Symmetry";
                lastLevel = "Elevation";
                requiresMenuing = new string[]{ "Credits", "The Manor" };
            }

            AttemptLivesplitConnection();

            Events.Scene.BeginSceneSwitchFadeOut.Subscribe(data =>
            {
                if (!started && data.sceneName_ == "GameMode" && G.Sys.GameManager_.NextLevelName_ == firstLevel)
                {
                    if (!livesplitSocket.Connected)
                    {
                        if (AttemptLivesplitConnection() == false)
                        {
                            return;
                        }
                    }
                    SendData("starttimer");
                    SendData("pausegametime");
                    started = true;
                    inLoad = true;
                }
            });

            Race.Loaded += (sender, args) =>
            {
                if (livesplitSocket.Connected && started)
                {
                    SendData("getcurrenttimerphase");
                    byte[] responseBytes = new byte[256];
                    livesplitSocket.Receive(responseBytes);
                    string responseString = Encoding.UTF8.GetString(responseBytes);
                    if (responseString.StartsWith("NotRunning"))
                    {
                        totalElapsedTime = new TimeSpan();
                        started = false;
                    }
                }
            };

            Race.Started += (sender, args) =>
            {
                if (started)
                {
                    SendData("unpausegametime");
                }
                inLoad = false;
            };

            LocalVehicle.Finished += (sender, args) =>
            {
                if (started)
                {
                    var segmentTime = TimeSpan.FromMilliseconds(args.FinalTime);
                    totalElapsedTime += segmentTime;
                    Console.WriteLine($"Completed {Game.LevelName} with a segment time of {segmentTime.ToString()}");
                    Console.WriteLine($"Total time: {totalElapsedTime.ToString()}");
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
                    else if (Array.Exists(requiresMenuing, levelName => levelName == Game.LevelName))
                    {
                        justFinished = true;
                    }
                }
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
                        Console.WriteLine($"Game paused. Current segment: {Race.ElapsedTime.ToString()}\nTotal time: {elapsedTime.ToString()}\n");
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
   
        bool AttemptLivesplitConnection()
        {
            try
            {
                livesplitSocket.Connect("localhost", 16834);
                livesplitSocket.Send(Encoding.UTF8.GetBytes($"initgametime\r\n"));
                Console.WriteLine("Connected successfully.");
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Failed to connect to the LiveSplit server: {e.Message}");
                return false;
            }
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
