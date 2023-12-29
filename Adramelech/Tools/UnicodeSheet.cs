using System.Text;

namespace Adramelech.Tools;

public class UnicodeSheet(bool separateRows = false)
{
    private readonly List<Column> _columns = [];

    public UnicodeSheet AddColumn(string title, IEnumerable<string> values)
    {
        _columns.Add(new Column { Title = title, Values = values.ToArray() });
        return this;
    }

    public string Build()
    {
        StringBuilder sb = new();

        var columnsLength = GetMaxColumnsLength();

        if (separateRows)
        {
            DrawLine(sb, '╔', '╦', '╗', columnsLength, '═');
            DrawSpecialLine(sb, '║', '║', '║', _columns.Select(c => c.Title).ToList(), columnsLength);
            DrawLine(sb, '╠', '╬', '╣', columnsLength, '═');
        }
        else
        {
            DrawLine(sb, '┌', '┬', '┐', columnsLength);
            DrawSpecialLine(sb, '│', '│', '│', _columns.Select(c => c.Title).ToList(), columnsLength);
            DrawLine(sb, '├', '┼', '┤', columnsLength);
        }

        for (var i = 0; i < _columns[0].Values.Length; i++)
        {
            var values = _columns.Select(c => c.Values[i]).ToList();
            DrawSpecialLine(sb, '│', '│', '│', values, columnsLength);

            if (!separateRows) continue;

            if (i == _columns[0].Values.Length - 1)
            {
                DrawLine(sb, '└', '┴', '┘', columnsLength);
                continue;
            }

            DrawLine(sb, '├', '┼', '┤', columnsLength);
        }

        if (!separateRows)
            DrawLine(sb, '└', '┴', '┘', columnsLength);

        return sb.ToString();
    }

    private List<int> GetMaxColumnsLength()
    {
        var max = _columns.Select(c => c.Title.Length).ToList();
        for (var i = 0; i < _columns.Count; i++)
        {
            var maxLength = _columns[i].Values.Max(v => v.Length);
            if (maxLength > max[i])
                max[i] = maxLength;
        }

        return max;
    }

    private void DrawLine(StringBuilder sb, char right, char middle, char left, IReadOnlyList<int> columnsLength,
        char separator = '─')
    {
        sb.Append(right);
        for (var i = 0; i < _columns.Count; i++)
        {
            sb.Append(separator.ToString().PadRight(columnsLength[i] + 2, separator));
            if (i < _columns.Count - 1)
                sb.Append(middle);
        }

        sb.Append(left).Append('\n');
    }

    private void DrawSpecialLine(StringBuilder sb, char right, char middle, char left,
        IReadOnlyCollection<string> values, IReadOnlyList<int> columnsLength, char separator = ' ')
    {
        sb.Append(right);

        for (var i = 0; i < _columns.Count; i++)
        {
            sb.Append($" {values.ElementAt(i).PadRight(columnsLength[i], separator)} ");
            if (i < _columns.Count - 1)
                sb.Append(middle);
        }

        sb.Append(left).Append('\n');
    }

    private struct Column
    {
        public string Title;
        public string[] Values;
    }
}