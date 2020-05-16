using System;
using System.ComponentModel;


namespace QMAnalysisTransfer
{
    //List<TransferData> transferdata = new List<TransferData>();
    //dgTransferData.ItemsSource = transferdata;

        /// <summary>
        /// 분석기 컴퓨터에서 Lan으로 전송받은 데이터를 저장하는 클래스
        /// </summary>
    class TransferData : INotifyPropertyChanged
    {
        //데이터를 전송받은 시간
        DateTime _Datetime { set; get; }
        public DateTime Datetime
        {
            set
            {
                _Datetime = value;
                Notify("Datetime");
            }
            get
            {
                return _Datetime;
            }
        }

        int _HeatNo { set; get; }
        public int HeatNo
        {
            set
            {
                _HeatNo = value;
                Notify("HeatNo");
            }
            get
            {
                return _HeatNo;
            }
        }

        string _JLT { set; get; }
        public string JLT
        {
            set
            {
                _JLT = value;
                Notify("JLT");
            }
            get
            {
                return _JLT;
            }
        }

        float _Cdata { set; get; }
        public float Cdata
        {
            set
            {
                _Cdata = value;
                Notify("Cdata");
            }
            get
            {
                return _Cdata;
            }
        }

        float _Sidata { set; get; }
        public float Sidata
        {
            set
            {
                _Sidata = value;
                Notify("Sidata");
            }
            get
            {
                return _Sidata;
            }
        }

        float _Mndata { set; get; }
        public float Mndata
        {
            set
            {
                _Mndata = value;
                Notify("Mndata");
            }
            get
            {
                return _Mndata;
            }
        }

        float _Pdata { set; get; }
        public float Pdata
        {
            set
            {
                _Pdata = value;
                Notify("Pdata");
            }
            get
            {
                return _Pdata;
            }
        }

        float _Sdata { set; get; }
        public float Sdata
        {
            set
            {
                _Sdata = value;
                Notify("Sdata");
            }
            get
            {
                return _Sdata;
            }
        }

        float _Cudata { set; get; }
        public float Cudata
        {
            set
            {
                _Cudata = value;
                Notify("Cudata");
            }
            get
            {
                return _Cudata;
            }
        }

        float _Crdata { set; get; }
        public float Crdata
        {
            set
            {
                _Crdata = value;
                Notify("Crdata");
            }
            get
            {
                return _Crdata;
            }
        }

        float _Nidata { set; get; }
        public float Nidata
        {
            set
            {
                _Nidata = value;
                Notify("Nidata");
            }
            get
            {
                return _Nidata;
            }
        }

        float _Vdata { set; get; }
        public float Vdata
        {
            set
            {
                _Vdata = value;
                Notify("Vdata");
            }
            get
            {
                return _Vdata;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TransferData(DateTime datetime,
                            int heatno,
                            string jlt,
                            float cdata,
                            float sidata,
                            float mndata,
                            float pdata,
                            float sdata,
                            float cudata,
                            float crdata,
                            float nidata,
                            float vdata)
        {
            this.Datetime = datetime;
            this.HeatNo = heatno;
            this.JLT = jlt;
            this.Cdata = cdata;
            this.Sidata = sidata;
            this.Mndata = mndata;
            this.Pdata = pdata;
            this.Sdata = sdata;
            this.Cudata = cudata;
            this.Crdata = crdata;
            this.Nidata = nidata;
            this.Vdata = vdata;
        }

        protected void Notify(string propName)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }

    
}
