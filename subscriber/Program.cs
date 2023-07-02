
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Subscriber.Dtos;

const string requesturi = "localhost:5202/api/subscriptions/1/messages";

Console.WriteLine("Press ESC to stop.");
do
{
    HttpClient client = new HttpClient();
    Console.WriteLine("Listening....");
    await ShowMessages(client);
} while (Console.ReadKey(true).Key != ConsoleKey.Escape);


static async Task ShowMessages(HttpClient client)
{
    while (!Console.KeyAvailable)
    {
        List<int> ackIds = await GetMessagesAsync(client, requesturi);

        Thread.Sleep(2000); // Slow down the process

        if (ackIds.Count() > 0)
        {
            await AckMessagesAsync(client, ackIds, requesturi);
        }
    }
}
static async Task<List<int>> GetMessagesAsync(HttpClient client, string requesturi)
{
    List<int> ackIds = new List<int>();
    List<MessageReadDtos>? newMessages = new List<MessageReadDtos>();

    try
    {
        newMessages = await client.GetFromJsonAsync<List<MessageReadDtos>>(requesturi);
    }
    catch (Exception)
    {

        return ackIds;
    }

    foreach (MessageReadDtos msg in newMessages!)
    {
        Console.WriteLine($"{msg.Id} - {msg.TopicMessage} - {msg.MessageStatus}");
        ackIds.Add(msg.Id);
    }

    return ackIds;
}
static async Task AckMessagesAsync(HttpClient httpClient, List<int> ackIds, string requesturi)
{
    var response = await httpClient.PostAsJsonAsync(requesturi, ackIds);
    var returnMessage = await response.Content.ReadAsStringAsync();

    Console.WriteLine(returnMessage);
}