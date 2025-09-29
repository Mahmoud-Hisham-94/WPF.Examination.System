using System.Collections.Generic;

namespace ExaminationSystem
{
    public class AnswerList : List<Answer>
    {
        public AnswerList() { }

        public AnswerList(AnswerList other)
        {
            foreach (var answer in other)
            {
                Add(new Answer(answer.Content));
            }
        }
    }
}
