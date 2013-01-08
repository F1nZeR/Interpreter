using System.Windows;
using Interpreter.Model;
using Interpreter.Utilities;

namespace Interpreter.Editor
{
    /// <summary>
    /// Логика взаимодействия для InputWindow.xaml
    /// </summary>
    public partial class InputWindow : Window
    {
        public InputWindow(Lexem lexem)
        {
            InitializeComponent();
            DataContext = lexem;
            lblInfo.Content = "Введите значение переменной \"" + lexem.Content + "\":";
        }

        private void ButtonClickConfirm(object sender, RoutedEventArgs e)
        {
            var lex = (Lexem) DataContext;
            Storage.SetVariableValue(lex, tbInput.Text);
            this.Close();
        }
    }
}
