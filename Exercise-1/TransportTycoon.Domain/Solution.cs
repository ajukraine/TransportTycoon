using System;
using System.Collections.Generic;

namespace TransportTycoon.Domain
{
    public class Solution
    {
        public static int Solve(string input)
        {
            var factory = new Queue<char>(input);
            var port = new Queue<char>();
            var warehouseA = new Queue<char>();
            var warehouseB = new Queue<char>();

            var map = new Dictionary<string, Track>()
            {
                ["PORT"] = new Track("Factory", "Port", 1),
                ["A"] = new Track("Port", "Warehouse A", 4),
                ["B"] = new Track("Factory", "Warehouse B", 5)
            };

            var vehicles = new Vehicle[]
            {
                new Vehicle(VehicleType.Truck, "Factory"),
                new Vehicle(VehicleType.Truck, "Factory"),
                new Vehicle(VehicleType.Ship, "Port")
            };

            IEnumerable<Order> MakeOrders(Vehicle vehicle)
            {
                if (!vehicle.CanAssignOrders()) yield break;

                if (vehicle.Location == "Factory" && vehicle.Type == VehicleType.Truck)
                {
                    if (!factory.TryDequeue(out var cargo)) yield break;

                    if (cargo == 'A')
                    {
                        yield return new Order(map["PORT"], () => port.Enqueue(cargo));
                        yield return new Order(map["PORT"].Backward());
                    }
                    if (cargo == 'B')
                    {
                        yield return new Order(map["B"], () => warehouseB.Enqueue(cargo));
                        yield return new Order(map["B"].Backward());
                    }
                }

                if (vehicle.Location == "Port" && vehicle.Type == VehicleType.Ship)
                {
                    if (!port.TryDequeue(out var cargo)) yield break;
                    
                    if (cargo == 'A')
                    {
                        yield return new Order(map["A"], () => warehouseA.Enqueue(cargo));
                        yield return new Order(map["A"].Backward());
                    }
                }
            }

            var time = new Time();

            int numberOfContainers = factory.Count;
            bool AllContainersAreDelivered() =>
                numberOfContainers == warehouseA.Count + warehouseB.Count;

            while (!AllContainersAreDelivered() && !time.HasExpired())
            {
                foreach(var vehicle in vehicles)
                    vehicle.AssignOrders(MakeOrders(vehicle));

                foreach(var vehicle in vehicles)
                    vehicle.Move();

                time.NextTick();
            }

            return time.CurrentTime;
        }   
    }

    class Time
    {
        public int CurrentTime { get; private set; }
        private int _expirationTime;

        public Time(int expirationTime = 1000) => _expirationTime = expirationTime;

        public void NextTick() => ++CurrentTime;

        public bool HasExpired() => CurrentTime >= _expirationTime;
    }

    class Track
    {
        public string StartLocation { get; set; }
        public string FinishLocation { get; set; }
        public int TravelTime { get; set; }

        public Track(string startLocation, string finishLocation, int travelTime) =>
            (StartLocation, FinishLocation, TravelTime) = (startLocation, finishLocation, travelTime);
        
        private Track _backwardTrack;
        public Track Backward() =>
            (_backwardTrack ??= new Track(FinishLocation, StartLocation, TravelTime));
    }

    class Order
    {
        public static readonly string IN_PROGRESS = "IN_PROGRESS";

        private int _timeSpentOnTrack;
        private Track _track;
        private Action _finishCallback;

        public Order(Track track, Action finishCallback = null) =>
            (_track, _timeSpentOnTrack, _finishCallback) = (track, 0, finishCallback);

        public bool IsFinished() => _track.TravelTime == _timeSpentOnTrack;

        public bool HasStarted() => _timeSpentOnTrack != 0;

        public string GetCurrentLocation()
        {
            if (!HasStarted()) return _track.StartLocation;
            if (IsFinished()) return _track.FinishLocation;

            return IN_PROGRESS;
        }

        public void Proceed()
        {
            ++_timeSpentOnTrack;

            if (IsFinished())
                _finishCallback?.Invoke();
        }
    }

    public enum VehicleType { Truck, Ship }

    class Vehicle
    {
        private Queue<Order> _orders;
        private Order _currentOrder;

        public VehicleType Type { get; set; }

        public string Location { get; set; }

        public Vehicle(VehicleType type, string location) =>
            (Location, Type, _orders) = (location, type, new Queue<Order>());

        public void AssignOrders(IEnumerable<Order> orders)
        {
            foreach(var order in orders)
                _orders.Enqueue(order);
        }

        public bool CanAssignOrders() => _orders.Count == 0 && (_currentOrder?.IsFinished() ?? true);

        public void Move()
        {
            if (_currentOrder == null || _currentOrder.IsFinished())
                if (!_orders.TryDequeue(out _currentOrder))
                    return;

            _currentOrder.Proceed();
            Location = _currentOrder.GetCurrentLocation();
        }
    }
}