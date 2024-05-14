using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.Security.Claims;

namespace Cafeteria.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            // Verificação se o user estiver logado.
            if (User.Identity.IsAuthenticated)
            {
                return Json(new { msg = "Usuário já logado!" });
            }

            return View();
        }

        public async Task<IActionResult> SignIn(string name, string email, string password)
        {
            // String de Conexão com o banco de dados SQLite.
            using (SqliteConnection Connection = new SqliteConnection("Data Source=CoffeDb.sqlite3"))
            {
                await Connection.OpenAsync();

                // Consulta SQL para autenticar o cliente ou fornecedor
                string query = $"SELECT id, name, email, password, null as role FROM Client WHERE email = '{email}' AND password = '{password}' UNION ALL " +
                               $"SELECT id, name, email, password, 'Supplier' as role FROM Supplier WHERE email = '{email}' AND password = '{password}'";

                using (SqliteCommand Command = new SqliteCommand(query, Connection))
                {
                    using (SqliteDataReader Reader = await Command.ExecuteReaderAsync())
                    {
                        if (await Reader.ReadAsync())
                        {
                            int userId = Reader.GetInt32(0);
                            string userName = Reader.GetString(1);
                            string userEmail = Reader.GetString(2);
                            string userRole = Reader.IsDBNull(4) ? null : Reader.GetString(4);

                            // Criando as reivindicações (claims) para o usuário autenticado
                            List<Claim> direitoAcesso = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                        new Claim(ClaimTypes.Name, userName),
                        new Claim(ClaimTypes.Email, userEmail)
                    };

                            if (userRole != null)
                            {
                                direitoAcesso.Add(new Claim(ClaimTypes.Role, userRole));
                            }

                            var identity = new ClaimsIdentity(direitoAcesso, "identity.User");
                            var userPrincipal = new ClaimsPrincipal(new[] { identity });

                            // Fazendo login do usuário
                            await HttpContext.SignInAsync(userPrincipal,
                                new AuthenticationProperties
                                {
                                    IsPersistent = false,
                                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                                });

                            return Json(new { msg = $"Name:{userName}, Email:{userEmail}, Role: {userRole}" });
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
                // Fazendo logout do usuário
                await HttpContext.SignOutAsync();
            }
            return RedirectToAction("Index", "Login");
        }
    }
}
