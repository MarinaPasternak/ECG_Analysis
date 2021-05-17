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
using Accord.Math.Random;
using Accord.Statistics;
using System.Numerics;

namespace Analyz_EKG
{
    public partial class Form1 : Form
    {
        string csv_file_path = @"C:\Users\Marina\Desktop\диплом\ECG_Analysis\Analyz_EKG\data.csv";
        int[] dataECG;
        double[] dataForExamination;
        double[] dataForExaminationWithNoise;
        int[] step;
        static int n = 500; // к-во точек
        int m = 4; //уровень дистуктеризации
        double R_2 = Math.Pow(2*Math.Log(n),(1/2));



        public Form1()
        {
            InitializeComponent();
        }

        public int parentKsiFunction(double x) 
        {
            if (x >= 0 && x < 1 / 2)
            {
                return 1;
            }
            else if (x >=1 /2 && x < 1) 
            {
                return -1;
            }

            return 0;
        }

        public double[] ksiFunction(int[] t) 
        {
            double[] results = new double[t.Length];
            int j = 0;
            for (j = 0; j < Math.Pow(2,4); j++) 
            {
                results[j] = Math.Pow(2, m / 2) * parentKsiFunction(Math.Pow(2,m)-j);
            }

            return results;
            
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
                if (numsArray[i] < min) {
                    min = numsArray[i];
                }
            }

            return min;
        }


        private void generateDataBtn_Click_1(object sender, EventArgs e)
        {
            Random randomNum = new Random();
            
            int j = 0;
            int a = randomNum.Next(0, 94500);
            int b = a + n;
            int i = 0;

            dataGridView1.RowCount = b - a;
            dataGridView1.ColumnCount = 1;
            
            dataForExamination = new double[b - a];
            dataForExaminationWithNoise = new double[b - a];
            double[] noise = new double[b-a];


            step = new int[b - a];
            
            for (i = a; i < b; i++)
            {
                step[j] = i;
                noise[j] = 2*(randomNum.Next(500, 1000) - 0.5);
                j++;
            }


            dataECG = readECGData();
            Array.Copy(dataECG, a, dataForExamination, 0, b - a);
            

            for (i = 0; i < b-a; i++) 
            {
                dataForExaminationWithNoise[i] = dataForExamination[i] + noise[i];
                dataGridView1.Rows[i].Cells[0].Value = dataForExaminationWithNoise[i];
            }


            double minValue = findMin(dataForExaminationWithNoise); 

            chart1.ChartAreas[0].AxisY.Minimum = minValue;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.Series[0].Points.DataBindXY(step, dataForExaminationWithNoise);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            double minValue = findMin(dataForExamination);

            chart2.ChartAreas[0].AxisY.Minimum = minValue;
            chart2.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart2.Series[0].Points.DataBindXY(step, dataForExamination);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //тестовое значение значение хаара 
            int i = 0;
            double[] waveletArray = new double[dataForExamination.Length];

            dataGridView2.RowCount = dataForExamination.Length;
            dataGridView2.ColumnCount = 1;

            Array.Copy(dataForExamination, 0, waveletArray, 0, dataForExamination.Length);

            IWavelet wavelet = new Accord.Math.Wavelets.Haar(m);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Forward(waveletArray);

            for (i = 0; i < waveletArray.Length; i++)
            {
                dataGridView2.Rows[i].Cells[0].Value = waveletArray[i];
            }
        }
    }
}
