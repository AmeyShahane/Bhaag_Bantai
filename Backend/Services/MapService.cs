using Backend.Models;
using System;
using System.IO;

namespace Backend.Services
{
    public class MapService
    {
        private readonly Map _map;
        private readonly ILogger<MapService> _logger;

        public MapService(ILogger<MapService> logger) 
        {
            _map = new Map();
            _logger = logger;

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectRoot = Path.Combine(baseDirectory, "..", "..", "..");
            string stationPath = Path.Combine(projectRoot, "Models", "Stations.txt");
            string connectionsPath = Path.Combine(projectRoot, "Models", "Map.txt");


            if (File.Exists(stationPath))
            {
                string[] lines = File.ReadAllLines(stationPath);

                foreach (string line in lines)
                {
                    
                    string[] parts = line.Split(' ');
                    _map.Nodes[parts[0]] = new Node 
                                            { 
                                              Label = parts[0], 
                                              XCoordinate = int.Parse(parts[1]), 
                                              YCoordinate = int.Parse(parts[2]) 
                                            };

                }
            }
            else
            {
                Console.WriteLine();
                _logger.LogError("Stations file not found.\n\n");
            }

            if (File.Exists(connectionsPath))
            {
                string[] lines = File.ReadAllLines(connectionsPath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(' ');
                    if (parts[2].Equals("water")) 
                    {
                        _map.Nodes[parts[0]].FerryConnections.Add(_map.Nodes[parts[1]]);
                        _map.Nodes[parts[1]].FerryConnections.Add(_map.Nodes[parts[0]]);
                    }
                    else if (parts[2].Equals("underground"))
                    {
                        _map.Nodes[parts[0]].LocalConnections.Add(_map.Nodes[parts[1]]);
                        _map.Nodes[parts[1]].LocalConnections.Add(_map.Nodes[parts[0]]);
                    }
                    else if (parts[2].Equals("bus"))
                    {
                        _map.Nodes[parts[0]].BusConnections.Add(_map.Nodes[parts[1]]);
                        _map.Nodes[parts[1]].BusConnections.Add(_map.Nodes[parts[0]]);
                    }
                    else if (parts[2].Equals("taxi"))
                    {
                        _map.Nodes[parts[0]].RikshawConnections.Add(_map.Nodes[parts[1]]);
                        _map.Nodes[parts[1]].RikshawConnections.Add(_map.Nodes[parts[0]]);
                    }
                }
            }
            else
            {
                Console.WriteLine("Map file not found.");
            }
        }
        public Map GetMap()
        {
            return _map;
        }
    }
}
