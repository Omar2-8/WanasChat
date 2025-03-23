
using WanasChat.API.Hubs;
using Orleans.Serialization.Configuration;
using WanasChat.Interfaces;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

builder.Host.UseOrleans(siloBuilder =>
{
    siloBuilder
        .UseLocalhostClustering()
        .Configure<TypeManifestOptions>(options =>
        {
            options.AllowedTypes.Add(typeof(IUserGrain).Assembly.GetName().Name);
        });

    string redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString") ?? "localhost:6379";
    int redisDatabaseNumber = builder.Configuration.GetValue<int>("Redis:DatabaseNumber", 0);

    siloBuilder.AddRedisGrainStorage(
      name: "redis",
      configureOptions: (options) =>
      { 
          var configOptions = ConfigurationOptions.Parse(redisConnectionString);
          configOptions.DefaultDatabase = redisDatabaseNumber;

          options.ConfigurationOptions = configOptions;
      });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.MapHub<ChatHub>("/wanas");

app.Run();