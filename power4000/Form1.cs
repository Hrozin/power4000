﻿using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace power4000
{
    public partial class Form1 : Form
    {
        private Thread keepAliveThread;
        private Socket socket;
        private int sequenceNumber = 0; // Sequence number for col_Seq
        private bool stopKeepAlive = false; // Flag to stop the keep-alive thread

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtip.Text = GetLocalIPAddress();
            txtport.Text = "4545";
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            string serverIp = txtip.Text;
            int serverPort = int.Parse(txtport.Text);

            try
            {
                // Create a TCP/IP socket.
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(serverIp);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, serverPort);

                // Connect to the remote endpoint.
                socket.Connect(remoteEP);

                Logger.Log("Connected to server at " + serverIp + ":" + serverPort);

                // Execute message sequences
                if (SendInitialMessage(socket))
                {
                    if (SendSecondMessage(socket))
                    {
                        if (SendThirdMessage(socket))
                        {
                            if (SendFourthMessage(socket))
                            {
                                // Start keep-alive thread after the last message
                                StartKeepAliveThread();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception: " + ex.Message);
                MessageBox.Show("Exception: " + ex.Message);
            }
        }

        private bool SendInitialMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200001001         ", "0002");
        }

        private bool SendSecondMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200060001         ", "0005");
        }

        private bool SendThirdMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200051001         ", "0005");
        }

        private bool SendFourthMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200034001         ", "0005");
        }

        private bool SendAndReceive(Socket socket, string message, string expectedResponse)
        {
            // Send data to the server
            SendData(socket, message);

            // Receive data from the server
            string responseData = ReceiveData(socket);

            // Extract col_Mid value from received data
            string colMidValue = responseData.Substring(4, 4);

            // Display received data in Dgv
            DisplayDataInDgv(responseData, "RECV");

            // Check if the response matches the expected value
            if (colMidValue == expectedResponse)
            {
                return true;
            }
            else
            {
                string errorMsg = $"Unexpected response received: {colMidValue}";
                Logger.Log(errorMsg);
                MessageBox.Show(errorMsg);
                return false;
            }
        }

        private void SendData(Socket socket, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            socket.Send(data);

            Logger.Log("송신: " + message);

            // Display sent data in Dgv
            DisplayDataInDgv(message, "SEND");
        }

        private string ReceiveData(Socket socket)
        {
            byte[] buffer = new byte[256];
            int bytesRead = socket.Receive(buffer);
            string responseData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Logger.Log("수신: " + responseData);

            return responseData;
        }

        private void DisplayDataInDgv(string data, string type)
        {
            string colMidValue = data.Substring(4, 4);
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            Dgv.Invoke((Action)(() =>
            {
                // Insert the new row at the top (index 0)
                Dgv.Rows.Insert(0);
                DataGridViewRow newRow = Dgv.Rows[0];
                newRow.Cells["col_Seq"].Value = sequenceNumber++;
                newRow.Cells["col_Time"].Value = currentTime;
                newRow.Cells["col_Mid"].Value = colMidValue;
                newRow.Cells["col_Type"].Value = type;

                if (type == "RECV")
                {
                    newRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
            }));

            // Call CheckReceivedData to handle specific responses
            CheckReceivedData(colMidValue);
        }

        private void CheckReceivedData(string colMidValue)
        {
            if (colMidValue == "0052")
            {
                // Stop keep-alive thread
                stopKeepAlive = true;
                // Wait for 2 seconds before sending the response
                //Thread.Sleep(2000);
                // Send the response message
                SendResponseMessage();
                //SendData(socket, "00200036001         ");
                // Restart keep-alive thread
                StartKeepAliveThread();
            }
            else if (colMidValue == "0035")
            {
                // Stop keep-alive thread
                stopKeepAlive = true;
                // Send the response message for "0035"
                SendData(socket, "00200036001         ");
                // Restart keep-alive thread
                StartKeepAliveThread();
            }
            else if (colMidValue == "0061")
            {
                // Stop keep-alive thread
                stopKeepAlive = true;
                // Send the response message for "0061"
                SendData(socket, "00200062001         ");
                // Restart keep-alive thread
                StartKeepAliveThread();
            }
        }

        private void SendResponseMessage()
        {
            string responseMessage = "00200053001         ";
            SendData(socket, responseMessage);


        }

        private void StartKeepAliveThread()
        {
            stopKeepAlive = false;
            keepAliveThread = new Thread(KeepConnectionAlive);
            keepAliveThread.IsBackground = true; // Make sure the thread ends when the main application ends
            keepAliveThread.Start();
        }

        private void KeepConnectionAlive()
        {
            while (!stopKeepAlive)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        // Send keep-alive message
                        string keepAliveMessage = "00209999001         ";
                        SendData(socket, keepAliveMessage);

                        // Receive response from server
                        string responseData = ReceiveData(socket);

                        // Display received data in Dgv
                        DisplayDataInDgv(responseData, "RECV");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("유지 중 예외 발생: " + ex.Message);
                    MessageBox.Show("유지 중 예외 발생: " + ex.Message);
                    break;
                }
                Thread.Sleep(3000); // 3 seconds interval
            }
        }        

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("로컬 IP를 찾을 수 없습니다.");
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_min_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btn_end_Click(object sender, EventArgs e)
        {
            try
            {
                // Stop the keep-alive thread
                stopKeepAlive = true;

                // Close the socket connection
                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    Logger.Log("소켓 닫힘");
                }
            }
            catch (Exception ex)
            {
                Logger.Log("종료 중 예외 발생: " + ex.Message);
                MessageBox.Show("종료 중 예외 발생: " + ex.Message);
            }
        }
    }
}
