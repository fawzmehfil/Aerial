namespace Drift
{
    public sealed class SpeedPortal : PortalBase
    {
        protected override void Apply(DroneController drone, float effectDuration)
        {
            switch (Kind)
            {
                case PortalKind.SpeedFast:
                    drone.ApplySpeedMultiplier(1.38f, effectDuration, "SPEED UP");
                    break;
                case PortalKind.SpeedSlow:
                    drone.ApplySpeedMultiplier(0.68f, effectDuration, "SLOW MODE");
                    break;
                default:
                    drone.ApplySpeedMultiplier(1f, 0f, "NORMAL SPEED");
                    break;
            }
        }
    }
}
