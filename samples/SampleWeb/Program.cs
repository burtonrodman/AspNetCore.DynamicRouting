using System.Text;
using burtonrodman.AspNetCore.DynamicRouting;
using SampleWeb;

var builder = WebApplication.CreateBuilder(args);

// ======= Sample of registering single DataSource/Updater =========
// builder.Services.AddDynamicRouting<DynamicRedirectsUpdater>(
//     "redirects", options => options.UpdateIntervalSeconds = "5");
// ======= Sample of registering single DataSource/Updater =========

// ======= Sample of registering multiple DataSources/Updaters =========
builder.Services.AddDynamicRouting<DynamicRedirectsUpdater, DynamicRedirectsUpdateServiceOptions>(
    "redirects", options => options.UpdateIntervalSeconds = "5");
builder.Services.AddDynamicRouting<CmsPageRoutesUpdater, CmsPageRoutesUpdateServiceOptions>(
    "pages", options => options.UpdateIntervalSeconds = "10");
// ======= Sample of registering multiple DataSources/Updaters =========

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> endpointSources) => {
    var sb = new StringBuilder();
    var endpoints = endpointSources.SelectMany(es => es.Endpoints);
    foreach (var endpoint in endpoints) {
        if (endpoint is RouteEndpoint routeEndpoint) {
            sb.AppendLine(routeEndpoint.RoutePattern.RawText);
        }
    }
    return sb.ToString();
});

// ======= Register the EndpointDataSource =========
app.MapDynamicRoutes("redirects");
app.MapDynamicRoutes("pages");

app.Run();
