Imports Microsoft.AspNetCore
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.AspNetCore.Identity
Imports Microsoft.eShopWeb.Infrastructure.Data
Imports Microsoft.eShopWeb.Infrastructure.Identity
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Logging


Public Class Program
        Public Shared Sub Main(ByVal args As String())
            Dim host = CreateWebHostBuilder(args).Build()

            Using scope = host.Services.CreateScope()
                Dim services = scope.ServiceProvider
                Dim loggerFactory = services.GetRequiredService(Of ILoggerFactory)()

                Try
                    Dim catalogContext = services.GetRequiredService(Of CatalogContext)()
                    CatalogContextSeed.SeedAsync(catalogContext, loggerFactory).Wait()
                    Dim userManager = services.GetRequiredService(Of UserManager(Of ApplicationUser))()
                    AppIdentityDbContextSeed.SeedAsync(userManager).Wait()
                Catch ex As Exception
                    Dim logger = loggerFactory.CreateLogger(Of Program)()
                    logger.LogError(ex, "An error occurred seeding the DB.")
                End Try
            End Using

            host.Run()
        End Sub

        Public Shared Function CreateWebHostBuilder(ByVal args As String()) As IWebHostBuilder
            Return WebHost.CreateDefaultBuilder(args).UseStartup(Of Startup)()
        End Function
    End Class

