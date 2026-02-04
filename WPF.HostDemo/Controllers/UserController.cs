using Microsoft.AspNetCore.Mvc;
using WPF.HostDemo.Models;

namespace WPF.HostDemo.Controllers
{
    [ApiController]
    [Route("iIot/api/user")]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("status")]
        public IActionResult GetStatus()
        {
            return Ok(new { Message = "WPF WebApi 服务正在运行 (.NET 8)", Time = DateTime.Now });
        }

        [HttpPost]
        [Route("post")]
        public IActionResult ReceiveData([FromBody] UserData data)
        {
            if (data == null)
                return BadRequest("数据不能为空");

            string responseMsg = $"收到数据: Name = {data.Name}, Age = {data.Age}";

            // 调用静态帮助类，将消息发送给 WPF 界面
            UiLogger.Log(responseMsg);

            return Ok(new { Result = true, Message = responseMsg });
        }
    }
}
