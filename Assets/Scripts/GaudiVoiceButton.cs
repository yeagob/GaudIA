using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.LoggerSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GaudIA
{
    /// <summary>
    /// Enhanced voice input button for GaudIA project.
    /// This button replaces the keyboard 'T' key functionality with a mobile-friendly UI button.
    /// Simply attach this script to any UI Button and it will automatically handle voice input.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class GaudiVoiceButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Visual Feedback")]
        [Tooltip("Image component to change color when recording")]
        [SerializeField] private Image buttonImage;

        [Tooltip("Color when idle (not recording)")]
        [SerializeField] private Color idleColor = Color.white;

        [Tooltip("Color when recording voice")]
        [SerializeField] private Color recordingColor = new Color(1f, 0.3f, 0.3f, 1f); // Red-ish

        [Tooltip("Scale multiplier when button is pressed")]
        [SerializeField] private float pressedScale = 1.2f;

        [Tooltip("Optional text to display recording status")]
        [SerializeField] private TMPro.TextMeshProUGUI statusText;

        [Header("Audio Feedback")]
        [Tooltip("Optional audio source for button sounds")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("Sound to play when starting recording")]
        [SerializeField] private AudioClip startRecordingSound;

        [Tooltip("Sound to play when stopping recording")]
        [SerializeField] private AudioClip stopRecordingSound;

        private ConvaiNPC _currentActiveNPC;
        private Button _button;
        private Vector3 _originalScale;
        private bool _isRecording;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;

            // Auto-detect button image if not assigned
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
            }
        }

        private void Start()
        {
            // Subscribe to NPC manager events
            if (ConvaiNPCManager.Instance != null)
            {
                ConvaiNPCManager.Instance.OnActiveNPCChanged += OnActiveNPCChangedHandler;
                _currentActiveNPC = ConvaiNPCManager.Instance.GetActiveConvaiNPC();
                ConvaiLogger.Info("GaudiVoiceButton: Listening to OnActiveNPCChanged event.", ConvaiLogger.LogCategory.Character);
            }
            else
            {
                ConvaiLogger.Warn("GaudiVoiceButton: ConvaiNPCManager instance not found.", ConvaiLogger.LogCategory.Character);
            }

            UpdateButtonState();
        }

        private void OnEnable()
        {
            if (ConvaiNPCManager.Instance != null)
            {
                ConvaiNPCManager.Instance.OnActiveNPCChanged += OnActiveNPCChangedHandler;
                _currentActiveNPC = ConvaiNPCManager.Instance.GetActiveConvaiNPC();
            }

            UpdateButtonState();
        }

        private void OnDisable()
        {
            if (ConvaiNPCManager.Instance != null)
            {
                ConvaiNPCManager.Instance.OnActiveNPCChanged -= OnActiveNPCChangedHandler;
            }

            // Stop recording if button is disabled while recording
            if (_isRecording)
            {
                StopVoiceRecording();
            }
        }

        private void OnDestroy()
        {
            if (ConvaiNPCManager.Instance != null)
            {
                ConvaiNPCManager.Instance.OnActiveNPCChanged -= OnActiveNPCChangedHandler;
            }
        }

        private void OnActiveNPCChangedHandler(ConvaiNPC newActiveNPC)
        {
            _currentActiveNPC = newActiveNPC;
            UpdateButtonState();

            if (_currentActiveNPC != null)
            {
                ConvaiLogger.Info($"GaudiVoiceButton: Active NPC changed to {_currentActiveNPC.name}", ConvaiLogger.LogCategory.Character);
            }
        }

        private void UpdateButtonState()
        {
            // Enable button only if there's an active NPC
            _button.interactable = _currentActiveNPC != null;

            // Update visual state
            if (!_button.interactable && _isRecording)
            {
                StopVoiceRecording();
            }

            UpdateVisuals();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_currentActiveNPC == null || !_button.interactable)
            {
                ConvaiLogger.Warn("GaudiVoiceButton: No active NPC or button is disabled.", ConvaiLogger.LogCategory.Character);
                return;
            }

            StartVoiceRecording();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_isRecording)
            {
                StopVoiceRecording();
            }
        }

        private void StartVoiceRecording()
        {
            if (_currentActiveNPC == null || _isRecording)
                return;

            _isRecording = true;

            // Update action config if available
            if (_currentActiveNPC.playerInteractionManager != null)
            {
                _currentActiveNPC.playerInteractionManager.UpdateActionConfig();
            }

            // Start listening
            _currentActiveNPC.StartListening();

            // Visual feedback
            UpdateVisuals();
            transform.localScale = _originalScale * pressedScale;

            // Audio feedback
            if (audioSource != null && startRecordingSound != null)
            {
                audioSource.PlayOneShot(startRecordingSound);
            }

            ConvaiLogger.DebugLog("GaudiVoiceButton: Started voice recording", ConvaiLogger.LogCategory.Character);
        }

        private void StopVoiceRecording()
        {
            if (!_isRecording)
                return;

            _isRecording = false;

            // Stop listening
            if (_currentActiveNPC != null)
            {
                _currentActiveNPC.StopListening();
            }

            // Visual feedback
            UpdateVisuals();
            transform.localScale = _originalScale;

            // Audio feedback
            if (audioSource != null && stopRecordingSound != null)
            {
                audioSource.PlayOneShot(stopRecordingSound);
            }

            ConvaiLogger.DebugLog("GaudiVoiceButton: Stopped voice recording", ConvaiLogger.LogCategory.Character);
        }

        private void UpdateVisuals()
        {
            if (buttonImage != null)
            {
                buttonImage.color = _isRecording ? recordingColor : idleColor;
            }

            if (statusText != null)
            {
                if (!_button.interactable)
                {
                    statusText.text = "Acércate a Gaudí";
                }
                else if (_isRecording)
                {
                    statusText.text = "Escuchando...";
                }
                else
                {
                    statusText.text = "Mantén para hablar";
                }
            }
        }

        /// <summary>
        /// Public method to manually start recording (can be called from other scripts or UI events)
        /// </summary>
        public void ManualStartRecording()
        {
            if (_button.interactable && !_isRecording)
            {
                StartVoiceRecording();
            }
        }

        /// <summary>
        /// Public method to manually stop recording (can be called from other scripts or UI events)
        /// </summary>
        public void ManualStopRecording()
        {
            if (_isRecording)
            {
                StopVoiceRecording();
            }
        }

        /// <summary>
        /// Returns whether the button is currently recording
        /// </summary>
        public bool IsRecording => _isRecording;

        /// <summary>
        /// Returns the current active NPC
        /// </summary>
        public ConvaiNPC CurrentActiveNPC => _currentActiveNPC;
    }
}
