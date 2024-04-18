using YamlDotNet.Serialization;

namespace QuizConsole;

[YamlSerializable]
class Quiz
{
    public Entry[] Questions { get; set; } = [];
}
