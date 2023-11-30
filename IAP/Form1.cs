using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;

/*------------兼容ZLG的数据类型---------------------------------*/

//1.ZLGCAN系列接口卡信息的数据类型。
public struct VCI_BOARD_INFO
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public byte[] Reserved;
}

/////////////////////////////////////////////////////
//2.定义CAN信息帧的数据类型。
unsafe public struct VCI_CAN_OBJ  //使用不安全代码
{
    public uint ID;
    public uint TimeStamp;        //时间标识
    public byte TimeFlag;         //是否使用时间标识
    public byte SendType;         //发送标志。保留，未用
    public byte RemoteFlag;       //是否是远程帧
    public byte ExternFlag;       //是否是扩展帧
    public byte DataLen;          //数据长度
    public fixed byte Data[8];    //数据
    public fixed byte Reserved[3];//保留位

}

//3.定义初始化CAN的数据类型
public struct VCI_INIT_CONFIG
{
    public UInt32 AccCode;
    public UInt32 AccMask;
    public UInt32 Reserved;
    public byte Filter;   //0或1接收所有帧。2标准帧滤波，3是扩展帧滤波。
    public byte Timing0;  //波特率参数，具体配置，请查看二次开发库函数说明书。
    public byte Timing1;
    public byte Mode;     //模式，0表示正常模式，1表示只听模式,2自测模式
}

/*------------其他数据结构描述---------------------------------*/
//4.USB-CAN总线适配器板卡信息的数据类型1，该类型为VCI_FindUsbDevice函数的返回参数。
public struct VCI_BOARD_INFO1
{
    public UInt16 hw_Version;
    public UInt16 fw_Version;
    public UInt16 dr_Version;
    public UInt16 in_Version;
    public UInt16 irq_Num;
    public byte can_Num;
    public byte Reserved;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] str_Serial_Num;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_hw_Type;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] str_Usb_Serial;
}

/*------------数据结构描述完成---------------------------------*/

public struct CHGDESIPANDPORT
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
    public byte[] szpwd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public byte[] szdesip;
    public Int32 desport;

    public void Init()
    {
        szpwd = new byte[10];
        szdesip = new byte[20];
    }
}
namespace IAP
{
    struct Books
    {
        public string FilePath;
        public string comboBox1Text;
        public string comboBox2Text;
        public bool CanOrUart;
    };
    public partial class Form1 : Form
    {
        //Sunisoft.IrisSkin.SkinEngine s;
        const int DEV_USBCAN = 3;
        const int DEV_USBCAN2 = 4;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DeviceType"></param>
        /// <param name="DeviceInd"></param>
        /// <param name="Reserved"></param>
        /// <returns></returns>
        /*------------兼容ZLG的函数描述---------------------------------*/
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_OpenDevice(UInt32 DeviceType, UInt32 DeviceInd, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_CloseDevice(UInt32 DeviceType, UInt32 DeviceInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_InitCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_INIT_CONFIG pInitConfig);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ReadBoardInfo(UInt32 DeviceType, UInt32 DeviceInd, ref VCI_BOARD_INFO pInfo);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_GetReceiveNum(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ClearBuffer(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_StartCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ResetCAN(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Transmit(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pSend, UInt32 Len);

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_Receive(UInt32 DeviceType, UInt32 DeviceInd, UInt32 CANInd, ref VCI_CAN_OBJ pReceive, UInt32 Len, Int32 WaitTime);

        /*------------其他函数描述---------------------------------*/

        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_ConnectDevice(UInt32 DevType, UInt32 DevIndex);
        [System.Runtime.InteropServices.DllImport("controlcan.dll")]
        static extern UInt32 VCI_UsbDeviceReset(UInt32 DevType, UInt32 DevIndex, UInt32 Reserved);
        [DllImport("controlcan.dll")]
        static extern UInt32 VCI_FindUsbDevice(ref VCI_BOARD_INFO1 pInfo);
        /*------------函数描述结束---------------------------------*/


        static UInt32 m_devtype = 4;//USBCAN2
        VCI_CAN_OBJ[] CAN_Buffer_Receive = new VCI_CAN_OBJ[1000];
        VCI_CAN_OBJ[] CAN_Buffer_Transmit = new VCI_CAN_OBJ[1000];

        UInt32[] m_arrdevtype = new UInt32[20];

        UInt32 m_bOpen = 0;
        UInt32 m_devind = 0;
        UInt32 m_canind = 0;
        UInt32 _gDownStatus = 0;
        //List<byte> termsList;
        //byte[] _gbyteBinList = new byte[1024 * 512];
        uint INIT_REV_CAN_ID = 0x104;
        uint INIT_SEND_CAN_ID = 0x103;
        uint INIT_PC_CAN_ID = 0x7df;
        uint MAX_WAIT_INIT = 100;
        string _gCheckSumStr = "";
        Books _gMessage;
        uint _gMax_wait = 5;
        public Form1()
        {
            InitializeComponent();
            //GetComList();
            //comboBox2.SelectedIndex = 4;
            //禁止编译器对跨线程访问做检查,危险，最好只是读，设置使用委托
            //Control.CheckForIllegalCrossThreadCalls = false;
            //this.comboBox1.DropDown += new System.EventHandler(this.comboBox1_DropDown);
            //serialPort1.DataReceived += new SerialDataReceivedEventHandler(SerialReceiveCallback);
            //serialPort1.Encoding = Encoding.UTF8;
            //comboBox3.SelectedIndex = 0;
        }
        delegate void Update(int x, string text);
        void Settext(int x, string text)
        {
            if (x == 0)
            {
                button2.Enabled = true;
            }
            else if (x == 1)
            {
                richTextBox1.AppendText(text + "\r\n");
            }
            else if (x == 2)
            {
                try
                {
                    progressBar1.Value = Convert.ToInt32(text);
                }
                catch
                {

                }
            }
        }
        public void SerialReceiveCallback(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (serialPort1.BytesToRead == 0)
                    return;

                //接收字节序列直接处理
                byte[] bytes = new byte[serialPort1.BytesToRead];
                serialPort1.Read(bytes, 0, bytes.Length);

                //或者再转成字符串进行打印\处理
                string receive_data = bytes.ToString();//System.Text.Encoding.Default.GetString(bytes);
                this.BeginInvoke(new Update(Settext), 1, receive_data);
                
            }
            catch
            {
                MessageBox.Show("数据接收出现异常");
            }
        }
        public void GetComList()
        {
            //string[] names = SerialPort.GetPortNames();
            //comboBox1.Items.Clear();
            //for (int i = 0; i < names.Length; i++)
            //{
            //    comboBox1.Items.Add(names[i]);
            //    comboBox1.SelectedIndex = 0;
            //}
            //comboBox1.Items.Clear();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_PnPEntity"))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        object com = hardInfo.Properties["Name"].Value;

                        if (com != null && com.ToString().Contains("(COM"))
                        {
                            if (com.ToString().Contains("CH34")  || com.ToString().Contains("蓝牙")
                                || com.ToString().Contains("JLink") || com.ToString().Contains("端口"))
                            {
                                continue;
                            }
                            //object value = hardInfo.Properties["Description"].Value;
                            //object value = hardInfo.Properties["CreationClassName"].Value;
                            //object value = hardInfo.Properties["Service"].Value;
                            //if (value != null)
                            //    richTextBox1.AppendText(value.ToString() + "\r\n");
                            string strComName = com.ToString();
                            string[] sArray = strComName.Split(new string[] { "(" }, 2, StringSplitOptions.RemoveEmptyEntries);
                            String subString = sArray[1].Substring(0, sArray[1].Length - 1);

                            int index = strComName.IndexOf("COM", 0);
                            if (index != -1)
                                strComName = subString + " " + strComName.Substring(0, index - 1);
                            else
                                strComName = subString + " " + strComName;

                            //comboBox1.Items.Add(strComName);
                            //comboBox1.Text = strComName;
                        }
                    }
                    //if (comboBox1.Items.Count == 0)
                    //    MessageBox.Show("未扫描到COM口，请检查USB连接");
                }
            }
            catch
            {
                MessageBox.Show("异常:扫描失败");
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            GetComList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openfile = new OpenFileDialog();
            openfile.Filter = "s19 files (*.s19)|*.s19";
            openfile.FilterIndex = 0;
            openfile.RestoreDirectory = true;

            openfile.FileName = null;
            openfile.Title = "选择s19文件";
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openfile.FileName;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void Print_Func(string str)
        {

            this.BeginInvoke(new Update(Settext), 1, str);
        }
        private void SetProgress(string str)
        {

            this.BeginInvoke(new Update(Settext), 2, str);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //try
            //{
            //    if (button3.Text == "打开串口")
            //    {
            //        button3.Text = "关闭串口";
            //        serialPort1.Close();
            //        //string[] sArray = comboBox1.Text.Split(new string[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
            //        //serialPort1.PortName = sArray[0];
            //        serialPort1.BaudRate = 115200;
            //        serialPort1.Parity = Parity.None;
            //        serialPort1.StopBits = StopBits.One;
            //        serialPort1.DataBits = 8;
            //        serialPort1.Open();
            //        textBox1.AppendText("打开串口成功\r\n");
            //    }
            //    else
            //    {
            //        button3.Text = "打开串口";
            //        textBox1.AppendText("串口已关闭\r\n");
            //        if (serialPort1.IsOpen == false)
            //            return;
            //        else
            //            serialPort1.Close();
            //    }
            //}
            //catch
            //{
            //    button3.Text = "打开串口";
            //    MessageBox.Show("打开串口失败");
            //}
        }

        private void button4_Click(object sender, EventArgs e)
        {

            byte[] byteArray = { 0x55,0xAA};
            serialPort1.Write(byteArray,0, byteArray.Length);
        }
        ThreadStart childref = null;
        Thread childThread = null;


        private bool openSerialPort()
        {
            try
            {
         
                
                    serialPort1.Close();
                    string[] sArray = _gMessage.comboBox1Text.Split(new string[] { " " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    serialPort1.PortName = sArray[0];
                    serialPort1.BaudRate = Convert.ToInt32(_gMessage.comboBox2Text);
                    serialPort1.Parity = Parity.None;
                    serialPort1.StopBits = StopBits.One;
                    serialPort1.DataBits = 8;
                    serialPort1.Open();
                    Print_Func("打开串口成功\r\n");
                    return true;


            }
            catch
            {

                Print_Func("打开串口失败");
            }
            return false;
        }
        unsafe private void Sendmsg_FC()
        {
            VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            //sendobj.Init();
            sendobj.RemoteFlag = 0;
            sendobj.ExternFlag = 0;
            sendobj.ID = INIT_SEND_CAN_ID;
            sendobj.DataLen = 8;
            sendobj.Data[0] = 0x30;
            for (byte i = 1; i < 8; i++)
            {
                sendobj.Data[i] = 0;
            }
            VCI_Transmit(m_devtype, m_devind, 0, ref sendobj, 1);
        }
        unsafe private string RecMsg()
        {
            uint max_wait = MAX_WAIT_INIT;//MAX_WAIT_INIT;
            string receive_msg = "";
            int byte_num = 0;
            while (max_wait >= 1)
            {
                Task.Delay(10).Wait();
                max_wait = max_wait - 1;
                UInt32 res = VCI_Receive(m_devtype, m_devind, 0, ref CAN_Buffer_Receive[0], 1000, 100);
                for (UInt32 i = 0; i < res; i++)
                {
                    if (CAN_Buffer_Receive[i].ID == INIT_REV_CAN_ID)
                    {
                        if ((CAN_Buffer_Receive[i].Data[0] & 0xf0) == 0x00)
                        {
                            for (int k = 1; k <= CAN_Buffer_Receive[i].Data[0]; k++)
                                receive_msg += Convert.ToString(CAN_Buffer_Receive[i].Data[k], 16).PadLeft(2, '0');
                            max_wait = 0;

                        }
                        else if ((CAN_Buffer_Receive[i].Data[0] & 0xf0) == 0x10)
                        {
                            //first frame
                            byte_num = (CAN_Buffer_Receive[i].Data[0] & 0x0f) * 256 + CAN_Buffer_Receive[i].Data[1];
                            //for k in range(2, 8):
                            for (int k = 2; k < 8; k++)
                                receive_msg += Convert.ToString(CAN_Buffer_Receive[i].Data[k], 16).PadLeft(2, '0');
                            byte_num = byte_num - 6;
                            //# flow caontrol frame
                            //Sendmsg_FC(my_dll, can_channel, INIT_PC_CAN_ID);
                            //VCI_Transmit(m_devtype, m_devind, 0, ref CAN_Buffer_Transmit[0], 1);
                            max_wait = MAX_WAIT_INIT;
                            //break;
                        }
                        else if ((CAN_Buffer_Receive[i].Data[0] & 0xf0) == 0x20)
                        {
                            if (byte_num > 7)
                            {
                                for (int k = 1; k < 8; k++)
                                {
                                    receive_msg += Convert.ToString(CAN_Buffer_Receive[i].Data[k], 16).PadLeft(2, '0');
                                }
                                byte_num = byte_num - 7;
                                max_wait = MAX_WAIT_INIT;
                            }
                            else
                            {
                                for (int k = 1; k < byte_num + 1; k++)
                                {
                                    receive_msg += Convert.ToString(CAN_Buffer_Receive[i].Data[k], 16).PadLeft(2, '0');
                                }
                                byte_num = 0;
                                max_wait = 0;
                            }
                        }

                    }
                }
            }
            return receive_msg;
        }


        private int Check_PR(string pr_msg)
        {
            string m_status = "wait";
            string re_msg = "";
            uint wait_cnt = _gMax_wait;
            while (m_status == "wait")
            {
                if (re_msg == "")
                    re_msg = RecMsg();
                if (re_msg.Length == 0)
                {
                    if (wait_cnt > 0)
                    {
                        //# wait 1 second
                        wait_cnt -= 1;
                        Print_Func("time to wait: " + wait_cnt);
                        //time.sleep(1)
                        Task.Delay(1000).Wait();
                    }
                    else
                    {
                        m_status = "stop";
                    }
                }
                else if (re_msg.Substring(0, 2) == "7f")
                {
                    if (re_msg.Substring(4, 2) == "78")
                    {
                        m_status = "wait";
                        wait_cnt = 6;
                        re_msg = re_msg.Substring(6, re_msg.Length - 6);
                    }
                    else
                    {

                        m_status = "stop";
                    }
                }
                else if (re_msg.Length >= pr_msg.Length && re_msg.Substring(0, pr_msg.Length) == pr_msg)
                {
                    if (_gDownStatus == 6 && re_msg.Length >= 12)
                        _gCheckSumStr = re_msg.Substring(8, 4);
                    m_status = "ongoing";
                }
                else
                {
                    m_status = "stop";
                }
            }
            if (m_status == "ongoing")
                return 1;
            else
                return 0;
        }
        unsafe private int SendCanCommd(string cmd_msg)
        {
            //VCI_CAN_OBJ sendobj = new VCI_CAN_OBJ();
            int msg_byte_len = 0;
            uint CAN_frame_num = 1;

            //sendobj.Init();
            CAN_Buffer_Transmit[0].RemoteFlag = 0;
            CAN_Buffer_Transmit[0].ExternFlag = 0;
            CAN_Buffer_Transmit[0].ID = INIT_SEND_CAN_ID;
            CAN_Buffer_Transmit[0].DataLen = 8;

            msg_byte_len = cmd_msg.Length / 2;
            if (msg_byte_len <= 7)
            {
                //CAN_Buffer_Transmit[0].DataLen = (byte)msg_byte_len;
                CAN_Buffer_Transmit[0].Data[0] = Convert.ToByte(msg_byte_len);
                //先设置数据
                for (int i = 0; i < msg_byte_len; i++)
                {
                    CAN_Buffer_Transmit[0].Data[i + 1] = Convert.ToByte(cmd_msg.Substring(i * 2, 2), 16);
                }
                //不足部分补0
                for (int i = msg_byte_len; i < 7; i++)
                {
                    CAN_Buffer_Transmit[0].Data[i + 1] = 0;
                }

            }
            else
            {
                //组装包头
                CAN_Buffer_Transmit[0].Data[0] = Convert.ToByte(0x10 + (msg_byte_len / 256));
                CAN_Buffer_Transmit[0].Data[1] = Convert.ToByte(msg_byte_len % 256);
                //设置第一包内容
                for (int i = 0; i < 6; i++)
                {
                    CAN_Buffer_Transmit[0].Data[i + 2] = Convert.ToByte(cmd_msg.Substring(i * 2, 2), 16);
                }
                //发送第一包
                VCI_Transmit(m_devtype, m_devind, 0, ref CAN_Buffer_Transmit[0], CAN_frame_num);
                uint max_wait = 5;
                while (max_wait >= 1)
                {
                    Task.Delay(10).Wait();
                    max_wait--;
                    UInt32 res = VCI_Receive(m_devtype, m_devind, 0, ref CAN_Buffer_Receive[0], 1000, 100);
                    for (UInt32 i = 0; i < res; i++)
                    {
                        if (CAN_Buffer_Receive[i].ID == INIT_REV_CAN_ID)
                        {
                            if (CAN_Buffer_Receive[i].Data[0] == 0x30)
                            {
                                max_wait = 0;
                                break;
                            }
                            else if (CAN_Buffer_Receive[i].Data[0] == 0x31)
                            {
                                max_wait = MAX_WAIT_INIT;
                                //break;
                            }
                            else
                            {
                                //_gDownStatus = 0;
                                return 0;
                            }

                        }
                    }
                }
                //发送剩余包

                int msg_index = 12;
                CAN_frame_num = 0;
                while (msg_index < cmd_msg.Length)
                {
                    CAN_Buffer_Transmit[CAN_frame_num].RemoteFlag = 0;
                    CAN_Buffer_Transmit[CAN_frame_num].ExternFlag = 0;
                    CAN_Buffer_Transmit[CAN_frame_num].ID = INIT_SEND_CAN_ID;
                    CAN_Buffer_Transmit[CAN_frame_num].DataLen = 8;
                    CAN_Buffer_Transmit[CAN_frame_num].Data[0] = Convert.ToByte(0x20 + (CAN_frame_num + 1) % 16);
                    for (int i = 1; i < 8; i++)
                    {
                        if (msg_index < cmd_msg.Length)
                        {
                            CAN_Buffer_Transmit[CAN_frame_num].Data[i] = Convert.ToByte(cmd_msg.Substring(msg_index, 2), 16);
                            msg_index += 2;
                        }
                        else
                        {
                            //if(CAN_Buffer_Transmit[CAN_frame_num].DataLen == 8)
                            //    CAN_Buffer_Transmit[CAN_frame_num].DataLen = Convert.ToByte(i);
                            CAN_Buffer_Transmit[CAN_frame_num].Data[i] = 0;
                        }
                    }
                    CAN_frame_num++;
                }
            }

            VCI_Transmit(m_devtype, m_devind, 0, ref CAN_Buffer_Transmit[0], CAN_frame_num);
            return 1;
        }
        bool OpenCanDevice(int btype)
        {
            m_devtype = 4;

            m_devind = 0;
            m_canind = 0;
            VCI_CloseDevice(m_devtype, m_devind);
            if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
            {
                return true;
            }

            m_bOpen = 1;
            VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
            config.AccCode = 0x00000000;
            config.AccMask = 0xFFFFFFFF;
            if (btype == 0)
                config.Timing0 = 00;
            else
                config.Timing0 = 01;
            config.Timing1 = 0x1C;
            config.Filter = 2;
            config.Mode = 0;
            VCI_InitCAN(m_devtype, m_devind, 0, ref config);
            //VCI_InitCAN(m_devtype, m_devind, 1, ref config);
            VCI_StartCAN(m_devtype, m_devind, 0);
            //VCI_StartCAN(m_devtype, m_devind, 1);

            return false;
        }
        private void CallToChildThread()
        {
            //if(openSerialPort())
            //    while (true)
            //    {
            //        if (serialPort1.BytesToRead == 0)
            //            Thread.Sleep(300);
            //        else
            //        {
            //            //接收字节序列直接处理
            //            byte[] bytes = new byte[serialPort1.BytesToRead];
            //            serialPort1.Read(bytes, 0, bytes.Length);

            //            //或者再转成字符串进行打印\处理
            //            //string receive_data = System.Text.Encoding.Default.GetString(bytes);
            //            string receive_data = bytes.ToString();
            //            Print_Func(receive_data);
            //        }


            //    }

            //this.BeginInvoke(new Update(Settext), 0, null);
            string cmdHead_msg = "3101";
            string pr_msg = "7101";
            string Rout_LID = "";
            string cmd_msg;
            //if (type == 1)
            //    Rout_LID = "ff0001";

            //else if (type == 1)
            //    Rout_LID = "ff0005";

            //else if (type == 2)
            //    Rout_LID = "ff01";

            //else if (type == 3)
            //    Rout_LID = "aa00";

            //else if (type == 1)
            //    Rout_LID = "f000";


            //cmd_msg = cmd_msg + Rout_LID;
            //progressBar1.Value = 0;
            //label2.Text = 0 + "%";
            UInt32 dataCount = 0;
            UInt16 checkSumUint16 = 0;
            UInt16 checkSumUint16_2 = 0;
            string filestring = "";
            char[] VerArr = new char[32];
            //string tempstr = "";
            if (textBox1.Text != "")
            {
                Print_Func("Data Loading");
                StreamReader sr = new StreamReader(textBox1.Text, Encoding.Default);
                String line;
                byte[] buffer = new byte[100];
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Substring(0, 2) == "S3" || line.Substring(0, 2) == "S2" || line.Substring(0, 2) == "S1")
                    {
                        UInt32 bufferCount = 0;
                        int i;
                        int len = Convert.ToByte(line.Substring(2, 2), 16);
                        if (len != (line.Length - 4) / 2)
                        {
                            MessageBox.Show("File format error", "ERROR",
                                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }
                        for (i = 2; i < line.Length; i += 2)
                        {
                            buffer[bufferCount++] = Convert.ToByte(line.Substring(i, 2), 16);
                        }
                        byte byNum = 0;
                        for (i = 0; i < bufferCount - 1; i++)
                        {
                            byNum += buffer[i];
                        }
                        if ((0xff - byNum) != buffer[i])
                        {
                            MessageBox.Show("File format error", "ERROR",
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            return;
                        }

                        //这里设置偏移位置，S1：2，S2：3，S3：4，
                        int count = Convert.ToInt16(line.Substring(1, 1)) + 1;
                        //for (i = count; i < bufferCount - 1; i++)
                        //{
                        //    //_gbyteBinList[dataCount++] = buffer[i];
                        //    checkSumUint16 += buffer[i];
                        //}
                        filestring += line.Substring(4 + count * 2, (len - 1 - count) * 2);
                        //if (line.Substring(0, 2) == "S3" || line.Substring(0, 2) == "S2" || line.Substring(0, 2) == "S1")
                        //byte[] bytes =  Convert.ToByte(line, 16))

                    }
                }
                checkSumUint16 = 0;
                for (int i = 0; i < filestring.Length / 2; i++)
                {
                    //if (i % 128 == 0)
                    //    richTextBox1.Text += Convert.ToString(checkSumUint16, 16).ToUpper() + "\n";
                    checkSumUint16 += Convert.ToByte(filestring.Substring(i * 2, 2), 16);

                }
                //richTextBox1.Text += Convert.ToString(checkSumUint16, 16).ToUpper() + "\n";
                //richTextBox1.Text = ""+ checkSumUint16_2; 
                sr.Close();
            }
            //测试文件用退出，正常注释掉
            //this.BeginInvoke(new Update(Settext), 0, "");
            //return;
            bool InitFlag = false;
            bool secInitFlag = false;
            m_devtype = 4;

            m_devind = 0;
            m_canind = 0;
            VCI_CloseDevice(m_devtype, m_devind);
            if (VCI_OpenDevice(m_devtype, m_devind, 0) == 0)
            {
                MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            Print_Func("Reset to 250K");
            m_bOpen = 1;
            VCI_INIT_CONFIG config = new VCI_INIT_CONFIG();
            config.AccCode = 0x00000000;
            config.AccMask = 0xFFFFFFFF;
            config.Timing0 = 01;
            config.Timing1 = 0x1C;
            config.Filter = 2;
            config.Mode = 0;
            VCI_InitCAN(m_devtype, m_devind, 0, ref config);
            //VCI_InitCAN(m_devtype, m_devind, 1, ref config);
            VCI_StartCAN(m_devtype, m_devind, 0);
            _gDownStatus = 1;
            _gMax_wait = 0;
            while (_gDownStatus > 0 && _gDownStatus < 10)
            {
                switch (_gDownStatus)
                {
                    case 1:
                        //CopyKernel
                        //第一次发送
                        Print_Func("CopyKernel: ");
                        Rout_LID = "f000";
                        cmd_msg = cmdHead_msg + Rout_LID;
                        pr_msg = "7101";
                        if (SendCanCommd(cmd_msg) == 0)
                        {
                            //_gDownStatus = 0;
                            //break;
                        }
                        if (Check_PR(pr_msg) == 0)
                        {
                            //_gDownStatus = 0;
                            InitFlag = true;
                        }
                        if (InitFlag)
                        {
                            if (OpenCanDevice(0) == false)
                            {
                                Print_Func("Reset to 500K");
                                Print_Func("CopyKernel: ");
                                Rout_LID = "f000";
                                cmd_msg = cmdHead_msg + Rout_LID;
                                pr_msg = "7101";
                                if (SendCanCommd(cmd_msg) == 0)
                                {
                                    //_gDownStatus = 0;
                                    //break;
                                }
                                if (Check_PR(pr_msg) == 0)
                                {
                                    _gDownStatus = 0;
                                    break;

                                }
                            }
                            else
                            {
                                _gDownStatus = 0;
                                break;
                            }
                        }
                        //else
                        _gDownStatus++;
                        break;
                    case 2:
                        //EraseAppFlash
                        //InitFlag = false;
                        Print_Func("Delay 2秒: ");
                        Task.Delay(3000).Wait();


                        //if (OpenCanDevice(1) == false)
                        {

                            Print_Func("CopyKernel: ");
                            Rout_LID = "f000";
                            cmd_msg = cmdHead_msg + Rout_LID;
                            pr_msg = "7101";
                            if (SendCanCommd(cmd_msg) == 0)
                            {
                                //_gDownStatus = 0;
                                //break;
                            }
                            if (Check_PR(pr_msg) == 0)
                            {
                                //_gDownStatus = 0;
                                //break;
                                //InitFlag = true;
                                secInitFlag = true;

                            }
                        }

                        if (secInitFlag)
                        {
                            if (InitFlag)
                            {
                                if (OpenCanDevice(1) == false)
                                {
                                    Print_Func("Reset to 250K");
                                    Print_Func("CopyKernel: ");
                                    Rout_LID = "f000";
                                    cmd_msg = cmdHead_msg + Rout_LID;
                                    pr_msg = "7101";
                                    if (SendCanCommd(cmd_msg) == 0)
                                    {
                                        //_gDownStatus = 0;
                                        //break;
                                    }
                                    if (Check_PR(pr_msg) == 0)
                                    {
                                        _gDownStatus = 0;
                                        break;

                                    }
                                }
                                else
                                {
                                    _gDownStatus = 0;
                                    break;
                                }
                            }
                            else
                            {
                                if (OpenCanDevice(0) == false)
                                {
                                    Print_Func("Reset to 500K");
                                    Print_Func("CopyKernel: ");
                                    Rout_LID = "f000";
                                    cmd_msg = cmdHead_msg + Rout_LID;
                                    pr_msg = "7101";
                                    if (SendCanCommd(cmd_msg) == 0)
                                    {
                                        //_gDownStatus = 0;
                                        //break;
                                    }
                                    if (Check_PR(pr_msg) == 0)
                                    {
                                        _gDownStatus = 0;
                                        break;

                                    }
                                }
                                else
                                {
                                    _gDownStatus = 0;
                                    break;
                                }
                            }
                        }
                        //if (OpenCanDevice(0))
                        //{
                        //    _gDownStatus = 0;
                        //    break;
                        //}
                        //Print_Func("Delay 2秒: ");
                        //Task.Delay(2000).Wait();
                        //Print_Func("CopyKernel: ");
                        //Rout_LID = "f000";
                        //cmd_msg = cmdHead_msg + Rout_LID;
                        //pr_msg = "7101";
                        //if (SendCanCommd(cmd_msg) == 0)
                        //{
                        //    _gDownStatus = 0;
                        //    break;
                        //}
                        //if (Check_PR(pr_msg) == 0)
                        //{
                        //    _gDownStatus = 0;
                        //    break;
                        //}
                        //else
                        _gMax_wait = 5;
                        Print_Func("EraseAppFlash: ");
                        Rout_LID = "ff0001";
                        cmd_msg = cmdHead_msg + Rout_LID;
                        pr_msg = "7101";
                        if (SendCanCommd(cmd_msg) == 0)
                        {
                            _gDownStatus = 0;
                            break;
                        }
                        if (Check_PR(pr_msg) == 0)
                        {
                            _gDownStatus = 0;
                        }
                        else
                            _gDownStatus++;
                        break;
                    case 3:
                        //ReqstDnld: App
                        //Task.Delay(1000).Wait();
                        Print_Func("ReqstDnld: App: ");
                        Rout_LID = "3400440000800000038000";
                        cmd_msg = Rout_LID;
                        pr_msg = "74201000";
                        if (SendCanCommd(cmd_msg) == 0)
                        {
                            _gDownStatus = 0;
                            break;
                        }
                        if (Check_PR(pr_msg) == 0)
                        {
                            _gDownStatus = 0;
                        }
                        else
                            _gDownStatus++;
                        break;
                    case 4:
                        //TransferData: App
                        int fileCount = 1;
                        Print_Func("Delay 1秒: ");
                        Task.Delay(1000).Wait();
                        Print_Func("TransferData: App: ");
                        for (int i = 0; i < filestring.Length; i += (0xFFD * 2))
                        {
                            string strHead = "36" + Convert.ToString(fileCount, 16).PadLeft(2, '0'); ; //fileCount.ToString(16).PadLeft(2, '0');
                            pr_msg = "76" + Convert.ToString(fileCount, 16).PadLeft(2, '0');
                            if (i + 0xFFD * 2 <= filestring.Length)
                            {
                                if (SendCanCommd(strHead + filestring.Substring(i, 0xFFD * 2)) == 0)
                                {
                                    _gDownStatus = 0;
                                    break;
                                }
                                //progressBar1.Value = 100 * i / filestring.Length;
                                SetProgress(""+ 100 * i / filestring.Length);
                                //label2.Text = 100 * i / filestring.Length + "%";
                            }
                            else
                            {
                                if (SendCanCommd(strHead + filestring.Substring(i, filestring.Length - i)) == 0)
                                {
                                    _gDownStatus = 0;
                                    break;
                                }
                                //progressBar1.Value = 99;
                                SetProgress("99");
                                //label2.Text = "99%";
                            }
                            if (Check_PR(pr_msg) == 0)
                            {
                                _gDownStatus = 0;
                                break;
                            }

                            //Task.Delay(500).Wait();
                            fileCount++;
                        }
                        if (_gDownStatus > 0)
                            _gDownStatus++;
                        //_gDownStatus = 5;
                        break;
                    case 5:
                        //TransferExit
                        Print_Func("TransferExit:");
                        Rout_LID = "37";
                        pr_msg = "77";
                        cmd_msg = Rout_LID;
                        if (SendCanCommd(cmd_msg) == 0)
                        {
                            _gDownStatus = 0;
                            break;
                        }
                        if (Check_PR(pr_msg) == 0)
                        {
                            _gDownStatus = 0;
                            break;
                        }
                        _gDownStatus++;

                        break;
                    case 6:
                        //CheckCS
                        Print_Func("CheckCS:");
                        Rout_LID = "ff01";
                        cmd_msg = cmdHead_msg + Rout_LID;
                        pr_msg = "7101";
                        if (SendCanCommd(cmd_msg) == 0)
                        {
                            _gDownStatus = 0;
                            break;
                        }
                        if (Check_PR(pr_msg) == 0)
                        {
                            _gDownStatus = 0;
                        }
                        else
                        {
                            if(checkSumUint16== Convert.ToUInt16(_gCheckSumStr, 16))
                                _gDownStatus++;
                            else
                                _gDownStatus = 0;
                        }
                        string strCheckRet = "checksum is " + Convert.ToString(checkSumUint16, 16) + ":" + _gCheckSumStr;
                        Print_Func(strCheckRet);
                        
                        break;
                    case 7:
                        //ECUReset
                        Print_Func("ECUReset:");
                        Rout_LID = "1103";
                        cmd_msg = Rout_LID;
                        pr_msg = "5103";
                        if (SendCanCommd(cmd_msg) == 0)
                        {
                            _gDownStatus = 0;
                            break;
                        }
                        if (Check_PR(pr_msg) == 0)
                        {
                            _gDownStatus = 0;
                        }
                        else
                            _gDownStatus = 10;


                        break;
                    default:
                        Task.Delay(80).Wait();
                        break;
                }
            }
            VCI_CloseDevice(m_devtype, m_devtype);
            if (_gDownStatus == 10)
            {
                //progressBar1.Value = 100;
                SetProgress("100");
                //label2.Text = 100 + "%";
                Print_Func("Flashing successfully, please fully power down for 10 senconds");
            }
            else if (_gDownStatus == 0)
            {
                Print_Func("Flashing failure, please refer to Datastream file for reason");
            }
            //button2.Enabled = true;
            this.BeginInvoke(new Update(Settext), 0, "");

        }
        private void button2_Click(object sender, EventArgs e)
        {
            //_gMessage.comboBox1Text = comboBox1.Text;
            //_gMessage.comboBox2Text = comboBox2.Text;
            _gMessage.FilePath = textBox1.Text;
            richTextBox1.Clear();
            //_gMessage.CanOrUart = checkBox1.Checked;

                    INIT_REV_CAN_ID = 0x7e8;
                    INIT_SEND_CAN_ID = 0x7e0;

            if (childref == null)
                childref = new ThreadStart(CallToChildThread);

            //创建Thread类的实例
            
            childThread = new Thread(childref);
            childThread.Start(); //开始一个线程
            progressBar1.Value = 0;


            button2.Enabled = false;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
