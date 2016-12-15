namespace Simulation.Traffic
{
    public class LaneDescription
    {
        /// <summary>
        /// Speed in km/h
        /// </summary>
        public float MaxSpeed { get; set; }

        public bool Reverse { get; set; }

        public Turn Turn { get; set; }
        public float Width { get; set; }

        public VehicleTypes VehicleTypes { get; set; } = VehicleTypes.Vehicle;
    }
}