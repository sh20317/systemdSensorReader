using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace systemdsensordaemon
{
  public class Worker : BackgroundService
  {
    enum ConfigStatusEnum {
      ConfigNotFound,
      ConfigParseError,
      ConfigOk
    }

    private readonly ILogger<Worker> _logger;
    private DaemonConfig config;
    
    public Worker(ILogger<Worker> logger)
    {
      _logger = logger;
      
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
      FileInfo fileInfo = new FileInfo("");
      _logger.LogInformation("samid worker starting...");
      switch (ReadConfig(fileInfo)) {
        case ConfigStatusEnum.ConfigOk:
          _logger.LogInformation("samid configuration file OK.");
          break;
        case ConfigStatusEnum.ConfigNotFound:
          _logger.LogInformation("samid configuration file NOT FOUND.");
          break;
        case ConfigStatusEnum.ConfigParseError:
          _logger.LogInformation("samid configuration file PARSE ERROR.");
          break;
      }
      return base.StartAsync(cancellationToken);      
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogInformation("samid running every: " + config.pollingFrequency + " seconds.");
      _logger.LogInformation("samid outputs to: " + config.outputServer);
      _logger.LogInformation("samid logging to journalctl: " + config.isLogging + ".");
      
      while (!stoppingToken.IsCancellationRequested) {
        if (config.isLogging) {
          config.sources.ForEach(
            (source) => {
              source.
            });
        }

        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
          await Task.Delay(config.pollingFrequency * 1000, stoppingToken);
        
      }
      _logger.LogInformation("samid POLLING STOPPED.");

    }

    private ConfigStatusEnum ReadConfig(FileInfo config) {
      JSchemaGenerator jSchemaGenerator = new JSchemaGenerator();
      JSchema configShema = jSchemaGenerator.Generate(typeof(DaemonConfig));
      if (config.Exists) {
        try {
          JsonTextReader jsonTextReader = new JsonTextReader(new StringReader(File.ReadAllText(config.FullName)));
          JSchemaValidatingReader jSchemaValidatingReader = new JSchemaValidatingReader(jsonTextReader);
          JsonSerializer serializer = new JsonSerializer();
          this.config = serializer.Deserialize<DaemonConfig>(jSchemaValidatingReader);
          return ConfigStatusEnum.ConfigOk;
        } catch (Exception) {
          return ConfigStatusEnum.ConfigParseError;
        }
      } else {
        return ConfigStatusEnum.ConfigNotFound;
      }
    }
  }
}
