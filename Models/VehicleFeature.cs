namespace ArabamTR.Models
{
    public class VehicleFeature
    {
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;

        public int FeatureId { get; set; }
        public virtual CarFeature Feature { get; set; } = null!;
    }
}
