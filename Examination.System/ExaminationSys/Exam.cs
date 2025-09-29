using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ExaminationSystem
{
    public enum ExamMode { Starting, Queued, Finished }

    public abstract class Exam
    {
        public int Time { get; set; }
        public int NumberOfQuestions => Questions.Count;
        public QuestionList Questions { get; protected set; }
        public Dictionary<Question, AnswerList> UserAnswers { get; protected set; }
        public Subject Subject { get; protected set; }
        public ExamMode Mode { get; protected set; }
        public event EventHandler<string> ExamStarted;
        public event EventHandler<string> ExamFinished;

        protected Exam(int time, Subject subject, string logFile)
        {
            Time = time;
            Subject = subject;
            Questions = new QuestionList(logFile);
            UserAnswers = new Dictionary<Question, AnswerList>();
            Mode = ExamMode.Queued;

            // Subscribe all students to exam events
            foreach (var student in subject.Students)
            {
                ExamStarted += (sender, msg) => student.Notify(msg);
                ExamFinished += (sender, msg) => student.Notify(msg);
            }
        }

        public void StartExam()
        {
            Mode = ExamMode.Starting;
            ExamStarted?.Invoke(this, $"{Subject.Name} exam has started.");
            Mode = ExamMode.Finished;
            ExamFinished?.Invoke(this, $"{Subject.Name} exam has finished.");
        }

        public abstract void ShowExam();
    }

    public class PracticeExam : Exam
    {
        public PracticeExam(int time, Subject subject, string questionFile)
            : base(time, subject, questionFile) { }

        public override void ShowExam()
        {
            // This method will be handled by the WPF UI
            // The actual exam logic will be in the ExamWindow
        }
    }

    public class FinalExam : Exam
    {
        public FinalExam(int time, Subject subject, string questionFile)
            : base(time, subject, questionFile) { }

        public override void ShowExam()
        {
            // This method will be handled by the WPF UI
            // The actual exam logic will be in the ExamWindow
        }
    }

    public class AnswerComparer : IEqualityComparer<Answer>
    {
        public bool Equals(Answer? x, Answer? y) =>
            x?.Content.Trim().Equals(y?.Content.Trim(), StringComparison.OrdinalIgnoreCase) ?? false;

        public int GetHashCode(Answer? obj) =>
            obj?.Content.Trim().ToLower().GetHashCode() ?? 0;
    }
}
