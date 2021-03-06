﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualDashboard
{
    public partial class Form1 : Form
    {
        //Port that connects to elm327
        static SerialPort OBDPort = new SerialPort();

        //commands to be sent to monitor various aspects of the vehicle
        static String[] commands = {
            "010C", //RPM
            "010D", //Speed
            "010E", //Timing Advance
            "010F", //Intake Air Temp
            "0110", //MAF Air Intake
            "0104", //Engine Load
            "0105"  //Coolant Temp
        };

        String RxString = "";

        public static Gauge [] DashElements = new Gauge[17];

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Get com port names and populate UI Box
            string[] ArrayComPortsNames = null;
            ArrayComPortsNames = SerialPort.GetPortNames();
            ComPort.DataSource = ArrayComPortsNames;

            //Common Baud Rates
            BaudRate.Items.Add(300);
            BaudRate.Items.Add(600);
            BaudRate.Items.Add(1200);
            BaudRate.Items.Add(2400);
            BaudRate.Items.Add(9600);
            BaudRate.Items.Add(14400);
            BaudRate.Items.Add(19200);
            BaudRate.Items.Add(38400);
            BaudRate.Items.Add(57600);
            BaudRate.Items.Add(115200);

            //Add listener to stop second thread when closing
            Form1.ActiveForm.FormClosing += Form1_Closing;
        }

        

        private void ConnectBtn_Click(object sender, EventArgs e)
        {
            OBDPort.PortName = Convert.ToString(ComPort.Text);
            OBDPort.BaudRate = Convert.ToInt32(BaudRate.Text);

            //Manually set the connection settings for elm327
            OBDPort.DataBits = Convert.ToInt16("8");
            OBDPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "1");
            OBDPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
            OBDPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
            try
            {
                OBDPort.Open();
                Console.Write("Port Opened");

                //Send Command and display device type in the form title
                OBDPort.WriteLine("ATI\r");
                Thread.Sleep(1000);
                OBDPort.ReadLine();
                OBDPort.ReadLine();
                string message = OBDPort.ReadLine();
                Form1.ActiveForm.Text = message;

                //Tell Elm to select the proper OBD protocol automatically
                OBDPort.WriteLine("ATSP0\r");
                OBDPort.ReadLine();
                OBDPort.ReadLine();
                OBDPort.ReadLine();

                //Create new monitor object to run on seperate thread
                Monitor mon = new Monitor(OBDPort, commands,this);
                mon.Start();

                //Assign UI elements to array for use in Monitor Class
                DashElements[4] = new Gauge(11, 10, 4, 1920, 1080, 11, "Load", 0, 100);
                DashElements[5] = new Gauge(22.5, 10, 5, 1920, 1080, 11, "Coolant", 0, 215);
                DashElements[12] = new Gauge(34, 10, 12, 1920, 1080, 11, "RPM", 0, 16383);
                DashElements[13] = new Gauge(45.5, 10, 13, 1920, 1080, 11, "Speed (KM/H)", 0, 125);
                DashElements[14] = new Gauge(57, 10, 14, 1920, 1080, 11, "Timing Advance", 0, 100);
                DashElements[15] = new Gauge(68.5, 10, 15, 1920, 1080, 11, "Intake Air Temp", 0, 100);
                DashElements[16] = new Gauge(80, 10, 16, 1920, 1080, 11, "Total Air Intake", 0, 655);
                this.Close();
            }
            catch(UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Monitor.Stop();
        }

    }
}
