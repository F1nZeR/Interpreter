using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using Interpreter.Analysis;
using Interpreter.Editor;
using Interpreter.Exceptions;
using Interpreter.Utilities;
using Microsoft.Win32;

namespace Interpreter
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            using (var reader = new XmlTextReader("./Editor/syntax.xshd"))
            {
                textEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(
                    reader, HighlightingManager.Instance);
            }

            textEditor.KeyDown += (sender, args) =>
                                      {
                                          if (args.Key == Key.F5) StartCompile(this, new RoutedEventArgs());
                                      };

            textEditor.TextChanged += (sender, args) => ClearErrors();
        }

        private void StartCompile(object sender, RoutedEventArgs e)
        {
            ClearErrors();
            Storage.Clear();

            tbOutput.Clear();
            var exprParser = new ReversePolishNotation();
            var text = textEditor.Text + "\r\n";

            try
            {
                exprParser.Interpret(text);
            }
            catch (SyntaxException ex)
            {
                Storage.Output = ex.Message;
                textEditor.TextArea.TextView.LineTransformers.Add(new LineColorizer(
                                                                      CalculateRow(ex.Lexem.Position),
                                                                      ex.Lexem.Position,
                                                                      ex.Lexem.Content.ToString().Length +
                                                                      ex.Lexem.Position));
            }

            tbOutput.Text = Storage.Output;
        }

        private int CalculateRow(int global)
        {
            var lines = textEditor.Document.Lines;
            return (from documentLine in lines where documentLine.EndOffset >= global select documentLine.LineNumber).FirstOrDefault();
        }

        private void ClearErrors()
        {
            var error = textEditor.TextArea.TextView.LineTransformers.SingleOrDefault(x => x is LineColorizer);
            textEditor.TextArea.TextView.LineTransformers.Remove(error);
        }

        private void ToolBarLoaded(object sender, RoutedEventArgs e)
        {
            var toolBar = sender as ToolBar;
            var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
            if (overflowGrid != null)
            {
                overflowGrid.Visibility = Visibility.Collapsed;
            }

            var mainPanelBorder = toolBar.Template.FindName("MainPanelBorder", toolBar) as FrameworkElement;
            if (mainPanelBorder != null)
            {
                mainPanelBorder.Margin = new Thickness(0);
            }
        }

        private void SaveFile(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "Все файлы (*.*)|*.*" };
            var result = sfd.ShowDialog();
            if (result == true)
            {
                File.WriteAllText(sfd.FileName, textEditor.Text, Encoding.Unicode);
            }
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog { Filter = "Все файлы (*.*)|*.*" };
            var result = ofd.ShowDialog();
            if (result == true)
            {
                textEditor.Text = File.OpenText(ofd.FileName).ReadToEnd();
            }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            ClearErrors();
            textEditor.Clear();
        }
    }
}
