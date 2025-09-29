using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ExaminationSystem
{
    internal class QuestionJsonRepository
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(), new AnswerJsonConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static void SaveQuestions(string filePath, List<Question> questions)
        {
            try
            {
                string json = JsonSerializer.Serialize(questions, _options);
                File.WriteAllText(filePath, json);
                System.Diagnostics.Debug.WriteLine($"Successfully saved questions to {filePath}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving questions: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        public static List<Question> LoadQuestions(string filePath)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                
                // First, parse the JSON to get raw data
                var jsonDocument = JsonDocument.Parse(json);
                var questions = new List<Question>();
                
                foreach (var questionElement in jsonDocument.RootElement.EnumerateArray())
                {
                    Question question = null;
                    
                    if (questionElement.TryGetProperty("$type", out var typeElement))
                    {
                        var type = typeElement.GetString();
                        
                        switch (type)
                        {
                            case "TrueFalse":
                                question = new TrueFalseQuestion();
                                break;
                            case "ChooseOne":
                                question = new ChooseOneQuestion();
                                break;
                            case "ChooseAll":
                                question = new ChooseAllQuestion();
                                break;
                        }
                    }
                    
                    if (question != null)
                    {
                        // Set basic properties
                        if (questionElement.TryGetProperty("Header", out var headerElement))
                            question.Header = headerElement.GetString();
                        if (questionElement.TryGetProperty("Body", out var bodyElement))
                            question.Body = bodyElement.GetString();
                        if (questionElement.TryGetProperty("Marks", out var marksElement))
                            question.Marks = marksElement.GetInt32();
                        
                        // Set choices for ChooseOne and ChooseAll questions
                        if (question is ChooseOneQuestion chooseOne && questionElement.TryGetProperty("Choices", out var choicesElement))
                        {
                            chooseOne.Choices = choicesElement.EnumerateArray().Select(x => x.GetString()).ToList();
                        }
                        else if (question is ChooseAllQuestion chooseAll && questionElement.TryGetProperty("Choices", out var choicesElement2))
                        {
                            chooseAll.Choices = choicesElement2.EnumerateArray().Select(x => x.GetString()).ToList();
                        }
                        
                        // Set correct answers (supports both string and object forms)
                        if (questionElement.TryGetProperty("CorrectAnswers", out var correctAnswersElement))
                        {
                            question.CorrectAnswers.Clear();
                            foreach (var answerElement in correctAnswersElement.EnumerateArray())
                            {
                                switch (answerElement.ValueKind)
                                {
                                    case JsonValueKind.String:
                                        question.CorrectAnswers.Add(new Answer(answerElement.GetString()));
                                        break;
                                    case JsonValueKind.Object:
                                        if (answerElement.TryGetProperty("Content", out var contentEl))
                                        {
                                            question.CorrectAnswers.Add(new Answer(contentEl.GetString()));
                                        }
                                        else
                                        {
                                            // Fallback: store raw object string
                                            question.CorrectAnswers.Add(new Answer(answerElement.ToString()));
                                        }
                                        break;
                                    default:
                                        // Fallback for other types (numbers, bools, null)
                                        question.CorrectAnswers.Add(new Answer(answerElement.ToString()));
                                        break;
                                }
                            }
                        }
                        
                        // Initialize answers for the question
                        question.InitializeAnswers();
                        questions.Add(question);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Successfully loaded {questions.Count} questions from {filePath}");
                return questions;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading questions: {ex.Message}", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return new List<Question>();
            }
        }
    }
}
