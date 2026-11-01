using UnityEngine;
using UnityEngine.InputSystem;

namespace Aprobado.XR.Regie
{
    public interface IRegie
    {
        public void SetActive(bool active);
        public string Label();
    }
    
    public abstract class BaseDeRegie : MonoBehaviour, IRegie
    {
        [SerializeField] private InputActionReference toggleAction;
        
        // public static bool IsActive;
        public virtual void SetActive(bool active)
        {
            // IsActive = active;
        }
        public virtual string Label()
        {
            return "Régie Sans Nom";
        }
        
        // Regies can be used individually if "ModeRegie" is not used
        protected virtual void OnEnable()
        {
            if (toggleAction != null) toggleAction.action.Enable();
        }
        
        protected virtual void OnDisable()
        {
            if (toggleAction != null) toggleAction.action.Disable();
        }
        
        protected virtual void Update()
        {
            if (toggleAction == null) return;
            SetActive(toggleAction.action.IsPressed());
        }
        
        protected virtual void OnDestroy()
        {
            SetActive(false);
        }
    }
}