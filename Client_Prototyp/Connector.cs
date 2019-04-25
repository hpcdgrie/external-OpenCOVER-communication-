using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Prototype Class that connects with a TCP-Socket to the HLRS-VRB-Server
namespace Client_Prototyp
{
    public partial class Connector : Form
    {
        TCP_Socket clientToHlrs;
        //Constructor
        public Connector()
        {
            InitializeComponent();
        }

        //return the Serve-rIp in the Message Box
        public string getHostname()
        {
            return tbHost.Text;
        }

        //return the Server-Port in the Message Box
        public int getPort()
        {
            return Convert.ToInt32(tbPort.Text);
        }

        //Connect to Server, receive Messages in seperate Thread and Send messages
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string ip = getHostname();
            int port = getPort ();

            //Connect with parameters
            clientToHlrs = new TCP_Socket(ip, port);

            //Init MessageBuffer
            MessageBuffer mb = new MessageBuffer();
            mb.add("COVER");
            mb.add(ip);

            // TODO: Wie muss der Konstrukoter der Message aussehen?
            Message msg = new Message(mb, Message.MessagesType.COVISE_MESSAGE_VRB_CONTACT);
            
            // This should be the Initial MessageType
            clientToHlrs.send_msg(msg);
        }

        // Close the Tcp-Socket
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            clientToHlrs.CloseSocket();
        }

    }
}
