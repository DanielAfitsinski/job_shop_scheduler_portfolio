
namespace JobShopScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Schedule> schedules;

            try
            {
                schedules = FileManager.LoadJobFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load schedules: {ex.Message}");
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
                return;
            }

            if (schedules.Count == 0)
            {
                Console.WriteLine("No schedules were found.");
                Console.WriteLine("Press Enter to exit...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Welcome to the Job Shop Scheduler!");
            Console.WriteLine($"{schedules.Count} schedules loaded.");
            Console.WriteLine("Please select a schedule to view details:");

            for(int i = 0; i < schedules.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {schedules[i].ScheduleName}");
            }

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

            Console.WriteLine($"You selected: {selectedSchedule.ScheduleName}");
            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
            
        }
    }
}