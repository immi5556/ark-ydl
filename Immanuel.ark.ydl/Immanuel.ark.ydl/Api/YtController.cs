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
        [Route("dld/vid")]
        public async Task<FileStreamResult> DownloadVideo()
        {
            string videoIdOrUrl = Request.Form["yurl"];
            if (string.IsNullOrEmpty(videoIdOrUrl)) throw new ApplicationException("Youtube url is mandatory.");
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoIdOrUrl);
            var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
            if (streamInfo is null) throw new ApplicationException("This video has no muxed streams.");

            var video = await youtube.Videos.GetAsync(videoIdOrUrl);
            var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
            var author = video.Author.Title; // "Blender"
            var duration = video.Duration;

            var fileName = System.IO.Path.Combine(_env.WebRootPath, $"{CurrentTimeStamp()}.{streamInfo.Container.Name}");
            await youtube.Videos.Streams.DownloadAsync(streamInfo, fileName, null);
            return File(System.IO.File.OpenRead(fileName), "application/octet-stream");
        }
    }
}