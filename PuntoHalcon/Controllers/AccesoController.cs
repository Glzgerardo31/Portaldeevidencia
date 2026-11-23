using Dominio.Entidades;
using Infraestructura.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace PuntoHalcon.Controllers;
public class AccesoController : Controller
{
 private readonly PuntoHalconDbContext _db; private readonly PasswordHasher<Usuario> _hasher;
 public AccesoController(PuntoHalconDbContext db, PasswordHasher<Usuario> hasher) { _db=db; _hasher=hasher; }
 [HttpGet] public IActionResult Login() => User.Identity?.IsAuthenticated == true ? RedirectToAction("Index","Home") : View();
 [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Login(string correo,string password)
 { correo=(correo??"").Trim().ToLower(); var u=await _db.Usuarios.FirstOrDefaultAsync(x=>x.Correo==correo); if(u==null || _hasher.VerifyHashedPassword(u,u.PasswordHash,password??"")==PasswordVerificationResult.Failed) { ViewBag.Error="Correo o contraseña incorrectos."; return View(); } await Iniciar(u); return RedirectToAction("Index","Home"); }
 [HttpGet] public IActionResult Registro() => View();
 [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Registro(string nombre,string correo,string password,string confirmarPassword)
 { nombre=(nombre??"").Trim(); correo=(correo??"").Trim().ToLower(); if(nombre.Length<2 || !correo.Contains('@') || (password??"").Length<8 || password!=confirmarPassword) { ViewBag.Error="Revisa tus datos. La contraseña debe tener al menos 8 caracteres y coincidir."; return View(); } if(await _db.Usuarios.AnyAsync(x=>x.Correo==correo)) { ViewBag.Error="Este correo ya está registrado."; return View(); } var u=new Usuario { Nombre=nombre,Correo=correo,Rol="Estudiante" }; u.PasswordHash=_hasher.HashPassword(u,password); _db.Usuarios.Add(u); await _db.SaveChangesAsync(); await Iniciar(u); return RedirectToAction("Index","Home"); }
 [HttpPost, Microsoft.AspNetCore.Authorization.Authorize] public async Task<IActionResult> Salir() { await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); return RedirectToAction("Login"); }
 private async Task Iniciar(Usuario u) { var claims=new List<Claim>{new(ClaimTypes.NameIdentifier,u.UsuarioId.ToString()),new(ClaimTypes.Name,u.Nombre),new(ClaimTypes.Email,u.Correo),new(ClaimTypes.Role,u.Rol)}; await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme))); }
}
