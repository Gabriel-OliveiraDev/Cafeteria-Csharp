using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cafeteria.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return Json(new { msg = "usuario encontrado!" });
            }

            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string name, bool isSupplier)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=CoffeDb.sqlite3"))
            {
                await connection.OpenAsync();

                string checkQuery = isSupplier ?
                    $"SELECT COUNT(*) FROM Supplier WHERE email = @Email" :
                    $"SELECT COUNT(*) FROM Client WHERE email = @Email";

                using (SqliteCommand checkCommand = new SqliteCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@Email", email);
                    var count = (long)await checkCommand.ExecuteScalarAsync();

                    if (count > 0)
                    {
                        return Json(new { msg = "Email já registrado." });
                    }
                }

                string insertQuery = isSupplier ?
                    $"INSERT INTO Supplier (email, password, name) VALUES (@Email, @Password, @Name)" :
                    $"INSERT INTO Client (email, password, name) VALUES (@Email, @Password, @Name)";

                using (SqliteCommand insertCommand = new SqliteCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Email", email);
                    insertCommand.Parameters.AddWithValue("@Password", password);
                    insertCommand.Parameters.AddWithValue("@Name", name);

                    await insertCommand.ExecuteNonQueryAsync();
                }

                return Json(new { msg = "Registro realizado com sucesso." });
            }
        }

        public async Task<IActionResult> SignIn(string email, string password)
        {
            using (SqliteConnection connection = new SqliteConnection("Data Source=CoffeDb.sqlite3"))
            {
                await connection.OpenAsync();

                string query = $"SELECT id, name, email, password, null as role FROM Client WHERE email = '{email}' AND password = '{password}' UNION ALL " +
                               $"SELECT id, name, email, password, 'Supplier' as role FROM Supplier WHERE email = '{email}' AND password = '{password}'";

                using (SqliteCommand command = new SqliteCommand(query, connection))
                {
                    using (SqliteDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            int userId = reader.GetInt32(0);
                            string userName = reader.GetString(1);
                            string userEmail = reader.GetString(2);
                            string userRole = reader.IsDBNull(4) ? null : reader.GetString(4);

                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                                new Claim(ClaimTypes.Name, userName),
                                new Claim(ClaimTypes.Email, userEmail)
                            };

                            if (userRole != null)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, userRole));
                            }

                            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                            var userPrincipal = new ClaimsPrincipal(new[] { identity });

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = false,
                                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                                });

                            string userType = (userRole == null) ? "Client" : "Supplier";

                            return Json(new { msg = $"Name:{userName}, Email:{userEmail}, Type: {userType}" });
                        }
                    }
                }
            }

            return Json(new { msg = "Erro, verifique suas credenciais." });
        }

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Account");
        }
    }
}
