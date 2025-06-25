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
        playerInputActions.Player.Attack.performed += OnFire;
        playerInputActions.Player.Attack.canceled += OnFire;
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
            gunController.Shoot();
        }
    }

    void HandleMovement()
    {
        // ����һ����ά�ƶ�����������Y��(ֻ��ˮƽ���ƶ�)
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void HandleRotation()
    {
        // ���������һ�����ߵ��������Ļ�ϵ�λ��
        Ray ray = viewCamera.ScreenPointToRay(lookInput);

        // ����һ�����޴�ġ��������ϵ�ƽ�棬λ������������ԭ��
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        // ���������ƽ���ཻ
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // ��ȡ������ƽ��Ľ���
            Vector3 point = ray.GetPoint(rayDistance);

            // ��������λ��ָ��Ŀ���ķ���
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                // ����һ������Ŀ�귽�����ת��ʹ��Slerp(�������Բ�ֵ)ƽ���ؽ���ǰ��ת���ɵ�Ŀ����ת
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

    void OnFire(InputAction.CallbackContext context)
    {
        isFiring = context.performed;
    }

    void OnEnable()
    {
        // ���ű��������ʱ��ȷ��Action Map�����õ�
        playerInputActions.Player.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Disable();

        // ȡ�������¼�����ֹ�ڴ�й©
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Attack.performed -= OnFire;
        playerInputActions.Player.Attack.canceled -= OnFire;
    }
}
