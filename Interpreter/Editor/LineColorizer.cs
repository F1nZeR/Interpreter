using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Rendering;

namespace Interpreter.Editor
{
    class LineColorizer : DocumentColorizingTransformer
    {
        private int _lineNumber;
        private readonly int _fromPos, _toPos;

        public LineColorizer(int lineNumber, int fromPos, int toPos)
        {
            _fromPos = fromPos;
            _toPos = toPos;
            if (lineNumber < 1)
                throw new ArgumentOutOfRangeException("lineNumber", lineNumber, "Номер строки должен быть > 0");
            _lineNumber = lineNumber;
        }

        public int LineNumber
        {
            get { return _lineNumber; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", value, "Номер строки должен быть > 0");
                _lineNumber = value;
            }
        }

        protected override void ColorizeLine(ICSharpCode.AvalonEdit.Document.DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == _lineNumber)
            {
                ChangeLinePart(_fromPos, _toPos, ApplyChanges);
            }
        }

        void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetForegroundBrush(Brushes.Black);
            element.TextRunProperties.SetBackgroundBrush(Brushes.Red);
        }
    }
}