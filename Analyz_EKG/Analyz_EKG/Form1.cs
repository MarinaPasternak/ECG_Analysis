using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Accord.Math;
using Accord.Math.Wavelets;
using Accord.Imaging.Filters;
using System.Numerics;

namespace Analyz_EKG
{
    public partial class Form1 : Form
    {
        string csv_file_path = @"C:\Users\Marina\Desktop\диплом\ECG_Analysis\Analyz_EKG\data.csv";
        int[] dataECG;
        double[] dataForExamination;
        int[] step;


        public Form1()
        {
            InitializeComponent();
        }

        public int[] readECGData() 
        {
            int[] readRezults;
            string[] stringSeparators = new string[] { "\r\n" };

            StreamReader readfile = new StreamReader(csv_file_path);
            string data = readfile.ReadToEnd();
            readRezults = data.Split(stringSeparators, StringSplitOptions.None).Select(x => int.Parse(x)).ToArray();

            
            return readRezults;
        }

        public double findMin(double[] numsArray)
        {
            double min = numsArray[0];
            int lengthArray = numsArray.Length;
            int i = 0;

            for (i = 0; i < lengthArray; i++)
            {
                if (numsArray[i] < min)
                {
                    min = numsArray[i];
                }
            }

            return min;
        }

        public double findSqtwolog() 
        {
            double T = 0;

            return T;
        }


        private void generateDataBtn_Click_1(object sender, EventArgs e)
        {
            Random randomNum = new Random();
            int j = 0;
            int a = randomNum.Next(0, 94500);
            int b = a + 500;
            int i = 0;

            dataGridView1.RowCount = b - a;
            dataGridView1.ColumnCount = 1;

            dataForExamination = new double[b - a];
            

            step = new int[b - a];
            
            for (i = a; i < b; i++)
            {
                step[j] = i;
                j++;
            }
            
            dataECG = readECGData();
            Array.Copy(dataECG, a, dataForExamination, 0, b - a);
            

            for (i = 0; i < b-a; i++) 
            {
                dataGridView1.Rows[i].Cells[0].Value = dataForExamination[i];
            }

            double minValue = findMin(dataForExamination); 

            chart1.ChartAreas[0].AxisY.Minimum = minValue;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.Series[0].Points.DataBindXY(step, dataForExamination);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            int i = 0;
            double[] waveletArray = new double[dataForExamination.Length];

            dataGridView2.RowCount = dataForExamination.Length;
            dataGridView2.ColumnCount = 1;

            Array.Copy(dataForExamination, 0, waveletArray, 0, dataForExamination.Length);

            IWavelet wavelet = new Accord.Math.Wavelets.Haar(4);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Forward(waveletArray);

            for (i = 0; i < waveletArray.Length; i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = waveletArray[i];
            }
        }
    }
}
