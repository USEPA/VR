using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace L58.EPAVR
{
    public class DeliveryBox : MonoBehaviour
    {
        #region Inspector Assigned Variables
        [Header("Important References")]
        [SerializeField] protected TriggerEventListener m_sampleTriggerListener;
        [SerializeField] protected DeliveryBoxGUI m_gui;
        [Header("Default Configuration")]
        [SerializeField] protected float m_deliveryTime = 15.0f;
        #endregion
        #region Private Variables
        private DeliveryState m_currentState;
        private List<Sample> m_deliveredSamples;

        private List<SampleRequest> m_deliveryQueue;

        private SampleRequest m_currentRequest;

        private float m_deliveryTimer;
        private SampleWipe m_currentWipe;
        private Coroutine m_respawnWipeRoutine;
        private Action<DeliveryState> m_onStateChange;

        #endregion
        #region Public Properties
        public static DeliveryBox Instance { get; set; }

        public DeliveryState CurrentState { get => m_currentState; }
        public List<Sample> DeliveredSamples { get => m_deliveredSamples; }

        public DeliveryBoxGUI GUI { get => m_gui; }

        public Action<DeliveryState> OnStateChange { get => m_onStateChange; set => m_onStateChange = value; }
        #endregion

        #region Initialization
        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        public void Init()
        {
            // Check for CoreGameManager
            if (CoreGameManager.Instance != null)
            {
                if (CoreGameManager.Instance.CurrentGamemode == Gamemode.ChemicalHunt)
                {
                    // Initialize lists
                    m_deliveredSamples = new List<Sample>();
                    m_deliveryQueue = new List<SampleRequest>();
                    m_currentRequest = null;
                    // Initialize sample trigger
                    m_sampleTriggerListener.Init(true);
                    m_sampleTriggerListener.OnTriggerStayed += i => ProcessTriggerStay(i);
                    m_sampleTriggerListener.OnTriggerExited += i => ProcessTriggerExit(i);
                    // Initialize GUI
                    m_gui.Init(this);
                    m_onStateChange += i => m_gui.SetState((int)i);
                    // Set the default state
                    SetState(DeliveryState.Idle);
                }
                else
                {
                    // Disable this object
                    gameObject.SetActive(false);
                }
            }

        }
        #endregion

        #region Sample-Related Functionality
        public void ProcessSampleRequest(SampleBag _bag)
        {
            if (m_currentRequest != null) return;
            UnityEngine.Debug.Log($"{gameObject.name} arrived in ProcessSampleRequest: {_bag.gameObject.name} || Time: {Time.time}");
            SetState(DeliveryState.Delivering);
            // Create the sample request
            CreateSampleRequest(_bag);
            // Disable the sample wipe
            _bag.CurrentWipe.ClearSample();
            if (_bag.CurrentWipe.SocketItem != null)
            {
                XRToolbeltItem item = _bag.CurrentWipe.SocketItem;
                if (!item.LinkSocket) item.LinkSocket = true;

                item.ForceAttachToSocket(VRUserManager.Instance.Player.MainDeviceAttachPoint);
                item.SetGrabbable(true);
            }
            
            m_currentWipe = _bag.CurrentWipe;
            _bag.CurrentWipe.gameObject.SetActive(false);
            //m_currentWipe.OnDestroyed += StopAllCoroutines;
            m_respawnWipeRoutine = StartCoroutine(RespawnWipe(m_currentWipe.RespawnDelay));
            // Destroy the sample bag
            _bag.Dispose();
            Destroy(_bag.gameObject);
        }
        public void ProcessSampleRequest(SampleWipe _wipe)
        {
            if (m_currentWipe == _wipe) return;

            UnityEngine.Debug.Log($"{gameObject.name} arrived in ProcessSampleRequest | Sample: {((_wipe.CurrentSample != null) ? _wipe.CurrentSample.Chemical.Name : "N/A")} || Time: {Time.time}");
            SetState(DeliveryState.Delivering);
            // Copy the current sample
            if (_wipe.CurrentSample != null)
            {
                Sample sample = new Sample(_wipe.CurrentSample);
                SampleRequest request = new SampleRequest(sample, Time.time);
                UnityEngine.Debug.Log($"Added sample request || Time: {Time.time}");
                m_currentRequest = request;
                m_deliveryTimer = Time.time + m_deliveryTime;
                /*
                if (m_currentRequest != null)
                {
                    m_deliveryQueue.Add(request);
                }
                else
                {
                                  UnityEngine.Debug.Log($"Added sample request || Time: {Time.time}");
                m_currentRequest = request;
                m_deliveryTimer = Time.time + m_deliveryTime;
                }
                */
            }
            // Disable the sample wipe
            _wipe.ClearSample();
            _wipe.GetComponent<XRToolbeltItem>().ForceAttachToSocket();
            m_currentWipe = _wipe;
            _wipe.gameObject.SetActive(false);
            //m_currentWipe.OnDestroyed += StopAllCoroutines;
            StartCoroutine(RespawnWipe(m_currentWipe.RespawnDelay));
        }

        private SampleRequest CreateSampleRequest(SampleBag _bag)
        {
            if (m_currentWipe == _bag.CurrentWipe || _bag.CurrentSample == null) 
            {
                UnityEngine.Debug.Log($"{gameObject.name} failed to make request for bag: {_bag.gameObject.name} | Sample: {((_bag.CurrentSample != null) ? _bag.CurrentSample.Chemical.Name : "N/A")}");
                return null;
            }


            Sample sample = new Sample(_bag.CurrentSample);
            SampleRequest request = new SampleRequest(sample, Time.time);
            UnityEngine.Debug.Log($"{gameObject.name} added sample request || Time: {Time.time}");
            m_currentRequest = request;
            m_deliveryTimer = Time.time + m_deliveryTime;

            return request;
        }
        public void DeliverSample(SampleRequest _request)
        {
            // Add this to the delivered samples
            OffsiteLab.AddSampleRequest(_request);
            //VRUserManager.Instance.DeliverSample(_request.Sample);
        }
        #endregion

        public IEnumerator RespawnWipe(float _duration)
        {
            /*
            float normalizedTime = 0.0f;
            while (normalizedTime <= 1.0f)
            {
                normalizedTime += Time.deltaTime / _duration;
                //UnityEngine.Debug.Log($"{gameObject.name} respawn: {normalizedTime} || Time: {Time.time}");
                yield return null;
            }
            */
            float respawnTimer = 0.0f;
            while (respawnTimer < _duration)
            {
                respawnTimer += Time.deltaTime;
                yield return null;
            }
            m_currentWipe.gameObject.SetActive(true);
            //m_currentWipe.OnDestroyed -= StopAllCoroutines;
            m_currentWipe = null;
            m_respawnWipeRoutine = null;
        }

        #region Collision-Related Logic
        public void ProcessTriggerStay(Collider other)
        {
            if (other.TryGetComponent<SampleBag>(out SampleBag bag) && bag.HasSample)
            {
                bag.Interactable.selectExited.AddListener(i => ProcessSampleRequest(bag));
            }
            
            /*
            if (other.TryGetComponent<SampleWipe>(out SampleWipe wipe))
            {
                wipe.GetComponent<XRGrabInteractable>().selectExited.AddListener(i => ProcessSampleRequest(wipe));
            }
            */
        }

        public void ProcessTriggerExit(Collider other)
        {
            if (other.TryGetComponent<SampleBag>(out SampleBag bag) && bag.HasSample)
            {
                bag.Interactable.selectExited.RemoveListener(i => ProcessSampleRequest(bag));
            }
            
            /*
            if (other.TryGetComponent<SampleWipe>(out SampleWipe wipe))
            {
                wipe.GetComponent<XRGrabInteractable>().selectExited.RemoveListener(i => ProcessSampleRequest(wipe));
            }
            */
            
        }
        #endregion

        #region State-Related Functionality
        public void SetState(DeliveryState _state)
        {
            // Set reference
            m_currentState = _state;
            // Invoke any necessary events
            m_onStateChange?.Invoke(_state);
        }
        #endregion

        // Update is called once per frame
        void Update()
        {
            if (m_currentRequest != null)
            {
                if (Time.time >= m_deliveryTimer)
                {
                    UnityEngine.Debug.Log($"Should be delivering sample || Time: {Time.time}");
                    // Deliver the sample
                    DeliverSample(m_currentRequest);
                    m_deliveryTimer = -1.0f;
                    m_currentRequest = null;
                    SetState(DeliveryState.Idle);
                    /*
                    if (m_deliveryQueue.Count > 0)
                    {
                        SampleRequest request = m_deliveryQueue[0];
                        m_deliveryQueue.Remove(request);
                        m_currentRequest = request;
                    }
                    else
                    {
                        m_currentRequest = null;
                    }
                    */
                }
            }
        }

        private void OnDestroy()
        {
            if (m_respawnWipeRoutine != null) StopCoroutine(m_respawnWipeRoutine);
            //StopAllCoroutines();
        }
    }
    public enum DeliveryState { Idle, Delivering, Processing}
    public class SampleRequest
    {
        #region Protected Variables
        protected Sample m_sample;
        protected float m_timeStamp;
        #endregion
        #region Public Properties
        public Sample Sample { get => m_sample; }
        public float TimeStamp { get => m_timeStamp; }
        #endregion

        public SampleRequest(Sample _sample, float _timeStamp)
        {
            m_sample = _sample;
            m_timeStamp = _timeStamp;
        }
    }
}

