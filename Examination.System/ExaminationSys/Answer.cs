using System;
using System.Text.Json.Serialization;

namespace ExaminationSystem
{
    public class Answer
    {
        public string Content { get; set; }

        // Parameterless constructor for JSON deserialization
        public Answer() { }

        public Answer(string content)
        {
            Content = content;
        }

        public override string ToString() => Content;
    }
}
