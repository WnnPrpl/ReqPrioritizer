using ReqPrioritizer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRequestPrioritizer(options =>
{
    options.Priorities["default"] = new PriorityConfig
    {
        Name = "default",
        MaxConcurrentRequests = 5,
        UseQueue = true,
        MaxQueueLength = 10,
        QueueWaitTimeoutMs = 5000,
        OnLimitExceeded = LimitExceededAction.Queue
    };

    options.Priorities["high"] = new PriorityConfig
    {
        Name = "high",
        MaxConcurrentRequests = 20,
        UseQueue = false,
        OnLimitExceeded = LimitExceededAction.Reject
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
