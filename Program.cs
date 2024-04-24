/// <summary>
/// This application hosts a REST endpoint that transforms code using the Gemini Model.
/// </summary>
using System.Text.Json.Serialization;
using CodeTransform;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.Configure<TransformerOptions>(
    builder.Configuration.GetSection(TransformerOptions.Transformer));

builder.Services.AddPredictionServiceClient(client => {
    var options = TransformerOptions.FromConfiguration(builder.Configuration);
    client.Endpoint = $"{options.LocationId}-aiplatform.googleapis.com";
});

builder.Services.AddSingleton<Transformer>();

var config = builder.Configuration;
var app = builder.Build();

app.UseStaticFiles();

var log = app.Logger;

app.MapPost("/transform", async (Transformer transformer, TransformRequest request) => 
{
    try
    {
        var result = await transformer.GenerateAsync(request);
        
        if (result == null)
        {
            return Results.NotFound("No response from Vertex AI");
        }
        else
        {
            return Results.Ok(result);
        }
    }
    catch (Exception error)
    {
        log.LogError("An error occurred while executing the prompt: {0},\n{1}", 
            error.Message, error.StackTrace);
        return Results.Problem(detail: error.StackTrace, title: error.Message, statusCode: 500);
    }
});

app.Run();
