using System;
using UnityEngine;

/// <summary>Handles Android Back and Escape for the current UI layer.</summary>
public class MobileUINavigationManager : MonoBehaviour
{
    public static MobileUINavigationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    internal static bool BackPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }

    private void Update()
    {
        var menu = MainMenuManager.Instance;
        if ((menu == null || !menu.isActiveAndEnabled) && BackPressedThisFrame())
            HandleBackNavigation();
    }

    public bool HandleBackNavigation()
    {
        var menu = MainMenuManager.Instance;
        if (menu != null && menu.isActiveAndEnabled)
            return menu.HandleBackNavigation();

        Transform top = null;
        Action closeTop = null;
        void Consider(Transform layer, Action close)
        {
            if (layer != null && layer.gameObject.activeInHierarchy && IsInFrontOf(layer, top))
            {
                top = layer;
                closeTop = close;
            }
        }

        var daoAn = DaoAnModalUI.Instance;
        if (daoAn != null && daoAn.IsModalOpen) Consider(daoAn.ModalTransform, daoAn.Close);
        var secretRealm = SecretRealmModalUI.Instance;
        if (secretRealm != null && secretRealm.IsModalOpen)
        {
            var secretLayer = transform.root.Find("SecretRealmModal");
            if (secretLayer == null && GetComponentInParent<Canvas>() != null)
                secretLayer = GetComponentInParent<Canvas>().transform.Find("SecretRealmModal");
            Consider(secretLayer, secretRealm.Close);
        }
        var dismantle = DismantleConfirmModalUI.Instance;
        var secretPlay = SecretRealmPlayController.Instance;
        if (secretPlay != null) Consider(secretPlay.ActiveNavigationLayer, secretPlay.CloseAll);
        if (dismantle != null && dismantle.IsModalOpen) Consider(dismantle.transform, dismantle.Close);
        var storage = StorageModalUI.Instance;
        if (storage != null && storage.IsModalOpen) Consider(storage.transform, storage.Close);
        var achievements = AchievementModalUI.Instance;
        if (achievements != null && achievements.IsModalOpen) Consider(achievements.transform, AchievementModalUI.Close);
        var milestones = MilestoneModalUI.Instance;
        if (milestones != null && milestones.IsModalOpen) Consider(milestones.transform, milestones.Close);
        var bodyDetail = transform.Find("BodyTalentDetailModal");
        if (bodyDetail != null) Consider(bodyDetail, () => bodyDetail.gameObject.SetActive(false));
        var character = CharacterDetailModalUI.Instance;
        if (character != null) Consider(character.transform, character.ClosePanel);
        var tribulation = TribulationModalUI.Instance;
        if (tribulation != null && tribulation.IsOpen) Consider(tribulation.transform, tribulation.OnCancelClicked);
        var offline = OfflineRewardPopupUI.Instance;
        if (offline != null && offline.IsModalOpen) Consider(offline.transform, offline.DismissFromBack);
        var pause = PauseMenuManager.Instance;
        if (pause != null && pause.IsModalOpen) Consider(pause.transform, pause.CloseTopLayer);
        var choices = GameplayChoicesService.Instance;
        if (choices != null && choices.IsChoosing) Consider(choices.ChoiceTransform, choices.ClosePanel);

        if (closeTop != null)
        {
            closeTop();
            return true;
        }

        var portraitHud = GetComponentInChildren<PortraitHudController>(true);
        if (portraitHud != null && portraitHud.CloseTopLayer()) return true;

        var secondaryMenu = GetComponent<JadeSecondaryMenu>();
        if (secondaryMenu != null && secondaryMenu.IsOpen)
        {
            secondaryMenu.Close();
            return true;
        }

        var drawer = MobileDrawerUIController.Instance;
        if (drawer != null && drawer.IsBlockingGameplay)
        {
            drawer.CloseActiveDrawer();
            return true;
        }

        if (pause == null) return false;
        pause.OpenPauseMenu();
        return true;
    }

    private static bool IsInFrontOf(Transform candidate, Transform current)
    {
        if (current == null) return true;
        var candidateCanvas = candidate.GetComponentInParent<Canvas>();
        var currentCanvas = current.GetComponentInParent<Canvas>();
        if (candidateCanvas != null && currentCanvas != null)
        {
            if (!candidateCanvas.overrideSorting) candidateCanvas = candidateCanvas.rootCanvas;
            if (!currentCanvas.overrideSorting) currentCanvas = currentCanvas.rootCanvas;
            int candidateLayer = SortingLayer.GetLayerValueFromID(candidateCanvas.sortingLayerID);
            int currentLayer = SortingLayer.GetLayerValueFromID(currentCanvas.sortingLayerID);
            if (candidateLayer != currentLayer) return candidateLayer > currentLayer;
            if (candidateCanvas.sortingOrder != currentCanvas.sortingOrder)
                return candidateCanvas.sortingOrder > currentCanvas.sortingOrder;
        }
        // Các nhánh cùng Canvas được vẽ theo thứ tự sibling.
        for (var branch = candidate; branch.parent != null; branch = branch.parent)
        {
            if (branch.parent == current) return true;
            var otherBranch = current;
            while (otherBranch.parent != null && otherBranch.parent != branch.parent)
                otherBranch = otherBranch.parent;
            if (otherBranch.parent == branch.parent)
                return branch.GetSiblingIndex() > otherBranch.GetSiblingIndex();
        }
        return false;
    }
}
