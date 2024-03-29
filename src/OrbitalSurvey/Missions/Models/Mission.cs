using KSP.Game.Missions.Definitions;
using KSP.Game.Missions.State;
using OrbitalSurvey.Missions.Utility;
using OrbitalSurvey.Models;
using Random = UnityEngine.Random;

namespace OrbitalSurvey.Missions.Models;

public class Mission
{
    public Mission(MissionData missionData) => Initialize(missionData);
    
    // for some reason KSP2 has separate instances for the definition and for the active mission
    // even though their definitions are nearly identical
    public MissionData DefinitionMissionData;
    public MissionData ActiveMissionData;
    
    public string Id;
    public string Name;
    public string Body;
    public string DiscoverableRegion;

    public MissionState GetState() => ActiveMissionData?.state ?? MissionState.Inactive;
    /*
    {
        var missionManager = GameManager.Instance.Game?.KSP2MissionManager;
        if (missionManager == null || missionManager.ActiveMissions?.Count == 0)
        {
            return MissionState.Invalid;
        }
        
        // try to find the mission in ActiveMissions and return the state
        foreach (var activeMission in missionManager.ActiveMissions[0].MissionDatas)
        {
            if (activeMission.ID == Id)
            {
                return activeMission.state;
            }
        }

        // if the mission wasn't found in ActiveMissions then it's inactive
        return MissionState.Inactive;
    }
    */

    /*
    public OptionalObjective ObjectiveA;
    public OptionalObjective ObjectiveB;
    public OptionalObjective ObjectiveC;
    */

    public MainObjective MainObjective;

    public List<OptionalObjective> Objectives = new();

    private void Initialize(MissionData missionData)
    {
        DefinitionMissionData = missionData;
        Id = missionData.ID;
        Name = missionData.name;
        ParseDataFromId(missionData.ID, out Body, out DiscoverableRegion);
        MainObjective = new MainObjective(Body, DiscoverableRegion);

        BuildOptionalObjectives();

        // TODO build optional objectives
        // TODO build main objective
    }
    
    private void ParseDataFromId(string id, out string body, out string region)
    {
        var data = id.Split('_');
        if (data.Length != 3)
        {
            throw new ArgumentException(
                "Error parsing Body and Region data from Mission ID. " +
                $"Expected format is '{MissionUtility.MISSION_ID_PREFIX}_Body_DiscoverableScienceRegion'");
        }

        body = data[1];
        region = data[2];
    }
    
    private void BuildOptionalObjectives()
    {
        int mainObjectiveAreaNumber = Random.Range(0, 3);

        var geoCoordinatesToAvoid = new List<(double latitude, double longitude)>();
        geoCoordinatesToAvoid.Add((MainObjective.Latitude, MainObjective.Longitude));

        for (int i = 0; i < 3; i++)
        {
            var objective = new OptionalObjective();
            
            if (i == mainObjectiveAreaNumber)
            {
                // create optional objective that contains the main objective
                objective.CreateForMainObjective(MainObjective, LocalizationStrings.MISSION_AREA_NAMES[i+1],i * 2);
            }
            else
            {
                // create optional objective
                objective.CreateRegularOptionalObjective(geoCoordinatesToAvoid, LocalizationStrings.MISSION_AREA_NAMES[i+1], i * 2);
            }
            
            geoCoordinatesToAvoid.Add((objective.Latitude, objective.Longitude));
                
            Objectives.Add(objective);
        }
    }
    
}