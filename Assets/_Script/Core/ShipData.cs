using UnityEngine;

[System.Serializable]
public class ShipData
{
    public float maxHP = 150f;
    public float currentHP = 150f;

    public float attackPower = 8f;
    public float moveSpeed = 6f;        // 기본 속도
    public float boostMultiplier = 2f;  // 부스트 배율

    public float maxGauge = 100f;
    public float currentGauge = 0f;
}

