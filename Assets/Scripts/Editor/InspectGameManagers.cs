using UnityEditor;
using UnityEngine;

public static class InspectGameManagers
{
    [MenuItem("Tools/Inspect GameManagers And DaoAnModalUI")]
    public static void Inspect()
    {
        var gm = GameObject.Find("GameManagers");
        if (gm != null)
        {
            var comps = gm.GetComponents<Component>();
            string list = "";
            foreach (var c in comps) if (c != null) list += c.GetType().Name + ", ";
            Debug.Log($"=== /GameManagers Comps: {list} ===");
            for (int i = 0; i < gm.transform.childCount; i++)
                Debug.Log($"  child [{i}]: {gm.transform.GetChild(i).name}");
        }
        else Debug.Log("/GameManagers NOT FOUND");

        var daoAn = GameObject.Find("DaoAnModalUI");
        if (daoAn != null)
        {
            var comps = daoAn.GetComponents<Component>();
            string list = "";
            foreach (var c in comps) if (c != null) list += c.GetType().Name + ", ";
            Debug.Log($"=== /DaoAnModalUI Comps: {list} ===");
        }
        else Debug.Log("/DaoAnModalUI NOT FOUND");
    }
}

