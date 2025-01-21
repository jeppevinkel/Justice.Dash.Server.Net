using Justice.Dash.Server.Services;
using Microsoft.EntityFrameworkCore;

namespace Justice.Dash.Server;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("SqlConnectionString");
        
        builder.Services.AddDbContext<DashboardDbContext>(options => options
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            )
        );
        
        builder.Services.AddHostedService<FoodAndCoService>();
        builder.Services.AddHostedService<AiService>();
        builder.Services.AddHostedService<DomicileService>();

        builder.Services.AddSingleton<StateService>();

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        using (IServiceScope scope = app.Services.CreateScope())
        {
            IServiceProvider services = scope.ServiceProvider;

            var context = services.GetRequiredService<DashboardDbContext>();
            context.Database.Migrate();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}