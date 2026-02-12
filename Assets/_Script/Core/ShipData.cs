using UnityEngine;

[System.Serializable]
public class ShipData
{
    [Header("Status")]
    public float maxHP = 150f;
    public float currentHP = 150f;
    public float attackPower = 8f;

    [Header("Movement (Upgradable)")]
    public float moveSpeed = 10f;       // 기본 6 -> 10 (아까 튜닝값 반영)
    public float boostMultiplier = 1.5f;// 부스트 배율

    [Header("Handling (Feeling)")]
    // 이 두 개가 추가되어야 조작감을 데이터로 관리 가능!
    [Range(1f, 50f)] public float acceleration = 5f; // 가속력 (반응성)
    public float turnSpeed = 200f;                   // 선회력

    [Header("자원 (타격 시 회복)")]
    // 1. 스킬 게이지 (필살기용)
    public float maxSkillGauge = 100f;
    public float currentSkillGauge = 0f;

    // 2. 회복 게이지 (자가 수리용)
    public float maxRepairGauge = 100f;
    public float currentRepairGauge = 0f;
}