using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System;

namespace ApiService
{
    public class Worker : BackgroundService
    {
        public HttpClient? _httpClient;
        public string? certIndexEndpoint;
        public string? baseUrl;
        public string? certShowEndpoint;
        public int repeatTime_inMin;
        private int usersWithCert;
        private readonly ILogger<Worker> _logger;
        private int _Total_Entries;
        public string connectionString;
        private readonly UserUtils _userUtils;
        private readonly CertUtils _certUtils;

        //private static System.Timers.Timer aTimer;

        public Worker(ILogger<Worker> logger)
        {
            _userUtils = new UserUtils(this);
            _certUtils = new CertUtils(this);
            _logger = logger;
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["testDB"].ConnectionString;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);


            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
            connectionString = builder.ConnectionString;
            _httpClient = new HttpClient();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {

                await connection.OpenAsync(stoppingToken);

                List<int> companyIds = GetUseXAPI(connection);

                List<CompanyProcessingInfo> companyProcessingInfos = companyIds.Select(companyId => new CompanyProcessingInfo
                {
                    CompanyId = companyId,
                    NextProcessingTime = DateTimeOffset.UtcNow
                }).ToList();

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Sort the list by next processing time
                    companyProcessingInfos.Sort((a, b) => DateTimeOffset.Compare(a.NextProcessingTime, b.NextProcessingTime));

                    // Get the company with the earliest next processing time
                    CompanyProcessingInfo nextCompany = companyProcessingInfos.First();

                    // Calculate the time to wait until the next processing
                    TimeSpan waitTime = nextCompany.NextProcessingTime - DateTimeOffset.UtcNow;

                    if (waitTime > TimeSpan.Zero)
                    {
                        // Wait until the next processing time
                        await Task.Delay(waitTime, stoppingToken);
                    }
                    
                    // Process the next company
                    await ProcessCompany(connection, nextCompany.CompanyId, stoppingToken);
                    

                    // Update the next processing time for the company
                    nextCompany.NextProcessingTime = DateTimeOffset.UtcNow.AddSeconds(GetRepeatTimeInMinutes(connection, nextCompany.CompanyId));

                }
            }
            _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);

        }





        private async Task ProcessCompany(SqlConnection connection, int companyId, CancellationToken stoppingToken)
        {
            

            int userWithCertificate = 0;
            certIndexEndpoint = null;
            certShowEndpoint = null;
            int userResponePages = 1; 
            int ResponsePerPage = 500; //Number of entries per page for pagination. Maximum 500.


            string userIndexEndpoint = GetConfig<string>(connection, "userIndexEndpoint", companyId);
            certIndexEndpoint = GetConfig<string>(connection, "certIndexEndpoint", companyId);
            certShowEndpoint = GetConfig<string>(connection, "certShowEndpoint", companyId);
            baseUrl = GetConfig<string>(connection, "baseUrl", companyId);
            string token = GetConfig<string>(connection, "Token", companyId);

            _logger.LogInformation($"Processing data for company with the Id: {companyId}");

            _httpClient.DefaultRequestHeaders.Remove("Authorization");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
            while (true)
            {
                string _userIndexEndpoint = userIndexEndpoint.Replace("{userResponePages}", userResponePages.ToString()).Replace("{ResponsePerPage}", ResponsePerPage.ToString());

                HttpResponseMessage userResponse = await _httpClient.GetAsync(baseUrl + _userIndexEndpoint);
                userResponse.EnsureSuccessStatusCode();
                string userResponseBody = await userResponse.Content.ReadAsStringAsync(stoppingToken);

                // Deserialize the JSON response for users
                UserIndex? userIndex = JsonConvert.DeserializeObject<UserIndex>(userResponseBody);


                // Extract the desired fields from user data
                List<User> users = new List<User>();
                //counts users that have certificates
                _userUtils.UpdateUserCount(connection, users.Count, userWithCertificate);

                foreach (User userData in userIndex.Data)
                {
                    User user = new User
                    {
                        id = userData.id,
                        username = userData.username,
                        email = userData.email,
                        full_name = userData.full_name,
                        first_name = userData.first_name,
                        last_name = userData.last_name,
                        personal_number = userData.personal_number,
                        date_of_entry = userData.date_of_entry,
                        date_of_leaving = userData.date_of_leaving,
                        roles = userData.roles,
                        user_information = userData.user_information
                    };

                    users.Add(user);
                }


                foreach (User user in users)
                {
                    // Check if the user exists in the database
                    User existingUser = _userUtils.GetUserById(connection, user.id);

                    if (existingUser != null)
                    {
                        // Compare existing user's data with the new data
                        if (!_userUtils.UserEquals(existingUser, user))
                        {
                            _userUtils.UpdateUser(connection, user, companyId);
                        }
                    }
                    else
                    {
                        _userUtils.InsertUser(connection, user, companyId);
                    }
                }
                


                // Fetch certificate data for each user
                foreach (User user in users)
                {
                    string certEndpoint = certIndexEndpoint.Replace("{userId}", user.id.ToString()).Replace("{ResponsePerPage}", ResponsePerPage.ToString());
                    Guid userOid = _userUtils.GetUserOid(connection, user.id);

                    // Fetch certificate data from the API for the current user
                    HttpResponseMessage certResponse = await _httpClient.GetAsync(baseUrl + certEndpoint);
                    certResponse.EnsureSuccessStatusCode();
                    string certResponseBody = await certResponse.Content.ReadAsStringAsync(stoppingToken);

                    // Deserialize the JSON response for certificates
                    CertIndex? certIndex = JsonConvert.DeserializeObject<CertIndex>(certResponseBody);


                    //counts users that have certificates
                    while (certIndex?.data.Count > userWithCertificate)
                    {
                        userWithCertificate++;
                    }

                    // Extract the desired fields from certificate data
                    List<Cert> certificates = new List<Cert>();

                    foreach (Cert certData in certIndex.data)
                    {
                        Cert certificate = new Cert
                        {
                            id = certData.id,
                            obtained_at = certData.obtained_at,
                            referenceable_type = certData.referenceable_type,
                            referenceable_id = certData.referenceable_id,
                            name = certData.name,
                            referenceable_name = certData.referenceable_name,
                            path = certData.path
                        };

                        certificates.Add(certificate);
                    }
                    foreach (Cert certificate in certificates)
                    {
                        try
                        {
                            byte[] certificateData = await _certUtils.GetCertData(user.id.ToString(), certificate.id.ToString());


                            // Check if the user exists in the database
                            Cert existingCertificate = _certUtils.GetCertById(connection, certificate.id);

                            if (existingCertificate != null)
                            {
                                // Compare existing user's data with the new data
                                if (!_certUtils.CertificateEquals(existingCertificate, certificate))
                                {

                                    _certUtils.UpdateCertificate(connection, user.id, certificate, certificateData, userOid);
                                }
                            }
                            else
                            {
                                _certUtils.InsertCertificate(connection, user.id, certificate, certificateData, userOid);
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "An error occurred while processing: {ErrorMessage}", ex.Message);
                        }


                    }
                }
                if (userResponePages < userIndex.Meta.Total_Pages)
                {
                    userResponePages++;
                }
                else
                {
                    break;
                }
            }
        }
        private int GetRepeatTimeInMinutes(SqlConnection connection, int companyId)
        {
            repeatTime_inMin = GetConfig<int>(connection, "repeatTime_inMin", companyId);

            return repeatTime_inMin;
        }
        public T? GetConfig<T>(SqlConnection connection, string config, int companyId)
        {
            string query = "SELECT " + config + " FROM [dbo].[Config] WHERE company_id = @companyId ";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@companyId", companyId);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (typeof(T) == typeof(string))
                        {
                            string value = reader.GetString(0);
                            return (T)(object)value;
                        }
                        else if (typeof(T) == typeof(int))
                        {
                            int value = reader.GetInt32(0);
                            return (T)(object)value;
                        }
                    }
                }
                return default;
            }
        }
        public List<int> GetUseXAPI(SqlConnection connection)
        {
            string query = "SELECT company_id FROM [dbo].[Config] ";

            List<int> companyIds = new List<int>();


            using (SqlCommand command = new SqlCommand(query, connection))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int company_id = reader.GetInt32(0);
                        companyIds.Add(company_id);
                    }
                }
            }
            return companyIds;
        }
    }
}
