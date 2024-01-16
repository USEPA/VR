using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace L58.EPAVR
{
    public class MapManager : MonoBehaviour, IManager
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] private MapCamera m_mapCamera;
        [Header("Default Configuration")]
        [SerializeField] private bool m_enableMapCamera = false;
        [SerializeField] private List<TrackTagInfo> m_tagsToTrack;
        #endregion
        #region Private Variables
        private ManagerStatus m_status;
        private Transform m_markerContainer;
        private Bounds m_levelBounds;
        private Vector3 m_levelBoundExtents;
        //private List<MapMarkerObject> m_activeMarkers;
        private Dictionary<string, List<MapMarkerObject>> m_activeMarkers;
        private Dictionary<string, List<MapMarkerUIObject>> m_activeUIMarkers;
        #endregion
        #region Public Properties
        public static MapManager Instance { get; set; }
        public ManagerStatus Status => m_status;

        public Transform MarkerContainer
        {
            get
            {
                if (!m_markerContainer)
                {
                    GameObject containerObj = new GameObject();
                    containerObj.name = "Map Marker Container";
                    containerObj.transform.parent = transform;

                    m_markerContainer = containerObj.transform;
                }
                return m_markerContainer;
            }
        }

        public Dictionary<string, List<MapMarkerUIObject>> ActiveUIMarkers
        {
            get
            {
                if (m_activeUIMarkers == null) m_activeUIMarkers = new Dictionary<string, List<MapMarkerUIObject>>();
                return m_activeUIMarkers;
            }
        }
        public Bounds Bounds { get => m_levelBounds; }

        public Vector3 BoundSize { get => m_levelBounds.size; }
        public Vector3 BoundExtents { get => m_levelBoundExtents; }

        public bool MapCameraEnabled { get => m_enableMapCamera; }
        #endregion

        #region Initialization
        void Awake()
        {
            // Set singleton
            if (!Instance)
                Instance = this;
            else
                Destroy(gameObject);


        }

        void Start()
        {
            /*
            // Get the map bounds
            m_mapCamera.FillCameraWithLevel();
            // Get the map bounds
            m_levelBounds = m_mapCamera.Bounds;
            m_levelBoundExtents = new Vector3(BoundSize.x / 2, BoundSize.y / 2, BoundSize.z / 2);
            */
        }

        public void Startup()
        {
            // Begin initialization
            m_status = ManagerStatus.Initializing;
            /*
            // Enable/position the map camera
            if (m_enableMapCamera)
            {
                m_mapCamera.gameObject.SetActive(true);
            }
            */
            //m_mapCamera.FillCameraWithLevel();
            // Enable/position the map camera
            if (m_enableMapCamera)
            {
                m_mapCamera.gameObject.SetActive(true);
            }
            else
            {
                m_mapCamera.gameObject.SetActive(false);
            }
            // Get the map bounds
            m_mapCamera.FillCameraWithLevel();
            // Get the map bounds
            m_levelBounds = m_mapCamera.Bounds;
            m_levelBoundExtents = new Vector3(BoundSize.x / 2, BoundSize.y / 2, BoundSize.z / 2);
            // Spawn all map markers
            SpawnMarkers();
            // Finish initialization
            UnityEngine.Debug.Log($"MapManager finished startup: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Started;
        }
        #endregion

        #region Map Marker-Related Functionality
        void SpawnMarkers()
        {
            if (m_activeMarkers != null && m_activeMarkers.Count > 0)
                DestroyActiveMarkers();

            // Initialize the list
            if (m_activeMarkers == null) m_activeMarkers = new Dictionary<string, List<MapMarkerObject>>();
            // Set up loop variables
            GameObject markerObject;
            MapMarkerObject marker;

            // Loop through all tags and spawn objects accordingly
            foreach(TrackTagInfo tagInfo in m_tagsToTrack)
            {
                // Find all game objects with this tag
                GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagInfo.Tag);
                List<MapMarkerObject> taggedObjectMarkers = new List<MapMarkerObject>();

                // Loop through each game object found with the specified tag
                foreach(GameObject obj in taggedObjects)
                {
                    // Instantiate and map marker
                    markerObject = Instantiate(tagInfo.Prefab.gameObject, MarkerContainer);
                    marker = markerObject.GetComponent<MapMarkerObject>();
                    // Initialize the marker
                    marker.Init(obj.transform, tagInfo.Color, tagInfo.IsDynamic);
                    // Add this to the registered active markers
                    taggedObjectMarkers.Add(marker);
                }
                // Add this tag and its corresponding markers to the registry
                m_activeMarkers.Add(tagInfo.Tag, taggedObjectMarkers);

                // Check if these markers should be disabled by default
                if (tagInfo.StartInactive) SetTaggedMarkersActive(tagInfo.Tag, false);
            }
        }

        public void SetTaggedMarkersActive(string _tag, bool _value)
        {
            // First, make sure there are active markers
            if (m_activeMarkers == null || m_activeMarkers.Count < 1 || !m_activeMarkers.ContainsKey(_tag)) return;
            // Get the marker list
            List<MapMarkerObject> markers = m_activeMarkers[_tag];
            //UnityEngine.Debug.Log($"{gameObject.name} SetMarkersActive - {_tag}: {_value} || Time: {Time.time}");
            // Loop through each marker and set whether or not it should be active
            foreach(MapMarkerObject marker in markers)
            {
                marker.gameObject.SetActive(_value);
            }
        }

        public void AddSiteMarker(MapMarkerUIObject _siteMarker, ContaminationSite _site, string _tag = "SampleArea")
        {
            if (m_activeUIMarkers == null) m_activeUIMarkers = new Dictionary<string, List<MapMarkerUIObject>>();
            if (!m_activeUIMarkers.ContainsKey(_tag)) m_activeUIMarkers.Add(_tag, new List<MapMarkerUIObject>());

            m_activeUIMarkers[_tag].Add(_siteMarker);
            _site.OnCleared.AddListener(() => RemoveUIMarker(_siteMarker));
        }

        public void SetTaggedUIMarkersActive(string _tag, bool _value)
        {
            // First, make sure there are active markers
            if (m_activeUIMarkers == null || m_activeUIMarkers.Count < 1 || !m_activeUIMarkers.ContainsKey(_tag)) return;
            // Get the marker list
            List<MapMarkerUIObject> markers = m_activeUIMarkers[_tag];
            //UnityEngine.Debug.Log($"{gameObject.name} SetMarkersActive - {_tag}: {_value} || Time: {Time.time}");
            // Loop through each marker and set whether or not it should be active
            foreach (MapMarkerUIObject marker in markers)
            {
                marker.gameObject.SetActive(_value);
            }
        }

        public void RemoveMarker(MapMarkerObject _marker)
        {
            // First, make sure there are active markers
            if (m_activeMarkers == null || m_activeMarkers.Count < 1) return;
            // Loop through each tag and try to find a matching marker
            foreach(KeyValuePair<string, List<MapMarkerObject>> entry in m_activeMarkers)
            {
                List<MapMarkerObject> entryMarkers = entry.Value;
                for (int i = 0; i < entryMarkers.Count; i++) 
                {
                    MapMarkerObject marker = entryMarkers[i];
                    if (marker == _marker)
                    {
                        entryMarkers.Remove(marker);
                        Destroy(marker.gameObject);
                        return;
                    }
                }
            }
        }

        public void RemoveUIMarker(MapMarkerUIObject _marker)
        {
            // First, make sure there are active markers
            if (m_activeUIMarkers == null || m_activeUIMarkers.Count < 1) return;
            // Loop through each tag and try to find a matching marker
            foreach (KeyValuePair<string, List<MapMarkerUIObject>> entry in m_activeUIMarkers)
            {
                List<MapMarkerUIObject> entryMarkers = entry.Value;
                for (int i = 0; i < entryMarkers.Count; i++)
                {
                    MapMarkerUIObject marker = entryMarkers[i];
                    if (marker == _marker)
                    {
                        entryMarkers.Remove(marker);
                        Destroy(marker.gameObject);
                        return;
                    }
                }
            }
        }
        #endregion

        #region Helper Methods
        public Vector2 GetBoundsPosition(Vector3 _worldPosition)
        {
            // Get closest point on the bounds
            Vector3 closestPosition = m_levelBounds.ClosestPoint(_worldPosition);
            // Get position relative to center
            Vector3 centerDiff = closestPosition - m_levelBounds.center;
            centerDiff.y = 0.0f;
            // Factor in offset
            Vector2 boundsOffset = Vector2.zero; // image offset would go here
            if (m_levelBoundExtents.z > m_levelBoundExtents.x)
            {
                boundsOffset.x += (m_levelBoundExtents.z - m_levelBoundExtents.x);
            }
            else
            {
                boundsOffset.y += (m_levelBoundExtents.x - m_levelBoundExtents.z);
            }
            // Normalize the position based on bound size
            return new Vector2(centerDiff.x / (m_levelBoundExtents.x + boundsOffset.x), centerDiff.z / (m_levelBoundExtents.z + boundsOffset.y));
        }

        public Vector3 GetBoundsToImagePosition(Vector2 _boundPosition, RectTransform _targetRect)
        {
            return new Vector2(_boundPosition.x * (_targetRect.rect.width / 2), _boundPosition.y * (_targetRect.rect.height / 2));
        }

        public Vector2 GetWorldToImagePosition(Vector3 _worldPosition, RectTransform _targetRect)
        {
            // Convert world to bound position
            Vector2 boundPosition = GetBoundsPosition(_worldPosition);
            // Get the image position based on the bounds position
            return GetBoundsToImagePosition(boundPosition, _targetRect);
        }

        public Vector2 GetImageToBoundsPosition(Vector2 _imagePosition, RectTransform _sourceRect)
        {
            // Get points relative to center
            Vector2 relativeBoundPoints = new Vector2((_imagePosition.x / (_sourceRect.rect.width / 2)), (_imagePosition.y / (_sourceRect.rect.height / 2)));
            // Factor in offset
            Vector2 boundsOffset = Vector2.zero; // image offset would go here
            if (m_levelBoundExtents.z > m_levelBoundExtents.x)
            {
                boundsOffset.x += (m_levelBoundExtents.z - m_levelBoundExtents.x);
            }
            else
            {
                boundsOffset.y += (m_levelBoundExtents.x - m_levelBoundExtents.z);
            }
            // Get bound position based on this relative position
            Vector2 boundPosition = new Vector2(relativeBoundPoints.x * (m_levelBoundExtents.x + boundsOffset.x), relativeBoundPoints.y * (m_levelBoundExtents.z + boundsOffset.y));
            //UnityEngine.Debug.Log($"Converting image position to bound position: {_sourceRect.gameObject.name} | Image Position: {_imagePosition} | Proportional Image Position: {relativeBoundPoints} | Bound Position: {boundPosition} || Time: {Time.time}");

            return boundPosition;
        }

        public Vector3 GetBoundsToWorldPosition(Vector2 _boundPosition)
        {
            // Add bound position to bound center
            return (m_levelBounds.center + new Vector3(_boundPosition.x, 0.0f, _boundPosition.y));
        }

        public Vector3 GetImageToWorldPosition(Vector2 _imagePosition, RectTransform _sourceRect)
        {
            // Get the bound position
            Vector2 boundPosition = GetImageToBoundsPosition(_imagePosition, _sourceRect);
            // Get the world position based on bound position
            Vector3 worldPosition = GetBoundsToWorldPosition(boundPosition);

            //UnityEngine.Debug.Log($"Converting image position to world position: {_sourceRect.gameObject.name} | Image Position: {_imagePosition} | Bound Position: {boundPosition} | World Position: {worldPosition} || Time: {Time.time}");
            return worldPosition;
        }

        public Vector3 GetWorldToImageRotation(Vector3 _eulerAngles)
        {
            return new Vector3(0, 0, -_eulerAngles.y);
        }

        #endregion

        #region Reset Functionality
        public void ResetToStart()
        {
            // Begin resetting
            m_status = ManagerStatus.Resetting;
            // Disable the map camera
            //m_mapCamera.gameObject.SetActive(false);
            // Clear all active markers
            DestroyActiveMarkers();
            DestroyActiveUIMarkers();
            // Finish reset
            UnityEngine.Debug.Log($"MapManager finished reset: {CoreGameManager.Instance.CurrentState} || Time: {Time.time}");
            m_status = ManagerStatus.Shutdown;
        }

        void DestroyActiveMarkers()
        {
            // Get all map markers
            List<MapMarkerObject> markers = new List<MapMarkerObject>();
            foreach(KeyValuePair<string, List<MapMarkerObject>> entry in m_activeMarkers)
            {
                List<MapMarkerObject> entryMarkers = entry.Value;
                for (int i = 0; i < entryMarkers.Count; i++) markers.Add(entryMarkers[i]);
            }

            MapMarkerObject blip;
            while (markers.Count > 0)
            {
                blip = markers[markers.Count - 1];
                //UnityEngine.Debug.Log($"Destroying marker: {blip.gameObject.name} | Index: {markers.Count - 1} || Time: {Time.time}");
                markers.Remove(blip);
                Destroy(blip.gameObject);
            }

            // Clear the dictionary
            m_activeMarkers = null;
        }


        void DestroyActiveUIMarkers()
        {
            // Get all map markers
            List<MapMarkerUIObject> markers = new List<MapMarkerUIObject>();

            if (m_activeUIMarkers == null || m_activeUIMarkers.Count < 1) return;
            foreach (KeyValuePair<string, List<MapMarkerUIObject>> entry in m_activeUIMarkers)
            {
                List<MapMarkerUIObject> entryMarkers = entry.Value;
                for (int i = 0; i < entryMarkers.Count; i++) markers.Add(entryMarkers[i]);
            }

            MapMarkerUIObject blip;
            while (markers.Count > 0)
            {
                blip = markers[markers.Count - 1];
                //UnityEngine.Debug.Log($"Destroying marker: {blip.gameObject.name} | Index: {markers.Count - 1} || Time: {Time.time}");
                markers.Remove(blip);
                Destroy(blip.gameObject);
            }

            // Clear the dictionary
            m_activeUIMarkers = null;
        }
        #endregion

        // Update is called once per frame
        void Update()
        {

        }

        #region Helper Methods
        public void SetSiteDebugView(bool _value)
        {
            string siteTag = (CoreGameManager.Instance.CurrentGamemode == Gamemode.ChemicalHunt) ? "SampleArea" : "RadiationZone";
            SetTaggedUIMarkersActive(siteTag, _value);
            /*
            if (m_activeMarkers == null || !m_activeMarkers.ContainsKey("SampleArea")) return;
            // Set the markers visibility accordingly
            SetTaggedMarkersActive("SampleArea", _value);
            */
        }
        #endregion
    }

    [System.Serializable]
    public struct TrackTagInfo
    {
        public string Tag;
        public Color Color;
        public MapMarkerObject Prefab;
        public bool IsDynamic;
        public bool StartInactive;
    }
}

