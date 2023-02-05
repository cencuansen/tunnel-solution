using System.Net;
using System.Net.Http.Headers;

namespace Client1
{
    internal class Program
    {
        private static HttpClient httpClient = new HttpClient(new HttpClientHandler
        {
            Proxy = new WebProxy
            {
                Address = new Uri("http://127.0.0.1:8001")
            }
        })
        {
            Timeout = TimeSpan.FromSeconds(3),
        };

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("向 http://127.0.0.1:8003 发送请求？");
                Console.ReadLine();
                var content = new StringContent("来自客户端的消息");
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                var response = await httpClient.PostAsync("http://127.0.0.1:8003", content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                Console.WriteLine();
            }
        }
    }
}