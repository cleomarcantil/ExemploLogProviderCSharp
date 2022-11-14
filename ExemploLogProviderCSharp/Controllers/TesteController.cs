using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ExemploLogProviderCSharp.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TesteController : ControllerBase
	{

		[HttpGet]
		public string Teste()
		{
			return "Teste";
		}

	}
}
