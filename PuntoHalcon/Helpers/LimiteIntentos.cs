using System.Collections.Concurrent;

namespace PuntoHalcon.Helpers;

// Límite sencillo de intentos fallidos (en memoria): 5 fallos en 10 minutos bloquean esa clave temporalmente.
// Se usa para el login (por correo + IP) y para el código de misión (por estudiante).
public class LimiteIntentos
{
    private const int MaximoFallos = 5;
    private static readonly TimeSpan Ventana = TimeSpan.FromMinutes(10);
    private readonly ConcurrentDictionary<string, (int Fallos, DateTime Desde)> _intentos = new();

    public bool Bloqueado(string clave, out int minutosEspera)
    {
        minutosEspera = 0;
        if (!_intentos.TryGetValue(clave, out var entrada)) return false;
        if (DateTime.UtcNow - entrada.Desde > Ventana) { _intentos.TryRemove(clave, out _); return false; }
        if (entrada.Fallos < MaximoFallos) return false;
        minutosEspera = Math.Max(1, (int)Math.Ceiling((entrada.Desde + Ventana - DateTime.UtcNow).TotalMinutes));
        return true;
    }

    public void RegistrarFallo(string clave) =>
        _intentos.AddOrUpdate(clave,
            _ => (1, DateTime.UtcNow),
            (_, actual) => DateTime.UtcNow - actual.Desde > Ventana ? (1, DateTime.UtcNow) : (actual.Fallos + 1, actual.Desde));

    public void Reiniciar(string clave) => _intentos.TryRemove(clave, out _);
}
