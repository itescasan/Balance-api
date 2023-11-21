using Balance_api.Contexts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.





var MyAllowSpecificOrigins = "CorsPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                         // policy.AllowAnyOrigin()
                         policy.WithOrigins("http://localhost:4200",
                                             "http://localhost:150",
                                             "http://165.98.96.131:150")
                         .AllowAnyHeader()             
                          .AllowAnyMethod(); 
                      });
});


builder.Services.AddControllers().AddNewtonsoftJson(); 

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BalanceEntities>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaCnxSqlServer")));


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

}



app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseSwagger();

app.UseSwaggerUI( c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Balance API");
});


// Redirect requests to the root URL to the Swagger UI
app.Use(async (context, next) => { 

    if(context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger/index.html");
        return;
    }

    await next();
} );





app.MapControllers();

app.Run();
