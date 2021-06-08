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
        //шлях до файлу
        string csv_file_path = @"C:\Users\Marina\Desktop\диплом\ECG_Analysis\Analyz_EKG\ecg_data_1.csv";
        double[] dataECG; // дані з цсв

        double[] dataForExamination; //обрані 500 точок

        double[] dij;

        double R_2; // поріг шумур
        double T; // рівень шуму

        int[] step; // час

        double[] wavlet;
        double[] wavletInversHard; // вейвлет для жорсткого порогу 
        double[] wavletInversSoft; // вейвлет для м'якого порогу 

        double[] deNoisedSignalHard; // сигнал для жорсткого порогу
        double[] deNoisedSignalSoft; // сигнал для м'якого порогу

        double SNRsoft = 0; //показник шум до сигналу для м'якого порогу
        double SNRhard = 0; //показник шум до сигналу для жорсткого порогу

        static int n = 500; // к-во точек
        static int n2 = 100; // к-во точек
        int m = 4; //уровень дистуктеризации

        public Form1()
        {
            InitializeComponent();
            deleteNoise.Visible = false;
            hideElement();
        }

        public void hideElement()
        {
            dataGridView1.Visible = false;

            chart2.Visible = false;
            chart3.Visible = false;
            chart5.Visible = false;
            chart6.Visible = false;

            label2.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            label5.Visible = false;
            label6.Visible = false;
            label7.Visible = false;
            label8.Visible = false;
            label9.Visible = false;
            label10.Visible = false;
            label11.Visible = false;
            label13.Visible = false;
            label14.Visible = false;
        }

        public void showElement()
        {
            dataGridView1.Visible = true;

            chart2.Visible = true;
            chart3.Visible = true;
            chart5.Visible = true;
            chart6.Visible = true;

            label2.Visible = true;
            label3.Visible = true;
            label4.Visible = true;
            label5.Visible = true;
            label6.Visible = true;
            label7.Visible = true;
            label8.Visible = true;
            label9.Visible = true;
            label10.Visible = true;
            label11.Visible = true;
            label13.Visible = true;
            label14.Visible = true;
        }

        public double[] readECGData()
        {
            double[] readRezults;
            string[] stringSeparators = new string[] { "\r\n" };

            StreamReader readfile = new StreamReader(csv_file_path);
            string data = readfile.ReadToEnd();
            readRezults = data.Split(stringSeparators, StringSplitOptions.None).Select(x => double.Parse(x)).ToArray();


            return readRezults;
        }

        public double findMin(double[] numsArray)
        {
            double min = numsArray[0];
            int lengthArray = numsArray.Length;
            int i = 0;

            for (i = 0; i < lengthArray; i++)
            {
                if (numsArray[i] <= min)
                {
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
                    if (array[i] < array[j])
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
            return array[array.Length / 2];
        }

        public double dispersion(double[] array)
        {
            double despersionArray = 0;
            double summ = 0;
            double average = 0;

            int i = 0;

            for (i = 0; i < array.Length; i++)
            {
                summ += array[i];
            }

            average = summ / array.Length;

            for (i = 0; i < array.Length; i++)
            {
                despersionArray += Math.Pow((array[i] - average), 2) / array.Length;
            }

            return despersionArray;

        }

        public void feelTable(double[] wavlet, double[] dij, double[] signal, double[] denoiseHard, double[] denoiseSoft)
        {
            int i = 0;

            dataGridView1.RowCount = n;
            dataGridView1.ColumnCount = 6;

            dataGridView1.Columns[0].HeaderText = "ЕКГ";
            dataGridView1.Columns[1].HeaderText = "Вейвлет Хаара";
            dataGridView1.Columns[2].HeaderText = "dij";
            dataGridView1.Columns[3].HeaderText = "Жорсткий поріг";
            dataGridView1.Columns[4].HeaderText = "М'який поріг";

            for (i = 0; i < n; i++)
            {
                dataGridView1.Rows[i].Cells[0].Value = signal[i];
                dataGridView1.Rows[i].Cells[1].Value = wavlet[i];
            }

            for (i = 0; i < dij.Length; i++)
            {
                dataGridView1.Rows[i].Cells[2].Value = dij[i];
            }


            for (i = 0; i < denoiseHard.Length; i++)
            {
                dataGridView1.Rows[i].Cells[3].Value = denoiseHard[i];
            }

            for (i = 0; i < denoiseSoft.Length; i++)
            {
                dataGridView1.Rows[i].Cells[4].Value = denoiseSoft[i];
            }


        }

        public void wavletHaarForward()
        {
            wavlet = new double[n];
            dij = new double[n / 2];

            Array.Copy(dataForExamination, 0, wavlet, 0, dataForExamination.Length);

            IWavelet wavelet = new Accord.Math.Wavelets.Haar(m);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Forward(wavlet);

            Array.Copy(wavlet, n / 2, dij, 0, dij.Length / 2);
        }

        public void wavletHaartInverseHard(double[] w)
        {
            deNoisedSignalHard = new double[w.Length];

            Array.Copy(w, 0, deNoisedSignalHard, 0, w.Length);
            IWavelet wavelet = new Accord.Math.Wavelets.Haar(m);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Backward(deNoisedSignalHard);
        }

        public void wavletHaartInverseSoft(double[] w)
        {
            deNoisedSignalSoft = new double[w.Length];

            Array.Copy(w, 0, deNoisedSignalSoft, 0, w.Length);
            IWavelet wavelet = new Accord.Math.Wavelets.Haar(m);
            WaveletTransform target = new WaveletTransform(wavelet);
            wavelet.Backward(deNoisedSignalSoft);
        }

        public double[] hardThreshold(double T, double[] array)
        {
            int i = 0;

            for (i = 0; i < array.Length; i++)
            {
                if (Math.Abs(array[i]) < T)
                {
                    array[i] = 0;
                }

            }

            return array;

        }

        public double[] softThreshold(double T, double[] array)
        {
            int i = 0;

            for (i = 0; i < array.Length; i++)
            {
                if (Math.Abs(array[i]) >= T)
                {
                    array[i] = Math.Abs(array[i]) - Math.Sign(array[i]) * T;
                }

                else
                {
                    array[i] = 0;
                }

            }

            return array;

        }

        public double SNR(double[] signal, double[] signalWithNoise)
        {
            double SNRvalue = 0;

            int i = 0;
            int len = signal.Length;

            for (i = 0; i < len; i++)
            {
                SNRvalue = Math.Pow(signalWithNoise[i], 2) / Math.Pow(signalWithNoise[i] - signal[i], 2);
            }

            SNRvalue = 10 * Math.Log10(SNRvalue);

            return SNRvalue;

        }

        private void generateDataBtn_Click_1(object sender, EventArgs e)
        {
            hideElement();

            Random randomNum = new Random();

            int j = 0;
            int a = randomNum.Next(0, 2000);
            int b = a + n;
            int i = 0;

            double[] dataSubset = new double[n2];
            int[] stepForSubset = new int[n2];
            dataForExamination = new double[n];

            step = new int[n];

            for (i = a; i < b; i++)
            {
                step[j] = i;
                j++;
            }


            dataECG = readECGData();
            Array.Copy(dataECG, a, dataForExamination, 0, n);
            Array.Copy(dataForExamination, 0, dataSubset, 0, n2);
            Array.Copy(step, 0, stepForSubset, 0, n2);

            double minValue = findMin(dataForExamination);

            chart1.ChartAreas[0].AxisY.Minimum = minValue;
            chart1.Series[0].Points.DataBindXY(step, dataForExamination);

            minValue = findMin(dataSubset);

            chart4.ChartAreas[0].AxisY.Minimum = minValue;
            chart4.Series[0].Points.DataBindXY(stepForSubset, dataSubset);

            wavletHaarForward();
            deleteNoise.Visible = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.Visible = true;
            chart2.Visible = true;
            chart3.Visible = true;

            double minValue = findMin(dataForExamination);
            double medianDij = 0;

            double[] diff = new double[n / 2];

            double medianOfDiff = 0;
            double dispersionSignal;

            double[] denoisedSoftSubset = new double[n2];
            double[] denoisedHardSubset = new double[n2];

            int[] denoisedSoftSubsetStep = new int[n2];
            int[] denoisedHardSubsetStep = new int[n2];

            int i = 0;

            dij = bubbleSort(dij);
            medianDij = median(dij);

            for (i = 0; i < n / 2; i++)
            {
                diff[i] = Math.Abs(dij[i] - medianDij);
            }

            diff = bubbleSort(diff);
            medianOfDiff = median(diff);

            R_2 = medianOfDiff / 0.6475;

            dispersionSignal = dispersion(dataForExamination);

            T = dispersionSignal * Math.Sqrt(2 * Math.Log(n));

            label9.Text = "" + Math.Round(dispersionSignal, 4);
            label10.Text = "" + Math.Round(T, 4);

            wavletInversHard = hardThreshold(T, wavlet);
            wavletHaartInverseHard(wavletInversHard);

            wavletInversSoft = softThreshold(T, wavlet);
            wavletHaartInverseSoft(wavletInversSoft);

            SNRsoft = SNR(deNoisedSignalSoft, dataForExamination);
            SNRhard = SNR(deNoisedSignalHard, dataForExamination);

            label7.Text = "" + Math.Round(SNRhard, 4);
            label11.Text = "" + Math.Round(SNRsoft, 4);

            feelTable(wavlet, dij, dataForExamination, deNoisedSignalHard, deNoisedSignalSoft);

            Array.Copy(deNoisedSignalHard, 0, denoisedHardSubset, 0, n2);
            Array.Copy(step, 0, denoisedHardSubsetStep, 0, n2);

            Array.Copy(deNoisedSignalSoft, 0, denoisedSoftSubset, 0, n2);
            Array.Copy(step, 0, denoisedSoftSubsetStep, 0, n2);

            minValue = findMin(deNoisedSignalHard);

            chart2.ChartAreas[0].AxisY.Minimum = minValue;
            chart2.Series[0].Points.DataBindXY(step, deNoisedSignalHard);

            minValue = findMin(deNoisedSignalSoft);

            chart3.ChartAreas[0].AxisY.Minimum = minValue;
            chart3.Series[0].Points.DataBindXY(step, deNoisedSignalSoft);

            minValue = findMin(denoisedHardSubset);

            chart5.ChartAreas[0].AxisY.Minimum = minValue;
            chart5.Series[0].Points.DataBindXY(denoisedHardSubsetStep, denoisedHardSubset);

            minValue = findMin(denoisedSoftSubset);

            chart6.ChartAreas[0].AxisY.Minimum = minValue;
            chart6.Series[0].Points.DataBindXY(denoisedSoftSubsetStep, denoisedSoftSubset);

            deleteNoise.Visible = false;
            showElement();

        }

     
    }
}
