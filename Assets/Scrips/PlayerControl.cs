using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float uplift = 5f;
    public float dashForce = 500f;
    public float dashCoolDown = 0.5f;
    public bool OnGround = false;
    Rigidbody rb;
    public float mouseSensitivity = 250f;
    public Transform playerBody; // reference to the player's transform
    private float xRotation = 45f;
    public Camera cam;
    public int dir = 0;
    public bool finished = true;
    public bool isRunning = false;
    public bool dashOnCoolDown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        /*
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * moveSpeed*Time.deltaTime);
            Debug.Log("W");
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
            Debug.Log("S");
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Space)&& OnGround)
        {
            rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);
            OnGround = false;
        }*/
        mousemovement();
    }
    void FixedUpdate()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        // Get input
        float horizontal = 0f;
        float vertical = 0f;

        //Sprint mechanics
        if (Input.GetKey(KeyCode.LeftShift) && (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
        {
            isRunning = true;
            moveSpeed = 10f;
        }
        else
        {
            isRunning = false;
            moveSpeed = 5f;
        }
        //All directional movement
        if (Input.GetKey(KeyCode.W))
        {
            vertical += 1f;
            dir = 1;
        }
        if (Input.GetKey(KeyCode.S)) {
            vertical -= 1f;
            dir = 2;
        }
        if (Input.GetKey(KeyCode.A)) {
            horizontal -= 1f;
            dir = 3;
        }
        if (Input.GetKey(KeyCode.D)) {
            horizontal += 1f;
            dir = 4;
        }
        if ((!OnGround&&dir!=0)|| !finished)
        {
            if (dir == 1)
            {
                finished = false;
                vertical += 1f;
            }
            if (dir == 2)
            {
                finished = false;
                vertical -= 1f;
            }
            if (dir == 3)
            {
                finished = false;
                horizontal -= 1f;
            }
            if (dir == 4)
            {
                finished = false;
                horizontal += 1f;
            }
        }
        if(finished) dir = 0;
        // Calculate movement direction relative to player's forward
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        // Preserve Y velocity (gravity / jump)
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);

        DashPlayer(moveDirection);
        // Jump
        if (Input.GetKey(KeyCode.Space) && OnGround)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            OnGround = false;
        }
    }

    void ResetDash()
    {
        dashOnCoolDown = false;
    }

    void DashPlayer(Vector3 moveDirection)
    {
        // Dash
        if (Input.GetKey(KeyCode.LeftAlt) && OnGround && dashOnCoolDown==false)
        {
            rb.linearVelocity = moveDirection * dashForce;
            dashOnCoolDown = true;
            Invoke(nameof(ResetDash),dashCoolDown);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        OnGround = true;
        finished = true;
    }
    void mousemovement()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        // Rotate the camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // limit looking too far up or down

        playerBody.Rotate(Vector3.up, mouseX);
        cam.transform.localRotation = Quaternion.Euler(xRotation,0, 0f);
        // Rotate the player left and right
    }
}