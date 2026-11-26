using UnityEngine;

namespace KhiVanChiTu.Remake
{
    [DisallowMultipleComponent]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }

        [SerializeField] private bool autoStartMainMenu = true;

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
            Application.runInBackground = true;
            _ = SaveService.Current;
        }

        private void Start()
        {
            if (autoStartMainMenu && SceneFlowController.Instance != null)
            {
                SceneFlowController.Instance.BootToMainMenu();
            }
        }
    }
}
