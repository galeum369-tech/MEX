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
    public event Action<bool> OnAttackHold;
    public event Action OnSkill;
    public event Action OnInteract; // [★추가] 상호작용 키 (F)

    public event Action OnInventory; // [★추가] 인벤토리 키 (Tab)

    public event Action OnQuickSlot1; // [★추가] 퀵슬롯 1 (1)
    public event Action OnQuickSlot2; // [★추가] 퀵슬롯 2 (2)
    public event Action OnQuickSlot3;
    public event Action OnQuickSlot4; // [★추가] 퀵슬롯 4 (4)

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
        input.PlayerSide.Inventory.performed += InventoryCtx;
        input.PlayerSide.QuickSlot1.performed += QuickSlot1Ctx;
        input.PlayerSide.QuickSlot2.performed += QuickSlot2Ctx;
        input.PlayerSide.QuickSlot3.performed += QuickSlot3Ctx;
        input.PlayerSide.QuickSlot4.performed += QuickSlot4Ctx;


        // 2. TopView 연결
        input.PlayerTop.Move.performed += ShipMoveCtx;
        input.PlayerTop.Move.canceled += ShipMoveCtx;
        input.PlayerTop.Dash.performed += BoostCtx;
        input.PlayerTop.Dash.canceled += BoostCtx;
        input.PlayerTop.Dodge.performed += EvasionCtx;
        input.PlayerTop.Attack.performed += AttackHoldCtx;
        input.PlayerTop.Attack.canceled += AttackHoldCtx;
        input.PlayerTop.SupportSkill.performed += SkillCtx;
        input.PlayerTop.Interact.performed += InteractCtx; // [★연결]
        input.PlayerTop.Inventory.performed += InventoryCtx;
        input.PlayerTop.QuickSlot1.performed += QuickSlot1Ctx;
        input.PlayerTop.QuickSlot2.performed += QuickSlot2Ctx;
        input.PlayerTop.QuickSlot3.performed += QuickSlot3Ctx;
        input.PlayerTop.QuickSlot4.performed += QuickSlot4Ctx;

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
        input.PlayerSide.Inventory.performed -= InventoryCtx;
        input.PlayerSide.QuickSlot1.performed -= QuickSlot1Ctx;
        input.PlayerSide.QuickSlot2.performed -= QuickSlot2Ctx;
        input.PlayerSide.QuickSlot3.performed -= QuickSlot3Ctx;
        input.PlayerSide.QuickSlot4.performed -= QuickSlot4Ctx;

        input.PlayerTop.Move.performed -= ShipMoveCtx;
        input.PlayerTop.Move.canceled -= ShipMoveCtx;
        input.PlayerTop.Dash.performed -= BoostCtx;
        input.PlayerTop.Dash.canceled -= BoostCtx;
        input.PlayerTop.Dodge.performed -= EvasionCtx;
        input.PlayerTop.Attack.performed -= AttackHoldCtx;
        input.PlayerTop.Attack.canceled -= AttackHoldCtx;
        input.PlayerTop.SupportSkill.performed -= SkillCtx;
        input.PlayerTop.Interact.performed -= InteractCtx;
        input.PlayerTop.Inventory.performed -= InventoryCtx;
        input.PlayerTop.QuickSlot1.performed -= QuickSlot1Ctx;
        input.PlayerTop.QuickSlot2.performed -= QuickSlot2Ctx;
        input.PlayerTop.QuickSlot3.performed -= QuickSlot3Ctx;
        input.PlayerTop.QuickSlot4.performed -= QuickSlot4Ctx;

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
    
    private void AttackHoldCtx(InputAction.CallbackContext ctx) => OnAttackHold?.Invoke(ctx.ReadValueAsButton());

    private void SkillCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnSkill?.Invoke(); }
    private void InteractCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnInteract?.Invoke(); }

    private void InventoryCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnInventory?.Invoke(); }

    private void QuickSlot1Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnQuickSlot1?.Invoke(); }

    private void QuickSlot2Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnQuickSlot2?.Invoke(); }

    private void QuickSlot3Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnQuickSlot3?.Invoke(); }

    private void QuickSlot4Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnQuickSlot4?.Invoke(); }
    #endregion
}