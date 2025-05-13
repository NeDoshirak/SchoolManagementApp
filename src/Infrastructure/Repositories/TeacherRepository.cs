using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class TeacherRepository : ITeacherRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public TeacherRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Teacher entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO teacher (full_name, is_active) VALUES (@full_name, @is_active) RETURNING teacher_id", connection);

            command.Parameters.AddWithValue("@full_name", entity.FullName);
            command.Parameters.AddWithValue("@is_active", entity.IsActive);

            connection.Open();
            entity.TeacherId = (int)command.ExecuteScalar();
            _notifier.NotifyTeacherChanged();
        }

        public Teacher GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Сначала получаем основную информацию об учителе
            using var command = new NpgsqlCommand(
                "SELECT teacher_id, full_name, is_active FROM teacher WHERE teacher_id = @id", connection);
            command.Parameters.AddWithValue("@id", id);

            Teacher teacher = null;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    teacher = new Teacher
                    {
                        TeacherId = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        IsActive = reader.GetBoolean(2),
                        TeacherSubjects = new List<TeacherSubject>()
                    };
                }
            }

            if (teacher != null)
            {
                // Загружаем связанные предметы
                using var subjectCommand = new NpgsqlCommand(
                    "SELECT subject_id FROM teacher_subject WHERE teacher_id = @teacher_id", connection);
                subjectCommand.Parameters.AddWithValue("@teacher_id", teacher.TeacherId);

                using var subjectReader = subjectCommand.ExecuteReader();
                while (subjectReader.Read())
                {
                    teacher.TeacherSubjects.Add(new TeacherSubject
                    {
                        TeacherId = teacher.TeacherId,
                        SubjectId = subjectReader.GetInt32(0)
                    });
                }
            }

            return teacher;
        }

        public IEnumerable<Teacher> GetAll()
        {
            var teachers = new List<Teacher>();
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Сначала получаем всех учителей
            using var command = new NpgsqlCommand(
                "SELECT teacher_id, full_name, is_active FROM teacher", connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    teachers.Add(new Teacher
                    {
                        TeacherId = reader.GetInt32(0),
                        FullName = reader.GetString(1),
                        IsActive = reader.GetBoolean(2),
                        TeacherSubjects = new List<TeacherSubject>()
                    });
                }
            }

            // Затем загружаем связанные предметы для каждого учителя
            foreach (var teacher in teachers)
            {
                using var subjectCommand = new NpgsqlCommand(
                    "SELECT subject_id FROM teacher_subject WHERE teacher_id = @teacher_id", connection);
                subjectCommand.Parameters.AddWithValue("@teacher_id", teacher.TeacherId);

                using var subjectReader = subjectCommand.ExecuteReader();
                while (subjectReader.Read())
                {
                    teacher.TeacherSubjects.Add(new TeacherSubject
                    {
                        TeacherId = teacher.TeacherId,
                        SubjectId = subjectReader.GetInt32(0)
                    });
                }
            }

            return teachers;
        }

        public void Update(Teacher entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE teacher SET full_name = @full_name, is_active = @is_active WHERE teacher_id = @id", connection);

            command.Parameters.AddWithValue("@full_name", entity.FullName);
            command.Parameters.AddWithValue("@is_active", entity.IsActive);
            command.Parameters.AddWithValue("@id", entity.TeacherId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyTeacherChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "DELETE FROM teacher WHERE teacher_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyTeacherChanged();
        }
    }
}