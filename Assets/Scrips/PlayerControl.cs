using UnityEditor;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpforce = 5f;
    public bool OnGround = false;
    Rigidbody rb;
    public float mouseSensitivity = 250f;

    public Transform playerBody; // reference to the player's transform

    private float xRotation = 0f;
    public Camera cam;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
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
        }
        mousemovement();
    }
    private void OnCollisionEnter(Collision collision)
    {
        OnGround = true;
    }
    void mousemovement()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // limit looking too far up or down

        playerBody.Rotate(Vector3.up*mouseSensitivity, mouseX);
        cam.transform.Rotate(Vector3.right,xRotation);
        // Rotate the player left and r
    }
}