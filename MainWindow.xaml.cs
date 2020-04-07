using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Stopwatch stopwatch = new Stopwatch();
        private Calc calculator = new Calc();
        private bool errorRaised = false;
        public MainWindow()
        {
            InitializeComponent();
            EventManager.RegisterClassHandler(typeof(Button), Button.ClickEvent, new RoutedEventHandler(OnButtonClick));
        }

        private void DeleteCharacter()
        {
            if (errorRaised)
            {
                ClearError();
            }
            var text = expressionText.Text;
            if(text.Length > 0)
            {
                expressionText.Text = text.Substring(0, text.Length - 1);
                UpdateOutput();
            }
            else
            {
                resultText.Text = "";
            }

            ScrollToEnd();
        }

        private void DeleteEverything()
        {
            expressionText.Text = "";
            resultText.Text = "";
            ScrollToEnd();
        }

        private void AddCharacter(string character)
        {
            expressionText.AppendText(character);
            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            expressionText.Select(expressionText.Text.Length, 0);
            expressionText.Focus();
        }

        private void AddOperator(string op)
        {
            if (op == "=" && !errorRaised)
            {
                expressionText.Text = resultText.Text;
            }
            else if (expressionText.Text.Length == 0)
            {
                if (op == "-" || op == "(")
                {
                    AddCharacter(op);
                }
            }
            else if(op == ".")
            {
                bool alreadyHasComma = false;
                for(int i = expressionText.Text.Length - 1; i > 0; i--)
                {
                    if(expressionText.Text[i] == '.')
                    {
                        alreadyHasComma = true;
                    }

                    if (Calc.Operators.Contains(expressionText.Text[i]))
                    {
                        break;
                    }
                }

                if (!alreadyHasComma)
                {
                    AddCharacter(op);
                }
            }
            else if (op == ")")
            {
                if (expressionText.Text.Count(ch => ch == '(') > expressionText.Text.Count(ch => ch == ')'))
                {
                    if(GetLastCharacter() != "(")
                    {
                        AddCharacter(")");
                    }
                }
            }
            else if (int.TryParse(GetLastCharacter(), out int _) && op != "(")
            {
                AddCharacter(op);
            }
            else if (calculator.CanInsertOperator(GetLastCharacter(), op))
            {
                AddCharacter(op);
            }
            else {
                if(op != "(")
                {
                    DeleteCharacter();
                    if(expressionText.Text.Length == 0)
                    {
                        AddOperator(op);
                        return;
                    }
                    if(GetLastCharacter() != "*")
                    {
                        AddCharacter(op);
                    }
                }
            }
        }

        private void UpdateOutput()
        {
            try
            {
                calculator.EvaluateExpression(expressionText.Text);
                resultText.Text = calculator.GetResult;
            }
            catch (DivideByZeroException)
            {
                RaiseError("Div by 0");
            }
            catch (OverflowException)
            {
                RaiseError("Value too big");
            }
            catch (ArithmeticException)
            {
                RaiseError("Number too long");
            }
            catch (Exception)
            {
                //empty on purpose
                //this means that the evaluation is invalid because expression is not yet complete
            }
        }

        private void RaiseError(string err)
        {
            resultText.Text = err;
            errorRaised = true;
            resultText.Foreground = Brushes.Red;
        }

        private void ClearError()
        {
            errorRaised = false;
            resultText.Foreground = Brushes.Gray;
        }

        private string GetLastCharacter()
        {
            return expressionText.Text[expressionText.Text.Length - 1].ToString();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Button src = (Button)sender;
            if (int.TryParse(src.Content.ToString(), out int _))
            {
                AddCharacter(src.Content.ToString());
            }
            else
            {
                AddOperator(src.Content.ToString());
            }

            UpdateOutput();
        }

        private void DelButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Start();
            e.Handled = true;
        }

        private void DelButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            stopwatch.Stop();
            if(stopwatch.ElapsedMilliseconds > 500)
            {
                DeleteEverything();
            }
            else
            {
                DeleteCharacter();
            }

            stopwatch.Reset();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if(((int)e.Key >= 34 && (int)e.Key <= 43) || ((int)e.Key >= 74 && (int)e.Key <= 83))
            {
                AddCharacter(e.Key.ToString().Substring(e.Key.ToString().Length - 1));
            }

            switch(e.Key)
            {
                case Key.Back: DeleteCharacter(); break;
                case Key.Enter: AddOperator("="); break;
                case Key.OemPlus:
                case Key.Add: AddOperator("+"); break;
                case Key.Oem2:
                case Key.Divide: AddOperator("/"); break;
                case Key.OemMinus:
                case Key.Subtract: AddOperator("-"); break;
                case Key.Multiply: AddOperator("*"); break;
                case Key.OemPeriod: AddOperator("."); break;
                default: break;
            }

            UpdateOutput();
        }
    }
}
