using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    // [핵심 1] 유니티 컴포넌트(PlayerInput) 대신, 네가 만든 클래스를 직접 변수로 씀
    private PlayerInput_Actions input;

    // --- [이벤트] ---
    // SideView (메카)
    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action<bool> OnFastFall;
    public event Action<bool> OnDash;
    public event Action OnDodge;

    // TopView (수송선)
    public event Action<Vector2> OnShipMove;
    public event Action<bool> OnBoost;
    public event Action OnEvasion;

    // 공통
    public event Action OnAttack;
    public event Action OnSkill;
    public event Action OnInteract;
    public event Action OnHeal;

    // 공통 아이템 사용
    public event Action OnUseItem01;
    public event Action OnUseItem02;
    public event Action OnUseItem03;
    public event Action OnUseItem04;

    private void Awake()
    {
        // [핵심 2] 스크립트가 깨어날 때, 키 세팅(인스턴스)을 직접 생성함!
        input = new PlayerInput_Actions();
    }

    private void OnEnable()
    {
        // [핵심 3] 문자열("Jump") 대신 변수로 바로 접근 (오타 걱정 끝!)

        // 1. PlayerSide 연결
        input.PlayerSide.Move.performed += MoveCtx;
        input.PlayerSide.Move.canceled += MoveCtx;
        input.PlayerSide.Jump.performed += JumpCtx;
        input.PlayerSide.FastFall.performed += FastFallCtx;
        input.PlayerSide.FastFall.canceled += FastFallCtx;
        input.PlayerSide.Dash.performed += DashCtx;
        input.PlayerSide.Dash.canceled += DashCtx;
        input.PlayerSide.Dodge.performed += DodgeCtx;

        // 2. PlayerTop 연결
        input.PlayerTop.Move.performed += ShipMoveCtx;
        input.PlayerTop.Move.canceled += ShipMoveCtx;
        input.PlayerTop.Dash.performed += BoostCtx;   // 이름 달라도 연결 쉬움
        input.PlayerTop.Dash.canceled += BoostCtx;
        input.PlayerTop.Dodge.performed += EvasionCtx;

        // 3. 공통 연결 (이름 같아도 각각 명시적으로 연결해주는 게 안전함)
        // (PlayerSide, PlayerTop 둘 다 같은 이름의 Action이 있다면 둘 다 연결)
        // 여기선 편의상 PlayerSide 기준으로 예시를 들게. 
        // 실제로는 두 맵에 다 'Attack'이 있으면 둘 다 연결해줘야 함.
        input.PlayerSide.Attack.performed += AttackCtx;
        input.PlayerTop.Attack.performed += AttackCtx;
        input.PlayerSide.SupportSkill.performed += SkillCtx;
        input.PlayerTop.SupportSkill.performed += SkillCtx;
        input.PlayerSide.Interact.performed += InteractCtx;
        input.PlayerTop.Interact.performed += InteractCtx;
        // input.PlayerSide.Heal.performed += HealCtx; // (액션 있으면 주석 해제)
        // input.PlayerTop.Heal.performed += HealCtx;  // (액션 있으면 주석 해제)
        // input.PlayerSide.UseItem01.performed += UseItem01Ctx;
        // input.PlayerTop.UseItem01.performed += UseItem01Ctx;
        // input.PlayerSide.UseItem02.performed += UseItem02Ctx;
        // input.PlayerTop.UseItem02.performed += UseItem02Ctx;
        // input.PlayerSide.UseItem03.performed += UseItem03Ctx;
        // input.PlayerTop.UseItem03.performed += UseItem03Ctx;
        // input.PlayerSide.UseItem04.performed += UseItem04Ctx;
        // input.PlayerTop.UseItem04.performed += UseItem04Ctx;


        // 중요: 기본적으로 시작할 때는 SideView 맵을 켜둔다.
        input.PlayerSide.Enable();
        input.PlayerTop.Disable();
    }

    private void OnDisable()
    {
        // [핵심 4] 연결 해제도 깔끔하게
        // (여기선 맵 전체를 Disable 해버리면 더 확실함)
        //input.Disable();

        // 하지만 이벤트 구독 해제(-=)는 정석대로 해주는 게 좋음
        // 1. PlayerSide 연결
        input.PlayerSide.Move.performed -= MoveCtx;
        input.PlayerSide.Move.canceled -= MoveCtx;
        input.PlayerSide.Jump.performed -= JumpCtx;
        input.PlayerSide.FastFall.performed -= FastFallCtx;
        input.PlayerSide.FastFall.canceled -= FastFallCtx;
        input.PlayerSide.Dash.performed -= DashCtx;
        input.PlayerSide.Dash.canceled -= DashCtx;
        input.PlayerSide.Dodge.performed -= DodgeCtx;

        // 2. PlayerTop 연결
        input.PlayerTop.Move.performed -= ShipMoveCtx;
        input.PlayerTop.Move.canceled -= ShipMoveCtx;
        input.PlayerTop.Dash.performed -= BoostCtx;   // 이름 달라도 연결 쉬움
        input.PlayerTop.Dash.canceled -= BoostCtx;
        input.PlayerTop.Dodge.performed -= EvasionCtx;

        // 3. 공통 연결 (이름 같아도 각각 명시적으로 연결해주는 게 안전함)
        // (PlayerSide, PlayerTop 둘 다 같은 이름의 Action이 있다면 둘 다 연결)
        // 여기선 편의상 PlayerSide 기준으로 예시를 들게. 
        // 실제로는 두 맵에 다 'Attack'이 있으면 둘 다 연결해줘야 함.
        input.PlayerSide.Attack.performed -= AttackCtx;
        input.PlayerTop.Attack.performed -= AttackCtx;
        input.PlayerSide.SupportSkill.performed -= SkillCtx;
        input.PlayerTop.SupportSkill.performed -= SkillCtx;
        input.PlayerSide.Interact.performed -= InteractCtx;
        input.PlayerTop.Interact.performed -= InteractCtx;
        // input.PlayerSide.Heal.performed -= HealCtx; // (액션 있으면 주석 해제)
        // input.PlayerTop.Heal.performed -= HealCtx;  // (액션 있으면 주석 해제)
        // input.PlayerSide.UseItem01.performed -= UseItem01Ctx;
        // input.PlayerTop.UseItem01.performed -= UseItem01Ctx;
        // input.PlayerSide.UseItem02.performed -= UseItem02Ctx;
        // input.PlayerTop.UseItem02.performed -= UseItem02Ctx;
        // input.PlayerSide.UseItem03.performed -= UseItem03Ctx;
        // input.PlayerTop.UseItem03.performed -= UseItem03Ctx;
        // input.PlayerSide.UseItem04.performed -= UseItem04Ctx;
        // input.PlayerTop.UseItem04.performed -= UseItem04Ctx;

    }

    // 씬 전환 시 호출 (GameManager에서)
    public void SetControlMode(bool isSideView)
    {
        if (isSideView)
        {
            // [핵심 5] 맵 전환을 내가 직접 한다! (확실함)
            input.PlayerTop.Disable();  // 탑뷰 끄고
            input.PlayerSide.Enable();  // 사이드뷰 켜고
            Debug.Log("Set Mode: SideView (Direct Control)");
        }
        else
        {
            input.PlayerSide.Disable(); // 사이드뷰 끄고
            input.PlayerTop.Enable();   // 탑뷰 켜고
            Debug.Log("Set Mode: TopView (Direct Control)");
        }
    }

    #region Callbacks
    // 콜백 함수 내용은 아까랑 100% 똑같음
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
    // private void HealCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnHeal?.Invoke(); }
    // private void UseItem01Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem01?.Invoke(); }
    // private void UseItem02Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem02?.Invoke(); }
    // private void UseItem03Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem03?.Invoke(); }
    // private void UseItem04Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem04?.Invoke(); }
    #endregion
}