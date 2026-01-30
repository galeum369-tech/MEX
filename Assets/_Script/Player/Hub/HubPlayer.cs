using UnityEngine;


[RequireComponent(typeof(PlayerInputHandler))]
public class HubPlayer : MonoBehaviour
{
    PlayerInputHandler inputHandler;
    HubMoveController mc;
    //Animcontroller ac;

    Rigidbody2D rb;

    //허브는 스탯 데이터가 따로 없음 해서 이 스크립트에서 직접 설정
    [Header("허브 플레이어 스탯")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float fastFallSpeed = 15f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputHandler = GetComponent<PlayerInputHandler>();
        mc = new HubMoveController(rb);
        //ac = new Animcontroller(GetComponent<Animator>());
    }

    private void Start()
    {
        // true = SideView (인간/메카닉), false = TopView (수송선)
        if (inputHandler != null)
        {
            inputHandler.SetControlMode(true); // 허브는 사이드뷰
        }
    }

    #region 입력 이벤트 구독/해제 허브에는 이동, 점프, 상호작용정도만 필요
    private void OnEnable()
    {
        inputHandler.OnMove += HandleMove;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnFastFall += HandleFastFall;
        inputHandler.OnInteract += HandleInteract;
    }
    private void OnDisable()
    {
        inputHandler.OnMove -= HandleMove;
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnFastFall -= HandleFastFall;
        inputHandler.OnInteract -= HandleInteract;
    }
    #endregion

    #region 이벤트 콜백
    void HandleMove(Vector2 input)
    {
        mc.Move(input.x, moveSpeed);

        FlipDirectionX(input.x);

        //ac.SetMoveAnim(xDirection);
    }

    void HandleJump()
    {
        mc.Jump(jumpForce);
    }

    void HandleFastFall(bool isPressed)
    {
        if (isPressed)
        {
            mc.FastFall(fastFallSpeed);
        }
    }
    void HandleInteract()
    {
        Debug.Log("Interact!");
    }
    #endregion
    
    void FlipDirectionX(float xDir)
    {
        if(xDir >0) transform.localScale = new Vector3(1, 1, 1);
        else if(xDir <0) transform.localScale = new Vector3(-1, 1, 1);
    }
}

