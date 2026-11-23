using Infraestructura.Data; using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using System.Security.Claims;
namespace PuntoHalcon.Controllers;
[Authorize]
public class HomeController : Controller
{
 private readonly PuntoHalconDbContext _db; public HomeController(PuntoHalconDbContext db){_db=db;}
 private int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 public async Task<IActionResult> Index(){ var id=UsuarioId; ViewBag.Total=await _db.Evidencias.CountAsync(x=>x.UsuarioId==id); ViewBag.Academica=await _db.Evidencias.CountAsync(x=>x.UsuarioId==id&&x.Categoria=="Academica"); ViewBag.Extra=await _db.Evidencias.CountAsync(x=>x.UsuarioId==id&&x.Categoria=="Extracurricular"); ViewBag.Proyectos=await _db.Evidencias.CountAsync(x=>x.UsuarioId==id&&x.Categoria=="Proyecto"); ViewBag.Recientes=await _db.Evidencias.Where(x=>x.UsuarioId==id).OrderByDescending(x=>x.FechaRegistro).Take(4).ToListAsync(); return View(); }
 public async Task<IActionResult> Perfil(){var u=await _db.Usuarios.FindAsync(UsuarioId);return View(u);}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> GuardarPerfil(string nombre,string telefono,string correo,string ine,string curp,IFormFile? actaNacimiento){var u=await _db.Usuarios.FindAsync(UsuarioId);if(u==null)return NotFound();u.Nombre=(nombre??"").Trim();u.Telefono=(telefono??"").Trim();u.Correo=(correo??"").Trim().ToLower();u.INE=(ine??"").Trim();u.CURP=(curp??"").Trim();if(actaNacimiento!=null&&actaNacimiento.Length>0){var ruta=await GuardarArchivo(actaNacimiento);u.ActaNacimientoRuta=ruta;}await _db.SaveChangesAsync();TempData["Ok"]="Tu perfil se actualizó correctamente.";return RedirectToAction("Perfil");}
 public async Task<IActionResult> FormacionAcademica(){ViewBag.Items=await _db.Evidencias.Where(x=>x.UsuarioId==UsuarioId&&x.Categoria=="Academica").OrderByDescending(x=>x.FechaRegistro).ToListAsync();return View();}
 public async Task<IActionResult> FormacionExtra(){ViewBag.Items=await _db.Evidencias.Where(x=>x.UsuarioId==UsuarioId&&x.Categoria=="Extracurricular").OrderByDescending(x=>x.FechaRegistro).ToListAsync();return View();}
 public async Task<IActionResult> Proyectos(){ViewBag.Items=await _db.Evidencias.Where(x=>x.UsuarioId==UsuarioId&&x.Categoria=="Proyecto").OrderByDescending(x=>x.FechaRegistro).ToListAsync();return View();}
 public async Task<IActionResult> Evidencias(){ViewBag.Items=await _db.Evidencias.Where(x=>x.UsuarioId==UsuarioId).OrderByDescending(x=>x.FechaRegistro).ToListAsync();return View();}
 public IActionResult Privacy()=>View();
 private async Task<string> GuardarArchivo(IFormFile archivo){var carpeta=Path.Combine(Directory.GetCurrentDirectory(),"wwwroot","uploads");Directory.CreateDirectory(carpeta);var nombre=Guid.NewGuid()+Path.GetExtension(archivo.FileName);var ruta=Path.Combine(carpeta,nombre);using var stream=System.IO.File.Create(ruta);await archivo.CopyToAsync(stream);return "/uploads/"+nombre;}
}
