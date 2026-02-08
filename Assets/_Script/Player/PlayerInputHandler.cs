using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput_Actions input;

    // --- Events ---
    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action<bool> OnFastFall;
    public event Action<bool> OnDash;
    public event Action OnDodge;

    public event Action<Vector2> OnShipMove;
    public event Action<bool> OnBoost;
    public event Action OnEvasion;

    public event Action OnAttack;
    public event Action OnSkill;
    public event Action OnInteract; // [★추가] 상호작용 키 (F)

    // (아이템/힐 이벤트 등은 필요시 주석 해제하여 사용)

    private void Awake()
    {
        input = new PlayerInput_Actions();
    }

    private void OnEnable()
    {
        // 1. SideView 연결
        input.PlayerSide.Move.performed += MoveCtx;
        input.PlayerSide.Move.canceled += MoveCtx;
        input.PlayerSide.Jump.performed += JumpCtx;
        input.PlayerSide.FastFall.performed += FastFallCtx;
        input.PlayerSide.FastFall.canceled += FastFallCtx;
        input.PlayerSide.Dash.performed += DashCtx;
        input.PlayerSide.Dash.canceled += DashCtx;
        input.PlayerSide.Dodge.performed += DodgeCtx;
        input.PlayerSide.Attack.performed += AttackCtx;
        input.PlayerSide.SupportSkill.performed += SkillCtx;
        input.PlayerSide.Interact.performed += InteractCtx; // [★연결]

        // 2. TopView 연결
        input.PlayerTop.Move.performed += ShipMoveCtx;
        input.PlayerTop.Move.canceled += ShipMoveCtx;
        input.PlayerTop.Dash.performed += BoostCtx;
        input.PlayerTop.Dash.canceled += BoostCtx;
        input.PlayerTop.Dodge.performed += EvasionCtx;
        input.PlayerTop.Attack.performed += AttackCtx;
        input.PlayerTop.SupportSkill.performed += SkillCtx;
        input.PlayerTop.Interact.performed += InteractCtx; // [★연결]

        // 기본값
        input.PlayerSide.Enable();
        input.PlayerTop.Disable();
    }

    private void OnDisable()
    {
        // 연결 해제
        input.PlayerSide.Move.performed -= MoveCtx;
        input.PlayerSide.Move.canceled -= MoveCtx;
        input.PlayerSide.Jump.performed -= JumpCtx;
        input.PlayerSide.FastFall.performed -= FastFallCtx;
        input.PlayerSide.FastFall.canceled -= FastFallCtx;
        input.PlayerSide.Dash.performed -= DashCtx;
        input.PlayerSide.Dash.canceled -= DashCtx;
        input.PlayerSide.Dodge.performed -= DodgeCtx;
        input.PlayerSide.Attack.performed -= AttackCtx;
        input.PlayerSide.SupportSkill.performed -= SkillCtx;
        input.PlayerSide.Interact.performed -= InteractCtx;

        input.PlayerTop.Move.performed -= ShipMoveCtx;
        input.PlayerTop.Move.canceled -= ShipMoveCtx;
        input.PlayerTop.Dash.performed -= BoostCtx;
        input.PlayerTop.Dash.canceled -= BoostCtx;
        input.PlayerTop.Dodge.performed -= EvasionCtx;
        input.PlayerTop.Attack.performed -= AttackCtx;
        input.PlayerTop.SupportSkill.performed -= SkillCtx;
        input.PlayerTop.Interact.performed -= InteractCtx;

        input.Disable();
    }

    public void SetControlMode(bool isSideView)
    {
        if (isSideView)
        {
            input.PlayerTop.Disable();
            input.PlayerSide.Enable();
            Debug.Log("🎮 Mode: SideView");
        }
        else
        {
            input.PlayerSide.Disable();
            input.PlayerTop.Enable();
            Debug.Log("🎮 Mode: TopView");
        }
    }

    // [★수정] 액션 맵 상태와 상관없이 하드웨어 마우스 좌표 반환
    public Vector2 GetMousePosition()
    {
        if (Mouse.current == null) return Vector2.zero;
        return Mouse.current.position.ReadValue();
    }

    #region Callbacks
    private void MoveCtx(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void JumpCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnJump?.Invoke(); }
    private void FastFallCtx(InputAction.CallbackContext ctx) => OnFastFall?.Invoke(ctx.ReadValueAsButton());
    private void DashCtx(InputAction.CallbackContext ctx) => OnDash?.Invoke(ctx.ReadValueAsButton());
    private void DodgeCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnDodge?.Invoke(); }

    private void ShipMoveCtx(InputAction.CallbackContext ctx) => OnShipMove?.Invoke(ctx.ReadValue<Vector2>());
    private void BoostCtx(InputAction.CallbackContext ctx) => OnBoost?.Invoke(ctx.ReadValueAsButton());
    private void EvasionCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnEvasion?.Invoke(); }

    private void AttackCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnAttack?.Invoke(); }
    private void SkillCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnSkill?.Invoke(); }
    private void InteractCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnInteract?.Invoke(); }
    #endregion
}