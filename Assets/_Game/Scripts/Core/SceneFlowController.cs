using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.Remake
{
    public sealed class SceneFlowController : MonoBehaviour
    {
        public static SceneFlowController Instance { get; private set; }

        public const string BootScene = "00_Boot";
        public const string MainMenuScene = "01_MainMenu";
        public const string CharacterSelectScene = "01b_CharacterSelect";
        public const string WorldAct1Scene = "02_World_Act1_Meadow";
        public const string WorldAct2Scene = "03_World_Act2_Wasteland";
        public const string WorldAct3Scene = "04_World_Act3_JadePeaks";
        public const string WorldDemonTowerScene = "05_World_DemonTower";
        public const string WorldSecretSwordScene = "06_World_Secret_SwordTomb";
        public const string WorldSecretHerbScene = "07_World_Secret_HerbGarden";
        public const string WorldSecretOreScene = "08_World_Secret_OreCavern";
        public const string UiOverlayScene = "99_UI_Overlay";

        public static string GetWorldSceneForAct(int actIndex) => Mathf.Clamp(actIndex, 0, 2) switch
        {
            1 => WorldAct2Scene,
            2 => WorldAct3Scene,
            _ => WorldAct1Scene
        };

        public static string GetWorldSceneForSecretRealm(RemakeSecretRealmKind kind) => kind switch
        {
            RemakeSecretRealmKind.SwordTomb => WorldSecretSwordScene,
            RemakeSecretRealmKind.HerbGarden => WorldSecretHerbScene,
            _ => WorldSecretOreScene
        };

        public bool IsTransitioning { get; private set; }
        public bool IsInGame { get; private set; }
        public string ActiveWorldSceneName { get; private set; }

        public event Action<bool, float, string> OnLoadingOverlayChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            Instance = null;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void BootToMainMenu()
        {
            if (!IsTransitioning) StartCoroutine(LoadMainMenuRoutine());
        }

        public void OpenCharacterSelect()
        {
            if (!IsTransitioning) StartCoroutine(OpenCharacterSelectRoutine());
        }

        public void EnterWorldAct1()
        {
            if (IsTransitioning) return;
            var save = SaveService.Current;
            if (SaveService.IsActiveSaveFromNewerVersion) return;
            GameplayEngine.Instance?.ReloadFromActiveProfile();
            StartCoroutine(TransitionToGameplayRoutine(GetWorldSceneForAct(save.activeAct)));
        }

        public void SwitchWorldAct(int actIndex)
        {
            string target = GetWorldSceneForAct(actIndex);
            if (!IsTransitioning && ActiveWorldSceneName != target)
                StartCoroutine(SwitchWorldSceneRoutine(target, ActStageDatabaseSO.GetActLabel(actIndex)));
        }

        public void SwitchToArenaScene(string targetSceneName, string displayTitle)
        {
            if (string.IsNullOrEmpty(targetSceneName) || !IsInGame) return;
            if (ActiveWorldSceneName == targetSceneName)
            {
                RemakeArenaController.Instance?.ResetForStageChange();
                return;
            }
            if (!IsTransitioning)
                StartCoroutine(SwitchWorldSceneRoutine(targetSceneName, displayTitle));
        }

        public void ReturnToCampaignWorld()
        {
            int act = SaveService.Current != null ? SaveService.Current.activeAct : 0;
            SwitchToArenaScene(GetWorldSceneForAct(act), ActStageDatabaseSO.GetActLabel(act));
        }

        public void ReturnToMainMenu()
        {
            if (!IsTransitioning) StartCoroutine(ReturnToMainMenuRoutine());
        }

        private IEnumerator LoadMainMenuRoutine()
        {
            IsTransitioning = true;
            IsInGame = false;
            OnLoadingOverlayChanged?.Invoke(true, 0.25f, "Đang mở Sơn Môn Thanh Vân...");

            if (!IsSceneLoaded(MainMenuScene))
            {
                yield return LoadSceneAdditiveAsync(MainMenuScene, p =>
                    OnLoadingOverlayChanged?.Invoke(true, 0.25f + p * 0.65f, "Đang dựng Tiên Cảnh ban ngày..."));
            }

            SetActiveSceneByName(MainMenuScene);
            RemakeAudioService.Instance?.PlayMenuMusic();
            GraphicsSettingsService.Instance?.ApplyFromSave();

            OnLoadingOverlayChanged?.Invoke(false, 1f, string.Empty);
            IsTransitioning = false;
        }

        private IEnumerator OpenCharacterSelectRoutine()
        {
            IsTransitioning = true;
            if (IsInGame) SaveService.Save();
            IsInGame = false;
            OnLoadingOverlayChanged?.Invoke(true, 0.15f, "Đang mở Động Phủ Chọn Nhân Vật...");

            yield return UnloadAllInstancesOfScene(MainMenuScene);
            yield return UnloadAllInstancesOfScene(UiOverlayScene);
            yield return UnloadAllInstancesOfScene(CharacterSelectScene);
            if (!string.IsNullOrEmpty(ActiveWorldSceneName))
                yield return UnloadAllInstancesOfScene(ActiveWorldSceneName);
            ActiveWorldSceneName = null;
            yield return Resources.UnloadUnusedAssets();

            if (!IsSceneLoaded(CharacterSelectScene))
            {
                yield return LoadSceneAdditiveAsync(CharacterSelectScene, p =>
                    OnLoadingOverlayChanged?.Invoke(true, 0.35f + p * 0.60f, "Đang dựng Tiên Đài 3D..."));
            }

            SetActiveSceneByName(CharacterSelectScene);
            RemakeAudioService.Instance?.PlayMenuMusic();
            GraphicsSettingsService.Instance?.ApplyFromSave();

            OnLoadingOverlayChanged?.Invoke(false, 1f, string.Empty);
            IsTransitioning = false;
        }

        private IEnumerator TransitionToGameplayRoutine(string worldSceneName)
        {
            IsTransitioning = true;
            OnLoadingOverlayChanged?.Invoke(true, 0.1f, "Đang thu dọn sảnh chính...");

            yield return UnloadAllInstancesOfScene(MainMenuScene);
            yield return UnloadAllInstancesOfScene(CharacterSelectScene);
            yield return Resources.UnloadUnusedAssets();

            OnLoadingOverlayChanged?.Invoke(true, 0.35f, "Đang mở Đồng Cỏ Thanh Vân...");
            if (!IsSceneLoaded(worldSceneName))
            {
                yield return LoadSceneAdditiveAsync(worldSceneName, p =>
                    OnLoadingOverlayChanged?.Invoke(true, 0.35f + p * 0.40f, "Đang tải Đấu Trường Vực 1..."));
            }
            ActiveWorldSceneName = worldSceneName;
            SetActiveSceneByName(worldSceneName);

            OnLoadingOverlayChanged?.Invoke(true, 0.8f, "Đang khởi tạo Giao Diện Tu Luyện...");
            if (!IsSceneLoaded(UiOverlayScene))
            {
                yield return LoadSceneAdditiveAsync(UiOverlayScene, p =>
                    OnLoadingOverlayChanged?.Invoke(true, 0.8f + p * 0.20f, "Hoàn tất chuẩn bị..."));
            }

            IsInGame = true;
            BackgroundCatchupService.TryCatchupFromElapsed();
            BackgroundCatchupService.MarkActiveTimestamp();
            RemakeAudioService.Instance?.PlayGameMusic();
            GraphicsSettingsService.Instance?.ApplyFromSave();
            GameplayEngine.Instance?.NotifyArenaReady();

            OnLoadingOverlayChanged?.Invoke(false, 1f, string.Empty);
            IsTransitioning = false;
        }

        private IEnumerator SwitchWorldSceneRoutine(string nextWorldSceneName, string displayTitle)
        {
            IsTransitioning = true;
            IsInGame = false;
            string title = string.IsNullOrEmpty(displayTitle) ? nextWorldSceneName : displayTitle;
            OnLoadingOverlayChanged?.Invoke(true, 0.15f, $"Đang thu hồi đấu trường & chuyển tới {title}...");

            if (!string.IsNullOrEmpty(ActiveWorldSceneName) && IsSceneLoaded(ActiveWorldSceneName))
            {
                yield return SceneManager.UnloadSceneAsync(ActiveWorldSceneName);
                yield return Resources.UnloadUnusedAssets();
            }

            OnLoadingOverlayChanged?.Invoke(true, 0.45f, $"Đang dựng {title}...");
            yield return LoadSceneAdditiveAsync(nextWorldSceneName, p =>
                OnLoadingOverlayChanged?.Invoke(true, 0.45f + p * 0.50f, $"Đang tải {title}..."));
            ActiveWorldSceneName = nextWorldSceneName;
            SetActiveSceneByName(nextWorldSceneName);

            IsInGame = true;
            GraphicsSettingsService.Instance?.ApplyFromSave();
            GameplayEngine.Instance?.NotifyArenaReady();
            OnLoadingOverlayChanged?.Invoke(false, 1f, string.Empty);
            IsTransitioning = false;
        }

        private IEnumerator ReturnToMainMenuRoutine()
        {
            IsTransitioning = true;
            SaveService.Save();
            IsInGame = false;
            OnLoadingOverlayChanged?.Invoke(true, 0.15f, "Đang lưu Tu Vi & rời Đấu Trường...");

            yield return UnloadAllInstancesOfScene(UiOverlayScene);
            yield return UnloadAllInstancesOfScene(CharacterSelectScene);
            if (!string.IsNullOrEmpty(ActiveWorldSceneName))
                yield return UnloadAllInstancesOfScene(ActiveWorldSceneName);
            ActiveWorldSceneName = null;

            yield return Resources.UnloadUnusedAssets();

            if (!IsSceneLoaded(MainMenuScene))
            {
                yield return LoadSceneAdditiveAsync(MainMenuScene, p =>
                    OnLoadingOverlayChanged?.Invoke(true, 0.55f + p * 0.45f, "Đang trở về Sơn Môn..."));
            }
            SetActiveSceneByName(MainMenuScene);
            RemakeAudioService.Instance?.PlayMenuMusic();

            OnLoadingOverlayChanged?.Invoke(false, 1f, string.Empty);
            IsTransitioning = false;
        }

        private static IEnumerator UnloadAllInstancesOfScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) yield break;
            for (int guard = 0; guard < 8; guard++)
            {
                Scene found = default;
                bool hasFound = false;
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    var s = SceneManager.GetSceneAt(i);
                    if (s.isLoaded && s.name == sceneName)
                    {
                        found = s;
                        hasFound = true;
                        break;
                    }
                }
                if (!hasFound) yield break;
                var op = SceneManager.UnloadSceneAsync(found);
                if (op == null) yield break;
                while (!op.isDone) yield return null;
            }
        }

        private static bool IsSceneLoaded(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (sc.isLoaded && sc.name == sceneName) return true;
            }
            return false;
        }

        private static void SetActiveSceneByName(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var sc = SceneManager.GetSceneAt(i);
                if (sc.isLoaded && sc.name == sceneName)
                {
                    SceneManager.SetActiveScene(sc);
                    return;
                }
            }
        }

        private static IEnumerator LoadSceneAdditiveAsync(string sceneName, Action<float> progressCb)
        {
            AsyncOperation op = null;
#if UNITY_EDITOR
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                string editorPath = $"Assets/_Game/Scenes/{sceneName}.unity";
                op = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode(
                    editorPath, new LoadSceneParameters(LoadSceneMode.Additive));
            }
#endif
            if (op == null)
            {
                op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            }

            if (op == null)
            {
                Debug.LogError($"[SceneFlowController] Failed to load additive scene: {sceneName}");
                yield break;
            }

            while (!op.isDone)
            {
                progressCb?.Invoke(op.progress);
                yield return null;
            }
            progressCb?.Invoke(1f);
        }
    }
}
