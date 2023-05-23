using QuizConsole;
using Spectre.Console;

var quizFile = args.FirstOrDefault();

Console.Title = "Quiz Console";

quizFile ??= AnsiConsole.Prompt(new SelectionPrompt<string>()
    .Title("Select a [green]quiz[/]: ")
    .MoreChoicesText("[grey](Move up and down to reveal more quizzes)[/]")
    .AddChoices(Directory.GetFiles("Quiz")));

var deserializer = new YamlDotNet.Serialization.Deserializer();

Quiz quiz;

using (var reader = new StreamReader(quizFile))
{
    quiz = deserializer.Deserialize<Quiz>(reader);
}

while (true)
{
    var score = 0;
    var questionCounter = 0;

    foreach (var question in quiz.Questions.OrderBy(x => Random.Shared.Next()))
    {
        questionCounter++;

        var answers = question.Answers.OrderBy(x => Random.Shared.Next()).ToArray();
        var correctAnswers = new HashSet<string>();

        foreach (var correct in question.Correct)
        {
            correctAnswers.Add(question.Answers[correct]);
        }

        var selectedAnswers = AnsiConsole.Prompt(new MultiSelectionPrompt<string>()
            .Title(question.Question)
            .AddChoices(answers));

        AnsiConsole.WriteLine(question.Question);
        AnsiConsole.WriteLine();

        foreach (var selectedAnswer in selectedAnswers)
        {
            if (correctAnswers.Contains(selectedAnswer))
            {
                AnsiConsole.MarkupLine($"- [green]{selectedAnswer} (CORRECT)[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"- [red]{selectedAnswer} (WRONG)[/]");
            }
        }

        foreach (var correctAnswer in correctAnswers.Where(x => !selectedAnswers.Contains(x)))
        {
            AnsiConsole.MarkupLine($"- [yellow]{correctAnswer} (MISSING)[/]");
        }

        AnsiConsole.WriteLine();

        // if all answers are correct
        if (correctAnswers.Count == selectedAnswers.Count && !correctAnswers.Except(selectedAnswers).Any())
        {
            score++;
        }

        if (score == questionCounter)
        {
            AnsiConsole.MarkupLine($"Score: [green]{score}[/]/[green]{questionCounter}[/]/{quiz.Questions.Length}");
        }
        else
        {
            AnsiConsole.MarkupLine($"Score: [yellow]{score}[/]/[yellow]{questionCounter}[/]/{quiz.Questions.Length}");
        }

        AnsiConsole.WriteLine();

        AnsiConsole.Write("Press any key to continue...");
        Console.ReadKey();
        AnsiConsole.Clear();
    }
}