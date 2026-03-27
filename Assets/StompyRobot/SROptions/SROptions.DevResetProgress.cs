using System.Collections;
using System.ComponentModel;
using UnityEngine;
using WattsTap.Core;
using WattsTap.Core.API;
using WattsTap.Game.Player;

public partial class SROptions
{
    #region Actions

    [Category("Dev Progress")]
    [DisplayName("⚠ Reset Progress")]
    [Sort(0)]
    public void DevResetProgressAction()
    {
        Debug.LogWarning("[SROptions] Resetting player progress...");
        SendDevResetProgress();
    }

    #endregion

    #region Private Helpers

    private void SendDevResetProgress()
    {
        if (!ServiceLocator.TryGet<ICoreServerService>(out var coreService) || !coreService.IsAuthenticated)
        {
            Debug.LogError("[SROptions] Not authenticated — cannot reset progress");
            return;
        }

        var runner = GetCoroutineRunner();
        if (runner == null)
        {
            Debug.LogError("[SROptions] No MonoBehaviour available to run coroutine");
            return;
        }

        Debug.Log("<color=#FF00FF>[SROptions] Sending progress/reset via CoreServer...</color>");
        runner.StartCoroutine(DevResetProgressCoroutine(coreService));
    }

    private IEnumerator DevResetProgressCoroutine(ICoreServerService coreService)
    {
        yield return coreService.ResetProgress(
            onSuccess: response =>
            {
                Debug.Log($"<color=#00FF00>[SROptions] Progress reset! message={response.message}</color>");

                // Sync local player data from server response
                if (response.progress != null && ServiceLocator.TryGet<IPlayerService>(out var playerService))
                {
                    playerService.LoadFromServer(
                        response.progress.level,
                        response.progress.watts,
                        response.progress.currentXp,
                        response.progress.totalXp
                    );
                    Debug.Log("<color=#00FF00>[SROptions] Local player data synced after reset</color>");
                }
            },
            onError: error =>
            {
                Debug.LogError($"[SROptions] Failed to reset progress: {error}");
            }
        );
    }

    #endregion
}


