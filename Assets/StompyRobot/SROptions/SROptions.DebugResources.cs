using System.Collections;
using System.ComponentModel;
using SRDebugger;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Core.Services;
using WattsTap.Game.Player;

public partial class SROptions
{
    private int _debugAddWatts = 1000;
    private int _debugAddXp = 500;

    #region Properties

    [Category("Debug Resources")]
    [DisplayName("Watts Amount")]
    [Increment(100)]
    [Sort(0)]
    public int DebugAddWatts
    {
        get => _debugAddWatts;
        set
        {
            _debugAddWatts = Mathf.Max(0, value);
            OnPropertyChanged("DebugAddWatts");
        }
    }

    [Category("Debug Resources")]
    [DisplayName("XP Amount")]
    [Increment(100)]
    [Sort(1)]
    public int DebugAddXp
    {
        get => _debugAddXp;
        set
        {
            _debugAddXp = Mathf.Max(0, value);
            OnPropertyChanged("DebugAddXp");
        }
    }

    #endregion

    #region Actions

    [Category("Debug Resources")]
    [DisplayName("Add Watts")]
    [Sort(2)]
    public void DebugAddWattsAction()
    {
        if (_debugAddWatts <= 0)
        {
            Debug.LogWarning("[SROptions] Watts amount must be greater than 0");
            return;
        }

        SendAddResources(_debugAddWatts, 0);
    }

    [Category("Debug Resources")]
    [DisplayName("Add XP")]
    [Sort(3)]
    public void DebugAddXpAction()
    {
        if (_debugAddXp <= 0)
        {
            Debug.LogWarning("[SROptions] XP amount must be greater than 0");
            return;
        }

        SendAddResources(0, _debugAddXp);
    }

    [Category("Debug Resources")]
    [DisplayName("Add Both (Watts + XP)")]
    [Sort(4)]
    public void DebugAddBothAction()
    {
        if (_debugAddWatts <= 0 && _debugAddXp <= 0)
        {
            Debug.LogWarning("[SROptions] At least one amount must be greater than 0");
            return;
        }

        SendAddResources(_debugAddWatts, _debugAddXp);
    }

    #endregion

    #region Private Helpers

    private void SendAddResources(int watts, int xp)
    {
        // Try new Core Server first (POST /dev/add-resources on api-dev.wattstap.energy)
        if (ServiceLocator.TryGet<ICoreServerService>(out var coreService) && coreService.IsAuthenticated)
        {
            var runner = GetCoroutineRunner();
            if (runner == null)
            {
                Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
                return;
            }

            Debug.Log($"<color=#FF00FF>[SROptions] Sending dev/add-resources via CoreServer: watts={watts}, xp={xp}</color>");
            runner.StartCoroutine(AddResourcesCoreCoroutine(coreService, watts, xp));
            return;
        }

#if OLD_SERVER
        // Fallback to legacy server
        if (ServiceLocator.TryGet<IReferralAPIService>(out var apiService) && apiService.IsAuthenticated)
        {
            var runner = GetCoroutineRunner();
            if (runner == null)
            {
                Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
                return;
            }

            var request = new AddResourcesRequest { watts = watts, xp = xp };
            Debug.Log($"<color=#FF00FF>[SROptions] Sending add-resources via legacy: watts={watts}, xp={xp}</color>");
            runner.StartCoroutine(AddResourcesLegacyCoroutine(apiService, request));
            return;
        }
#endif

        Debug.LogError("[SROptions] Not authenticated — cannot add resources");
    }

    private IEnumerator AddResourcesCoreCoroutine(ICoreServerService coreService, int watts, int xp)
    {
        yield return coreService.DevAddResources(
            watts, xp,
            onSuccess: response =>
            {
                Debug.Log($"<color=#00FF00>[SROptions] Resources added via CoreServer: {response.message}</color>");
                Debug.Log($"<color=#00FF00>[SROptions] Added watts={response.addedWatts}, xp={response.addedXp}</color>");

                // Update local player data from server response
                if (response.progress != null && ServiceLocator.TryGet<IPlayerService>(out var playerService))
                {
                    playerService.LoadFromServer(
                        response.progress.level,
                        response.progress.watts,
                        response.progress.currentXp,
                        response.progress.totalXp
                    );
                    Debug.Log($"<color=#00FF00>[SROptions] Local player data synced from server</color>");
                }
            },
            onError: error =>
            {
                Debug.LogError($"[SROptions] Failed to add resources via CoreServer: {error}");
            }
        );
    }

#if OLD_SERVER
    private IEnumerator AddResourcesLegacyCoroutine(IReferralAPIService apiService, AddResourcesRequest request)
    {
        yield return apiService.AddResources(
            request,
            onSuccess: response =>
            {
                Debug.Log($"<color=#00FF00>[SROptions] Resources added: {response.message}</color>");

                // Update local player data from server response
                if (response.progress != null && ServiceLocator.TryGet<IPlayerService>(out var playerService))
                {
                    playerService.LoadFromServer(
                        response.progress.level,
                        response.progress.watts,
                        response.progress.currentXp,
                        response.progress.totalXp
                    );
                    Debug.Log($"<color=#00FF00>[SROptions] Local player data synced from server</color>");
                }
            },
            onError: error =>
            {
                Debug.LogError($"[SROptions] Failed to add resources: {error}");
            }
        );
    }
#endif

    private MonoBehaviour GetCoroutineRunner()
    {
        if (ServiceLocator.TryGet<IApplicationEntry>(out var appEntry) && appEntry is MonoBehaviour mb)
        {
            return mb;
        }

        return Object.FindObjectOfType<MonoBehaviour>();
    }

    #endregion
}


