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
        private int Sequence = 0;
        bool FormMove;
        int FormMX, FormMY; // 폼 X, Y

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
            txtip.ReadOnly = true;
            txtport.ReadOnly = true;

            string IP = txtip.Text;
            int Port = int.Parse(txtport.Text);

            try
            {
                // 소켓 생성
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint EP = new IPEndPoint(ipAddress, Port);

                // 서버에 연결
                await socket.ConnectAsync(EP);

                Logger.Log("서버 연결 " + IP + ":" + Port);

                // 메시지 시퀀스 실행
                if (await SwitchLoop(socket))
                {
                    // 마지막 메시지 이후 Keep-Alive 메시지와 수신(스레드) 시작
                    WaitingMode();
                    StartRECVThread();
                }
            }
            catch (Exception ex)
            {
                string errorMsg = "서버 연결 실패 : " + ex.Message;
                MessageBox.Show(errorMsg, "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Log("예외: " + ex.Message);
            }
        }

        private async Task<bool> SwitchLoop(Socket socket)
        {
            int step = 1;

            while (true)
            {
                bool result;
                switch (step)
                {
                    case 1:
                        result = await FirMsg(socket);
                        break;
                    case 2:
                        result = await SecMsg(socket);
                        break;
                    case 3:
                        result = await ThirMsg(socket);
                        break;
                    case 4:
                        result = await FourMsg(socket);
                        break;
                    default:
                        return false;
                }

                if (!result)
                {
                    return false;
                }

                step++;
                if (step > 4)
                {
                    return true;
                }
            }
        }        

        private Task<bool> FirMsg(Socket socket)
        {
            return SendAndRecv(socket, "00200001001         ", "0002");
        }

        private Task<bool> SecMsg(Socket socket)
        {
            return SendAndRecv(socket, "00200060001         ", "0005");
        }

        private Task<bool> ThirMsg(Socket socket)
        {
            return SendAndRecv(socket, "00200051001         ", "0005");
        }

        private Task<bool> FourMsg(Socket socket)
        {
            return SendAndRecv(socket, "00200034001         ", "0005");
        }

        private async Task<bool> SendAndRecv(Socket socket, string sendMsg, string expectedRecvMsg)
        {
            try
            {
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

                return recvMsg.Contains(expectedRecvMsg);
            }
            catch (Exception ex)
            {
                Logger.Log("SendAndRecv 예외: " + ex.Message);
                return false;
            }
        }

        private async Task S_Data(Socket socket, string Message)
        {
            byte[] data = Encoding.ASCII.GetBytes(Message);
            await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

            Logger.Log("송신: " + Message);
            DataInDgv(Message, "SEND");
        }

        private async Task<string> R_Data(Socket socket)
        {
            byte[] buffer = new byte[256];
            int bytesRead = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            string RespData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

            Logger.Log("수신: " + RespData);            

            return RespData;
        }

        private void DataInDgv(string data, string type)
        {
            string Mid = data.Substring(4, 4);
            string currentTime = DateTime.Now.ToString("HH:mm:ss");

            Dgv.Invoke((Action)(() =>
            {
                // SwitchLoop 중 "0005"가 같은 값으로 들어오기 때문 중복문자 삭제
                //bool alreadyExists = Dgv.Rows.Cast<DataGridViewRow>().Any(row =>
                //    row.Cells["col_Time"].Value?.ToString() == currentTime &&
                //    row.Cells["col_Mid"].Value?.ToString() == Mid &&
                //    row.Cells["col_Type"].Value?.ToString() == type);
                //if (!alreadyExists)

                Dgv.Rows.Insert(0);
                DataGridViewRow Row = Dgv.Rows[0];
                Row.Cells["col_Seq"].Value = Sequence++;
                Row.Cells["col_Time"].Value = currentTime;
                Row.Cells["col_Mid"].Value = Mid;
                Row.Cells["col_Type"].Value = type;
                Row.Cells["col_Msg"].Value = data;
                
                if (type == "RECV")
                {
                    Row.DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
                else if (type == "Debug" || type == "Error")
                {
                    Row.DefaultCellStyle.ForeColor = Color.Red;
                }
                else
                {
                    Row.DefaultCellStyle.ForeColor = Color.Black;
                }
                
            }));
        }

        private Task Check_RecvData(string Mid)
        {
            switch (Mid)
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

        private void WaitingMode()
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
                        string keepAliveMessage = "00209999001         ";
                        await S_Data(socket, keepAliveMessage);
                    }
                    else
                    {
                        Disconn_socket();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("유지 중 예외 발생: " + ex.Message);

                    Disconn_socket();
                    break;
                }
                await Task.Delay(8000);
            }
        }

        private void StartRECVThread()
        {
            Task.Run(R_DataLoop);
        }

        private async Task R_DataLoop()
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
                            await Check_RecvData(Mid);
                        }
                        else
                        {
                            Disconn_socket();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("데이터 수신 중 예외 발생: " + ex.Message);

                    Disconn_socket();
                    break;
                }
            }
        }

        // 소켓 연결 안될 경우 메세지
        private void Disconn_socket()
        {
            string Message = "00200004001         ";
            DataInDgv(Message, "Debug");
        }

        #region GetIP
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
        #endregion

        #region Event
        private void btn_exit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("종료하시겠습니까?", "종료 확인", MessageBoxButtons.YesNo);

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
            try
            {
                keepAliveCancellationTokenSource?.Cancel();

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
