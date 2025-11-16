using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

namespace GaudIA
{
    /// <summary>
    /// Controla la colocación del NPC en superficies AR detectadas.
    /// </summary>
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARNPCPlacementController : MonoBehaviour
    {
        [Header("NPC Settings")]
        [Tooltip("Prefab del NPC a instanciar (debe tener ConvaiNPC)")]
        [SerializeField] private GameObject npcPrefab;

        [Tooltip("Permitir reubicar el NPC después de colocado")]
        [SerializeField] private bool allowReposition = true;

        [Header("Visual Feedback")]
        [Tooltip("Prefab de indicador de posición (opcional)")]
        [SerializeField] private GameObject placementIndicator;

        [Tooltip("Offset del NPC respecto al punto de toque")]
        [SerializeField] private Vector3 npcOffset = Vector3.zero;

        [Header("UI References")]
        [Tooltip("Texto de instrucciones para el usuario")]
        [SerializeField] private TMPro.TextMeshProUGUI instructionText;

        private ARRaycastManager _arRaycastManager;
        private GameObject _spawnedNPC;
        private GameObject _indicatorInstance;
        private List<ARRaycastHit> _hits = new List<ARRaycastHit>();

        private bool _npcPlaced = false;

        private void Awake()
        {
            _arRaycastManager = GetComponent<ARRaycastManager>();
        }

        private void Start()
        {
            if (placementIndicator != null)
            {
                _indicatorInstance = Instantiate(placementIndicator);
                _indicatorInstance.SetActive(false);
            }

            UpdateInstructionText("Escanea el suelo apuntando con la cámara...");
        }

        private void Update()
        {
            // Si ya hay NPC y no se permite reposicionar, salir
            if (_npcPlaced && !allowReposition)
                return;

            // Detectar toque en pantalla
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                // Ignorar si se toca sobre UI
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;

                // Solo procesar al levantar el dedo
                if (touch.phase == TouchPhase.Ended)
                {
                    if (TryPlaceNPC(touch.position))
                    {
                        if (!_npcPlaced)
                        {
                            _npcPlaced = true;
                            UpdateInstructionText("Gaudí colocado. Acércate para hablar.");
                        }
                        else if (allowReposition)
                        {
                            UpdateInstructionText("Gaudí reposicionado.");
                        }
                    }
                }
            }

            // Actualizar indicador de posición
            UpdatePlacementIndicator();
        }

        private void UpdatePlacementIndicator()
        {
            if (_indicatorInstance == null || _npcPlaced)
            {
                if (_indicatorInstance != null)
                    _indicatorInstance.SetActive(false);
                return;
            }

            // Raycast desde el centro de la pantalla
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

            if (_arRaycastManager.Raycast(screenCenter, _hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = _hits[0].pose;
                _indicatorInstance.transform.SetPositionAndRotation(hitPose.position, hitPose.rotation);
                _indicatorInstance.SetActive(true);

                if (!_npcPlaced)
                {
                    UpdateInstructionText("Toca la pantalla para colocar a Gaudí");
                }
            }
            else
            {
                _indicatorInstance.SetActive(false);

                if (!_npcPlaced)
                {
                    UpdateInstructionText("Escanea el suelo apuntando con la cámara...");
                }
            }
        }

        private bool TryPlaceNPC(Vector2 touchPosition)
        {
            if (_arRaycastManager.Raycast(touchPosition, _hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = _hits[0].pose;

                if (_spawnedNPC == null)
                {
                    // Primera vez: instanciar NPC
                    _spawnedNPC = Instantiate(npcPrefab, hitPose.position + npcOffset, hitPose.rotation);

                    // Opcional: crear anchor para estabilidad
                    ARRaycastHit hit = _hits[0];
                    if (hit.trackable is ARPlane plane)
                    {
                        var anchor = plane.GetComponent<ARAnchorManager>()?.AttachAnchor(plane, hitPose);
                        if (anchor != null)
                        {
                            _spawnedNPC.transform.SetParent(anchor.transform);
                        }
                    }

                    Debug.Log($"NPC instanciado en {hitPose.position}");
                }
                else if (allowReposition)
                {
                    // Reposicionar NPC existente
                    _spawnedNPC.transform.SetPositionAndRotation(hitPose.position + npcOffset, hitPose.rotation);
                    Debug.Log($"NPC reposicionado a {hitPose.position}");
                }

                return true;
            }

            return false;
        }

        private void UpdateInstructionText(string message)
        {
            if (instructionText != null)
            {
                instructionText.text = message;
            }
        }

        /// <summary>
        /// Remueve el NPC de la escena
        /// </summary>
        public void RemoveNPC()
        {
            if (_spawnedNPC != null)
            {
                Destroy(_spawnedNPC);
                _spawnedNPC = null;
                _npcPlaced = false;
                UpdateInstructionText("Escanea el suelo para colocar a Gaudí...");
            }
        }

        /// <summary>
        /// Retorna si hay un NPC colocado
        /// </summary>
        public bool IsNPCPlaced => _npcPlaced;

        /// <summary>
        /// Retorna el NPC instanciado
        /// </summary>
        public GameObject SpawnedNPC => _spawnedNPC;
    }
}