using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class ClassRepository : IClassRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public ClassRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Class entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO class (number, letter, teacher_id) VALUES (@number, @letter, @teacher_id) RETURNING class_id", connection);

            command.Parameters.AddWithValue("@number", entity.Number);
            command.Parameters.AddWithValue("@letter", entity.Letter);
            command.Parameters.AddWithValue("@teacher_id", entity.TeacherId ?? (object)DBNull.Value);

            connection.Open();
            entity.ClassId = (int)command.ExecuteScalar();
            _notifier.NotifyClassChanged();
        }

        public Class GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT c.class_id, c.number, c.letter, c.teacher_id,
                         t.teacher_id AS teacher_teacher_id, t.full_name, t.is_active, t.photo_path, t.cabinet_number
                  FROM class c
                  LEFT JOIN teacher t ON c.teacher_id = t.teacher_id
                  WHERE c.class_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapClassFromReader(reader);
            }

            return null;
        }

        public IEnumerable<Class> GetAll()
        {
            var result = new List<Class>();

            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT c.class_id, c.number, c.letter, c.teacher_id,
                         t.teacher_id AS teacher_teacher_id, t.full_name, t.is_active, t.photo_path, t.cabinet_number
                  FROM class c
                  LEFT JOIN teacher t ON c.teacher_id = t.teacher_id", connection);

            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(MapClassFromReader(reader));
            }

            return result;
        }

        public void Update(Class entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE class SET number = @number, letter = @letter, teacher_id = @teacher_id WHERE class_id = @id", connection);

            command.Parameters.AddWithValue("@number", entity.Number);
            command.Parameters.AddWithValue("@letter", entity.Letter);
            command.Parameters.AddWithValue("@teacher_id", entity.TeacherId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@id", entity.ClassId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyClassChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Сначала удаляем связанные записи в schedule
            using var deleteScheduleCommand = new NpgsqlCommand(
                "DELETE FROM schedule WHERE class_id = @id", connection);
            deleteScheduleCommand.Parameters.AddWithValue("@id", id);
            deleteScheduleCommand.ExecuteNonQuery();

            // Затем удаляем класс
            using var deleteClassCommand = new NpgsqlCommand(
                "DELETE FROM class WHERE class_id = @id", connection);
            deleteClassCommand.Parameters.AddWithValue("@id", id);
            deleteClassCommand.ExecuteNonQuery();

            _notifier.NotifyClassChanged();
        }

        private Class MapClassFromReader(NpgsqlDataReader reader)
        {
            var classEntity = new Class
            {
                ClassId = reader.GetInt32(0),
                Number = reader.GetInt32(1),
                Letter = reader.GetString(2),
                TeacherId = reader.IsDBNull(3) ? null : (int?)reader.GetInt32(3)
            };

            // Проверяем, есть ли данные об учителе (LEFT JOIN может вернуть NULL)
            if (!reader.IsDBNull(4)) // teacher_teacher_id
            {
                classEntity.Teacher = new Teacher
                {
                    TeacherId = reader.GetInt32(4),
                    FullName = reader.GetString(5),
                    IsActive = reader.GetBoolean(6),
                    PhotoPath = reader.GetString(7),
                    CabinetNumber = reader.IsDBNull(8) ? null : (int?)reader.GetInt32(8)
                };
            }

            return classEntity;
        }
    }
}