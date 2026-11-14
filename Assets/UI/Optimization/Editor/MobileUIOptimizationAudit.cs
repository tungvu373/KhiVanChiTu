#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

public static class MobileUIOptimizationAudit
{
    public const string Evidence="Docs/UI_Redesign/Backups/Phase10-20261113-101509Z";
    public static object ConfigureMobileTextures()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Evidence+"/before-audit.json"))
            throw new InvalidOperationException("Requires Edit mode and the Phase10 importer checkpoint");
        var targets=new[]{(name:"Android",target:BuildTarget.Android,format:TextureImporterFormat.ETC2_RGBA8),
            (name:"iPhone",target:BuildTarget.iOS,format:TextureImporterFormat.ASTC_4x4)};
        foreach(var target in targets)if(!TextureImporter.IsPlatformTextureFormatValid(TextureImporterType.Sprite,target.target,target.format))
            throw new InvalidOperationException("Unsupported sprite format: "+target.name+" "+target.format);
        var changes=new JArray();
        // Keep the already compressed RGB portrait and all non-generated assets unchanged.
        var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/Art/UI/Generated/")&&p.EndsWith(".png")&&!p.EndsWith("/ImmortalRealmPortrait.png")).OrderBy(p=>p).ToArray();
        foreach(string path in paths)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            changes.Add(JObject.FromObject(new{path,guid=AssetDatabase.AssetPathToGUID(path),before=targets.Select(t=>importer.GetPlatformTextureSettings(t.name)).ToArray()}));
        }
        File.WriteAllText(Evidence+"/mobile-texture-overrides-before.json",changes.ToString());
        foreach(string path in paths)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            foreach(var target in targets)
            {
                var settings=importer.GetPlatformTextureSettings(target.name);
                settings.name=target.name;settings.overridden=true;settings.maxTextureSize=importer.maxTextureSize;
                settings.format=target.format;settings.textureCompression=TextureImporterCompression.CompressedHQ;
                settings.compressionQuality=100;settings.crunchedCompression=false;
                importer.SetPlatformTextureSettings(settings);
            }
            importer.SaveAndReimport();
            foreach(var target in targets)
            {
                var readback=importer.GetPlatformTextureSettings(target.name);
                if(!readback.overridden||readback.format!=target.format||readback.maxTextureSize!=importer.maxTextureSize)
                    throw new InvalidOperationException("Platform override readback failed: "+path+" "+target.name);
            }
        }
        AssetDatabase.SaveAssets();Export("after");
        return new{textures=paths.Length,android="ETC2_RGBA8",ios="ASTC_4x4",nativeImportValidated=false};
    }
    public static object Export(string label)
    {
        var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.EndsWith(".png",StringComparison.OrdinalIgnoreCase)&&
            (p.StartsWith("Assets/Art/UI/")||p.StartsWith("Assets/Resources/SecretRealm/"))).ToArray();
        var textures=new JArray();
        foreach(string path in paths)
        {
            var importer=AssetImporter.GetAtPath(path) as TextureImporter;var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(importer==null||texture==null)continue;
            importer.GetSourceTextureWidthAndHeight(out int sourceWidth,out int sourceHeight);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
            textures.Add(JObject.FromObject(new{path,guid=AssetDatabase.AssetPathToGUID(path),sourceWidth,sourceHeight,width=texture.width,height=texture.height,format=texture.format.ToString(),graphicsFormat=texture.graphicsFormat.ToString(),runtimeBytes=Profiler.GetRuntimeMemorySizeLong(texture),importer.isReadable,importer.mipmapEnabled,importer.maxTextureSize,compression=importer.textureCompression.ToString(),importer.compressionQuality,importer.alphaIsTransparency,importer.sRGBTexture,importer.spritePixelsPerUnit,spriteImportMode=importer.spriteImportMode.ToString(),android=importer.GetPlatformTextureSettings("Android"),ios=importer.GetPlatformTextureSettings("iPhone"),sprites=sprites.Select(s=>new{name=s.name,rect=s.rect.ToString(),border=s.border.ToString(),ppu=s.pixelsPerUnit}).ToArray()}));
        }
        var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);
        var counters=new JArray();foreach(var h in handles)
        {var desc=ProfilerRecorderHandle.GetDescription(h);counters.Add(JObject.FromObject(new{name=desc.Name,category=desc.Category.Name,unit=desc.UnitType.ToString(),dataType=desc.DataType.ToString()}));}
        var scene=UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        var canvas=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Canvas>(true)).First(c=>c.name=="Canvas");
        var graphics=canvas.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Select(g=>new{path=AnimationUtility.CalculateTransformPath(g.transform,null),type=g.GetType().Name,active=g.isActiveAndEnabled,g.raycastTarget,alpha=g.color.a,material=g.material!=null?AssetDatabase.GetAssetPath(g.material):null,texture=g.mainTexture!=null?AssetDatabase.GetAssetPath(g.mainTexture):null}).ToArray();
        var layouts=canvas.GetComponentsInChildren<UnityEngine.UI.LayoutGroup>(true).Select(l=>new{path=AnimationUtility.CalculateTransformPath(l.transform,null),type=l.GetType().Name,active=l.isActiveAndEnabled,hasContentSizeFitter=l.GetComponent<UnityEngine.UI.ContentSizeFitter>()!=null}).ToArray();
        var result=JObject.FromObject(new{label,utc=DateTime.UtcNow.ToString("o"),scene=scene.path,playing=Application.isPlaying,textures,counters,graphics,layouts,atlases=AssetDatabase.FindAssets("t:SpriteAtlas").Select(AssetDatabase.GUIDToAssetPath).ToArray(),spritePackerMode=EditorSettings.spritePackerMode.ToString(),platform=new{activeTarget=EditorUserBuildSettings.activeBuildTarget.ToString(),androidSupported=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android),iosSupported=BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS)},hardware=new{SystemInfo.graphicsDeviceName,api=SystemInfo.graphicsDeviceType.ToString(),SystemInfo.processorType,SystemInfo.systemMemorySize,SystemInfo.graphicsMemorySize},memory=new{totalAllocated=Profiler.GetTotalAllocatedMemoryLong(),totalReserved=Profiler.GetTotalReservedMemoryLong(),monoUsed=Profiler.GetMonoUsedSizeLong(),monoHeap=Profiler.GetMonoHeapSizeLong()}});
        File.WriteAllText(Evidence+"/"+label+"-audit.json",result.ToString());
        return new{textures=textures.Count,counters=counters.Count,graphics=graphics.Length,layouts=layouts.Length};
    }
}
#endif
