using System.ComponentModel;
using UnityEngine;
using WattsTap.Game.SpineView;

public partial class SROptions
{
    private int _debugSkinIndex;
    private static readonly string[] SkinNames = { "skin 1", "skin 2", "skin 3" };

    #region Properties

    [Category("Character Skins")]
    [DisplayName("Skin (0–2)")]
    [NumberRange(0, 2)]
    [Increment(1)]
    [Sort(0)]
    public int DebugSkinIndex
    {
        get => _debugSkinIndex;
        set
        {
            _debugSkinIndex = Mathf.Clamp(value, 0, SkinNames.Length - 1);
            OnPropertyChanged("DebugSkinIndex");
        }
    }

    #endregion

    #region Actions

    [Category("Character Skins")]
    [DisplayName("▶ Apply Skin")]
    [Sort(1)]
    public void DebugApplySkinAction()
    {
        var controller = Object.FindFirstObjectByType<BlacksmithSpineController>();
        if (controller == null)
        {
            Debug.LogWarning("[SROptions] BlacksmithSpineController not found on scene");
            return;
        }

        var skinName = SkinNames[_debugSkinIndex];
        Debug.Log($"<color=#FF00FF>[SROptions] Switching skin → '{skinName}'</color>");
        controller.SetSkin(skinName);
    }

    [Category("Character Skins")]
    [DisplayName("Skin 1")]
    [Sort(2)]
    public void DebugSetSkin1() => ApplySkinByName("skin 1");

    [Category("Character Skins")]
    [DisplayName("Skin 2")]
    [Sort(3)]
    public void DebugSetSkin2() => ApplySkinByName("skin 2");

    [Category("Character Skins")]
    [DisplayName("Skin 3")]
    [Sort(4)]
    public void DebugSetSkin3() => ApplySkinByName("skin 3");

    #endregion

    #region Private Helpers (Skins)

    private void ApplySkinByName(string skinName)
    {
        var controller = Object.FindFirstObjectByType<BlacksmithSpineController>();
        if (controller == null)
        {
            Debug.LogWarning("[SROptions] BlacksmithSpineController not found on scene");
            return;
        }

        Debug.Log($"<color=#FF00FF>[SROptions] Switching skin → '{skinName}'</color>");
        controller.SetSkin(skinName);
    }

    #endregion
}


