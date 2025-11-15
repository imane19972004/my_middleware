using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace WebServer
{
    class Program
    {
        private static HttpListener _listener;
        private static string _webRoot;

        static void Main(string[] args)
        {
            // Chemin vers votre dossier LetsGoBikingWeb
            _webRoot = Path.Combine(
                Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName,
                "LetsGoBikingWeb"
            );

            Console.WriteLine($"📁 Dossier web: {_webRoot}");

            if (!Directory.Exists(_webRoot))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ ERREUR: Dossier LetsGoBikingWeb introuvable !");
                Console.ResetColor();
                Console.WriteLine($"   Attendu: {_webRoot}");
                Console.WriteLine("\nAppuyez sur une touche pour quitter...");
                Console.ReadKey();
                return;
            }

            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8080/");

            try
            {
                _listener.Start();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("╔════════════════════════════════════════════╗");
                Console.WriteLine("║       WebServer - DÉMARRÉ ✅              ║");
                Console.WriteLine("╚════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine("\n🌐 Serveur web lancé sur:");
                Console.WriteLine("   http://localhost:8080/");
                Console.WriteLine("   http://localhost:8080/index.html");
                Console.WriteLine("\n📂 Fichiers servis depuis:");
                Console.WriteLine($"   {_webRoot}");
                Console.WriteLine("\n   Appuyez sur ENTRÉE pour arrêter.\n");

                // Thread pour écouter les requêtes
                Thread listenerThread = new Thread(HandleRequests);
                listenerThread.Start();

                Console.ReadLine();

                _listener.Stop();
                listenerThread.Join();

                Console.WriteLine("🛑 WebServer arrêté.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ ERREUR: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nAppuyez sur une touche pour quitter...");
                Console.ReadKey();
            }
        }

        static void HandleRequests()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = _listener.GetContext();
                    ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    if (_listener.IsListening)
                    {
                        Console.WriteLine($"⚠️ Erreur requête: {ex.Message}");
                    }
                }
            }
        }

        static void ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // Extraire le chemin du fichier demandé
            string requestPath = request.Url.AbsolutePath.TrimStart('/');

            // Par défaut, servir index.html
            if (string.IsNullOrEmpty(requestPath))
            {
                requestPath = "index.html";
            }

            string filePath = Path.Combine(_webRoot, requestPath);

            Console.WriteLine($"📥 {request.HttpMethod} {requestPath}");

            try
            {
                if (File.Exists(filePath))
                {
                    // Déterminer le Content-Type
                    string contentType = GetContentType(filePath);
                    response.ContentType = contentType;

                    // ✅ HEADERS CORS CRITIQUES
                    response.Headers.Add("Access-Control-Allow-Origin", "*");
                    response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                    response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                    // Lire et envoyer le fichier
                    byte[] buffer = File.ReadAllBytes(filePath);
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"   ✅ 200 OK - {buffer.Length} bytes");
                    Console.ResetColor();
                }
                else
                {
                    // 404 Not Found
                    response.StatusCode = 404;
                    byte[] buffer = Encoding.UTF8.GetBytes($"404 - Fichier non trouvé: {requestPath}");
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"   ⚠️ 404 Not Found");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ❌ Erreur: {ex.Message}");
                Console.ResetColor();

                response.StatusCode = 500;
                byte[] buffer = Encoding.UTF8.GetBytes("500 - Erreur serveur");
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
            }
            finally
            {
                response.OutputStream.Close();
            }
        }

        static string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".html":
                case ".htm":
                    return "text/html; charset=utf-8";
                case ".css":
                    return "text/css";
                case ".js":
                    return "application/javascript";
                case ".json":
                    return "application/json";
                case ".png":
                    return "image/png";
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".gif":
                    return "image/gif";
                case ".svg":
                    return "image/svg+xml";
                case ".ico":
                    return "image/x-icon";
                default:
                    return "application/octet-stream";
            }
        }
    }
}