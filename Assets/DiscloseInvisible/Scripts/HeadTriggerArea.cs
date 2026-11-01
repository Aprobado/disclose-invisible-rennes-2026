using UnityEngine;
using UnityEngine.Events;

public class HeadTriggerArea : MonoBehaviour
{
    public UnityEvent TriggerEnterEvent;
    public UnityEvent TriggerExitEvent;
    
    private bool isInside;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PlayerHead")) return;
        TriggerEnterEvent?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("PlayerHead")) return;
        TriggerExitEvent?.Invoke();
    }
}
