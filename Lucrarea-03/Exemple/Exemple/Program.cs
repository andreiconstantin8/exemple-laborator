﻿using Exemple.Domain.Models;
using System;
using System.Collections.Generic;
using static Exemple.Domain.Models.ExamGrades;
using static Exemple.Domain.ExamGradesOperation;
using Exemple.Domain;
using System.Threading.Tasks;
using LanguageExt;

namespace Exemple
{
    class Program
    {
        private static readonly Random random = new Random();

        static async Task Main(string[] args)
        {
            var listOfGrades = ReadListOfGrades().ToArray();
            PublishGradesCommand command = new(listOfGrades);
            PublishGradeWorkflow workflow = new();
            var result = await workflow.ExecuteAsync(command, CheckStudentExists);

            result.Match(
                    whenExamGradesPublishFaildEvent: @event =>
                    {
                        Console.WriteLine($"Publish failed: {@event.Reason}");
                        return @event;
                    },
                    whenExamGradesPublishScucceededEvent: @event =>
                    {
                        Console.WriteLine($"Publish succeeded.");
                        Console.WriteLine(@event.Csv);
                        return @event;
                    }
                );
        }

        private static List<UnvalidatedStudentGrade> ReadListOfGrades()
        {
            List <UnvalidatedStudentGrade> listOfGrades = new();
            do
            {
                //read registration number and grade and create a list of greads
                var registrationNumber = ReadValue("Registration Number: ");
                if (string.IsNullOrEmpty(registrationNumber))
                {
                    break;
                }

                var examGrade = ReadValue("Exam Grade: ");
                if (string.IsNullOrEmpty(examGrade))
                {
                    break;
                }

                var activityGrade = ReadValue("Activity Grade: ");
                if (string.IsNullOrEmpty(activityGrade))
                {
                    break;
                }

                listOfGrades.Add(new (registrationNumber, examGrade, activityGrade));
            } while (true);
            return listOfGrades;
        }

        private static string? ReadValue(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        private static TryAsync<bool> CheckStudentExists(StudentRegistrationNumber student) => async () => true;
    }
}
