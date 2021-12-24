using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VSharp.Core.Interactive;

public abstract class AbstractRepl
{
    private readonly List<string> m_submissionHistory = new();
    private int m_submissionHistoryIndex;
    private bool m_done;

    public void Run()
    {
        while (true)
        {
            var text = EditSubmission();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (!text.Contains(Environment.NewLine) && text.StartsWith('#'))
            {
                EvaluateMetaCommand(text);
            }
            else
            {
                EvaluateSubmission(text);
            }

            m_submissionHistory.Add(text);
            m_submissionHistoryIndex = 0;
        }
    }


    private sealed class SubmissionView
    {
        private readonly Action<string> m_lineRenderer;
        private readonly ObservableCollection<string> m_submissionDocument;
        private readonly int m_cursorTop;
        private int m_renderedLineCount;
        private int m_currentLine;
        private int m_currentCharacter;

        public SubmissionView(Action<string> lineRenderer, ObservableCollection<string> submissionDocument)
        {
            m_lineRenderer = lineRenderer;
            m_submissionDocument = submissionDocument;
            m_submissionDocument.CollectionChanged += SubmissionDocumentChanged;
            m_cursorTop = Console.CursorTop;
            Render();
        }

        public int CurrentLine
        {
            get => m_currentLine;
            set
            {
                if (m_currentLine != value)
                {
                    m_currentLine = value;
                    m_currentCharacter = Math.Min(m_submissionDocument[m_currentLine].Length, m_currentCharacter);
                    UpdateCursorPosition();
                }
            }
        }

        public int CurrentCharacter
        {
            get => m_currentCharacter;
            set
            {
                if (m_currentCharacter != value)
                {
                    m_currentCharacter = value;
                    UpdateCursorPosition();
                }
            }
        }
        
        private void SubmissionDocumentChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Render();
        }

        private void Render()
        {
            Console.CursorVisible = false;
            var lineCount = 0;

            foreach (var line in m_submissionDocument)
            {
                Console.SetCursorPosition(0, m_cursorTop + lineCount);
                Console.ForegroundColor = ConsoleColor.Green;

                if (lineCount == 0)
                {
                    Console.Write("» ");
                }
                else
                {
                    Console.Write("· ");
                }

                Console.ResetColor();
                m_lineRenderer(line);
                Console.Out.WriteLine(new string(' ', Console.WindowWidth - line.Length));
                lineCount++;
            }

            var numberOfBlankLines = m_renderedLineCount - lineCount;
            if (numberOfBlankLines > 0)
            {
                var blankLine = new string(' ', Console.WindowWidth);
                for (var i = 0; i < numberOfBlankLines; i++)
                {
                    Console.SetCursorPosition(0, m_cursorTop + lineCount + i);
                    Console.Out.WriteLine(blankLine);
                }
            }

            m_renderedLineCount = lineCount;

            Console.CursorVisible = true;
            UpdateCursorPosition();
        }

        private void UpdateCursorPosition()
        {
            Console.CursorTop = m_cursorTop + m_currentLine;
            Console.CursorLeft = 2 + m_currentCharacter;
        }
    }

    private string EditSubmission()
    {
        m_done = false;

        var document = new ObservableCollection<string> { "" };
        var view = new SubmissionView(RenderLine, document);

        while (!m_done)
        {
            var key = Console.ReadKey(true);
            HandleKey(key, document, view);
        }

        view.CurrentLine = document.Count - 1;
        view.CurrentCharacter = document[view.CurrentCharacter].Length;
        Console.Out.WriteLine();

        return string.Join(Environment.NewLine, document);
    }

    private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionView view)
    {
        if (key.Modifiers == default)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    HandleEnter(document, view);
                    break;
                case ConsoleKey.Escape:
                    HandleEscape(document, view);
                    break;
                case ConsoleKey.LeftArrow:
                    HandleLeftArrow(document, view);
                    break;
                case ConsoleKey.RightArrow:
                    HandleRightArrow(document, view);
                    break;
                case ConsoleKey.UpArrow:
                    HandleUpArrow(document, view);
                    break;
                case ConsoleKey.DownArrow:
                    HandleDownArrow(document, view);
                    break;
                case ConsoleKey.Backspace:
                    HandleBackspace(document, view);
                    break;
                case ConsoleKey.Delete:
                    HandleDelete(document, view);
                    break;
                case ConsoleKey.Home:
                    HandleHome(document, view);
                    break;
                case ConsoleKey.End:
                    HandleEnd(document, view);
                    break;
                case ConsoleKey.Tab:
                    HandleTab(document, view);
                    break;
                case ConsoleKey.PageUp:
                    HandlePageUp(document, view);
                    break;
                case ConsoleKey.PageDown:
                    HandlePageDown(document, view);
                    break;
            }
        }
        else if (key.Modifiers == ConsoleModifiers.Control)
        {
            switch (key.Key)
            {
                case ConsoleKey.Enter:
                    HandleControlEnter(document, view);
                    break;
            }
        }

        if (key.KeyChar >= ' ')
        {
            HandleTyping(document, view, key.KeyChar.ToString());
        }
    }

    private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
    {
        var submissionText = string.Join(Environment.NewLine, document);
        if (submissionText.StartsWith('#') || IsCompleteSubmission(submissionText))
        {
            m_done = true;
            return;
        }

        InsertLine(document, view);
    }

    private static void HandleEscape(ObservableCollection<string> document, SubmissionView view)
    {
        document[view.CurrentLine] = string.Empty;
        view.CurrentCharacter = 0;
    }

    private static void HandleControlEnter(ObservableCollection<string> document, SubmissionView view)
    {
        InsertLine(document, view);
    }

    private static void InsertLine(ObservableCollection<string> document, SubmissionView view)
    {
        var remainder = document[view.CurrentLine][view.CurrentCharacter..];
        document[view.CurrentLine] = document[view.CurrentLine][..view.CurrentCharacter];

        var lineIndex = view.CurrentLine + 1;
        document.Insert(lineIndex, remainder);
        view.CurrentCharacter = 0;
        view.CurrentLine = lineIndex;
    }

    private static void HandleLeftArrow(ObservableCollection<string> document, SubmissionView view)
    {
        if (view.CurrentCharacter > 0)
        {
            view.CurrentCharacter--;
        }
    }
        
    private static void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
    {
        var line = document[view.CurrentLine];
        if (view.CurrentCharacter <= line.Length - 1)
        {
            view.CurrentCharacter++;
        }
    }
        
    private static void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
    {
        if (view.CurrentLine > 0)
        {
            view.CurrentLine--;
        }
    }

    private static void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
    {
        if (view.CurrentLine < document.Count - 1)
        {
            view.CurrentLine++;
        }
    }

    private static void HandleBackspace(ObservableCollection<string> document, SubmissionView view)
    {
        var start = view.CurrentCharacter;
        if (start == 0)
        {
            if (view.CurrentLine == 0)
            {
                return;
            }

            var currentLine = document[view.CurrentLine];
            var previousLine = document[view.CurrentLine - 1];
            document.RemoveAt(view.CurrentLine);
            view.CurrentLine--;
            document[view.CurrentLine] = previousLine + currentLine;
            view.CurrentCharacter = previousLine.Length;
        }
        else
        {
            var lineIndex = view.CurrentLine;
            var line = document[lineIndex];
            var before = line[..(start - 1)];
            var after = line[start..];
            document[lineIndex] = before + after;
            view.CurrentCharacter--;
        }
    }
    
    private static void HandleDelete(ObservableCollection<string> document, SubmissionView view)
    {
        var lineIndex = view.CurrentLine;
        var line = document[lineIndex];
        var start = view.CurrentCharacter;
        if (start >= line.Length - 1)
        {
            if (view.CurrentLine == document.Count - 1)
            {
                return;
            }

            var nextLine = document[view.CurrentLine + 1];
            document[view.CurrentLine] += nextLine;
            document.RemoveAt(view.CurrentLine + 1);
            return;
        }

        var before = line[..start];
        var after = line[(start + 1)..];
        document[lineIndex] = before + after;
    }
        
    private static void HandleHome(ObservableCollection<string> document, SubmissionView view)
    {
        view.CurrentCharacter = 0;
    }

    private static void HandleEnd(ObservableCollection<string> document, SubmissionView view)
    {
        view.CurrentCharacter = document[view.CurrentLine].Length;
    }
    
    private static void HandleTyping(ObservableCollection<string> document, SubmissionView view, string text)
    {
        var lineIndex = view.CurrentLine;
        var start = view.CurrentCharacter;
        document[lineIndex] = document[lineIndex].Insert(start, text);
        view.CurrentCharacter += text.Length;
    }

    private static void HandleTab(ObservableCollection<string> document, SubmissionView view)
    {
        const int tabsWidth = 4;
        var start = view.CurrentCharacter;
        var remainingSpaces = tabsWidth - start % tabsWidth;
        var line = document[view.CurrentLine];
        document[view.CurrentLine] = line.Insert(start, new string(' ', remainingSpaces));
        view.CurrentCharacter += remainingSpaces;
    }
        
    private void HandlePageUp(ObservableCollection<string> document, SubmissionView view)
    {
        m_submissionHistoryIndex--;
        if (m_submissionHistoryIndex < 0)
        {
            m_submissionHistoryIndex = m_submissionHistory.Count - 1;
        }

        UpdateDocumentFromHistory(document, view);
    }
        
    private void HandlePageDown(ObservableCollection<string> document, SubmissionView view)
    {
        m_submissionHistoryIndex++;
        if (m_submissionHistoryIndex > m_submissionHistory.Count - 1)
        {
            m_submissionHistoryIndex = 0;
        }

        UpdateDocumentFromHistory(document, view);
    }

    private void UpdateDocumentFromHistory(ObservableCollection<string> document, SubmissionView view)
    {
        if (m_submissionHistory.Count == 0)
        {
            return;
        }
            
        document.Clear();

        var historyItem = m_submissionHistory[m_submissionHistoryIndex];
        var lines = historyItem.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            document.Add(line);
        }
            
        view.CurrentLine = document.Count - 1;
        view.CurrentCharacter = document[view.CurrentLine].Length;
    }

    protected void ClearHistory()
    {
        m_submissionHistory.Clear();
    }

    protected virtual void EvaluateMetaCommand(string input)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Out.WriteLine($"invalid command {input}");
        Console.ResetColor();
    }
    
    protected virtual void RenderLine(string line)
    {
        Console.Out.WriteLine(line);
    }

    protected abstract bool IsCompleteSubmission(string text);
    protected abstract void EvaluateSubmission(string text);
}