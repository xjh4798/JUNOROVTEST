using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

namespace JUNOROV
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("------------程序开始(Ctrl+C退出)------------");
            TcpListener server = null;
            

            try
            {
                server = new TcpListener(IPAddress.Parse("10.126.243.211"), 8080);
                server.Start();//开始监听客户端的请求
                Byte[] result = new Byte[9];//缓存读入的数据
                Console.WriteLine("-------------服务已启动(Ctrl+C退出)---------------");
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();//获取用于读取和写入的流对象
                int i;
                Random random = new Random();
                
                while ((i = stream.Read(result, 0, result.Length)) != 0)
                {
                    //接收消息打印与解析
                    if (i!= 9)
                    {
                        Console.WriteLine("-----------------消息长度错误，开启下次接收--------------");
                        continue;
                    }
                    byte[] rec_check_first = new byte[4] { 0x00, 0x00, 0x00,0x05 };
                    if (!Enumerable.SequenceEqual(result.Skip(0).Take(4), rec_check_first))
                    {
                        Console.WriteLine("-----------------消息指令头错误，开启下次接收--------------");
                        continue;
                    }
                    byte rec_check_end = Get_CheckXor(result.Skip(4).Take(4).ToArray());
                    if (result[8] != rec_check_end)
                    {
                        Console.WriteLine("-----------------消息异或校验错误，开启下次接收--------------");
                        continue;
                    }
                    byte[] msg = { 1, 2, 3, 4, 5, 6, 7, 8, 9,10};
                    switch (result[5])
                    {
                        case 0x10:
                            switch (result[6])
                            {
                                case 0x01:
                                    msg = AppendBack(ROV_Init_ok);
                                    Console.WriteLine("初始化ROV");
                                    break;
                                case 0x02://打开深度自稳定
                                    Console.WriteLine("打开深度自稳定");
                                    msg = AppendBack(ROV_SelfStabilizing_ok);
                                    break;
                                case 0x03://关闭深度自稳定
                                    Console.WriteLine("关闭深度自稳定");
                                    msg = AppendBack(ROV_SelfStabilizing_no);
                                    break;
                                case 0x04://下位机重启
                                    Console.WriteLine("下位机重启");
                                    break;
                            }
                            break;
                        case 0x20:
                            switch (result[6])
                            {
                                case 0x11://前
                                    //result[7]是跨步步长单位s
                                    Console.WriteLine($"向前{result[7]}s");
                                    msg = AppendBack(ROV_ROV_hMove);
                                    break;
                                case 0x22://hou
                                    Console.WriteLine($"向后{result[7]}s");
                                    msg = AppendBack(ROV_ROV_hMove);
                                    break;
                                case 0x33://zuo
                                    Console.WriteLine($"向左{result[7]}s");
                                    msg = AppendBack(ROV_ROV_hMove);
                                    break;
                                case 0x44://you
                                    Console.WriteLine($"向右{result[7]}s");
                                    msg = AppendBack(ROV_ROV_hMove);
                                    break;
                            }
                            break;
                        case 0x21: //向上
                            //result[6]高位 result[7]低位
                            ushort combinedup = (ushort)(result[6] << 8 | result[7]);
                            Console.WriteLine($"向上{combinedup}脉冲");
                            msg = AppendBack(ROV_ROV_vMove);
                            break;
                        case 0x22: //向下
                            //result[6]高位 result[7]低位
                            ushort combineddown = (ushort)(result[6] << 8 | result[7]);
                            Console.WriteLine($"向下{combineddown}脉冲");
                            msg = AppendBack(ROV_ROV_vMove);
                            break;
                        case 0x30:
                            switch (result[7]) //result[7]是打开或关闭
                            {
                                case 0x00:
                                    switch (result[6])
                                    {
                                        case 0x01://T1
                                            Console.WriteLine("打开T1");
                                            msg = AppendBack(ROV_ROV_Transducer1_open);
                                            break;
                                        case 0x02://T2
                                            Console.WriteLine("打开T2");
                                            msg = AppendBack(ROV_ROV_Transducer2_open);
                                            break;
                                        case 0x03://T3
                                            Console.WriteLine("打开T3");
                                            msg = AppendBack(ROV_ROV_Transducer3_open);
                                            break;
                                        case 0x04://T4
                                            Console.WriteLine("打开T4");
                                            msg = AppendBack(ROV_ROV_Transducer4_open);
                                            break;
                                        case 0x05://T5
                                            Console.WriteLine("打开T5");
                                            msg = AppendBack(ROV_ROV_Transducer5_open);
                                            break;
                                        case 0x06://C
                                            Console.WriteLine("打开CAMANER");
                                            msg = AppendBack(ROV_ROV_Camerar_open);
                                            break;
                                        case 0x07://E
                                            Console.WriteLine("打开E");
                                            msg = AppendBack(ROV_ROV_Electromagnet_open);
                                            break;
                                    }
                                    break;
                                case 0x11:
                                    switch (result[6])
                                    {
                                        case 0x01://T1
                                            Console.WriteLine("关闭T1");
                                            msg = AppendBack(ROV_ROV_Transducer1_close);
                                            break;
                                        case 0x02://T2
                                            Console.WriteLine("关闭T2");
                                            msg = AppendBack(ROV_ROV_Transducer2_close);
                                            break;
                                        case 0x03://T3
                                            Console.WriteLine("关闭T3");
                                            msg = AppendBack(ROV_ROV_Transducer3_close);
                                            break;
                                        case 0x04://T4
                                            Console.WriteLine("关闭T4");
                                            msg = AppendBack(ROV_ROV_Transducer4_close);
                                            break;
                                        case 0x05://T5
                                            Console.WriteLine("关闭T5");
                                            msg = AppendBack(ROV_ROV_Transducer5_close);
                                            break;
                                        case 0x06://C
                                            Console.WriteLine("关闭camera");
                                            msg = AppendBack(ROV_ROV_Camerar_close);
                                            break;
                                        case 0x07://E
                                            Console.WriteLine("关闭E");
                                            msg = AppendBack(ROV_ROV_Electromagnet_close);
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case 0x40:
                            switch (result[6])
                            {
                                case 0x00://
                                    //result[7]是修改的系数
                                    Console.WriteLine($"设置速度系数{result[7]}");
                                    msg = AppendBack(ROV_Para_ok);
                                    break;
                                case 0x01://
                                    Console.WriteLine($"设置加速度系数{result[7]}");
                                    msg = AppendBack(ROV_Para_ok);

                                    break;
                                case 0x02://
                                    Console.WriteLine($"设置减速度系数{result[7]}");
                                    msg = AppendBack(ROV_Para_ok);

                                    break;
                                case 0x03://
                                    Console.WriteLine($"设置水平电机速度系数{result[7]}");
                                    msg = AppendBack(ROV_Para_ok);

                                    break;
                            }
                            break;
                        case 0x50:
                            // 组合成ushort
                            int originalNumber = (ushort)((result[6] << 8) | result[7]) / 2000;
                            Console.WriteLine($"设置目标深度{originalNumber}");
                            break;
                    }
                    
                    stream.Write(msg, 0, msg.Length);



                    //给定80%的概率生成随机数作为压力什么的回传值
                    double randomValue = random.NextDouble();
                    if(randomValue < 0.8)
                    {

                    }
                }
                client.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("------------------程序退出----------------");

        }







        public static string ByteToHexStr(byte[] bytes)
        {
            if (bytes == null)
                return "";
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(string.Format("{0:X2} ", bytes[i]));
            }
            string returnStr = builder.ToString().Trim();
            return returnStr;
        }

        public static byte[] AppendBack(byte[] bytes)
        {
            List<byte> bList = bytes.ToList<byte>();
            bList.Add(Get_CheckXor(bytes.Skip(4).Take(5).ToArray()));
            return bList.ToArray();
        }

        public static byte Get_CheckXor(byte[] data)
        {
            Byte CheckCode = 0;
            int len = data.Length;
            for (int i = 0; i < len; i++)
            {
                CheckCode ^= data[i];
            }
            return CheckCode;
        }

        public static byte[] ROV_Init_ok                 = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x10, 0x33, 0x00, 0x00 };//初始化完成指令
        public static byte[] ROV_SelfStabilizing_ok      = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x10, 0x66, 0x00, 0x00 };//深度自稳定打开
        public static byte[] ROV_SelfStabilizing_no      = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x10, 0x77, 0x00, 0x00 };//深度自稳定关闭
        public static byte[] ROV_Para_ok                 = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x40, 0x11, 0x00, 0x00 };//参数设置成功
        public static byte[] ROV_ROV_vMove               = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x20, 0x11, 0x00, 0x00 };//垂直电机执行完毕
        public static byte[] ROV_ROV_hMove               = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x20, 0x22, 0x00, 0x00 };//水平电机执行完毕
        public static byte[] ROV_ROV_Transducer1_open    = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x01, 0x11, 0x00 };//打开换能器1完毕
        public static byte[] ROV_ROV_Transducer2_open    = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x02, 0x11, 0x00 };//打开换能器2完毕
        public static byte[] ROV_ROV_Transducer3_open    = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x03, 0x11, 0x00 };//打开换能器3完毕
        public static byte[] ROV_ROV_Transducer4_open    = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x04, 0x11, 0x00 };//打开换能器4完毕
        public static byte[] ROV_ROV_Transducer5_open    = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x05, 0x11, 0x00 };//打开换能器5完毕
        public static byte[] ROV_ROV_Camerar_open        = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x06, 0x11, 0x00 };//打开摄像机完毕
        public static byte[] ROV_ROV_Electromagnet_open  = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x07, 0x11, 0x00 };//打开电磁铁完毕
        public static byte[] ROV_ROV_Transducer1_close   = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x01, 0x00, 0x00 };//关闭换能器1完毕
        public static byte[] ROV_ROV_Transducer2_close   = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x02, 0x00, 0x00 };//关闭换能器2完毕
        public static byte[] ROV_ROV_Transducer3_close   = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x03, 0x00, 0x00 };//关闭换能器3完毕
        public static byte[] ROV_ROV_Transducer4_close   = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x04, 0x00, 0x00 };//关闭换能器4完毕
        public static byte[] ROV_ROV_Transducer5_close   = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x05, 0x00, 0x00 };//关闭换能器5完毕
        public static byte[] ROV_ROV_Camerar_close       = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x06, 0x00, 0x00 };//关闭摄像机完毕
        public static byte[] ROV_ROV_Electromagnet_close = new byte[9] { 0x00, 0x00, 0x00, 0x06, 0x50, 0x30, 0x07, 0x00, 0x00 };//关闭电磁铁完毕
 
     }
}
