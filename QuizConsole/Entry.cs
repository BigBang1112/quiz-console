namespace QuizConsole;

class Entry
{
    public required string Question { get; set; }
    public string[] Answers { get; set; } = Array.Empty<string>();
    public int[] Correct { get; set; } = Array.Empty<int>();
}
