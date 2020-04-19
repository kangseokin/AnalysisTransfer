using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace BTProductionInformation
{
    public partial class BTProductionInformation: UserControl
    {
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

        public BTProductionInformation()
        {
            InitializeComponent();
        }

        private void BTProductionInformation_Load(object sender, EventArgs e)
        {
            Open();

            timer1.Start();
        }

        //UserControl의 종료 이벤트를 생성시키는 부분
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            this.ParentForm.FormClosing += new FormClosingEventHandler(ParentForm_FormClosing);
        }

        /// <summary>
        /// //종료 이벤트를 처리하는 부분
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ParentForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (MessageBox.Show("是否关闭父窗体?", "关闭父窗体?",
            //    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            //{
            //    e.Cancel = true;
            //}
            timer1.Stop();
            timer1.Enabled = false;

            Close();

            //e.Cancel = false;
        }

        // 데이터베이스 열기
        public bool Open()
        {
            //conn = new SqlConnection(source);
            //conn.Open();
            ERPDataBase_conn = new SqlConnection(ConnectionStringERP10GG0);
            QCMANAGER_Data_conn = new SqlConnection(ConnectionStringQCMANAGER_Data);
            //MessageBox.Show("ERPDataBase_conn...시작");

            try
            {
                ERPDataBase_conn.Open();
                QCMANAGER_Data_conn.Open();

            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString());
                return false;
            }
            finally
            {
                //try { if (DB2_conn != null) DB2_conn.Close(); }
                //catch (Exception) { }
                //return true;
            }
            //MessageBox.Show("ERPDataBase_conn...끝");
            return false;


        }

        // 데이터베이스 닫기
        public void Close()
        {
            ERPDataBase_conn.Close();
            QCMANAGER_Data_conn.Close();
        }

        // ERP 연주 데이터.
        public int ERPDataBase_GetYEONJUCount(out string YEONJU_LotNo, out int YEONJU_Count)
        {
            YEONJU_LotNo = "";

            YEONJU_Count = 0;

            if (ERPDataBase_conn != null)
            {
                string commandString = "select LOT_NO,WKRSLT_COUNT from V_MES_WKRSLT_CNT where PMEQP_NO = 'A100-10'";

                SqlCommand myCommand = new SqlCommand(commandString, ERPDataBase_conn);

                SqlDataReader myReader = myCommand.ExecuteReader();

                if (myReader.HasRows)
                {
                    myReader.Read();

                    YEONJU_LotNo = myReader.GetString(0);

                    YEONJU_Count = (int)myReader.GetDecimal(1);
                }

                myReader.Close();
            }
            else
            {
                return -1;   //db에 연결이 안되어있다. 다시 연결하는 부분을 넣어주어야 한다.
            }

            return 0;
        }

        // HDR 카운트.
        public int ERPDataBase_GetHDRCount(out string HDR_LotNo, out int HDR_Count)
        {
            HDR_LotNo = "";

            HDR_Count = 0;

            if (ERPDataBase_conn != null)
            {
                string commandString = "select LOT_NO,WKRSLT_COUNT from V_MES_WKRSLT_CNT where PMEQP_NO = 'B100-10'";

                SqlCommand myCommand = new SqlCommand(commandString, ERPDataBase_conn);

                SqlDataReader myReader = myCommand.ExecuteReader();

                if (myReader.HasRows)
                {
                    myReader.Read();

                    HDR_LotNo = myReader.GetString(0);

                    HDR_Count = (int)myReader.GetDecimal(1);
                }

                myReader.Close();
            }
            else
            {
                return -1;
            }

            return 0;
        }

        // QCMANAGER의 데이터중에 호칭,길이, 강종을 얻어오는 부분.
        public int QCMANAGERData_GetLotData(string YEONJULotNo, out string 강종, out string 호칭, out string 길이)
        {
            강종 = "";
            호칭 = "";
            길이 = "";

            if (QCMANAGER_Data_conn != null)
            {
                if (YEONJULotNo.Length > 0)
                {
                    string commandString = "select HCNM,YKWGI,GJGB from dbo.TQCQ1100 where (HeatNo = '" + YEONJULotNo + "') and  (GSGB='T');"; // 여기에 T/D를 구분하는 쿼리를 넣어야 한다.

                    SqlCommand myCommand = new SqlCommand(commandString, QCMANAGER_Data_conn);

                    SqlDataReader myReader = myCommand.ExecuteReader();

                    if (myReader.HasRows)
                    {
                        if (myReader.Read())
                        {
                            호칭 = myReader.GetString(0);

                            길이 = myReader.GetDecimal(1).ToString();

                            강종 = myReader.GetString(2);

                            myReader.Close();
                        }
                        else
                        {
                            myReader.Close();

                            return 2;   // 데이터를 얻지 못했음.(HeatNo에 해당하는 데이터가 없음.분석실에서 미리 데이터를 입력하지 않은 상태)
                        }
                    }

                    myReader.Close();
                }
                else
                {
                    //myReader.Close();
                    return 1;   // 검색할 HeatNo의 데이터가 없다.
                }

                //myReader.Close();

            }
            else
            {
                return -1; //연결이 되지 앟은 상태이다....다시 연결하는 코드를 나중에 넣어 주어야 한다.
            }

            return 0;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            화면의전광판에내용을표시();
        }

        /// <summary>
        /// 화면의 전광판에 내용을 표시.
        /// 
        /// [사용하는 전역 변수]
        /// YEONJU_LotNo - 연주 생산 비렛트 HeatNO
        /// HDR_LotNo    - HDR 장입 비렛트 HeatNO
        /// HCNM_Data    - 호칭
        /// YKWGI_Data   - 길이
        /// GJGB_Data    - 강종
        /// YEONJU_Count - 연주 생산 비렛트 카운트
        /// HDR_Count    - HDR 장입 비렛트 카운트
        /// </summary>
        private void 화면의전광판에내용을표시()
        {
            //카운트 데이터 가져오기
            ERPDataBase_GetYEONJUCount(out YEONJU_LotNo, out YEONJU_Count); //연주 절단 카운트 가져오기

            ERPDataBase_GetHDRCount(out HDR_LotNo, out HDR_Count); //압연 HDR 장입 카운트 가져오기

            QCMANAGERData_GetLotData(YEONJU_LotNo, out GJGB_Data, out HCNM_Data, out YKWGI_Data); //생산 정보 가져오기

            // 카운트 표시
            button1.Text = YEONJU_Count.ToString();
            
            button2.Text = HDR_Count.ToString();

            //MessageBox.Show(YEONJU_LotNo);

            //label2.Text = YEONJU_LotNo;

            //YEONJU_PreCount = YEONJU_Count;

            //HDR_PreCount = HDR_Count;

            // 생산 정보 표시
            label2.Text = YEONJU_LotNo; //HeatNO

            label4.Text = HCNM_Data;   //호칭

            label6.Text = YKWGI_Data;  //길이

            label8.Text = GJGB_Data;    //강종

            if ((string.Compare(YEONJU_LotNo, HDR_LotNo)) == 0)
            {
                groupBox2.Text = "연주 절단 카운트";

                groupBox3.Text = "압연 HDR 장입 카운트";

                //if (!QCMANAGER_Data_Flag)
                //{
                //    QCMANAGER_Data_Flag = true;
                //}
            }
            else
            {
                groupBox2.Text = "연주 절단 카운트(" + YEONJU_LotNo + ")";

                if(HDR_LotNo=="") groupBox3.Text = "압연 HDR 장입 카운트";
                else groupBox3.Text = "압연 HDR 장입 카운트(" + HDR_LotNo + ")";

                //아래 코드는 회사에서 확인을 해서 다시 작성...
                /*
                if (QCMANAGER_Data_Flag)
                {
                    //데이타 읽어와서 화면에 표시.
                    label27.Text = "";
                    label26.Text = "";
                    label25.Text = "";

                    switch (QCMANAGERData_GetLotData(YEONJU_LotNo))
                    {
                        case 0:   // 데이터를 성공적으로 얻었음.

                            label20.Text = YEONJU_LotNo;
                            label27.Text = HCNM_Data;
                            label26.Text = YKWGI_Data;
                            label25.Text = GJGB_Data;


                            QCMANAGER_Data_Flag = false;

                            break;

                        case 1:

                            // 오더가 없음을 확인하는 메세지를 출력해야 한다.

                            break;

                        case 2:

                            // 분석실에서 T/D 데이터를 미리 입력하지 않은 상태

                            break;

                        case 99:

                            // 예외가 발생하였다

                            break;
                    }


                    QCMANAGER_Data_Flag = false;
                }
                */
                // "연주 HeatNo가 변경되었습니다."

            }

            /*
            if (YEONJU_Count != 0)
            {
                if (YEONJU_PreCount != YEONJU_Count)
                {
                    //Status_String = "연주 비렛트 절단  :  " + DateTime.Now.ToString() + ", HeatNO : " + YEONJU_LotNo + " - " + YEONJU_Count + "개가 절단되었습니다.";
                    //listBox1.Items.Add(Status_String);
                    YEONJUCount_flag = true;
                }
            }
            else
            {
                if (YEONJUCount_flag)
                {
                    //Status_String = "------------------------------------------------------------------------------------------------";
                    //listBox1.Items.Add(Status_String);
                    //Status_String = "연주 비렛트 절단  ===>  HeatNO : " + YEONJU_PreLotNo + " - " + YEONJU_PreCount + "개로 완료해습니다.";
                    //listBox1.Items.Add(Status_String);
                    //Status_String = "------------------------------------------------------------------------------------------------";
                    //listBox1.Items.Add(Status_String);

                    YEONJUCount_flag = false;
                }
            }

            YEONJU_PreCount = YEONJU_Count;
            YEONJU_PreLotNo = YEONJU_LotNo;

            if (HDR_Count != 0)
            {
                if (HDR_PreCount != HDR_Count)
                {
                    //Status_String = "압연 H.D.R  장입  :  " + DateTime.Now.ToString() + ", HeatNO : " + HDR_LotNo + " - " + HDR_Count + "개가 장입되었습니다.";
                    //listBox1.Items.Add(Status_String);
                    //HDRCount_flag = true;
                }
            }
            else
            {
                if (HDRCount_flag)
                {
                    //Status_String = "------------------------------------------------------------------------------------------------";
                    //listBox1.Items.Add(Status_String);
                    //Status_String = "압연 H.D.R  장입  ===>  HeatNO : " + HDR_PreLotNo + " - " + HDR_PreCount + "개로 완료했습니다.";
                    //listBox1.Items.Add(Status_String);
                    //Status_String = "------------------------------------------------------------------------------------------------";
                    //listBox1.Items.Add(Status_String);

                    HDRCount_flag = false;
                }
            }

            HDR_PreCount = HDR_Count;
            HDR_PreLotNo = HDR_LotNo;
            */
        }

        
    }
}

