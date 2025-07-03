using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(GunController))]
public class PlayerController : LivingEntity
{
    CharacterController controller;
    InputSystem_Actions playerInputActions;
    Camera viewCamera;
    GunController gunController;

    Vector2 moveInput;      // �洢����Ķ�ά����
    Vector2 lookInput;      // �洢�����Ļλ�õĶ�ά����
    bool isFiring;
    float velocityY;        // ���ڴ洢�ͼ��㴹ֱ�ٶ�

    public float moveSpeed = 6.0f;
    public float turnSpeed = 15.0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        gunController = GetComponent<GunController>();

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();

        // �����ƶ������λ���¼�
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        // ���Ŀ����¼�
        playerInputActions.Player.Attack.performed += OnFirePerformed;
        playerInputActions.Player.Attack.canceled += OnFireCanceled;
    }

    protected override void Start()
    {
        base.Start();

        viewCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        HandleRotation();

        if (isFiring)
        {
            gunController.OnTriggerHold();
        }
        if (!isFiring)
        {
            gunController.OnTriggerRelease();
        }
    }

    void HandleMovement()
    {
        Vector3 horizontalVelocity = new Vector3(moveInput.x, 0, moveInput.y) * moveSpeed;

        if (controller.isGrounded)
        {
            velocityY = -1f;
        }
        else 
        {
            velocityY += Physics.gravity.y * Time.deltaTime;
        }

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * velocityY;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    public void ResetMovementState()
    {
        moveInput = Vector2.zero;
        velocityY = 0f;
    }

    void HandleRotation()
    {
        Ray ray = viewCamera.ScreenPointToRay(lookInput);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    void OnFirePerformed(InputAction.CallbackContext context)
    {
        isFiring = true;
    }

    void OnFireCanceled(InputAction.CallbackContext context)
    {
        isFiring = false;
    }

    void OnEnable()
    {
        playerInputActions.Player.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Disable();

        // ȡ�������¼�����ֹ�ڴ�й©
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Attack.performed -= OnFirePerformed;
        playerInputActions.Player.Attack.canceled -= OnFireCanceled;
    }
}
