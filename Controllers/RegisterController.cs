using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;

namespace Cafeteria.Controllers
{
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SignUp(string email,  string password, string name, bool supplier)
        {

            try
            {
                using(SqliteConnection connection = new SqliteConnection("Data Source=CoffeDb.sqlite3"))
                {
                    await connection.OpenAsync();

                    //Verificação se já existe.

                    string checkQuery = supplier ? "SELECT COUNT(*) FROM Supplier WHERE email = @Email"
                                                :  "SELECT COUNT(*) FROM Client WHERE email = @Email";

                    using(SqliteCommand checkCommand = new SqliteCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("email", email);
                        int existingCount = (int)await checkCommand.ExecuteScalarAsync();

                        if(existingCount > 0) { Json(new { msg = "Este email já está registrado." } ); }
                    }



                    string query = supplier ? $"INSERT INTO Supplier( email, password, name ) VALUES (@Email, @Password, @Name) "
                                            : $"INSERT INTO Client  ( email, password, name ) VALUES (@Email, @Password, @Name) ";

                    using(SqliteCommand command = new SqliteCommand(query, connection))
                    {
                        //Parametros
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Name", name);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                    

                return Json(new {msg= "Sucesso"});

            }

            catch(Exception ex) { return Json(new { msg = "Erro" }); }
        }
    }
}
