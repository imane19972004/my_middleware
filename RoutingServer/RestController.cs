using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RoutingServer
{
    /// <summary>
    /// Contrôleur REST simple pour éviter les problèmes CORS avec SOAP
    /// </summary>
    public class RestController
    {
        private readonly RoutingService _routingService;
        private HttpListener _listener;

        public RestController()
        {
            _routingService = new RoutingService();
        }

        public void Start()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://localhost:8004/api/");

            try
            {
                _listener.Start();

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n╔════════════════════════════════════════════╗");
                Console.WriteLine("║      REST API - DÉMARRÉ ✅                ║");
                Console.WriteLine("╚════════════════════════════════════════════╝");
                Console.ResetColor();
                Console.WriteLine("📍 URL REST: http://localhost:8004/api/itinerary");
                Console.WriteLine("   (Alternative sans CORS au SOAP)\n");

                Task.Run(() => HandleRequests());
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erreur REST API: {ex.Message}");
                Console.ResetColor();
            }
        }

        private async Task HandleRequests()
        {
            while (_listener.IsListening)
            {
                try
                {
                    var context = await _listener.GetContextAsync();
                    await ProcessRequest(context);
                }
                catch (Exception ex)
                {
                    if (_listener.IsListening)
                    {
                        Console.WriteLine($"⚠️ Erreur requête REST: {ex.Message}");
                    }
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // ✅ HEADERS CORS - TOUJOURS
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Methods", "POST, GET, OPTIONS");
            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Accept");
            response.Headers.Add("Access-Control-Max-Age", "86400");

            try
            {
                // Gérer OPTIONS (preflight)
                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    Console.WriteLine("✅ OPTIONS (preflight) répondu");
                    return;
                }

                // Gérer POST /api/itinerary
                if (request.HttpMethod == "POST" && request.Url.AbsolutePath.Contains("/itinerary"))
                {
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        var body = await reader.ReadToEndAsync();
                        var itineraryRequest = JsonConvert.DeserializeObject<ItineraryRequest>(body);

                        Console.WriteLine($"\n🚴 REST API: {itineraryRequest.Origin} → {itineraryRequest.Destination}");

                        // Appeler le service existant
                        var result = await _routingService.GetItinerary(itineraryRequest);

                        // Retourner en JSON
                        var jsonResponse = JsonConvert.SerializeObject(result);
                        var buffer = Encoding.UTF8.GetBytes(jsonResponse);

                        response.ContentType = "application/json; charset=utf-8";
                        response.ContentLength64 = buffer.Length;
                        response.StatusCode = 200;
                        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);

                        Console.WriteLine("✅ Réponse REST envoyée");
                    }
                }
                else
                {
                    // 404
                    response.StatusCode = 404;
                    var buffer = Encoding.UTF8.GetBytes("{\"error\":\"Endpoint non trouvé\"}");
                    await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Erreur REST: {ex.Message}");
                Console.ResetColor();

                response.StatusCode = 500;
                var errorJson = JsonConvert.SerializeObject(new { error = ex.Message });
                var buffer = Encoding.UTF8.GetBytes(errorJson);
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
            finally
            {
                response.Close();
            }
        }

        public void Stop()
        {
            _listener?.Stop();
            _listener?.Close();
        }
    }
}