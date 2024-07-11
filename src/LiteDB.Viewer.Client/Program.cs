using BlazorComponentBus;
using BlazorFileSaver;
using CurrieTechnologies.Razor.Clipboard;
using LiteDB.Viewer.Client;
using LiteDB.Viewer.Client.Services;
using LiteDB.Viewer.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices(c =>
{
    c.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
    c.SnackbarConfiguration.ShowTransitionDuration = 500;
    c.SnackbarConfiguration.VisibleStateDuration = 2500;
    c.SnackbarConfiguration.HideTransitionDuration = 1000;
});
builder.Services.AddClipboard();

builder.Services.AddScoped<ComponentBus>();
builder.Services.AddBlazorFileSaver();

builder.Services.AddSingleton<IAppStateService, AppStateService>();

await builder.Build().RunAsync();