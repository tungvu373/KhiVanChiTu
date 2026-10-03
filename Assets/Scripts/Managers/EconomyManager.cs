using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public float currentLinhThach;
    public int currentCoDuyen;

    public event Action<float> OnLinhThachChanged;
    public event Action<int> OnCoDuyenChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddLinhThach(float amount)
    {
        currentLinhThach += amount;
        OnLinhThachChanged?.Invoke(currentLinhThach);
    }

    public bool SpendLinhThach(float amount)
    {
        if (currentLinhThach >= amount)
        {
            currentLinhThach -= amount;
            OnLinhThachChanged?.Invoke(currentLinhThach);
            return true;
        }
        return false;
    }

    public void AddCoDuyen(int amount)
    {
        currentCoDuyen += amount;
        OnCoDuyenChanged?.Invoke(currentCoDuyen);
    }

    public bool SpendCoDuyen(int amount)
    {
        if (currentCoDuyen >= amount)
        {
            currentCoDuyen -= amount;
            OnCoDuyenChanged?.Invoke(currentCoDuyen);
            return true;
        }
        return false;
    }
}
