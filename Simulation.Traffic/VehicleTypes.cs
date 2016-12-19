using System;

namespace Simulation.Traffic
{
    [Flags]
    public enum VehicleTypes
    {
        None = 0,
        Vehicle = Normal | Truck | Bus | Emergency | Taxi | Hazmat, 
        Normal = 1,
        Truck = 2,
        Bus = 4,
        Bicycle = 8,
        Pedestrian = 16,
        Emergency = 32,
        Taxi = 64,
        Hazmat = 128,
        Tram = 256,
        Train = 512,
    }
}