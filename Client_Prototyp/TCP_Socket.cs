using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client_Prototyp
{
    public class TCP_Socket
    {
        public TcpClient TcpClientInstance;
        public NetworkStream ns;
        const int BUFFER_SIZE = 6400;

        //Create the Tcp-Socket and starts the receive Method in seperate Thread
        public TCP_Socket(string IP, int Port)
        {
            TcpClientInstance = new TcpClient(IP, Port);
            if (TcpClientInstance.Connected) // direkt nach dem Verbindungsaufbau wird als erstes mal ein Byte ausgetauscht.
            {
                // Sends data immediately upon calling NetworkStream.Write.
                TcpClientInstance.NoDelay = true;
                LingerOption lingerOption = new LingerOption(false, 0);
                TcpClientInstance.LingerState = lingerOption;

                NetworkStream s = TcpClientInstance.GetStream();
                Byte[] data = new Byte[256];
                data[0] = 1;
                try
                {
                    s.Write(data, 0, 1);
                }
                catch (System.IO.IOException)
                {
                    // probably socket closed
                    return;
                }

                int numRead = 0;
                try
                {
                    numRead = s.Read(data, 0, 1);
                }
                catch (System.IO.IOException)
                {
                    // probably socket closed
                    return;
                }
                Task.Run(() => this.Read());
            }
        }

        //  Send Messages to connected Server
        public void send_msg(Message message)
        {
            int len = message.message.buf.Length + (4 * 4);
            Byte[] data = new Byte[len];
            ByteSwap.swapTo((uint)message.type, data, 2 * 4);
            ByteSwap.swapTo((uint)message.message.buf.Length, data, 3 * 4);
            message.message.buf.CopyTo(data, 4 * 4);
            TcpClientInstance.GetStream().Write(data, 0, len);
            // das geht bestimmt auch asynchron, ich hab einfach das kopiert was im Revit Plugin funktioniert.
            //ns = TcpClientInstance.GetStream();
            //ns.BeginWrite(data, 0, data.Length, EndSend, data);
        }

   
        // Method to start sending Messages
        public void BeginSend(string data)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(data);
                var ns = TcpClientInstance.GetStream();
                ns.BeginWrite(bytes, 0, bytes.Length, EndSend, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        // Async Task to Read incoming Messages
        public async Task Read()
        {
            var buffer = new byte[64000];
            var ns = TcpClientInstance.GetStream();

            while (true)
            {
                /*var bytesRead = await ns.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) return; // Stream was closed
                string receivedMessage = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Console.WriteLine(receivedMessage);*/
                int len = 0;
                while (len < 16)
                {
                    int numRead;
                    try
                    {
                        numRead = ns.Read(buffer, len, 16 - len);
                    }
                    catch (System.IO.IOException)
                    {
                        // probably socket closed
                        return;
                    }
                    len += numRead;
                }

                int msgType = BitConverter.ToInt32(buffer, 2 * 4);
                int length = BitConverter.ToInt32(buffer, 3 * 4);
                length = (int)ByteSwap.swap((uint)length);
                msgType = (int)ByteSwap.swap((uint)msgType);
                len = 0;
                while (len < length)
                {
                    int numRead;
                    try
                    {
                        numRead = ns.Read(buffer, len, length - len);
                    }
                    catch (System.IO.IOException)
                    {
                        // probably socket closed
                        return;
                    }
                    len += numRead;
                }
                Message m = new Message(new MessageBuffer(buffer), (Client_Prototyp.Message.MessagesType)msgType);
            }
        }

        // Method to Close the Socket
        public void CloseSocket()
        {
            TcpClientInstance.Close();
        }

        // Method to cancel sending Messages
        public void EndSend(IAsyncResult result)
        {
            try
            {
                var bytes = (byte[])result.AsyncState;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }

}
