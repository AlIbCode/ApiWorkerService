using ApiService;
using Serilog;

var programDir= Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(programDir, "log.txt"))
            .CreateLogger();
try
{
    IHost host = Host.CreateDefaultBuilder(args)
    .UseSerilog()
    .UseWindowsService()
    .ConfigureServices((hostContext, services) =>
    {
        hostContext.Configuration.GetSection("HostOptions").Bind(hostContext.Configuration);
        services.Configure<HostOptions>(hostContext.Configuration);

        services.AddHostedService<Worker>();

    })
    
    .Build();

    await host.RunAsync();

} catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly", ex.Message); 
}
finally
{
    Log.CloseAndFlush();
}


