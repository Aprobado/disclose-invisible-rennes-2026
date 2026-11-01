using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
public class ObjectLabel : MonoBehaviour
{
    [SerializeField] private GameObject target;
    
    void Start()
    {
        var tmp = GetComponent<TextMeshPro>();
        tmp.text = target != null ? target.name : gameObject.name;
    }
}
