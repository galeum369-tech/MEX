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

    InputSystem_Actions input;

    private void Awake()
    {
        input = new InputSystem_Actions();

        //input.Player.Move.performed += HandleMove;

    }
}
