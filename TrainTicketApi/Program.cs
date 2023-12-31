/******************************************************************************
 * Program.cs
 * 
 * Description: This file is the entry point for the .NET backend application.
 * It configures and runs the application, allowing it to handle incoming requests,
 * manage data, and provide services to client applications.
 * 
 * 
 *****************************************************************************/

using TrainTicketApi.Models;
using TrainTicketApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<TrainTicketDatabaseSettings>(
    builder.Configuration.GetSection("TrainTicketDatabase"));

// Register the Service classes in the system
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<TravellerService>();
builder.Services.AddSingleton<TrainService>();
builder.Services.AddSingleton<ScheduleService>();
builder.Services.AddSingleton<ReservationService>();

builder.Services.AddControllers()
               .AddJsonOptions(options =>
               {
                   options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
               });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(setup =>
{
    setup.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder
            .SetIsOriginAllowed(_ => true) // Allow any origin
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
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
