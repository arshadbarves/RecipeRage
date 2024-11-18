// using Cinemachine;
// using Core.EventSystem;
// using UnityEngine;
// using Utilities;
// using Random = UnityEngine.Random;
//
// namespace Camera
// {
//     public class AdvancedCameraController : NetworkSingleton<AdvancedCameraController>
//     {
//         [Header("Camera Settings")] [SerializeField]
//         private CinemachineVirtualCamera virtualCamera;
//
//         [SerializeField] private float followSpeed = 5f;
//         [SerializeField] private float zoomSpeed = 2f;
//         [SerializeField] private float minZoom = 5f;
//         [SerializeField] private float maxZoom = 15f;
//         [SerializeField] private Vector2 cameraOffset = new Vector2(0f, 5f);
//
//         [Header("Boundary Settings")] [SerializeField]
//         private float boundaryBuffer = 1f;
//
//         [SerializeField] private Vector2 mapSize = new Vector2(100f, 100f);
//
//         [Header("Transition Settings")] [SerializeField]
//         private float transitionDuration = 0.5f;
//
//         private Transform _currentTarget;
//         private Vector3 _targetPosition;
//         private float _currentZoom;
//         private bool _isTransitioning = false;
//         private float _transitionStartTime;
//
//         private CinemachineFramingTransposer _framingTransposer;
//
//         public override void OnNetworkSpawn()
//         {
//             if (IsClient)
//             {
//                 Initialize();
//             }
//         }
//
//         private void Initialize()
//         {
//             _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
//             _currentZoom = (minZoom + maxZoom) / 2f;
//             _framingTransposer.m_CameraDistance = _currentZoom;
//
//             // Set initial target to local player
//             SetCameraTarget(NetworkManager.LocalClient.PlayerObject.transform);
//
//             // Subscribe to player death and respawn events
//             EventManager.Instance.AddLocalListener<>();
//             EventSystem.Instance.AddListener("LocalPlayerDeath", OnLocalPlayerDeath);
//             EventSystem.Instance.AddListener("LocalPlayerRespawn", OnLocalPlayerRespawn);
//         }
//
//         private void LateUpdate()
//         {
//             if (!IsClient) return;
//
//             UpdateCameraPosition();
//             HandleZoomInput();
//             ClampCameraPosition();
//         }
//
//         private void UpdateCameraPosition()
//         {
//             if (_currentTarget == null) return;
//
//             Vector3 targetPos = _currentTarget.position + new Vector3(cameraOffset.x, cameraOffset.y, 0f);
//
//             if (_isTransitioning)
//             {
//                 float t = (Time.time - _transitionStartTime) / transitionDuration;
//                 _targetPosition = Vector3.Lerp(_targetPosition, targetPos, t);
//
//                 if (t >= 1f)
//                 {
//                     _isTransitioning = false;
//                 }
//             }
//             else
//             {
//                 _targetPosition = Vector3.Lerp(_targetPosition, targetPos, Time.deltaTime * followSpeed);
//             }
//
//             virtualCamera.transform.position = _targetPosition;
//         }
//
//         private void HandleZoomInput()
//         {
//             // For mobile, you'd use touch input here. This is a simple example using mouse scroll.
//             float zoomInput = Input.GetAxis("Mouse ScrollWheel");
//             _currentZoom = Mathf.Clamp(_currentZoom - zoomInput * zoomSpeed, minZoom, maxZoom);
//             _framingTransposer.m_CameraDistance = Mathf.Lerp(_framingTransposer.m_CameraDistance, _currentZoom,
//                 Time.deltaTime * zoomSpeed);
//         }
//
//         private void ClampCameraPosition()
//         {
//             Vector3 pos = virtualCamera.transform.position;
//             pos.x = Mathf.Clamp(pos.x, -mapSize.x / 2 + boundaryBuffer, mapSize.x / 2 - boundaryBuffer);
//             pos.y = Mathf.Clamp(pos.y, -mapSize.y / 2 + boundaryBuffer, mapSize.y / 2 - boundaryBuffer);
//             virtualCamera.transform.position = pos;
//         }
//
//         private void SetCameraTarget(Transform newTarget)
//         {
//             _currentTarget = newTarget;
//             _isTransitioning = true;
//             _transitionStartTime = Time.time;
//         }
//
//         private void OnLocalPlayerDeath(object data)
//         {
//             // Find a random teammate to follow
//             var teammates = FindObjectsOfType<KitchenBrawler>()
//                 .Where(kb => kb.IsAlive && kb.TeamId == NetworkManager.LocalClient.PlayerObject
//                     .GetComponent<KitchenBrawler>().TeamId)
//                 .ToList();
//
//             if (teammates.Count > 0)
//             {
//                 int randomIndex = Random.Range(0, teammates.Count);
//                 SetCameraTarget(teammates[randomIndex].transform);
//             }
//         }
//
//         private void OnLocalPlayerRespawn(object data)
//         {
//             // Set camera back to the local player
//             SetCameraTarget(NetworkManager.LocalClient.PlayerObject.transform);
//         }
//
//         // Call this method when the map size changes (e.g., in Food Truck Frenzy mode)
//         public void UpdateMapBoundaries(Vector2 newMapSize)
//         {
//             mapSize = newMapSize;
//         }
//
//         // Shake the camera (e.g., when hit or using a powerful ability)
//         public void ShakeCamera(float intensity, float duration)
//         {
//             StartCoroutine(CameraShake(intensity, duration));
//         }
//
//         private System.Collections.IEnumerator CameraShake(float intensity, float duration)
//         {
//             CinemachineBasicMultiChannelPerlin noise =
//                 virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
//             noise.m_AmplitudeGain = intensity;
//
//             yield return new WaitForSeconds(duration);
//
//             noise.m_AmplitudeGain = 0f;
//         }
//     }
// }
