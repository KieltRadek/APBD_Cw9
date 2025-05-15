using Cwiczenia_9.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                          ?? "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=WarehouseDB;Integrated Security=True;";

builder.Services.AddScoped<IWarehouseService>(sp => new WarehouseService(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();