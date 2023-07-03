using Broadcaster.Classes.Ignite;
using Broadcaster.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using SignalR.Common;

Thread.Sleep(10000);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//builder.Services.AddSignalR();
builder.Services.AddCors((options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins(new string[] { "https://localhost:7134", "https://localhost:1751", "https://localhost:44340" })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
}));

//builder.Services.AddSingleton<IgniteClient>(provider =>
//{
//    // Get the IHubContext<OMSHub> from the service provider
//    var hubContext = provider.GetRequiredService<IHubContext<OMSHub>>();

//    // Create the IgniteClient instance with the hub context
//    return new IgniteClient(hubContext);
//});
builder.Services.AddSingleton<IgniteClient>();
builder.Services.AddHostedService<IgniteClientHostService>();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" });
    //opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/html" });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles();

app.UseRouting();
app.MapBlazorHub();
app.MapHub<OMSHub>("/OMSHub");

app.MapFallbackToPage("/_Host");

app.Run();

