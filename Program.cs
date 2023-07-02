using MessageBroker.Data;
using MessageBroker.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlite("Data Source=MessageBroker.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Create topic
app.MapPost("api/topics", async (AppDbContext context, Topic topic) =>
{
    await context.Topics.AddAsync(topic);

    await context.SaveChangesAsync();

    return Results.Created($"api/topics/{topic.Id}", topic);
});

// Return all topics
app.MapGet("api/topics", async (AppDbContext context) =>
{
    var topics = await context.Topics.ToListAsync();

    return Results.Ok(topics);
});

// Publish message
app.MapPost("api/topics/{id}/messages", async (AppDbContext context, int id, Message message) =>
{
    bool topics = await context.Topics.AnyAsync(t => t.Id == id);
    if (!topics) return Results.NotFound("Topic not found.");

    var subs = context.Subscriptions.Where(s => s.TopicId == id);
    if (subs.Count() == 0) return Results.NotFound("There are no subscriptions for this topic");

    foreach (var sub in subs)
    {
        Message msg = new Message
        {
            TopicMessage = message.TopicMessage,
            Subscriptionid = sub.Id,
            ExpiresAfter = message.ExpiresAfter,
            MessgageStatus = message.MessgageStatus
        };

        await context.Messages.AddAsync(msg);
    }

    await context.SaveChangesAsync();

    return Results.Ok("Message has been published.");
});


// Create Subscriptions
app.MapPost("api/topics/{id}/subscriptions", async (AppDbContext context, int id, Subscription sub) =>
{

    bool topics = await context.Topics.AnyAsync(t => t.Id == id);

    if (!topics) return Results.NotFound("Topic not found.");

    sub.TopicId = id;

    await context.Subscriptions.AddAsync(sub);
    await context.SaveChangesAsync();

    return Results.Created($"/api/topics/{id}/subscriptions/{sub.Id}", sub);
});

// Get subscribers
app.MapGet("api/subscriptions/{id}/messages", async (AppDbContext context, int id) =>
{
    bool subs = await context.Subscriptions.AnyAsync(s => s.Id == id);
    if (!subs) return Results.NotFound("Subscription not found");

    var messages = context.Messages.Where(m => m.Subscriptionid == id && m.MessgageStatus != "SENT");
    if (messages.Count() == 0) return Results.NotFound("No new messages.");

    foreach (var msg in messages)
    {
        msg.MessgageStatus = "REQUESTED";
    }

    await context.SaveChangesAsync();

    return Results.Ok(messages);
});


app.MapPost("api/subscriptions/{id}/messages", async (AppDbContext context, int id, int[] confs) =>
{
    bool subs = await context.Subscriptions.AnyAsync(s => s.Id == id);
    if (!subs) return Results.NotFound("Subscription not found.");

    if (confs.Length <= 0) return Results.BadRequest();

    int count = 0;

    foreach (var i in confs)
    {
        var msg = context.Messages.FirstOrDefault(m => m.Id == i);
        if (msg != null)
        {
            msg.MessgageStatus = "SENT";
            await context.SaveChangesAsync();
            count++;
        }
    }
    return Results.Ok($"Acknowledged {count}/{confs.Length} message.");
});

app.Run();