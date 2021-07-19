using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        public static Func<string, string> GetValidFileName = (fileName) =>
        {
            // remove any invalid character from the filename.
            string ret = Regex.Replace(fileName.Trim(), "[^A-Za-z0-9_. ]+", "");
            return ret.Replace(" ", "_");
        };
        [HttpPost]
        [Route("dld/vid")]
        public async Task<dynamic> DownloadVideo()
        {
            string videoIdOrUrl = Request.Form["yurl"];
            //if (string.IsNullOrEmpty(videoIdOrUrl)) throw new ApplicationException("Youtube url is mandatory.");
            return await DownloadVideoGet(videoIdOrUrl);
        }
        [HttpGet]
        [Route("dld/vid/{yurl}")]
        public async Task<dynamic> DownloadVideoGet(string yurl)
        {
            if (string.IsNullOrEmpty(yurl)) throw new ApplicationException("Youtube url is mandatory.");
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(yurl);
            var streamInfo = streamManifest.GetMuxedStreams().TryGetWithHighestVideoQuality();
            if (streamInfo is null) throw new ApplicationException("This video has no muxed streams.");

            var video = await youtube.Videos.GetAsync(yurl);
            var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
            var author = video.Author.Title; // "Blender"
            var duration = video.Duration;

            var fileName = $"{CurrentTimeStamp()}_{GetValidFileName(title)}_{GetValidFileName(author)}.{streamInfo.Container.Name}";
            var download_filename = (fileName.Length > 25 ? fileName.Substring(0, 25) : fileName) + "." + streamInfo.Container.Name;
            fileName = $"{fileName}.{streamInfo.Container.Name}";
            var fullpath = System.IO.Path.Combine(_env.WebRootPath, fileName);

            await youtube.Videos.Streams.DownloadAsync(streamInfo, fullpath, null);
            //return File(System.IO.File.OpenRead(fullpath), "application/octet-stream", fileName);
            return new
            {
                Path = $"/{fileName}",
                FileName = download_filename
            };
        }
        [HttpPost]
        [Route("dld/aud")]
        public async Task<dynamic> DownloadAud()
        {
            string videoIdOrUrl = Request.Form["yurl"];
            return await DownloadAudioGet(videoIdOrUrl);
        }
        [HttpGet]
        [Route("dld/aud/{yurl}")]
        public async Task<dynamic> DownloadAudioGet(string yurl)
        {
            string videoIdOrUrl = yurl;
            if (string.IsNullOrEmpty(videoIdOrUrl)) throw new ApplicationException("Youtube url is mandatory.");
            var youtube = new YoutubeClient();
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoIdOrUrl);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            if (streamInfo is null) throw new ApplicationException("This video has no audio streams.");

            var video = await youtube.Videos.GetAsync(videoIdOrUrl);
            var title = video.Title; // "Collections - Blender 2.80 Fundamentals"
            var author = video.Author.Title; // "Blender"
            var duration = video.Duration;

            var fileName = $"{CurrentTimeStamp()}_{GetValidFileName(title)}_{GetValidFileName(author)}.{streamInfo.Container.Name}";
            var download_filename = (fileName.Length > 25 ? fileName.Substring(0, 25) : fileName) + "." + streamInfo.Container.Name;
            fileName = $"{fileName}.{streamInfo.Container.Name}";
            var fullpath = System.IO.Path.Combine(_env.WebRootPath, fileName);

            await youtube.Videos.Streams.DownloadAsync(streamInfo, fullpath, null);
            //return File(System.IO.File.OpenRead(fullpath), "application/octet-stream", fileName);
            return new
            {
                Path = $"/{fileName}",
                FileName = download_filename
            };
        }
    }
}