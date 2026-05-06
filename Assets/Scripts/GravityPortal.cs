namespace Drift
{
    public sealed class GravityPortal : PortalBase
    {
        protected override void Apply(DroneController drone, float effectDuration)
        {
            switch (Kind)
            {
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
