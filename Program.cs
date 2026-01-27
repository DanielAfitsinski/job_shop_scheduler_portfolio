
namespace JobShopScheduler
{
    class Program
    {
        static void Main(string[] args)
        {

            List<Schedule> schedules = FileManager.LoadJobFiles();

            Console.WriteLine("Welcome to the Job Shop Scheduler!");
            Console.WriteLine($"{schedules.Count} schedules loaded.");
            Console.WriteLine("Please select a schedule to view details:");

            for(int i = 0; i < schedules.Count; i++)
            {
                Console.WriteLine($"{i + 1}: {schedules[i].ScheduleName}");
            }

            
            int choice = int.Parse(Console.ReadLine());
            
            Schedule selectedSchedule = schedules[choice - 1];

            Console.WriteLine($"You selected: {selectedSchedule.ScheduleName}");
            
        }
    }
}