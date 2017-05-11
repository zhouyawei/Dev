using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            int remotePort = int.Parse(_remoteServerIP);
            var remoteIP = IPAddress.Parse(_remoteServerIP);
            EndPoint remotEndPoint = new IPEndPoint(remoteIP, remotePort);
            clientSocket.Connect(remotEndPoint);

            ReceiveAsync(clientSocket);
            for (int i = 0; i < 200000000; i++)
            {
                string content = GetSendData2();
                byte[] messagesInBytes = Encoding.UTF8.GetBytes(content);
                SendDataChunk(clientSocket, messagesInBytes);
                //ReceiveAsync(clientSocket);
            }

            clientSocket.Close();

            Console.WriteLine("测试完成");
            Console.Read();
        }

        /// <summary>
        /// 使用Socket接口发送以字节数据表示的数据
        /// </summary>
        /// <param name="clientSocket"></param>
        /// <param name="dataInBytes"></param>
        private static void SendData(Socket clientSocket, byte[] dataInBytes)
        {
            int dataChunkBodyLengh = BUFFER_SIZE - 4;//包的数据长度
            byte[] dataChunk = new byte[BUFFER_SIZE];
            int sourceIndex = 0;
            int destinationIndex = 4;

            /*先发4个字节的表示数据的长度，后面的是数据内容*/
            byte[] dataTotalLengthInBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataInBytes.Length));//数据总长度

            if (dataInBytes.Length <= dataChunkBodyLengh)
            {
                var dataBuffer = dataInBytes.ToArray();
                /*数据复制*/
                Array.Copy(dataInBytes, sourceIndex, dataChunk, destinationIndex, dataInBytes.Length);
                /*数据总长度写入每个包*/
                Array.Copy(dataTotalLengthInBytes, dataChunk, dataTotalLengthInBytes.Length);
                var dataChunkLength = dataInBytes.Length + dataTotalLengthInBytes.Length;
                int bytesSend = clientSocket.Send(dataChunk, dataChunkLength, SocketFlags.None);
            }
            else
            {
                /*发送的数据量大于一个buffer的最大长度，需要将数据分包*/
                int numOfDataChunk = (int)Math.Ceiling((dataInBytes.Length * 1.0) / (dataChunkBodyLengh * 1.0));//分包数

                /*填充每个数据包并发送*/
                for (int i = 0; i < numOfDataChunk; i++)
                {
                    if (i < numOfDataChunk - 1)
                    {
                        /*数据复制*/
                        Array.Copy(dataInBytes, sourceIndex, dataChunk, destinationIndex, dataChunkBodyLengh);
                        /*数据总长度写入每个包*/
                        Array.Copy(dataTotalLengthInBytes, dataChunk, dataTotalLengthInBytes.Length);
                        int bytesSend = clientSocket.Send(dataChunk, BUFFER_SIZE, SocketFlags.None);
                    }
                    else
                    {
                        var lastDataChunkSize = dataInBytes.Length - (numOfDataChunk - 1) * dataChunkBodyLengh;
                        byte[] dataChunk_Last = new byte[lastDataChunkSize + dataTotalLengthInBytes.Length];
                        /*数据复制*/
                        Array.Copy(dataInBytes, sourceIndex, dataChunk_Last, destinationIndex, lastDataChunkSize);
                        /*数据总长度写入每个包*/
                        Array.Copy(dataTotalLengthInBytes, dataChunk_Last, dataTotalLengthInBytes.Length);
                        int bytesSend = clientSocket.Send(dataChunk_Last, dataChunk_Last.Length, SocketFlags.None);
                    }

                    /*更新包计数器*/
                    sourceIndex += dataChunkBodyLengh;
                }
            }
        }

        private static void SendDataChunk(Socket clientSocket, byte[] dataInBytes)
        {
            int dataChunkBodyLengh = BUFFER_SIZE - 4;//包的数据长度
            byte[] dataChunk = new byte[BUFFER_SIZE];
            int sourceIndex = 0;
            int destinationFirstDataChunkIndex = 4;

            /*先发4个字节的表示数据的长度，后面的是数据内容*/
            byte[] dataTotalLengthInBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(dataInBytes.Length));//数据总长度

            if (dataInBytes.Length <= dataChunkBodyLengh)
            {
                var dataBuffer = dataInBytes.ToArray();
                /*数据复制*/
                Array.Copy(dataInBytes, sourceIndex, dataChunk, destinationFirstDataChunkIndex, dataInBytes.Length);
                /*数据总长度写入每个包*/
                Array.Copy(dataTotalLengthInBytes, dataChunk, dataTotalLengthInBytes.Length);
                var dataChunkLength = dataInBytes.Length + dataTotalLengthInBytes.Length;
                int bytesSend = clientSocket.Send(dataChunk, dataChunkLength, SocketFlags.None);
            }
            else
            {
                /*发送的数据量大于一个buffer的最大长度，需要将数据分包*/
                int numOfDataChunk = (int)Math.Ceiling(((dataInBytes.Length + 4) * 1.0) / (BUFFER_SIZE * 1.0));//分包数

                /*填充每个数据包并发送*/
                for (int i = 0; i < numOfDataChunk; i++)
                {
                    if (i == 0)
                    {
                        /*数据总长度写入第一个包*/
                        Array.Copy(dataTotalLengthInBytes, dataChunk, dataTotalLengthInBytes.Length);
                        /*Body写入第一个包*/
                        Array.Copy(dataInBytes, sourceIndex, dataChunk, destinationFirstDataChunkIndex, dataChunkBodyLengh);
                        /*发送*/
                        int bytesSend = clientSocket.Send(dataChunk, BUFFER_SIZE, SocketFlags.None);

                        /*更新包计数器*/
                        sourceIndex += dataChunkBodyLengh;
                    }
                    else if (i < numOfDataChunk - 1 && i > 0)
                    {
                        /*数据复制*/
                        Array.Copy(dataInBytes, sourceIndex, dataChunk, 0, BUFFER_SIZE);
                        int bytesSend = clientSocket.Send(dataChunk, BUFFER_SIZE, SocketFlags.None);

                        /*更新包计数器*/
                        sourceIndex += BUFFER_SIZE;
                    }
                    else
                    {
                        var lastDataChunkSize = dataInBytes.Length + 4 - (numOfDataChunk - 1) * BUFFER_SIZE;
                        byte[] dataChunk_Last = new byte[lastDataChunkSize];
                        /*数据复制*/
                        Array.Copy(dataInBytes, sourceIndex, dataChunk_Last, 0, lastDataChunkSize);
                        int bytesSend = clientSocket.Send(dataChunk_Last, dataChunk_Last.Length, SocketFlags.None);

                        /*更新包计数器*/
                        sourceIndex += lastDataChunkSize;
                    }
                }
            }
        }

        private static void Receive(Socket clientSocket)
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
            int numOfDataChunk = (int)Math.Ceiling(((dataTotalLength + DATA_CHUNK_LENGTH_HEADER) * 1.0) / (BUFFER_SIZE * 1.0));//分包数
            byte[] dataChunkBody = new byte[dataChunkBodyMaxLength];
            dataChunkBodyActualLength = BUFFER_SIZE;/*第二个包开始全是包的body*/

            for (int i = 1; i < numOfDataChunk; i++)
            {
                bytesRead = clientSocket.Receive(buffer);
                totalBytesRead += bytesRead;

                if (i < numOfDataChunk - 1)
                {
                    /*得到每个包中的内容*/
                    Array.Copy(buffer, 0, dataChunkBody, sourceIndex, dataChunkBodyActualLength);
                    dataInBytes.AddRange(dataChunkBody);
                }
                else
                {
                    /*计算最后一个包的长度*/
                    int lastDataChunkLength = dataTotalLength + DATA_CHUNK_LENGTH_HEADER - BUFFER_SIZE * (numOfDataChunk - 1);
                    byte[] dataChunkLastBody = new byte[lastDataChunkLength];
                    /*得到最后一个包的内容*/
                    Array.Copy(buffer, 0, dataChunkLastBody, sourceIndex, lastDataChunkLength);
                    dataInBytes.AddRange(dataChunkLastBody);
                }
            }

            string message = Encoding.UTF8.GetString(dataInBytes.ToArray(), 0, dataInBytes.Count);

            Console.WriteLine(message);
        }

        private static void ReceiveAsync(Socket clientSocket)
        {
            var userToken = GetAsyncUserToken(clientSocket);
            SocketAsyncEventArgs readEventArgs = userToken.ReceiveSocketAsyncEventArgs;
            if (!clientSocket.ReceiveAsync(readEventArgs))
            {
                ProcessReceive(readEventArgs);
            }
        }

        private static AsyncUserToken GetAsyncUserToken(Socket clientSocket)
        {
            AsyncUserToken userToken = null;
            if (_asyncUserTokenPool.Count == 0)
            {
                userToken = new AsyncUserToken() { Socket = clientSocket };

                userToken.ReceiveSocketAsyncEventArgs.SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
                userToken.ReceiveSocketAsyncEventArgs.AcceptSocket = clientSocket;
                userToken.ReceiveSocketAsyncEventArgs.Completed += IO_Completed;
                _asyncUserTokenPool.Add(userToken);
            }
            else
            {
                userToken = _asyncUserTokenPool[0];
            }

            return userToken;
        }

        static void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation == SocketAsyncOperation.Receive)
            {
                ProcessReceive(e);
            }
        }

        private static void ProcessReceive(SocketAsyncEventArgs readEventArgs)
        {
            /*需要检查客户端是否关闭了连接*/
            AsyncUserToken asyncUserToken = readEventArgs.UserToken as AsyncUserToken;
            
            try
            {
                if (readEventArgs.BytesTransferred > 0 && readEventArgs.SocketError == SocketError.Success)
                {
                    Interlocked.Add(ref _totalBytesReceived, (long)readEventArgs.BytesTransferred);
                    _log.Info(string.Format("目前已接受{0}字节的数据", _totalBytesReceived));

                    /*读取缓冲区中的数据*/
                    /*半包, 粘包*/
                    try
                    {
                        /*读取数据*/
                        byte[] dataTransfered = new byte[readEventArgs.BytesTransferred];
                        Array.Copy(readEventArgs.Buffer, readEventArgs.Offset, dataTransfered, 0, readEventArgs.BytesTransferred);
                        asyncUserToken.Buffer.AddRange(dataTransfered);

                        /* 4字节包头(长度) + 包体*/
                        /* Header + Body */

                        /* 接收到的数据可能小于一个包的大小，需分多次接收
                         * 先判断包头的大小，够一个完整的包再处理
                         */

                        while (asyncUserToken.Buffer.Count > DATA_CHUNK_LENGTH_HEADER)
                        {
                            /*判断包的长度*/
                            byte[] lenBytes = asyncUserToken.Buffer.GetRange(0, DATA_CHUNK_LENGTH_HEADER).ToArray();
                            int bodyLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lenBytes, 0));//包体的长度

                            var packageLength = DATA_CHUNK_LENGTH_HEADER + bodyLen; //一个数据包的长度，4字节包头 + 包体的长度
                            var receivedLengthExcludeHeader = asyncUserToken.Buffer.Count - DATA_CHUNK_LENGTH_HEADER; //去掉包头之后接收的长度

                            /*接收的数据长度不够时，退出循环，让程序继续接收*/
                            if (receivedLengthExcludeHeader < bodyLen)
                            {
                                break;
                            }

                            /*接收的数据长度大于一个包的长度时，则提取出来，交给后面的程序去处理*/
                            byte[] receivedBytes = asyncUserToken.Buffer.GetRange(DATA_CHUNK_LENGTH_HEADER, bodyLen).ToArray();
                            asyncUserToken.Buffer.RemoveRange(0, packageLength); /*从缓冲区重移出取出的数据*/

                            /*抽象数据处理方法，receivedBytes是一个完整的包*/
                            ProcessData(asyncUserToken, receivedBytes);
                        }

                        /*继续接收, 非常关键的一步*/
                        if (asyncUserToken.Socket != null && !asyncUserToken.Socket.ReceiveAsync(readEventArgs))
                        {
                            ProcessReceive(readEventArgs);
                        }
                    }
                    catch (Exception ex)
                    {
                        _log.ErrorFormat("Program->ProcessReceive出现异常, Exception = {0}", ex);
                    }
                }
                else
                {
                    CloseClientSocket(asyncUserToken);
                }
            }
            finally
            {
                
            }
        }

        /*抽象数据处理方法，receivedBytes是一个完整的包*/
        protected static void ProcessData(AsyncUserToken asyncUserToken, byte[] receivedBytes)
        {
            string message = Encoding.UTF8.GetString(receivedBytes, 0, receivedBytes.Length);

            Console.WriteLine(message);

            _log.Info(string.Format("ProcessData: message = {0}", message));
        }

        private static void CloseClientSocket(AsyncUserToken asyncUserToken)
        {
            string clientIP = string.Empty;
            try
            {
                if (asyncUserToken.Socket != null)
                {
                    clientIP = asyncUserToken.Socket.RemoteEndPoint.ToString();
                    asyncUserToken.Socket.Shutdown(SocketShutdown.Both);
                    asyncUserToken.Socket.Close();
                    asyncUserToken.Socket = null;
                    asyncUserToken.ReceiveSocketAsyncEventArgs.AcceptSocket = null;
                    asyncUserToken.SendSocketAsyncEventArgs.AcceptSocket = null;
                }
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("AsyncServerBase->CloseClientSocket出现异常{0}, clientIP = {1}", ex, clientIP);
            }

            asyncUserToken.Reset();
        }

        private static string GetSendData()
        {
            var t = @"编者按：本文来自微信公众号“游戏葡萄”（ID：youxiputao）， 作者托马斯之颅

　　老李最近过得不太好。

　　老李今年 35 岁，他曾担任发行公司的高管，主推过几则月流水千万的项目，后来离职，创立了一家小型发行公司，结果因为“没想清楚”失败。如今团队解散，他准备卖掉妻子的包，找一家公司打工，把天使投资人和亲戚的钱慢慢还回去。

　　老李的白发越来越多，他开始回忆自己 25 岁的时候。那时他吃穿住行都在公司，全心全意为事业奋斗；如今他身陷日常生活的泥沼，甚至葡萄君想约老李上午聊聊，老李都说不行：“要陪孩子去打针。”

　　孩子生病了，你做父亲的总得请个假，陪他去医院吧？你跑不掉的。自由的日子没有了。你已经没有多少机会了。如果你在打德州，你的筹码已经不够几手了。

　　狭路相逢。老李遇到了中年危机。

　　职场

　　中年危机曾经是 40 岁的专利。但在普遍年轻的游戏行业，无论是猎头、HR 还是从业者似乎都达成了共识，默认 35 岁是一个意味着“老了”的门槛。

　　游戏产业的爆发与增长没有几年。在黄金岁月中，没有人会感受到中年危机的临近；但如今行业逐渐稳定，大浪淘沙，对一些 35 岁左右，工作面临调整和变动的从业者来说，失业和降薪已经成了绕不过去的一道槛。葡萄君也打听到了一些例子：

83 年，策划出身，做过端游、手游、发行、创业。创业失败后转运营，薪资拿到 30K，但最后被炒，想回归研发却不能，因为主策的岗位往往要求主导过千万流水的项目；
79 年，大专，10 年以上从业，擅长 MOBA 和 RTS 运营，最近失业，学历不够，年龄太大，找不到工作；
70 后，05 年左右创业做端游研运一体，做到 2015 年，转型手游遭遇困难，后来手游创业也不顺利，最后离开游戏圈；
82 年，曾于知名厂商做助理制作人，后来创业失败，应聘制作人、主策难以成功，一度担任普通策划，月薪不到 20K。
　　剔除学历、履历等方面的因素，年龄真的会影响入职成功率吗？曾经在数家游戏公司之间辗转的 HR 小京介绍，在筛选程序简历的时候，年龄确实会是 HR 看重的因素。

如果一名技术有5-7 年的经验，但还只是一个普通程序员，那就说明能力一般，简历会直接 Pass 掉，录用的可能性也很低。除非说你一直在游戏行业里，做过的项目很好，经验也确实很丰富。

　　79 年的吗啡曾先后在 EA、Zynga、Kabam 负责技术。在 Kabam 北京工作室解散后，他在经历短暂的焦虑后重新找到了不错的工作，但他周围同事的求职并不都这么顺利：

那几名 30 岁左右的同事最快找到工作，有几个人还升职到了主程；但几位没有管理经验，年龄也比较大的同事就艰难一些。有三名同事去了一家新晋的休闲游戏厂商面试，聊了近 3 个小时，向面试官给出了许多技术解决方案，结果最后都被拒绝了。

　　他们被拒绝之后，我发现这家公司的职位要求从“精通”改成了“愿意学习”，还加上了“1-3 年经验为佳”。的确，技术岗工作时间长，经常要加班，脏活累活都得干，年轻力壮的小鲜肉确实更加适合。他们扛得住长时间的工作，而且更便宜。

　　虽然大龄程序要兼顾家庭，不能满足游戏公司的加班要求，但他们远比策划幸运。

　　小京称，游戏策划需要玩足够的游戏，35 岁以上的人很少爱玩游戏，老板永远想要年轻的策划。她甚至很少接触 85 年之前的主策，大龄执行策划更不会被列入考虑的名单。资深游戏 HR 小W也表示，35 岁以上的策划，如果没有专注在一个类目进行深耕，或者没有成功项目在身，恐怕会越来越寸步难行。

　　女性同样是这场危机当中的弱势群体。小京称，游戏公司的高管大多是男性，在国内做研发，超过 35 岁的女性职业发展会越来越不明朗。如果公司经营状况不好，人员编制过剩，年龄过大的女性也会面临裁员的风险。

　　一名资深猎头总结，如果求职者在 35 岁以上，只有到达主程、经理、总监、制作人等级别才有被猎头光顾的可能，普通岗位几乎没有客户需要。

　　“普通岗怎么定义？”

　　“目前月薪还在 20K 以下的人。Title 是虚的，到手多少钱才是实际，你说对吧？”

　　安全

　　对很多人来说，月薪 20K 并不是一个特别容易达成的数字。但在上海的小京看来，足以抵消中年危机的数字还应该再高一些。

　　小京认为，在有车有房，还清贷款，不生大病的前提下，夫妻的税前年收入 70 万，到手 40-50 万，这才能保证 35-40 岁的生活将没有太大压力——也就是说，在夫妻收入均等的情况下，每个人的工资要达到近 30K。

　　你买了一套房子，每个月 1 万贷款，物业费、停车费、小孩学费、生活费，杂七杂八加在一起，在上海一年你要花掉 20 万。之前游戏行业加班压力大，你也不能保证你不生病，万一生一场病，看病可能又要十几万。

　　如果你过了 35 岁，收入还是平平，每个月收入 1 万多，还要还7-8K 的房贷，那我个人觉得就不要在一线城市混了。你生活压力过大，始终绷着弦，上班也不可能有好的状态。

　　“你觉得有多少人能在 35 岁前达到这条安全线？”

　　“100 个人里面可能有 10 几个吧。”

　　如果说月薪还是流动的数字，那作为更加可见，价值也更为可观的资产，房子则是更多人安全感的来源。可惜游戏行业的成功率已经不如以往，无法产生那么多新贵和富豪，房价上涨的速度更是几乎超越了游戏行业的发展。

　　你看看工资，想买房？怎么可能！你去问老板，老板心想，我创业成功也就买个房！一个公司上市了，也就能保证几个人买房。

　　抛开经济层面的安全，职场上的安全其实也拥有不低的门槛。小京计算了一个游戏人可能的发展轨迹——在游戏行业，35 岁很可能就意味着职业上升的停滞。

　　你要是刚开始在一家小公司，入职就是主管、主策，工作三年，最多也就能做到副总。可小公司的副总算什么呢？你想学到更多东西，获得更高的社会地位，那可能会去大公司，做一个主管或者经理。

　　如果你在同一家大公司工作 5 年以上，或者有7，8 年甚至十几年的行业背景，之前待过的都是大公司或者好项目，那也许能做到一家大公司的中层。

　　但做到中层之后你的上升空间在哪儿？如果不创业，走晋升通道，那你就要从业务线进入管理线。30 岁以上做到中层已经很难，一家游戏公司的高层更是可能只有 20 人左右，除非公司扩张，或者有高管犯错、离职，否则你升入高层的可能性非常低。

　　当然，也有人认为心态和能力才是安全感的来源。小W表示，中年职场上的安全感既不会来源于所在的平台，也不会来自职位 title，最终还是源于“不可替代”。“只有保持终身学习的心态，不断提升核心竞争力，到某个年龄阶段就该具备这个年龄段应有的经验和不断精进的技能，才不会被淘汰。”

　　吗啡也有类似的观点。“只要不是财务自由，你的收入就永远不够安全。你必须保持敏锐的行业洞察力，进行持续的自我学习和提升，调整心态，充满自信。我们这一行本来就和稳定绝缘，谁都不能保证有一个铁饭碗。”

　　出路

　　危机感易有，安全感难寻，管理岗太少，上升期不保。对于 35 岁“伪中年”的游戏从业者来说，面对突如其来的中年危机，出路可能会在哪里？

　　程序和美术或许是相对幸运的群体。虽然国内不大重视技术专家岗位的设置，但仍然有资深工程师、架构师、技术导师职位提供。不过吗啡介绍，如果一名程序始终没有参加过大用户量项目的历练，这条路是不大走得通的。

　　比如对后端程序而言，伴随用户量的增加，技术深度和难度其实在几何增长。如果你的产品没经历过大用户量、大数据的考验，你的技术水平是不够的。如果用户量不构成压力，那你就无法增强自己的技术，只能怪自己之前的选择太安逸了。

　　而策划、运营以及商务则大多没有这么幸运。在大部分游戏公司，这些工种并没有多少专家岗位的设置。如果大龄游戏人没有相关的管理经验，求职的难度要大上不少。

　　在职场发展遭遇天花板之后，许多既不想在大厂做螺丝钉，也不看好小公司长久发展的大龄游戏人难免会选择创业。小W表示，他最近在寻找资深的技术大牛，但发现端游时代或者手游早期的，40 岁左右的从业者大部分都积累了足够的原始资本，正在创业做自己喜欢的事情。

　　当然，创业并不一定是中年危机最好的解决方法。吗啡的一个朋友曾经外出创业做游戏三年，付出了无数心血，公司每年流水一度达到大几千万。但后来他们的第二款项目失败，他只能回到游戏公司，重新成为一名普通工程师。

　　他之前是技术经理，管着十几个人，如果继续做管理，做到技术总监应该没有问题。他亲口告诉我，创业毁掉了他的职业生涯。

　　在创业之外，也有许多大龄游戏人选择逃离。“我认识的很多人都已经跳出了游戏圈，而且不想再回来。游戏圈现在乱象丛生，风气也不大好，换皮、加班……还有很多人是为了赚钱，而不是做产品而做游戏。”事实上，吗啡现在的这份工作也和游戏研发无关。

　　离开北上广深则是更进一步的逃离方式，小京就认可了逃离北上广深的可能性。

　　你在上海的收入比老家高，那能不能努力工作，多挣些钱，回老家或者在上海周边买一套房？

　　你在上海的大公司做中层，但没有户口。那杭州有好的工作机会，可以让你在普通公司做高层，也帮你解决户口，那你是不是可以去杭州？这既解决了职业规划的问题，也解决了家庭的问题。

　　这种人不在少数。大家聊天经常会说这个人马上回家了，以后就不来上海了。

　　不过并非每个家庭都能承受这种逃离。老李也考虑过离开大城市，他觉得在哪里生活无所谓。可他的妻子并不同意：“做互联网行业的，去三四线城市你能干什么？这不是你一个人说了算的，这是整个家庭做的决策。”

　　危机

　　这就是游戏行业关于中年危机的现象、疑虑、挣扎和病症。

　　知名产品经理纯银认为，中年危机意味着“对这个世界的无力感，以及对老去的感知与恐惧。”而在葡萄君看来，中年危机意味着个体意识到自己已经不在这个世界的中心，清楚过往的选择无法改变，并准备在余下的时间承担过往选择的后果。

　　葡萄君承认，游戏是一个容易产生中年危机的行业。这个行业变化莫测，职业选择千千万万，顺风顺水太难太难；

　　葡萄君也明白，这是一个容易产生中年危机的时代。阶级壁垒日趋固化，社会流动越来越难。游戏行业已经不足以让你一夜暴富，实现阶层的跨越。

　　你当然有权利居安思危，谨慎地做出每个选择，将有车有房，月薪 30K 作为未来的主要目标，活出成功中年人应有的安详样子。可是如果达成这些目标，等到你 35 岁甚至 40 岁，中年危机就荡然无存了吗？

　　置身基层要抵抗生活成本的侵蚀，跻身中层要面临天花板的制约，晋升高层则要面对责任感的桎梏；物质的充盈无法填补精神的空虚，精神的丰富也不一定能满足物质的贫瘠。如果人人都将面临一场中年危机，那么，对中年危机的恐惧和抵御还有什么意义？

　　94 年的小四刚刚毕业，加入了一家大厂，她说自己从来没有想过中年危机的事情。“上升期不会有危机感。”；87 年的老梁则在一家初创公司负责核心项目的运营，项目数据节节攀升。“想过这个问题，但真的没有亲身体会。”

　　他们没有危机意识吗？或许没有。但事实上，他们正是所有还没感到中年危机的，少年不识愁滋味的游戏人的缩影。如果他们在中年危机还未到来之际就瞻前顾后，生怕选择了错误的道路，那游戏行业断然不会像今天一样有趣和可爱。

　　老李回忆，在他处于事业上升期的时候，也有人问过他“你买房了吗？”“你有户口吗？”等关乎中年危机的问题，当时他全然不放在心上。

　　其实这是个概率问题。99% 的人都解决不了这些问题，凭什么我觉得我是1%？但那时候我就想，我努力工作十年之后，难道还解决不了这些问题？

　　这就是一场 MOBA，对面六神装，你这边六个塔都没了，主宰也被打了，队友纷纷掉线，只剩下你一个。你还会打下去吗？你仍然会全力以赴。你想着对面 5 个人全部掉线你就赢了。

　　你总觉得你还有机会。

　　注：文中部分案例来自资深游戏猎头 Vivi 与豆丁";
            return t;
        }

        private static string GetSendData2()
        {
            return _sendRecorder++.ToString();
        }

        private static int _sendRecorder = 0;
        private static long _totalBytesReceived = 0;
        private static ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static List<AsyncUserToken> _asyncUserTokenPool = new List<AsyncUserToken>();
        private static object _locker = new object();
        private static string _remoteServerIP = ConfigurationManager.AppSettings["RemoteServerIP"];
        private static string _remoteServerPort = ConfigurationManager.AppSettings["RemoteServerPort"];

        private const int BUFFER_SIZE = 4096;
        private const int DATA_CHUNK_LENGTH_HEADER = 4;
    }
}
