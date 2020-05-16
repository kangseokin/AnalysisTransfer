using System;
using System.ComponentModel;


namespace QMAnalysisTransfer
{
    //List<ElectricfurnaceWorkEndData> electricfurnaceworkenddata = new List<ElectricfurnaceWorkEndData>();
    //dgElectricfurnaceWorkEndData.ItemsSource = electricfurnaceworkenddata;

    //전기로 출강 정보
    /// <summary>
    /// 전기로 출강 정보를 저장하는 클래스
    /// </summary>
    class ElectricfurnaceWorkEndData : INotifyPropertyChanged
    {
        //출강 시간
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

        string _HeatNo { set; get; }
        public string HeatNo
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

        string _OnToTap { set; get; }
        public string OnToTap
        {
            set
            {
                _OnToTap = value;
                Notify("OnToTap");
            }
            get
            {
                return _OnToTap;
            }
        }

        string _TapToTap { set; get; }
        public string TapToTap
        {
            set
            {
                _TapToTap = value;
                Notify("TapToTap");
            }
            get
            {
                return _TapToTap;
            }
        }

        string _Watt { set; get; }
        public string Watt
        {
            set
            {
                _Watt = value;
                Notify("Watt");
            }
            get
            {
                return _Watt;
            }
        }

        string _OffToTap { set; get; }
        public string OffToTap
        {
            set
            {
                _OffToTap = value;
                Notify("OffToTap");
            }
            get
            {
                return _OffToTap;
            }
        }

        string _Temp { set; get; }
        public string Temp
        {
            set
            {
                _Temp = value;
                Notify("Temp");
            }
            get
            {
                return _Temp;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ElectricfurnaceWorkEndData(DateTime datetime, string heatno, string ontotap, string taptotap, string watt, string temp)
        {

            this.Datetime = datetime;
            this.HeatNo = heatno;
            this.OnToTap = ontotap;
            this.TapToTap = taptotap;
            this.Watt = watt;
            this.Temp = temp;
            this.OffToTap = (int.Parse(taptotap) - int.Parse(ontotap)).ToString();
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
