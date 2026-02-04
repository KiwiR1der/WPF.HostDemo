using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;

namespace WPF.HostDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IHost? _host;
        public MainWindow()
        {
            InitializeComponent();

            // 订阅日志事件
            UiLogger.OnLogReceived += UiLogger_OnLogReceived;
        }

        private void UiLogger_OnLogReceived(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"[{System.DateTime.Now:HH:mm:ss}] {msg}\r\n");
                txtLog.ScrollToEnd();
            });
        }

        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            // 1.创建 Builder
            var builder = WebApplication.CreateBuilder();

            //2.配置监听地址和端口
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(9000); // 监听 http://localhost:9000
            });

            // 3.添加服务
            builder.Services.AddControllers();
            builder.Services.AddRouting();

            // 3.1 添加 Swagger 支持（可选）
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 4.构建应用
            var webApp = builder.Build();

            // 5.映射控制器路由
            webApp.UseSwagger();    // 可选：启用 Swagger 生成的 JSON 文档
            webApp.UseSwaggerUI();  // 可选：启用 Swagger UI 界面，默认地址 /swagger

            webApp.MapControllers();

            _host = webApp;

            // 6.启动应用（非阻塞方式）
            await _host.StartAsync();

            UiLogger.Log("Web Host started on http://localhost:9000" + Environment.NewLine);

        }

        private const string BaseAddress = "http://localhost:9000/";
        private const string BaseApiUrl = BaseAddress + "iIot/api/user/";
      
        private async void btnGet_Click(object sender, RoutedEventArgs e)
        {
            string getUrl = BaseApiUrl + "status";

            // 建议：HttpClient 最好做成静态单例，不要在 using 里频繁创建（虽然测试用例里这样写也没大问题）
            using (var client = new HttpClient())
            {
                try
                {
                    // 注意 2：使用 await，不要用 .Result
                    // 这会让出 UI 线程，让 Controller 里的 Dispatcher.Invoke 能够执行
                    var response = await client.GetAsync(getUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        // 注意 3：这里也要用 await
                        var result = await response.Content.ReadAsStringAsync();
                        UiLogger.Log("GET 响应: " + result + Environment.NewLine);
                    }
                    else
                    {
                        UiLogger.Log("GET 请求失败: " + response.StatusCode + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    UiLogger.Log("请求异常: " + ex.Message);
                }
            }
        }

        // 同理修改 Post 方法
        private async void btnPost_Click(object sender, RoutedEventArgs e)
        {
            string postUrl = BaseApiUrl + "post";

            var postData = new
            {
                Name = "张三",
                Age = 30
            };

            using (var client = new HttpClient())
            {
                try
                {
                    // 使用 await
                    var response = await client.PostAsJsonAsync(postUrl, postData);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        UiLogger.Log("POST 响应: " + result + Environment.NewLine);
                    }
                    else
                    {
                        UiLogger.Log("POST 请求失败: " + response.StatusCode + Environment.NewLine);
                    }
                }
                catch (Exception ex)
                {
                    UiLogger.Log("请求异常: " + ex.Message);
                }
            }
        }

        protected override async void OnClosing(CancelEventArgs e)
        {
            // 优雅关闭服务
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }

            base.OnClosing(e);
        }
    }
}