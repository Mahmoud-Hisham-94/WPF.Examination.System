using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ExaminationSystem;

namespace ExaminationSys
{
    public partial class ExamWindow : Window
    {
        private Exam _exam;
        private int _currentQuestionIndex;
        private int _totalScore;
        private int _totalMarks;
        private bool _isPracticeExam;
        private DispatcherTimer _timer;
        private TimeSpan _timeRemaining;
        private List<RadioButton> _radioButtons;
        private List<CheckBox> _checkBoxes;

        public ExamWindow(Exam exam, string examTitle)
        {
            InitializeComponent();
            _exam = exam;
            _isPracticeExam = exam is PracticeExam;
            _currentQuestionIndex = 0;
            _totalScore = 0;
            _totalMarks = 0;
            _radioButtons = new List<RadioButton>();
            _checkBoxes = new List<CheckBox>();

            ExamTitleText.Text = examTitle;
            _timeRemaining = TimeSpan.FromMinutes(_exam.Time);
            
            InitializeTimer();
            LoadQuestion();
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            UpdateTimerDisplay();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _timeRemaining = _timeRemaining.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimerDisplay();

            if (_timeRemaining <= TimeSpan.Zero)
            {
                _timer.Stop();
                MessageBox.Show("Time's up! The exam will be submitted automatically.", "Time Up", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                SubmitExam();
            }
        }

        private void UpdateTimerDisplay()
        {
            TimerText.Text = _timeRemaining.ToString(@"mm\:ss");
            
            // Change color when time is running low
            if (_timeRemaining.TotalMinutes < 5)
            {
                TimerText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (_timeRemaining.TotalMinutes < 10)
            {
                TimerText.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                TimerText.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void LoadQuestion()
        {
            if (_currentQuestionIndex >= _exam.Questions.Count)
            {
                SubmitExam();
                return;
            }

            var question = _exam.Questions[_currentQuestionIndex];
            
            // Update UI elements
            QuestionHeaderText.Text = question.Header;
            QuestionBodyText.Text = question.Body;
            MarksText.Text = $"Marks: {question.Marks}";
            QuestionCounterText.Text = $"Question {_currentQuestionIndex + 1} of {_exam.Questions.Count}";
            
            // Update progress bar
            ProgressBar.Value = ((double)(_currentQuestionIndex + 1) / _exam.Questions.Count) * 100;

            // Clear previous answer options
            AnswerOptionsPanel.Children.Clear();
            _radioButtons.Clear();
            _checkBoxes.Clear();

            // Generate answer options based on question type
            if (question is TrueFalseQuestion)
            {
                CreateTrueFalseOptions(question);
            }
            else if (question is ChooseOneQuestion chooseOne)
            {
                CreateChooseOneOptions(chooseOne);
            }
            else if (question is ChooseAllQuestion chooseAll)
            {
                CreateChooseAllOptions(chooseAll);
            }

            // Show correct answer for practice exam
            if (_isPracticeExam)
            {
                ShowCorrectAnswer(question);
            }

            // Update navigation buttons
            UpdateNavigationButtons();
        }

        private void CreateTrueFalseOptions(Question question)
        {
            var trueButton = new RadioButton
            {
                Content = "True",
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0),
                Tag = 0
            };
            var falseButton = new RadioButton
            {
                Content = "False",
                FontSize = 14,
                Margin = new Thickness(0, 5, 0, 0),
                Tag = 1
            };

            _radioButtons.Add(trueButton);
            _radioButtons.Add(falseButton);

            AnswerOptionsPanel.Children.Add(trueButton);
            AnswerOptionsPanel.Children.Add(falseButton);
        }

        private void CreateChooseOneOptions(ChooseOneQuestion question)
        {
            for (int i = 0; i < question.Choices.Count; i++)
            {
                var radioButton = new RadioButton
                {
                    Content = question.Choices[i],
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 0),
                    Tag = i
                };
                _radioButtons.Add(radioButton);
                AnswerOptionsPanel.Children.Add(radioButton);
            }
        }

        private void CreateChooseAllOptions(ChooseAllQuestion question)
        {
            for (int i = 0; i < question.Choices.Count; i++)
            {
                var checkBox = new CheckBox
                {
                    Content = question.Choices[i],
                    FontSize = 14,
                    Margin = new Thickness(0, 5, 0, 0),
                    Tag = i
                };
                _checkBoxes.Add(checkBox);
                AnswerOptionsPanel.Children.Add(checkBox);
            }
        }

        private void ShowCorrectAnswer(Question question)
        {
            CorrectAnswerPanel.Visibility = Visibility.Visible;
            CorrectAnswerText.Text = string.Join(", ", question.CorrectAnswers.Select(a => a.Content));
        }

        private void UpdateNavigationButtons()
        {
            PreviousButton.IsEnabled = _currentQuestionIndex > 0;
            
            if (_currentQuestionIndex == _exam.Questions.Count - 1)
            {
                NextButton.Visibility = Visibility.Collapsed;
                SubmitButton.Visibility = Visibility.Visible;
            }
            else
            {
                NextButton.Visibility = Visibility.Visible;
                SubmitButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveCurrentAnswer()
        {
            var question = _exam.Questions[_currentQuestionIndex];
            var userAnswer = new AnswerList();

            if (question is TrueFalseQuestion || question is ChooseOneQuestion)
            {
                var selectedRadio = _radioButtons.FirstOrDefault(rb => rb.IsChecked == true);
                if (selectedRadio != null)
                {
                    int index = (int)selectedRadio.Tag;
                    userAnswer.Add(question.Answers[index]);
                }
            }
            else if (question is ChooseAllQuestion)
            {
                foreach (var checkBox in _checkBoxes)
                {
                    if (checkBox.IsChecked == true)
                    {
                        int index = (int)checkBox.Tag;
                        userAnswer.Add(question.Answers[index]);
                    }
                }
            }

            _exam.UserAnswers[question] = userAnswer;
        }

        private void CheckAnswer(Question question, AnswerList userAnswer)
        {
            if (userAnswer.SequenceEqual(question.CorrectAnswers, new AnswerComparer()))
            {
                _totalScore += question.Marks;
            }
            _totalMarks += question.Marks;
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentQuestionIndex > 0)
            {
                SaveCurrentAnswer();
                _currentQuestionIndex--;
                LoadQuestion();
            }
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            SaveCurrentAnswer();
            _currentQuestionIndex++;
            LoadQuestion();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to submit the exam?", "Submit Exam", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                SubmitExam();
            }
        }

        private void SubmitExam()
        {
            _timer?.Stop();
            
            // Save the last answer
            SaveCurrentAnswer();
            
            // Calculate final score
            foreach (var kvp in _exam.UserAnswers)
            {
                CheckAnswer(kvp.Key, kvp.Value);
            }

            // Show results
            var resultsWindow = new ResultsWindow(_exam, _totalScore, _totalMarks, _isPracticeExam);
            resultsWindow.Owner = this;
            resultsWindow.ShowDialog();
            
            this.Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit the exam? Your progress will be lost.", 
                "Exit Exam", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                _timer?.Stop();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
