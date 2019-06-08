using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Kursovaya_Arthur_Laggay_Pi3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        int fp, fa, Ap, Aa, f1, f2, f3, f4, f5, M, Ndisc, m, Fs;
        double fc, delta,N,Tdisc,alpha;
        double[] a, alphaI, Sdisk, Sq, St, h, H;

        private void button1_Click(object sender, EventArgs e)
        {
            Vich();
        }



        // методы начинаются здесь
        void Vich()
        {
            if (!int.TryParse(textBox1.Text, out fp) || fp < 1 || !int.TryParse(textBox2.Text, out fa) || fa < 1 || !int.TryParse(textBox3.Text, out Ap) || Ap < 1 || !int.TryParse(textBox4.Text, out Aa) || Aa < 1 || !int.TryParse(textBox5.Text, out f1) || f1 < 1 || !int.TryParse(textBox6.Text, out f2) || f2 < 1 || !int.TryParse(textBox7.Text, out f3) || f3 < 1 || !int.TryParse(textBox8.Text, out f4) || f4 < 1 || !int.TryParse(textBox9.Text, out f5) || f5 < 1 || !int.TryParse(textBox10.Text, out m) || m <= 0 || !int.TryParse(textBox11.Text, out Fs) || Fs <= 0)
            {
                MessageBox.Show("Ошибка в входных данных"); return;
            }
            listBox1.Items.Clear(); listBox2.Items.Clear(); listBox3.Items.Clear(); listBox4.Items.Clear(); chart1.Series[0].Points.Clear(); chart2.Series[0].Points.Clear(); chart3.Series[0].Points.Clear(); chart4.Series[0].Points.Clear(); chart5.Series[0].Points.Clear(); chart6.Series[0].Points.Clear(); chart7.Series[0].Points.Clear(); chart8.Series[0].Points.Clear();
            double beta1 = (Math.Pow(10, 0.05 * Ap) - 1) / (Math.Pow(10, 0.05 * Ap) + 1);
            double beta2 = Math.Pow(10, -0.05 * Aa);
            delta = Math.Min(beta1, beta2);

            double Bf;


            Bf = fa - fp;
            fc = fp + Bf / 2;

            double A;
            A = -20 * Math.Log10(delta);
            double D;
            if (A <= 21)
            {
                D = 0.92;
            }
            else
            {
                D = (A - 7.95) / 14.36;// A > 21
            }

            M = (int)Math.Ceiling((Fs * D) / Bf);
            label12.Text = M.ToString();
            N = M + 1;

            Kaiser(A);
            Tdisc = 1.0 / m; //Шаг дискретизации

            Ndisc =(int)(1 / Tdisc);//Количество отсчетных значений/////////////////////////////////////
            a = raschet_a();
            alphaI = raschet_alphaI();
            H = raschet_h(alphaI);//Коэффициенты фильтра
            Sdisk = Get_Sk();
            delta = (Sdisk.Max() - Sdisk.Min()) / (m - 1);
            label13.Text = delta.ToString("F4");
            h = raschet_h(Get_Triangle_mass());
            for (int i = 0; i < h.Length; i++) listBox2.Items.Add("[" + (i + 1) + "]\t" + Math.Round(h[i], 6));

            Triangle_window();


            h = raschet_h(Heming_mass());
            for (int i = 0; i < h.Length; i++) listBox3.Items.Add("[" + (i + 1) + "]\t" + Math.Round(h[i], 6));
           

            Heming_window();


            h = raschet_h(Get_Blackman_mass());
            for (int i = 0; i < H.Length; i++)
            { listBox4.Items.Add("[" + (i + 1) + "]\t" + Math.Round(h[i], 6)); listBox1.Items.Add("[" + (i + 1) + "]\t" + Math.Round(H[i], 6)); }
            if (true)
            {

                Blackman_window();

                drawSt();
                Sq = kwant_S();
                for (int i = 0; i < Sq.Length; i++)//Цифровой сигнал
                    chart2.Series[0].Points.AddXY(i, Sq[i]);

                St = Get_S_from_filter();
                for (int i = 0; i < St.Length; i++)//Фильтрованный сигнал
                    chart3.Series[0].Points.AddXY(i, St[i]);


                S_repaired();
                chart4.ChartAreas[0].AxisY.Minimum = 1;
                chart4.ChartAreas[0].AxisY.Maximum = 1.04;
                chart4.ChartAreas[0].AxisY.Interval = 0.005;


                double[] MyHjw = GetHjw();
                for (int i = 0; i < MyHjw.Length; i++)//АЧХ
                { chart4.Series[0].Points.AddXY(i, MyHjw[i]); }
            }
        }
        void Kaiser(double A)
        {
            //параметр окна Кайзера
            if (A <= 21)
            {
                alpha = 0;
            }
            else if (A > 21 && A <= 50)
            {
                alpha = .5842 * Math.Pow(A - 21, .4) + .07886 * (A - 21);
            }
            else alpha = .1102 * (A - 8.7);

        }
        double[] kwant_S()
        {
            double[] mas = new double[Ndisc];
            for (int i = 0; i < mas.Length; i++)
                mas[i] = Math.Floor(Sdisk[i] / delta) * delta;
            return mas;
        }



        double[] Get_Sk()
        {// дискретный сигнал
            double[] Sdisk = new double[Ndisc];
            for (int i = 0; i < Sdisk.Length; i++)
                Sdisk[i] = S(i * Tdisc);
            return Sdisk;
        }
        
        int Factorial(int n)
        {
            int k = n;
            for (int i = 1; i < n; i++) k *= i;
            return k;
        }

        double[] Get_Blackman_mass()
        {
            double[] w = new double[a.Length];
            for (int i = 0; i < w.Length; i++)
                w[i] = (0.42 - 0.5 * Math.Cos(Math.PI * 2 * i / N) + 0.08 * Math.Cos(Math.PI * 4 * i / N)) * a[i];
            return w;
        }


        double I0(double x)
        {// функция Бесселя
            double I = 1;
            for (int k = 1; k <= 10; k++)
                I += Math.Pow(Math.Pow(x / 2, k) / Factorial(k), 2);
            return I;
        }
        double[] raschet_alphaI()
        {// коэффициенты Фурье с помощью окна Кайзера 
            double[] mas = new double[M / 2 + 1];
            for (int i = 0; i < mas.Length; i++)
            {
                double beta = alpha * Math.Sqrt(1 - Math.Pow(2 * i / M, 2));
                double w = I0(beta) / I0(alpha);
                mas[i] = a[i] * w;
            }
            return mas;
        }
        double[] raschet_h(double[] aK)
        {// коэффициенты фильтра
            double[] mas = new double[M];
            for (int i = 0; i <= M / 2 - 1; i++)
                mas[i] = aK[M / 2 - i];

            mas[M / 2] = aK[0];

            for (int i = M / 2 + 1; i < M; i++)
                mas[i] = mas[M - i - 1];

            return mas;
        }





        void Blackman_window()
        {
            chart8.ChartAreas[0].AxisX.Maximum = 10;
            for (double w = 0; w < fp; w += 0.1)
            {
                double Re = 0, Im = 0;
                for (int n = 1; n <= M; n++)
                {
                    Re += h[n - 1] * Math.Cos(n * w);
                    Im += h[n - 1] * Math.Sin(n * w);
                }
                chart8.Series[0].Points.AddXY(w, Math.Sqrt(Math.Pow(Re, 2) + Math.Pow(Im, 2)));
            }
        }






        double[] GetHjw()
        {// Частотная характеристика
            double[] h = raschet_h(alphaI);
            double[] H = new double[fp * 10];

            for (double w = 0, i = 0; i < H.Length; w += .1, i++)
            {
                double Re = 0, Im = 0;
                for (int n = 1; n <= M; n++)
                {
                    Re += h[n - 1] * Math.Cos(n * w);
                    Im += h[n - 1] * Math.Sin(n * w);
                }
                H[(int)i] = Math.Sqrt(Math.Pow(Re, 2) + Math.Pow(Im, 2));
            }
            return H;
        }
       
        double[] Get_S_from_filter()
        {
            double[] mas = new double[Ndisc];
            for (int i = 0; i < mas.Length; i++)
            {
                double S = 0;
                for (int j = 0; j <= i; j++)
                    if (i - j < H.Length)
                        S += Sq[j] * H[i - j];
                mas[i] = S;
            }
            return mas;
        }
      
        void Triangle_window()
        {
            List<double> triag =  new List<double>();
           
            chart6.ChartAreas[0].AxisY.Minimum = 1;
            chart6.ChartAreas[0].AxisY.Maximum = 1.04;
            chart6.ChartAreas[0].AxisY.Interval =0.005;
            chart6.ChartAreas[0].AxisX.Interval = 2.5;
            chart6.ChartAreas[0].AxisX.Maximum = 15;
            for (double w = 0; w < fp; w += 0.1)
            {
                double Re = 0, Im = 0;
                for (int n = 1; n <= M; n++)
                {
                    Re += h[n - 1] * Math.Cos(n * w);
                    Im += h[n - 1] * Math.Sin(n * w);
                }
                triag.Add(Math.Sqrt(Math.Pow(Re, 2) + Math.Pow(Im, 2)));
                chart6.Series[0].Points.AddXY(w, (Math.Sqrt(Math.Pow(Re, 2) + Math.Pow(Im, 2))));
            }
        }

        double[] raschet_a()
        {// коэффициенты Фурье
            double[] mas = new double[M / 2 + 1];
            mas[0] = 2 * fc / m;

            if (M % 2 == 0)
                for (int i = 1; i < mas.Length; i++)
                    mas[i] = Math.Sin(2 * Math.PI * i * fc / m) / (Math.PI * i);
            else
                for (int i = 1; i < mas.Length; i++)
                    mas[i] = Math.Sin(2 * Math.PI * (i - 0.5) * fc / m) / (Math.PI * (i - 0.5));
            return mas;
        }



        double[] Heming_mass()
        {
            double[] w = new double[a.Length];
            double alpha1 = alpha / 4;
            for (int i = 0; i < w.Length; i++)
                w[i] = (alpha1 - (1 - alpha1) * Math.Cos(Math.PI * 2 * i / N)) * a[i];
            return w;
        }
        void Heming_window()
        {
            
            
            
            chart7.ChartAreas[0].AxisX.Maximum = 15;
            for (double w = 0; w < fp; w += 0.1)
            {
                double Re = 0, Im = 0;
                for (int n = 0; n < M; n++)
                {
                    Re += h[n] * Math.Cos(n * w);
                    Im += h[n] * Math.Sin(n * w);
                }
                chart7.Series[0].Points.AddXY(w, (Math.Sqrt(Math.Pow(Re, 2) + Math.Pow(Im, 2))));
            }
        }
       
        
        void S_repaired()
        {
            double[] C = new double[Ndisc / 2];
            double[] fi = new double[C.Length];
            double N = Ndisc;
            double t = 1.0 / St.Length;

            for (int i = 0; i < St.Length; i++)
            {
                C[0] += St[i];
                C[0] *= (1 / N);
            }

            for (int i = 1; i < C.Length; i++)
            {
                double Re = 0, Im = 0;
                for (int j = 0; j < St.Length; j++)
                {
                    Re += St[j] * Math.Cos(2 * Math.PI * i * j / N);
                    Im += St[j] * Math.Sin(2 * Math.PI * i * j / N);
                }
                Re *= (1 / N);
                Im *= (1 / N);
                C[i] = Math.Sqrt(Math.Pow(Re, 2) + Math.Pow(Im, 2));
                fi[i] = Math.Atan(Im / Re);
            }

            for (int i = 0; i < St.Length; i++)
            {//строим восстановленный сигнал
                double res = 0;
                for (int j = 1; j < C.Length - 1; j++)
                    res += 2 * C[j] * Math.Cos(2 * j * Math.PI * i * t + fi[j]);
                res += C[0] + C[C.Length - 1] * Math.Cos((C.Length - 1) * Math.PI * i * t + fi[fi.Length - 1]);
                chart5.Series[0].Points.AddXY(i, res);
            }
        }
        void drawSt() { for (double i = 0; i < 1; i += 0.01) chart1.Series[0].Points.AddXY(i, S(i)); }
        double S(double t) => Math.Cos(2 * Math.PI * f1 * t) + Math.Cos(2 * Math.PI * f2 * t) + Math.Cos(2 * Math.PI * f3 * t) + Math.Cos(2 * Math.PI * f4 * t) + Math.Cos(2 * Math.PI * f5 * t);
        // void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
        double[] Get_Triangle_mass()
        {
            double[] w = new double[a.Length];
            for (int i = 0; i < w.Length; i++)
                w[i] = (1 - i / N) * a[i];
            return w;
        }
    }

}
