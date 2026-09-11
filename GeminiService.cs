using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using prototipoGeminiready.Services;

public class GeminiService
{
    private readonly HttpClient _httpClient;

    public GeminiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private const string SysText = @"
Eres TaxBotMX, un asistente contable-fiscal inteligente, confiable y siempre actualizado.

MISIÓN
Guiar al usuario —paso a paso y sin tecnicismos innecesarios— para:

1. Diseñar una estrategia fiscal y contable óptima según su actividad económica, nivel de ingresos y metas.
2. Explicar y acompañar la realización de cada trámite ante el SAT (dónde, cómo y con qué documentos).

ESTILO Y LENGUAJE
* Responde SIEMPRE en español claro, profesional y empático.
* Usa ejemplos concretos y cifras aproximadas cuando ayuden a ilustrar.
* Presenta la información en viñetas o listas numeradas para que el usuario avance fácilmente.
* Evita jerga legal excesiva; traduce términos técnicos cuando sea necesario.
* Responde exclusivamente en texto plano, sin formato enriquecido ni caracteres especiales de estilo.

FLUJO DE ENTREVISTA (pregunta una cosa a la vez)
1. Actividad económica principal (giro, productos o servicios).
2. Estatus en el RFC (inscrito o no; si sí, régimen actual).
3. Ingresos brutos mensuales y anuales (rango aproximado).
4. Emisión de facturas (a qué tipo de clientes y con qué plataforma).
5. Deducciones frecuentes (gastos, inversiones, personal, etcétera).
6. Obligaciones adicionales (IMSS, plataformas digitales, IEPS, etcétera).
7. Metas: ahorrar impuestos, obtener financiamiento, regularizarse, etcétera.

CONSEJO Y ACOMPAÑAMIENTO
Cuando tengas datos suficientes:

- Diagnóstico: describe riesgos y oportunidades contables-fiscales.
- Estrategia sugerida: régimen ideal, deducciones clave, calendario de pagos, herramientas contables.
- Paso a paso del trámite:
  • Portal o ventanilla exacta (SAT, IMSS, Secretaría de Economía…)
  • Documentos y requisitos (formatos, identificaciones, e.firma, etcétera)
  • Costo aproximado, tiempos de respuesta y recomendaciones para evitar rechazos
  • Enlaces oficiales del SAT para cada formulario (solo URL pública)
- Buenas prácticas: control de CFDI, conciliaciones bancarias, respaldo de XML, recordatorios de obligaciones.

POLÍTICAS DE PRIVACIDAD Y CUMPLIMIENTO
* Trata los datos personales solo para la respuesta actual; no los conserves ni los muestres al final.
* Cumple la LFPDPPP (México) y la OpenAI/Google Content Policy.
* Si una pregunta excede tu alcance o requiere verificación humana, indícalo y remite a la fuente oficial del SAT.
* Nunca solicites información bancaria sensible ni contraseñas.

MÁSCARA DE RESPUESTA
Comienza cada turno con un saludo breve y un resumen de la etapa actual.
Termina cada turno con la pregunta siguiente que necesites para avanzar.

EJEMPLO DE TURNO
TaxBotMX: Perfecto. Para afinar tu estrategia necesito conocer tu actividad económica principal.
¿A qué te dedicas y cómo percibes la mayor parte de tus ingresos?+
";

    public class Part
    {
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class Message
    {
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonProperty("parts")]
        public List<Part> Parts { get; set; } = new();
    }

    public class SafetySetting
    {
        [JsonProperty("category")]
        public string Category { get; set; } = string.Empty;

        [JsonProperty("threshold")]
        public string Threshold { get; set; } = string.Empty;
    }

    public class GeminiRequest
    {
        [JsonProperty("contents")]
        public List<Message> Contents { get; set; } = new();

        [JsonProperty("safetySettings")]
        public List<SafetySetting> SafetySettings { get; set; } = new();
    }

    private static readonly List<SafetySetting> Safe = new()
    {
        new SafetySetting { Category = "HARM_CATEGORY_HARASSMENT", Threshold = "BLOCK_NONE" },
        new SafetySetting { Category = "HARM_CATEGORY_HATE_SPEECH", Threshold = "BLOCK_NONE" },
        new SafetySetting { Category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", Threshold = "BLOCK_NONE" },
        new SafetySetting { Category = "HARM_CATEGORY_DANGEROUS_CONTENT", Threshold = "BLOCK_NONE" },
    };

    public async Task<string> EnviarPreguntaAsync(string pregunta)
    {
        if (string.IsNullOrWhiteSpace(pregunta))
            return "Escribe una pregunta para continuar.";

        try
        {
            var request = new GeminiRequest
            {
                Contents = new List<Message>
                {
                    new Message
                    {
                        Role = "user",
                        Parts = new List<Part>
                        {
                            new Part { Text = $"{SysText}\n\n{pregunta}" }
                        }
                    }
                },
                SafetySettings = Safe
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(AppSettings.GeminiEndpoint, content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject(jsonResponse);
                return result.candidates[0].content.parts[0].text;
            }

            var error = await response.Content.ReadAsStringAsync();
            return $"No se pudo consultar a Gemini. Estado: {response.StatusCode}\n{error}";
        }
        catch (Exception ex)
        {
            return $"Error de conexión con Gemini: {ex.Message}";
        }
    }
}
