using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Aprobado.XR.Regie
{
    // Names using "Régie" are in French to make it clear what it's used for.
    public class ModeRegie : MonoBehaviour
    {
        [SerializeField] private InputActionReference toggleAction;
        [SerializeField] private InputActionReference switchSubModeAction;
        [SerializeField] private List<BaseDeRegie> regies;
        public static bool IsActive;

        private int currentRegie = 0;

        private void OnEnable()
        {
            toggleAction.action.Enable();
            switchSubModeAction.action.Enable();
        }

        private void OnDisable()
        {
            toggleAction.action.Disable();
            switchSubModeAction.action.Disable();
        }

        private void Update()
        {
            var pressed = toggleAction.action.IsPressed();
            if (pressed)
            {
                if (switchSubModeAction.action.WasPressedThisFrame())
                {
                    currentRegie++;
                    if (currentRegie >= regies.Count) currentRegie = 0;
                }
            }
            SetPressed(pressed);
        }

        private void OnDestroy()
        {
            SetPressed(false);
        }

        private void SetPressed(bool pressed)
        {
            IsActive = pressed;
            for (var i = 0; i < regies.Count; i++)
            {
                var active = pressed && currentRegie == i;
                if (regies[i] != null) regies[i].SetActive(active);
            }
        }

        public string GetCurrentRegieLabel()
        {
            if (regies.Count == 0) return "Error: Pas de régie";
            return regies[currentRegie] != null ? regies[currentRegie].Label() : "Error: Régie inexistante";
        }
    }
}