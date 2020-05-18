using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Comput_Method_2
{
  public partial class Form1 : Form
  {
    double[,] res_A;
    double[,] res_B;
    double L, T,  t, h, bCoef1, bCoef2, bCoef3, phiCoef1, phiCoef2;

    int hAmount, tAmount;
    int portion;

    double timeForA, timeForB;

    public Form1()
    {
      InitializeComponent();
      ChartArea chart = chart1.ChartAreas[0];
      chart.AxisX.RoundAxisValues();
      progressBar1.Minimum = 0;
      progressBar1.Value = 0;
      progressBar1.Step = 1;
    }

    private void TextBoxesDouble_TextFormat(object sender, KeyPressEventArgs e)
    {
      if (!((e.KeyChar >= '0' && e.KeyChar <= '9')
        || e.KeyChar == ',' || e.KeyChar == '.' || e.KeyChar == 8))
      {
        e.Handled = true;
        return;
      }
      if (e.KeyChar == '.')
      {
        e.KeyChar = ',';
      }
    }

    private void TextBoxesInt_TextFormat(object sender, KeyPressEventArgs e)
    {
      if (!((e.KeyChar >= '0' && e.KeyChar <= '9')
        || e.KeyChar == 8))
      {
        e.Handled = true;
        return;
      }
    }

    private void Form1_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
      {
        e.Handled = true;
        SelectNextControl(ActiveControl, true, true, true, true);
      }
      if (e.KeyCode == Keys.A)  // Space
      {
        e.Handled = true;
        if (label12.Text == "")
        {
          MessageBox.Show(" Часть А не может быть выполнено до части B!\n Выполните сначала её, сделав рассчет!\n");
          return;
        }
        string part_a = "W(x, T)";
        if (chart1.Series[part_a].Enabled == false)
        {
          progressBar1.Value = 0;
          res_A = new double[hAmount, tAmount];
          portion = 0;
          backgroundWorker1.RunWorkerAsync();
        }
      }
    }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label11_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
    {
      if(backgroundWorker1.IsBusy != true)
      {
        label10.Text = "";
        label12.Text = "";

        progressBar1.Value = 0;
        chart1.Series.Clear();
        chart1.Legends.Clear();

        L = double.Parse(textBox1.Text);
        T = double.Parse(textBox2.Text);
        tAmount = int.Parse(textBox3.Text);
        hAmount = int.Parse(textBox4.Text);
        phiCoef1 = double.Parse(textBox5.Text);
        phiCoef2 = double.Parse(textBox6.Text);
        bCoef1 = double.Parse(textBox7.Text);
        bCoef2 = double.Parse(textBox8.Text);
        bCoef3 = double.Parse(textBox9.Text);
        h = L / (hAmount - 1);
        t = T / (tAmount - 1);
        if (t / (h * h) < 1.0 / 4)
        {
          MessageBox.Show("Ошибка!\n Должно выполняться условие устойчивости:\n (Время)/(Длина)^2 < 0.25");
          return;
        }
        chart1.ChartAreas[0].AxisX.Minimum = 0;
        chart1.ChartAreas[0].AxisX.Maximum = L;
        res_B = new double[hAmount, tAmount];

        progressBar1.Maximum = tAmount;

        portion = 1;
        backgroundWorker1.RunWorkerAsync();
      }
    }

    private void BackgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
    {
      BackgroundWorker worker = sender as BackgroundWorker;


      if (portion == 1)
        res_B = CalculationResult();
      else
        res_A = CalculationResult();
    }

    private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      if (portion == 1)
      {
        PortionB();
        label12.Text = (timeForB).ToString();
      }
      else
      {
        PortionA();
        label10.Text = (timeForA).ToString();
      }
    }

    private double PhiFunction(double x)
    {
      return (1.0 / L) + phiCoef1 * Math.Cos((Math.PI * x) / L) + phiCoef2 * Math.Cos((2 * Math.PI * x) / L);
    }
    
    public double BFunction(double x)
    {
      return portion * bCoef1 + bCoef2 * Math.Cos((Math.PI * x) / L) + bCoef3 * Math.Cos((2 * Math.PI * x) / L);
    }

    private double MethodSimpson(ref double[,] res, int tstep, int portion)
    {
      int coeff;
      double I;
      double current_x = h;

      if (portion == 1)
      {
        I = BFunction(0) * res[0, tstep];
        for (int i = 1; i < hAmount - 1; ++i)
        {
          coeff = 4;
          if (i % 2 == 0) coeff = 2;
          I += BFunction(current_x) * coeff * res[i, tstep];
          current_x += h;
        }
        I += BFunction(current_x) * res[hAmount - 1, tstep];
      }
      else
      {
        I = res[0, tstep];
        for (int i = 1; i < hAmount - 1; i++)
        {
          coeff = 4;
          if (i % 2 == 0) coeff = 2;
          I += coeff * res[i, tstep];
          current_x += h;
        }
        I += res[hAmount - 1, tstep];
      }
      I *= (h / 3);
      return I;
    }

    private double[] Fcount(ref double[,] res, int tstep, int portion)
    {
      double[] F = new double[hAmount];
      for (int i = 0; i < hAmount; i++)
        F[i] = h * h * res[i, tstep] * (t * (BFunction(h * i) - portion * MethodSimpson(ref res, tstep, portion)) + 1);
      return F;
    }

    public double[,] CalculationResult()
    {
      double[] F;
      double[] alpha = new double[hAmount];
      double[] beta = new double[hAmount];
      double[,] res = new double[hAmount, tAmount];

      var timer = System.Diagnostics.Stopwatch.StartNew();

      double current_x = 0;
      for (int i = 0; i < hAmount; i++)
      {
        res[i, 0] = PhiFunction(current_x);
        current_x += h;
      }

      int K = hAmount;
      double B, C0, Ak, Ck, AK;
      B = h * h + 2 * t;
      C0 = AK = -2 * t;
      Ak = Ck = -1 * t;

      for (int tstep = 0; tstep < tAmount - 1; tstep++)
      {
        F = Fcount(ref res, tstep, portion);

        alpha[0] = -1 * (C0 / B);
        beta[0] = F[0] / B;

        for (int i = 1; i < K - 1; i++)
        {
          alpha[i] = (-1 * Ck) / (Ak * alpha[i - 1] + B);
          beta[i] = (F[i] - Ak * beta[i - 1]) / (Ak * alpha[i - 1] + B);
        }

        res[K - 1, tstep + 1] = (F[K - 1] - AK * beta[K - 2]) / (AK * alpha[K - 2] + B);

        for (int i = K - 1; i > 0; i--)
        {
          res[i - 1, tstep + 1] = alpha[i - 1] * res[i, tstep + 1] + beta[i - 1];
        }

        progressBar1.Value++;
      }

      if (portion == 0)
      {
        double I = MethodSimpson(ref res, tAmount - 1, 0);
        for (int i = 0; i < K; i++)
          res[i, tAmount - 1] /= I;
      }

      timer.Stop();
      double time = (timer.Elapsed).TotalMilliseconds;
      if (portion == 0) timeForA = time;
      else timeForB = time;

      return res;
    }

    private void PortionA()
    {
      progressBar1.Value = tAmount;

      string part_a = "W(x, T)";
      chart1.Series[part_a].Enabled = true;
      chart1.Series[part_a].IsVisibleInLegend = true;

      double current_x = 0;
      for (int i = 0; i < hAmount; ++i)
      {
        chart1.Series[part_a].Points.AddXY(current_x, res_A[i, tAmount - 1]);
        current_x += h;
      }
    }
    
    private void PortionB()
    {
      string part_B = "Y(x, T)";
      string input_data = "Φ(x)";
      string part_a = "W(x, T)";

      chart1.Series.Add(input_data);
      chart1.Series.Add(part_B);
      chart1.Series.Add(part_a);

      chart1.Series[input_data].IsVisibleInLegend = true;
      chart1.Series[part_B].IsVisibleInLegend = true;
      chart1.Series[part_a].IsVisibleInLegend = false;

      chart1.Series[part_a].Enabled = false;

      chart1.Series[input_data].ChartType = SeriesChartType.Spline;
      chart1.Series[part_B].ChartType = SeriesChartType.Spline;
      chart1.Series[part_a].ChartType = SeriesChartType.Spline;

      chart1.Series[input_data].BorderWidth = 2;
      chart1.Series[part_B].BorderWidth = 2;
      chart1.Series[part_a].BorderWidth = 2;

      chart1.Series[input_data].Color = Color.RoyalBlue;
      chart1.Series[part_B].Color = Color.DarkRed;  // RoyalBlue
      chart1.Series[part_a].Color = Color.ForestGreen;

            chart1.Legends.Add(new Legend("Legend"));
      chart1.Legends["Legend"].Font = new Font(chart1.Legends["Legend"].Font.FontFamily, 10);
      chart1.Legends["Legend"].Docking = Docking.Top;
      chart1.Legends["Legend"].LegendStyle = LegendStyle.Table;

      double current_x = 0;
      for (int k = 0; k < hAmount; k++)
      {
        chart1.Series[input_data].Points.AddXY(current_x, res_B[k, 0]);
        chart1.Series[part_B].Points.AddXY(current_x, res_B[k, tAmount - 1]);
        current_x += h;
      }

      MyDataGrid.RowCount = hAmount;
      MyDataGrid.ColumnCount = 2;
      MyDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
      MyDataGrid.Columns[0].HeaderText = "Φ[]";
      MyDataGrid.Columns[1].HeaderText = "B[]";

      for (int k = 0; k < hAmount; k++)
      {
        MyDataGrid.Rows[k].HeaderCell.Value = k.ToString();
        MyDataGrid.Rows[k].Cells[0].Value = res_B[k, 0].ToString();
        MyDataGrid.Rows[k].Cells[1].Value = BFunction(h * k).ToString();
      }
      progressBar1.Value = tAmount;
    }
  }
}
