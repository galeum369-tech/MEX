using UnityEngine;

[System.Serializable]
public class MechData
{
    public float maxHP = 100f;
    public float currentHP = 100f;

    public float attackPower = 10f;
    public float defense = 5f;

    public float maxGauge = 100f;
    public float currentGauge = 0f;

    public int equippedMeleeWeaponID = 0;
}

