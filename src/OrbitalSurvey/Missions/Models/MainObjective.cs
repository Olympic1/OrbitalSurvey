using KSP.Sim;
using OrbitalSurvey.Missions.Utility;
using OrbitalSurvey.Utilities;

namespace OrbitalSurvey.Missions.Models;

public class MainObjective
{
    public MainObjective(string bodyName, string discoverableRegion)
    {
        var bodyComponent = OrbitalSurvey.Utilities.Utility.GetAllCelestialBodies().Find(b => b.Name == bodyName);
        
        foreach (var discoverable in MissionUtility.Discoverables[bodyName].Discoverables)
        {
            if (discoverable.ScienceRegionId == discoverableRegion)
            {
                LocalPosition = discoverable.Position;
                Position = new Position(bodyComponent.SimulationObject.transform.bodyFrame, LocalPosition);
                bodyComponent.GetLatLonAltFromRadius(Position, out Latitude, out Longitude, out AltFromRadius);
            }
        }
    }

    public Position Position;
    public Vector3d LocalPosition;
    public double Latitude;
    public double Longitude;
    public double AltFromRadius;
}