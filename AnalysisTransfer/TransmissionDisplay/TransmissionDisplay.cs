using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TransmissionDisplay
{
    public partial class TransmissionDisplay: UserControl
    {
        public TransmissionDisplay()
        {
            InitializeComponent();
        }

        public void Display_CHNO(string data)
        {
            label15.Text = data;
        }

        public void Display_ONTOTAP(string data)
        {
            label16.Text = data;
        }
        public void Display_TAPTOTAP(string data)
        {
            label17.Text = data;
        }

        public void Display_WATT(string data)
        {
            label18.Text = data;
        }

        public void Display_TEMP(string data)
        {
            label19.Text = data;
        }

        public void Display_C(string data)
        {
            label20.Text = data;
        }
        public void Display_SI(string data)
        {
            label21.Text = data;
        }
        public void Display_MN(string data)
        {
            label22.Text = data;
        }
        public void Display_P(string data)
        {
            label23.Text = data;
        }
        public void Display_S(string data)
        {
            label24.Text = data;
        }
        public void Display_CU(string data)
        {
            label25.Text = data;
        }
        public void Display_CR(string data)
        {
            label26.Text = data;
        }
        public void Display_NI(string data)
        {
            label27.Text = data;
        }
        public void Display_V(string data)
        {
            label28.Text = data;
        }

    }
}
