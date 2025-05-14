using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class GradeRepository : IGradeRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public GradeRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Grade entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO grade (student_id, subject_id, quarter_id, grade_value) VALUES (@student_id, @subject_id, @quarter_id, @grade_value) RETURNING grade_id", connection);

            command.Parameters.AddWithValue("@student_id", entity.StudentId);
            command.Parameters.AddWithValue("@subject_id", entity.SubjectId);
            command.Parameters.AddWithValue("@quarter_id", entity.QuarterId);
            command.Parameters.AddWithValue("@grade_value", entity.GradeValue);

            connection.Open();
            entity.GradeId = (int)command.ExecuteScalar();
            _notifier.NotifyGradeChanged();
        }

        public Grade GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT g.grade_id, g.student_id, g.subject_id, g.quarter_id, g.grade_value,
                         s.student_id, s.full_name,
                         sub.subject_id, sub.subject_name,
                         q.quarter_id, q.quarter_number, q.academic_year,
                         t.teacher_id, t.full_name
                  FROM grade g
                  JOIN student s ON g.student_id = s.student_id
                  JOIN subject sub ON g.subject_id = sub.subject_id
                  JOIN quarter q ON g.quarter_id = q.quarter_id
                  JOIN teacher_subject ts ON ts.subject_id = sub.subject_id
                  JOIN teacher t ON ts.teacher_id = t.teacher_id
                  WHERE g.grade_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var grade = new Grade
                {
                    GradeId = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    SubjectId = reader.GetInt32(2),
                    QuarterId = reader.GetInt32(3),
                    GradeValue = reader.GetInt32(4),
                    Student = new Student { StudentId = reader.GetInt32(5), FullName = reader.GetString(6) },
                    Subject = new Subject { SubjectId = reader.GetInt32(7), SubjectName = reader.GetString(8) },
                    Quarter = new Quarter { QuarterId = reader.GetInt32(9), QuarterNumber = reader.GetInt32(10), AcademicYear = reader.GetString(11) }
                };
                grade.Subject.TeacherSubjects = new List<TeacherSubject>
                {
                    new TeacherSubject { TeacherId = reader.GetInt32(12), SubjectId = grade.SubjectId }
                };
                return grade;
            }
            return null;
        }

        public IEnumerable<Grade> GetAll()
        {
            var grades = new List<Grade>();
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT g.grade_id, g.student_id, g.subject_id, g.quarter_id, g.grade_value,
                         s.student_id, s.full_name,
                         sub.subject_id, sub.subject_name,
                         q.quarter_id, q.quarter_number, q.academic_year,
                         t.teacher_id, t.full_name
                  FROM grade g
                  JOIN student s ON g.student_id = s.student_id
                  JOIN subject sub ON g.subject_id = sub.subject_id
                  JOIN quarter q ON g.quarter_id = q.quarter_id
                  JOIN teacher_subject ts ON ts.subject_id = sub.subject_id
                  JOIN teacher t ON ts.teacher_id = t.teacher_id", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var grade = new Grade
                {
                    GradeId = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    SubjectId = reader.GetInt32(2),
                    QuarterId = reader.GetInt32(3),
                    GradeValue = reader.GetInt32(4),
                    Student = new Student { StudentId = reader.GetInt32(5), FullName = reader.GetString(6) },
                    Subject = new Subject { SubjectId = reader.GetInt32(7), SubjectName = reader.GetString(8) },
                    Quarter = new Quarter { QuarterId = reader.GetInt32(9), QuarterNumber = reader.GetInt32(10), AcademicYear = reader.GetString(11) }
                };
                grade.Subject.TeacherSubjects = new List<TeacherSubject>
                {
                    new TeacherSubject { TeacherId = reader.GetInt32(12), SubjectId = grade.SubjectId }
                };
                grades.Add(grade);
            }
            return grades;
        }

        public IEnumerable<Grade> GetByStudentId(int studentId)
        {
            var grades = new List<Grade>();
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT g.grade_id, g.student_id, g.subject_id, g.quarter_id, g.grade_value,
                         sub.subject_id, sub.subject_name,
                         q.quarter_id, q.quarter_number, q.academic_year,
                         t.teacher_id, t.full_name
                  FROM grade g
                  JOIN subject sub ON g.subject_id = sub.subject_id
                  JOIN quarter q ON g.quarter_id = q.quarter_id
                  JOIN teacher_subject ts ON ts.subject_id = sub.subject_id
                  JOIN teacher t ON ts.teacher_id = t.teacher_id
                  WHERE g.student_id = @student_id", connection);

            command.Parameters.AddWithValue("@student_id", studentId);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var grade = new Grade
                {
                    GradeId = reader.GetInt32(0),
                    StudentId = reader.GetInt32(1),
                    SubjectId = reader.GetInt32(2),
                    QuarterId = reader.GetInt32(3),
                    GradeValue = reader.GetInt32(4),
                    Subject = new Subject { SubjectId = reader.GetInt32(5), SubjectName = reader.GetString(6) },
                    Quarter = new Quarter { QuarterId = reader.GetInt32(7), QuarterNumber = reader.GetInt32(8), AcademicYear = reader.GetString(9) }
                };
                grade.Subject.TeacherSubjects = new List<TeacherSubject>
                {
                    new TeacherSubject { TeacherId = reader.GetInt32(10), SubjectId = grade.SubjectId }
                };
                grades.Add(grade);
            }
            return grades;
        }

        public void Update(Grade entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE grade SET student_id = @student_id, subject_id = @subject_id, quarter_id = @quarter_id, grade_value = @grade_value WHERE grade_id = @id", connection);

            command.Parameters.AddWithValue("@student_id", entity.StudentId);
            command.Parameters.AddWithValue("@subject_id", entity.SubjectId);
            command.Parameters.AddWithValue("@quarter_id", entity.QuarterId);
            command.Parameters.AddWithValue("@grade_value", entity.GradeValue);
            command.Parameters.AddWithValue("@id", entity.GradeId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyGradeChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "DELETE FROM grade WHERE grade_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyGradeChanged();
        }
    }
}