using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerControl : MonoBehaviour
{
    public float mouseSensitivity = 0.1f;
    public float moveSpeed = 5f;
    public float verticalClampMin = -45f;
    public float verticalClampMax = 70f;
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.05f;
    public bool veiwBob;

    public Transform modelBody;
    public Rigidbody playerRigidbody;
    public Animator playerAnimator;
    public Animation landingAnimation;
    public Transform CameraTransform;

    private Transform playerBody;
    private PlayerInput playerInput;
    private InputAction lookAction;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector3 originalCameraLocalPos;
    private float bobTimer = 0f;
    private float verticalRotation = 0f;

    void Start()
    {
        playerBody = GetComponent<Transform>();

        playerInput = GetComponent<PlayerInput>();
        lookAction = playerInput.actions["Look"];
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        Cursor.lockState = CursorLockMode.Locked;

        originalCameraLocalPos = CameraTransform.localPosition;
    }

    void Update()
    {
        // --- Look ---
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float mouseX = lookInput.x * (mouseSensitivity / 10f);
        float mouseY = lookInput.y * (mouseSensitivity / 10f);

        // Horizontal rotation (player yaw)
        playerBody.Rotate(Vector3.up * mouseX);

        // Vertical rotation (camera pitch)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, verticalClampMin, verticalClampMax);
        CameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // --- Movement ---
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        float threshold = 0.1f;
        Vector2 dir = moveInput.normalized;

        if (moveInput.magnitude < threshold && IsGrounded())
        {
            if (playerAnimator.GetInteger("AnimState") == 10) { }
            playerAnimator.SetInteger("AnimState", 0);
        }
        else if (Approximately(dir, new Vector2(0, 1)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 1);
        else if (Approximately(dir, new Vector2(0, -1)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 2);
        else if (Approximately(dir, new Vector2(-1, 0)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 3);
        else if (Approximately(dir, new Vector2(1, 0)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 4);
        else if (Approximately(dir, new Vector2(0.71f, 0.71f)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 6);
        else if (Approximately(dir, new Vector2(-0.71f, 0.71f)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 5);
        else if (Approximately(dir, new Vector2(0.71f, -0.71f)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 8);
        else if (Approximately(dir, new Vector2(-0.71f, -0.71f)) && IsGrounded())
            playerAnimator.SetInteger("AnimState", 7);
        else if (!IsGrounded())
            playerAnimator.SetInteger("AnimState", 10);

        // Jump
        if (jumpAction.triggered && IsGrounded())
        {
            playerAnimator.SetInteger("AnimState", 9);
            playerRigidbody.AddForce(Vector3.up * 5f, ForceMode.Impulse);
        }

        // Apply Movement
        playerBody.position += playerBody.forward * moveInput.y * moveSpeed * Time.deltaTime;
        playerBody.position += playerBody.right * moveInput.x * moveSpeed * Time.deltaTime;
        modelBody.position = playerBody.position;

        // --- Head Bob ---
        if (veiwBob && moveInput.magnitude > 0.1f && IsGrounded())
        {
            bobTimer += Time.deltaTime * bobFrequency;
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;
            Vector3 newCamPos = originalCameraLocalPos + new Vector3(0, bobOffset, 0);
            CameraTransform.localPosition = newCamPos;
        }
        else
        {
            bobTimer = 0;
            CameraTransform.localPosition = Vector3.Lerp(CameraTransform.localPosition, originalCameraLocalPos, Time.deltaTime * 10f);
        }
    }

    private bool IsGrounded()
    {
        Debug.DrawRay(modelBody.position, Vector3.down * 0.05f, Color.red);
        return Physics.Raycast(modelBody.position + new Vector3(0, 0.05f, 0), Vector3.down, 0.1f);
    }

    private bool Approximately(Vector2 a, Vector2 b)
    {
        return Vector2.Distance(a, b) < 0.05f;
    }
}
