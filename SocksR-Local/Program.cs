using Cipher;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SocksR_Local
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var listener = new TcpListener(IPAddress.Any, 8001);
            listener.Start();
            Console.WriteLine($"SocksR-Local，运行在：http://127.0.0.1:8001");

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
            byte[] requestBuffer = requestBufferList.ToArray();
            var requestString = Encoding.UTF8.GetString(requestBuffer);

            // 和 SocksR-Remote 进行通信
            var socksrRemoteHost = "127.0.0.1";
            var socksrRemotePort = 8002;
            using TcpClient targetClient = new TcpClient(socksrRemoteHost, socksrRemotePort);
            using NetworkStream targetStream = targetClient.GetStream();
            // 加密处理
            byte[] cipherBuffer = AESTool.Encrypt(requestString, Encoding.UTF8.GetBytes(AESTool.DefaultKey), Encoding.UTF8.GetBytes(AESTool.DefaultIV));
            targetStream.Write(cipherBuffer, 0, cipherBuffer.Length);

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
            var responseCipherBuffer = responseBufferList.ToArray();
            // 解密处理
            string decipherString = AESTool.Decrypt(responseCipherBuffer, Encoding.Default.GetBytes(AESTool.DefaultKey), Encoding.Default.GetBytes(AESTool.DefaultIV));
            var responseBuffer = Encoding.Default.GetBytes(decipherString);
            clientStream.Write(responseBuffer, 0, responseBuffer.Length);
        }
    }
}