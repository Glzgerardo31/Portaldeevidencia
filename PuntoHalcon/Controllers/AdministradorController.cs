using Dominio.Entidades;
using Dominio.Enum;
using Infraestructura.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
namespace PuntoHalcon.Controllers;

[Authorize(Roles = "Administrador")]
public class AdministradorController : Controller
{
    private readonly PuntoHalconDbContext _db;
    private readonly IConfiguration _config;
    public AdministradorController(PuntoHalconDbContext db, IConfiguration config) { _db = db; _config = config; }

    public async Task<IActionResult> Index()
    {
        // Cada misión se muestra junto con su insignia.
        ViewBag.Misiones = await _db.Misiones.Include(x => x.Usuario).Include(x => x.Insignias)
            .OrderByDescending(x => x.EsPropuesta).ThenByDescending(x => x.FechaCreacion).ToListAsync();
        // Insignias que no dependen de una misión (logro especial o propuestas antiguas sin misión).
        ViewBag.InsigniasSueltas = await _db.Insignias.Include(x => x.Usuario).Where(x => x.MisionId == null)
            .OrderByDescending(x => x.EsPropuesta).ThenBy(x => x.InsigniaId).ToListAsync();
        // Cuántos estudiantes completaron cada misión (para avisar antes de eliminarla).
        ViewBag.Completadas = await _db.Progresos.Where(p => p.Estado == EstadoProgreso.Completada)
            .GroupBy(p => p.MisionId).Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N);
        // Código de demostración: si una misión lo sigue usando, el panel lo marca para cambiarlo.
        ViewBag.CodigoDefecto = _config["Missions:VerificationCode"] ?? "HALCON2026";
        return View();
    }

    public async Task<IActionResult> Perfil()
    {
        var id = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Usuarios.FindAsync(id);
        ViewBag.NombreCuenta = user?.Nombre ?? "Administrador"; ViewBag.Apodo = user?.Apodo ?? ""; ViewBag.Rol = "Administrador";
        return View("~/Views/Home/Perfil.cshtml");
    }

    // ---------------------------------------------------------------- Misiones (con su insignia y código)

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearMision(string nombre, string descripcion, CategoriaMision categoria, string lugar, DificultadMision dificultad,
        string? codigo, string insigniaNombre, string? insigniaIcono)
    {
        nombre = (nombre ?? "").Trim(); descripcion = (descripcion ?? "").Trim(); lugar = (lugar ?? "").Trim();
        insigniaNombre = (insigniaNombre ?? "").Trim();
        var icono = string.IsNullOrWhiteSpace(insigniaIcono) ? "🏅" : insigniaIcono.Trim();
        codigo = string.IsNullOrWhiteSpace(codigo) ? GenerarCodigo() : codigo.Trim();

        var error = ValidarMision(nombre, descripcion, lugar, codigo) ?? ValidarInsignia(insigniaNombre, icono);
        if (error != null) return Volver(error, true);

        var mision = new Mision
        {
            Nombre = nombre, Descripcion = descripcion, Categoria = categoria, Lugar = lugar, Dificultad = dificultad,
            Codigo = codigo, EsPropuesta = false, FechaCreacion = DateTime.UtcNow
        };
        var insignia = new Insignia
        {
            Mision = mision, Nombre = insigniaNombre, Descripcion = $"Completa la misión «{nombre}».",
            Etiqueta = EtiquetaDe(categoria), Icono = icono, EsPropuesta = false
        };
        _db.Misiones.Add(mision); _db.Insignias.Add(insignia);
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver($"Misión e insignia publicadas. Código de la misión: {codigo}");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarMision(int id, string nombre, string descripcion, string lugar, CategoriaMision categoria, DificultadMision dificultad,
        string? codigo, int? insigniaId, string? insigniaNombre, string? insigniaDescripcion, string? insigniaEtiqueta, string? insigniaIcono)
    {
        var mision = await _db.Misiones.Include(x => x.Insignias).FirstOrDefaultAsync(x => x.MisionId == id);
        if (mision == null) return Volver("La misión ya no existe.", true);

        nombre = (nombre ?? "").Trim(); descripcion = (descripcion ?? "").Trim(); lugar = (lugar ?? "").Trim();
        codigo = (codigo ?? "").Trim();
        // Una misión publicada siempre necesita código; en una propuesta puede quedar vacío hasta aprobarla.
        if (codigo.Length == 0 && !mision.EsPropuesta) return Volver("Una misión publicada necesita un código de verificación.", true);
        var error = ValidarMision(nombre, descripcion, lugar, codigo.Length == 0 ? null : codigo);
        if (error != null) return Volver(error, true);

        var insignia = insigniaId.HasValue ? mision.Insignias.FirstOrDefault(i => i.InsigniaId == insigniaId.Value) : mision.Insignias.FirstOrDefault();
        var iNombre = (insigniaNombre ?? "").Trim();
        if (insignia != null || iNombre.Length > 0)
        {
            var iIcono = string.IsNullOrWhiteSpace(insigniaIcono) ? "🏅" : insigniaIcono.Trim();
            var iDesc = (insigniaDescripcion ?? "").Trim();
            var iEtiqueta = string.IsNullOrWhiteSpace(insigniaEtiqueta) ? EtiquetaDe(categoria) : insigniaEtiqueta.Trim().ToUpperInvariant();
            error = ValidarInsignia(iNombre, iIcono);
            if (error == null && (iDesc.Length < 10 || iDesc.Length > 120)) error = "La descripción de la insignia debe tener entre 10 y 120 caracteres.";
            if (error == null && iEtiqueta.Length > 30) error = "La categoría de la insignia no puede pasar de 30 caracteres.";
            if (error != null) return Volver(error, true);
            if (insignia == null) { insignia = new Insignia { Mision = mision, UsuarioId = mision.UsuarioId }; _db.Insignias.Add(insignia); }
            insignia.Nombre = iNombre; insignia.Descripcion = iDesc; insignia.Etiqueta = iEtiqueta; insignia.Icono = iIcono;
            insignia.EsPropuesta = mision.EsPropuesta; // la insignia siempre sigue el estado de su misión
        }

        mision.Nombre = nombre; mision.Descripcion = descripcion; mision.Lugar = lugar;
        mision.Categoria = categoria; mision.Dificultad = dificultad;
        mision.Codigo = codigo.Length == 0 ? null : codigo;
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver(mision.Codigo == null ? "Cambios guardados." : $"Cambios guardados. Código de la misión: {mision.Codigo}");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarMision(int id)
    {
        var mision = await _db.Misiones.Include(x => x.Insignias).FirstOrDefaultAsync(x => x.MisionId == id);
        if (mision == null) return Volver("La misión ya no existe.", true);
        // Si el administrador no escribió un código, se genera uno y se le muestra.
        if (string.IsNullOrWhiteSpace(mision.Codigo)) mision.Codigo = GenerarCodigo();
        mision.EsPropuesta = false; mision.Rechazada = false; mision.NotaAdmin = null;
        foreach (var i in mision.Insignias) i.EsPropuesta = false;
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver($"Misión e insignia publicadas. Código de la misión: {mision.Codigo}");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarMision(int id)
    {
        var mision = await _db.Misiones.Include(x => x.Insignias).FirstOrDefaultAsync(x => x.MisionId == id);
        if (mision == null) return Volver("La misión ya no existe.", true);
        // Las insignias de la misión se eliminan con ella; si no, quedarían sueltas y se desbloquearían solas.
        _db.Insignias.RemoveRange(mision.Insignias);
        _db.Misiones.Remove(mision);
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver($"Misión «{mision.Nombre}» eliminada junto con su insignia.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RechazarMision(int id, string? nota)
    {
        var mision = await _db.Misiones.FirstOrDefaultAsync(x => x.MisionId == id && x.EsPropuesta);
        if (mision == null) return Volver("La propuesta ya no existe o ya fue publicada.", true);
        nota = (nota ?? "").Trim();
        if (nota.Length > 200) return Volver("La nota no puede pasar de 200 caracteres.", true);
        mision.Rechazada = true; mision.NotaAdmin = nota.Length == 0 ? null : nota;
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver($"Propuesta «{mision.Nombre}» rechazada. El estudiante verá el estado en su pantalla de Explorar.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReconsiderarMision(int id)
    {
        var mision = await _db.Misiones.FirstOrDefaultAsync(x => x.MisionId == id && x.EsPropuesta);
        if (mision == null) return Volver("La propuesta ya no existe.", true);
        mision.Rechazada = false; mision.NotaAdmin = null;
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver($"Propuesta «{mision.Nombre}» devuelta a revisión.");
    }

    // Genera un código nuevo para una misión publicada (por ejemplo si se filtró).
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerarCodigo(int id)
    {
        var mision = await _db.Misiones.FirstOrDefaultAsync(x => x.MisionId == id);
        if (mision == null) return Volver("La misión ya no existe.", true);
        mision.Codigo = GenerarCodigo();
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver($"Nuevo código de «{mision.Nombre}»: {mision.Codigo}");
    }

    // ---------------------------------------------------------------- Insignias sueltas (sin misión)

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditarInsignia(int id, string nombre, string descripcion, string etiqueta, string icono, int? misionId)
    {
        var item = await _db.Insignias.FindAsync(id);
        if (item == null) return Volver("La insignia ya no existe.", true);
        nombre = (nombre ?? "").Trim(); descripcion = (descripcion ?? "").Trim(); etiqueta = (etiqueta ?? "").Trim().ToUpperInvariant(); icono = (icono ?? "").Trim();
        var error = ValidarInsignia(nombre, icono);
        if (error == null && (descripcion.Length < 10 || descripcion.Length > 120)) error = "La descripción de la insignia debe tener entre 10 y 120 caracteres.";
        if (error == null && (etiqueta.Length < 2 || etiqueta.Length > 30)) error = "La categoría debe tener entre 2 y 30 caracteres.";
        if (error == null && misionId.HasValue && !await _db.Misiones.AnyAsync(m => m.MisionId == misionId.Value)) error = "La misión elegida no existe.";
        if (error != null) return Volver(error, true);
        item.Nombre = nombre; item.Descripcion = descripcion; item.Etiqueta = etiqueta; item.Icono = icono; item.MisionId = misionId;
        var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver("Insignia actualizada.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AprobarInsignia(int id)
    {
        var item = await _db.Insignias.FindAsync(id);
        if (item == null) return Volver("La insignia ya no existe.", true);
        if (item.MisionId == null) return Volver("Asocia la insignia a una misión (en Modificar) antes de publicarla.", true);
        item.EsPropuesta = false; var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true);
        return Volver("Insignia publicada.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarInsignia(int id)
    {
        var item = await _db.Insignias.FindAsync(id);
        if (item != null) { _db.Insignias.Remove(item); var fallo = await GuardarAsync(); if (fallo != null) return Volver(fallo, true); }
        return Volver("Insignia eliminada.");
    }

    // ---------------------------------------------------------------- Auxiliares

    // Guarda los cambios y, si la base de datos rechaza la operación, devuelve un mensaje en vez de un error 500.
    private async Task<string?> GuardarAsync()
    {
        try { await _db.SaveChangesAsync(); return null; }
        catch (DbUpdateException) { return "No se pudo guardar en la base de datos. Revisa los datos e inténtalo de nuevo."; }
    }

    private IActionResult Volver(string mensaje, bool esError = false)
    {
        TempData[esError ? "Error" : "Mensaje"] = mensaje;
        return RedirectToAction(nameof(Index));
    }

    private static string? ValidarMision(string nombre, string descripcion, string lugar, string? codigo)
    {
        if (nombre.Length < 3 || nombre.Length > 40) return "El nombre de la misión debe tener entre 3 y 40 caracteres.";
        if (descripcion.Length < 10 || descripcion.Length > 120) return "La descripción de la misión debe tener entre 10 y 120 caracteres.";
        if (lugar.Length < 3 || lugar.Length > 40) return "El lugar debe tener entre 3 y 40 caracteres.";
        if (codigo != null && !Regex.IsMatch(codigo, "^[A-Za-z0-9_-]{4,40}$")) return "El código debe tener de 4 a 40 caracteres: letras, números, guion o guion bajo, sin espacios.";
        return null;
    }

    private static string? ValidarInsignia(string nombre, string icono)
    {
        if (nombre.Length < 3 || nombre.Length > 60) return "El nombre de la insignia debe tener entre 3 y 60 caracteres.";
        if (icono.Length < 1 || icono.Length > 10) return "El icono de la insignia no es válido.";
        return null;
    }

    private static string EtiquetaDe(CategoriaMision categoria) =>
        categoria == CategoriaMision.Tecnologia ? "TECNOLOGÍA" : categoria.ToString().ToUpperInvariant();

    // Código legible, por ejemplo HALCON-4821.
    private static string GenerarCodigo() => "HALCON-" + RandomNumberGenerator.GetInt32(1000, 10000);
}
