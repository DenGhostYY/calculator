using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleCalculator
{
    public partial class Form1 : Form
    {
        private Calculation.TypesErros error = Calculation.TypesErros.NoError;
        private char codeKey = (char)0;
        private int selectedInput = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void AddInputTextBox(string s)
        {
            codeKey = '0';
            selectedInput = inputFieldTextBox.SelectionStart;
            inputFieldTextBox.Text = inputFieldTextBox.Text.Insert(selectedInput, s);
        }

        // здесь должно вычисляться выражение
        private void Calulate()
        {
            if (inputFieldTextBox.TextLength == 0)
            {
                return;
            }
            outputFieldTextBox.Text =
                Calculation.Calculate(inputFieldTextBox.Text, out error);
        }

        private void Back(bool isButton)
        {
            codeKey = (char)0;
            if (inputFieldTextBox.TextLength == 0)
            {
                outputFieldTextBox.Text = "";
                return;
            }
            if (isButton)
            {
                codeKey = (char)0;
                selectedInput = inputFieldTextBox.SelectionStart;
                if (selectedInput > 0)
                {
                    inputFieldTextBox.Text =
                        inputFieldTextBox.Text.Remove(selectedInput - 1, 1);
                }
                if (!inputFieldTextBox.Focused)
                {
                    inputFieldTextBox.SelectionStart = (selectedInput > 0 ? selectedInput - 1 : 0);
                    inputFieldTextBox.Focus();
                }

            }
            if (inputFieldTextBox.TextLength != 0)
            {
                Calulate();
            }
            else
            {
                outputFieldTextBox.Text = "";
            }
        }

        private void Drop()
        {
            codeKey = (char)0;
            inputFieldTextBox.Text = outputFieldTextBox.Text = "";
            if (!inputFieldTextBox.Focused)
            {
                inputFieldTextBox.SelectionStart = 0;
                inputFieldTextBox.Focus();
            }
        }

        private void Finish()
        {
            if (error != Calculation.TypesErros.NoError)
            {
                string text = "";
                switch (error)
                {
                    case Calculation.TypesErros.Lexical:
                        text = "Лексическая ошибка";
                        break;
                    case Calculation.TypesErros.Syntax:
                        text = "Синтаксическая ошибка";
                        break;
                    case Calculation.TypesErros.DivideByZero:
                        text = "Деление на ноль";
                        break;
                    case Calculation.TypesErros.RootNegative:
                        text = "Некорректные параметры корня";
                        break;
                    default:
                        text = "Неизвестная ошибка";
                        break;
                }
                MessageBox.Show(text, "Ошибка", MessageBoxButtons.OK);
                return;
            }
            inputFieldTextBox.Text = outputFieldTextBox.Text;
            inputFieldTextBox.SelectionStart = 0;
            inputFieldTextBox.SelectionLength = inputFieldTextBox.TextLength;
            inputFieldTextBox.Focus();
        }

        private void inputFieldTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) || e.KeyChar == (char)Keys.Back ||
                    e.KeyChar == (char)Keys.Enter || e.KeyChar == (char)Keys.Delete ||
                    e.KeyChar == '+' || e.KeyChar == '-' || e.KeyChar == '*' ||
                    e.KeyChar == '/' || e.KeyChar == ',' || e.KeyChar == '.' ||
                    e.KeyChar == '(' || e.KeyChar == ')' || e.KeyChar == '^' ||
                    e.KeyChar == '\\')
            {
                if (e.KeyChar == '.')
                {
                    e.KeyChar = ',';
                }
                if (e.KeyChar == '\\')
                {
                    e.KeyChar = '√';
                }
                codeKey = e.KeyChar;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void button0_Click(object sender, EventArgs e)
        {
            if (sender is Button)
            {
                AddInputTextBox(((Button)sender).Text);
            }
        }

        private void finishButton_Click(object sender, EventArgs e)
        {
            Finish();
        }

        private void dropButton_Click(object sender, EventArgs e)
        {
            Drop();
        }

        private void backspaceButton_Click(object sender, EventArgs e)
        {
            Back(true);
        }

        private void cправкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text =
                "3√64=4 (корень кубический)" + Environment.NewLine +
                "3*√64=3*8=24 (корень квадратный)" + Environment.NewLine +
                "Чтобы ввести '√' с клавиатуры, нажмите '\\'";
            MessageBox.Show(text, "Справка", MessageBoxButtons.OK);
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string text =
                "Автор программы" + Environment.NewLine +
                "Чумаков Денис" + Environment.NewLine +
                "Группа ЕТ-312" + Environment.NewLine +
                "2021";
            MessageBox.Show(text, "О программе", MessageBoxButtons.OK);
        }

        private void inputFieldTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Drop();
            }
        }

        private void inputFieldTextBox_TextChanged(object sender, EventArgs e)
        {
            if (codeKey == (char)0)
            {
                return;
            }

            if (codeKey == (char)Keys.Back)
            {
                Back(false);
            }
            else if (codeKey == (char)Keys.Enter)
            {
                codeKey = (char)0;
                int count = Environment.NewLine.Length;
                inputFieldTextBox.Text =
                    inputFieldTextBox.Text.Remove(inputFieldTextBox.TextLength - count, count);
                Finish();
            }
            else
            {
                codeKey = (char)0;
                Calulate();
                if (!inputFieldTextBox.Focused)
                {
                    inputFieldTextBox.SelectionStart = selectedInput + 1;
                    inputFieldTextBox.Focus();
                }
            }
        }
    }
}
