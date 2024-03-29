using KSP.Game;
using SpaceWarp.API.Logging;

namespace OrbitalSurvey.Managers;

public abstract class ManagerBase<T> where T : class
{
    protected ManagerBase()
    {
        Logger = new BepInExLogger(BepInEx.Logging.Logger.CreateLogSource($"OrbitalSurvey.{GetType().Name}"));
    }

    private static readonly Lazy<T> _instance = new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);

    public static T Instance => _instance.Value;
    
    internal ILogger Logger { get; }

    public abstract void Initialize();
}