using System.ComponentModel.Design;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ApiService
{
    public class CertUtils
    {
        private readonly Worker _worker;

        public CertUtils(Worker worker)
        {
            _worker = worker;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="certId"></param>
        /// <returns></returns>
        public Cert GetCertById(SqlConnection connection, int certId)
        {
            string query = "SELECT * FROM [dbo].[CertificatesIndex] WHERE xx_id = @xx_id";
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@xx_id", certId);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        Cert cert = new Cert
                        {
                            id = reader.GetInt32(reader.GetOrdinal("xx_id")),

                        };

                        return cert;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="certId"></param>
        /// <returns></returns>
        public async Task<byte[]> GetCertData(string userId, string certId/*, string full_name, string certName*/)
        {
            using (SqlConnection connection = new SqlConnection(_worker.connectionString))
            {
                await connection.OpenAsync();
                List<int> companyIds = new List<int>(_worker.GetUseXAPI(connection));



                string _certShowEndpoint = _worker.certShowEndpoint.Replace("{userId}", userId).Replace("{certId}", certId);
                HttpResponseMessage certShowResponse = await _worker._httpClient.GetAsync(_worker.baseUrl + _certShowEndpoint);
                certShowResponse.EnsureSuccessStatusCode();


                byte[] certificateData = await certShowResponse.Content.ReadAsByteArrayAsync();


                ////convert byte array to stream and save certificate as pdf in specific path
                
                //string Path = "C:\\xxx\\xxxx\\xxx\\xxxx\\{fileName}.pdf";//path on windows 
                //string _fileName = full_name + ", " + certName;
                //string PathWithFileName = Path.Replace("{fileName}", _fileName);

                //using (MemoryStream stream = new MemoryStream(certificateData))
                //{
                //    using (FileStream fileStream = new FileStream(PathWithFileName, FileMode.Create))
                //    {
                //        stream.CopyTo(fileStream);
                //    }
                //}
                return certificateData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="userId"></param>
        /// <param name="certificate"></param>
        /// <param name="certificateData"></param>
        /// <returns></returns>
        public void InsertCertificate(SqlConnection connection, int userId, Cert certificate, byte[] certificateData, Guid userOid)
        {
            string query = "INSERT INTO [dbo].[CertificatesIndex] "; 
            query += "(xx_id, obtained_at, referenceable_type, referenceable_id, name, referenceable_name , path, user_id, certShow, user_oid) ";
            query += "VALUES (@xx_id, @obtained_at, @referenceable_type, @referenceable_id, @name, @referenceable_name , @path, @user_id, @certShow, @user_oid) ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@xx_id", certificate.id);
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@user_oid", userOid);

                command.Parameters.AddWithValue("@certShow", certificateData);

                var checkNotNull = new[]
                {

                    nameof(certificate.obtained_at),
                    nameof(certificate.referenceable_type),
                    nameof(certificate.referenceable_id),
                    nameof(certificate.name),
                    nameof(certificate.referenceable_name),
                    nameof(certificate.path)
                };
                foreach (var check in checkNotNull)
                {
                    var checkValue = typeof(Cert).GetProperty(check)?.GetValue(certificate);
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
        /// <param name="userId"></param>
        /// <param name="certificate"></param>
        /// <param name="certificateData"></param>
        /// <returns></returns>
        public void UpdateCertificate(SqlConnection connection, int userId, Cert certificate, byte[] certificateData, Guid userOid)
        {
            string query = "UPDATE [dbo].[CertificatesIndex] SET ";
            query += "obtained_at = @obtained_at, referenceable_type = @referenceable_type, referenceable_id = @referenceable_id, name = @name, referenceable_name = @referenceable_name ,path = @path, user_id = @user_id, certShow = @certShow , user_oid = @user_oid ";
            query += "WHERE xx_id = @xx_id";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@xx_id", certificate.id);
                command.Parameters.AddWithValue("@user_id", userId);
                command.Parameters.AddWithValue("@certShow", certificateData);
                command.Parameters.AddWithValue("@user_oid", userOid);


                var checkNotNull = new[]
                {

                    nameof(certificate.obtained_at),
                    nameof(certificate.referenceable_type),
                    nameof(certificate.referenceable_id),
                    nameof(certificate.name),
                    nameof(certificate.referenceable_name),
                    nameof(certificate.path)
                };
                foreach (var check in checkNotNull)
                {
                    var checkValue = typeof(Cert).GetProperty(check)?.GetValue(certificate);
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
        /// <param name="cert1"></param>
        /// <param name="cert2"></param>
        /// <returns></returns>
        public bool CertificateEquals(Cert cert1, Cert cert2)
        {
            return cert1.id == cert2.id &&
                cert1.obtained_at == cert2.obtained_at &&
                cert1.referenceable_type == cert2.referenceable_type &&
                cert1.referenceable_id == cert2.referenceable_id &&
                cert1.name == cert2.name &&
                cert1.referenceable_name == cert2.referenceable_name &&
                cert1.path == cert2.path;


        }

    }
}