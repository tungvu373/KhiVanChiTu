using System;
using UnityEngine;

namespace KhiVanChiTu.Remake
{
    public sealed class BackgroundCatchupService : MonoBehaviour
    {
        public static BackgroundCatchupService Instance { get; private set; }

        private float _heartbeatTimer;

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

        private void Update()
        {
            if (!SaveService.Current.hasCreatedCharacter) return;
            if (SceneFlowController.Instance == null || !SceneFlowController.Instance.IsInGame) return;

            _heartbeatTimer += Time.unscaledDeltaTime;
            if (_heartbeatTimer >= 2.0f)
            {
                _heartbeatTimer = 0f;
                TryCatchupFromElapsed();
                MarkActiveTimestamp();
            }
        }

        public static void MarkActiveTimestamp()
        {
            if (!SaveService.Current.hasCreatedCharacter) return;
            SaveService.Current.lastTickUtcTicks = DateTime.UtcNow.Ticks;
        }

        public static void TryCatchupFromElapsed()
        {
            var save = SaveService.Current;
            if (!save.hasCreatedCharacter) return;
            if (SceneFlowController.Instance == null || !SceneFlowController.Instance.IsInGame) return;
            if (save.lastTickUtcTicks <= 0)
            {
                save.lastTickUtcTicks = DateTime.UtcNow.Ticks;
                return;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            double elapsedSeconds = (nowTicks - save.lastTickUtcTicks) / (double)TimeSpan.TicksPerSecond;
            save.lastTickUtcTicks = nowTicks;

            if (elapsedSeconds < 3.0) return;

            // Cap offline progression at 12 hours per session; short tab-hide gets 100%, >30m gets 85% Be Quan
            double clampedSeconds = Math.Min(elapsedSeconds, 12.0 * 3600.0);
            double efficiency = clampedSeconds <= 1800.0 ? 1.0 : 0.85;

            if (GameplayEngine.Instance != null)
            {
                GameplayEngine.Instance.ApplyCatchupSimulation(clampedSeconds, efficiency);
            }
        }
    }
}
