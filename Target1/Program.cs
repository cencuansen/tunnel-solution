using System.Net.Sockets;
using System.Text;

namespace Target1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var listener = new TcpListener(System.Net.IPAddress.Any, 8003);
            listener.Start();
            Console.WriteLine($"目标服务，运行在：http://127.0.0.1:8003");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
            }
        }

        static void ClientThread(object client)
        {
            // 代理服务
            using TcpClient proxyClient = (TcpClient)client;
            using NetworkStream proxyServerStream = proxyClient.GetStream();

            // 读取请求报文
            List<byte> requestBufferList = new List<byte>();
            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead = proxyServerStream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                byte[] buffer2 = new byte[bytesRead];
                Array.Copy(buffer, buffer2, bytesRead);
                requestBufferList.AddRange(buffer2);
                if (bytesRead < buffer.Length) break;

            }
            byte[] requestBuffer = requestBufferList.ToArray();
            var requestString = Encoding.Default.GetString(requestBuffer);
            Console.WriteLine(requestString);
            Console.WriteLine();

            // 进行响应
            byte[] responseBytes = Encoding.Default.GetBytes("HTTP/1.1 200 OK\r\n\r\n来自目标服务的消息");
            proxyServerStream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}