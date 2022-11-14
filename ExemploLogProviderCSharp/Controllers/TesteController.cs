using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExemploLogProviderCSharp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TesteController : ControllerBase
	{
		private readonly ILogger<TesteController> logger;

		public TesteController(ILogger<TesteController> logger)
		{
			this.logger = logger;
		}

		[HttpGet]
		public string Teste()
		{
			logger.LogInformation($"Chamado método {nameof(Teste)}");

			return "Teste";
		}


		[HttpPost]
		public void TesteErro()
		{
			try
			{
				throw new Exception("Teste erro!");
			}
			catch (Exception ex)
			{
				logger.LogError(ex, $"Chamado método {nameof(TesteErro)}");
			}
		}

	}
}
