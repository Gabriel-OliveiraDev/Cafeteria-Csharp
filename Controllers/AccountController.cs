using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cafeteria.Controllers
{
    public class AccountController : Controller
    {

        public IActionResult Index()
        {
            // Verificando se há Usuário Autenticado. Se sim Redirecionando para a Página inicial, se não, para o Login
            return User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : (IActionResult)RedirectToAction("SignIn", "Account");
        }


        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string name, string checkbox) 
        {
            // Validação se é fornecedor
            bool isSupplier = checkbox != null;

            // Variável da entidade(fornecedor/cliente)
            string entityType = isSupplier? "Supplier" : "Client";

            // Conexão
            using SqliteConnection connection = new("Data Source=CoffeDb.sqlite3");
            await connection.OpenAsync();

            // Método que faz validação de Email, se encontrar algum email retorna um JSON
            if (await CheckEmail(entityType, email, connection)) { return Json(new { msg = "Email ja registrado." }); }
            else
            {
                // Fazendo Inserção
                Insert(connection, email, password, name, entityType);

                connection.Close();
                // Retorno em JSON
                string json = isSupplier ? "Registro de Fornecedor realizado" : "Registro de Cliente realizado";
                return Json(new { json });
            }
        }

        private async Task<Boolean> CheckEmail(string entity, string email, SqliteConnection connection)
        {
            // Query
            string checkQuery = $"SELECT COUNT(*) FROM {entity} WHERE email = @Email";
            using (SqliteCommand checkCommand = new(checkQuery, connection))
            {
                checkCommand.Parameters.AddWithValue("@Email", email);
                long count = (long)await checkCommand.ExecuteScalarAsync();

                if (count > 0) { return true; }
            }
            return false;
        }
        
        private static void Insert(SqliteConnection connection, string email, string password, string name, string entity)
        {
            // Query de inserção 
            string insertQuery = $"INSERT INTO {entity} (email, password, name) VALUES (@Email, @Password, @Name)";
            
            SqliteCommand insertCommand = new(insertQuery, connection);
            insertCommand.Parameters.AddWithValue("@Email", email);
            insertCommand.Parameters.AddWithValue("@Password", password);
            insertCommand.Parameters.AddWithValue("@Name", name);

            insertCommand.ExecuteNonQueryAsync();
        }

        
        public async Task<IActionResult> SignIn(string email, string password)
        {
            // Validação de Autenticação
            if (User.Identity.IsAuthenticated) { return RedirectToAction("Index", "Home"); }
         
            // Conexão
            using SqliteConnection connection = new("Data Source=CoffeDb.sqlite3");
            await connection.OpenAsync();

            // Query e execução
            string query =  $"SELECT id, name, email, password, null as role FROM Client WHERE email = '{email}' AND password = '{password}' UNION ALL " +
                            $"SELECT id, name, email, password, 'Supplier' as role FROM Supplier WHERE email = '{email}' AND password = '{password}'";
            using SqliteCommand command = new(query, connection);

            // Leitura dos dados
            using SqliteDataReader reader = await command.ExecuteReaderAsync();
            // Se algum usuário for encontrado
            if (await reader.ReadAsync())
            { 
                //Leitura
                int userId = reader.GetInt32(0);
                string userName = reader.GetString(1);
                string userEmail = reader.GetString(2);
                string? userRole = reader.IsDBNull(4) ? null : reader.GetString(4);

                // Define as Claims do usuário
                var claims = new List<Claim>
                        {
                            new (ClaimTypes.NameIdentifier, userId.ToString()),
                            new (ClaimTypes.Name, userName),
                            new (ClaimTypes.Email, userEmail)
                        };

                // Adicionando um papel/Role se puder
                if (userRole != null) { claims.Add(new Claim(ClaimTypes.Role, userRole)); }

                // Cria a identidade para o usuário
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var userPrincipal = new ClaimsPrincipal([identity]);

                // Faz o Login do usuário
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal,
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = DateTime.UtcNow.AddHours(1)
                    });

                // Define o tipo de usuário com base ao papel/Role
                string userType = (userRole == null) ? "Client" : "Supplier";
                connection.Close();
                // Retornando um JSON
                // return Json(new { msg = $"Name:{userName}, Email:{userEmail}, Type: {userType}" } );
                // Redirecionando para página inicial
                
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
