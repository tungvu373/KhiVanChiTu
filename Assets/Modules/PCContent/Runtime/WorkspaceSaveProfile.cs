using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.Collaboration
{
    /// <summary>Opt-in local save isolation. Scenes without a marker keep their existing save.</summary>
    [DisallowMultipleComponent]
    public sealed class WorkspaceSaveProfile : MonoBehaviour
    {
        public string profileId = "pc-workspace";

        public static string Resolve(Scene scene)
        {
            WorkspaceSaveProfile found = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (WorkspaceSaveProfile candidate in root.GetComponentsInChildren<WorkspaceSaveProfile>(true))
                {
                    if (found != null)
                        throw new InvalidOperationException("A gameplay scene must have at most one workspace save profile.");
                    found = candidate;
                }
            }
            if (found == null) return string.Empty;
            ValidateId(found.profileId);
            if (string.IsNullOrEmpty(found.profileId))
                throw new ArgumentException("A workspace marker must have a non-empty profile ID.");
            return found.profileId;
        }

        public static string Key(string original, string profile)
        {
            ValidateId(profile);
            return string.IsNullOrEmpty(profile) ? original : profile + "::" + original;
        }

        public static string FileName(string original, string profile)
        {
            ValidateId(profile);
            return string.IsNullOrEmpty(profile) ? original : profile + "_" + original;
        }

        private static void ValidateId(string profile)
        {
            if (string.IsNullOrEmpty(profile)) return;
            if (profile.Length > 40) throw new ArgumentException("Workspace profile ID is too long.");
            foreach (char c in profile)
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                      (c >= '0' && c <= '9') || c == '-' || c == '_'))
                    throw new ArgumentException("Workspace profile ID may only contain ASCII letters, digits, '-' and '_'.");
        }
    }
}
