using System.Collections;
using BepInEx.Logging;
using OrbitalSurvey.Missions.Managers;
using OrbitalSurvey.Missions.Models;
using OrbitalSurvey.UI;
using OrbitalSurvey.UI.Controls;
using OrbitalSurvey.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using Logger = BepInEx.Logging.Logger;

namespace OrbitalSurvey.Missions.UI;

public class MissionAreaController : MonoBehaviour
{
    public static MissionAreaController Instance;
    
    private MainGuiController _mainGuiController;
    private VisualElement _root;
    private VisualElement _mapContainer;
    private VisualElement _missionCanvas;
    
    private float _canvasWidth;
    private float _canvasHeight;
    private bool _canvasInitialized;
    
    private Action<float, float> _windowResizedHandler;
    private Action<float> _zoomFactorChangeHandler;
    private Action<Vector2> _panExecutedHandler;
    
    private static readonly ManualLogSource _LOGGER = Logger.CreateLogSource("OrbitalSurvey.MissionAreaController");
    
    public void Start()
    {
        Instance = this;
        _mainGuiController = GetComponent<MainGuiController>();
        _root = GetComponent<UIDocument>().rootVisualElement[0];
        
        _missionCanvas = _root.Q<VisualElement>("mission-canvas");
        
        StartCoroutine(GetCanvasSize());
        StartCoroutine(RegisterForWindowResize());
        StartCoroutine(RegisterForZoomFactorChanged());
        StartCoroutine(RegisterForPanExecuted());
       
        _LOGGER.LogInfo("Initialized.");
    }
    
    private IEnumerator GetCanvasSize()
    {
        // wait for 1 frame for the canvas to get its size
        yield return null;
        
        _canvasWidth = _missionCanvas.layout.width;
        _canvasHeight = _missionCanvas.layout.height;
        _canvasInitialized = true;
    }
    
    public void RebuildMarkers(string body) => StartCoroutine(Rebuild(body));
    private IEnumerator Rebuild(string body)
    {
        while(!_canvasInitialized)
        {
            yield return null;
        }
        
        _missionCanvas.Clear();
        
        if (!MissionManager.Instance.ActiveMissions.TryGetValue(SceneController.Instance.SelectedBody, out var mission))
            yield break;

        foreach (var objective in mission.Objectives)
        {
            UiUtility.PositionMarkerOnTheMap(objective.AreaMarker, objective.MapPositionPercentage, _canvasWidth, _canvasHeight);
            _missionCanvas.Add(objective.AreaMarker);
        }
    }
    
    /// <summary>
    /// Gets the new width and height of the canvas after window is resized by the player.
    /// Then repositions all mission markers. 
    /// </summary>
    private IEnumerator RegisterForWindowResize()
    {
        if (ResizeController.Instance == null)
        {
            yield return null;
        }
        
        _windowResizedHandler = (newWidth, newHeight) =>
        {
            _canvasWidth = _missionCanvas.layout.width;
            _canvasHeight = _missionCanvas.layout.height;
            RepositionAllControls();
        };

        ResizeController.Instance.OnWindowResized += _windowResizedHandler;
    }
    
    /// <summary>
    /// Repositions all markers when zoom factor is changed
    /// </summary>
    private IEnumerator RegisterForZoomFactorChanged()
    {
        if (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }
        
        _zoomFactorChangeHandler = (zoomFactor) => RepositionAllControls();

        ZoomAndPanController.Instance.OnZoomFactorChanged += _zoomFactorChangeHandler;
    }
    
    /// <summary>
    /// Repositions all markers when a pan is executed
    /// </summary>
    private IEnumerator RegisterForPanExecuted()
    {
        if (ZoomAndPanController.Instance == null)
        {
            yield return null;
        }

        _panExecutedHandler = (panOffset) => RepositionAllControls();
        
        ZoomAndPanController.Instance.OnPanExecuted += _panExecutedHandler;
    }
    
    
    
    /// <summary>
    /// Refreshes positions of markers.
    /// Called after a UI event triggered that requires repositioning (zoom, pan, resize)
    /// </summary>
    private void RepositionAllControls()
    {
        if (!MissionManager.Instance.ActiveMissions.TryGetValue(SceneController.Instance.SelectedBody, out var mission))
            return;

        foreach (var objective in mission.Objectives)
        {
            UiUtility.PositionMarkerOnTheMap(objective.AreaMarker, objective.MapPositionPercentage, _canvasWidth, _canvasHeight);
        }
    }

    public void ToggleAreaNames()
    {
        if (!MissionManager.Instance.ActiveMissions.TryGetValue(SceneController.Instance.SelectedBody, out var mission))
            return;
        
        foreach (var objective in mission.Objectives)
        {
            objective.AreaMarker.SetNameVisibility(SceneController.Instance.IsMarkerNamesVisible);
        }
    }
}