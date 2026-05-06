namespace Drift
{
    public sealed class GravityPortal : PortalBase
    {
        protected override void Apply(DroneController drone, float effectDuration)
        {
            switch (Kind)
            {
                case PortalKind.GravityInverted:
                    drone.ApplyOrientation(180f, 0f, "GRAVITY FLIP");
                    break;
                case PortalKind.GravitySideways:
                    drone.ApplyOrientation(90f, 0f, "SIDEWAYS GRAVITY");
                    break;
                default:
                    drone.ApplyOrientation(0f, 0f, "NORMAL GRAVITY");
                    break;
            }
        }
    }
}
