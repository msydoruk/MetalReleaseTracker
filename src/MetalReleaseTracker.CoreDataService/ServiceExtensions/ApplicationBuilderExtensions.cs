using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;
using Serilog;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class ApplicationBuilderExtensions
{
    public static WebApplication UseApplicationMiddleware(this WebApplication app, IWebHostEnvironment env)
    {
        app.UseSerilogRequestLogging();
        app.UseSecurityHeaders();
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

        var forwardedHeadersOptions = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };
        forwardedHeadersOptions.KnownProxies.Clear();
        forwardedHeadersOptions.KnownNetworks.Clear();
        app.UseForwardedHeaders(forwardedHeadersOptions);

        app.UseHttpsRedirection();
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = context =>
            {
                var path = context.File.Name;
                if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";
                }
            },
        });
        app.UseCors("AllowSPA");
        app.UseRouting();
        app.UseSlugRedirect();
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}