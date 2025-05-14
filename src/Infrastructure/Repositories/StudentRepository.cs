using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public StudentRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Student entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO student (full_name, class_id, photo_path) VALUES (@full_name, @class_id, @photo_path)", connection);

            command.Parameters.AddWithValue("@full_name", entity.FullName);
            command.Parameters.AddWithValue("@class_id", entity.ClassId);
            command.Parameters.AddWithValue("@photo_path", entity.PhotoPath ?? (object)DBNull.Value);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyStudentChanged();
        }

        public Student GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "SELECT s.student_id, s.full_name, s.class_id, s.photo_path, c.number, c.letter " +
                "FROM student s JOIN class c ON s.class_id = c.class_id WHERE s.student_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                var student = new Student
                {
                    StudentId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    ClassId = reader.GetInt32(2),
                    PhotoPath = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Class = new Class { ClassId = reader.GetInt32(2), Number = reader.GetInt32(4), Letter = reader.GetString(5) }
                };
                reader.Close(); // Close reader before new query

                // Calculate average grade
                student.AverageGrade = CalculateAverageGrade(id, connection);

                return student;
            }

            return null;
        }

        public IEnumerable<Student> GetAll()
        {
            var result = new List<Student>();

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "SELECT s.student_id, s.full_name, s.class_id, s.photo_path, c.number, c.letter " +
                "FROM student s JOIN class c ON s.class_id = c.class_id", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Student
                {
                    StudentId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    ClassId = reader.GetInt32(2),
                    PhotoPath = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Class = new Class { ClassId = reader.GetInt32(2), Number = reader.GetInt32(4), Letter = reader.GetString(5) }
                });
            }
            reader.Close(); 

            foreach (var student in result)
            {
                student.AverageGrade = CalculateAverageGrade(student.StudentId, connection);
            }

            return result;
        }

        public void Update(Student entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE student SET full_name = @full_name, class_id = @class_id, photo_path = @photo_path WHERE student_id = @id", connection);

            command.Parameters.AddWithValue("@full_name", entity.FullName);
            command.Parameters.AddWithValue("@class_id", entity.ClassId);
            command.Parameters.AddWithValue("@photo_path", entity.PhotoPath ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@id", entity.StudentId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyStudentChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand("DELETE FROM student WHERE student_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyStudentChanged();
        }

        private double? CalculateAverageGrade(int studentId, NpgsqlConnection connection)
        {
            using var command = new NpgsqlCommand(
                "SELECT AVG(grade_value) FROM grade WHERE student_id = @student_id", connection);
            command.Parameters.AddWithValue("@student_id", studentId);

            var result = command.ExecuteScalar();
            if (result == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDouble(result); 
        }
    }
}