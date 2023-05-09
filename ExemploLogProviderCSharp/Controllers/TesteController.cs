using ExemploLogProviderCSharp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExemploLogProviderCSharp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TesteController : ControllerBase
	{
		private readonly ILogger<TesteController> logger;
		private readonly TesteService testeService;

		public TesteController(ILogger<TesteController> logger, TesteService testeService)
		{
			this.logger = logger;
			this.testeService = testeService;
		}

		[HttpGet]
		public async Task<string> Teste()
		{
			using var loggerScope = logger.BeginScope("Scopo1", "A", "B");

			logger.LogInformation($"TesteController.Teste");

			await testeService.Metodo1();

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
