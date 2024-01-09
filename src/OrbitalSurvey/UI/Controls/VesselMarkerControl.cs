﻿using UnityEngine;
using UnityEngine.UIElements;
// ReSharper disable InconsistentNaming

namespace OrbitalSurvey.UI.Controls
{
    public class VesselMarkerControl: VisualElement
    {
        public const string UssClassName = "vessel-marker";
        public const string UssClassName_Name = UssClassName + "__name";
        public const string UssClassName_Marker = UssClassName + "__marker";
        public const string UssClassName_MarkerGoodTint = UssClassName_Marker + "--good";
        public const string UssClassName_MarkerWarningTint = UssClassName_Marker + "--warning";
        public const string UssClassName_MarkerErrorTint = UssClassName_Marker + "--error";
        public const string UssClassName_MarkerInnactiveTint = UssClassName_Marker + "--innactive";
        public static string UssClassName_Latitude = UssClassName + "__latitude";
        public static string UssClassName_Longitude = UssClassName + "__longitude";
        
        private Label _nameLabel;
        private VisualElement _markerElement;
        private Label _latitudeLabel;
        private Label _longitudeLabel;
        
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
            set => _latitudeLabel.text = $"LAT: {value.ToString("F3")} °";
        }
        
        public double LongitudeValue
        {
            //get => LongitudeLabel.text;
            set => _longitudeLabel.text = $"LON: {value.ToString("F3")} °";
        }

        public VesselMarkerControl(string name, double latitude, double longitude) : this()
        {
            NameValue = name;
            LatitudeValue = latitude;
            LongitudeValue = longitude;
        }

        public VesselMarkerControl(bool isVesselNameVisible, bool isGeoCoordinatesVisible) : this()
        {
            SetVesselNameVisibility(isVesselNameVisible);
            SetGeoCoordinatesVisibility(isGeoCoordinatesVisible);
        }
        
        public VesselMarkerControl()
        {
            AddToClassList(UssClassName);

            _nameLabel = new Label()
            {
                name = "vessel-name"
            };
            _nameLabel.AddToClassList(UssClassName_Name);
            hierarchy.Add(_nameLabel);
            
            _markerElement = new VisualElement()
            {
                name = "vessel-marker"
            };
            _markerElement.AddToClassList(UssClassName_Marker);
            hierarchy.Add(_markerElement);
            
            _latitudeLabel = new Label()
            {
                name = "vessel-latitude"
            };
            _latitudeLabel.AddToClassList(UssClassName_Latitude);
            hierarchy.Add(_latitudeLabel);
            
            _longitudeLabel = new Label()
            {
                name = "vessel-longitude"
            };
            _longitudeLabel.AddToClassList(UssClassName_Longitude);
            hierarchy.Add(_longitudeLabel);
        }

        public void SetAsNormal()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInnactiveTint);
        }

        public void SetAsGood()
        {
            _markerElement.AddToClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInnactiveTint);
        }

        public void SetAsWarning()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.AddToClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInnactiveTint);
        }

        public void SetAsError()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.AddToClassList(UssClassName_MarkerErrorTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerInnactiveTint);
        }

        public void SetAsInnactive()
        {
            _markerElement.RemoveFromClassList(UssClassName_MarkerGoodTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerWarningTint);
            _markerElement.RemoveFromClassList(UssClassName_MarkerErrorTint);
            _markerElement.AddToClassList(UssClassName_MarkerInnactiveTint);
        }

        public void SetVesselNameVisibility(bool isVisible)
        {
            _nameLabel.visible = isVisible;
        }

        public void SetGeoCoordinatesVisibility(bool isVisible)
        {
            _latitudeLabel.visible = isVisible;
            _longitudeLabel.visible = isVisible;
        }
        
        public new class UxmlFactory : UxmlFactory<VesselMarkerControl, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            UxmlStringAttributeDescription _name = new ()
                { name = "VesselName", defaultValue = "Fly-Safe-1" };
            
            UxmlDoubleAttributeDescription _latitude = new ()
                { name = "Latitude", defaultValue = 45.813053 };
            UxmlDoubleAttributeDescription _longitude = new ()
                { name = "Latitude", defaultValue = 15.977301 };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);

                if (ve is VesselMarkerControl control)
                {
                    control.NameValue = _name.GetValueFromBag(bag, cc);
                    control.LatitudeValue = _latitude.GetValueFromBag(bag, cc);
                    control.LongitudeValue = _longitude.GetValueFromBag(bag, cc);
                }
            }
        }
    }
}