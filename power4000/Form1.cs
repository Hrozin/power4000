using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
//using System.Runtime.Remoting.Messaging;
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
            // 서버와 연결 이후 txt_ip, txt_port 수정 불가
            txtip.ReadOnly = true;
            txtport.ReadOnly = true;

            string IP = txtip.Text;
            int Port = int.Parse(txtport.Text);

            try
            {
                // 소켓생성
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, Port);

                // Connect to the remote endpoint.
                await socket.ConnectAsync(remoteEP);

                Logger.Log("서버연결 " + IP + ":" + Port);

                // 첫번째 메세지 시퀀스 실행
                if (await FirMsg(socket))
                {
                    if (await SecMsg(socket))
                    {
                        if (await ThirMsg(socket))
                        {
                            if (await FourMsg(socket))
                            {
                                // 마지막 메세지 후 Keep-Alive와 수신(스레드)
                                WaitingMode();
                                StartRECVThread();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Log("예외: " + ex.Message);                
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

        private async Task<bool> SendAndRecv(Socket socket, string Message, string expectedResponse)
        {
            // 서버로 데이터 송신
            await S_Data(socket, Message);

            // 서버로 데이터 수신
            string RespData = await R_Data(socket);

            // 수신한 데이터에서 col_Mid 값
            string Mid = RespData.Substring(4, 4);

            // Dgv에 수신한 데이터 표시
            DataInDgv(RespData, "RECV");

            // 응답이 예상 값과 일치하는지 확인
            if (Mid == expectedResponse)
            {
                return true;
            }
            else
            {
                string errorMsg = $"Unexpected response received: {Mid}";
                Logger.Log(errorMsg);
                //MessageBox.Show(errorMsg);
                return false;
            }
        }

        private async Task S_Data(Socket socket, string Message)
        {
            byte[] data = Encoding.ASCII.GetBytes(Message);
            await socket.SendAsync(new ArraySegment<byte>(data), SocketFlags.None);

            Logger.Log("송신: " + Message);

            // Dgv에 송신된 데이터 표시하기
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
                // 값 중복 방지를 위해 데이터 확인
                bool alreadyExists = Dgv.Rows.Cast<DataGridViewRow>().Any(row =>
                    row.Cells["col_Time"].Value?.ToString() == currentTime &&
                    row.Cells["col_Mid"].Value?.ToString() == Mid &&
                    row.Cells["col_Type"].Value?.ToString() == type);

                // 새 행을 맨 위(인덱스 0)에 추가
                Dgv.Rows.Insert(0);
                DataGridViewRow newRow = Dgv.Rows[0];
                newRow.Cells["col_Seq"].Value = Sequence++;
                newRow.Cells["col_Time"].Value = currentTime;
                newRow.Cells["col_Mid"].Value = Mid;
                newRow.Cells["col_Type"].Value = type;
                newRow.Cells["col_Msg"].Value = data;

                if (type == "RECV")
                {
                    newRow.DefaultCellStyle.ForeColor = Color.DarkBlue;
                }
                else if (type == "Debug" || type == "Error")
                {
                    newRow.DefaultCellStyle.ForeColor = Color.Red;
                }
                else
                {
                    newRow.DefaultCellStyle.ForeColor = Color.Black;
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
            // 응답 메세지 전송
            await S_Data(socket, "00200053001         ");                        
        }

        private async Task Response0035()
        {
            // 0035'에 대한 응답 메시지를 전송합니다.
            await S_Data(socket, "00200036001         ");
        }

        private async Task Response0061()
        {
            // 0061'에 대한 응답 메시지를 전송합니다.
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
                        // Keep-Alive 메세지 송신
                        string keepAliveMessage = "00209999001         ";
                        await S_Data(socket, keepAliveMessage);                        
                    }
                    else
                    {
                        HandleSocketDisconnection();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("유지 중 예외 발생: " + ex.Message);
                    //MessageBox.Show("유지 중 예외 발생: " + ex.Message);
                    HandleSocketDisconnection();
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
                            // Dgv에 수신된 데이터 표시
                            DataInDgv(RespData, "RECV");

                            // 수신된 데이터에서 col_Mid 값을 추출
                            string Mid = RespData.Substring(4, 4);

                            // 수신 데이터 확인
                            await Check_RecvData(Mid);
                        }
                        else
                        {
                            HandleSocketDisconnection();
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("데이터 수신 중 예외 발생: " + ex.Message);
                    //MessageBox.Show("데이터 수신 중 예외 발생: " + ex.Message);
                    HandleSocketDisconnection();
                    break;
                }
            }
        }

        private void HandleSocketDisconnection()
        {
            string Message = "00200004001         ";
            DataInDgv(Message, "Debug");
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
                // Keep-Alive 정지
                if (keepAliveCancellationTokenSource != null)
                {
                    keepAliveCancellationTokenSource.Cancel();
                }

                // 소켓 연결 닫힘
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
                //MessageBox.Show("종료 중 예외 발생: " + ex.Message);
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
            if(e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }        

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (FormMove == true)
            {
                SetDesktopLocation(MousePosition.X - FormMX, MousePosition.Y - FormMY);
            }
        }
    }
}
