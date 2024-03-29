using KSP.Sim;
using Random = UnityEngine.Random;

namespace OrbitalSurvey.Missions.Models;

public class OptionalObjective
{
    public bool IsComplete;
    public bool ContainsMainObjective;
    public Position Position;
    public Vector3d LocalPosition;
    public double Latitude;
    public double Longitude;
    public int StageIndex;

    /// <summary>
    /// Creates an optional objective that contains the location of the main objective
    /// </summary>
    /// <param name="mainObjective">Main objective that contains the discoverable position</param>
    /// <param name="stageIndex">Zero based index of the mission Stage that this objective will have</param>
    public void CreateForMainObjective(MainObjective mainObjective, int stageIndex)
    {
        ContainsMainObjective = true;
        //Position = mainObjective.Position;
        //LocalPosition = mainObjective.LocalPosition;
        Latitude = mainObjective.Latitude;
        Longitude = mainObjective.Longitude;
        StageIndex = stageIndex;
    }
    
    /// <summary>
    /// Creates a regular optional objective (false positive)
    /// </summary>
    /// <param name="geoCoordinatesToAvoid">Latitude and Longitude coordinates of existing objectives that should remain at a distance in relation to this objective</param>
    /// <param name="stageIndex">Zero based index of the mission Stage that this objective will have</param>
    public void CreateRegularOptionalObjective(List<(double latitude, double longitude)> geoCoordinatesToAvoid, int stageIndex)
    {
        float latitude = 0;
        float longitude = 0;
        float distance = 50;
        bool validCoordinates = false;
        
        while (!validCoordinates)
        {
            validCoordinates = true;
            
            latitude = Random.Range(-70f, 70f);
            longitude = Random.Range(-160f, 160f);

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

        ContainsMainObjective = false;
        Latitude = latitude;
        Longitude = longitude;
        
        StageIndex = stageIndex;
    }
}