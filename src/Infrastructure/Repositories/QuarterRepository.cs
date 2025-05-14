using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class QuarterRepository : IQuarterRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public QuarterRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Quarter entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO quarter (quarter_number, academic_year) VALUES (@quarter_number, @academic_year) RETURNING quarter_id", connection);

            command.Parameters.AddWithValue("@quarter_number", entity.QuarterNumber);
            command.Parameters.AddWithValue("@academic_year", entity.AcademicYear);

            connection.Open();
            entity.QuarterId = (int)command.ExecuteScalar();
            _notifier.NotifyQuarterChanged();
        }

        public Quarter GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "SELECT quarter_id, quarter_number, academic_year FROM quarter WHERE quarter_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Quarter
                {
                    QuarterId = reader.GetInt32(0),
                    QuarterNumber = reader.GetInt32(1),
                    AcademicYear = reader.GetString(2)
                };
            }
            return null;
        }

        public IEnumerable<Quarter> GetAll()
        {
            var quarters = new List<Quarter>();
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "SELECT quarter_id, quarter_number, academic_year FROM quarter", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                quarters.Add(new Quarter
                {
                    QuarterId = reader.GetInt32(0),
                    QuarterNumber = reader.GetInt32(1),
                    AcademicYear = reader.GetString(2)
                });
            }
            return quarters;
        }

        public void Update(Quarter entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "UPDATE quarter SET quarter_number = @quarter_number, academic_year = @academic_year WHERE quarter_id = @id", connection);

            command.Parameters.AddWithValue("@quarter_number", entity.QuarterNumber);
            command.Parameters.AddWithValue("@academic_year", entity.AcademicYear);
            command.Parameters.AddWithValue("@id", entity.QuarterId);

            connection.Open();
            command.ExecuteNonQuery();
            _notifier.NotifyQuarterChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();
            try
            {
                // Удаляем связанные записи из schedule
                using var deleteScheduleCommand = new NpgsqlCommand(
                    "DELETE FROM schedule WHERE quarter_id = @id", connection, transaction);
                deleteScheduleCommand.Parameters.AddWithValue("@id", id);
                deleteScheduleCommand.ExecuteNonQuery();

                // Удаляем связанные записи из grade
                using var deleteGradeCommand = new NpgsqlCommand(
                    "DELETE FROM grade WHERE quarter_id = @id", connection, transaction);
                deleteGradeCommand.Parameters.AddWithValue("@id", id);
                deleteGradeCommand.ExecuteNonQuery();

                // Удаляем четверть
                using var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM quarter WHERE quarter_id = @id", connection, transaction);
                deleteCommand.Parameters.AddWithValue("@id", id);
                deleteCommand.ExecuteNonQuery();

                transaction.Commit();
                _notifier.NotifyQuarterChanged();
            }
            catch (NpgsqlException ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при удалении: Ограничение внешнего ключа (23503). Проверьте связанные данные в schedule или grade. Подробности: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при удалении: {ex.Message}", ex);
            }
        }
    }
}