﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Analyz_EKG
{
    public partial class Form1 : Form
    {
        string csv_file_path = @"C:\Users\Marina\Desktop\диплом\Analyz_EKG\data.csv";
        int[] dataECG;
        int[] dataForExamination;
        int[] step;

        public Form1()
        {
            InitializeComponent();
        }

        public int[] readECGData() {
            int[] readRezults;
            string[] stringSeparators = new string[] { "\r\n" };

            StreamReader readfile = new StreamReader(csv_file_path);
            string data = readfile.ReadToEnd();
            readRezults = data.Split(stringSeparators, StringSplitOptions.None).Select(x => int.Parse(x)).ToArray();

            return readRezults;
        }

        public int findMin(int[] numsArray) {
            int min = numsArray[0];
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


        private void generateDataBtn_Click_1(object sender, EventArgs e)
        {
            Random randomNum = new Random();
            int j = 0;
            int a = randomNum.Next(0, 94500);
            int b = a + 500;

            dataForExamination = new int[b - a];
            step = new int[b - a];
            
            for (int i = a; i < b; i++) {
                step[j] = i;
                j++;
            }
            
            dataECG = readECGData();
            Array.Copy(dataECG, a, dataForExamination, 0, b - a);

            int minValue = findMin(dataForExamination);

            chart1.ChartAreas[0].AxisY.Minimum = minValue;
            chart1.ChartAreas[0].AxisX.ScrollBar.IsPositionedInside = true;
            chart1.Series[0].Points.DataBindXY(step, dataForExamination);

        }
    }
}