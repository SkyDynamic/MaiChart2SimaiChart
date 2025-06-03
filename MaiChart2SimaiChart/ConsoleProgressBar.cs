using System.Text;

namespace MaiChart2SimaiChart;

public class ConsoleProgressBar : IDisposable
{
    private int _total;
    private int _current;
    private readonly object _lock = new ();
    private readonly TextWriter _originalOut;
    private readonly InterceptingTextWriter _interceptingWriter;
    private bool _disposed;

    public ConsoleProgressBar(int total = 100)
    {
        if (total <= 0)
            throw new ArgumentException("The total progress must be greater than 0", nameof(total));

        _total = total;
        _current = 0;

        // 保存原始的Console.Out并设置拦截器
        _originalOut = Console.Out;
        _interceptingWriter = new InterceptingTextWriter(this, _originalOut);
        Console.SetOut(_interceptingWriter);

        // 初始化进度条显示
        DrawProgressBar();
    }

    public void SetTotal(int value)
    {
        if (value <= 0)
            throw new ArgumentException("The total progress must be greater than 0", nameof(value));

        _total = value;
        _current = Math.Min(_current, _total);

        DrawProgressBar();
    } 

    public void Update(int progress = 1)
    {
        lock (_lock)
        {
            if (_disposed) return;

            _current = Math.Min(_current + progress, _total);
            DrawProgressBar();
        }
    }

    public void SetProgress(int progress)
    {
        lock (_lock)
        {
            if (_disposed) return;

            _current = Math.Max(0, Math.Min(progress, _total));
            DrawProgressBar();
        }
    }

    internal void HandleConsoleOutput(string text)
    {
        lock (_lock)
        {
            if (_disposed) return;

            // 清除进度条
            ClearProgressBar();

            // 输出被拦截的文本到原始输出
            _originalOut.Write(text);

            // 重新绘制进度条
            DrawProgressBar();
        }
    }

    private void DrawProgressBar()
    {
        if (_disposed) return;

        // 保存当前光标位置
        int currentTop = Console.CursorTop;
        int currentLeft = Console.CursorLeft;

        try
        {
            Console.SetCursorPosition(0, Console.WindowHeight - 1);

            double percentage = (double)_current / _total;

            int consoleWidth = Console.WindowWidth;

            string percentText = $" {percentage:P1}";
            int percentWidth = percentText.Length;

            int availableWidth = consoleWidth - 2 - percentWidth; // 2 for [ and ]

            if (availableWidth > 0)
            {
                // 计算填充字符数量
                int filledWidth = (int)(percentage * availableWidth);
                int emptyWidth = availableWidth - filledWidth;

                // 构建进度条
                string progressBar = "[";

                if (filledWidth > 0)
                {
                    // 添加已完成部分（=）和进度指示符（>）
                    if (filledWidth == 1 && _current < _total)
                    {
                        progressBar += ">";
                    }
                    else if (_current < _total && filledWidth > 1)
                    {
                        progressBar += new string('=', filledWidth - 1) + ">";
                    }
                    else
                    {
                        progressBar += new string('=', filledWidth);
                    }
                }

                // 添加空白部分
                progressBar += new string(' ', emptyWidth);
                progressBar += "]";
                progressBar += percentText;

                // 直接写入原始输出，避免递归拦截
                _originalOut.Write(progressBar);
            }
        }
        finally
        {
            // 如果原来的光标位置不是最后一行，则恢复光标位置
            if (currentTop < Console.WindowHeight - 1)
            {
                Console.SetCursorPosition(currentLeft, currentTop);
            }
        }
    }

    private void ClearProgressBar()
    {
        if (_disposed) return;

        // 保存当前光标位置
        var currentTop = Console.CursorTop;
        var currentLeft = Console.CursorLeft;

        try
        {
            // 移动到控制台底部并清除
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
            _originalOut.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.WindowHeight - 1);
        }
        finally
        {
            // 恢复光标位置
            if (currentTop < Console.WindowHeight - 1)
            {
                Console.SetCursorPosition(currentLeft, currentTop);
            }
        }
    }

    public void Complete()
    {
        SetProgress(_total);
    }

    public void Clear()
    {
        lock (_lock)
        {
            if (_disposed) return;

            ClearProgressBar();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            if (_disposed) return;

            // 清除进度条
            ClearProgressBar();

            // 恢复原始的Console.Out
            Console.SetOut(_originalOut);

            // 释放拦截器
            _interceptingWriter.Dispose();

            _disposed = true;
        }
    }

    public bool IsCompleted => _current >= _total;
    public int Current => _current;
    public int Total => _total;
    public double Percentage => (double)_current / _total;
}

// 拦截Console输出的TextWriter
internal class InterceptingTextWriter : TextWriter
{
    private readonly ConsoleProgressBar _progressBar;
    private readonly TextWriter _originalWriter;

    public InterceptingTextWriter(ConsoleProgressBar progressBar, TextWriter originalWriter)
    {
        _progressBar = progressBar;
        _originalWriter = originalWriter;
    }

    public override Encoding Encoding => _originalWriter.Encoding;

    public override void Write(char value)
    {
        _progressBar.HandleConsoleOutput(value.ToString());
    }

    public override void Write(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _progressBar.HandleConsoleOutput(value);
        }
    }

    public override void Write(char[]? buffer, int index, int count)
    {
        if (buffer == null || count <= 0) return;

        var text = new string(buffer, index, count);
        _progressBar.HandleConsoleOutput(text);
    }

    public override void WriteLine()
    {
        _progressBar.HandleConsoleOutput(Environment.NewLine);
    }

    public override void WriteLine(string? value)
    {
        _progressBar.HandleConsoleOutput(value + Environment.NewLine);
    }

    public override void WriteLine(char value)
    {
        _progressBar.HandleConsoleOutput(value + Environment.NewLine);
    }

    public override void WriteLine(char[]? buffer, int index, int count)
    {
        if (buffer != null && count > 0)
        {
            string text = new string(buffer, index, count);
            _progressBar.HandleConsoleOutput(text + Environment.NewLine);
        }
    }

    // 重写其他常用的Write方法
    public override void Write(bool value) => Write(value.ToString());
    public override void Write(int value) => Write(value.ToString());
    public override void Write(uint value) => Write(value.ToString());
    public override void Write(long value) => Write(value.ToString());
    public override void Write(ulong value) => Write(value.ToString());
    public override void Write(float value) => Write(value.ToString());
    public override void Write(double value) => Write(value.ToString());
    public override void Write(decimal value) => Write(value.ToString());
    public override void Write(object? value) => Write(value?.ToString() ?? "");

    public override void WriteLine(bool value) => WriteLine(value.ToString());
    public override void WriteLine(int value) => WriteLine(value.ToString());
    public override void WriteLine(uint value) => WriteLine(value.ToString());
    public override void WriteLine(long value) => WriteLine(value.ToString());
    public override void WriteLine(ulong value) => WriteLine(value.ToString());
    public override void WriteLine(float value) => WriteLine(value.ToString());
    public override void WriteLine(double value) => WriteLine(value.ToString());
    public override void WriteLine(decimal value) => WriteLine(value.ToString());
    public override void WriteLine(object? value) => WriteLine(value?.ToString() ?? "");

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // 不要释放原始writer，它可能仍在使用
        }

        base.Dispose(disposing);
    }
}