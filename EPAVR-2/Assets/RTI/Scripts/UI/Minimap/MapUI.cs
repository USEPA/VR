using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace L58.EPAVR
{
    public class MapUI : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected Image m_mapImage;
        [SerializeField] protected Transform m_dynamicMarkerContainer;
        [SerializeField] protected Transform m_staticMarkerContainer;
        [SerializeField] private MapMarkerUIObject m_playerMarkerPrefab;
        [SerializeField] private MapMarkerUIObject m_siteMarkerPrefab;
        [SerializeField] private ChemCloudMapMarkerUIObject m_chemSiteMarkerPrefab;
        [SerializeField] private MapMarkerUIObject m_measurementMarkerPrefab;
        [Header("Default Configuration")]
        [SerializeField] private float m_defaultMapScale = 1.5f;
        [SerializeField] private Vector2 m_mapScaleBounds = new Vector2(0.5f, 2.0f);
        [SerializeField] private Gradient m_measurementMarkerGradient;
        #endregion
        #region Private Variables
        protected RectTransform m_rectTransform;
        protected RectTransform m_parentRectTransform;

        protected Vector2 m_parentRectExtents;
        protected Vector2 m_rectExtents;

        protected Vector2 m_cursorPosition;
        protected float m_mapScale;
        protected float m_mapScaleFactor = 1.0f;

        private MapMarkerUIObject m_playerMarker;
        protected List<MapMarkerUIObject> m_staticMarkers;
        public bool m_inScrollMode = false;
        #endregion
        #region Public Properties
        public static MapUI Instance { get; private set; }
        public RectTransform ParentRectTransform
        {
            get
            {
                if (!m_parentRectTransform) m_parentRectTransform = GetComponent<RectTransform>();
                return m_parentRectTransform;
            }
        }
        public RectTransform RectTransform 
        {
            get 
            {
                if (!m_rectTransform) m_rectTransform = m_mapImage.GetComponent<RectTransform>();
                return m_rectTransform;
            }
        }
        
        public Transform StaticMarkerContainer { get => m_staticMarkerContainer; }

        public Image Image { get => m_mapImage; }
        public Vector3 RectTransformScale { get => RectTransform.localScale; }
        public Vector3 RectTransformLocalPosition { get => RectTransform.localPosition; }

        public Vector2 AnchoredPosition { get => RectTransform.anchoredPosition; }

        public Vector2 CursorPosition
        {
            get => m_cursorPosition;
            private set
            {
                m_cursorPosition = value;
                RectTransform.anchoredPosition = m_cursorPosition;
            }
        }

        public List<MapMarkerUIObject> StaticMarkers
        {
            get
            {
                if (m_staticMarkers == null) m_staticMarkers = new List<MapMarkerUIObject>();
                return m_staticMarkers;
            }
        }

        public bool IsScrollMode
        {
            get => m_inScrollMode;
            set
            {
                m_inScrollMode = value;
            }
        }
        #endregion

        private void Awake()
        {
            if (!Instance) Instance = this;
        }

        /*
        private void Start()
        {
            // Calculate parent rect size
            m_parentRectExtents = new Vector2(ParentRectTransform.rect.width / 2, ParentRectTransform.rect.height / 2);
            // Set default scale
            SetScale(m_defaultMapScale);
        }
        */

        #region Initialization
        public virtual void Init(Sprite _image)
        {
            if (!Instance) Instance = this;
            // Initialize the map transform
            InitMapTransform();
            // Set the map image
            m_mapImage.sprite = _image;
        }

        protected virtual void InitMapTransform()
        {
            // Calculate parent rect size
            m_parentRectExtents = new Vector2(ParentRectTransform.rect.width / 2, ParentRectTransform.rect.height / 2);
            // Set default scale
            SetScale(m_defaultMapScale);

            // Create the necessary markers
            CreateMarkers();
        }

        protected virtual void CreateMarkers()
        {
            // Create the player marker
            if (!m_playerMarker) m_playerMarker = Instantiate(m_playerMarkerPrefab, m_dynamicMarkerContainer);
            m_playerMarker.Init(this, VRUserManager.Instance.Player.XRRig.Camera.transform, true, true);

            if (ScenarioManager.Instance.ContaminantSites != null)
            {
                // Get the gamemode type
                Gamemode gamemode = CoreGameManager.Instance.CurrentGamemode;

                if (gamemode == Gamemode.ChemicalHunt)
                {
                    foreach (ContaminationSite site in ScenarioManager.Instance.ContaminantSites)
                    {
                        // Get site info
                        ChemicalContaminantSite chemSite = (ChemicalContaminantSite)site;
                        float scaleFactor = 1.0f;
                        scaleFactor = chemSite.SpillRadius;
                        // Create map markers for each site
                        ChemCloudMapMarkerUIObject siteMarker = (ChemCloudMapMarkerUIObject) CreateMarker(m_chemSiteMarkerPrefab, site.transform.position, true);
                        siteMarker.LoadVaporCloudInfo(chemSite.VaporCloud);
                        //MapMarkerUIObject siteMarker = Instantiate(m_siteMarkerPrefab, m_staticMarkerContainer);
                        //siteMarker.Init(this, site.transform, false);
                        string siteTag = "SampleArea";
                        siteMarker.gameObject.SetActive(false);

                        site.OnCleared.AddListener(() => siteMarker.SetClearStatus(true));
                        //MapManager.Instance.AddSiteMarker(siteMarker, site, siteTag);


                        /*
                        if (gamemode == Gamemode.ChemicalHunt && site is ChemicalContaminantSite chemSite)
                        {
                            // Calculate scale based on site radius
                            scaleFactor = chemSite.SpillRadius;
                        }
                        else
                        {
                            siteTag = "RadiationZone";
                            if (site is RadSite radSite)
                            {
                                scaleFactor = radSite.RadRadius;
                            }
                        }
                        
                        siteMarker.SetScale(scaleFactor);
                        MapManager.Instance.AddSiteMarker(siteMarker, site, siteTag);
                        */

                        //site.OnCleared.AddListener(() => Destroy(siteMarker.gameObject));
                    }
                }
                

                if (DebugManager.Instance && MapManager.Instance) MapManager.Instance.SetSiteDebugView(DebugManager.Instance.ShowSitesOnMinimap);
            }
        }

        public virtual MapMarkerUIObject CreateMarker(MapMarkerUIObject _markerPrefab, bool _isStatic = true)
        {
            MapMarkerUIObject marker = Instantiate(_markerPrefab, (!_isStatic) ? m_dynamicMarkerContainer : m_staticMarkerContainer);
            return marker;
        }

        public virtual MapMarkerUIObject CreateMarker(MapMarkerUIObject _markerPrefab, Vector3 _worldPosition, bool _isStatic = true)
        {
            MapMarkerUIObject marker = CreateMarker(_markerPrefab, _isStatic);
            marker.Init(this, _worldPosition);

            return marker;
        }
        #endregion

        #region Update
        public virtual void OnUpdate()
        {
            // Check for player marker
            if (m_playerMarker) 
            {
                // Update player marker position
                m_playerMarker.UpdateTransform();
                // Keep the map focused on the player
                if (!m_inScrollMode) FocusMapOnImagePosition(m_playerMarker.ImagePosition);
            }

        }
        #endregion

        #region Map Marker-Related Functionality
        public virtual MapMarkerUIObject AddMeasurementMarker(Vector3 _position, float _score)
        {
            // Create map markers for each site
            MapMarkerUIObject siteMarker = Instantiate(m_measurementMarkerPrefab, m_staticMarkerContainer);
            siteMarker.Init(this, _position);
            string siteTag = "MeasurementMarker";
            float scaleFactor = 1.0f;

            siteMarker.SetColor(m_measurementMarkerGradient.Evaluate(_score));

            return siteMarker;
            //MapManager.Instance.AddSiteMarker(siteMarker, site, siteTag);
        }
        #endregion

        #region Display-Related Functionality
        public void FocusMapOnImagePosition(Vector2 _imagePosition)
        {
            // Get image position according to map scale
            Vector2 imagePosition = new Vector2(_imagePosition.x * -m_mapScale, _imagePosition.y * -m_mapScale);
            // Set cursor position
            CursorPosition = imagePosition;
        }

        public void FocusMapOnBoundPosition(Vector2 _boundPosition)
        {
            // Get the image position from bounds
            Vector2 imagePosition = GetImagePositionFromBounds(_boundPosition);
            // Focus the map on the image position
            FocusMapOnImagePosition(imagePosition);
        }

        public void FocusMapOnPlayer()
        {
            FocusMapOnImagePosition(m_playerMarker.ImagePosition);
        }

        public void SetScale(float _scale)
        {
            m_mapScale = Mathf.Clamp(_scale, m_mapScaleBounds.x, m_mapScaleBounds.y);
            RectTransform.localScale = new Vector3(m_mapScale, m_mapScale, RectTransform.localScale.z);
        }

        public void ZoomIn()
        {
            SetScale(m_mapScale * 2.0f);
        }

        public void ZoomOut()
        {
            SetScale(m_mapScale * 0.5f);
        }

        public void ResetScale()
        {
            SetScale(m_defaultMapScale);
        }

        public void ResetFocusOnPlayer()
        {
            IsScrollMode = false;
            // Reset Scale
            ResetScale();
            // Recenter on player
            FocusMapOnPlayer();
        }
        #endregion

        #region Helper Methods
        public Vector2 GetImagePositionFromBounds(Vector2 _boundPosition)
        {
            return MapManager.Instance.GetBoundsToImagePosition(_boundPosition, m_rectTransform);
        }

        public Vector2 GetImagePositionFromWorld(Vector3 _worldPosition)
        {
            return MapManager.Instance.GetWorldToImagePosition(_worldPosition, m_rectTransform);
        }

        public Vector2 GetClampedAnchorPosition(Vector2 _position)
        {
            Vector2 desiredAnchorPosition = new Vector2(_position.x * -m_mapScale, _position.y * -m_mapScale);
            float x = Mathf.Clamp(desiredAnchorPosition.x, (RectTransform.rect.min.x * m_mapScale) + m_parentRectExtents.x, (m_rectTransform.rect.max.x * m_mapScale) - m_parentRectExtents.x);
            float y = Mathf.Clamp(desiredAnchorPosition.y, (RectTransform.rect.min.y * m_mapScale) + m_parentRectExtents.y, (m_rectTransform.rect.max.y * m_mapScale) - m_parentRectExtents.y);
            return new Vector2(x, y);
        }
        #endregion
    }
}

