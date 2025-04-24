using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;

namespace TI3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ElHamalCipherer _cipherer = null;
        private EncryptMath _math = new EncryptMath();
        private string _inputFilePath = null;
        private string _outputFilePath = null;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnFindPrimitiveRoots_Click(object sender, RoutedEventArgs e)
        {
            cmbBoxG.Items.Clear();
            long p = 0;
            try
            {
                p = long.Parse(this.txtP.Text);
            } 
            catch (Exception ee) 
            {
                System.Windows.MessageBox.Show(ee.Message);
                return;
            }
            if (!_math.IsPrime(p))
            {
                System.Windows.MessageBox.Show("p не простое число");
                return;
            }
            var primintiveRoots = _math.FindPrimitiveRoots(p);
            foreach (var item in primintiveRoots)
            {
                cmbBoxG.Items.Add(item.ToString());
            }
            cmbBoxG.SelectedIndex = 0;
        }


        private string SetOutputFilePath()
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            return "";
        }

        private string SetInputFilePath()
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return "";
        }

        private bool isInputCorrect()
        {
            if (!File.Exists(_inputFilePath))
            {
                System.Windows.MessageBox.Show("Не существует файла");
                return false;
            }
            long p, g, x, k;
            try
            {
                p = long.Parse(this.txtP.Text);
                g = long.Parse(this.cmbBoxG.Text);
                x = long.Parse(this.txtX.Text);
                k = long.Parse(this.cmbBoxK.Text);
            }
            catch
            {
                System.Windows.MessageBox.Show("Неверный ввод");
                return false;
            }
            if (!_math.IsPrime(p) || !(x > 1 && x < p - 1))
            {
                System.Windows.MessageBox.Show("Неверный ввод");
                return false;
            }
            return true;
        }
        // Вспомогательные методы для форматирования
        private string FormatBytes(byte[] bytes)
        {
            string result = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                result += $"{i + 1}. {bytes[i]}\n";
            }
            return result;
        }

        private string FormatBytePairs(byte[] bytes)
        {
            string result = "";
            var abLength = _cipherer.AOrBLength;
            switch (abLength)
            {
                case 1:
                    for (int i = 0; i < bytes.Length; i += 2 * abLength)
                    {
                        var a = bytes[i + 1];
                        var b = bytes[i];
                        result += $"{i / (2 * abLength) + 1}. [{a}, {b}]\n";
                    }
                    break;
                case 2:
                    for (int i = 0; i < bytes.Length; i += 2 * abLength)
                    {
                        var a = BitConverter.ToUInt16(bytes, i + abLength);
                        var b = BitConverter.ToUInt16(bytes, i);
                        result += $"{i / (2 * abLength) + 1}. [{a}, {b}]\n";
                    }
                    break;
                case 3:
                    for (int i = 0; i < bytes.Length; i += 2 * abLength)
                    {
                        var a = BitConverter.ToUInt32(bytes, i + abLength);
                        var b = BitConverter.ToUInt32(bytes, i);
                        result += $"{i / (2 * abLength) + 1}. [{a}, {b}]\n";
                    }
                    break;
                case 4:
                    for (int i = 0; i < bytes.Length; i += 2 * abLength)
                    {
                        var a = BitConverter.ToUInt64(bytes, i + abLength);
                        var b = BitConverter.ToUInt64(bytes, i);
                        result += $"{i / (2 * abLength) + 1}. [{a}, {b}]\n";
                    }
                    break;
            }
            return result;
        }
        private void btnEncrypt_Click(object sender, RoutedEventArgs e)
        {
            _inputFilePath = SetInputFilePath();
            _outputFilePath = SetOutputFilePath();
            if (!isInputCorrect())
                return;
            var p = long.Parse(this.txtP.Text);
            var g = long.Parse(this.cmbBoxG.Text);
            var x = long.Parse(this.txtX.Text);
            var k = long.Parse(this.cmbBoxK.Text);
            _cipherer = new ElHamalCipherer(_math, p, g, x, k);
            using (var inputFileStream = new FileStream(_inputFilePath, FileMode.Open))
            using (var outputFileStream = new FileStream(_outputFilePath, FileMode.Create))
            {
                var abLength = _cipherer.AOrBLength;
                // Буферы для хранения первых и последних 10 байт
                byte[] first10InputBytes = new byte[10];
                byte[] last10InputBytes = new byte[10];
                byte[] first10OutputBytes = new byte[20 * abLength];
                byte[] last10OutputBytes = new byte[20 * abLength]; 

                inputFileStream.Read(first10InputBytes, 0, 10);
                if (inputFileStream.Length >= 10)
                {
                    inputFileStream.Seek(-10, SeekOrigin.End);
                    inputFileStream.Read(last10InputBytes, 0, 10);
                }
                inputFileStream.Seek(0, SeekOrigin.Begin);


                var buffer = new byte[abLength * 2];
                while (inputFileStream.Position < inputFileStream.Length)
                {
                    inputFileStream.Read(buffer, 0, 1);
                    var cipheredByte = _cipherer.CipherByte(buffer[0]);
                    Array.Copy(BitConverter.GetBytes(cipheredByte[0]), 0, buffer, 0, abLength);
                    Array.Copy(BitConverter.GetBytes(cipheredByte[1]), 0, buffer, buffer.Length / 2, abLength);
                    outputFileStream.Write(buffer, 0, buffer.Length);
                }

                outputFileStream.Seek(0, SeekOrigin.Begin);
                outputFileStream.Read(first10OutputBytes, 0, 20 * abLength);
                if (inputFileStream.Length >= 10)
                {
                    outputFileStream.Seek(-20 * abLength * abLength * abLength, SeekOrigin.End);
                    outputFileStream.Read(last10OutputBytes, 0, 20 * abLength);
                }

                // Формируем текстовое представление для отображения
                string inputText = "Первые 10 байт исходного файла:\n";
                inputText += FormatBytes(first10InputBytes);
                if (inputFileStream.Length >= 10)
                {
                    inputText += "\n\nПоследние 10 байт исходного файла:\n";
                    inputText += FormatBytes(last10InputBytes);
                }

                string outputText = "Первые 10 пар байт зашифрованного файла:\n";
                outputText += FormatBytePairs(first10OutputBytes);
                if (inputFileStream.Length >= 10)
                {
                    outputText += "\n\nПоследние 10 пар байт зашифрованного файла:\n";
                    outputText += FormatBytePairs(last10OutputBytes);
                }

                txtInput.Text = inputText;
                txtOutput.Text = outputText;
            }
            System.Windows.MessageBox.Show("Запись прошла  успешно!");
        }
        private void btnDecrypt_Click(object sender, RoutedEventArgs e)
        {
            _inputFilePath = SetInputFilePath();
            _outputFilePath = SetOutputFilePath();
            if (!isInputCorrect())
                return;
            var p = long.Parse(this.txtP.Text);
            var g = long.Parse(this.cmbBoxG.Text);
            var x = long.Parse(this.txtX.Text);
            var k = long.Parse(this.cmbBoxK.Text);
            _cipherer = new ElHamalCipherer(_math, p, g, x, k);
            using (var inputFileStream = new FileStream(_inputFilePath, FileMode.Open))
            using (var outputFileStream = new FileStream(_outputFilePath, FileMode.Create))
            {
                var abLength = _cipherer.AOrBLength;
                // Буферы для хранения первых и последних 10 байт
                byte[] first10InputBytes = new byte[20 * abLength];
                byte[] last10InputBytes = new byte[20 * abLength];
                byte[] first10OutputBytes = new byte[10];
                byte[] last10OutputBytes = new byte[10];

                inputFileStream.Read(first10InputBytes, 0, 20 * abLength);
                if (outputFileStream.Length >= 10)
                {
                    inputFileStream.Seek(-20 * abLength, SeekOrigin.End);
                    inputFileStream.Read(last10InputBytes, 0, 20 * abLength);
                }
                inputFileStream.Seek(0, SeekOrigin.Begin);

                byte[] buffer = new byte[abLength * 2];
                while (inputFileStream.Position < inputFileStream.Length)
                {
                    inputFileStream.Read(buffer, 0, buffer.Length);
                    buffer[0] = _cipherer.Decipher(buffer);
                    outputFileStream.Write(buffer, 0, 1);
                }

                outputFileStream.Seek(0, SeekOrigin.Begin);
                outputFileStream.Read(first10OutputBytes, 0, 10);
                if (outputFileStream.Length >= 10)
                {
                    outputFileStream.Seek(-10, SeekOrigin.End);
                    outputFileStream.Read(last10OutputBytes, 0, 10);
                }

                // Формируем текстовое представление для отображения
                string inputText = "Первые 10 пар байт исходного файла:\n";
                inputText += FormatBytePairs(first10InputBytes);
                if (outputFileStream.Length >= 10)
                {
                    inputText += "\n\nПоследние 10 пар байт исходного файла:\n";
                    inputText += FormatBytePairs(last10InputBytes);
                }

                string outputText = "Первые 10 байт расшифрованного файла:\n";
                outputText += FormatBytes(first10OutputBytes);

                if (outputFileStream.Length >= 10)
                {
                    outputText += "\n\nПоследние 10 байт расшифрованного файла:\n";
                    outputText += FormatBytes(last10OutputBytes);
                }

                    // Выводим в UI (не забудьте Invoke если это другой поток)
                    txtInput.Text = inputText;
                txtOutput.Text = outputText;
            }
            System.Windows.MessageBox.Show("Запись прошла  успешно!");
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            _inputFilePath = SetInputFilePath();
        }
        private void btnFindKs_Click(object sender, RoutedEventArgs e)
        { 
            cmbBoxK.Items.Clear();
            long p = 0;
            try
            {
                p = long.Parse(this.txtP.Text);
            }
            catch (Exception ee)
            {
                System.Windows.MessageBox.Show(ee.Message);
                return;
            }
            if (!_math.IsPrime(p))
            {
                System.Windows.MessageBox.Show("p не простое число");
                return;
            }
            var primintiveRoots = _math.GetCoprimes(p - 1);
            foreach (var item in primintiveRoots)
            {
                cmbBoxK.Items.Add(item.ToString());
            }
            cmbBoxK.SelectedIndex = 0;
        }
    }
}
