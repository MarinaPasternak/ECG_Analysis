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

        double[] dataForExamination; //обрані 500 точок
        double[] dataForExaminationWithNoise; // сигнал із шумом
        double[] dij;

        double R_2;
        double T;

        int[] step;
        double[] wavlet;
        double[] wavletInversHard;
        double[] wavletInversSoft;
        double[] deNoisedSignalHard;
        double[] deNoisedSignalSoft;

        static int n = 500; // к-во точек
        int m = 4; //уровень дистуктеризации

        //hard threshold
        //soft threshold

        public Form1()
        {
            InitializeComponent();
        }

        public int parentKsiFunction(double x) 
        {
            if (0 <= x && x <= 0.5)
            {
                return 1;
            }

            else if (0.5 < x && x <= 1)
            {
                return -1;
            }

            return 0;
        }

        public double[] ksiFunction(int k, int j) 
        {
            double[] result = new double[n];
            double[] x = new double[n];
            int i = 0;

            for (i = 0; i < x.Length; i++)
            {
                x[i] = i * 0.001;
            }

            for (i = 0; i < x.Length; i++)
            {
                result[i] = (Math.Pow(2, j / 2)) * parentKsiFunction(Math.Pow(2, j) * x[i] - k);
            }
            return result;

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
                if (numsArray[i] <= min) {
                    min = numsArray[i];
                }
            }

            return min;
        }

        public double[] bubbleSort(double[] array) 
        {
            double temp;
            int i = 0;

            for (i = 0; i < array.Length - 1; i++)
            {
                for (int j = i + 1; j < array.Length; j++)
                {
                    if (array[i] > array[j])
                    {
                        temp = array[i];
                        array[i] = array[j];
                        array[j] = temp;
                    }
                }
            }

            return array;
        }

        public double median(double[] array)
        {
            return array[array.Length/2];
        }

        public void feelTable(double[] wavlet, double[] dij, double[] signal, double[] denoisedHard)
        {
            int i = 0;

            dataGridView1.RowCount = n;
            dataGridView1.ColumnCount = 5;

            dataGridView1.Columns[0].HeaderText = "ЕКГ";
            dataGridView1.Columns[1].HeaderText = "Вейвлет Хаара";
            dataGridView1.Columns[2].HeaderText = "dij";
            dataGridView1.Columns[3].HeaderText = "Жорсткий поріг";

            for (i = 0; i < n; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = signal[i];
                dataGridView1.Rows[i].Cells[1].Value = wavlet[i];
            }

            for (i = 0; i < dij.Length; i++)
            {
                dataGridView1.Rows[i].Cells[2].Value = dij[i];
            }


            for (i = 0; i < denoisedHard.Length; i++)
            {
                dataGridView1.Rows[i].Cells[3].Value = denoisedHard[i];
            }


        }

        public void wavletHaarForward() 
        {
            wavlet = new double[n];
            dij = new double[n/2];

            Array.Copy(dataForExaminationWithNoise, 0, wavlet, 0, dataForExaminationWithNoise.Length);
            
            IWavelet wavelet = new Accord.Math.Wavelets.Haar(m);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Forward(wavlet);

            Array.Copy(wavlet, n/2, dij, 0, dij.Length/2);
        }

        public void wavletHaartInverse(double[] array) 
        {
            deNoisedSignalHard = new double[array.Length];

            Array.Copy(array, 0, deNoisedSignalHard, 0, array.Length);
            IWavelet wavelet = new Accord.Math.Wavelets.Haar(m);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Backward(deNoisedSignalHard);
        }

        public double[] hardThreshold(double T, double[] array)
        {
            List<double> result = new List<double>();
            int i = 0;

            for (i = 0; i < array.Length; i++) {
                if (array[i] > T) 
                {
                    result.Add(array[i]);
                }
            
            }

            return result.ToArray();
        
        }

        private void generateDataBtn_Click_1(object sender, EventArgs e)
        {
            Random randomNum = new Random();
            
            int j = 0;
            int a = randomNum.Next(0, 94500);
            int b = a + n;
            int i = 0;

            double[] noise = new double[n];
            dataForExamination = new double[n];
            dataForExaminationWithNoise = new double[n];


            step = new int[n];
            
            for (i = a; i < b; i++)
            {
                step[j] = i;
                noise[j] = (randomNum.Next(500, 1000) - 0.5)* randomNum.Next(0, 3);
                j++;
            }


            dataECG = readECGData();
            Array.Copy(dataECG, a, dataForExamination, 0, n);
            

            for (i = 0; i < b - a; i++) 
            {
                dataForExaminationWithNoise[i] = dataForExamination[i] + noise[i];
            }


            double minValue = findMin(dataForExaminationWithNoise);
            

            chart1.ChartAreas[0].AxisY.Minimum = minValue;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.Series[0].Points.DataBindXY(step, dataForExaminationWithNoise);

            wavletHaarForward();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            double minValue = findMin(dataForExamination);
            double medianDij = 0;
            double[] diff = new double[n / 2];
            double medianOfDiff = 0;

            int i = 0;

            dij = bubbleSort(dij);
            medianDij = median(dij);

            for (i = 0; i < n/2; i++) 
            {
                diff[i] = Math.Abs(dij[i] - medianDij);
            
            }

            diff = bubbleSort(diff);
            medianOfDiff = median(diff);

            R_2 = medianOfDiff / 0.6475;
            T = R_2 * Math.Sqrt(2*Math.Log(n));

            label9.Text = "" + Math.Round(R_2, 4);
            label10.Text = "" + Math.Round(T, 4);

            wavletInversHard = hardThreshold(T, wavlet);
            wavletHaartInverse(wavletInversHard);

            feelTable(wavlet, dij, dataForExaminationWithNoise, deNoisedSignalHard);

            chart2.ChartAreas[0].AxisY.Minimum = minValue;
            chart2.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart2.Series[0].Points.DataBindXY(step, dataForExamination);

            chart3.ChartAreas[0].AxisY.Minimum = minValue;
            chart3.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart3.Series[0].Points.DataBindXY(step, deNoisedSignalHard);
        }

        
    }
}
