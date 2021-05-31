using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Immanuel.ark.ydl
{
    [Route("api/yt")]
    [ApiController]
    public class YtController : ControllerBase
    {
        IWebHostEnvironment _env;
        public YtController(IWebHostEnvironment env)
        {
            _env = env;
        }
        public static Func<string> CurrentTimeStamp = () => $"{DateTime.Now.Year}{DateTime.Now.Month.ToString().PadLeft(2, '0')}{DateTime.Now.Day.ToString().PadLeft(2, '0')}_{DateTime.Now.Hour.ToString().PadLeft(2, '0')}{DateTime.Now.Minute.ToString().PadLeft(2, '0')}{DateTime.Now.Second.ToString().PadLeft(2, '0')}_{DateTime.Now.Millisecond.ToString().PadLeft(4, '0')}";
        [HttpPost]
        public async Task<FileStreamResult> DownloadVideo()
        {
            string videoIdOrUrl = Request.Form["yurl"];
            if (string.IsNullOrEmpty(videoIdOrUrl)) throw new ApplicationException("Youtube url is mandatory.");
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoIdOrUrl);
            var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
            if (streamInfo is null) throw new ApplicationException("This video has no muxed streams.");
            var fileName =   $"{CurrentTimeStamp()}.{streamInfo.Container.Name}";
            using (var progress = new InlineProgress()) // display progress in console
                await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, progress);
            return File(System.IO.File.OpenRead(fileName), "application/octet-stream");
        }
    }
    internal class InlineProgress : IProgress<double>, IDisposable
    {
        private readonly int _posX;
        private readonly int _posY;

        public InlineProgress()
        {
            _posX = Console.CursorLeft;
            _posY = Console.CursorTop;
        }

        public void Report(double progress)
        {
            Console.SetCursorPosition(_posX, _posY);
            Console.WriteLine($"{progress:P1}");
        }

        public void Dispose()
        {
            Console.SetCursorPosition(_posX, _posY);
            Console.WriteLine("Completed ✓");
        }
    }
}