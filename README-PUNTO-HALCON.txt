PUNTO HALCON - VERSION ACTUALIZADA

CAMBIOS INCLUIDOS
- Interfaz clara en blanco, verde y grises.
- Acceso por cuenta y navegación separada para Estudiante y Administrador.
- El nombre original de la cuenta y el correo no se editan desde el perfil; el apodo sí se puede modificar y guardar para ambos roles.
- El progreso de misiones se registra en Progresos por UsuarioId y MisionId.
- Las misiones publicadas se consultan desde SQL Server.
- Cada estudiante ve automáticamente en "Misiones pendientes" las misiones publicadas que aún no ha completado.
- El código de verificación se captura en un formulario dentro de la tarjeta de misión o del apartado de pendientes; ya no aparece una ventana prompt.
- Las propuestas de misiones e insignias se guardan en SQL Server con el UsuarioId del estudiante que las envió.
- Después de enviar una propuesta, el estudiante recibe una confirmación de que fue enviada y está en revisión.
- El panel de administración muestra el autor (apodo o nombre y correo) de las propuestas pendientes, y permite aceptarlas, modificarlas o eliminarlas.
- El ranking consulta la base de datos y solo muestra estudiantes con al menos una misión completada o una insignia obtenida. No utiliza nombres ficticios ni datos de localStorage.
- Las rutas de escritura directa de las API de misiones e insignias están restringidas al rol Administrador; los estudiantes solo consultan el contenido publicado y envían propuestas.

PARA EJECUTAR
1. Haz una copia de seguridad de la base de datos PuntoHalconBd.
2. Descomprime el ZIP.
3. Abre PuntoHalcon.slnx en Visual Studio (o PuntoHalcon.csproj).
4. Verifica la cadena de conexión SQL Server en PuntoHalcon/appsettings.json.
5. Ejecuta el proyecto con HTTPS.

CAMBIOS DE BASE DE DATOS
Al iniciar, Program.cs agrega las columnas Apodo en Usuarios (si falta), UsuarioId en Progresos (si falta), y UsuarioId nullable en Misiones e Insignias (si faltan). Las nuevas propuestas quedan relacionadas con el estudiante que las envió. Las propuestas antiguas sin UsuarioId se mostrarán como enviadas por "Estudiante" porque no se puede deducir con certeza quién las creó.

El progreso antiguo que tenga UsuarioId=0 no se asigna automáticamente a ninguna cuenta para evitar atribuir actividad a un usuario incorrecto.

CODIGO DE MISIONES
El código de demostración sigue configurado en appsettings.json, dentro de Missions:VerificationCode. Valor inicial: HALCON2026. Cámbialo antes de utilizar la aplicación fuera de pruebas.

VERIFICACION
El archivo wwwroot/js/site.js pasó la comprobación sintáctica de Node.js (node --check). No se pudo ejecutar una compilación .NET en el entorno de edición porque no tiene instalado el SDK de .NET. Es necesario compilar y probar la conexión a SQL Server desde Visual Studio.


=====================================================================
ACTUALIZACION: MISION + INSIGNIA, CODIGOS POR MISION Y PROGRESO
=====================================================================
- Proponer una misión ahora incluye su insignia (nombre e icono) en el mismo formulario. Se guardan juntas y el administrador las revisa juntas.
- Panel de administración: cada misión se muestra con su insignia y su código. "Modificar" permite cambiar misión, insignia y código en cualquier momento. Aceptar publica misión e insignia a la vez. Eliminar una misión elimina también su insignia.
- El administrador puede crear misiones directamente (con insignia y código) desde "Nueva misión con su insignia".
- Código por misión: nueva columna Misiones.Codigo. Solo se muestra en el panel del administrador; las APIs de estudiantes no lo envían. Las misiones ya publicadas reciben al iniciar el código de Missions:VerificationCode (HALCON2026) y desde ahí se puede cambiar. Si se acepta una propuesta sin código, se genera uno (HALCON-####) y se muestra al administrador. La comparación del código ya no distingue mayúsculas/minúsculas.
- Progreso de insignias: el servidor calcula qué insignias están desbloqueadas (Helpers/Logros.cs) y esa misma regla se usa en Insignias, Perfil y Ranking. Al publicar una misión con insignia, el total sube y el porcentaje baja (ej. 4/4 = 100% -> 4/5 = 80%).
- "Conquistador del campus" (insignia sin misión) se obtiene al completar 3 misiones (constante en Helpers/Logros.cs).
- Una insignia sin misión ya no se desbloquea sola: para publicarla hay que asociarla a una misión.

OTRAS CORRECCIONES
- Los nombres de misión de más de 40 caracteres provocaban error 500 al guardar (el formulario permitía 60). Ahora se valida en formulario y servidor.
- El filtro "Tecnología" no mostraba las misiones de esa categoría (la API devolvía "Tecnologia"); también se corrigieron los textos de dificultad.
- Las llamadas a /api sin sesión devuelven 401/403 en vez de redirigir a la página de login.
- Se quitaron las tarjetas fijas de Explorar e Insignias (parpadeaban antes de cargar los datos reales).
- Interfaz más sobria: se quitaron tarjetas flotantes, chispas, banners repetidos y textos técnicos.

PARA PROBAR (no se pudo compilar aquí: no hay SDK de .NET)
1. Respaldar la base de datos y abrir PuntoHalcon.slnx en Visual Studio. Compilar.
2. Al iniciar se agrega la columna Codigo automáticamente.
3. Entrar como estudiante, proponer una misión con insignia; entrar como administrador, modificar el código y aceptar.
4. Como estudiante, completar la misión con el código nuevo y revisar Insignias y Ranking.

=====================================================================
SEGUNDA ACTUALIZACION
=====================================================================
- Códigos distintos: las misiones publicadas sin código reciben uno propio (HALCON-####). Las que ya tenían HALCON2026 se marcan en el panel con un aviso; se cambian con "Modificar" o con "Nuevo código".
- Límite de intentos: 5 fallos en 10 minutos bloquean temporalmente el login (por correo + IP) y el código de una misión (por estudiante). Es en memoria: se reinicia al reiniciar la aplicación.
- Seguimiento de propuestas: el administrador puede rechazar con un motivo (o reconsiderar); el estudiante ve "En revisión / Aceptada / Rechazada" en Explorar > Mis propuestas. Columnas nuevas: Misiones.Rechazada y Misiones.NotaAdmin.
- Insignias: se eliminó Insignia.Desbloqueada y se borra su columna al iniciar (el desbloqueo se calcula por estudiante). FechaDesbloqueo sigue en la tabla pero ya no se usa.
- Eliminar: el aviso indica el nombre y cuántos estudiantes perderán progreso; tras borrar se muestra un mensaje de confirmación.
- Pendiente para ti: cambiar Admin:Password en appsettings.json y, si lo necesitas, pasar los cambios de base de datos a migraciones de EF.

=====================================================================
TERCERA ACTUALIZACION (requisitos de la Unidad 3)
=====================================================================
- Manejo de excepciones de base de datos: try/catch con DbUpdateException al guardar (registro, apodo, propuestas, progreso y todo el panel de administración) y un middleware en Program.cs que responde JSON claro en /api si falla la base de datos o la conexión.
- Cookie de autenticación: HttpOnly, SameSite=Lax y Secure cuando se usa HTTPS.
- Uso de HttpContext.Session: Explorar recuerda la última categoría elegida (api/SesionApi/filtro). Solo se guarda una categoría de una lista permitida. Se limpia al cerrar sesión y al iniciar otra sesión.
  Prueba sugerida: elegir "Cultura" en Explorar, ir a otra página y volver (se conserva); cerrar sesión, volver a entrar y comprobar que vuelve a "Todas".
- Identificación del usuario: cookie de autenticación con claims (id, nombre, correo, rol). La contraseña nunca se guarda en la sesión ni en la cookie; en la base de datos solo existe su hash.
