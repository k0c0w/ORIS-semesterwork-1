using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Server
{
    public class ServerSettings
    {
        public int Port { get; init; } = 8080;

        public string Path { get; init; } = @"./static/";

        public string DatabaseConnectionString { get; init; } = "";

        public static ServerSettings? GetServerSettingsFromFile(ILogger logger) 
        {
            ServerSettings? i = null;
            try
            {
                i = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes("./settings.json"));
            }
            catch(JsonException ex)
            {
                logger.ReportError(ex.Message);
            }
            
            return i ?? new ServerSettings();
        }
    }
}
