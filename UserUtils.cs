using System.Data.SqlClient;
using System.Security.Cryptography;

namespace ApiService
{
    public class UserUtils
    {
        
        private readonly Worker _worker;

        public UserUtils(Worker worker)
        {
            _worker = worker;
        }

        public UserUtils()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public User GetUserById(SqlConnection connection, int userId)
        {
            string query = "SELECT * FROM [dbo].[UserIndex] WHERE xx_id = @userId";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        User user = new User
                        {
                            id = reader.GetInt32(reader.GetOrdinal("xx_id")),
                        };

                        return user;
                    }
                }
            }

            return null;
        }
        public Guid GetUserOid(SqlConnection connection, int userId)
        {
            string query = "SELECT oid FROM [dbo].[UserIndex] WHERE xx_id = @userId";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", userId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetGuid(reader.GetOrdinal("oid"));
                    }
                }
            }

            return default;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public void UpdateUser(SqlConnection connection, User user, int company_id)
        {
            string query = "UPDATE [dbo].[UserIndex] SET ";
            query += " email = @email, full_name = @full_name, first_name = @first_name,last_name = @last_name, date_of_entry = @date_of_entry , company_id = @company_id ";
            query += "WHERE xx_id = @userId";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", user.id);
                command.Parameters.AddWithValue("@company_id", company_id);

                var checkNotNull = new[]
                {

                    nameof(user.email),
                    nameof(user.username),
                    nameof(user.full_name),
                    nameof(user.first_name),
                    nameof(user.last_name),
                    nameof(user.personal_number),
                    nameof(user.date_of_entry),
                    nameof(user.date_of_leaving)
                };
                foreach (var check in checkNotNull)
                {
                    var checkValue = typeof(User).GetProperty(check)?.GetValue(user);
                    if (checkValue != null)
                    {
                        if (checkValue is string stringValue)
                        {
                            command.Parameters.AddWithValue("@" + check, stringValue);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@" + check, checkValue);
                        }
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@" + check, DBNull.Value);
                    }
                }


                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public void InsertUser(SqlConnection connection, User user, int company_id)
        {
            string query = "INSERT INTO [dbo].[UserIndex] ";
            query += "(xx_id, email, full_name, first_name, last_name, date_of_entry, company_id) ";
            query += "VALUES ( @userId, @email,  @full_name, @first_name, @last_name, @date_of_entry, @company_id) ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@userId", user.id);
                command.Parameters.AddWithValue("@company_id", company_id);


                var checkNotNull = new[]
                {

                    nameof(user.email),
                    nameof(user.username),
                    nameof(user.full_name),
                    nameof(user.first_name),
                    nameof(user.last_name),
                    nameof(user.personal_number),
                    nameof(user.date_of_entry),
                    nameof(user.date_of_leaving)
                };
                foreach (var check in checkNotNull)
                {
                    var checkValue = typeof(User).GetProperty(check)?.GetValue(user);
                    if (checkValue != null)
                    {
                        if (checkValue is string stringValue)
                        {
                            // Handle string properties (including email) here
                            command.Parameters.AddWithValue("@" + check, stringValue);
                        }
                        else
                        {
                            // Handle other types of properties (int, DateTime, etc.) here
                            command.Parameters.AddWithValue("@" + check, checkValue);
                        }
                    }
                    else
                    {
                        // Handle NULL values here
                        command.Parameters.AddWithValue("@" + check, DBNull.Value);
                    }
                }

                command.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userCount"></param>
        /// <param name="userWithCertificate"></param>
        /// <returns></returns>
        public void UpdateUserCount(SqlConnection connection, int userCount, int userWithCertificate)
        {
            string query = "UPDATE [dbo].[UserCount] SET ";
            query += "usersCount = @usersCount, usersWithCertificate = @usersWithCertificate ";
            query += "WHERE count_id = count_id";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@usersCount", userCount);
                command.Parameters.AddWithValue("@usersWithCertificate", userWithCertificate);

                command.ExecuteNonQuery();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <returns></returns>
        public bool UserEquals(User user1, User user2)
        {
            return user1.id == user2.id &&
           user1.email == user2.email &&
           user1.username == user2.username &&
           user1.full_name == user2.full_name &&
           user1.first_name == user2.first_name &&
           user1.last_name == user2.last_name &&
           user1.personal_number == user2.personal_number &&
           user1.date_of_entry == user2.date_of_entry &&
           user1.date_of_leaving == user2.date_of_leaving;


        }

    }
}