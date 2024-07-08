using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace power4000
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource keepAliveCancellationTokenSource;
        private Socket socket;
        private int sequenceNumber = 0; // Sequence number for col_Seq
        bool TagMove;
        int MValX, MValY;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtip.Text = LocalIP();
            txtport.Text = "4545";
        }

        private async void btn_start_Click(object sender, EventArgs e)
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
                await socket.ConnectAsync(remoteEP);

                Logger.Log("서버연결 " + serverIp + ":" + serverPort);

                // Execute message sequences
                if (await SendInitialMessage(socket))
                {
                    if (await SendSecondMessage(socket))
                    {
                        if (await SendThirdMessage(socket))
                        {
                            if (await SendFourthMessage(socket))
                            {
                                // Start keep-alive and receive threads after the last message
                                StartKeepAliveThread();
                                StartRECVThread();
                            }
                        }
                    }
                }                

            }
            catch (Exception ex)
            {
                Logger.Log("예외: " + ex.Message);
                MessageBox.Show("예외: " + ex.Message);
            }
        }

        private Task<bool> SendInitialMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200001001         ", "0002");
        }

        private Task<bool> SendSecondMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200060001         ", "0005");
        }

        private Task<bool> SendThirdMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200051001         ", "0005");
        }

        private Task<bool> SendFourthMessage(Socket socket)
        {
            return SendAndReceive(socket, "00200034001         ", "0005");
        }

        private async Task<bool> SendAndReceive(Socket socket, string message, string expectedResponse)
        {
            // Send data to the server
            await SendData(socket, message);

            // Receive data from the server
            string responseData = await ReceiveData(socket);

            // Extract col_Mid value from received data
            string MidValue = responseData.Substring(4, 4);

            // Display received data in Dgv
            DataInDgv(responseData, "RECV");

            // Check if the response matches the expected value
            if (MidValue == expectedResponse)
            {
                return true;
            }
            else
            {
                string errorMsg = $"Unexpected response received: {MidValue}";
                Logger.Log(errorMsg);
                MessageBox.Show(errorMsg);
                return false;
            }
        }

        private async Task SendData(Socket socket, string message)
        {
            byte[] data = Encoding.ASCII.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

            Logger.Log("송신: " + message);

            // Display sent data in Dgv
            DataInDgv(message, "SEND");
        }

        private async Task<string> ReceiveData(Socket socket)
        {
            byte[] buffer = new byte[256];
            int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            string responseData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Logger.Log("수신: " + responseData);

            return responseData;
        }

        private void DataInDgv(string data, string type)
        {
            string MidValue = data.Substring(4, 4);
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            Dgv.Invoke((Action)(() =>
            {

                // Check if the data already exists in Dgv to prevent duplicates
                bool alreadyExists = Dgv.Rows.Cast<DataGridViewRow>().Any(row =>
                    row.Cells["col_Time"].Value?.ToString() == currentTime &&
                    row.Cells["col_Mid"].Value?.ToString() == MidValue &&
                    row.Cells["col_Type"].Value?.ToString() == type);

                // Insert the new row at the top (index 0)
                Dgv.Rows.Insert(0);
                DataGridViewRow newRow = Dgv.Rows[0];
                newRow.Cells["col_Seq"].Value = sequenceNumber++;
                newRow.Cells["col_Time"].Value = currentTime;
                newRow.Cells["col_Mid"].Value = MidValue;
                newRow.Cells["col_Type"].Value = type;

                if (type == "RECV")
                {
                    newRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
            }));

            // Call CheckReceivedData to handle specific responses
            //_ = CheckReceivedData(MidValue); // Fire and forget
        }

        private Task CheckReceivedData(string MidValue)
        {
            switch (MidValue)
            {
                case "0052":
                    return Response0052();
                case "0035":
                    return Response0035();
                case "0061":
                    return Response0061();
                default:
                    return Task.CompletedTask;
            }
        }

        private async Task Response0052()
        {
            // Stop keep-alive thread
            //keepAliveCancellationTokenSource.Cancel();
            // Send the response message
            await SendData(socket, "00200053001         ");                        
        }

        private async Task Response0035()
        {
            // Stop keep-alive thread
            keepAliveCancellationTokenSource.Cancel();
            // Send the response message for "0035"
            await SendData(socket, "00200036001         ");
        }

        private async Task Response0061()
        {
            // Stop keep-alive thread
            keepAliveCancellationTokenSource.Cancel();
            // Send the response message for "0061"
            await SendData(socket, "00200062001         ");
        }

        private void StartKeepAliveThread()
        {
            keepAliveCancellationTokenSource = new CancellationTokenSource();
            var token = keepAliveCancellationTokenSource.Token;
            Task.Run(() => KeepConn(token), token);
        }

        private async Task KeepConn(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        // Send keep-alive message
                        string keepAliveMessage = "00209999001         ";
                        await SendData(socket, keepAliveMessage);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("유지 중 예외 발생: " + ex.Message);
                    MessageBox.Show("유지 중 예외 발생: " + ex.Message);
                    break;
                }
                await Task.Delay(8000);
            }
        }

        private void StartRECVThread()
        {
            Task.Run(ReceiveDataLoop);
        }

        private async Task ReceiveDataLoop()
        {
            while (true)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        string responseData = await ReceiveData(socket);
                        if (!string.IsNullOrEmpty(responseData))
                        {
                            // Display received data in Dgv
                            DataInDgv(responseData, "RECV");

                            // Extract col_Mid value from received data
                            string MidValue = responseData.Substring(4, 4);

                            // Check received data
                            await CheckReceivedData(MidValue);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("데이터 수신 중 예외 발생: " + ex.Message);
                    MessageBox.Show("데이터 수신 중 예외 발생: " + ex.Message);
                    break;
                }
            }
        }

        private string LocalIP()
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
            Close();
        }

        private void btn_min_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btn_end_Click(object sender, EventArgs e)
        {
            try
            {
                // Stop the keep-alive thread
                if (keepAliveCancellationTokenSource != null)
                {
                    keepAliveCancellationTokenSource.Cancel();
                }

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

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            TagMove = true;
            MValX = e.X;
            MValY = e.Y;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            TagMove = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (TagMove == true)
            {
                SetDesktopLocation(MousePosition.X - MValX, MousePosition.Y - MValY);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            TagMove = true;
            MValX = e.X;
            MValY = e.Y;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            TagMove = false;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (TagMove == true)
            {
                SetDesktopLocation(MousePosition.X - MValX, MousePosition.Y - MValY);
            }
        }
    }
}
