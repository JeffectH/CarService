namespace CarService
{
    internal class Program
    {
        static void Main(string[] args) {

            var parts = new Dictionary<Part, int>
            {
                { new Part(TypePart.Engine, 3000), 2 },
                { new Part(TypePart.AirFilter, 100), 10 },
                { new Part(TypePart.Battery, 500), 5 },
                { new Part(TypePart.BrakingSystem, 1500), 3 },
                { new Part(TypePart.Suspension, 1200), 4 },
                { new Part(TypePart.Transmission, 2000), 2 },
                { new Part(TypePart.FuelPump, 300), 8 },
                { new Part(TypePart.CoolingSystem, 800), 5 },
                { new Part(TypePart.BrakeDiscs, 1000), 4 },
                { new Part(TypePart.SteeringSystem, 1000), 3 }
            };

            var carQueue = new Queue<Car>(new[]
            {
                new Car(ModelCar.Audi),
                new Car(ModelCar.Lada),
                new Car(ModelCar.Mesedez),
                new Car(ModelCar.Mustang)
            });

            var warehouse = new Warehouse(parts);
            var carService = new CarService(10000, warehouse, carQueue);

            carService.Work();
        }
    }

    class CarService
    {
        private const int RepairPercentage = 25;
        private const int FixedPenalty = 500;
        private const int PenaltyForPart = 300;

        private int _cashBalance;
        private Warehouse _warehouse;
        private Queue<Car> _carQueue;

        public CarService(int cashBalance, Warehouse warehouse, Queue<Car> carQueue) {
            _cashBalance = cashBalance;
            _warehouse = warehouse;
            _carQueue = carQueue;
        }

        public void Work() {
            bool isOpen = true;

            while (isOpen){
                Car currentCar = _carQueue.Dequeue();
                Console.Clear();
                Console.WriteLine($"\nАвтосервис открыт! Текущий баланс: {_cashBalance}");
                Console.WriteLine($"Данные со склада: {_warehouse.GetData()}\n");

                Console.WriteLine($"Aвтомобиль марки {currentCar.Model} подъехал на ремонт.");
                Console.WriteLine($"После диагностики произведена оценка работ. Стоимость ремонта {CalculateRepairCost(currentCar)}");

                ShowBrokenParts(currentCar);

                Console.Write("\nБудете ремонтировать автомобиль? 1 - да, 2 - нет: ");
                if (int.TryParse(Console.ReadLine(), out int choice)){
                    if (choice is >= 1 and <= 2){
                        switch (choice){
                            case 1:
                                Console.WriteLine();
                                RepairCar(currentCar);
                                break;

                            case 2:
                                currentCar = _carQueue.Dequeue();
                                continue;
                            default:
                                Console.WriteLine("\n Такой команды нет!");
                                break;
                        }
                    }
                }

                Console.ReadKey();
            }
        }
        private void ShowBrokenParts(Car currentCar) {
            Console.WriteLine("Необходимо отремонтировать следующие детали:");
            var brokenParts = currentCar.GetBrokenParts();
            for (int i = 0; i < brokenParts.Count; i++)
                Console.WriteLine($"{i + 1}. {brokenParts[i].Type}");
        }
        private void RepairCar(Car currentCar) {
            ShowBrokenParts(currentCar);
            var brokenParts = currentCar.GetBrokenParts();
            Console.Write("\nВведите номер детали которую будете ремонтировать:");
            if (!int.TryParse(Console.ReadLine(), out int numberPart))
                return;
            if (numberPart < 1 || numberPart > brokenParts.Count)
                return;
            if (RepairPart(brokenParts[numberPart - 1])){
                brokenParts[numberPart - 1].IsBroken = true;
                Console.WriteLine($"\nДеталь {brokenParts[numberPart - 1].Type} успешно починили");
            }
            else{
                brokenParts[numberPart - 1].IsBroken = false;
                Console.WriteLine($"\nДеталь {brokenParts[numberPart - 1].Type} НЕуспешно починили");
            }
        }

        private float CalculateRepairCost(Car car) {
            float cost = 0;
            var brokenPart = car.GetBrokenParts();
            foreach (Part part in brokenPart){
                int pricePart = _warehouse.GetPricePart(part.Type);
                cost += pricePart;
                cost += pricePart * RepairPercentage / 100f;
            }
            return cost;
        }

        private bool RepairPart(Part part) {
            int repairProbability = UserUtils.GenerateRandomNumber(0, 7);
            return repairProbability <= 3;
        }
    }

    class Warehouse
    {
        public Dictionary<Part, int> _parts;
        public Warehouse(Dictionary<Part, int> parts) {
            _parts = parts;
        }

        public Part GetPart(TypePart partType) {
            foreach (var part in _parts){
                if (part.Key.Type != partType)
                    continue;
                if (part.Value <= 0)
                    continue;
                _parts[part.Key] = part.Value - 1;
                return part.Key;
            }
            return null;
        }

        public string GetData() {
            string data = "";
            foreach (var part in _parts)
                data += $"{part.Key.Type} - {part.Value}; ";
            return data;
        }

        public int GetPricePart(TypePart partType) {
            foreach (var part in _parts){
                if (part.Key.Type == partType)
                    return part.Key.Price;
            }
            throw new KeyNotFoundException($"Деталь типа {partType} не найдена на складе.");
        }
    }

    class Part
    {
        public bool IsBroken;
        public TypePart Type;
        public int Price;

        public Part(TypePart type, int price) {
            Type = type;
            Price = price;
        }
    }

    class Car
    {
        public ModelCar Model;
        private List<Part> _parts = [];

        public Car(ModelCar model) {
            Model = model;

            AssemblingCar();
        }

        private void AssemblingCar() {
            int maxPartsAmount = 10;
            int minPartsAmount = 5;
            int quantity = UserUtils.GenerateRandomNumber(minPartsAmount, maxPartsAmount);

            for (int i = 0; i < quantity; i++){
                var part = new Part((TypePart)i, 0);
                int breakProbability = UserUtils.GenerateRandomNumber(0, 2);
                part.IsBroken = breakProbability > 0;
                _parts.Add(part);
                //количество поломанных будет не меньше 1 детали.
            }
        }
        // Машина состоит из деталей и количество поломанных будет не меньше 1 детали. 
        // Надо показывать все детали, которые поломанные.
        public List<Part> GetBrokenParts() {
            var brokenParts = new List<Part>();
            foreach (Part part in _parts){
                if (part.IsBroken){
                    brokenParts.Add(part);
                }
            }
            return brokenParts;
        }
    }

    public static class UserUtils
    {
        private static readonly Random s_random = new Random();

        public static int GenerateRandomNumber(int min, int max) {
            return s_random.Next(min, max);
        }
    }

    enum TypePart
    {
        Engine,
        BrakingSystem,
        Suspension,
        FuelPump,
        Battery,
        CoolingSystem,
        SteeringSystem,
        AirFilter,
        BrakeDiscs,
        Transmission
    }

    enum ModelCar
    {
        Toyota,
        BMW,
        Lada,
        Audi,
        Mesedez,
        Mustang
    }
}
