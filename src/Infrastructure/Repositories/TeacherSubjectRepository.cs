using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class TeacherSubjectRepository : ITeacherSubjectRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public TeacherSubjectRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(TeacherSubject entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO teacher_subject (teacher_id, subject_id) VALUES (@teacher_id, @subject_id)", connection);

            command.Parameters.AddWithValue("@teacher_id", entity.TeacherId);
            command.Parameters.AddWithValue("@subject_id", entity.SubjectId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyTeacherSubjectChanged();
        }

        public TeacherSubject GetById(int teacherId, int subjectId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "SELECT teacher_id, subject_id FROM teacher_subject WHERE teacher_id = @teacher_id AND subject_id = @subject_id", connection);

            command.Parameters.AddWithValue("@teacher_id", teacherId);
            command.Parameters.AddWithValue("@subject_id", subjectId);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new TeacherSubject
                {
                    TeacherId = reader.GetInt32(0),
                    SubjectId = reader.GetInt32(1)
                };
            }

            return null;
        }

        public IEnumerable<TeacherSubject> GetAll()
        {
            var result = new List<TeacherSubject>();

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand("SELECT teacher_id, subject_id FROM teacher_subject", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(new TeacherSubject
                {
                    TeacherId = reader.GetInt32(0),
                    SubjectId = reader.GetInt32(1)
                });
            }

            return result;
        }

        public void Delete(int teacherId, int subjectId)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand("DELETE FROM teacher_subject WHERE teacher_id = @teacher_id AND subject_id = @subject_id", connection);

            command.Parameters.AddWithValue("@teacher_id", teacherId);
            command.Parameters.AddWithValue("@subject_id", subjectId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyTeacherSubjectChanged();
        }
    }
}