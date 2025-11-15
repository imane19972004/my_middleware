using System;

namespace NotificationService
{
    /// <summary>
    /// Point d'entrée du service de notifications
    /// Simule un service externe qui publie des événements (météo, pollution, etc.)
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "NotificationService - Let's Go Biking";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            var publisher = new NotificationPublisher();
            publisher.Start();

            Console.WriteLine("\nAppuyez sur une touche pour quitter...");
            Console.ReadKey();
        }
    }
}