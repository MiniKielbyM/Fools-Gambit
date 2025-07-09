using UnityEngine;
using UnityEngine.UI;
public class Dummy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float health = 100;
    public Image healthbar;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        healthbar.fillAmount = health / 100;
        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
    public void minushealth()
    {
        health -= 20;
    }
}
