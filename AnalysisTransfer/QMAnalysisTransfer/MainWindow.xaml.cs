using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

            timer.Interval = TimeSpan.FromMilliseconds(0.01);    //시간간격 설정
            timer.Tick += new EventHandler(timer_Tick);          //이벤트 추가
            timer.Start();                                       //타이머 시작. 종료는 timer.Stop(); 으로 한다
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

        #endregion

        private void timer_Tick(object sender, EventArgs e)
        {
            //카운트 데이터 가져오기
            ERPDataBase_GetYEONJUCount(out YEONJU_LotNo, out YEONJU_Count); //연주 절단 카운트 가져오기

            ERPDataBase_GetHDRCount(out HDR_LotNo, out HDR_Count); //압연 HDR 장입 카운트 가져오기

            QCMANAGERData_GetLotData(YEONJU_LotNo, out GJGB_Data, out HCNM_Data, out YKWGI_Data); //생산 정보 가져오기

            // 카운트 표시
            //button1.Text = YEONJU_Count.ToString();

            //button2.Text = HDR_Count.ToString();
        }
    }
}
