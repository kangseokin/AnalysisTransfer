/*
< SerialComm 사용 >

using System.IO.Ports;

using SerialCommLib;
...
SerialComm serial = new SerialComm();
serial.DataReceivedHandler = DataReceivedHandler;
serial.DisconnectedHandler = DisconnectedHandler;
...
serial.OpenComm("COM3", 9600, 8, StopBits.One, Parity.None, Handshake.None);
...
serial.CloseComm();

private void DataReceivedHandler(byte[] receiveData)
{
    // do something with receiveData here
}

private void DisconnectedHandler()
{
    Console.WriteLine("serial disconnected");
}
 */

using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialCommLib
{
    public class SerialComm
    {
        public delegate void DataReceivedHandlerFunc(byte[] receiveData);
        public DataReceivedHandlerFunc DataReceivedHandler;

        public delegate void DisconnectedHandlerFunc();
        public DisconnectedHandlerFunc DisconnectedHandler;

        private SerialPort serialPort;

        public bool IsOpen
        {
            get
            {
                if (serialPort != null) return serialPort.IsOpen;
                return false;
            }
        }

        // serial port check
        private Thread threadCheckSerialOpen;
        private bool isThreadCheckSerialOpen = false;

        public SerialComm()
        {
        }

        public bool OpenComm(string portName, int baudrate, int databits, StopBits stopbits, Parity parity, Handshake handshake)
        {
            try
            {
                serialPort = new SerialPort();

                serialPort.PortName = portName;
                serialPort.BaudRate = baudrate;
                serialPort.DataBits = databits;
                serialPort.StopBits = stopbits;
                serialPort.Parity = parity;
                serialPort.Handshake = handshake;

                serialPort.Encoding = new System.Text.ASCIIEncoding();
                serialPort.NewLine = "\r\n";
                //serialPort.ErrorReceived += serialPort_ErrorReceived;
                serialPort.DataReceived += serialPort_DataReceived;

                serialPort.Open();

                StartCheckSerialOpenThread();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
        }

        public void CloseComm()
        {
            try
            {
                if (serialPort != null)
                {
                    //serialPort.ErrorReceived -= serialPort_ErrorReceived;
                    //serialPort.DataReceived -= serialPort_DataReceived;
                    
                    //Thread.Sleep(100);

                    StopCheckSerialOpenThread();
                    serialPort.Close();
                    serialPort = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        public bool Send(string sendData)
        {
            //Debug.WriteLine("Send(string sendData) 시작");
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    //Debug.WriteLine("Send(string sendData)");
                    serialPort.Write(sendData);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        public bool Send(byte[] sendData)
        {
            //Debug.WriteLine("Send(byte[] sendData) 시작");
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    //Debug.WriteLine("Send(byte[] sendData)");
                    serialPort.Write(sendData, 0, sendData.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        public bool Send(byte[] sendData, int offset, int count)
        {
            //Debug.WriteLine("Send(byte[] sendData, int offset, int count) 시작");
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    //Debug.WriteLine("Send(byte[] sendData, int offset, int count) ");
                    serialPort.Write(sendData, offset, count);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        private byte[] ReadSerialByteData()
        {
            serialPort.ReadTimeout = 100;
            byte[] bytesBuffer = new byte[serialPort.BytesToRead];
            int bufferOffset = 0;
            int bytesToRead = serialPort.BytesToRead;

            Thread.Sleep(100);

            while (bytesToRead > 0)
            {
                try
                {
                    int readBytes = serialPort.Read(bytesBuffer, bufferOffset, bytesToRead - bufferOffset);
                    bytesToRead -= readBytes;
                    bufferOffset += readBytes;
                }
                catch (TimeoutException ex)
                {
                    //Debug.WriteLine("2 : "+ex.ToString());
                }
            }

            return bytesBuffer;
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(100);

            try
            {
                byte[] bytesBuffer = ReadSerialByteData();
                string strBuffer = Encoding.ASCII.GetString(bytesBuffer);

                if (DataReceivedHandler != null)
                    DataReceivedHandler(bytesBuffer);

                //Debug.WriteLine("received(" + strBuffer.Length + ") : " + strBuffer);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("1 : "+ex.ToString());
            }
        }

        private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            //Debug.WriteLine("3 : "+e.ToString());
            SerialError serialError = e.EventType;
            string errorMessage = string.Empty;

            switch (serialError)
            {
                case SerialError.Frame: errorMessage = "하드웨어에서 프레이밍 오류를 발견 했습니다.";   break;
                case SerialError.Overrun: errorMessage = "문자 버퍼 오버런이 발생 했습니다. 다음 문자가 손실 됩니다."; break;

                case SerialError.RXOver: errorMessage = "입력된 버퍼 오버플로가 발생 했습니다. 입력된 버퍼에 공간이 없거나 또는 파일 끝 (EOF) 문자 뒤에 문자를 받았습니다."; break;

                case SerialError.RXParity: errorMessage = "하드웨어는 패리티 오류를 발견 했습니다."; break;
                case SerialError.TXFull: errorMessage = "응용 프로그램에서 문자를 전송 하려고 했으나 출력 버퍼가 꽉 찼습니다."; break;
                default: break;
            }
            Debug.WriteLine("3 : " + errorMessage);
        }

        private void StartCheckSerialOpenThread()
        {
            StopCheckSerialOpenThread();

            isThreadCheckSerialOpen = true;
            threadCheckSerialOpen = new Thread(new ThreadStart(ThreadCheckSerialOpen));
            threadCheckSerialOpen.Start();
        }

        private void StopCheckSerialOpenThread()
        {
            if (threadCheckSerialOpen != null)
            {
                isThreadCheckSerialOpen = false;
                if (Thread.CurrentThread != threadCheckSerialOpen)
                    threadCheckSerialOpen.Join();
                threadCheckSerialOpen = null;
            }
        }

        private void ThreadCheckSerialOpen()
        {
            while (isThreadCheckSerialOpen)
            {
                Thread.Sleep(100);

                try
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        Debug.WriteLine("seriaport disconnected");
                        if (DisconnectedHandler != null)
                            DisconnectedHandler();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    //Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}
