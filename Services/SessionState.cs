using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prototipoGeminiready.Services
{
    public class SessionState
    {
        public string RfcUsuario { get; set; } = "";
        public Guid SesionId { get; set; } = Guid.NewGuid();
        public DateTime Inicio { get; set; } = DateTime.UtcNow;
    }

}
