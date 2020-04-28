using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//using System.IO.Ports;
using SerialCommLib;

namespace AnalysisTransfer
{
    public partial class MainForm : Form
    {
        //비동기 소켓
        private Socket m_ServerSocket;
        private List<Socket> m_ClientSocket;
        private byte[] szData;

        //시리얼 포트
        SerialComm serial = new SerialComm();

        string SerialPortName;
        int SerialBaudRate;
        int SerialDataBits;
        StopBits SerialStopBits;
        Parity SerialParity;
        Handshake SerialHandshake;

        public MainForm()
        {
            
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
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
        }

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
            Socket ClientSocket = e.AcceptSocket;
            m_ClientSocket.Add(ClientSocket);

            if (m_ClientSocket != null)
            {
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
                m_ServerSocket.AcceptAsync(e);
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

                //MessageBox.Show("--" + AnalysisData.HeatNO + " -- " + AnalysisData.JLT + "--");


                int EleIndex = TransReadData.IndexOf("Fe");
                //for (int ii = TransReadData.IndexOf("Fe"); ii < 원소의총갯수 * 12 - 12; ii += 5)
                int sumii = 0;    
                for (int ii = 0 ; ii < 원소의총갯수; ii++)
                {
                    
                    항목별로데이터저장(TransReadData.Substring(EleIndex+ sumii, 5).Trim(), Convert.ToDouble(TransReadData.Substring(EleIndex + sumii + 5, 7).Trim()));

                    sumii += 12;

                    if ((EleIndex + sumii + 5+ 7) > TransReadData.Length) 
                    {
                        break;
                    }
                }

                string SendData="";
                string SendData2;

                //SendData = String.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}         A is {0} and B is {1}, {0} - {1} is {2}", A, B, A - B);

                SendData += ""; //Hex : 0x05
                

                SendData += "A" + AnalysisData.HeatNO.Substring(2,4);
                //MessageBox.Show(AnalysisData.HeatNO.Substring(2, 4));

                SendData += "B" + "\n\n\n"; //이 부분은 전기로에서 오는 ONTOTAP데이터이며 절대 전송하면 안된다.
                

                SendData += "C" + "\n\n\n";//이 부분은 전기로에서 오는 TAPTOTAP데이터이며 절대 전송하면 안된다.
                

                SendData += "D" + "\n\n\n"; //이 부분은 전기로에서 오는 WATT데이터이며 절대 전송하면 안된다.
                

                SendData += "E" + "0000"; // 이 부분은 전광판의 온도 부분이다. 나중에 C-WRIE 개수가 들어갈 예정이다.

                
                SendData += "F" + Math.Round(AnalysisData.C_Data*100);




                string MnData = Math.Round(AnalysisData.Mn_Data * 100).ToString();
                if (MnData.Length == 2) SendData += "H" + "0" + MnData;
                else SendData += "H" + MnData;
                SendData2 = String.Format("A:{0}B\n\n\nC\n\n\nD\n\n\nE0000F{1:00}G{2:00}H{3:000}I{4:00}J{5:00}K{6:00}L{7:00}M{8:00}N{9:00} ",
                                                                                                    AnalysisData.HeatNO.Substring(2, 4),
                                                                                                    Math.Round(AnalysisData.C_Data*100),
                                                                                                    Math.Round(AnalysisData.Si_Data * 100),
                                                                                                    MnData,
                                                                                                    Math.Round(AnalysisData.P_Data * 1000),
                                                                                                    Math.Round(AnalysisData.S_Data * 1000),
                                                                                                    Math.Round(AnalysisData.Cu_Data * 100),
                                                                                                    Math.Round(AnalysisData.Cr_Data * 100),
                                                                                                    Math.Round(AnalysisData.Ni_Data * 100),
                                                                                                    Math.Round(AnalysisData.V_Data * 1000));
                MessageBox.Show(SendData2);


                //string bbb = Math.Round(AnalysisData.Si_Data * 100);

                SendData += "G" + Math.Round(AnalysisData.Si_Data * 100);


                string aass = Math.Round(AnalysisData.Mn_Data * 100).ToString();
                if (aass.Length == 2) SendData += "H" +"0"+ aass;
                else SendData += "H" + aass;


                //SendData += "H" + Math.Round(AnalysisData.Mn_Data * 100);
                SendData += "I" + Math.Round(AnalysisData.P_Data * 1000);

                SendData += "J" + Math.Round(AnalysisData.S_Data * 1000);
                                
                SendData += "K" + Math.Round(AnalysisData.Cu_Data * 100);
                                
                SendData += "L" + Math.Round(AnalysisData.Cr_Data * 100);
                                
                SendData += "M" + Math.Round(AnalysisData.CE_Data * 100); ; // AnalysisData.Ni_Data; 
                                
                SendData += "N" + Math.Round(AnalysisData.V_Data * 1000); ;
                
                
                //serial.Send(SendData);


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
            string INIFilePath = Application.StartupPath + @"\\Serial.INI";

            FileInfo INIFileInfo = new FileInfo(INIFilePath);
            
            if( INIFileInfo.Exists)  //파일이 있는지 확인, 있을때(true), 없으면(false)
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

        private void DataReceivedHandler(byte[] receiveData)
        {
            int ReadIndex = 0;
            char ReadData;
            string EleValue=""; ;
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate()
                                {
                                    transmissionDisplay1.Display_CHNO(EleValue);
                                }));
                            } catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_ONTOTAP(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_TAPTOTAP(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_WATT(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_TEMP(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_C(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_SI(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_MN(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_P(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_S(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_CU(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_CR(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_NI(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
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
                                Int32.Parse(EleValue);

                                this.Invoke(new MethodInvoker(delegate ()
                                {
                                    transmissionDisplay1.Display_V(EleValue);
                                }));
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                break;
                            }
                        }

                        break;
                }
                ReadIndex++;
            }
        }

        private void DisconnectedHandler()
        {
            Console.WriteLine("serial disconnected");
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Debug.WriteLine("종료");
            //args.Dispose();
            m_ServerSocket.Close();
            serial.CloseComm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //A2942B000C099D000E0008F29G13H060I25J29K27L41M51N08

            serial.Send("A0001B\n\n\nC\n\n\nD\n\n\nE0005F29G13H060I25J29K27L41M51N08");


        }
    }
}
