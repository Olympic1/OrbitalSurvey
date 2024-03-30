using Assets.Scripts.Missions.Definitions;
using KSP.Game;
using OrbitalSurvey.Managers;
using OrbitalSurvey.Missions.Models;
using OrbitalSurvey.Missions.Utility;
using OrbitalSurvey.Models;
using UnityEngine;

namespace OrbitalSurvey.Missions.Managers;

public class MissionManager : ManagerBase<MissionManager>
{
    public Dictionary<string, List<Mission>> Missions = new();
    public Dictionary<string, Mission> ActiveMissions = new();

    public bool ReadyForMissionLoading;
    
    public override void Initialize()
    {
        Logger.LogInfo("Spice must flow");
        
        Missions.Clear();
        ActiveMissions.Clear();
        
        InitializeMissions();
        InitializeActiveMissions();
        InitializeMissionGranter();

        ReadyForMissionLoading = true;
    }

    private void InitializeMissions()
    {
        foreach (var missionData in GameManager.Instance.Game?.KSP2MissionManager?._missionDefinitions)
        {
            if (missionData.ID.StartsWith(MissionUtility.MISSION_ID_PREFIX))
            {
                var newMission = new Mission(missionData);

                if (Missions.ContainsKey(newMission.Body))
                {
                    Missions[newMission.Body].Add(newMission);
                }
                else
                {
                    Missions.Add(newMission.Body, new List<Mission> { newMission });
                }
            }
        }
    }

    private void InitializeActiveMissions()
    {
        var allOrbitalSurveyMissions = Missions.Values.SelectMany(list => list).ToList();

        var activeMissions = MissionUtility.KSP2ActiveMissions;
        if (activeMissions == null)
            return;
        
        foreach (var activeKsp2Mission in activeMissions)
        {
            foreach (var mission in allOrbitalSurveyMissions)
            {
                if (activeKsp2Mission.ID == mission.Id)
                {
                    mission.ActiveMissionData = activeKsp2Mission;
                    ActiveMissions.Add(mission.Body, mission);
                    allOrbitalSurveyMissions.Remove(mission);
                    break;
                }
            }
        }
    }
    
    private void InitializeMissionGranter()
    {
        var granters = GameManager.Instance.Game.KSP2MissionManager.MissionGranterManager.MissionGranters;

        if (granters.Find(g => g.NameKey == "OrbitalSurvey") == null)
        {
            var granter = ScriptableObject.CreateInstance<MissionGranter>();
            granter.NameKey = "OrbitalSurvey";
            granter.LogoKey = "Assets/Images/Icons/icon.png";
            granter.LocalizationNameKey = LocalizationStrings.MISSIONS_GRANTER_NAME;
            granter.LocalizationDescriptionKey = LocalizationStrings.MISSIONS_GRANTER_DESCRIPTION;
            granters.Add(granter);
        }
    }
}