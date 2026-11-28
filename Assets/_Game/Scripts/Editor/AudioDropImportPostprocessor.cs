using UnityEditor;
using UnityEngine;

namespace KhiVanChiTu.Remake.Editor
{
    /// <summary>
    /// Automatically optimizes audio files dropped into Assets/_Game/Resources/AudioDrop/
    /// for WebGL PC + iOS Safari + Mobile performance and low bundle footprint.
    /// </summary>
    public sealed class AudioDropImportPostprocessor : AssetPostprocessor
    {
        private const string AudioDropFolder = "Assets/_Game/Resources/AudioDrop/";

        private void OnPreprocessAudio()
        {
            if (string.IsNullOrEmpty(assetPath)) return;
            string norm = assetPath.Replace('\\', '/');
            if (!norm.StartsWith(AudioDropFolder)) return;

            var importer = assetImporter as AudioImporter;
            if (importer == null) return;

            bool isAmbience = norm.IndexOf("/Ambience/", System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool isBgm = norm.IndexOf("/BGM/", System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool isVoice = norm.IndexOf("/Voice/", System.StringComparison.OrdinalIgnoreCase) >= 0;
            bool isCinematicVoice = isVoice && norm.IndexOf("_realm_", System.StringComparison.OrdinalIgnoreCase) >= 0;

            // Keep stereo for Ambience, BGM, and FullCinematic breakthrough voice lines (baked music bed);
            // force mono for short combat SFX, monster sounds, and clean voice lines to halve size!
            importer.forceToMono = !(isAmbience || isBgm || isCinematicVoice);
            importer.loadInBackground = isAmbience || isBgm;
            importer.ambisonic = false;

            var settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = isBgm || isCinematicVoice ? 0.50f : (isVoice ? 0.48f : 0.42f);
            settings.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            settings.sampleRateOverride = isAmbience || isBgm || isCinematicVoice ? 44100u : 22050u;
            settings.loadType = (isAmbience || isBgm || isVoice)
                ? AudioClipLoadType.CompressedInMemory
                : AudioClipLoadType.DecompressOnLoad;
            importer.defaultSampleSettings = settings;

            var webSettings = settings;
            webSettings.compressionFormat = AudioCompressionFormat.AAC;
            webSettings.loadType = (isAmbience || isBgm)
                ? AudioClipLoadType.CompressedInMemory
                : AudioClipLoadType.DecompressOnLoad;
            importer.SetOverrideSampleSettings("WebGL", webSettings);
        }
    }
}
