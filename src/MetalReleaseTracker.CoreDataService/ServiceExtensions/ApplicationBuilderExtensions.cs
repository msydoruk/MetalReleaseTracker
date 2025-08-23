using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app, IWebHostEnvironment env)
    {
        app.UseSerilogRequestLogging();
        app.UseErrorHandling();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Metal Release Tracker API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCors("AllowSPA");
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}