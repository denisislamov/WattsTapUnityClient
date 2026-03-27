using System.Collections;
using System.ComponentModel;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;

public partial class SROptions
{
    private string _devBoosterCode = "double_tap";

    #region Properties

    [Category("Dev Boosters")]
    [DisplayName("Booster Code")]
    [Sort(0)]
    public string DevBoosterCode
    {
        get => _devBoosterCode;
        set
        {
            _devBoosterCode = value ?? "";
            OnPropertyChanged("DevBoosterCode");
        }
    }

    #endregion

    #region Actions

    [Category("Dev Boosters")]
    [DisplayName("▶ Grant Booster")]
    [Sort(1)]
    public void DevGrantBoosterAction()
    {
        if (string.IsNullOrEmpty(_devBoosterCode))
        {
            Debug.LogWarning("[SROptions] Booster code must not be empty");
            return;
        }

        SendDevGrantBooster(_devBoosterCode);
    }

    #endregion

    #region Private Helpers

    private void SendDevGrantBooster(string code)
    {
        if (!ServiceLocator.TryGet<ICoreServerService>(out var coreService) || !coreService.IsAuthenticated)
        {
            Debug.LogError("[SROptions] Not authenticated — cannot grant booster");
            return;
        }

        var runner = GetCoroutineRunner();
        if (runner == null)
        {
            Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
            return;
        }

        Debug.Log($"<color=#FF00FF>[SROptions] Granting booster: code={code}</color>");
        runner.StartCoroutine(DevGrantBoosterCoroutine(coreService, code));
    }

    private IEnumerator DevGrantBoosterCoroutine(ICoreServerService coreService, string code)
    {
        yield return coreService.DevGrantBooster(
            code,
            onSuccess: response =>
            {
                Debug.Log($"<color=#00FF00>[SROptions] Booster granted!</color>");
                Debug.Log($"<color=#00FF00>[SROptions]   playerBoosterId={response.playerBoosterId}</color>");
                Debug.Log($"<color=#00FF00>[SROptions]   code={response.boosterCode}, type={response.boosterType}</color>");
                Debug.Log($"<color=#00FF00>[SROptions]   acquiredAt={response.acquiredAt}</color>");
            },
            onError: error =>
            {
                Debug.LogError($"[SROptions] Failed to grant booster: {error}");
            }
        );
    }

    #endregion
}


