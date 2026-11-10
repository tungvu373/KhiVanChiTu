using System;
using KhiVanChiTu.Collaboration;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KhiVanChiTu.PCContent.Tests
{
    public class WorkspaceSaveProfileTests
    {
        private Scene scene;

        [SetUp]
        public void SetUp() { scene = EditorSceneManager.NewPreviewScene(); }

        [TearDown]
        public void TearDown() { EditorSceneManager.ClosePreviewScene(scene); }

        [Test]
        public void SceneWithoutMarkerKeepsLegacySaveNames()
        {
            string profile = WorkspaceSaveProfile.Resolve(scene);
            Assert.That(WorkspaceSaveProfile.Key("KhiVanChiTu_SaveData", profile), Is.EqualTo("KhiVanChiTu_SaveData"));
            Assert.That(WorkspaceSaveProfile.FileName("KhiVanChiTu_SaveData.json", profile), Is.EqualTo("KhiVanChiTu_SaveData.json"));
        }

        [Test]
        public void WorkspaceNeverSharesProductionKeyOrBackup()
        {
            AddMarker("pc-workspace");
            string profile = WorkspaceSaveProfile.Resolve(scene);
            Assert.That(WorkspaceSaveProfile.Key("KhiVanChiTu_SaveData", profile), Is.EqualTo("pc-workspace::KhiVanChiTu_SaveData"));
            Assert.That(WorkspaceSaveProfile.FileName("KhiVanChiTu_SaveData.json", profile), Is.EqualTo("pc-workspace_KhiVanChiTu_SaveData.json"));
        }

        [Test]
        public void InactiveMarkerStillIsolatesSave()
        {
            AddMarker("pc-workspace").gameObject.SetActive(false);
            Assert.That(WorkspaceSaveProfile.Resolve(scene), Is.EqualTo("pc-workspace"));
        }

        [Test]
        public void DuplicateMarkersFailInsteadOfChoosingAnArbitrarySave()
        {
            AddMarker("pc-one");
            AddMarker("pc-two");
            Assert.Throws<InvalidOperationException>(() => WorkspaceSaveProfile.Resolve(scene));
        }

        [Test]
        public void EmptyWorkspaceIdFailsInsteadOfFallingBackToProduction()
        {
            AddMarker("");
            Assert.Throws<ArgumentException>(() => WorkspaceSaveProfile.Resolve(scene));
        }

        [TestCase("../production")]
        [TestCase("pc/production")]
        [TestCase("pc:production")]
        public void UnsafeProfileCannotSelectAnotherBackupPath(string profile)
        {
            Assert.Throws<ArgumentException>(() => WorkspaceSaveProfile.FileName("save.json", profile));
        }

        private WorkspaceSaveProfile AddMarker(string id)
        {
            var markerObject = new GameObject("TestSaveProfile");
            SceneManager.MoveGameObjectToScene(markerObject, scene);
            WorkspaceSaveProfile marker = markerObject.AddComponent<WorkspaceSaveProfile>();
            marker.profileId = id;
            return marker;
        }
    }
}
