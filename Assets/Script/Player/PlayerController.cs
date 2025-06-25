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

    Vector2 moveInput;      // 存储输入的二维向量
    Vector2 lookInput;      // 存储鼠标屏幕位置的二维向量
    bool isFiring;

    public float moveSpeed = 6.0f;
    public float turnSpeed = 15.0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        gunController = GetComponent<GunController>();

        playerInputActions = new InputSystem_Actions();
        playerInputActions.Player.Enable();

        // 订阅移动与鼠标位置事件
        playerInputActions.Player.Move.performed += OnMove;
        playerInputActions.Player.Move.canceled += OnMove;
        playerInputActions.Player.Look.performed += OnLook;
        // 订阅开火事件
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
        // 创建一个三维移动向量，忽略Y轴(只在水平面移动)
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    void HandleRotation()
    {
        // 从相机发射一条射线到鼠标在屏幕上的位置
        Ray ray = viewCamera.ScreenPointToRay(lookInput);

        // 创建一个无限大的、法线向上的平面，位置在世界坐标原点
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        // 如果射线与平面相交
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // 获取射线与平面的交点
            Vector3 point = ray.GetPoint(rayDistance);

            // 计算从玩家位置指向目标点的方向
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0;

            if (lookDirection != Vector3.zero)
            {
                // 创建一个朝向目标方向的旋转并使用Slerp(球面线性插值)平滑地将当前旋转过渡到目标旋转
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
        // 当脚本组件启用时，确保Action Map是启用的
        playerInputActions.Player.Enable();
    }

    void OnDisable()
    {
        playerInputActions.Player.Disable();

        // 取消订阅事件，防止内存泄漏
        playerInputActions.Player.Move.performed -= OnMove;
        playerInputActions.Player.Move.canceled -= OnMove;
        playerInputActions.Player.Look.performed -= OnLook;
        playerInputActions.Player.Attack.performed -= OnFire;
        playerInputActions.Player.Attack.canceled -= OnFire;
    }
}
