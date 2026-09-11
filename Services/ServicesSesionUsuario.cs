using System;

namespace prototipoGeminiready.Services
{
    public class SesionUsuario
    {
        public string Rfc { get; set; } = "";
        public Guid SesionId { get; set; } = Guid.NewGuid();
        public DateTime InicioSesion { get; set; } = DateTime.UtcNow;
    }
}
