using UnityEngine;
using System;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    public float currentLinhThach;
    public int currentCoDuyen;
    public int currentKiemY;

    public event Action<float> OnLinhThachChanged;
    public event Action<int> OnCoDuyenChanged;
    public event Action<int> OnKiemYChanged;

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

    public void AddKiemY(int amount)
    {
        currentKiemY += amount;
        OnKiemYChanged?.Invoke(currentKiemY);
    }

    public static string FormatNumber(float number)
    {
        if (number >= 1000000000) return (number / 1000000000f).ToString("0.##") + "B";
        if (number >= 1000000) return (number / 1000000f).ToString("0.##") + "m";
        if (number >= 1000) return (number / 1000f).ToString("0.##") + "k";
        return Mathf.FloorToInt(number).ToString();
    }
}
