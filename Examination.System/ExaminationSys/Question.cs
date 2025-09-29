using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ExaminationSystem
{
    [JsonDerivedType(typeof(TrueFalseQuestion), typeDiscriminator: "TrueFalse")]
    [JsonDerivedType(typeof(ChooseOneQuestion), typeDiscriminator: "ChooseOne")]
    [JsonDerivedType(typeof(ChooseAllQuestion), typeDiscriminator: "ChooseAll")]
    public abstract class Question : ICloneable, IComparable<Question>
    {
        public string Header { get; set; }
        public string Body { get; set; }
        public int Marks { get; set; }

        [JsonIgnore]
        public AnswerList Answers { get; } = new AnswerList();

        public AnswerList CorrectAnswers { get; set; } = new AnswerList();

        protected Question() { } // Required for JSON deserialization

        protected Question(string header, string body, int marks)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Body = body ?? throw new ArgumentNullException(nameof(body));
            Marks = marks >= 0 ? marks : throw new ArgumentException("Marks cannot be negative");
        }

        protected Question(Question other) : this(other.Header, other.Body, other.Marks)
        {
            CorrectAnswers = new AnswerList(other.CorrectAnswers);
        }

        public abstract object Clone();
        public abstract void InitializeAnswers();

        public int CompareTo(Question? other) => other == null ? 1 : Marks.CompareTo(other.Marks);

        public override bool Equals(object? obj) => obj is Question other &&
            Header == other.Header &&
            Body == other.Body &&
            Marks == other.Marks &&
            Answers.SequenceEqual(other.Answers) &&
            CorrectAnswers.SequenceEqual(other.CorrectAnswers);

        public override int GetHashCode() => HashCode.Combine(Header, Body, Marks);
    }

    public class TrueFalseQuestion : Question
    {
        public TrueFalseQuestion() => InitializeAnswers();

        public TrueFalseQuestion(string header, string body, int marks) : base(header, body, marks)
            => InitializeAnswers();

        public override object Clone() => new TrueFalseQuestion();

        public override void InitializeAnswers()
        {
            Answers.Clear();
            Answers.Add(new Answer("True"));
            Answers.Add(new Answer("False"));
        }

        public override string ToString() =>
            $"{Header}\n{Body}\n(True/False) Correct: {CorrectAnswers.FirstOrDefault()}";
    }

    public class ChooseOneQuestion : Question
    {
        public List<string> Choices { get; set; } = new List<string>();

        public ChooseOneQuestion() : base("", "", 0) { Choices = new List<string>(); } // For JSON

        public ChooseOneQuestion(string header, string body, int marks, List<string> choices)
            : base(header, body, marks)
        {
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
            InitializeAnswers();
        }
        public override object Clone() => new ChooseOneQuestion();

        public override void InitializeAnswers()
        {
            Answers.Clear();
            foreach (var choice in Choices)
                Answers.Add(new Answer(choice));
        }

        public override string ToString() =>
            $"{Header}\n{Body}\nChoices: {string.Join(", ", Choices)}\nCorrect: {CorrectAnswers.FirstOrDefault()}";
    }

    public class ChooseAllQuestion : Question
    {
        public List<string> Choices { get; set; } = new List<string>();

        public ChooseAllQuestion() : base("", "", 0) { Choices = new List<string>(); } // For JSON

        public ChooseAllQuestion(string header, string body, int marks, List<string> choices)
            : base(header, body, marks)
        {
            Choices = choices ?? throw new ArgumentNullException(nameof(choices));
            InitializeAnswers();
        }

        public override object Clone() => new ChooseAllQuestion();

        public override void InitializeAnswers()
        {
            Answers.Clear();
            foreach (var choice in Choices)
                Answers.Add(new Answer(choice));
        }

        public override string ToString() =>
            $"{Header}\n{Body}\nChoices: {string.Join(", ", Choices)}\nCorrect: {string.Join(", ", CorrectAnswers)}";
    }
}
