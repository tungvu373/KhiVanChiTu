using UnityEngine;
using System;

public class SwordLoadoutService : MonoBehaviour
{
    public static SwordLoadoutService Instance { get; private set; }
    public const int MAX_SWORD_CAPACITY = 10;
    public event Action<int> OnCapacityChanged;
    private int currentCapacity = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public int CalculateCapacity(RealmId realm, int subLevel)
    {
        switch (realm)
        {
            case RealmId.LuyenKhi: return 1;
            case RealmId.TrucCo:   return 2;
            case RealmId.KimDan:   return 3;
            case RealmId.NguyenAnh:return 4;
            case RealmId.HoaThan:  return 5;
            case RealmId.LuyenHu:  return 6;
            case RealmId.HopThe:   return 7;
            case RealmId.DaiThua:  return 8;
            case RealmId.DoKiep:   return subLevel >= 10 ? 10 : 9;
            default: return 1;
        }
    }

    public void UpdateCapacity(RealmId realm, int subLevel)
    {
        int newCap = CalculateCapacity(realm, subLevel);
        if (newCap != currentCapacity)
        {
            currentCapacity = newCap;
            OnCapacityChanged?.Invoke(currentCapacity);
            Debug.Log($"[SwordLoadout] Capacity updated: {currentCapacity} swords.");
        }
    }

    public int GetCurrentCapacity() => currentCapacity;
}
