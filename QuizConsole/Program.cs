using QuizConsole;
using Spectre.Console;

Console.Title = "Quiz Console";

// Quiz selection

var quizFile = args.FirstOrDefault();

Quiz quiz;

while (true)
{
    quizFile ??= AnsiConsole.Prompt(new SelectionPrompt<string>()
        .Title("Select a [green]quiz[/]: ")
        .MoreChoicesText("[grey](Move up and down to reveal more quizzes)[/]")
        .AddChoices(Directory.GetFiles("Quiz")));

    var deserializer = new YamlDotNet.Serialization.Deserializer();

    try
    {
        using var reader = new StreamReader(quizFile);
        
        quiz = deserializer.Deserialize<Quiz>(reader);
    }
    catch (YamlDotNet.Core.YamlException ex)
    {
        AnsiConsole.WriteLine(ex.ToString());
        AnsiConsole.WriteLine();

        if (AnsiConsole.Confirm("Restart?"))
        {
            AnsiConsole.Clear();
            quizFile = null;
            continue;
        }
        else
        {
            return;
        }
    }
    catch (Exception ex)
    {
        AnsiConsole.WriteException(ex);
        AnsiConsole.WriteLine();

        if (AnsiConsole.Confirm("Restart?"))
        {
            AnsiConsole.Clear();
            quizFile = null;
            continue;
        }
        else
        {
            return;
        }
    }

    break;
}

// Quiz action

while (true)
{
    // Question count

    var questionCount = AnsiConsole.Prompt(new TextPrompt<int>("How many questions?")
        .DefaultValue(quiz.Questions.Length)
        .Validate(number => number > 0 ? ValidationResult.Success() : ValidationResult.Error("Must be greater than 0")));

    Console.Clear();

    // Is revisit?

    var revisit = AnsiConsole.Prompt(new TextPrompt<bool>("Revisit incorrectly answered questions?").DefaultValue(true));

    Console.Clear();

    var score = 0;
    var questionCounter = 0; // unlike pickedQuestions index can vary depending on revisit questions

    var pickedQuestions = quiz.Questions.OrderBy(x => Random.Shared.Next()).Take(questionCount).ToList();
    var revisitQuestions = new HashSet<int>();

    for (int i = 0; i < pickedQuestions.Count; i++)
    {
        var question = pickedQuestions[i];
        var isRevisitQuestion = revisitQuestions.Contains(i);

        if (!isRevisitQuestion)
        {
            questionCounter++;
        }

        if (AskQuestion(question))
        {
            if (!isRevisitQuestion)
            {
                score++;
            }
        }
        else if (revisit)
        {
            var indexToInsert = Math.Min(pickedQuestions.Count - 1, i + 2 + Random.Shared.Next(5));
            pickedQuestions.Insert(indexToInsert, question);
            revisitQuestions.Add(indexToInsert);
        }

        if (score == questionCounter)
        {
            AnsiConsole.MarkupLine($"Score: [green]{score}[/]/[green]{questionCounter}[/]/{questionCount}");
        }
        else
        {
            AnsiConsole.MarkupLine($"Score: [yellow]{score}[/]/[yellow]{questionCounter}[/]/{questionCount}");
        }

        AnsiConsole.WriteLine();

        AnsiConsole.Write("Press any key to continue...");
        Console.ReadKey();
        AnsiConsole.Clear();
    }
}

static bool AskQuestion(Entry question)
{
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
    return correctAnswers.Count == selectedAnswers.Count && !correctAnswers.Except(selectedAnswers).Any();
}