using System;
using System.Collections.Generic;
using System.Linq;

namespace TransportTycoon.ConsoleApp
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
                new Vehicle("Truck 1", "Factory"),
                new Vehicle("Truck 2", "Factory"),
                new Vehicle("Ship", "Port")
            };
            
            IEnumerable<Order> MakeOrders(Vehicle vehicle)
            {
                if (!vehicle.CanTakeOrders()) yield break;

                if (vehicle.Location == "Factory" && vehicle.Name != "Ship")
                {
                    if (!factory.TryDequeue(out var cargo)) yield break;

                    if (cargo == 'A')
                    {
                        yield return new Order(map["PORT"], () => port.Enqueue('A'));
                        yield return new Order(map["PORT"], true);
                    }
                    if (cargo == 'B')
                    {
                        yield return new Order(map["B"], () => warehouseB.Enqueue('B'));
                        yield return new Order(map["B"], true);
                    }
                }

                if (vehicle.Location == "Port" && vehicle.Name == "Ship")
                {
                    if (!port.TryDequeue(out var cargo)) yield break;
                    
                    if (cargo == 'A')
                    {
                        yield return new Order(map["A"], () => warehouseA.Enqueue('A'));
                        yield return new Order(map["A"], true);
                    }
                }
            }

            var time = 0;

            int numberOfContainers = factory.Count;
            bool AllContainersAreDelivered() =>
                numberOfContainers == warehouseA.Count + warehouseB.Count;

            while (!AllContainersAreDelivered() && time != -1)
            {
                foreach(var vehicle in vehicles)
                {
                    vehicle.AssignOrders(MakeOrders(vehicle));
                }

                foreach(var vehicle in vehicles)
                {
                    vehicle.ForwardTime();
                }

                if (++time > 1000) time = -1;
            }

            return time;
        }   
    }

    class Track
    {
        public string LocationA { get; set; }

        public string LocationB { get; set; }

        public int TravelTime { get; set; }

        public Track(string pointA, string pointB, int travelTime) =>
            (LocationA, LocationB, TravelTime) = (pointA, pointB, travelTime);
    }

    class Order
    {
        public static readonly string InProgress = "IN_PROGRESS";

        private int _timeOnTrack;

        private Track _track;

        private bool _isBackward;

        private Action _finishCallback;

        public Order(Track track, bool isBackward = false) =>
            (_track, _isBackward, _timeOnTrack) = (track, isBackward, 0);

        public Order(Track track, Action finishCallback) : this(track)
        {
            _finishCallback = finishCallback;
        }

        public bool IsFinished() => _track.TravelTime == _timeOnTrack;

        public bool HasStarted() => _timeOnTrack != 0;

        public string GetCurrentLocation()
        {
            if (!HasStarted()) return _isBackward ? _track.LocationB : _track.LocationA;
            if (IsFinished()) return _isBackward ? _track.LocationA : _track.LocationB;

            return InProgress;
        }

        public void ForwardTime()
        {
            ++_timeOnTrack;

            if (IsFinished())
                _finishCallback?.Invoke();
        }
    }

    class Vehicle
    {
        private Queue<Order> _orders;

        private Order _currentOrder;

        public string Location { get; set; }

        public string Name { get; set; }

        public Vehicle(string name, string location)
        {
            (Name, Location) = (name, location);

            _orders = new Queue<Order>();
        }

        public void AssignOrders(IEnumerable<Order> orders)
        {
            foreach(var order in orders)
                _orders.Enqueue(order);
        }

        public bool CanTakeOrders() => _orders.Count == 0 && (_currentOrder?.IsFinished() ?? true);

        public void ForwardTime()
        {
            if (_currentOrder == null || _currentOrder.IsFinished())
            {
                var tookNewOrder = _orders.TryDequeue(out _currentOrder);
                if (!tookNewOrder) return;
            }

            _currentOrder.ForwardTime();
            Location = _currentOrder.GetCurrentLocation();
        }
    }
}