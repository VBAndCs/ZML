Imports Ardalis.ListStartupServices
Imports Microsoft.AspNetCore.Builder
Imports Microsoft.AspNetCore.Diagnostics.HealthChecks
Imports Microsoft.AspNetCore.Hosting
Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Identity
Imports Microsoft.AspNetCore.Identity.UI
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.AspNetCore.Mvc.ApplicationModels
Imports Microsoft.AspNetCore.Mvc.Razor
Imports Microsoft.AspNetCore.Routing
Imports Microsoft.eShopWeb.ApplicationCore.Interfaces
Imports Microsoft.eShopWeb.ApplicationCore.Services
Imports Microsoft.eShopWeb.Infrastructure.Data
Imports Microsoft.eShopWeb.Infrastructure.Identity
Imports Microsoft.eShopWeb.Infrastructure.Logging
Imports Microsoft.eShopWeb.Infrastructure.Services
Imports Microsoft.eShopWeb.Web.HealthChecks
Imports Microsoft.eShopWeb.Web.Interfaces
Imports Microsoft.eShopWeb.Web.Services
Imports Microsoft.Extensions.Configuration
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Extensions.Diagnostics.HealthChecks
Imports Newtonsoft.Json
Imports Swashbuckle.AspNetCore.Swagger
Imports System.Net.Mime
Imports Microsoft.EntityFrameworkCore

Public Class Startup
        Private _services As IServiceCollection

    Public Sub New(ByVal configuration As IConfiguration)
        Me.Configuration = configuration
    End Sub

    Public ReadOnly Property Configuration As IConfiguration

        Public Sub ConfigureDevelopmentServices(ByVal services As IServiceCollection)
            ConfigureInMemoryDatabases(services)
        End Sub

        Private Sub ConfigureInMemoryDatabases(ByVal services As IServiceCollection)
        services.AddDbContext(Of CatalogContext)(Sub(c) c.UseInMemoryDatabase("Catalog"))
        services.AddDbContext(Of AppIdentityDbContext)(Function(options) options.UseInMemoryDatabase("Identity"))
        ConfigureServices(services)
        End Sub

        Public Sub ConfigureProductionServices(ByVal services As IServiceCollection)
        services.AddDbContext(Of CatalogContext)(Sub(c) c.UseSqlServer(Configuration.GetConnectionString("CatalogConnection")))
        services.AddDbContext(Of AppIdentityDbContext)(Sub(options) options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")))
        ConfigureServices(services)
        End Sub

        Public Sub ConfigureServices(ByVal services As IServiceCollection)
            ConfigureCookieSettings(services)
            CreateIdentityIfNotCreated(services)
            services.AddScoped(GetType(IAsyncRepository(Of)), GetType(EfRepository(Of)))
            services.AddScoped(Of ICatalogViewModelService, CachedCatalogViewModelService)()
            services.AddScoped(Of IBasketService, BasketService)()
            services.AddScoped(Of IBasketViewModelService, BasketViewModelService)()
            services.AddScoped(Of IOrderService, OrderService)()
            services.AddScoped(Of IOrderRepository, OrderRepository)()
            services.AddScoped(Of CatalogViewModelService)()
            services.Configure(Of CatalogSettings)(Configuration)
            services.AddSingleton(Of IUriComposer)(New UriComposer(Configuration.[Get](Of CatalogSettings)()))
            services.AddScoped(GetType(IAppLogger(Of)), GetType(LoggerAdapter(Of)))
            services.AddTransient(Of IEmailSender, EmailSender)()
            services.AddMemoryCache()
            services.AddRouting(
                Sub(options)
                    options.ConstraintMap("slugify") = GetType(SlugifyParameterTransformer)
                End Sub)

            services.AddMvc(
                Sub(options)
                    options.Conventions.Add(New RouteTokenTransformerConvention(New SlugifyParameterTransformer()))
                End Sub).AddRazorPagesOptions(
                            Sub(options)
                                options.Conventions.AuthorizePage("/Basket/Checkout")
                                options.AllowAreas = True
                            End Sub).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)

            services.AddHttpContextAccessor()
            services.AddSwaggerGen(Sub(c)
                                       c.SwaggerDoc("v1", New Info With {
                                           .Title = "My API",
                                           .Version = "v1"
                                       })
                                   End Sub)

            services.AddHealthChecks().AddCheck(Of HomePageHealthCheck)("home_page_health_check").AddCheck(Of ApiHealthCheck)("api_health_check")
            services.Configure(Of ServiceConfig)(Sub(config)
                                                     config.Services = New List(Of ServiceDescriptor)(services)
                                                     config.Path = "/allservices"
                                                 End Sub)

        _services = services
        End Sub

        Private Shared Sub CreateIdentityIfNotCreated(ByVal services As IServiceCollection)
            Dim sp = services.BuildServiceProvider()

            Using scope = sp.CreateScope()
                Dim existingUserManager = scope.ServiceProvider.GetService(Of UserManager(Of ApplicationUser))()

                If existingUserManager Is Nothing Then
                    services.AddIdentity(Of ApplicationUser, IdentityRole)().AddDefaultUI(UIFramework.Bootstrap4).AddEntityFrameworkStores(Of AppIdentityDbContext)().AddDefaultTokenProviders()
                End If
            End Using
        End Sub

        Private Shared Sub ConfigureCookieSettings(ByVal services As IServiceCollection)
        services.Configure(Of CookiePolicyOptions)(Sub(options)
                                                       options.CheckConsentNeeded = Function(context) True
                                                       options.MinimumSameSitePolicy = SameSiteMode.None
                                                   End Sub)
        services.ConfigureApplicationCookie(Sub(options)
                                                options.Cookie.HttpOnly = True
                                                options.ExpireTimeSpan = TimeSpan.FromHours(1)
                                                options.LoginPath = "/Account/Login"
                                                options.LogoutPath = "/Account/Signout"
                                                options.Cookie = New CookieBuilder With {
                                                        .IsEssential = True
                                                    }
                                            End Sub)
    End Sub

    Public Sub Configure(ByVal app As IApplicationBuilder, ByVal env As IHostingEnvironment)
        app.UseHealthChecks("/health", New HealthCheckOptions With {
            .ResponseWriter = Async Function(context, report)
                                  Dim result = JsonConvert.SerializeObject(New With {
                                                                           Key .status = report.Status.ToString(),
                                                                           Key .errors = report.Entries.[Select](Function(e) New With {
                                                                                                                Key .key = e.Key,
                                                                                                                Key .value = [Enum].GetName(GetType(HealthStatus), e.Value.Status)
                                      })
                                  })
                                  context.Response.ContentType = MediaTypeNames.Application.Json
                                  Await context.Response.WriteAsync(result)
                              End Function
        })

        If env.IsDevelopment() Then
            app.UseDeveloperExceptionPage()
            app.UseShowAllServicesMiddleware()
            app.UseDatabaseErrorPage()
        Else
            app.UseExceptionHandler("/Error")
            app.UseHsts()
        End If

        app.UseHttpsRedirection()
        app.UseStaticFiles()
        app.UseCookiePolicy()
        app.UseAuthentication()
        app.UseSwagger()
        app.UseSwaggerUI(Sub(c)
                             c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1")
                         End Sub)
        app.UseMvc(Sub(routes)
                       routes.MapRoute(name:="identity", template:="Identity/{controller=Account}/{action=Register}/{id?}")
                       routes.MapRoute(name:="default", template:="{controller:slugify=Home}/{action:slugify=Index}/{id?}")
                   End Sub)


        ZML.ZmlPages.Compile()
    End Sub

End Class
