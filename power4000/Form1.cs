using System;
using System.Diagnostics;
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
        private int Sequence = 1;
        bool FormMove;
        int FormMX, FormMY; // 폼 X, Y
        // 동기화 객체 추가, 초기 항목 수와 최대 동시 수를 지정, 스레드들이 순차적 접근하도록 하는 역할
        private SemaphoreSlim sendSemaphore = new SemaphoreSlim(1, 1);

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
            btn_start.Enabled = false;
            btn_end.Enabled = true;
            btn_exit.Enabled = true;

            txtip.ReadOnly = true;
            txtport.ReadOnly = true;

            string IP = txtip.Text;
            int Port = int.Parse(txtport.Text);

            try
            {
                // 소켓 생성
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

                // 서버에 연결
                await socket.ConnectAsync(remoteEP);

                Logger.Log("Server Connecting " + IP + ": " + Port);

                // 메시지 시퀀스 실행
                if (await BeforeWaitingMode(socket))
                {
                    // 마지막 메시지 이후 Keep-Alive 메시지와 RECV(스레드) 시작                
                    WaitingMode();
                    RecvThread();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "Server Connection Fail : " + ex.Message;
                MessageBox.Show(errorMsg, "Connection Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log("Exception: " + ex.Message);
            }
        }



        //Waiting Mode 이전 값들 순차 송신
        private async Task<bool> BeforeWaitingMode(Socket socket)
        {
            string[] Signal = { "00200001001         ", "00200060001         ", "00200051001         ", "00200034001         " };
            string[] SignalRes = { "0002", "0005", "0005", "0005" };

            for (int i = 0; i < Signal.Length; i++)
            {
                Logger.LogData(Signal[i], "SEND"); // Added logging for SEND messages) (Server)
                bool result = await SendAndRecvWithRetry(socket, Signal[i], SignalRes[i], 3, 8000);
                if (!result)
                {
                    Logger.LogData(Signal[i], "SEND FAIL"); // Added logging for failed SEND messages
                    return false;
                }
                //Logger.LogData(SignalRes[i], "RECV"); // Added logging for RECV messages (Server)
            }            
            return true;
        }



        // 송수신이 되지 않을 경우 Retry메세지 박스 및 재시도
        private async Task<bool> SendAndRecvWithRetry(Socket socket, string sendMsg, string expectedRecvMsg, int maxRetries, int delay)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                bool result = await SendAndRecv(socket, sendMsg, expectedRecvMsg);
                if (result)
                {
                    return true;
                }
                else if (attempt < maxRetries - 1)
                {
                    DialogResult retryResult = MessageBox.Show(
                        $"Failed to receive the message. {maxRetries - attempt - 1} more attempts left. Do you want to continue?",
                        "Retry",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Warning);

                    if (retryResult == DialogResult.Cancel)
                    {
                        return false;
                    }
                    await Task.Delay(delay);
                }
            }
            EndOfConn();
            return false;
        }



        // 송수신
        private async Task<bool> SendAndRecv(Socket socket, string sendMsg, string expectedRecvMsg)
        {
            try
            {
                // 동기화 시작
                await sendSemaphore.WaitAsync();

                byte[] sendBuffer = Encoding.ASCII.GetBytes(sendMsg);
                await socket.SendAsync(new ArraySegment<byte>(sendBuffer), SocketFlags.None);

                byte[] recvBuffer = new byte[1024];
                int recvBytes = await socket.ReceiveAsync(new ArraySegment<byte>(recvBuffer), SocketFlags.None);
                string recvMsg = Encoding.ASCII.GetString(recvBuffer, 0, recvBytes);

                Invoke((Action)(() =>
                {
                    DataInDgv(sendMsg, "SEND");
                    DataInDgv(recvMsg, "RECV");
                }));

                //Logger.LogData(sendMsg, "SEND"); // Added logging for SEND messages, Waiting Mode 이전 메세지
                Logger.LogData(recvMsg, "RECV"); // Added logging for RECV messages, Waiting Mode 이전 메세지

                return recvMsg.Contains(expectedRecvMsg);
            }
            catch (Exception ex)
            {
                Logger.Log("SendAndRecv Exception: " + ex.Message);
                return false;
            }
            finally
            {
                // 동기화 종료
                sendSemaphore.Release();
            }
        }



        // 송신
        private async Task S_Data(Socket socket, string Message)
        {
            // 동기화 시작, Waiting Mode 이후 중복 값을 피하기 위해
            await sendSemaphore.WaitAsync();

            try
            {
                byte[] data = Encoding.ASCII.GetBytes(Message);
                await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

                Logger.Log("[SEND] - " + Message);
                //Logger.LogData(Message, "SEND");
                DataInDgv(Message, "SEND");
            }
            finally
            {
                // 동기화 종료. sendSemaphore => 스레드 수 제한
                sendSemaphore.Release();
            }
        }



        // 수신
        private async Task<string> R_Data(Socket socket)
        {
            byte[] buffer = new byte[256];
            int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            string RespData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Logger.Log("[RECV] - " + RespData); // 9999 Waiting Mode Message RECV
            //Logger.LogData(RespData, "RECV"); // Added logging for RECV messages

            return RespData;
        }



        // DataGridview 업데이트
        private void DataInDgv(string data, string type)
        {
            string Mid = data.Substring(4, 4);
            string currentTime = DateTime.Now.ToString("yyyy-MM-d HH:mm:ss");

            Dgv.Invoke((Action)(() =>
            {
                // 비어있는 행을 제거합니다.
                for (int i = Dgv.Rows.Count - 1; i >= 0; i--)
                {
                    DataGridViewRow r = Dgv.Rows[i];
                    if (r.Cells["col_Seq"].Value == null || string.IsNullOrEmpty(r.Cells["col_Seq"].Value.ToString()))
                    {
                        Dgv.Rows.RemoveAt(i);
                    }
                }

                Dgv.Rows.Insert(0);
                DataGridViewRow Row = Dgv.Rows[0];
                Row.Cells["col_Seq"].Value = Sequence++;
                Row.Cells["col_Daytime"].Value = currentTime;
                Row.Cells["col_Mid"].Value = Mid;
                Row.Cells["col_Type"].Value = type;
                Row.Cells["col_Msg"].Value = data;

                if (type == "SEND" && data == "00209999001         ")
                {
                    Row.Cells["col_Msg"].Value = "Waiting For Connection";
                }

                if (type == "RECV")
                {
                    Row.DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
                else if (type == "Debug")
                {
                    Row.DefaultCellStyle.ForeColor = Color.Red;
                }
                else
                {
                    Row.DefaultCellStyle.ForeColor = Color.Black;
                }
            }));
        }



        // Waiting Mode 이후 RecvData 체크 후 송신
        private async Task CheckRecvData(string Mid)
        {
            switch (Mid)
            {
                case "0052":
                    await Response0052();
                    break;
                case "0035":
                    await Response0035();
                    break;
                case "0061":
                    await Response0061();
                    break;
                default:
                    break;
            }
        }



        #region Waiting Mode 이후 값
        private async Task Response0052()
        {
            await S_Data(socket, "00200053001         ");
        }

        private async Task Response0035()
        {
            await S_Data(socket, "00200036001         ");
        }

        private async Task Response0061()
        {
            await S_Data(socket, "00200062001         ");
        }
        #endregion



        // Waiting Mode 토큰
        private void WaitingMode()
        {
            keepAliveCancellationTokenSource = new CancellationTokenSource();
            var token = keepAliveCancellationTokenSource.Token;
            Task.Run(() => KeepConn(token), token);
        }



        // Waiting Mode 송신 유지
        private async Task KeepConn(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        string keepAliveMessage = "00209999001         ";
                        await S_Data(socket, keepAliveMessage);
                    }
                    else
                    {
                        EndOfConn();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Exception Issue: " + ex.Message);

                    EndOfConn();
                    break;
                }
                // Waiting Delay
                await Task.Delay(8000);
            }
        }



        // RecvThread
        private void RecvThread()
        {
            Task.Run(RecvDataLoop);
        }



        // 수신 받은 데이터 확인 및 DataGridView 출력
        private async Task RecvDataLoop()
        {
            while (true)
            {
                try
                {
                    if (socket != null && socket.Connected)
                    {
                        string RespData = await R_Data(socket);

                        if (!string.IsNullOrEmpty(RespData))
                        {
                            DataInDgv(RespData, "RECV");

                            string Mid = RespData.Substring(4, 4);
                            await CheckRecvData(Mid);
                        }
                        else
                        {
                            EndOfConn();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Failed to receive data: " + ex.Message);

                    EndOfConn();
                    break;
                }
            }
        }



        // 연결이 끊어졌을 경우
        private void EndOfConn()
        {
            string Message = "00200004001         ";
            DataInDgv(Message, "Debug");

            Invoke((Action)(() =>
            {
                btn_start.Enabled = true;
                btn_end.Enabled = false;
                txtip.ReadOnly = false;
                txtport.ReadOnly = false;
            }));

            MessageBox.Show("The connection will be terminated");
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
            throw new Exception("Unable to find local IP");
        }



        #region
        private void btn_exit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Exit?", "종료 확인", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void btn_min_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btn_end_Click(object sender, EventArgs e)
        {
            btn_start.Enabled = true;

            try
            {
                keepAliveCancellationTokenSource?.Cancel();

                if (socket != null && socket.Connected)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    Logger.Log("Socket Closed");

                    try
                    { 
                        socket.Close();
                    }
                    catch
                    {
                        btn_start.Enabled = true;
                        btn_end.Enabled = true;
                        txtip.ReadOnly = true;
                        txtport.ReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Exception during termination: " + ex.Message);
            }
            finally
            {
                // btn_start 버튼을 다시 활성화
                btn_start.Enabled = true;
                btn_end.Enabled = false;
                txtip.ReadOnly = false;
                txtport.ReadOnly = false;
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            FormMove = true;
            FormMX = e.X;
            FormMY = e.Y;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            FormMove = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (FormMove == true)
            {
                SetDesktopLocation(MousePosition.X - FormMX, MousePosition.Y - FormMY);
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            FormMove = true;
            FormMX = e.X;
            FormMY = e.Y;
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            FormMove = false;
        }

        private void btn_start_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (FormMove == true)
            {
                SetDesktopLocation(MousePosition.X - FormMX, MousePosition.Y - FormMY);
            }
        }
        #endregion
    }
}
