using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System;
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
            if (string.IsNullOrWhiteSpace(entity.FullName))
            {
                throw new ArgumentException("FullName cannot be null or empty.");
            }

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO teacher (full_name, is_active, photo_path, cabinet_number, class_id) " +
                "VALUES (@full_name, @is_active, @photo_path, @cabinet_number, @class_id) RETURNING teacher_id", connection);

            command.Parameters.AddWithValue("@full_name", entity.FullName);
            command.Parameters.AddWithValue("@is_active", entity.IsActive);
            command.Parameters.AddWithValue("@photo_path", (object)entity.PhotoPath ?? DBNull.Value);
            command.Parameters.AddWithValue("@cabinet_number", (object)entity.CabinetNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@class_id", (object)entity.ClassId ?? DBNull.Value);

            try
            {
                connection.Open();
                entity.TeacherId = (int)command.ExecuteScalar();
                _notifier.NotifyTeacherChanged();
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in Create: {ex.Message}");
                throw;
            }
        }

        public Teacher GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var command = new NpgsqlCommand(
                "SELECT teacher_id, full_name, is_active, photo_path, cabinet_number, class_id FROM teacher WHERE teacher_id = @id", connection);
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
                        PhotoPath = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CabinetNumber = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        ClassId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                        TeacherSubjects = new List<TeacherSubject>()
                    };
                }
            }

            if (teacher != null)
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

            return teacher;
        }

        public IEnumerable<Teacher> GetAll()
        {
            var teachers = new List<Teacher>();
            using var connection = new NpgsqlConnection(_connectionString);
            try
            {
                connection.Open();
                using var command = new NpgsqlCommand(
                    "SELECT teacher_id, full_name, is_active, photo_path, cabinet_number, class_id FROM teacher", connection);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        teachers.Add(new Teacher
                        {
                            TeacherId = reader.GetInt32(0),
                            FullName = reader.GetString(1),
                            IsActive = reader.GetBoolean(2),
                            PhotoPath = reader.IsDBNull(3) ? null : reader.GetString(3),
                            CabinetNumber = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                            ClassId = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                            TeacherSubjects = new List<TeacherSubject>()
                        });
                    }
                }

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
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Error loading teachers: {ex.Message}");
                throw;
            }

            return teachers;
        }

        public void Update(Teacher entity)
        {
            if (string.IsNullOrWhiteSpace(entity.FullName))
            {
                throw new ArgumentException("FullName cannot be null or empty.");
            }

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE teacher SET full_name = @full_name, is_active = @is_active, " +
                "photo_path = @photo_path, cabinet_number = @cabinet_number, class_id = @class_id " +
                "WHERE teacher_id = @id", connection);

            command.Parameters.AddWithValue("@full_name", entity.FullName);
            command.Parameters.AddWithValue("@is_active", entity.IsActive);
            command.Parameters.AddWithValue("@photo_path", (object)entity.PhotoPath ?? DBNull.Value);
            command.Parameters.AddWithValue("@cabinet_number", (object)entity.CabinetNumber ?? DBNull.Value);
            command.Parameters.AddWithValue("@class_id", (object)entity.ClassId ?? DBNull.Value);
            command.Parameters.AddWithValue("@id", entity.TeacherId);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                _notifier.NotifyTeacherChanged();
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in Update: {ex.Message}");
                throw;
            }
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "DELETE FROM teacher WHERE teacher_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                _notifier.NotifyTeacherChanged();
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database error in Delete: {ex.Message}");
                throw;
            }
        }
    }
}