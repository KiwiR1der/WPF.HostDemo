namespace WPF.HostDemo
{
    // 用于在 Controller 和 WPF 界面之间传递日志消息
    public static class UiLogger
    {
        // 事件，当有新的日志消息时触发
        public static event Action<string>? OnLogReceived;

        public static void Log(string message)
        {
            OnLogReceived?.Invoke(message);
        }
    }
}
