using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExaminationSystem
{
    public class QuestionList : List<Question>
    {
        private readonly string _logFilePath;

        public QuestionList(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public new void Add(Question question)
        {
            if (question == null) return;

            // Initialize answers before adding
            question.InitializeAnswers();
            base.Add(question);

            // Log to JSON file
            QuestionJsonRepository.SaveQuestions(_logFilePath, this);
        }

        public new void AddRange(IEnumerable<Question> questions)
        {
            foreach (var q in questions)
            {
                this.Add(q);
            }
        }
    }
}
