using UnityEngine;
using UnityEngine.InputSystem;
public class hitdummy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created#
    public float range = 100;
    public Camera cam;
  AudioSource gunsound;
    void Start()
    {
        gunsound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            hit();
            Debug. Log ("shot");
        }
    }
    void hit()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        gunsound.Play();
        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.Log(hit.transform.name);
            GameObject hitGameObject = hit.transform.gameObject;
            if (hitGameObject.CompareTag("dummy"))
            {
                hitGameObject.GetComponent<Dummy>().minushealth();
                Debug.Log("hit");
            }

        }

    }
    public void OnAttack(InputAction.CallbackContext context)
    {
            hit();
            Debug.Log("shot");   
    }
}