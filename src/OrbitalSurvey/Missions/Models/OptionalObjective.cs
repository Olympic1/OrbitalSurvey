using KSP.Sim;
using OrbitalSurvey.UI;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using UnityEngine;
using Random = UnityEngine.Random;

namespace OrbitalSurvey.Missions.Models;

public class OptionalObjective
{
    public string Name;
    public bool IsComplete;
    public bool ContainsMainObjective;
    public Position Position;
    public Vector3d LocalPosition;
    public double Latitude;
    public double Longitude;
    public int StageIndex;

    // UI properties
    public MapMarkerControl AreaMarker { get; set; }
    public Vector2 MapPositionPercentage { get; set; }

    /// <summary>
    /// Creates an optional objective that contains the location of the main objective
    /// </summary>
    /// <param name="mainObjective">Main objective that contains the discoverable position</param>
    /// <param name="name">Name of the Area that will be displayed in the UI</param>
    /// <param name="stageIndex">Zero based index of the mission Stage that this objective will have</param>
    public void CreateForMainObjective(MainObjective mainObjective, string name, int stageIndex)
    {
        Name = name;
        ContainsMainObjective = true;
        //Position = mainObjective.Position;
        //LocalPosition = mainObjective.LocalPosition;
        Latitude = mainObjective.Latitude;
        Longitude = mainObjective.Longitude;
        StageIndex = stageIndex;
        
        CreateAreaMarker();
    }

    /// <summary>
    /// Creates a regular optional objective (false positive)
    /// </summary>
    /// <param name="geoCoordinatesToAvoid">Latitude and Longitude coordinates of existing objectives that should remain at a distance in relation to this objective</param>
    /// <param name="name">Name of the Area that will be displayed in the UI</param>
    /// <param name="stageIndex">Zero based index of the mission Stage that this objective will have</param>
    public void CreateRegularOptionalObjective(List<(double latitude, double longitude)> geoCoordinatesToAvoid, string name, int stageIndex)
    {
        float latitude = 0;
        float longitude = 0;
        float distance = Settings.MIN_ANGULAR_DISTANCE_FOR_MISSION_AREAS;
        bool validCoordinates = false;
        
        while (!validCoordinates)
        {
            validCoordinates = true;
            
            latitude = Random.Range(-Settings.MIN_MAX_LATITUDE_FOR_MISSION_AREAS, Settings.MIN_MAX_LATITUDE_FOR_MISSION_AREAS);
            longitude = Random.Range(-Settings.MIN_MAX_LONGITUDE_FOR_MISSION_AREAS , Settings.MIN_MAX_LONGITUDE_FOR_MISSION_AREAS);

            foreach (var avoidedCoord in geoCoordinatesToAvoid)
            {
                if (Math.Abs(latitude - avoidedCoord.latitude) < distance && 
                    Math.Abs(longitude - avoidedCoord.longitude) < distance)
                {
                    validCoordinates = false;
                    break;
                }
            }
        }

        Name = name;
        ContainsMainObjective = false;
        Latitude = latitude;
        Longitude = longitude;
        StageIndex = stageIndex;
        
        CreateAreaMarker();
    }

    private void CreateAreaMarker()
    {
        AreaMarker = new(
            name: Name,
            latitude: Latitude,
            longitude: Longitude,
            isNameVisible: SceneController.Instance.IsMarkerNamesVisible,
            isGeoCoordinatesVisible: false,
            type: MapMarkerControl.MarkerType.MissionArea);

        MapPositionPercentage = UiUtility.GetPositionPercentageFromGeographicCoordinates(Latitude, Longitude);
    }
}