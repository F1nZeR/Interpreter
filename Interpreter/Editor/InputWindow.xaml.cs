using System.Windows;
using System.Windows.Input;
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
            tbInput.Focus();
            tbInput.KeyDown += (sender, args) =>
                                   {
                                       if (args.Key == Key.Enter) ButtonClickConfirm(this, new RoutedEventArgs());
                                   };
        }

        private void ButtonClickConfirm(object sender, RoutedEventArgs e)
        {
            var lex = (Lexem) DataContext;
            Storage.SetVariableValue(lex, tbInput.Text);
            this.Close();
        }
    }
}
