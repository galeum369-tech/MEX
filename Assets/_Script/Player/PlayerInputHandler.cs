using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action OnAttack;
    public event Action OnSupportSkill;   // 우클릭
    public event Action OnDash;           // Shift
    public event Action OnDodge;          // Space
    public event Action OnHeal;            // E
    public event Action OnInteract;        // F

    PlayerCon input;

    private void Awake()
    {
        input = new PlayerCon();

        input.Player.Move.performed += HandleMove;
        input.Player.Move.canceled += HandleMove;
        input.Player.Attack.performed += HandleAttack;
        input.Player.SupportSkill.performed += HandleSupportSkill;
        input.Player.Dash.performed += HandleDash;
        input.Player.Dash.canceled += HandleDash;
        input.Player.Dodge.performed += HandleDodge;
        input.Player.Heal.performed += HandleHeal;
        input.Player.Interact.performed += HandleInteract;
    }

    private void OnEnable()
    {
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    private void OnDestroy()
    {
        input?.Dispose();
    }

    #region 입력 이벤트 핸들러
    void HandleMove(InputAction.CallbackContext context)
    {
        OnMove?.Invoke(context.ReadValue<Vector2>());
    }
    void HandleAttack(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke();
    }
    void HandleSupportSkill(InputAction.CallbackContext context)
    {
        OnSupportSkill?.Invoke();
    }
    void HandleDash(InputAction.CallbackContext context)
    {
        OnDash?.Invoke();
    }
    void HandleDodge(InputAction.CallbackContext context)
    {
        OnDodge?.Invoke();
    }
    void HandleHeal(InputAction.CallbackContext context)
    {
        OnHeal?.Invoke();
    }
    void HandleInteract(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }
    #endregion
}
