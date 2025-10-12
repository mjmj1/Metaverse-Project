using UnityEngine;

public class TestScripts : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        originalColor = rend.material.color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hammer")) // 두더지에 Tag 부여
        {
            if (rend != null)
                rend.material.color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Hammer"))
        {
            if (rend != null)
                rend.material.color = originalColor;
        }
    }
}
