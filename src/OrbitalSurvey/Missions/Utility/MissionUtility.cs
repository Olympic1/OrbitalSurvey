using KSP.Game;
using KSP.Game.Missions;
using KSP.Game.Missions.Definitions;
using KSP.Game.Science;

namespace OrbitalSurvey.Missions.Utility;

public static class MissionUtility
{
    public const string MISSION_ID_PREFIX = "OrbitalSurvey";

    public static KSP2MissionManager KSP2MissionManager => GameManager.Instance.Game?.KSP2MissionManager;

    public static List<MissionData> KSP2ActiveMissions => KSP2MissionManager.ActiveMissions?.FirstOrDefault()?.MissionDatas;
    
    public static Dictionary<string, CelestialBodyBakedDiscoverables> Discoverables => GameManager.Instance.Game?.ScienceManager?.ScienceRegionsDataProvider?._cbToScienceRegionDiscoverables;
}