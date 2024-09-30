using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
//using System.FileStream;

using System.IO.Ports;
using System.IO;




namespace SerialCommunicate
{
    public partial class Form1 : Form
    {

        int W25Q80_Maximum_Memory = 0x100000;
        int W25Q16_Maximum_Memory = 0x200000;
        int W25Q32_Maximum_Memory = 0x400000;
        int W25Q64_Maximum_Memory = 0x800000;
        int W25Q128_Maximum_Memory = 0x1000000;
        int W25Q256_Maximum_Memory = 0x2000000;
        UInt32 Addr_start = 0x00;                //写起始地址
        int Send_Data_Max = 4096;

        FileStream fs;       //打开文件
        BinaryReader br;             //使用二进制读取
        byte[] dataArray = new byte[4096];                  //获取文件大小

        long ISize = 0;                             //获取文件大小
        byte Flag = 0;
        int data_clock;     //获取多少次
        int data_size;      //最后一个块发送的字节数
        int data_clock_num = 0;

        byte button5_Flag = 0;


        public Form1()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox1.Text;
                serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text,10);//十进制数据转换     配置波特率
                switch (comboBox4.Text)                                   //配置停止位      
                {
                    case "None":            serialPort1.StopBits = StopBits.None;   break;
                    case "One":             serialPort1.StopBits = StopBits.One; break;
                    case "OnePointFive":    serialPort1.StopBits = StopBits.OnePointFive; break;
                    case "Two":             serialPort1.StopBits = StopBits.Two; break;
                    default:                serialPort1.StopBits = StopBits.One; break;
                }
                serialPort1.DataBits = Convert.ToInt32(comboBox5.Text, 10); //配置数据位
                switch (comboBox4.Text)                                   //配置停止位      
                {
                    case "None":    serialPort1.Parity = Parity.None;   break;
                    case "Odd":     serialPort1.Parity = Parity.Odd;    break;
                    case "Even":    serialPort1.Parity = Parity.Even;   break;
                    case "Mark":    serialPort1.Parity = Parity.Mark;   break;
                    case "Space":   serialPort1.Parity = Parity.Space;  break;
                    default:        serialPort1.Parity = Parity.None;   break;
                }

                comboBox2.Enabled = false;
                comboBox4.Enabled = false;
                comboBox5.Enabled = false;
                comboBox6.Enabled = false;


                serialPort1.Open();
                button1.Enabled = false;//打开串口按钮不可用
                button2.Enabled = true;//关闭串口
            }
            catch {
                MessageBox.Show("端口错误,请检查串口", "错误");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 在LinkLable控件中可以添加多个链接
            this.linkLabel1.Links.Add(0, 33, @"https://blog.csdn.net/qq_41906031");
          
            for (int i = 1; i < 20; i++)
            {
                comboBox1.Items.Add("COM" + i.ToString());
            }
            comboBox1.Text = "COM0";//串口号多额默认值
            comboBox2.Text = "115200";//波特率默认值
            comboBox4.Text = "One";
            comboBox5.Text = "8";
            comboBox6.Text = "None";

            comboBox3.Text = "W25Q32";      //默认写W25Q32
            
            /*****************非常重要************************/
            
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);//必须手动添加事件处理程序

        }
/*-----------------------------------------------------------------串口相关函数-----------------------------------------------------------------*/
        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)//串口数据接收事件
        {
            if (!radioButton3.Checked)//如果接收模式为字符模式
            {
                string str = serialPort1.ReadExisting();//字符串方式读
                textBox1.AppendText(str);//添加内容
                if (button5_Flag == 1 && str.EndsWith("A"))
                {
                    if (data_clock_num < data_clock)
                    {
                        try
                        {
                            int nBytesRead = br.Read(dataArray, 0, Send_Data_Max);   //  连续读取一个扇区
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("文件发送错误", "错误");//出错提示
                        }
                        try
                        {
                            serialPort1.Write(dataArray, 0, Send_Data_Max);//发送数据
                            textBox1.AppendText("\\" + data_clock_num.ToString());
                            progressBar1.Value = data_clock_num;
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("串口发送错误", "错误");//出错提示
                        }
                        data_clock_num++;
                    }
                    else 
                    {
                        try
                        {
                            int nBytesRead = br.Read(dataArray, 0, data_size);   //  连续读取一个扇区
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("文件发送错误", "错误");//出错提示
                        }
                        try
                        {
                            serialPort1.Write(dataArray, 0, data_size);////发送最后一个文件块
                            progressBar1.Value = (int)(ISize / Send_Data_Max);
                            textBox1.AppendText("nn");
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("串口发送错误", "错误");//出错提示
                        }
                        MessageBox.Show("文件发送完毕", "提示");//文件发送完毕提示
                        button5_Flag = 0;

                        br.Close();
                        fs.Close();     //关闭流并释放与它相关的资源


                    }


                }

            }
            else { //如果接收模式为数值接收
                byte data;
                data = (byte)serialPort1.ReadByte();//此处需要强制类型转换，将(int)类型数据转换为(byte类型数据，不必考虑是否会丢失数据

                string str = Convert.ToString(data, 16).ToUpper();//转换为大写十六进制字符串

                textBox1.AppendText("0x" + (str.Length == 1 ? "0" + str : str) + " ");//空位补“0”



                //上一句等同为：if(str.Length == 1)
                //                  str = "0" + str;
                //              else 
                //                  str = str;
                //              textBox1.AppendText("0x" + str);
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.Close();//关闭串口
                button1.Enabled = true;//打开串口按钮可用
                button2.Enabled = false;//关闭串口按钮不可用

                comboBox2.Enabled = true;
                comboBox4.Enabled = true;
                comboBox5.Enabled = true;
                comboBox6.Enabled = true;
            }
            catch (Exception err)//一般情况下关闭串口不会出错，所以不需要加处理程序
            {

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] Data = new byte[1];//作用同上集
            if (serialPort1.IsOpen)//判断串口是否打开，如果打开执行下一步操作
            {
                if (textBox2.Text != "")
                {
                    if (!radioButton1.Checked)//如果发送模式是字符模式
                    {
                        try
                        {
                            serialPort1.WriteLine(textBox2.Text);//写数据
                        }
                        catch (Exception err)
                        {
                            MessageBox.Show("串口数据写入错误", "错误");//出错提示
                            serialPort1.Close();
                            button1.Enabled = true;//打开串口按钮可用
                            button2.Enabled = false;//关闭串口按钮不可用
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (textBox2.Text.Length - textBox2.Text.Length % 2) / 2; i++)//取余3运算作用是防止用户输入的字符为奇数个
                        {
                            Data[0] = Convert.ToByte(textBox2.Text.Substring(i * 2, 2), 16);
                            serialPort1.Write(Data, 0, 1);//循环发送（如果输入字符为0A0BB,则只发送0A,0B）
                        }
                        if (textBox2.Text.Length % 2 != 0)//剩下一位单独处理
                        {
                            Data[0] = Convert.ToByte(textBox2.Text.Substring(textBox2.Text.Length-1, 1), 16);//单独发送B（0B）
                            serialPort1.Write(Data, 0, 1);//发送
                        }
                   }
                }
            }
        }

        private void SearchAndAddSerialToComboBox(SerialPort MyPort, ComboBox MyBox)                /*烧苗串口*/
        {                                                               //将可用端口号添加到ComboBox
            string[] MyString = new string[20];                         //最多容纳20个，太多会影响调试效率
            string Buffer;                                              //缓存
            MyBox.Items.Clear();                                        //清空ComboBox内容
            MyBox.Items.Add("Search");
            for (int i = 1; i < 20; i++)                                //循环
            {
                try                                                     //核心原理是依靠try和catch完成遍历
                {
                    Buffer = "COM" + i.ToString();
                    MyPort.PortName = Buffer;
                    MyPort.Open();                                      //如果失败，后面的代码不会执行
                    MyString[i - 1] = Buffer;
                    MyBox.Items.Add(Buffer);                            //打开成功，添加至下俩列表
                    MyPort.Close();                                     //关闭
                }
                catch
                {

                }
            }
            MyBox.Text = MyString[0];                                   //初始化
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(comboBox1.Text == "Search")
            {
                SearchAndAddSerialToComboBox(serialPort1, comboBox1);       //搜索串口
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();          //创建文件夹对象
            ofd.ShowDialog();                                   //实现对话框的显示
            string path = ofd.FileName;                         //获取选择文件的绝对路径
            textBox5.Text = path;
            Addr_start = (uint)Convert.ToInt32(textBox3.Text, 16);            //将0x开头的十六进制字符串转换为整数
            if (File.Exists(path))
                ISize = new FileInfo(path).Length;
            
            textBox4.Text = "0x" + (ISize + Addr_start).ToString("x");

            if ((comboBox3.Text == "W25Q80") && ((ISize + Addr_start) > W25Q80_Maximum_Memory))
                    MessageBox.Show("文件超出内存", "错误");//出错提示
            else if ((comboBox3.Text == "W25Q16") && ((ISize + Addr_start) > W25Q16_Maximum_Memory))
                MessageBox.Show("文件超出内存", "错误");//出错提示
            else if ((comboBox3.Text == "W25Q32") && ((ISize + Addr_start) > W25Q32_Maximum_Memory))
                MessageBox.Show("文件超出内存", "错误");//出错提示
            else if ((comboBox3.Text == "W25Q64") && ((ISize + Addr_start) > W25Q64_Maximum_Memory))
                MessageBox.Show("文件超出内存", "错误");//出错提示
            else if ((comboBox3.Text == "W25Q128") && ((ISize + Addr_start) > W25Q128_Maximum_Memory))
                MessageBox.Show("文件超出内存", "错误");//出错提示
            else if ((comboBox3.Text == "W25Q256") && ((ISize + Addr_start) > W25Q256_Maximum_Memory))
                MessageBox.Show("文件超出内存", "错误");//出错提示

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "")
            {
                MessageBox.Show("请选择文件要写入的文件", "应用提示");
            }
            else
            {
                button5_Flag = 1;                       //写入文件按键按下标志

                if (button1.Enabled == false)
                {
                    //path
                    byte[] data = new byte[4];
                    data[0] = 0xFE;
                    data[1] = 0xFF;
                    serialPort1.Write(data, 0, 2);          //发送数据起始位


                    Addr_start = Convert.ToUInt32(textBox3.Text, 16);
                    data[0] = (byte)(Addr_start >> 0);
                    data[1] = (byte)(Addr_start >> 8);
                    data[2] = (byte)(Addr_start >> 16);
                    data[3] = (byte)(Addr_start >> 24);

                    serialPort1.Write(data, 0, 4);          //发送数据起始地址

                    //ISize = Convert.ToUInt32(textBox4.Text, 16);
                    data[0] = (byte)(ISize >> 0);
                    data[1] = (byte)(ISize >> 8);
                    data[2] = (byte)(ISize >> 16);
                    data[3] = (byte)(ISize >> 24);
                    serialPort1.Write(data, 0, 4);          //发送文件长度

                    data[0] = (byte)0xFe;
                    serialPort1.Write(data, 0, 1);          //发送数据结束位

                    progressBar1.Value = 0;
                    progressBar1.Minimum = 0;                   //配置文件进度条
                    progressBar1.Maximum = (int)(ISize/ Send_Data_Max);

                    data_clock_num = 0;

                    textBox1.AppendText("Start: " + Addr_start.ToString("x") + " " + ISize.ToString("x") + " ");
                    string filepath = textBox5.Text;        //获取文件名路径和名称

                    if (!File.Exists(filepath))     //如果文件不存在，就提示错误
                    {
                        Console.WriteLine("\n\t读取失败！\n错误原因：可能不存在此文件");
                        return;
                    }
                    else
                    {
                        fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);       //打开文件
                        br = new BinaryReader(fs);             //使用二进制读取


                        data_clock = (int)ISize / Send_Data_Max;     //获取多少次
                        data_size = (int)ISize % Send_Data_Max;      //最后一个块发送的字节数
                    }
                }
                else
                    MessageBox.Show("请打开串口", "错误");//出错提示

            }

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try                                                     //防止用户输入错误的数据
            {
                textBox4.Text = "0x" + ((uint)Convert.ToInt32(textBox3.Text, 16) + ISize).ToString("x");
            }
            catch
            {
                MessageBox.Show("请输入十六进制数，以 0x 开头", "错误");//出错提示
            }


        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click_1(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.linkLabel1.Links[this.linkLabel1.Links.IndexOf(e.Link)].Visited = true;
            string targetUrl = e.Link.LinkData as string;
            if (string.IsNullOrEmpty(targetUrl))
                MessageBox.Show("没有链接地址！");
            else
                System.Diagnostics.Process.Start("iexplore.exe", targetUrl);
        }

        private void label11_Click(object sender, EventArgs e)
        {

        }
    }
}
