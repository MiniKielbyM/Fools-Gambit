using UnityEngine;

public class ViewBobbing : MonoBehaviour
{
    public float bobbingSpeed = 6f;
    public float bobbingAmount = 0.05f;
    private float defaultYPos;
    private float timer = 0f;
    public bool grounded;
    void Start()
    {
        defaultYPos = transform.localPosition.y;
    }

    void Update()
    {
        grounded = gameObject.GetComponentInParent<PlayerControl>().OnGround;
        // Check if any movement keys are pressed
        bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                        Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMoving&& grounded)
        {
            timer += Time.deltaTime * bobbingSpeed;
            float newY = defaultYPos + Mathf.Sin(timer) * bobbingAmount;
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
        else
        {
            // Not moving — smoothly return to default position
            timer = 0f;
            float newY = Mathf.Lerp(transform.localPosition.y, defaultYPos, Time.deltaTime * bobbingSpeed);
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }
}
