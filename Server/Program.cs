using Server;

var logger = new ConsoleLogger();



await new HttpServer(logger, ServerSettings.GetServerSettingsFromFile(logger))
    .Start();



