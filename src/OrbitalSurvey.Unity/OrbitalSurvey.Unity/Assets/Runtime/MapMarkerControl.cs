using System;
using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable InconsistentNaming

namespace OrbitalSurvey.UI.Controls
{
    public class MapMarkerControl: VisualElement
    {
        public const string UssClassName = "map-marker";
        
        public const string UssClassName_NameContainer = UssClassName + "__name-container";
        public const string UssClassName_Name = UssClassName + "__name";
        
        public const string UssClassName_MarkerContainer = UssClassName + "__marker-container";
        public const string UssClassName_MissionMarkerContainer = UssClassName + "__mission-marker-container";
        
        public const string UssClassName_VesselMarker = UssClassName + "__vessel-marker";
        public const string UssClassName_MarkerGoodTint = UssClassName_VesselMarker + "--good";
        public const string UssClassName_MarkerWarningTint = UssClassName_VesselMarker + "--warning";
        public const string UssClassName_MarkerErrorTint = UssClassName_VesselMarker + "--error";
        public const string UssClassName_MarkerInactiveTint = UssClassName_VesselMarker + "--inactive";
        
        public const string UssClassName_WaypointMarker = UssClassName + "__waypoint-marker";
        public const string UssClassName_MarkerYellow = UssClassName_WaypointMarker + "--yellow";
        public const string UssClassName_MarkerRed = UssClassName_WaypointMarker + "--red";
        public const string UssClassName_MarkerGreen = UssClassName_WaypointMarker + "--green";
        public const string UssClassName_MarkerBlue = UssClassName_WaypointMarker + "--blue";
        public const string UssClassName_MarkerGray = UssClassName_WaypointMarker + "--gray";
        
        public const string UssClassName_MouseOverMarker = UssClassName + "__mouse-over-marker";
        
        public const string UssClassName_MissionAreaMarker = UssClassName + "__mission-area-marker";
        
        public const string UssClassName_GeoCoordinatesContainer = UssClassName + "__geo-coordinates-container";
        
        public static string UssClassName_Latitude = UssClassName + "__latitude";
        public static string UssClassName_Longitude = UssClassName + "__longitude";
        
        private VisualElement _nameContainer;
        private Label _nameLabel;
        private VisualElement _markerElementContainer;
        private VisualElement _markerElement;
        private VisualElement _geoCoordinatesContainer;
        private Label _latitudeLabel;
        private Label _longitudeLabel;

        private bool _nameVisibilityState;
        private bool _geoCoordinatesVisibilityState;
        
        public string NameValue
        {
            get => _nameLabel.text;
            set => _nameLabel.text = value;
        }
        
        public Texture2D MarkerTexture
        {
            get => _markerElement.style.backgroundImage.value.texture;
            set => _markerElement.style.backgroundImage = value;
        }
        
        public double LatitudeValue
        {
            //get => LatitudeLabel.text;
            set => _latitudeLabel.text = $"LAT: {value:F3}°";
        }
        
        public double LongitudeValue
        {
            //get => LongitudeLabel.text;
            set => _longitudeLabel.text = $"LON: {value:F3}°";
        }

        public MapMarkerControl(string name, double latitude, double longitude, bool isNameVisible, bool isGeoCoordinatesVisible, MarkerType type)
            : this(isNameVisible, isGeoCoordinatesVisible, type)
        {
            NameValue = name;
            LatitudeValue = latitude;
            LongitudeValue = longitude;
        }
        
        public MapMarkerControl(bool isNameVisible, bool isGeoCoordinatesVisible, MarkerType type) : this()
        {
            SetNameVisibility(isNameVisible);
            SetGeoCoordinatesVisibility(isGeoCoordinatesVisible);

            switch (type)
            {
                case MarkerType.Vessel: SetAsVessel();
                    break;
                case MarkerType.Waypoint: SetAsWaypoint();
                    break;
                case MarkerType.MouseOver: SetAsMouseOver();
                    break;
                case MarkerType.MissionArea: SetAsMissionArea();
                    break;
            }
        }
        
        public MapMarkerControl()
        {
            AddToClassList(UssClassName);
            
            _nameContainer = new VisualElement()
            {
                name = "map-marker_name-container"
            };
            _nameContainer.AddToClassList(UssClassName_NameContainer);
            _nameContainer.pickingMode = PickingMode.Ignore;

            _nameLabel = new Label()
            {
                name = "map-marker__name"
            };
            _nameLabel.AddToClassList(UssClassName_Name);
            _nameLabel.pickingMode = PickingMode.Ignore;
            _nameContainer.Add(_nameLabel);
            hierarchy.Add(_nameContainer);

            _markerElementContainer = new VisualElement()
            {
                name = "map-marker_marker-container"
            };
            _markerElementContainer.AddToClassList(UssClassName_MarkerContainer);
            
            _markerElement = new VisualElement()
            {
                name = "map-marker_marker"
            };
            _markerElement.pickingMode = PickingMode.Ignore;
            _markerElementContainer.Add(_markerElement);
            hierarchy.Add(_markerElementContainer);
            
            _geoCoordinatesContainer = new VisualElement()
            {
                name = "map-marker_geo-coordinates-container"
            };
            _geoCoordinatesContainer.AddToClassList(UssClassName_GeoCoordinatesContainer);
            _geoCoordinatesContainer.pickingMode = PickingMode.Ignore;
            
            _latitudeLabel = new Label()
            {
                name = "map-marker__latitude"
            };
            _latitudeLabel.AddToClassList(UssClassName_Latitude);
            _latitudeLabel.pickingMode = PickingMode.Ignore;
            _geoCoordinatesContainer.Add(_latitudeLabel);
            
            _longitudeLabel = new Label()
            {
                name = "map-marker__longitude"
            };
            _longitudeLabel.AddToClassList(UssClassName_Longitude);
            _longitudeLabel.pickingMode = PickingMode.Ignore;
            _geoCoordinatesContainer.Add(_longitudeLabel);
            
            hierarchy.Add(_geoCoordinatesContainer);

            // Show/hide name and geo coordinates on hovering
            _markerElementContainer.RegisterCallback<PointerEnterEvent>(_ =>
            {
                _nameVisibilityState = _nameLabel.visible;
                _geoCoordinatesVisibilityState = _latitudeLabel.visible;
                SetNameVisibility(true);
                SetGeoCoordinatesVisibility(true);
            });
            _markerElementContainer.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                SetNameVisibility(_nameVisibilityState);
                SetGeoCoordinatesVisibility(_geoCoordinatesVisibilityState);
            });
            
            this.pickingMode = PickingMode.Ignore;
            
            // Forward events to the ZoomController that handles zooming (mousewheel) and panning (down/move/up)
            // RegisterCallback<PointerDownEvent>(OnPointerDown);
            // RegisterCallback<PointerMoveEvent>(OnPointerMove);
            // RegisterCallback<PointerUpEvent>(OnPointerUp);
            // RegisterCallback<WheelEvent>(OnWheel);
        }

        // private void OnPointerDown(PointerDownEvent evt) => OnPointerDownEvent?.Invoke(evt);
        // private void OnPointerMove(PointerMoveEvent evt) => OnPointerMoveEvent?.Invoke(evt);
        // private void OnPointerUp(PointerUpEvent evt) => OnPointerUpEvent?.Invoke(evt);
        // private void OnWheel(WheelEvent evt) => OnWheelEvent?.Invoke(evt);
        //
        // public event Action<PointerDownEvent> OnPointerDownEvent;
        // public event Action<PointerMoveEvent> OnPointerMoveEvent;
        // public event Action<PointerUpEvent> OnPointerUpEvent;
        // public event Action<WheelEvent> OnWheelEvent;
        
        public void SetAsVessel()
        {
            _markerElement.AddToClassList(UssClassName_VesselMarker);
            _markerElement.RemoveFromClassList(UssClassName_WaypointMarker);
            _markerElement.RemoveFromClassList(UssClassName_MouseOverMarker);
            _markerElement.RemoveFromClassList(UssClassName_MissionAreaMarker);
            
            _markerElementContainer.AddToClassList(UssClassName_MarkerContainer);
            _markerElementContainer.RemoveFromClassList(UssClassName_MissionMarkerContainer);
            
            _markerElement.pickingMode = PickingMode.Position;
        }

        public void SetAsWaypoint()
        {
            _markerElement.RemoveFromClassList(UssClassName_VesselMarker);
            _markerElement.AddToClassList(UssClassName_WaypointMarker);
            _markerElement.RemoveFromClassList(UssClassName_MouseOverMarker);
            _markerElement.RemoveFromClassList(UssClassName_MissionAreaMarker);
            
            _markerElementContainer.AddToClassList(UssClassName_MarkerContainer);
            _markerElementContainer.RemoveFromClassList(UssClassName_MissionMarkerContainer);
            
            _markerElement.pickingMode = PickingMode.Position;
        }

        public void SetAsMouseOver()
        {
            _markerElement.RemoveFromClassList(UssClassName_VesselMarker);
            _markerElement.RemoveFromClassList(UssClassName_WaypointMarker);
            _markerElement.AddToClassList(UssClassName_MouseOverMarker);
            _markerElement.RemoveFromClassList(UssClassName_MissionAreaMarker);
            
            _markerElementContainer.AddToClassList(UssClassName_MarkerContainer);
            _markerElementContainer.RemoveFromClassList(UssClassName_MissionMarkerContainer);
            
            _markerElementContainer.pickingMode = PickingMode.Ignore;
        }
        
        public void SetAsMissionArea()
        {
            _markerElement.RemoveFromClassList(UssClassName_VesselMarker);
            _markerElement.RemoveFromClassList(UssClassName_WaypointMarker);
            _markerElement.RemoveFromClassList(UssClassName_MouseOverMarker);
            _markerElement.AddToClassList(UssClassName_MissionAreaMarker);
            
            _markerElementContainer.RemoveFromClassList(UssClassName_MarkerContainer);
            _markerElementContainer.AddToClassList(UssClassName_MissionMarkerContainer);
            
            _markerElementContainer.pickingMode = PickingMode.Ignore;
        }

        // Vessel classes
        public void SetAsNormal()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInactiveTint);
        }

        public void SetAsGood()
        {
            _markerElement.AddToClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInactiveTint);
        }

        public void SetAsWarning()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.AddToClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInactiveTint);
        }

        public void SetAsError()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.AddToClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInactiveTint);
        }

        public void SetAsInactive()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.AddToClassList(UssClassName_MarkerInactiveTint);
        }

        // Waypoint classes
        public void SetAsYellow()
        {
            _markerElement.AddToClassList(UssClassName_MarkerYellow);
            _markerElement.RemoveFromClassList(UssClassName_MarkerRed);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGreen);
            _markerElement.RemoveFromClassList(UssClassName_MarkerBlue);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGray);
        }
        
        public void SetAsRed()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerYellow);
            _markerElement.AddToClassList(UssClassName_MarkerRed);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGreen);
            _markerElement.RemoveFromClassList(UssClassName_MarkerBlue);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGray);
        }
        
        public void SetAsGreen()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerYellow);
            _markerElement.RemoveFromClassList(UssClassName_MarkerRed);
            _markerElement.AddToClassList(UssClassName_MarkerGreen);
            _markerElement.RemoveFromClassList(UssClassName_MarkerBlue);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGray);
        }
        
        public void SetAsBlue()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerYellow);
            _markerElement.RemoveFromClassList(UssClassName_MarkerRed);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGreen);
            _markerElement.AddToClassList(UssClassName_MarkerBlue);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGray);
        }
        
        public void SetAsGray()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerYellow);
            _markerElement.RemoveFromClassList(UssClassName_MarkerRed);
            _markerElement.RemoveFromClassList(UssClassName_MarkerGreen);
            _markerElement.RemoveFromClassList(UssClassName_MarkerBlue);
            _markerElement.AddToClassList(UssClassName_MarkerGray);
        }

        public void SetNameVisibility(bool isVisible)
        {
            _nameLabel.visible = isVisible;
        }

        public void SetGeoCoordinatesVisibility(bool isVisible)
        {
            _latitudeLabel.visible = isVisible;
            _longitudeLabel.visible = isVisible;
        }
        
        public new class UxmlFactory : UxmlFactory<MapMarkerControl, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _name = new ()
                { name = "MarkerName", defaultValue = "Fly-Safe-1" };
            UxmlDoubleAttributeDescription _latitude = new ()
                { name = "Latitude", defaultValue = 45.813053 };
            UxmlDoubleAttributeDescription _longitude = new ()
                { name = "Latitude", defaultValue = 15.977301 };
            UxmlBoolAttributeDescription _isWaypoint = new()
                { name = "IsWaypoint", defaultValue = false };
            UxmlBoolAttributeDescription _isMissionArea = new()
                { name = "IsMissionArea", defaultValue = false };
            UxmlBoolAttributeDescription _isYellow = new()
                { name = "IsYellow", defaultValue = false };
            UxmlBoolAttributeDescription _isRed = new()
                { name = "IsRed", defaultValue = false };
            UxmlBoolAttributeDescription _isGreen = new()
                { name = "IsGreen", defaultValue = false };
            UxmlBoolAttributeDescription _isBlue = new()
                { name = "IsBlue", defaultValue = false };
            UxmlBoolAttributeDescription _isGray = new()
                { name = "IsGray", defaultValue = false };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is MapMarkerControl control)
                {
                    control.NameValue = _name.GetValueFromBag(bag, cc);
                    control.LatitudeValue = _latitude.GetValueFromBag(bag, cc);
                    control.LongitudeValue = _longitude.GetValueFromBag(bag, cc);
                    
                    if (_isMissionArea.GetValueFromBag(bag, cc))
                    {
                        control.SetAsMissionArea();
                    }
                    else if (_isWaypoint.GetValueFromBag(bag, cc))
                    {
                        control.SetAsWaypoint();
                    }
                    else
                    {
                        control.SetAsVessel();    
                    }
                    
                    if (_isYellow.GetValueFromBag(bag, cc)) control.SetAsYellow();
                    if (_isRed.GetValueFromBag(bag, cc)) control.SetAsRed();
                    if (_isGreen.GetValueFromBag(bag, cc)) control.SetAsGreen();
                    if (_isBlue.GetValueFromBag(bag, cc)) control.SetAsBlue();
                    if (_isGray.GetValueFromBag(bag, cc)) control.SetAsGray();
                }
            }
        }
        
        public enum MarkerType
        {
            Vessel = 0,
            Waypoint = 1,
            MouseOver = 2,
            MissionArea = 3
        }
    }
}