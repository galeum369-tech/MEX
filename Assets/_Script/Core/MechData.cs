using UnityEngine;

[System.Serializable]
public class MechData
{
    [Header("생존 스탯")]
    public float maxHP = 500f;
    public float currentHP = 500f;

    [Header("자원 (타격 시 회복)")]
    // 1. 스킬 게이지 (필살기용)
    public float maxSkillGauge = 100f;
    public float currentSkillGauge = 0f;

    // 2. 회복 게이지 (자가 수리용)
    public float maxRepairGauge = 100f;
    public float currentRepairGauge = 0f;

    [Header("기동성")]
    public float moveSpeed = 4f;
    public float dashSpeed = 12f;
    public float jumpPower = 18f;
    public float fastFallSpeed = 30f;

    [Header("액션 제어 (스테미너 대신 사용)")]
    // 회피를 연속으로 못 쓰게 하는 쿨타임
    public float dodgeCooldown = 1.0f;

    // 공격 후 경직 시간 (무기마다 다를 수 있지만 기본값 설정)
    public float attackDelay = 0.5f;

    [Header("장비")]
    public int equippedMeleeWeaponID = 0;
}