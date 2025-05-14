using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class ScheduleRepository : IScheduleRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public ScheduleRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Schedule entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO schedule (class_id, quarter_id, subject_id, day_of_week, lesson_number) VALUES (@class_id, @quarter_id, @subject_id, @day_of_week, @lesson_number) RETURNING schedule_id", connection);

            command.Parameters.AddWithValue("@class_id", entity.ClassId);
            command.Parameters.AddWithValue("@quarter_id", entity.QuarterId);
            command.Parameters.AddWithValue("@subject_id", entity.SubjectId);
            command.Parameters.AddWithValue("@day_of_week", entity.DayOfWeek);
            command.Parameters.AddWithValue("@lesson_number", entity.LessonNumber);

            connection.Open();
            entity.ScheduleId = (int)command.ExecuteScalar();
            _notifier.NotifyScheduleChanged();
        }

        public Schedule GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT s.schedule_id, s.class_id, s.quarter_id, s.subject_id, s.day_of_week, s.lesson_number,
                         c.class_id, c.number, c.letter,
                         q.quarter_id, q.quarter_number, q.academic_year,
                         sub.subject_id, sub.subject_name
                  FROM schedule s
                  JOIN class c ON s.class_id = c.class_id
                  JOIN quarter q ON s.quarter_id = q.quarter_id
                  JOIN subject sub ON s.subject_id = sub.subject_id
                  WHERE s.schedule_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Schedule
                {
                    ScheduleId = reader.GetInt32(0),
                    ClassId = reader.GetInt32(1),
                    QuarterId = reader.GetInt32(2),
                    SubjectId = reader.GetInt32(3),
                    DayOfWeek = reader.GetString(4),
                    LessonNumber = reader.GetInt32(5),
                    Class = new Class
                    {
                        ClassId = reader.GetInt32(6),
                        Number = reader.GetInt32(7),
                        Letter = reader.GetString(8)
                    },
                    Quarter = new Quarter
                    {
                        QuarterId = reader.GetInt32(9),
                        QuarterNumber = reader.GetInt32(10),
                        AcademicYear = reader.GetString(11)
                    },
                    Subject = new Subject
                    {
                        SubjectId = reader.GetInt32(12),
                        SubjectName = reader.GetString(13)
                    }
                };
            }

            return null;
        }

        public IEnumerable<Schedule> GetAll()
        {
            var result = new List<Schedule>();

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT s.schedule_id, s.class_id, s.quarter_id, s.subject_id, s.day_of_week, s.lesson_number,
                         c.class_id, c.number, c.letter,
                         q.quarter_id, q.quarter_number, q.academic_year,
                         sub.subject_id, sub.subject_name
                  FROM schedule s
                  JOIN class c ON s.class_id = c.class_id
                  JOIN quarter q ON s.quarter_id = q.quarter_id
                  JOIN subject sub ON s.subject_id = sub.subject_id", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new Schedule
                {
                    ScheduleId = reader.GetInt32(0),
                    ClassId = reader.GetInt32(1),
                    QuarterId = reader.GetInt32(2),
                    SubjectId = reader.GetInt32(3),
                    DayOfWeek = reader.GetString(4),
                    LessonNumber = reader.GetInt32(5),
                    Class = new Class
                    {
                        ClassId = reader.GetInt32(6),
                        Number = reader.GetInt32(7),
                        Letter = reader.GetString(8)
                    },
                    Quarter = new Quarter
                    {
                        QuarterId = reader.GetInt32(9),
                        QuarterNumber = reader.GetInt32(10),
                        AcademicYear = reader.GetString(11)
                    },
                    Subject = new Subject
                    {
                        SubjectId = reader.GetInt32(12),
                        SubjectName = reader.GetString(13)
                    }
                });
            }

            return result;
        }

        public void Update(Schedule entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE schedule SET class_id = @class_id, quarter_id = @quarter_id, subject_id = @subject_id, day_of_week = @day_of_week, lesson_number = @lesson_number WHERE schedule_id = @id", connection);

            command.Parameters.AddWithValue("@class_id", entity.ClassId);
            command.Parameters.AddWithValue("@quarter_id", entity.QuarterId);
            command.Parameters.AddWithValue("@subject_id", entity.SubjectId);
            command.Parameters.AddWithValue("@day_of_week", entity.DayOfWeek);
            command.Parameters.AddWithValue("@lesson_number", entity.LessonNumber);
            command.Parameters.AddWithValue("@id", entity.ScheduleId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyScheduleChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand("DELETE FROM schedule WHERE schedule_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyScheduleChanged();
        }
    }
}