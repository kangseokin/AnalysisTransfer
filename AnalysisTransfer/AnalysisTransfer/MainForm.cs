using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnalysisTransfer
{
    public partial class MainForm : Form
    {

        private Socket m_ServerSocket;
        private List<Socket> m_ClientSocket;
        private byte[] szData;

        public MainForm()
        {
            
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            비동기소켓서버시작();
        }

        private void 비동기소켓서버시작()
        {
            //------------------------------------------------------------------
            //소켓
            //------------------------------------------------------------------
            m_ClientSocket = new List<Socket>();

            m_ServerSocket = new Socket(
            AddressFamily.InterNetwork,
            SocketType.Stream,
            ProtocolType.Tcp);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 7222);
            m_ServerSocket.Bind(ipep);
            m_ServerSocket.Listen(20);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed
            += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            m_ServerSocket.AcceptAsync(args);
            //-------------------------------------------------------------------
        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = e.AcceptSocket;
            m_ClientSocket.Add(ClientSocket);

            if (m_ClientSocket != null)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                szData = new byte[1024];
                args.SetBuffer(szData, 0, 1024);
                args.UserToken = m_ClientSocket;
                args.Completed
                += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                ClientSocket.ReceiveAsync(args);
            }
            e.AcceptSocket = null;
            m_ServerSocket.AcceptAsync(e);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {
            Socket ClientSocket = (Socket)sender;
            if (ClientSocket.Connected && e.BytesTransferred > 0)
            {
                byte[] szData = e.Buffer;    // 데이터 수신

                //string sData = Encoding.Unicode.GetString(szData);

                //MessageBox.Show(sData);
                //sData = Encoding.UTF8.GetString(szData);
                //MessageBox.Show(sData);
                string sData = Encoding.ASCII.GetString(szData);
                //MessageBox.Show(sData);

                //richTextBox4.Text = sData;
                //SetText(sData);

                전송받은데이터에서항목별로데이터추출(sData);


                string Test = sData.Replace("\0", "").Trim();
                //SetText(Test);
                for (int i = 0; i < szData.Length; i++)
                {
                    szData[i] = 0;
                }
                e.SetBuffer(szData, 0, 1024);
                ClientSocket.ReceiveAsync(e);
            }
            else
            {
                ClientSocket.Disconnect(false);
                //ClientSocket.Dispose();
                m_ClientSocket.Remove(ClientSocket);
            }
        }


        // 전송 받은 데이터에서 성분별로 데이터를 추출해서 저장하는 변수
        public struct AnalysisDataStruct
        {
            public string GroupName; // 분석시 사용했던 그룹이름

            public string HeatNO;  // Heat NO

            public Double FE_Data;    // FE  분석 성분 값
            public Double C_Data;     // C   분석 성분 값
            public Double Si_Data;    // Si  분석 성분 값
            public Double Mn_Data;    // Mn  분석 성분 값
            public Double P_Data;     // P   분석 성분 값
            public Double S_Data;     // S   분석 성분 값
            public Double Cu_Data;    // Cu  분석 성분 값
            public Double Cr_Data;    // Cr  분석 성분 값
            public Double Ni_Data;    // Ni  분석 성분 값
            public Double V_Data;     // V   분석 성분 값
            public Double Nb_Data;    // Nb  분석 성분 값
            public Double Mo_Data;    // Mo  분석 성분 값
            public Double Ti_Data;    // Ti  분석 성분 값
            public Double Al_Data;    // Al  분석 성분 값
            public Double Sn_Data;    // Sn  분석 성분 값
            public Double W_Data;     // W   분석 성분 값
            public Double As_Data;    // As  분석 성분 값
            public Double Sb_Data;    // Sb  분석 성분 값
            public Double Pb_Data;    // Pb  분석 성분 값
            public Double N_Data;     // N   분석 성분 값
            public Double Ca_Data;    // Ca   분석 성분 값
            public Double Zn_Data;    // Zn  분석 성분 값
            public Double INT_Data;   // Int 분석 성분 값
            public Double CE_Data;    // CE  분석 성분 값

            public string JLT;        // 분석 구분자 (J:용락, L:래들, T:턴디쉬)

            public DateTime TransTimeData; // 전송받은 시간
        }

        AnalysisDataStruct AnalysisData;

        public int 전송받은데이터에서항목별로데이터추출(string TransReadData)
        {
            //"HK      [086084T                            FE   97.718C    .48970SI   .01986MN   .04615P    .02845S    .05308CU   .31849CR   .09907V    .00000NI   .07710NB   .00778MO   .03682TI   .00000AL   .91860SN   .01344W    .12702           AS   .02638SB   .01511PB   .00000ZN   .00478N    .00000INT  7.6728CE   .55094                                                                                        HK      [086084T                            FE   97.718C    .48970SI   .01986MN   .04615P    .02845S    .05308CU   .31849CR   .09907V    .00000NI   .07710NB   .00778MO   .03682TI   .00000AL   .91860SN   .01344W    .12702           AS   .02638SB   .01511PB   .00000ZN   .00478N    .00000INT  7.6728CE   .55094                                                                                        ";

            int i = 0;
            int 원소의총갯수 = 24; // 원소의 총 갯수  --> 나중에 환결설정 파일에 저장.

            // 분석 성분 측정치인지 검사
            if (TransReadData[i].Equals(''))
            {
                AnalysisData.GroupName = TransReadData.Substring(1, TransReadData.IndexOf('[') - 1).Trim(); //그룹을 구한다.

                AnalysisData.HeatNO = TransReadData.Substring(TransReadData.IndexOf('[') + 1, (TransReadData.IndexOf("Fe") - 1) - (TransReadData.IndexOf('[') + 1)).Trim(); //HeatNO를 구한다.

                AnalysisData.HeatNO = AnalysisData.HeatNO.Replace(" ", "");  //HeatNO에 공백을 완전히 지운다.

                //MessageBox.Show("."+AnalysisData.HeatNO+".");

                // 검사 구분을 추출.
                switch (AnalysisData.HeatNO[AnalysisData.HeatNO.Length - 1])
                {
                    case 'J':
                    case 'j':
                        AnalysisData.JLT = "용락";
                        break;
                    case 'L':
                    case 'l':
                        AnalysisData.JLT = "L/D";
                        break;
                    case 'T':
                    case 't':
                        AnalysisData.JLT = "T/D";
                        break;
                    default:
                        // 전송하는 데이터가 아니라고 판단하여 전송 취소.
                        MessageBox.Show("검사구분 문자가 없어서 취소");
                        return 101;
                }

                // HeatNO를 추출.
                AnalysisData.HeatNO = AnalysisData.HeatNO.Substring(0, AnalysisData.HeatNO.Length - 1);

                MessageBox.Show("--" + AnalysisData.HeatNO + " -- " + AnalysisData.JLT + "--");


                int EleIndex = TransReadData.IndexOf("Fe");
                //for (int ii = TransReadData.IndexOf("Fe"); ii < 원소의총갯수 * 12 - 12; ii += 5)
                int sumii = 0;    
                for (int ii = 0 ; ii < 원소의총갯수; ii++)
                {
                    
                    항목별로데이터저장(TransReadData.Substring(EleIndex+ sumii, 5).Trim(), Convert.ToDouble(TransReadData.Substring(EleIndex + sumii + 5, 7).Trim()));

                    sumii += 12;

                    if ((EleIndex + sumii + 5+ 7) < TransReadData.Length) 
                    {
                        break;
                    }
                }
            }
            else
            {
                // 분석 성분 측정치의 시작을 알리는 문자열을 찾지 못했다. 즉, 분석 성분이 전송된것이 아니다.
                MessageBox.Show("분석 성분 측정치의 시작을 알리는 문자열을 찾지 못했다. 즉, 분석 성분이 전송된것이 아니다.");
                return 10;
            }

            return 0;
        }

        public bool 항목별로데이터저장(string EleName, double Value)
        {
            //MessageBox.Show(EleName.ToUpper()+" :: " + Value.ToString());

            switch (EleName.ToUpper())
            {
                case "FE":
                    AnalysisData.FE_Data = Value;

                    MessageBox.Show("FE : " + AnalysisData.FE_Data.ToString());

                    break;

                case "C":
                    AnalysisData.C_Data = Value;

                    MessageBox.Show("C : " + AnalysisData.C_Data.ToString());

                    break;

                case "SI":

                    AnalysisData.Si_Data = Value;

                    MessageBox.Show("Si : " + AnalysisData.Si_Data.ToString());

                    break;

                case "MN":
                    AnalysisData.Mn_Data = Value;

                    MessageBox.Show("MN : " + AnalysisData.Mn_Data.ToString());

                    break;

                case "P":
                    AnalysisData.P_Data = Value;

                    MessageBox.Show("P : " + AnalysisData.P_Data.ToString());

                    break;

                case "S":
                    AnalysisData.S_Data = Value;

                    MessageBox.Show("S : " + AnalysisData.S_Data.ToString());

                    break;

                case "CU":
                    AnalysisData.Cu_Data = Value;

                    MessageBox.Show("CU : " + AnalysisData.Cu_Data.ToString());

                    break;

                case "CR":
                    AnalysisData.Cr_Data = Value;

                    MessageBox.Show("CR : " + AnalysisData.Cr_Data.ToString());

                    break;

                case "NI":
                    AnalysisData.Ni_Data = Value;

                    MessageBox.Show("NI : " + AnalysisData.Ni_Data.ToString());

                    break;

                case "V":
                    AnalysisData.V_Data = Value;

                    MessageBox.Show("V : " + AnalysisData.V_Data.ToString());

                    break;

                case "NB":
                    AnalysisData.Nb_Data = Value;

                    MessageBox.Show("NB : " + AnalysisData.Nb_Data.ToString());

                    break;

                case "MO":
                    AnalysisData.Mo_Data = Value;

                    MessageBox.Show("MO : " + AnalysisData.Mo_Data.ToString());

                    break;

                case "TI":
                    AnalysisData.Ti_Data = Value;

                    MessageBox.Show("TI : " + AnalysisData.Ti_Data.ToString());

                    break;

                case "AL":
                    AnalysisData.Al_Data = Value;

                    MessageBox.Show("AL : " + AnalysisData.Al_Data.ToString());

                    break;

                case "SN":
                    AnalysisData.Sn_Data = Value;

                    MessageBox.Show("SN : " + AnalysisData.Sn_Data.ToString());

                    break;

                case "W":
                    AnalysisData.W_Data = Value;

                    MessageBox.Show("W : " + AnalysisData.W_Data.ToString());

                    break;

                case "AS":
                    AnalysisData.As_Data = Value;

                    MessageBox.Show("AS : " + AnalysisData.As_Data.ToString());

                    break;

                case "SB":
                    AnalysisData.Sb_Data = Value;

                    MessageBox.Show("SB : " + AnalysisData.Sb_Data.ToString());

                    break;

                case "PB":
                    AnalysisData.Pb_Data = Value;

                    MessageBox.Show("PB : " + AnalysisData.Pb_Data.ToString());

                    break;

                case "ZN":
                    AnalysisData.Zn_Data = Value;

                    MessageBox.Show("ZN : " + AnalysisData.Zn_Data.ToString());

                    break;

                case "N":
                    AnalysisData.N_Data = Value;

                    MessageBox.Show("N : " + AnalysisData.N_Data.ToString());

                    break;

                case "Ca":
                    AnalysisData.Ca_Data = Value;

                    MessageBox.Show("Ca : " + AnalysisData.N_Data.ToString());

                    break;

                case "INT":
                    AnalysisData.INT_Data = Value;

                    MessageBox.Show("INT : " + AnalysisData.INT_Data.ToString());

                    break;

                case "CE":
                    AnalysisData.CE_Data = Value;
                    // AnalysisData.Ni_Data = AnalysisData.CE_Data;
                    MessageBox.Show("CE : " + AnalysisData.CE_Data.ToString());
                    //MessageBox.Show("ni : " + AnalysisData.Ni_Data.ToString());
                    break;


                    //default:
                    //return false;
            }

            return true;
        }

        
    }
}
