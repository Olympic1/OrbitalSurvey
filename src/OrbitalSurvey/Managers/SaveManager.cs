using BepInEx.Logging;
using KSP.Game;
using OrbitalSurvey.Missions.Managers;
using OrbitalSurvey.Missions.Models;
using OrbitalSurvey.Models;
using OrbitalSurvey.UI;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using SpaceWarp.API.SaveGameManager;

namespace OrbitalSurvey.Managers;

public class SaveManager
{
    private SaveManager() { }
    
    public static SaveManager Instance { get; } = new();
    
    public SaveDataAdapter bufferedLoadData;
    public bool HasBufferedLoadData;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.SaveManager");

    public void Register()
    {
        ModSaves.RegisterSaveLoadGameData<SaveDataAdapter>(
            OrbitalSurveyPlugin.ModGuid,
            OnSave,
            OnLoad
        );
    }

    public void OnSave(SaveDataAdapter dataToSave)
    {
        _LOGGER.LogDebug("OnSave triggered.");

        dataToSave.WindowPosition = SceneController.Instance.WindowPosition;
        dataToSave.SessionGuidString = Utility.SessionGuidString;
        dataToSave.Bodies.Clear();

        // save map data
        foreach (var celestialData in Core.Instance.CelestialDataDictionary)
        {
            if (!celestialData.Value.ContainsData)
                continue;
            
            var mapsDataAdapter = new Dictionary<MapType, SaveDataAdapter.MapsAdapter>();
            
            foreach (var mapData in celestialData.Value.Maps) 
            { 
                if (!mapData.Value.HasData) 
                    continue;

                var mapsAdapter = new SaveDataAdapter.MapsAdapter();
                if (mapData.Value.IsFullyScanned)
                {
                    mapsAdapter.IsFullyScanned = true;
                    mapsAdapter.DiscoveredPixels = string.Empty;
                }
                else
                {
                    mapsAdapter.IsFullyScanned = false;
                    mapsAdapter.DiscoveredPixels = SaveUtility.CompressData(mapData.Value.DiscoveredPixels);
                }

                mapsAdapter.ExperimentLevel = mapData.Value.ExperimentLevel;
                
                mapsDataAdapter.Add(mapData.Key, mapsAdapter);
            }
            
            dataToSave.Bodies.Add(celestialData.Key, mapsDataAdapter);
            
            _LOGGER.LogDebug($"{celestialData.Key} prepared for saving.");
        }
        
        // save waypoints
        dataToSave.Waypoints.Clear();
        foreach (var waypointModel in SceneController.Instance.Waypoints)
        {
            dataToSave.Waypoints.Add(waypointModel.Waypoint.Serialize());
        }
        
        // save missions
        dataToSave.Missions.Clear();
        foreach (var mission in MissionManager.Instance.ActiveMissions.Values)
        { 
            var objectives = new List<SaveDataAdapter.MissionOptionalObjectiveAdapter>();
            foreach (var objective in mission.Objectives)
            {
                objectives.Add(new SaveDataAdapter.MissionOptionalObjectiveAdapter
                {
                    ContainsMainObjective = objective.ContainsMainObjective,
                    Latitude = objective.Latitude,
                    Longitude = objective.Longitude,
                    StageIndex = objective.StageIndex
                });
            }
            dataToSave.Missions.Add(mission.Id, objectives);
        }
    }

    public void OnLoad(SaveDataAdapter dataToLoad)
    {
        _LOGGER.LogDebug("OnLoad triggered.");

        bufferedLoadData = dataToLoad;
        HasBufferedLoadData = true;
        
        // skip loading if Maps haven't been initialized yet. Initialization will call LoadData();
        //if (!Core.Instance.MapsInitialized)
        //    return;
        
        //#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        LoadData();
        //#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
    }

    private async void LoadData()
    {
        // await for Core initialization and MissionManager initialization
        while (!Core.Instance.MapsInitialized || !MissionManager.Instance.ReadyForMissionLoading)
        {
            await Task.Delay(500);
        }
        
        LoadMapsData();
        await LoadWaypointData();
        LoadMissionData();
        
        SceneController.Instance.WindowPosition = bufferedLoadData.WindowPosition;
        Core.Instance.SessionGuidString = bufferedLoadData.SessionGuidString;

        bufferedLoadData = null;
        HasBufferedLoadData = false;
        
        VesselManager.Instance.SetLastRefreshTimeToNow();
    }

    private void LoadMapsData()
    {
        // mapping data
        foreach (var celestialData in Core.Instance.CelestialDataDictionary)
        {
            if (!bufferedLoadData.Bodies.ContainsKey(celestialData.Key))
            {
                // this body is not in saved data. Need to reset all data.
                foreach (var map in celestialData.Value.Maps)
                {
                    if (map.Value.HasData) 
                        map.Value.ClearMap();
                }
            }
            else
            {
                // this body has discovered pixels in saved data
                foreach (var map in celestialData.Value.Maps)
                {
                    var mapsDataAdapter = bufferedLoadData.Bodies[celestialData.Key];
                    
                    if (!mapsDataAdapter.ContainsKey(map.Key))
                    {
                        // this specific map isn't in saved data. Need to reset all data
                        if (map.Value.HasData)
                            map.Value.ClearMap();
                    }
                    else
                    {
                        // this map holds data. Need to update the map.
                        if (mapsDataAdapter[map.Key].IsFullyScanned)
                        {
                            map.Value.UpdateDiscoveredPixels(null, true);
                        }
                        else
                        {
                            var loadedPixels =
                                SaveUtility.DecompressData(mapsDataAdapter[map.Key].DiscoveredPixels);
                            map.Value.UpdateDiscoveredPixels(loadedPixels);
                        }

                        map.Value.ExperimentLevel = mapsDataAdapter[map.Key].ExperimentLevel;
                    }
                }
            }
        }
        _LOGGER.LogInfo("Mapping data loaded.");
    }

    private async Task LoadWaypointData()
    {
        SceneController.Instance.Waypoints.Clear();
        SceneController.Instance.WaypointInitialized = false;
        if (bufferedLoadData.Waypoints.Count > 0)
        {
            _LOGGER.LogDebug($"Found {bufferedLoadData.Waypoints.Count} waypoints to load.");
        
            // wait until all celestial bodies are loaded into UniverseModel
            await WaitUntilAllWaypointBodiesAreLoaded();
            
            foreach (var serializedWaypoint in bufferedLoadData.Waypoints)
            {
                var waypointModel = new WaypointModel();
                waypointModel.Waypoint = serializedWaypoint.Deserialize();
                waypointModel.Body = waypointModel.Waypoint.BodyName;
                waypointModel.MapPositionPercentage = UiUtility.GetPositionPercentageFromGeographicCoordinates(
                    waypointModel.Waypoint.Latitude, waypointModel.Waypoint.Longitude);
                
                SceneController.Instance.Waypoints.Add(waypointModel);
                _LOGGER.LogDebug($"Loaded waypoint '{waypointModel.Waypoint.Name}' on '{waypointModel.Waypoint.BodyName}'.");
            }
            _LOGGER.LogInfo("Waypoint data loaded.");
        }
    }
    
    private async Task WaitUntilAllWaypointBodiesAreLoaded()
    {
        //select all bodies containing waypoints
        var waypointBodies = bufferedLoadData.Waypoints.Select(w => w.BodyName).Distinct();
        
        // try to find each body in UniverseModel and wait until it's loaded
        foreach (var waypointBody in waypointBodies)
        {
            bool isBodyLoaded = false;
            while (!isBodyLoaded)
            {
                var loadedCelestialBodies = GameManager.Instance.Game.UniverseModel.GetAllCelestialBodies();
                if (loadedCelestialBodies.Find(loadedBodies => loadedBodies.Name == waypointBody) != null)
                {
                    isBodyLoaded = true;
                }
                else
                {
                    // body isn't loaded yet, try again in 100 ms
                    await Task.Delay(100);
                }
            }
        }
    }
    
    private void LoadMissionData()
    {
        foreach (var mission in MissionManager.Instance.ActiveMissions.Values)
        {
            if (!bufferedLoadData.Missions.TryGetValue(mission.Id, out var loadedObjectives))
            {
                _LOGGER.LogWarning(
                    $"Mission with ID '{mission.Id}' is marked as 'Active' but its data is not found in the loaded save game data." +
                    $"\nThat is weird, it should never happen! Mission will be now initialized like it's new.");
             
                // just initialize the mission - new optional objective data will be created
                mission.Initialize();
                
                continue;
            }

            if (loadedObjectives.Count != 3)
            {
                _LOGGER.LogError($"Mismatched count of loaded optional objectives for mission '{mission.Id}'." +
                                 $"\nCount is: '{loadedObjectives.Count}', but it should be 3." +
                                 $"Mission will be now initialized like it's new.");
                
                // just initialize the mission - new optional objective data will be created
                mission.Initialize();
                
                continue;
            }

            mission.Objectives.Clear();
            for (int i = 0; i < loadedObjectives.Count; i++)
            {
                var objective = new OptionalObjective();
                objective.CreateFromLoadedData(loadedObjectives[i], i);
                mission.Objectives.Add(objective);
            }
            mission.IsInitialized = true;
            
            _LOGGER.LogInfo($"Loaded data for mission '{mission.Id}'");
        }

        MissionManager.Instance.ReadyForMissionLoading = false;
        
        _LOGGER.LogInfo("Mission data loaded.");
    }
}