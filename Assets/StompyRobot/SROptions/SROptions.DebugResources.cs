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
        if (!ServiceLocator.TryGet<IReferralAPIService>(out var apiService))
        {
            Debug.LogError("[SROptions] IReferralAPIService not found");
            return;
        }

        if (!apiService.IsAuthenticated)
        {
            Debug.LogError("[SROptions] Not authenticated — cannot add resources");
            return;
        }

        var runner = GetCoroutineRunner();
        if (runner == null)
        {
            Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
            return;
        }

        var request = new AddResourcesRequest
        {
            watts = watts,
            xp = xp
        };

        Debug.Log($"<color=#FF00FF>[SROptions] Sending add-resources: watts={watts}, xp={xp}</color>");

        runner.StartCoroutine(AddResourcesCoroutine(apiService, request));
    }

    private IEnumerator AddResourcesCoroutine(IReferralAPIService apiService, AddResourcesRequest request)
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
