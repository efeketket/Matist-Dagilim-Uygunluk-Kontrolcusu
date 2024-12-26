using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MathNet.Numerics.Distributions;


namespace daglimuygunlukkontrolcüsü
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.ColumnCount = 6;
            dataGridView1.Columns[0].Name = "xᵢ";
            dataGridView1.Columns[1].Name = "nᵢ";
            dataGridView1.Columns[2].Name = "xᵢ * nᵢ";
            dataGridView1.Columns[3].Name = "xᵢ² * nᵢ";
            dataGridView1.Columns[4].Name = "xᵢ³ * nᵢ";
            dataGridView1.Columns[5].Name = "xᵢ⁴ * nᵢ";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // OpenFileDialog oluştur
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Dosya Seç",
                Filter = "Metin Dosyaları (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                // DataReader ile dosyayı yükle
                var dataReader = new DataReader();
                try
                {
                    dataReader.LoadData(filePath);

                    // DataGridView'i doldur
                    dataGridView1.Rows.Clear(); // Önce eski verileri temizleyelim
                    for (int i = 0; i < dataReader.XiList.Count; i++)
                    {
                        int xi = dataReader.XiList[i];
                        int ni = dataReader.NiList[i];
                        int xi_ni = xi * ni;
                        int xi2_ni = xi * xi * ni;
                        int xi3_ni = xi * xi * xi * ni;
                        int xi4_ni = xi * xi * xi * xi * ni;

                        // Tabloya satır ekle
                        dataGridView1.Rows.Add(xi, ni, xi_ni, xi2_ni, xi3_ni, xi4_ni);
                    }

                    MessageBox.Show("Veriler başarıyla yüklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            double alpha = Convert.ToDouble(textBox1.Text);
            if (!double.TryParse(textBox1.Text, out alpha) || alpha <= 0 || alpha >= 1)
            {
                MessageBox.Show("Lütfen 0 ile 1 arasında geçerli bir anlamlılık düzeyi girin (ör: 0.05)", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Gözlenen frekanslar (n_i)
            var observed = new List<int>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[1].Value != null)
                    observed.Add(int.Parse(row.Cells[1].Value.ToString()));
            }

            // Beklenen frekanslar
            var expected = new List<double>();
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["BeklenenFrekans"].Value != null)
                    expected.Add(double.Parse(row.Cells["BeklenenFrekans"].Value.ToString()));
            }

            // Serbestlik derecesi
            int degreesOfFreedom = observed.Count - 1 - 1; // Kategori Sayısı - 1 - Tahmin Edilen Parametreler

            // Chi-Kare Testi
            var chiSquareTest = new ChiSquareTest();
            double chiSquare = chiSquareTest.CalculateChiSquare(observed, expected);
            double pValue = chiSquareTest.CalculatePValue(chiSquare, degreesOfFreedom);

            // Anlamlılık düzeyine göre karar ver
            
            label2.Text = $"Chi-Kare Değeri: {chiSquare:F2}";
            label3.Text = $"p - Değeri: {pValue} ";
            if (pValue < alpha)
            {
                MessageBox.Show($"Chi-Kare Değeri: {chiSquare:F2}, p-Değeri: {pValue:F4}\nSonuç: Poisson dağılımına uygun değil.", "Test Sonucu");
            }
            else
            {
                MessageBox.Show($"Chi-Kare Değeri: {chiSquare:F2}, p-Değeri: {pValue:F4}\nSonuç: Poisson dağılımına uygun.", "Test Sonucu");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // DataGridView'den verileri oku
            var xiList = new List<int>();
            var niList = new List<int>();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                // Eğer satır boş değilse
                if (row.Cells[0].Value != null && row.Cells[1].Value != null)
                {
                    int xi = int.Parse(row.Cells[0].Value.ToString());
                    int ni = int.Parse(row.Cells[1].Value.ToString());

                    xiList.Add(xi);
                    niList.Add(ni);
                }
            }

            // Poisson hesaplayıcıyı başlat
            var calculator = new PoissonCalculator();

            // M değerini hesapla
            double M = calculator.CalculateM(xiList, niList);
            label1.Text = $"M (Lambda): {M}";

            // Beklenen frekansları hesapla
            double totalObservations = niList.Sum();
            var expectedFrequencies = calculator.CalculateExpectedFrequencies(M, xiList, totalObservations);

            if (!dataGridView1.Columns.Contains("Beklenen Frekans"))
            {
                dataGridView1.Columns.Add("BeklenenFrekans", "Beklenen Frekans");
            }

            // Beklenen frekansları ilgili satırlara yaz
            for (int i = 0; i < expectedFrequencies.Length; i++)
            {
                // DataGridView'deki ilgili satıra beklenen frekansı yaz
                if (i < dataGridView1.Rows.Count - 1) // Boş satırı hariç tut
                {
                    dataGridView1.Rows[i].Cells["BeklenenFrekans"].Value = expectedFrequencies[i].ToString("F2"); // İki ondalıklı format
                }
            }
    }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }

    public class PoissonCalculator
    {
        // M (Lambda) değerini hesaplar
        public double CalculateM(List<int> xiList, List<int> niList)
        {
            double sumXiNi = 0;
            double sumNi = 0;

            for (int i = 0; i < xiList.Count; i++)
            {
                sumXiNi += xiList[i] * niList[i];
                sumNi += niList[i];
            }

            return sumXiNi / sumNi; // M (Lambda) değeri
        }

        // Beklenen frekansları hesaplar
        public double[] CalculateExpectedFrequencies(double lambda, List<int> xiList, double totalObservations)
        {
            var expectedFrequencies = new double[xiList.Count];
            for (int i = 0; i < xiList.Count; i++)
            {
                int xi = xiList[i];
                double probability = Math.Exp(-lambda) * Math.Pow(lambda, xi) / Factorial(xi);
                expectedFrequencies[i] = probability * totalObservations;
            }

            return expectedFrequencies;
        }

        // Faktöriyel hesaplama
        private int Factorial(int n)
        {
            if (n == 0 || n == 1)
                return 1;

            int result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;

            return result;
        }
    }

    public class DataReader
    {
        public List<int> XiList { get; private set; }
        public List<int> NiList { get; private set; }

        public DataReader()
        {
            XiList = new List<int>();
            NiList = new List<int>();
        }

        // Dosyadan veri oku
        public void LoadData(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Dosya bulunamadı!");

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(','); // xᵢ ve nᵢ arasına virgül koyulduğunu varsayıyoruz
                XiList.Add(int.Parse(parts[0]));
                NiList.Add(int.Parse(parts[1]));
            }
        }
    }

    public class ChiSquareTest
    {
        // Chi-Kare değerini hesaplar
        public double CalculateChiSquare(List<int> observed, List<double> expected)
        {
            double chiSquare = 0.0;

            for (int i = 0; i < observed.Count; i++)
            {
                if (expected[i] > 0) // Beklenen frekans sıfır olmamalı
                {
                    chiSquare += Math.Pow(observed[i] - expected[i], 2) / expected[i];
                }
            }

            return chiSquare;
        }

        // p-Değeri hesaplar
        public double CalculatePValue(double chiSquare, int degreesOfFreedom)
        {
            // MathNet.Numerics ile p-değeri hesaplama
            return 1.0 - ChiSquared.CDF(degreesOfFreedom, chiSquare);
        }

        public double CalculateMean(List<int> xiList, List<int> niList)
        {
            double sumXiNi = 0;
            double sumNi = 0;

            for (int i = 0; i < xiList.Count; i++)
            {
                sumXiNi += xiList[i] * niList[i];
                sumNi += niList[i];
            }

            return sumXiNi / sumNi; // Standart Ortalama

        }

        public double CalculateVariance(List<int> xiList, List<int> niList, double mean)
        {
            double sumXiMinusMeanSquared = 0;
            double sumNi = 0;

            for (int i = 0; i < xiList.Count; i++)
            {
                sumXiMinusMeanSquared += niList[i] * Math.Pow(xiList[i] - mean, 2);
                sumNi += niList[i];
            }

            return sumXiMinusMeanSquared / sumNi; // Standart Varyans
        }

        // Teorik Momentler
        public double GetTheoreticalMean(double lambda)
        {
            return lambda; // Poisson ortalaması
        }

        public double GetTheoreticalVariance(double lambda)
        {
            return lambda; // Poisson varyansı
        }

    }
}
