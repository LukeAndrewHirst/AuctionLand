using AuctionService.Consumers;
using AuctionService.Data;
using AuctionService.RequestHelpers;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<DataConext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(cfg => {cfg.AddProfile<MappingProfiles>();});
builder.Services.AddMassTransit(mt =>
{
    mt.AddEntityFrameworkOutbox<DataConext>(o => { o.QueryDelay = TimeSpan.FromSeconds(3); o.UsePostgres(); o.UseBusOutbox(); });
    mt.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    mt.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    mt.UsingRabbitMq((context, cfg) =>
    {
       cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
   options.Authority = builder.Configuration["IdentityServiceUrl"];
   options.RequireHttpsMetadata = false;
   options.TokenValidationParameters.ValidateAudience = false;
   options.TokenValidationParameters.NameClaimType = "username";
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

try
{
    DbIntializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

app.Run();