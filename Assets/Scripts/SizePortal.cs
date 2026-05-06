namespace Drift
{
    public sealed class SizePortal : PortalBase
    {
        protected override void Apply(DroneController drone, float effectDuration)
        {
            switch (Kind)
            {
                case PortalKind.SizeSmall:
                    drone.ApplySize(0.62f, effectDuration, "MINI DRONE");
                    break;
                case PortalKind.SizeLarge:
                    drone.ApplySize(1.22f, effectDuration, "LARGE DRONE");
                    break;
                default:
                    drone.ApplySize(1f, 0f, "NORMAL SIZE");
                    break;
            }
        }
    }
}
