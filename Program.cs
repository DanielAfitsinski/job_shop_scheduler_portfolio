
namespace JobShopScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                FileManager.PreloadJobFiles();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load schedules at startup: {ex.Message}");
                return;
            }

            Menu.Run();
        }

    }
}