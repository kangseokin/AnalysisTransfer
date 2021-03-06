﻿using SerialCommLib;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;//DispatcherTimer


namespace QMAnalysisTransfer
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        double orginalWidth, originalHeight;
        ScaleTransform scale = new ScaleTransform();
        //-----------------------------------------------
        // Server 정보.
        const string Server_IP = "HK";
        const string Server_ID = "sa";
        const string Server_PW = "samanager";
        const string Server_DB1 = "ERP";
        const string Server_DB2 = "ERP10HKJ";

        //string ConnectionStringERP = "Server=" + Server_IP + ";uid=" + Server_ID + ";password=" + Server_PW + ";database=" + Server_DB1 + ";Connect Timeout=3000";
        string ConnectionStringERP10GG0 = "Server=" + Server_IP + ";uid=" + Server_ID + ";password=" + Server_PW + ";database=" + Server_DB2 + ";Connect Timeout=3000";
        string ConnectionStringQCMANAGER_Data = "Server= HK ;uid= hkqcuser;password= hkqcuser;database=hkqcdb ;Connect Timeout=3000";

        SqlConnection ERPDataBase_conn;
        SqlConnection QCMANAGER_Data_conn;

        string YEONJU_LotNo;
        //string YEONJU_PreLotNo;
        int YEONJU_Count;

        //int YEONJU_PreCount;

        string HDR_LotNo;
        //string HDR_PreLotNo;
        int HDR_Count;
        //int HDR_PreCount;

        string HCNM_Data;
        string YKWGI_Data;
        string GJGB_Data;
        
        //----------------------------------------------------
        
        DispatcherTimer timer = new DispatcherTimer();    //객체생성

        //--------------------------------------------------------

        //비동기 소켓
        private Socket m_ServerSocket;
        private List<Socket> m_ClientSocket;
        private byte[] szData;
        //-------------------------------------------------
        //시리얼 포트
        SerialComm serial = new SerialComm();

        string SerialPortName;
        int SerialBaudRate;
        int SerialDataBits;
        StopBits SerialStopBits;
        Parity SerialParity;
        Handshake SerialHandshake;
        //----------------------------------------------------------------

        List<TransferData> transferdata = new List<TransferData>();

        //---------------------------------------------------------------------

        List<ElectricfurnaceWorkEndData> electricfurnaceworkenddata = new List<ElectricfurnaceWorkEndData>();

        //------------------------------------------

        bool TransOKflag = false;
        bool ElectricfurnaceWorkEndflag = false;

        string FrontHeatNo="0";
        string FrontOnToTap="0";
        string FrontTapToTap="0";
        string FrontWatt="0";


        //----------------------------------------
        Thread ERPDataBase_GetYEONJUCount_Thread;
        Thread ERPDataBase_GetHDRCount_Thread;
        Thread QCMANAGERData_GetLotData_Thread;
        bool ERPDataBase_GetYEONJUCount_Thread_flag = true;
        bool ERPDataBase_GetHDRCount_Thread_flag = true;
        bool QCMANAGERData_GetLotData_Thread_flag = true;
        //-------------------

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainWindow_Loaded);
        }

        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            orginalWidth = this.Width;
            originalHeight = this.Height;

            if (this.WindowState == WindowState.Maximized)
            {
                ChangeSize(this.ActualWidth, this.ActualHeight);
            }

            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged);
            //-----------------------------------
            //timer.Interval = TimeSpan.FromMilliseconds(1000);    //시간간격 설정
            //timer.Tick += new EventHandler(timer_Tick);          //이벤트 추가
            //timer.Start();                                       //타이머 시작. 종료는 timer.Stop(); 으로 한다

            //------------------------------
            비동기소켓서버시작();
            
            //SerialCommINI();
            SerialPortName = "COM3";
            SerialBaudRate = 9600;
            SerialDataBits = 8;
            SerialParity = Parity.None;
            SerialStopBits = StopBits.One;
            SerialHandshake = Handshake.None;

            //MessageBox.Show("[\0\0\0\0]");
            serial.DataReceivedHandler = DataReceivedHandler;
            serial.DisconnectedHandler = DisconnectedHandler;

            serial.OpenComm(SerialPortName, SerialBaudRate, SerialDataBits, SerialStopBits, SerialParity, SerialHandshake);
            
            dgTransferData.ItemsSource = transferdata;

            dgElectricfurnaceWorkEndData.ItemsSource = electricfurnaceworkenddata;
            
            //------------------------------
            
            ERPDataBase_GetYEONJUCount_Thread = new Thread(() => {
                while (ERPDataBase_GetYEONJUCount_Thread_flag)
                {
                    Thread.Sleep(900);

                    using (SqlConnection con = new SqlConnection(ConnectionStringERP10GG0))
                    {
                        string query = "select LOT_NO,WKRSLT_COUNT from V_MES_WKRSLT_CNT where PMEQP_NO = 'A100-10'";

                        using (SqlCommand com = new SqlCommand(query, con))
                        {
                            try
                            {
                                con.Open();

                                using (SqlDataReader reader = com.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Read();

                                        YEONJU_LotNo = reader.GetString(0);

                                        YEONJU_Count = (int)reader.GetDecimal(1);
                                    }
                                }

                                //tbBTCount.Text = YEONJU_Count.ToString();
                                //this.Invoke(new Action(delegate () // this == Form 이다. Form이 아닌 컨트롤의 Invoke를 직접호출해도 무방하다. 
                                //{ //Invoke를 통해 lbl_Result 컨트롤에 결과값을 업데이트한다. 
                                //    lbl_Result.Text = result.ToString(); 6
                                //}));
                                tbBTCount.Dispatcher.Invoke(new Action(delegate ()
                               {
                                   tbBTCount.Text = YEONJU_Count.ToString();
                               }));

                            }
                            catch (Exception ex)
                            {
                                // Anything you want to do with ex
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                }
            });




            ERPDataBase_GetYEONJUCount_Thread.IsBackground = true;

            ERPDataBase_GetYEONJUCount_Thread.Start();

            //----------------------------
            ERPDataBase_GetHDRCount_Thread = new Thread(() => {
                while (ERPDataBase_GetHDRCount_Thread_flag)
                {
                    Thread.Sleep(900);

                    using (SqlConnection con = new SqlConnection(ConnectionStringERP10GG0))
                    {
                        string query = "select LOT_NO,WKRSLT_COUNT from V_MES_WKRSLT_CNT where PMEQP_NO = 'B100-10'";

                        using (SqlCommand com = new SqlCommand(query, con))
                        {
                            try
                            {
                                con.Open();

                                using (SqlDataReader reader = com.ExecuteReader())
                                {
                                    if (reader.HasRows)
                                    {
                                        reader.Read();

                                        HDR_LotNo = reader.GetString(0);

                                        HDR_Count = (int)reader.GetDecimal(1);
                                    }

                                    tbBTCount.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbHDRCount.Text = HDR_Count.ToString();
                                    }));

                                }
                            }
                            catch (Exception ex)
                            {
                                // Anything you want to do with ex
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                }
            });

            ERPDataBase_GetHDRCount_Thread.IsBackground = true;

            ERPDataBase_GetHDRCount_Thread.Start();
            //-----------------------------
            ////QCMANAGERData_GetLotData(YEONJU_LotNo, out GJGB_Data, out HCNM_Data, out YKWGI_Data); //생산 정보 가져오기
            QCMANAGERData_GetLotData_Thread = new Thread(() => {
                while (QCMANAGERData_GetLotData_Thread_flag)
                {
                    Thread.Sleep(900);

                    using (SqlConnection con = new SqlConnection(ConnectionStringQCMANAGER_Data))
                    {
                        //string commandString = "select HCNM,YKWGI,GJGB from dbo.TQCQ1100 where (HeatNo = '" + YEONJULotNo + "') and  (GSGB='T');"; // 여기에 T/D를 구분하는 쿼리를 넣어야 한다.
                        string query = "select HCNM,YKWGI,GJGB from dbo.TQCQ1100 where (HeatNo = '" + YEONJU_LotNo + "') and  (GSGB='T');";

                        using (SqlCommand com = new SqlCommand(query, con))
                        {
                            try
                            {
                                con.Open();

                                using (SqlDataReader reader = com.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        HCNM_Data = reader.GetString(0);

                                        YKWGI_Data = reader.GetDecimal(1).ToString();

                                        GJGB_Data = reader.GetString(2);

                                        tbBTCount.Dispatcher.Invoke(new Action(delegate ()
                                        {
                                            tbBTHeatNo.Text = YEONJU_LotNo;
                                            tbBTHCNM.Text = HCNM_Data;
                                            tbBTYKWGI.Text = YKWGI_Data;
                                            tbBTGJGB.Text = GJGB_Data;

                                            //Debug.WriteLine(YEONJU_LotNo);
                                        }));


                                        reader.Close();
                                    }
                                }


                            }
                            catch (Exception ex)
                            {
                                // Anything you want to do with ex
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                }
            });

            QCMANAGERData_GetLotData_Thread.IsBackground = true;

            QCMANAGERData_GetLotData_Thread.Start();
            //-----------------------------

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //ERPDataBase_GetYEONJUCount_Thread.
            //timer.Stop();
            ERPDataBase_GetYEONJUCount_Thread_flag = false;
            ERPDataBase_GetHDRCount_Thread_flag = false;
            QCMANAGERData_GetLotData_Thread_flag = false;


            serial.CloseComm();

            DeleteListAll();
            m_ServerSocket = null;
            m_ClientSocket = null;

            
        }

        private void DeleteListAll()
        {
            this.Dispatcher.Invoke(new Action(delegate ()
            {
                //m_ClientSocket.Clear();
                //m_ClientSocket.RemoveAll(DeleteList);
                //m_ClientSocket.Clear();
                //m_ServerSocket.
                //m_ServerSocket.Close();
                m_ServerSocket = null;
                m_ClientSocket = null;
            }));
        }

        private static bool DeleteList(Socket chi)
        {
            if (chi != null)
                return true;
            return false;
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeSize(e.NewSize.Width, e.NewSize.Height);
        }

        private void ChangeSize(double width, double height)
        {
            scale.ScaleX = width / orginalWidth;
            scale.ScaleY = height / originalHeight;

            FrameworkElement rootElement = this.Content as FrameworkElement;

            rootElement.LayoutTransform = scale;
        }

        #region QM 서버에서데이터 가져오기

        // ERP 연주 데이터.
        public void ERPDataBase_GetYEONJUCount(out string YEONJU_LotNo, out int YEONJU_Count)
        {
            YEONJU_LotNo = "";

            YEONJU_Count = 0;

            try
            {
                string commandString = "select LOT_NO,WKRSLT_COUNT from V_MES_WKRSLT_CNT where PMEQP_NO = 'A100-10'";

                ERPDataBase_conn = new SqlConnection(ConnectionStringERP10GG0);

                SqlCommand myCommand = new SqlCommand(commandString, ERPDataBase_conn);

                ERPDataBase_conn.Open();

                using (SqlDataReader myReader = myCommand.ExecuteReader())
                {
                    if (myReader.HasRows)
                    {
                        myReader.Read();

                        YEONJU_LotNo = myReader.GetString(0);

                        YEONJU_Count = (int)myReader.GetDecimal(1);
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                ERPDataBase_conn.Close();
                Thread.Sleep(1000);
            }
        }

        // HDR 카운트.
        public void ERPDataBase_GetHDRCount(out string HDR_LotNo, out int HDR_Count)
        {
            HDR_LotNo = "";

            HDR_Count = 0;
            try
            {
                string commandString = "select LOT_NO,WKRSLT_COUNT from V_MES_WKRSLT_CNT where PMEQP_NO = 'B100-10'";

                ERPDataBase_conn = new SqlConnection(ConnectionStringERP10GG0);

                SqlCommand myCommand = new SqlCommand(commandString, ERPDataBase_conn);

                ERPDataBase_conn.Open();

                using (SqlDataReader myReader = myCommand.ExecuteReader())
                {
                    if (myReader.HasRows)
                    {
                        myReader.Read();

                        HDR_LotNo = myReader.GetString(0);

                        HDR_Count = (int)myReader.GetDecimal(1);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                ERPDataBase_conn.Close();
                Thread.Sleep(1000);
            }
        }


        // QCMANAGER의 데이터중에 호칭,길이, 강종을 얻어오는 부분.
        public void QCMANAGERData_GetLotData(string YEONJULotNo, out string 강종, out string 호칭, out string 길이)
        {
            강종 = "";
            호칭 = "";
            길이 = "";
            try
            {
                if (YEONJULotNo.Length > 0)
                {
                    string commandString = "select HCNM,YKWGI,GJGB from dbo.TQCQ1100 where (HeatNo = '" + YEONJULotNo + "') and  (GSGB='T');"; // 여기에 T/D를 구분하는 쿼리를 넣어야 한다.

                    QCMANAGER_Data_conn = new SqlConnection(ConnectionStringQCMANAGER_Data);

                    SqlCommand myCommand = new SqlCommand(commandString, QCMANAGER_Data_conn);

                    QCMANAGER_Data_conn.Open();

                    using (SqlDataReader myReader = myCommand.ExecuteReader())
                    {
                        if (myReader.HasRows)
                        {
                            if (myReader.Read())
                            {
                                호칭 = myReader.GetString(0);

                                길이 = myReader.GetDecimal(1).ToString();

                                강종 = myReader.GetString(2);

                                myReader.Close();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                ERPDataBase_conn.Close();
            }

        }

        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                //카운트 데이터 가져오기
                ERPDataBase_GetYEONJUCount(out YEONJU_LotNo, out YEONJU_Count); //연주 절단 카운트 가져오기

                //ERPDataBase_GetHDRCount(out HDR_LotNo, out HDR_Count); //압연 HDR 장입 카운트 가져오기

                //QCMANAGERData_GetLotData(YEONJU_LotNo, out GJGB_Data, out HCNM_Data, out YKWGI_Data); //생산 정보 가져오기

                // 카운트 표시
                tbBTCount.Text = YEONJU_Count.ToString();

                tbHDRCount.Text = HDR_Count.ToString();

                tbBTHeatNo.Text = YEONJU_LotNo;
                tbBTHCNM.Text = HCNM_Data;
                tbBTYKWGI.Text = YKWGI_Data;
                tbBTGJGB.Text = GJGB_Data;
            } catch
            {
                //Debug.WriteLine("sdfsdf");
            }
            
        }

        #endregion

        #region 네트워크 통신

        private void 비동기소켓서버시작()
        {
            //------------------------------------------------------------------
            //소켓
            //------------------------------------------------------------------
            m_ClientSocket = new List<Socket>();

            m_ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 7222);

            m_ServerSocket.Bind(ipep);

            m_ServerSocket.Listen(20);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Accept_Completed);
            m_ServerSocket.AcceptAsync(args);

            //-------------------------------------------------------------------

        }

        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            
            

            if (m_ClientSocket != null)
            {
                Socket ClientSocket = e.AcceptSocket;

                m_ClientSocket.Add(ClientSocket);

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                szData = new byte[1024];
                args.SetBuffer(szData, 0, 1024);
                args.UserToken = m_ClientSocket;
                args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);
                ClientSocket.ReceiveAsync(args);
            }
            e.AcceptSocket = null;
            try
            {
                if (m_ServerSocket != null)
                {
                    m_ServerSocket.AcceptAsync(e);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Accept_Completed의 m_ServerSocket.AcceptAsync(e);에서 에러 발생, " + ex.ToString());
            }
            //Debug.WriteLine(m_ServerSocket.AcceptAsync(e) + "이 부분 실행");
            //m_ServerSocket.EndAccept()

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

            public float FE_Data;    // FE  분석 성분 값
            public float C_Data;     // C   분석 성분 값
            public float Si_Data;    // Si  분석 성분 값
            public float Mn_Data;    // Mn  분석 성분 값
            public float P_Data;     // P   분석 성분 값
            public float S_Data;     // S   분석 성분 값
            public float Cu_Data;    // Cu  분석 성분 값
            public float Cr_Data;    // Cr  분석 성분 값
            public float Ni_Data;    // Ni  분석 성분 값
            public float V_Data;     // V   분석 성분 값
            public float Nb_Data;    // Nb  분석 성분 값
            public float Mo_Data;    // Mo  분석 성분 값
            public float Ti_Data;    // Ti  분석 성분 값
            public float Al_Data;    // Al  분석 성분 값
            public float Sn_Data;    // Sn  분석 성분 값
            public float W_Data;     // W   분석 성분 값
            public float As_Data;    // As  분석 성분 값
            public float Sb_Data;    // Sb  분석 성분 값
            public float Pb_Data;    // Pb  분석 성분 값
            public float N_Data;     // N   분석 성분 값
            public float Ca_Data;    // Ca   분석 성분 값
            public float Zn_Data;    // Zn  분석 성분 값
            public float INT_Data;   // Int 분석 성분 값
            public float CE_Data;    // CE  분석 성분 값

            public string JLT;        // 분석 구분자 (J:용락, L:래들, T:턴디쉬)

            public DateTime TransTimeData; // 전송받은 시간
        }

        AnalysisDataStruct AnalysisData;
        //전송받은데이터에서항목별로데이터추출("HK      [086084T                            FE   97.718C    .48970SI   .01986MN   .04615P    .02845S    .05308CU   .31849CR   .09907V    .00000NI   .07710NB   .00778MO   .03682TI   .00000AL   .91860SN   .01344W    .12702AS   .02638SB   .01511PB   .00000ZN   .00478N    .00000INT  7.6728CE   .55094                                                                                        HK      [086084T                            FE   97.718C    .48970SI   .01986MN   .04615P    .02845S    .05308CU   .31849CR   .09907V    .00000NI   .07710NB   .00778MO   .03682TI   .00000AL   .91860SN   .01344W    .12702           AS   .02638SB   .01511PB   .00000ZN   .00478N    .00000INT  7.6728CE   .55094                                                                                        ");
        public int 전송받은데이터에서항목별로데이터추출(string TransReadData)
        {
            //"HK      [086084T                            FE   97.718C    .48970SI   .01986MN   .04615P    .02845S    .05308CU   .31849CR   .09907V    .00000NI   .07710NB   .00778MO   .03682TI   .00000AL   .91860SN   .01344W    .12702           AS   .02638SB   .01511PB   .00000ZN   .00478N    .00000INT  7.6728CE   .55094                                                                                        HK      [086084T                            FE   97.718C    .48970SI   .01986MN   .04615P    .02845S    .05308CU   .31849CR   .09907V    .00000NI   .07710NB   .00778MO   .03682TI   .00000AL   .91860SN   .01344W    .12702           AS   .02638SB   .01511PB   .00000ZN   .00478N    .00000INT  7.6728CE   .55094                                                                                        ";

            int i = 0;
            int 원소의총갯수 = 24; // 원소의 총 갯수  --> 나중에 환결설정 파일에 저장.

            string UpperTransReadData = TransReadData.ToUpper(); //문자열을 대문자로 변환


            // 분석 성분 측정치인지 검사
            if (UpperTransReadData[i].Equals(''))
            {
                AnalysisData.GroupName = UpperTransReadData.Substring(1, UpperTransReadData.IndexOf('[') - 1).Trim(); //그룹을 구한다.

                AnalysisData.HeatNO = UpperTransReadData.Substring(UpperTransReadData.IndexOf('[') + 1, (UpperTransReadData.IndexOf("FE") - 1) - (UpperTransReadData.IndexOf('[') + 1)).Trim(); //HeatNO를 구한다.

                AnalysisData.HeatNO = AnalysisData.HeatNO.Replace(" ", "");  //HeatNO에 공백을 완전히 지운다.

                //MessageBox.Show("."+AnalysisData.HeatNO+".");

                AnalysisData.TransTimeData = DateTime.Now;

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

                //MessageBox.Show("--" + AnalysisData.HeatNO + " -- " + AnalysisData.JLT + "--");


                int EleIndex = UpperTransReadData.IndexOf("FE");
                //for (int ii = TransReadData.IndexOf("Fe"); ii < 원소의총갯수 * 12 - 12; ii += 5)
                int sumii = 0;
                for (int ii = 0; ii < 원소의총갯수; ii++)
                {
                    string aaa = UpperTransReadData.Substring(EleIndex + sumii, 5).Trim(); //"."
                    if (aaa == "") break;
                    float bbb = Convert.ToSingle(UpperTransReadData.Substring(EleIndex + sumii + 5, 6).Trim()); //"97.718"

                    항목별로데이터저장(aaa, bbb);
                    //항목별로데이터저장(TransReadData.Substring(EleIndex + sumii, 5).Trim(), Convert.ToSingle(TransReadData.Substring(EleIndex + sumii + 5, 6).Trim()));

                    sumii += 12;

                    if ((EleIndex + sumii + 5 + 7) > UpperTransReadData.Length)
                    {
                        break;
                    }
                }
                //------------------------------

                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    tbJLT.Text = AnalysisData.JLT;

                    transferdata.Add(new TransferData(AnalysisData.TransTimeData,
                                                      Convert.ToInt32(AnalysisData.HeatNO),
                                                      AnalysisData.JLT,
                                                      (int)Math.Round(AnalysisData.C_Data * 100),
                                                      (int)Math.Round(AnalysisData.Si_Data * 100),
                                                      (int)Math.Round(AnalysisData.Mn_Data * 100),
                                                      (int)Math.Round(AnalysisData.P_Data * 1000),
                                                      (int)Math.Round(AnalysisData.S_Data * 1000),
                                                      (int)Math.Round(AnalysisData.Cu_Data * 100),
                                                      (int)Math.Round(AnalysisData.Cr_Data * 100),
                                                      (int)Math.Round(AnalysisData.CE_Data * 100),
                                                      (int)Math.Round(AnalysisData.V_Data * 1000)));
                    dgTransferData.Items.Refresh();

                    if (AnalysisData.JLT == "용락")
                    {
                        FrontHeatNo = AnalysisData.HeatNO;
                    }

                    TransOKflag = true;
                }));

                //전송(송신)에 필요한 데이터 조합
                string SendData;
                SendData = String.Format("A{0}B\0\n\nC\n\n\nD\n\n\nE0000F{1}G{2}H{3}I{4}J{5}K{6}L{7}M{8}N{9}",
                                          AnalysisData.HeatNO.Substring(2, 4),
                                          transferdata[transferdata.Count - 1].Cdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Sidata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Mndata.ToString("000"),
                                          transferdata[transferdata.Count - 1].Pdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Sdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Cudata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Crdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Nidata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Vdata.ToString("00"));

                //실제 전송(송신)을 하는 부분
                serial.Send(SendData);
                
               
                


            }
            else
            {
                // 분석 성분 측정치의 시작을 알리는 문자열을 찾지 못했다. 즉, 분석 성분이 전송된것이 아니다.
                MessageBox.Show("분석 성분 측정치의 시작을 알리는 문자열을 찾지 못했다. 즉, 분석 성분이 전송된것이 아니다.");
                return 10;
            }

            return 0;
        }

        public bool 항목별로데이터저장(string EleName, float Value)
        {
            //MessageBox.Show(EleName.ToUpper()+" :: " + Value.ToString());

            switch (EleName.ToUpper())
            {
                case "FE":
                    AnalysisData.FE_Data = Value;

                    //MessageBox.Show("FE : " + AnalysisData.FE_Data.ToString());

                    break;

                case "C":
                    AnalysisData.C_Data = Value;

                    //MessageBox.Show("C : " + AnalysisData.C_Data.ToString());

                    break;

                case "SI":

                    AnalysisData.Si_Data = Value;

                    //MessageBox.Show("Si : " + AnalysisData.Si_Data.ToString());

                    break;

                case "MN":
                    AnalysisData.Mn_Data = Value;

                    //MessageBox.Show("MN : " + AnalysisData.Mn_Data.ToString());

                    break;

                case "P":
                    AnalysisData.P_Data = Value;

                    //MessageBox.Show("P : " + AnalysisData.P_Data.ToString());

                    break;

                case "S":
                    AnalysisData.S_Data = Value;

                    //MessageBox.Show("S : " + AnalysisData.S_Data.ToString());

                    break;

                case "CU":
                    AnalysisData.Cu_Data = Value;

                    //MessageBox.Show("CU : " + AnalysisData.Cu_Data.ToString());

                    break;

                case "CR":
                    AnalysisData.Cr_Data = Value;

                    //MessageBox.Show("CR : " + AnalysisData.Cr_Data.ToString());

                    break;

                case "NI":
                    AnalysisData.Ni_Data = Value;

                    //MessageBox.Show("NI : " + AnalysisData.Ni_Data.ToString());

                    break;

                case "V":
                    AnalysisData.V_Data = Value;

                    //MessageBox.Show("V : " + AnalysisData.V_Data.ToString());

                    break;

                case "NB":
                    AnalysisData.Nb_Data = Value;

                    //MessageBox.Show("NB : " + AnalysisData.Nb_Data.ToString());

                    break;

                case "MO":
                    AnalysisData.Mo_Data = Value;

                    //MessageBox.Show("MO : " + AnalysisData.Mo_Data.ToString());

                    break;

                case "TI":
                    AnalysisData.Ti_Data = Value;

                    //MessageBox.Show("TI : " + AnalysisData.Ti_Data.ToString());

                    break;

                case "AL":
                    AnalysisData.Al_Data = Value;

                    //MessageBox.Show("AL : " + AnalysisData.Al_Data.ToString());

                    break;

                case "SN":
                    AnalysisData.Sn_Data = Value;

                    //MessageBox.Show("SN : " + AnalysisData.Sn_Data.ToString());

                    break;

                case "W":
                    AnalysisData.W_Data = Value;

                    //MessageBox.Show("W : " + AnalysisData.W_Data.ToString());

                    break;

                case "AS":
                    AnalysisData.As_Data = Value;

                    //MessageBox.Show("AS : " + AnalysisData.As_Data.ToString());

                    break;

                case "SB":
                    AnalysisData.Sb_Data = Value;

                    //MessageBox.Show("SB : " + AnalysisData.Sb_Data.ToString());

                    break;

                case "PB":
                    AnalysisData.Pb_Data = Value;

                    //MessageBox.Show("PB : " + AnalysisData.Pb_Data.ToString());

                    break;

                case "ZN":
                    AnalysisData.Zn_Data = Value;

                    //MessageBox.Show("ZN : " + AnalysisData.Zn_Data.ToString());

                    break;

                case "N":
                    AnalysisData.N_Data = Value;

                    //MessageBox.Show("N : " + AnalysisData.N_Data.ToString());

                    break;

                case "CA":
                    AnalysisData.Ca_Data = Value;

                    //MessageBox.Show("Ca : " + AnalysisData.Ca_Data.ToString());

                    break;

                case "INT":
                    AnalysisData.INT_Data = Value;

                    //MessageBox.Show("INT : " + AnalysisData.INT_Data.ToString());

                    break;

                case "CE":
                    AnalysisData.CE_Data = Value;
                    //MessageBox.Show("CE : " + AnalysisData.CE_Data.ToString());
                    break;


                    //default:
                    //return false;
            }

            return true;
        }

        #endregion

        #region 시리얼통신
        /// <summary>
        /// 시리얼포트에 대한 기본 환경설정(INI)을 불려오거나 새로 만든다.
        /// --------------------------------------------------------------
        /// * 아래의 변수가 있어야 한다.
        ///   string    SerialPortName;
        ///   int       SerialBaudRate;
        ///   int       SerialDataBits;
        ///   StopBits  SerialStopBits;
        ///   Parity    SerialParity;
        ///   Handshake SerialHandshake;
        /// </summary>
        public void SerialCommINI()
        {
            string INIFilePath = AppDomain.CurrentDomain.BaseDirectory + @"\\Serial.INI";

            FileInfo INIFileInfo = new FileInfo(INIFilePath);

            if (INIFileInfo.Exists)  //파일이 있는지 확인, 있을때(true), 없으면(false)
            {
                IniFile SerialINI = new IniFile();
                // ini 읽기
                SerialINI.Load(INIFilePath);

                SerialPortName = SerialINI["시리얼포트"]["포트"].ToString();
                SerialBaudRate = SerialINI["시리얼포트"]["속도"].ToInt();
                SerialDataBits = SerialINI["시리얼포트"]["데이터"].ToInt();
                SerialParity = (Parity)SerialINI["시리얼포트"]["패리티"].ToInt();
                SerialStopBits = (StopBits)SerialINI["시리얼포트"]["정지"].ToInt();
                SerialHandshake = (Handshake)SerialINI["시리얼포트"]["핸드세이크"].ToInt();

            }
            else  //설정 파일이 없으므로 기본 설정을 지정하여 새로 만든다.
            {
                IniFile SerialINI = new IniFile();

                SerialINI["시리얼포트"]["포트"] = "COM1";
                SerialINI["시리얼포트"]["속도"] = "9600";
                SerialINI["시리얼포트"]["데이터"] = "8";
                SerialINI["시리얼포트"]["패리티"] = (int)Parity.None;
                SerialINI["시리얼포트"]["정지"] = (int)StopBits.None;
                SerialINI["시리얼포트"]["핸드세이크"] = (int)Handshake.None;

                SerialINI.Save(INIFilePath);
            }
        }
        bool Onflag = true;
        void ReceivLampOnOff()
        {
            //ReceivLamp.Source = 이미지/녹색원.png;

            if (Onflag)
            {
                var uriSource = new Uri(@"/이미지/녹색원.png", UriKind.Relative);
                ReceivLamp.Source = new BitmapImage(uriSource);
                Onflag = false;
            } else
            {
                ReceivLamp.Source = null;
                Onflag = true;
            }
        }

        void TransOKLamp()
        {
            var uriSource = new Uri(@"/이미지/ok_accept_15562.png", UriKind.Relative);
            imTransOK.Source = new BitmapImage(uriSource);
            Onflag = false;
        }

        void TransNGLamp()
        {
            var uriSource = new Uri(@"/이미지/빨간원 엑스 표시.png", UriKind.Relative);

            imTransOK.Source = new BitmapImage(uriSource);
            Onflag = false;
        }

        private void DataReceivedHandler(byte[] receiveData)
        {
            int ReadIndex = 0;
            char ReadData;
            string EleValue = "";
            int tryint;

            this.Dispatcher.Invoke(new Action(delegate ()
            {
                ReceivLampOnOff();

            }));

            
            //if (serial.IsOpen) Debug.WriteLine("com open");
            //else Debug.WriteLine("com close");
            Thread.Sleep(100);

            //Debug.WriteLine(ex.ToString()5);
            while (ReadIndex < receiveData.Length)
            {
                ReadData = (char)receiveData[ReadIndex];

                switch (ReadData.ToString().ToUpper())
                {
                    case "A":
                        if (receiveData.Length > ReadIndex + 4)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {
                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayHeatNo.Text = EleValue;
                                        
                                    }));

                                    
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }
                        break;

                    case "B":
                        if (receiveData.Length > ReadIndex + 3)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        // 출강
                                        if ((EleValue == "000") && (FrontOnToTap != "0") && (ElectricfurnaceWorkEndflag == false))
                                        {
                                            //Debug.WriteLine("DateTime.Now : "+ DateTime.Now);
                                            //Debug.WriteLine("FrontHeatNo : "+ FrontHeatNo);
                                            //Debug.WriteLine("FrontOnToTap : " + FrontOnToTap);
                                            //Debug.WriteLine("FrontTapToTap : "+ FrontTapToTap);
                                            //Debug.WriteLine("FrontWatt : "+ FrontWatt);


                                            electricfurnaceworkenddata.Add(new ElectricfurnaceWorkEndData(DateTime.Now, FrontHeatNo, FrontOnToTap, FrontTapToTap, FrontWatt, "0"));
                                            dgElectricfurnaceWorkEndData.Items.Refresh();

                                            ElectricfurnaceWorkEndflag = true;

                                            Debug.WriteLine("출강 데이터 기록");
                                        }
                                        

                                        tbDisplayOnToTap.Text = EleValue;

                                        if (EleValue != "000")
                                        {
                                            FrontOnToTap = EleValue;
                                            ElectricfurnaceWorkEndflag = false;
                                        }

                                        //Debug.WriteLine("EleValue : " + EleValue);

                                        //Debug.WriteLine("FrontOnToTap : " + FrontOnToTap);
                                    }));

                                }

                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }
                        break;

                    case "C":
                        if (receiveData.Length > ReadIndex + 3)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayTapToTap.Text = EleValue;
                                        //FrontTapToTap = EleValue;

                                        //tbDisplayOnToTap.Text = EleValue;

                                        if (EleValue != "000") FrontTapToTap = EleValue;

                                        Debug.WriteLine("TapToTap EleValue : " + EleValue);

                                        Debug.WriteLine("TapToTap FrontOnToTap : " + FrontTapToTap);
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }
                        break;
                    case "D": //WATT
                        if (receiveData.Length > ReadIndex + 3)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayWatt.Text = EleValue;
                                        FrontWatt = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }
                        break;
                    case "E": //TEMP
                        if (receiveData.Length > ReadIndex + 4)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayTemp.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }
                        break;
                    case "F": //C
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayC.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "G":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplaySi.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "H":
                        if (receiveData.Length > ReadIndex + 3)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayMn.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "I":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayP.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "J":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayS.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "K":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayCu.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "L":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayCr.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "M":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayNi.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                    case "N":
                        if (receiveData.Length > ReadIndex + 2)
                        {
                            try
                            {
                                ReadIndex++;
                                ReadData = (char)receiveData[ReadIndex];
                                EleValue = ((char)receiveData[ReadIndex]).ToString();// + receiveData[ReadIndex + 2] + receiveData[ReadIndex + 3] + receiveData[ReadIndex + 4]);
                                ReadIndex++;
                                EleValue += ((char)receiveData[ReadIndex]).ToString();

                                //정상적인 데이터인지 검사
                                if (Int32.TryParse(EleValue, out tryint))
                                {

                                    this.Dispatcher.Invoke(new Action(delegate ()
                                    {
                                        tbDisplayV.Text = EleValue;
                                    }));
                                }
                            }
                            catch (Exception ex)
                            {
                                //Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                }
                ReadIndex++;
            }
            /*
             transferdata.Add(new TransferData(AnalysisData.TransTimeData,
                                                      AnalysisData.HeatNO, 
                                                      AnalysisData.JLT, 
                                                      ((int)Math.Round(AnalysisData.C_Data * 100)).ToString(),
                                                      ((int)Math.Round(AnalysisData.Si_Data * 100)).ToString(),
                                                      MnData,
                                                      ((int)Math.Round(AnalysisData.P_Data * 1000)).ToString(),
                                                      ((int)Math.Round(AnalysisData.S_Data * 1000)).ToString(),
                                                      ((int)Math.Round(AnalysisData.Cu_Data * 100)).ToString(),
                                                      ((int)Math.Round(AnalysisData.Cr_Data * 100)).ToString(),
                                                      ((int)Math.Round(AnalysisData.CE_Data * 100)).ToString(),
                                                      ((int)Math.Round(AnalysisData.V_Data * 1000)).ToString()));
            */

            //전송이 재대로 되었는지 판단하는 부분---------------------------------------------
            if ( TransOKflag)
            {
                string MnData = Math.Round(AnalysisData.Mn_Data * 100).ToString();
                switch (MnData.Length)
                {
                    case 0:
                        MnData = "000";
                        break;
                    case 1:
                        MnData = "00" + MnData;
                        break;
                    case 2:
                        MnData = "0" + MnData;
                        break;
                }

                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    imTransOK.Source = null;

                    /*
                    SendData = String.Format("A{0}B\0\n\nC\n\n\nD\n\n\nE0000F{1}G{2}H{3}I{4}J{5}K{6}L{7}M{8}N{9}",
                                          AnalysisData.HeatNO.Substring(2, 4),
                                          transferdata[transferdata.Count - 1].Cdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Sidata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Mndata.ToString("000"),
                                          transferdata[transferdata.Count - 1].Pdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Sdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Cudata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Crdata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Nidata.ToString("00"),
                                          transferdata[transferdata.Count - 1].Vdata.ToString("00"));
                    */
                    if ((tbDisplayHeatNo.Text == AnalysisData.HeatNO.Substring(2, 4)) &&
                         (tbDisplayC.Text == transferdata[transferdata.Count - 1].Cdata.ToString("00")) &&
                         (tbDisplaySi.Text == transferdata[transferdata.Count - 1].Sidata.ToString("00")) &&
                         (tbDisplayMn.Text == transferdata[transferdata.Count - 1].Mndata.ToString("000")) &&
                         (tbDisplayP.Text == transferdata[transferdata.Count - 1].Pdata.ToString("00")) &&
                         (tbDisplayS.Text == transferdata[transferdata.Count - 1].Sdata.ToString("00")) &&
                         (tbDisplayCu.Text == transferdata[transferdata.Count - 1].Cudata.ToString("00")) &&
                         (tbDisplayCr.Text == transferdata[transferdata.Count - 1].Crdata.ToString("00")) &&
                         (tbDisplayNi.Text == transferdata[transferdata.Count - 1].Nidata.ToString("00")) &&
                         (tbDisplayV.Text == transferdata[transferdata.Count - 1].Vdata.ToString("00")))
                    {
                        Debug.WriteLine("전송성분이 재대로 전송이 되었다.");

                        TransOKflag = false;

                        TransOKLamp();
                    }
                    else
                    {
                        TransNGLamp();

                        Debug.WriteLine("재전송해야 합니다.");

                        Debug.WriteLine(((int)Math.Round(AnalysisData.C_Data * 100)).ToString());
                        Debug.WriteLine(((int)Math.Round(AnalysisData.Si_Data * 100)).ToString());
                        Debug.WriteLine(MnData);
                        Debug.WriteLine(((int)Math.Round(AnalysisData.P_Data * 1000)).ToString());
                        Debug.WriteLine(((int)Math.Round(AnalysisData.S_Data * 1000)).ToString());
                        Debug.WriteLine(((int)Math.Round(AnalysisData.Cu_Data * 100)).ToString());
                        Debug.WriteLine(((int)Math.Round(AnalysisData.Cr_Data * 100)).ToString());
                        Debug.WriteLine(((int)Math.Round(AnalysisData.CE_Data * 100)).ToString());
                        Debug.WriteLine(((int)Math.Round(AnalysisData.V_Data * 1000)).ToString());
                    }
                }));


                

            }


            //-----------------------------------------------



        }

        private void DisconnectedHandler()
        {
            Debug.WriteLine("serial disconnected");
        }

        #endregion
    }

    
}
