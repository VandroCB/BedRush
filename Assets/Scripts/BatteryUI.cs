using UnityEngine;
using UnityEngine.UI;

// Shows the battery level of the flashlight as a horizontal bar. The bar is filled when the battery is full, and empty when the battery is empty.
public class BatteryUI : MonoBehaviour
{
    [SerializeField] private Flashlight flashlight;
    [Tooltip("UI Image with Image Type = Filled (Fill Method = Horizontal)")]
    [SerializeField] private Image fill;

    // Update is called once per frame
    void Update()
    {
        fill.fillAmount = flashlight.Battery01;
    }
}
