using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class BasicMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float movementSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("视角设置")]
    public float mouseSensitivity = 100f;
    public Transform cameraPivot;
    public Camera mainCamera;

    [Header("视角限制")]
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 70f;

    [Header("视角抖动")]
    public float shakeAmount = 0.1f;
    public float shakeFrequency = 10f;
    public float shakeSmooth = 5f;

    private CharacterController controller;
    private float horizontalRotation;
    private float verticalRotation;
    private Vector3 initialCameraPos;
    private Vector3 targetShakeOffset;
    private Vector3 velocity;
    private float verticalVelocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        initialCameraPos = mainCamera.transform.localPosition;
    }

    void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleRotation();
        HandleJump();
        UpdateShake();
    }

    void HandleGroundCheck()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        controller.Move(move * movementSpeed * Time.deltaTime);

        // 应用重力
        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        horizontalRotation += mouseX;
        verticalRotation = Mathf.Clamp(verticalRotation - mouseY, minVerticalAngle, maxVerticalAngle);

        transform.rotation = Quaternion.Euler(0, horizontalRotation, 0);
        cameraPivot.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void UpdateShake()
    {
        // 当移动时产生抖动
        if (controller.velocity.magnitude > 0.1f && isGrounded)
        {
            float shake = Mathf.Sin(Time.time * shakeFrequency) * shakeAmount;
            targetShakeOffset = Vector3.up * shake;
        }
        else
        {
            targetShakeOffset = Vector3.Lerp(targetShakeOffset, Vector3.zero, Time.deltaTime * shakeSmooth);
        }

        // 应用抖动到摄像机位置
        mainCamera.transform.localPosition = Vector3.Lerp(
            mainCamera.transform.localPosition,
            initialCameraPos + targetShakeOffset,
            Time.deltaTime * shakeSmooth
        );
    }
}