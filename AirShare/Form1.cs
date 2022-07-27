using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace AirShare
{


    public partial class Form1 : Form
    {

        Thread thread = null, thread1,thread2;
        TcpListener server = null;
        TcpClient client = null;
        volatile string IP = "127.0.0.1";
        volatile int totalLength = 0;
        volatile string[] strlist;
        static volatile bool isClientRunning = true;
        static volatile string selectedFileName = null;
    

        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeServices() { 
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                isClientRunning = true;
                IP = GetLocalIPAddress();
                thread = new Thread(RunServer);
                thread.Start();
                UpdateIP(IP);

                thread1 = new Thread(UpdateService);
                thread1.Start();

                thread2 = new Thread(UpdateSpeed);
                thread2.Start();
            }
            catch (Exception exception) {
                Console.WriteLine(exception.Message);
            }
        }

        public void UpdateIP(string ip)
        {
            label5.Text = ip;
        }

        private  void RunServer()
        {
            try
            {
                server = new TcpListener(IPAddress.Parse(IP), 8888);
                server.Start();  // this will start the server
                this.Invoke(new Action(() => this.label2.Text = "SERVER STARTED"));

                while (isClientRunning)   //we wait for a connection
                {
                    client = server.AcceptTcpClient();  //if a connection exists, the server will accept it
                    this.Invoke(new Action(() => this.label2.Text = "SERVER CONNECTED"));
                    NetworkStream ns = client.GetStream(); //networkstream is used to send/receive messages

                    byte[] fileInfo = new byte[4096];
                    int len = ns.Read(fileInfo, 0, 4096);
                    string str = CleanMessage(fileInfo, len);
                    char[] separator = { ','};
                    strlist = str.Split(separator,2);

                    this.Invoke(new Action(() => this.progressBar1.Visible = true));
                    this.Invoke(new Action(() => this.progressBar1.Minimum = 0));
                    this.Invoke(new Action(() => this.progressBar1.Maximum = (Convert.ToInt32(strlist[1]))+5),null);

                 

                    string path = "C:\\Users\\Loki\\Downloads\\AirShare\\" + strlist[0];

                    using (FileStream fs = File.Create(path))
                    
                    Console.WriteLine("RECEIVING FILE");

                    FileStream streem = new FileStream(path, FileMode.OpenOrCreate);
                    totalLength = 0;
                    byte[] msg = new byte[1024];     //the messages arrive as byte array
                    int numByte = ns.Read(msg, 0, msg.Length);
                    totalLength += numByte;
                    this.Invoke(new Action(() => this.progressBar1.Value = (int)totalLength),null);
                    try
                    {
                        while ((numByte != 0))
                        {
                            streem.Write(msg, 0, 1024);
                            numByte = ns.Read(msg, 0, msg.Length);
                            totalLength += numByte;
                        }

                        Console.WriteLine("FILE RECEIVED");
                        this.Invoke(new Action(() => this.label2.Text = "FILE RECEIVED"));
                        this.Invoke(new Action(() => this.progressBar1.Value = (int)Convert.ToInt32(strlist[1])));
                        totalLength = 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        streem.Close();
                        ns.Dispose();
                    }
                    
                    ns.Dispose();
                    streem.Close();
                }
            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
        }


        private void RunClient()
        {
            try
            {
             
                    


            }
            catch (Exception d)
            {
                Console.WriteLine(d.Message);
            }
        }

        private void UpdateService() {
            
            try
            {
                while (true)
                {
                    this.Invoke(new Action(() => this.progressBar1.Value = (int)totalLength));
                    Thread.Sleep(500);
                }
            }
            catch (Exception exce)
            {
                Console.WriteLine(exce.Message);
            }
        }

        private void UpdateSpeed()
        {

            try
            {
                double start = 0;
                double intVal = 0;
                while (true)
                {
                    Thread.Sleep(900);
                    double totalSize = totalLength - start;
                    start = totalLength;
                    if (totalSize > 1024) {
                        totalSize = totalSize / 1024;
                        if (totalSize>1024)
                        {
                            totalSize = totalSize / 1024;
                            intVal = Math.Round(totalSize,2);
                            this.Invoke(new Action(() => this.label6.Text = Convert.ToString(intVal) + "MB/S"));
                        }
                        else
                        {
                            intVal = Math.Round(totalSize,2);
                            this.Invoke(new Action(() => this.label6.Text = Convert.ToString(intVal) + "KB/S"));
                        }
                    }
                    else
                    {
                        intVal = Math.Round(totalSize);
                        this.Invoke(new Action(() => this.label6.Text = Convert.ToString(intVal) + "Bits/S"));
                    }
                }
            }
            catch (Exception exce)
            {
                Console.WriteLine(exce.Message);
            }
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
        private static string CleanMessage(byte[] bytes, int size)
        {
            string message = Encoding.Default.GetString(bytes);
            string messageToPrint = null;
            
                for(int i = 0; i < size; i++)
                {
                    var nullChar = message[i];
                   if(nullChar != '\0')
                {
                    messageToPrint += nullChar;
                }
                }
         
            return messageToPrint;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try {
                isClientRunning = false;
                server.Stop();
                label2.Text = "ABORTED";
            }
            catch(Exception h)
            {
                Console.WriteLine(h.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                client.Close();
                label2.Text = "INTERRUPTED";
            }catch(NullReferenceException nu)
            {
                Console.WriteLine(nu.Message);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = "C:\\";
            //openFileDialog.Filter = ".mp3";
            openFileDialog.FilterIndex = 0;
            openFileDialog.RestoreDirectory = true;

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFileName = openFileDialog.FileName;
                Console.WriteLine(selectedFileName);
                label1.Text = selectedFileName;
            }
        }
    }
    
}
