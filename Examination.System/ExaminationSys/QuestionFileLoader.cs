using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExaminationSystem
{
    public static class QuestionFileLoader
    {
        public static void LoadQuestions(string path, QuestionList list)
        {
            var lines = File.ReadAllLines(path);
            string header = "", body = "", type = "";
            int marks = 0;
            List<string> choices = null;
            List<string> correct = null;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (type == "TrueFalse")
                    {
                        var q = new TrueFalseQuestion(header, body, marks);
                        if (correct != null)
                            q.CorrectAnswers.Add(new Answer(correct[0]));
                        list.Add(q);
                    }
                    else if (type == "ChooseOne")
                    {
                        var q = new ChooseOneQuestion(header, body, marks, choices);
                        if (correct != null)
                            q.CorrectAnswers.Add(new Answer(correct[0]));
                        list.Add(q);
                    }
                    else if (type == "ChooseAll")
                    {
                        var q = new ChooseAllQuestion(header, body, marks, choices);
                        if (correct != null)
                            foreach (var ans in correct)
                                q.CorrectAnswers.Add(new Answer(ans));
                        list.Add(q);
                    }

                    // Reset for next question
                    header = body = type = "";
                    marks = 0;
                    choices = new List<string>();
                    correct = new List<string>();
                    continue;
                }

                if (line.StartsWith("Header: "))
                    header = line.Substring(8).Trim();
                else if (line.StartsWith("Body: "))
                    body = line.Substring(6).Trim();
                else if (line.StartsWith("Type: "))
                    type = line.Substring(6).Trim();
                else if (line.StartsWith("Marks: "))
                    marks = int.Parse(line.Substring(7).Trim());
                else if (line.StartsWith("Choices: "))
                    choices = line.Substring(9).Split(',').Select(c => c.Trim()).ToList();
                else if (line.StartsWith("Correct: "))
                    correct = line.Substring(9).Split(',').Select(c => c.Trim()).ToList();
            }
        }
    }
}
