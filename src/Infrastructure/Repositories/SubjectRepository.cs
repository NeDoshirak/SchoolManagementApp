using Application.Interfaces;
using Npgsql;
using SchoolDB.Application.Interfaces;
using SchoolDB.Domain.Entities;
using System.Collections.Generic;

namespace Infrastructure.Repositories
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly string _connectionString;
        private readonly IDataChangeNotifier _notifier;

        public SubjectRepository(string connectionString, IDataChangeNotifier notifier)
        {
            _connectionString = connectionString;
            _notifier = notifier;
        }

        public void Create(Subject entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                "INSERT INTO subject (subject_name) VALUES (@subject_name) RETURNING subject_id", connection);

            command.Parameters.AddWithValue("@subject_name", entity.SubjectName);

            connection.Open();
            entity.SubjectId = (int)command.ExecuteScalar();
            _notifier.NotifySubjectChanged();
        }

        public Subject GetById(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT s.subject_id, s.subject_name,
                         ts.subject_id AS ts_subject_id, ts.teacher_id,
                         t.teacher_id AS t_teacher_id, t.full_name, t.is_active, t.photo_path, t.cabinet_number, t.class_id
                  FROM subject s
                  LEFT JOIN teacher_subject ts ON s.subject_id = ts.subject_id
                  LEFT JOIN teacher t ON ts.teacher_id = t.teacher_id
                  WHERE s.subject_id = @id", connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            var subject = new Subject { TeacherSubjects = new List<TeacherSubject>() };
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (subject.SubjectId == 0)
                {
                    subject.SubjectId = reader.GetInt32(0);
                    subject.SubjectName = reader.GetString(1);
                }
                if (!reader.IsDBNull(2)) // ts_subject_id
                {
                    var teacherSubject = new TeacherSubject
                    {
                        SubjectId = reader.GetInt32(2),
                        TeacherId = reader.GetInt32(3),
                        Teacher = new Teacher
                        {
                            TeacherId = reader.GetInt32(4),
                            FullName = reader.GetString(5),
                            IsActive = reader.GetBoolean(6),
                            PhotoPath = reader.IsDBNull(7) ? null : reader.GetString(7),
                            CabinetNumber = reader.IsDBNull(8) ? null : (int?)reader.GetInt32(8),
                            ClassId = reader.IsDBNull(9) ? null : (int?)reader.GetInt32(9)
                        }
                    };
                    subject.TeacherSubjects.Add(teacherSubject);
                }
            }
            return subject.SubjectId == 0 ? null : subject;
        }

        public IEnumerable<Subject> GetAll()
        {
            var subjects = new Dictionary<int, Subject>();
            using var connection = new NpgsqlConnection(_connectionString);
            using var command = new NpgsqlCommand(
                @"SELECT s.subject_id, s.subject_name,
                         ts.subject_id AS ts_subject_id, ts.teacher_id,
                         t.teacher_id AS t_teacher_id, t.full_name, t.is_active, t.photo_path, t.cabinet_number, t.class_id
                  FROM subject s
                  LEFT JOIN teacher_subject ts ON s.subject_id = ts.subject_id
                  LEFT JOIN teacher t ON ts.teacher_id = t.teacher_id", connection);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int subjectId = reader.GetInt32(0);
                if (!subjects.ContainsKey(subjectId))
                {
                    subjects[subjectId] = new Subject
                    {
                        SubjectId = subjectId,
                        SubjectName = reader.GetString(1),
                        TeacherSubjects = new List<TeacherSubject>()
                    };
                }
                if (!reader.IsDBNull(2)) // ts_subject_id
                {
                    var teacherSubject = new TeacherSubject
                    {
                        SubjectId = reader.GetInt32(2),
                        TeacherId = reader.GetInt32(3),
                        Teacher = new Teacher
                        {
                            TeacherId = reader.GetInt32(4),
                            FullName = reader.GetString(5),
                            IsActive = reader.GetBoolean(6),
                            PhotoPath = reader.IsDBNull(7) ? null : reader.GetString(7),
                            CabinetNumber = reader.IsDBNull(8) ? null : (int?)reader.GetInt32(8),
                            ClassId = reader.IsDBNull(9) ? null : (int?)reader.GetInt32(9)
                        }
                    };
                    subjects[subjectId].TeacherSubjects.Add(teacherSubject);
                }
            }
            return subjects.Values;
        }

        public void Update(Subject entity)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Обновляем предмет
            using var updateCommand = new NpgsqlCommand(
                "UPDATE subject SET subject_name = @subject_name WHERE subject_id = @id", connection);
            updateCommand.Parameters.AddWithValue("@subject_name", entity.SubjectName);
            updateCommand.Parameters.AddWithValue("@id", entity.SubjectId);
            updateCommand.ExecuteNonQuery();

            // Удаляем старые связи
            using var deleteCommand = new NpgsqlCommand(
                "DELETE FROM teacher_subject WHERE subject_id = @id", connection);
            deleteCommand.Parameters.AddWithValue("@id", entity.SubjectId);
            deleteCommand.ExecuteNonQuery();

            // Добавляем новые связи
            if (entity.TeacherSubjects != null)
            {
                using var insertCommand = new NpgsqlCommand(
                    "INSERT INTO teacher_subject (subject_id, teacher_id) VALUES (@subject_id, @teacher_id)", connection);
                foreach (var teacherSubject in entity.TeacherSubjects)
                {
                    insertCommand.Parameters.Clear();
                    insertCommand.Parameters.AddWithValue("@subject_id", entity.SubjectId);
                    insertCommand.Parameters.AddWithValue("@teacher_id", teacherSubject.TeacherId);
                    insertCommand.ExecuteNonQuery();
                }
            }

            _notifier.NotifySubjectChanged();
        }

        public void Delete(int id)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            // Начинаем транзакцию
            using var transaction = connection.BeginTransaction();
            try
            {
                // Удаляем связи из teacher_subject
                using var deleteLinkCommand = new NpgsqlCommand(
                    "DELETE FROM teacher_subject WHERE subject_id = @id", connection, transaction);
                deleteLinkCommand.Parameters.AddWithValue("@id", id);
                deleteLinkCommand.ExecuteNonQuery();

                // Удаляем связи из schedule
                using var deleteScheduleCommand = new NpgsqlCommand(
                    "DELETE FROM schedule WHERE subject_id = @id", connection, transaction);
                deleteScheduleCommand.Parameters.AddWithValue("@id", id);
                deleteScheduleCommand.ExecuteNonQuery();

                // Удаляем предмет
                using var deleteCommand = new NpgsqlCommand(
                    "DELETE FROM subject WHERE subject_id = @id", connection, transaction);
                deleteCommand.Parameters.AddWithValue("@id", id);
                deleteCommand.ExecuteNonQuery();

                transaction.Commit();
                _notifier.NotifySubjectChanged();
            }
            catch (NpgsqlException ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при удалении: Ограничение внешнего ключа (23503). Проверьте связанные данные в teacher_subject или schedule. Подробности: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception($"Ошибка при удалении: {ex.Message}", ex);
            }
        }
    }
}