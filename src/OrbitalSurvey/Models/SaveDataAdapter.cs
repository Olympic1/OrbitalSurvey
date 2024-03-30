using OrbitalSurvey.Missions.Models;
using UnityEngine;

namespace OrbitalSurvey.Models;

public class SaveDataAdapter
{
    public string SessionGuidString;
    public Vector3? WindowPosition;
    public Dictionary<string, Dictionary<MapType, MapsAdapter>> Bodies = new();
    public List<OrbitalSurveySerializedWaypoint> Waypoints = new();
    public Dictionary<string, List<MissionOptionalObjectiveAdapter>> Missions = new();

    public struct MapsAdapter
    {
        public string DiscoveredPixels;
        public bool IsFullyScanned;
        public ExperimentLevel ExperimentLevel;
    }

    public struct MissionOptionalObjectiveAdapter
    {
        public bool ContainsMainObjective;
        public double Latitude;
        public double Longitude;
        public int StageIndex;
    }
}