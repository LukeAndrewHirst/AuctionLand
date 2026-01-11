using AuctionService.Data;
using AuctionService.IntegrationTests.Util;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using WebMotions.Fake.Authentication.JwtBearer;

namespace AuctionService.IntegrationTests.Fixtures
{
    public class CustomWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private PostgreSqlContainer postgresSqlContainer = new PostgreSqlBuilder("postgres:16-alpine").Build();

        public async Task InitializeAsync()
        {
            await postgresSqlContainer.StartAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
               services.RemoveDbConext<DataContext>();

               services.AddDbContext<DataContext>(opt =>
               {
                  opt.UseNpgsql(postgresSqlContainer.GetConnectionString()); 
               });

               services.AddMassTransitTestHarness();

               services.EnsureCreated<DataContext>();

               services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer(opt =>
               {
                   opt.BearerValueType = FakeJwtBearerBearerValueType.Jwt;
               });
            });
        }

        Task IAsyncLifetime.DisposeAsync() => postgresSqlContainer.DisposeAsync().AsTask();
    }
}