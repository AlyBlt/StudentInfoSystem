using Bogus;
using Microsoft.EntityFrameworkCore;
using App.Api.Data;
using App.Api.Models;

var builder = WebApplication.CreateBuilder(args);

// Veritabaný baðlantýsý
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer("Server=.;Database=StudentDb;Trusted_Connection=True;TrustServerCertificate=True;"));

builder.Services.AddControllers();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.AllowAnyOrigin()
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});


var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Database oluþturma ve seed

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Students.Any())
    {
        var faker = new Faker<StudentEntity>("tr")
            .RuleFor(s => s.FirstName, f => f.Name.FirstName())
            .RuleFor(s => s.LastName, f => f.Name.LastName())
            .RuleFor(s => s.StudentNumber, f => f.Random.Number(1000, 9999).ToString())
            .RuleFor(s => s.Grade, f => f.Random.Number(1, 12))
            .RuleFor(s => s.Email, (f, s) => f.Internet.Email(s.FirstName, s.LastName)); 
            

        var fakeStudents = faker.Generate(20);
        db.Students.AddRange(fakeStudents);
        db.SaveChanges();
    }
}

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
