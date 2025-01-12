using Microsoft.AspNetCore.Mvc;

namespace ProjectMmApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private static readonly List<StreamWriter> _clients = [];

        [HttpGet]
        public async Task Subscribe()
        {
            Response.Headers.Append("Content-Type", "text/event-stream");
            Response.Headers.Append("Cache-Control", "no-cache");
            Response.Headers.Append("Connection", "keep-alive");

            var streamWriter = new StreamWriter(Response.Body);
            _clients.Add(streamWriter);

            try
            {
                while (!HttpContext.RequestAborted.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), HttpContext.RequestAborted);
                }
            }
            finally
            {
                _clients.Remove(streamWriter);
            }
        }

        public static async Task NotifyClients(string data)
        {
            List<StreamWriter> deadClients = [];

            foreach (var client in _clients)
            {
                try
                {
                    await client.WriteAsync($"data: {data}\n\n");
                    await client.FlushAsync();
                }
                catch
                {
                    deadClients.Add(client);
                }
            }

            foreach (var deadClient in deadClients)
            {
                _clients.Remove(deadClient);
            }
        }
    }
}