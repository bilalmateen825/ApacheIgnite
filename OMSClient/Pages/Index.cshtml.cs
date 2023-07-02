using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using SignalR.Common;

namespace OMSClient.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHubContext<OMSHub> _hubContext;

        public IndexModel(ILogger<IndexModel> logger, IHubContext<OMSHub> hubContext)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void OnGet()
        {

        }
        public async Task<IActionResult> OnPostSendMessage(string user, string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", user, message);
            return new EmptyResult();
        }
    }
}