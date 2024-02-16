using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChattingServerConsoleClient
{
    class ConsoleClient
    {
        TcpClient client = null;
        Thread receiveMessageThread = null;
        ConcurrentBag<string> sendMessageListToVIew = null;
        ConcurrentBag<string> receiveMessageListToView = null;
        private string name = null;

        public void Run()
        {
            sendMessageListToVIew = new ConcurrentBag<string>();
            receiveMessageListToView = new ConcurrentBag<string>();

            receiveMessageThread = new Thread(receiveMessage);
            Console.WriteLine("============= 클라이언트 =============");
            Console.WriteLine("1. 서버연결");
            Console.WriteLine("2. Message 보내기");
            Console.WriteLine("3. 보낸 Message 확인");
            Console.WriteLine("4. 받은 Message 확인");
            Console.WriteLine("0. 종료");
            Console.WriteLine("======================================");

            while (true)
            {
                string key = Console.ReadLine();
                int order = 0;

                if(int.TryParse(key, out order)) 
                { 
                    switch(order)
                    {
                        case StaticDefine.CONNECT:
                            {
                                if(client != null)
                                {
                                    Console.WriteLine("이미 연결되어있습니다.");
                                }
                                else
                                {
                                    Connect();
                                }
                                break;
                            }
                        case StaticDefine.SEND_MESSAGE:
                            {
                                if(client == null)
                                {
                                    Console.WriteLine("먼저 서버와 연결해주세요.");
                                }
                                else
                                {
                                    SendMessage();
                                }
                                break;
                            }
                        case StaticDefine.SEND_MSG_VIEW:
                            {
                                SendMessageView();
                                break;
                            }
                        case StaticDefine.RECEIVE_MSG_VIEW:
                            {
                                ReceiveMessageView();
                                break;
                            }
                        case StaticDefine.EXIT:
                            {
                                if(client != null)
                                {
                                    client.Close();
                                }
                                receiveMessageThread.Abort();
                                return;
                            }
                    }
                }
                else
                {
                    Console.WriteLine("잘못 입력하셨습니다.");
                }
                Thread.Sleep(50);
            }
        }

        private void SendMessageView()
        {
            if (sendMessageListToVIew.Count == 0)
            {
                Console.WriteLine("보낸 메시지가 없습니다.");
                return;
            }
            foreach (var item in sendMessageListToVIew)
            {
                Console.WriteLine(item);
            }
        }

        private void ReceiveMessageView()
        {
            if (receiveMessageListToView.Count == 0)
            {
                Console.WriteLine("보낸 메시지가 없습니다.");
                return;
            }
            foreach (var item in receiveMessageListToView)
            {
                Console.WriteLine(item);
            }
        }
        private void receiveMessage()
        {
            string receiveMessage = "";
            List<string> receiveMessageList = new List<string>();

            while (true)
            {
                byte[] receiveByte = new byte[1024];
                client.GetStream().Read(receiveByte, 0, receiveByte.Length);

                receiveMessage = Encoding.Default.GetString(receiveByte);

                string[] receiveMessageArray = receiveMessage.Split('>');
                foreach(var item in receiveMessageArray)
                {
                    if (!item.Contains("<"))
                        continue;
                    if (item.Contains("관리자<TEST"))
                        continue;
                    receiveMessageList.Add(item);
                }

                ParsingReceiveMessage(receiveMessageList);

                Thread.Sleep(500);
            }
        }

        private void ParsingReceiveMessage(List<string> messageList)
        {
            foreach(var item in messageList) 
            {
                string sender = "";
                string message = "";

                if (item.Contains('<'))
                {
                    string[] splitedMsg = item.Split('<');

                    sender = splitedMsg[0];
                    message = splitedMsg[1];

                    if(sender == "관리자")
                    {
                        string userList = "";
                        string[] splitedUser = message.Split('$');
                        foreach(var el in splitedUser)
                        {
                            if (string.IsNullOrEmpty(el))
                                continue;
                            userList += el + " ";
                        }
                        Console.WriteLine(string.Format("[현재 접속인원] {0}", userList));
                        messageList.Clear();
                        return;
                    }

                    Console.WriteLine(string.Format("[메시지가 도착하였습니다] {0} : {1}", sender, message));
                    receiveMessageListToView.Add(string.Format("[{0}] Sender : {1}, Message : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),sender, message));
                }
            }
            messageList.Clear();
        }

        private void SendMessage()
        {
            string getUserList = string.Format("{0}<GiveMeUserList>", name);
            byte[] getUserByte = Encoding.Default.GetBytes(getUserList);
            client.GetStream().Write(getUserByte, 0, getUserByte.Length);

            Console.WriteLine("수신자를 입력해주세요");
            string receiver = Console.ReadLine();

            Console.WriteLine("보낼 Message를 입력해주세요");
            string message = Console.ReadLine();

            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
            {
                Console.WriteLine("수신자와 보낼 Message를 확인해주세요.");
                return;
            }

            string parsedMessage = string.Format("{0}<{1}>", receiver, message);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedMessage);

            client.GetStream().Write(byteData, 0, byteData.Length);
            sendMessageListToVIew.Add(string.Format("[{0}] Receiver : {1}, Message : {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), receiver, message));
            Console.WriteLine("전송성공");
        }

        private void Connect()
        {
            Console.WriteLine("이름을 입력해주세요");
            name = Console.ReadLine();

            string parsedName = "%^&" + name;
            if(parsedName == "%^&")
            {
                Console.WriteLine("제대로된 이름을 입력해주세요");
                return;
            }

            client = new TcpClient();
            client.Connect("127.0.0.1", 708);

            byte[] byteData = new byte[1024];
            byteData = Encoding.Default.GetBytes(parsedName);
            client.GetStream().Write(byteData , 0, byteData.Length);

            receiveMessageThread.Start();

            Console.WriteLine("서버연결 성공. 이제 Message를 보낼 수 있습니다.");
            
        }
    }
}
