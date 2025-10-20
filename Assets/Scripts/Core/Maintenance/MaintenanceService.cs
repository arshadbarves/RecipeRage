using System;
using Core.Events;
using Cysharp.Threading.Tasks;
using Epic.OnlineServices;
using PlayEveryWare.EpicOnlineServices;
using PlayEveryWare.EpicOnlineServices.Samples;
using UnityEngine;

namespace Core.Maintenance
{
    /// <summary>
    /// Service to check maintenance status from EOS Title Storage
    /// Publishes MaintenanceModeEvent via EventBus
    /// </summary>
    public class MaintenanceService : IMaintenanceService
    {
        private const string MAINTENANCE_FILE_NAME = "maintenance.json";
        private static readonly string[] MAINTENANCE_TAGS = { "CONFIG", "MAINTENANCE" };

        private readonly IEventBus _eventBus;
        private MaintenanceData _cachedMaintenanceData;
        private bool _isChecking;

        public MaintenanceService(IEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        /// <summary>
        /// Check maintenance status from Title Storage
        /// This requires user to be logged in
        /// </summary>
        public async UniTask<bool> CheckMaintenanceStatusAsync()
        {
            if (_isChecking)
            {
                Debug.LogWarning("[MaintenanceService] Already checking maintenance status");
                return false;
            }

            _isChecking = true;

            try
            {
                // Check if user is logged in
                ProductUserId userId = EOSManager.Instance?.GetProductUserId();
                if (userId == null || !userId.IsValid())
                {
                    Debug.LogWarning("[MaintenanceService] User not logged in, cannot check maintenance status");
                    _isChecking = false;
                    return false;
                }

                Debug.Log("[MaintenanceService] Querying maintenance file from Title Storage...");

                // Query file list with maintenance tags
                bool querySuccess = false;
                Result queryResult = Result.UnexpectedError;

                TitleStorageService.Instance.QueryFileList(MAINTENANCE_TAGS, (result) =>
                {
                    queryResult = result;
                    querySuccess = result == Result.Success;
                });

                // Wait for query to complete with timeout
                var timeout = TimeSpan.FromSeconds(10);
                var startTime = DateTime.UtcNow;

                while (!querySuccess && queryResult == Result.UnexpectedError)
                {
                    if (DateTime.UtcNow - startTime > timeout)
                    {
                        Debug.LogWarning("[MaintenanceService] Query file list timed out");
                        _eventBus?.Publish(new MaintenanceCheckFailedEvent
                        {
                            Error = "Maintenance check timed out"
                        });
                        _isChecking = false;
                        return false;
                    }

                    await UniTask.Delay(100);
                }

                if (!querySuccess)
                {
                    Debug.LogWarning($"[MaintenanceService] Query file list failed: {queryResult}");
                    _eventBus?.Publish(new MaintenanceCheckFailedEvent
                    {
                        Error = $"Failed to query maintenance data: {queryResult}"
                    });
                    _isChecking = false;
                    return false;
                }

                // Check if maintenance file exists
                var fileNames = TitleStorageService.Instance.GetCachedCurrentFileNames();
                if (!fileNames.Contains(MAINTENANCE_FILE_NAME))
                {
                    Debug.Log("[MaintenanceService] No maintenance file found - service is operational");
                    PublishNoMaintenanceEvent();
                    _isChecking = false;
                    return true;
                }

                Debug.Log("[MaintenanceService] Maintenance file found, downloading...");

                // Download maintenance file
                bool downloadSuccess = false;
                Result downloadResult = Result.UnexpectedError;

                TitleStorageService.Instance.DownloadFile(MAINTENANCE_FILE_NAME, (result) =>
                {
                    downloadResult = result;
                    downloadSuccess = result == Result.Success;
                });

                // Wait for download to complete with timeout
                startTime = DateTime.UtcNow;

                while (!downloadSuccess && downloadResult == Result.UnexpectedError)
                {
                    if (DateTime.UtcNow - startTime > timeout)
                    {
                        Debug.LogWarning("[MaintenanceService] Download file timed out");
                        _eventBus?.Publish(new MaintenanceCheckFailedEvent
                        {
                            Error = "Maintenance file download timed out"
                        });
                        _isChecking = false;
                        return false;
                    }

                    await UniTask.Delay(100);
                }

                if (!downloadSuccess)
                {
                    Debug.LogWarning($"[MaintenanceService] Download file failed: {downloadResult}");
                    _eventBus?.Publish(new MaintenanceCheckFailedEvent
                    {
                        Error = $"Failed to download maintenance data: {downloadResult}"
                    });
                    _isChecking = false;
                    return false;
                }

                // Parse downloaded file
                if (TitleStorageService.Instance.GetLocallyCachedData().TryGetValue(MAINTENANCE_FILE_NAME, out string fileContent))
                {
                    ParseAndPublishMaintenanceData(fileContent);
                    _isChecking = false;
                    return true;
                }
                else
                {
                    Debug.LogError("[MaintenanceService] Failed to retrieve downloaded file from cache");
                    _eventBus?.Publish(new MaintenanceCheckFailedEvent
                    {
                        Error = "Failed to retrieve maintenance data from cache"
                    });
                    _isChecking = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MaintenanceService] Exception during maintenance check: {ex.Message}");
                _eventBus?.Publish(new MaintenanceCheckFailedEvent
                {
                    Error = $"Maintenance check error: {ex.Message}"
                });
                _isChecking = false;
                return false;
            }
        }

        /// <summary>
        /// Show maintenance screen for server down scenario (no estimation available)
        /// </summary>
        public void ShowServerDownMaintenance(string error)
        {
            Debug.Log($"[MaintenanceService] Showing server down maintenance: {error}");

            _eventBus?.Publish(new MaintenanceModeEvent
            {
                IsMaintenanceMode = true,
                EstimatedEndTime = null,
                Message = "We're experiencing technical difficulties. Please try again later.",
                AllowRetry = true
            });
        }

        /// <summary>
        /// Get cached maintenance data
        /// </summary>
        public MaintenanceData GetCachedMaintenanceData()
        {
            return _cachedMaintenanceData;
        }

        private void ParseAndPublishMaintenanceData(string jsonContent)
        {
            try
            {
                var data = JsonUtility.FromJson<MaintenanceData>(jsonContent);
                _cachedMaintenanceData = data;

                Debug.Log($"[MaintenanceService] Parsed maintenance data - IsMaintenanceMode: {data.isMaintenanceMode}");

                _eventBus?.Publish(new MaintenanceModeEvent
                {
                    IsMaintenanceMode = data.isMaintenanceMode,
                    EstimatedEndTime = data.estimatedEndTime,
                    Message = data.message,
                    AllowRetry = data.allowRetry
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MaintenanceService] Failed to parse maintenance JSON: {ex.Message}");
                _eventBus?.Publish(new MaintenanceCheckFailedEvent
                {
                    Error = "Failed to parse maintenance data"
                });
            }
        }

        private void PublishNoMaintenanceEvent()
        {
            _cachedMaintenanceData = new MaintenanceData
            {
                isMaintenanceMode = false,
                estimatedEndTime = null,
                message = "Service is operational",
                allowRetry = false
            };

            _eventBus?.Publish(new MaintenanceModeEvent
            {
                IsMaintenanceMode = false,
                EstimatedEndTime = null,
                Message = "Service is operational",
                AllowRetry = false
            });
        }
    }

    /// <summary>
    /// Maintenance data structure (matches JSON from Title Storage)
    /// </summary>
    [Serializable]
    public class MaintenanceData
    {
        public bool isMaintenanceMode;
        public string estimatedEndTime;
        public string message;
        public bool allowRetry;
    }
}
