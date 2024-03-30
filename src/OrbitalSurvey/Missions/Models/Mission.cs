using KSP.Game.Missions.Definitions;
using KSP.Game.Missions.State;
using OrbitalSurvey.Missions.Utility;
using OrbitalSurvey.Models;
using Random = UnityEngine.Random;

namespace OrbitalSurvey.Missions.Models;

public class Mission
{
    public Mission(MissionData missionData) => Preinitialize(missionData);
    
    public string Id;
    public string Name;
    public string Body;
    public string DiscoverableRegion;

    public bool IsInitialized;
    
    // for some reason KSP2 has separate instances for the definition and for the active mission
    // even though their definitions are nearly identical
    public MissionData DefinitionMissionData;
    public MissionData ActiveMissionData;

    public MissionState GetState() => ActiveMissionData?.state ?? MissionState.Inactive;

    public MainObjective MainObjective;

    public List<OptionalObjective> Objectives = new();

    private void Preinitialize(MissionData missionData)
    {
        DefinitionMissionData = missionData;
        Id = missionData.ID;
        Name = missionData.name;
        ParseDataFromId(missionData.ID, out Body, out DiscoverableRegion);
        MainObjective = new MainObjective(Body, DiscoverableRegion);
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
    
    public void Initialize()
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

        IsInitialized = true;
    }
    
}