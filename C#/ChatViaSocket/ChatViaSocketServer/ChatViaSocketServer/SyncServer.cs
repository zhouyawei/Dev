using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketServer
{
    class SyncServer
    {
        public void Serve()
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int localPort = 1569;
            EndPoint localEndPoint = new IPEndPoint(IPAddress.Any, localPort);
            serverSocket.Bind(localEndPoint);
            int backlog = 10000;
            serverSocket.Listen(backlog);

            /*解析第一个包*/
            Socket clientSocket = serverSocket.Accept();
            ProcessReceive(clientSocket);
            
            clientSocket.Close();

            serverSocket.Close();
        }

        private static void ProcessReceive(Socket clientSocket)
        {
            byte[] buffer = new byte[BUFFER_SIZE];

            int dataChunkBodyMaxLength = BUFFER_SIZE - 4;//包的数据长度
            int totalBytesRead = 0;

            int bytesRead = clientSocket.Receive(buffer);
            totalBytesRead += bytesRead;

            byte[] dataTotalLengthInBytes = new byte[DATA_CHUNK_LENGTH_HEADER];
            Array.Copy(buffer, dataTotalLengthInBytes, DATA_CHUNK_LENGTH_HEADER);
            int dataTotalLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataTotalLengthInBytes, 0));

            int sourceIndex = 0;
            int destinationIndex = 4;

            int dataChunkBodyActualLength = bytesRead < BUFFER_SIZE
                ? bytesRead - DATA_CHUNK_LENGTH_HEADER
                : BUFFER_SIZE - DATA_CHUNK_LENGTH_HEADER;

            /*得到第一个包中的内容*/
            byte[] dataChunkFirstBody = new byte[dataChunkBodyActualLength];
            Array.Copy(buffer, destinationIndex, dataChunkFirstBody, sourceIndex, dataChunkBodyActualLength);
            List<byte> dataInBytes = new List<byte>();
            dataInBytes.AddRange(dataChunkFirstBody);

            /*读取分包*/
            int numOfDataChunk = (int)Math.Ceiling((dataTotalLength * 1.0) / (dataChunkBodyMaxLength * 1.0));//分包数
            byte[] dataChunkBody = new byte[dataChunkBodyMaxLength];
            for (int i = 1; i < numOfDataChunk; i++)
            {
                bytesRead = clientSocket.Receive(buffer);
                totalBytesRead += bytesRead;

                Array.Copy(buffer, dataTotalLengthInBytes, DATA_CHUNK_LENGTH_HEADER);
                int dataTotalLength2 = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(dataTotalLengthInBytes, 0));

                if (dataTotalLength != dataTotalLength2)
                    throw new Exception(string.Format("非法数据包，不一致的相邻包大小"));

                dataChunkBodyActualLength = bytesRead < BUFFER_SIZE
                ? bytesRead - DATA_CHUNK_LENGTH_HEADER
                : BUFFER_SIZE - DATA_CHUNK_LENGTH_HEADER;

                if (i < numOfDataChunk - 1)
                {
                    /*得到每个包中的内容*/
                    Array.Copy(buffer, destinationIndex, dataChunkBody, sourceIndex, dataChunkBodyActualLength);
                    dataInBytes.AddRange(dataChunkBody);
                }
                else
                {
                    /*计算最后一个包的长度*/
                    int lastDataChunkLength = dataTotalLength - (BUFFER_SIZE - DATA_CHUNK_LENGTH_HEADER) * (numOfDataChunk - 1);
                    byte[] dataChunkLastBody = new byte[lastDataChunkLength];
                    /*得到最后一个包的内容*/
                    Array.Copy(buffer, destinationIndex, dataChunkLastBody, sourceIndex, lastDataChunkLength);
                    dataInBytes.AddRange(dataChunkLastBody);
                }
            }

            string message = Encoding.UTF8.GetString(dataInBytes.ToArray(), 0, dataInBytes.Count);

            Console.WriteLine(message);
        }

        private const int BUFFER_SIZE = 4096;
        private const int DATA_CHUNK_LENGTH_HEADER = 4;
    }
}
