using Cafeteria.Contexts;
using Cafeteria.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cafeteria.Controllers
{
    public class ProductController : Controller
    {
        private readonly CafeteriaContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductController(CafeteriaContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        // GET: ProductController
        public ActionResult Index()
        {
            // Id do suppplier
            if (!string.IsNullOrEmpty(GenIdSupplier()))
            {
                ViewBag.supplierId = Convert.ToInt32(GenIdSupplier());
            }

            var products = _context.Product.ToList();
            return View(products);
        }

        // GET: ProductController/Details/5
        public ActionResult Details(int id)
        {
            var products = _context.Product.Find(id);
            return View(products);
        }

        // GET: ProductController/Create
        [Authorize(Roles = "Supplier")] // Limitando para apenas Fornecedores
        public ActionResult Create()
        {
            return View();
        }

        // POST: ProductController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supplier")]
        public ActionResult Create(Product product)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Puxando o ID do fornecedor do usuário autenticado
                    var supplierId = GenIdSupplier();
                    if (!string.IsNullOrEmpty(supplierId))
                    {
                        // Adicionar o ID do fornecedor ao produto antes de salvar
                        product.SupplierId = Convert.ToInt32(supplierId);
                        _context.Product.Add(product);
                        _context.SaveChanges();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        // Tratar caso o ID do fornecedor não seja encontrado
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }
            }
            catch
            {
                // Log the error or handle it appropriately
            }
            return View(product);
        }


        // GET: ProductController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // Método que retorna o Id do Supplier
        private string? GenIdSupplier() => _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
