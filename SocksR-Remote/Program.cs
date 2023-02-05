using System.Net.Sockets;
using System.Net;
using System.Text;
using Cipher;

namespace SocksR_Remote
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Any, 8002);
            listener.Start();
            Console.WriteLine($"SocksR-Remote，运行在：http://127.0.0.1:8002");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), client);
            }
        }

        static void ClientThread(object client)
        {
            // 客户端
            using TcpClient tcpClient = (TcpClient)client;
            using NetworkStream clientStream = tcpClient.GetStream();

            // 读取客户端请求报文，获得目标服务信息
            List<byte> requestBufferList = new List<byte>();
            while (true)
            {
                byte[] buffer = new byte[4096];
                int bytesRead = clientStream.Read(buffer, 0, 4096);
                if (bytesRead == 0) break;
                byte[] buffer2 = new byte[bytesRead];
                Array.Copy(buffer, buffer2, bytesRead);
                requestBufferList.AddRange(buffer2);
                if (bytesRead < buffer.Length) break;

            }
            byte[] cipherBuffer = requestBufferList.ToArray();

            // 解密处理
            var decipherString = AESTool.Decrypt(cipherBuffer, Encoding.Default.GetBytes(AESTool.DefaultKey), Encoding.Default.GetBytes(AESTool.DefaultIV));
            var decipherBuffer = Encoding.Default.GetBytes(decipherString);

            // 目标服务
            Tuple<string, int> hostAndPort = GetTargetServer(decipherString);
            using TcpClient targetClient = new TcpClient(hostAndPort.Item1, hostAndPort.Item2);
            using NetworkStream targetStream = targetClient.GetStream();

            // 发送请求到目标服务
            targetStream.Write(decipherBuffer, 0, decipherBuffer.Length);

            // 读取响应数据
            List<byte> responseBufferList = new List<byte>();
            while (true)
            {
                byte[] bufferFromTarget = new byte[4096];
                int bytesRead = targetStream.Read(bufferFromTarget, 0, bufferFromTarget.Length);
                if (bytesRead == 0) break;
                byte[] buffer2 = new byte[bytesRead];
                Array.Copy(bufferFromTarget, buffer2, bytesRead);
                responseBufferList.AddRange(buffer2);
                if (bytesRead < bufferFromTarget.Length) break;
            }
            var responseBuffer = responseBufferList.ToArray();
            var responseString = Encoding.Default.GetString(responseBuffer);
            // 加密处理
            var responseCipherBuffer = AESTool.Encrypt(responseString, Encoding.Default.GetBytes(AESTool.DefaultKey), Encoding.Default.GetBytes(AESTool.DefaultIV));
            clientStream.Write(responseCipherBuffer, 0, responseCipherBuffer.Length);
        }

        static Tuple<string, int> GetTargetServer(string request)
        {
            int start = request.IndexOf("Host: ") + 6;
            int end = request.IndexOf("\r\n", start);
            string host = request[start..end];
            return Tuple.Create(host.Split(":")[0], Convert.ToInt32(host.Split(":")[1]));
        }
    }
}