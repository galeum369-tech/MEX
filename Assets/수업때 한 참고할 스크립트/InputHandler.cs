//using UnityEngine;
//using System;
//using UnityEngine.InputSystem;

//public class InputHandler : MonoBehaviour
//{
//    //이벤트 정의
//    public event Action<Vector2> OnMove;
//    public event Action OnAttack;

//    //입력 액션 참조
//    InputSystem_Actions input;

//    private void Awake()
//    {
//        input = new InputSystem_Actions();

//        input.Player.Move.performed += HandleMove;
//        input.Player.Move.canceled += HandleMove;
//        input.Player.Attack.performed += HandleAttack;
//    }

    

//    private void OnEnable()
//    {
//        input.Enable();
//    }
//    private void OnDisable()
//    {
//        input.Disable();
//    }

//    private void OnDestroy()
//    {
//        input?.Dispose();
//    }

//    #region 입력 이벤트 핸들러
//    void HandleMove(InputAction.CallbackContext context)
//    {
//        OnMove?.Invoke(context.ReadValue<Vector2>());
//    }

//    void HandleAttack(InputAction.CallbackContext context)
//    {
//        OnAttack?.Invoke();
//    }
//    #endregion
//}
