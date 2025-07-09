using UnityEngine;
using UnityEngine.InputSystem;
public class hitdummy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created#
    public float range = 100;
    public Camera cam;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    }
    void hit()
    {
        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, range))
        {
            GameObject hitGameObject = hit.transform.gameObject;
            if (hitGameObject == gameObject.CompareTag("dummy"))
            {
                hitGameObject.GetComponent<Dummy>().minushealth();
                Debug.Log("hit");
            }

        }

    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            hit();
            Debug.Log("shot");
        }
    }
}