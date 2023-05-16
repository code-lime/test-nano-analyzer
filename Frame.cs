using System.Collections.Generic;
using System.Text;

namespace NanoAnalyzer;

public class Frame
{
    public readonly Frame? parent;
    public readonly int current;
    public readonly int total;
    public readonly string? data;

    public Frame(int current, int total, string? data, Frame? parent)
    {
        this.current = current;
        this.total = total;
        this.data = data;
        this.parent = parent;
    }

    public static Frame Of(int current, int total, string? data = null, Frame? parent = null)
        => new Frame(current, total, data, parent);

    public Frame GlobalParent(Frame parent)
    {
        Frame? parentValue = this.parent;
        Stack<Frame> stack = new Stack<Frame>();
        while (parentValue != null)
        {
            stack.Push(parentValue);
            parentValue = parentValue.parent;
        }
        if (stack.Count == 0) return new Frame(current, total, data, parent);
        else
        {
            while (stack.TryPop(out Frame? lastParent)) parent = lastParent.WithParent(parent);
            return new Frame(current, total, data, parent);
        }
    }
    public Frame WithParent(Frame parent)
        => new Frame(current, total, data, parent);

    public Frame Child(int current, int total, string? data = null)
        => new Frame(current, total, data ?? this.data, this);

    public double GetProgress()
    {
        if (total == 0) return 0;
        double localProgress = current / (double)total;
        if (parent == null) return localProgress;
        if (parent.total == 0) return 0;
        return parent.GetProgress() + localProgress / parent.total;
    }
    public string GetText()
    {
        StringBuilder builder = new StringBuilder($"{current} / {total}");
        if (!string.IsNullOrWhiteSpace(data)) builder.Append($" | {data}");
        Frame? frame = this;
        while (frame != null)
        {
            builder.Append(frame.total == 0 ? " | 0%" : $" | {frame.current * 100 / frame.total}%");
            frame = frame.parent;
        }
        return builder.ToString();
    }
}
