using VideoPlaybackService;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PlaybackController>();

var app = builder.Build();

app.MapPost("/play", (PlayRequest request, PlaybackController controller) =>
{
    if (string.IsNullOrWhiteSpace(request.VideoFile))
    {
        return Results.BadRequest("VideoFile path is required.");
    }

    var state = controller.Play(request.VideoFile);

    if (state.Status == PlaybackStatus.Error)
        return Results.BadRequest(state);
    else
        return Results.Ok(state);
});

app.MapPost("/stop", (PlaybackController controller) =>
{
    var state = controller.Stop();
    return Results.Ok(state);
});

app.MapGet("/status", (PlaybackController controller) =>
{
    return Results.Ok(controller.GetState());
});

app.Run();

record PlayRequest(string VideoFile);