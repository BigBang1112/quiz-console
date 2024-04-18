using YamlDotNet.Serialization;

namespace QuizConsole;

[YamlSerializable]
class Entry
{
    public string Question { get; set; } = "";
    public string[] Answers { get; set; } = [];
    public int[] Correct { get; set; } = [];
}
