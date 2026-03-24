public class Menu
{
    public static void Run(){

        while(true)
        {
            DisplayMenu();
            string? input = Console.ReadLine();
            switch(input){
                case "1":
                    RunScheduleSelection();
                    break;
                case "2":
                    Console.WriteLine("Exiting...");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please enter 1 or 2.");
                    break;
            }
        }

    }

    private static void DisplayMenu()
    {
        Console.WriteLine("Job Shop Scheduler");
        Console.WriteLine("==================");
        Console.WriteLine("1. Run Job Shop Scheduler");
        Console.WriteLine("2. Exit");
        Console.Write("Select an option: ");
    }

    private static void DisplaySchedules()
    {
        IReadOnlyList<Schedule> schedules = FileManager.CachedSchedules;

        Console.WriteLine($"{schedules.Count} schedules loaded:");
        for(int i = 0; i < schedules.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {schedules[i].ScheduleName}");
        }
    }

    private static void DisplayAlgorithms()
    {
        Console.WriteLine("Available Algorithms:");
        Console.WriteLine("1. Benchmark Algorithm");
        Console.WriteLine("2. Genetic Algorithm");
    }
    private static void RunScheduleSelection()
        {
            IReadOnlyList<Schedule> schedules = FileManager.CachedSchedules;

            DisplaySchedules();

            int choice;
            while (true)
            {
                Console.Write("Enter selection number: ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out choice) && choice >= 1 && choice <= schedules.Count)
                {
                    break;
                }

                Console.WriteLine($"Invalid selection. Enter a number between 1 and {schedules.Count}.");
            }
            
            Schedule selectedSchedule = schedules[choice - 1];

            Console.WriteLine($"Selected Schedule: {selectedSchedule.ScheduleName}");
            DisplayAlgorithms();
            int algoChoice;
            while (true)
            {
                Console.Write("Enter algorithm number: ");
                string? input = Console.ReadLine();

                if (int.TryParse(input, out algoChoice) && (algoChoice == 1 || algoChoice == 2))
                {
                    break;
                }

                Console.WriteLine("Invalid selection.");
            }
            
        }   

}