using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpforce = 5f;
    public bool OnGround = false;
    Rigidbody rb;
    public float XmouseSensitivity = 250f;
    public float YmouseSensitivity = 2;
    public Transform playerBody; // reference to the player's transform
    private float xRotation = 45f;
    public Camera cam;
    public int dir = 0;
    public bool finished = true;
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

        if (Input.GetKey(KeyCode.W)) {
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
        // Calculate movement direction relative to player�s forward
        Vector3 moveDirection = (transform.forward * vertical + transform.right * horizontal).normalized;

        // Preserve Y velocity (gravity / jump)
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z);
       
        // Jump
        if (Input.GetKey(KeyCode.Space) && OnGround)
        {
            rb.AddForce(Vector3.up * jumpforce, ForceMode.Impulse);
            OnGround = false;
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
        float mouseX = Input.GetAxis("Mouse X") * XmouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * XmouseSensitivity * Time.deltaTime;

        // Rotate the camera up and down
        xRotation -= mouseY;
        float camrot = xRotation * YmouseSensitivity;
        camrot = Mathf.Clamp(xRotation, -90f, 90f); // limit looking too far up or down
        playerBody.Rotate(Vector3.up*XmouseSensitivity, mouseX);
        cam.transform.localRotation = Quaternion.Euler(camrot,0, 0f);
        // Rotate the player left and right
    }
}