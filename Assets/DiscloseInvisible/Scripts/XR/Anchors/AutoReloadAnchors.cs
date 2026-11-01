using System.Collections;
using UnityEngine;

namespace Aprobado.XR.Anchors
{
    [RequireComponent(typeof(SpatialAnchorLoader))]
    public class AutoReloadAnchors : MonoBehaviour
    {
        private SpatialAnchorLoader loader;

        private void Awake()
        {
            loader = FindAnyObjectByType<SpatialAnchorLoader>();
        }

        private void Start()
        {
            StartCoroutine(AutoReloadCoroutine());
        }

        private IEnumerator AutoReloadCoroutine()
        {
            yield return new WaitForSeconds(3f);
            loader.LoadAnchorsFromDefaultLocalStorage();
        }
    }
}