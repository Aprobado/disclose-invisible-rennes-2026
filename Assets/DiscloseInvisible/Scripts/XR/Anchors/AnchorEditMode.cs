using Aprobado.XR.Regie;

namespace Aprobado.XR.Anchors
{
    public class AnchorEditMode : BaseDeRegie
    {
        public static bool IsActive;
        
        public override void SetActive(bool active)
        {
            IsActive = active;
        }
        
        public override string Label()
        {
            return "Ancres";
        }
    }
}
