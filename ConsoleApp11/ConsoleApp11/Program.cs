namespace YouTubeCommandPattern
{
    // Интерфейс команды
    public interface ICommand
    {
        Task ExecuteAsync(string url);
    }

    // Команда для получения информации о видео
    public class GetVideoInfoCommand : ICommand
    {
        public async Task ExecuteAsync(string url)
        {
            var videoId = GetVideoId(url);
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "YOUR_API_KEY", // Замените на ваш API ключ
                ApplicationName = GetType().ToString()
            });

            var videosListRequest = youtubeService.Videos.List("snippet");
            videosListRequest.Id = videoId;

            var response = await videosListRequest.ExecuteAsync();
            var video = response.Items[0];

            Console.WriteLine($"Title: {video.Snippet.Title}");
            Console.WriteLine($"Description: {video.Snippet.Description}");
        }

        private string GetVideoId(string url)
        {
            // Извлечение videoId из URL (предполагается, что он в стандартном формате)
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
    }

    // Команда для скачивания видео
    public class DownloadVideoCommand : ICommand
    {
        public async Task ExecuteAsync(string url)
        {
            var videoId = GetVideoId(url);
            var youtube = new YoutubeClient();

            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);
            var streamInfo = streamManifest.GetMuxed().WithHighestBitrate();
            var filePath = $"{videoId}.mp4";

            await youtube.Videos.Streams.DownloadAsync(streamInfo, filePath);
            Console.WriteLine($"Video downloaded: {filePath}");
        }

        private string GetVideoId(string url)
        {
            var uri = new Uri(url);
            var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
            return query["v"];
        }
    }

    // Класс для обработки ввода команды
    public class CommandInvoker
    {
        private ICommand _command;

        public void SetCommand(ICommand command)
        {
            _command = command;
        }

        public async Task ExecuteCommandAsync(string url)
        {
            if (_command != null)
            {
                await _command.ExecuteAsync(url);
            }
        }
    }

    // Основной класс программы
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Введите URL YouTube видео:");
            var url = Console.ReadLine();

            CommandInvoker invoker = new CommandInvoker();

            // Команда для получения информации о видео
            var getInfoCommand = new GetVideoInfoCommand();
            invoker.SetCommand(getInfoCommand);
            await invoker.ExecuteCommandAsync(url);

            // Команда для скачивания видео
            var downloadCommand = new DownloadVideoCommand();
            invoker.SetCommand(downloadCommand);
            await invoker.ExecuteCommandAsync(url);
        }
    }
}
