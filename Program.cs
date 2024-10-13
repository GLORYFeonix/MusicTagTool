using System.Text;
using System.Text.RegularExpressions;
using CliWrap;

internal class Tag
{
    public string? Title { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
    public string? AlbumArtist { get; set; }
    public string? Year { get; set; }
    public string? Track { get; set; }
}

internal class Program
{
    private static async Task Main(string[] args)
    {
        Console.InputEncoding = Encoding.UTF8;
        Console.WriteLine("输入文件名:");
        var srcFile = Console.ReadLine();

        if (!File.Exists(srcFile))
        {
            Console.WriteLine("文件不存在！");
            return;
        }

        var arguments = new List<string> { "-v", "quiet", "-show_format", srcFile };
        StringBuilder sb = new();
        await Cli.Wrap("ffprobe").WithArguments(arguments, true).WithStandardOutputPipe(PipeTarget.ToStringBuilder(sb)).ExecuteAsync();

        Tag tag = new();

        string pattern = @"TAG:ARTIST=([\S]+)\s";
        Match match = Regex.Match(sb.ToString(), pattern);
        if (match.Success)
        {
            tag.Artist = match.Groups[1].Value;
        }

        pattern = @"TAG:TITLE=([\S]+)\s";
        match = Regex.Match(sb.ToString(), pattern);
        if (match.Success)
        {
            tag.Title = match.Groups[1].Value;
        }

        pattern = @"TAG:ALBUM=([\S]+)\s";
        match = Regex.Match(sb.ToString(), pattern);
        if (match.Success)
        {
            tag.Album = match.Groups[1].Value;
        }

        pattern = @"TAG:album_artist=([\S]+)\s";
        match = Regex.Match(sb.ToString(), pattern);
        if (match.Success)
        {
            tag.AlbumArtist = match.Groups[1].Value;
        }

        pattern = @"TAG:DATE=([\S]+)\s";
        match = Regex.Match(sb.ToString(), pattern);
        if (match.Success)
        {
            tag.Year = match.Groups[1].Value;
        }

        pattern = @"TAG:track=([\S]+)\s";
        match = Regex.Match(sb.ToString(), pattern);
        if (match.Success)
        {
            tag.Track = match.Groups[1].Value;
        }

        var noDataFile = "NoData" + Path.GetExtension(srcFile);
        arguments = ["-i", srcFile, "-map_metadata", "-1", "-map", "0:a", "-c", "copy", noDataFile];
        await Cli.Wrap("ffmpeg").WithArguments(arguments, true).ExecuteAsync();

        var tmpFile = Path.GetFileNameWithoutExtension(srcFile) + "_src" + Path.GetExtension(srcFile);
        File.Move(srcFile, tmpFile);
        Console.WriteLine($"原文件已重命名为: {tmpFile}");

        arguments = ["-i", noDataFile];
        Console.WriteLine("标题：" + tag.Title);
        var title = Console.ReadLine();
        if (title != string.Empty)
        {
            tag.Title = title;
        }
        if (!string.IsNullOrEmpty(tag.Title))
        {
            arguments.Add("-metadata");
            arguments.Add($"title={tag.Title}");
        }
        Console.WriteLine("艺术家：" + tag.Artist);
        var artist = Console.ReadLine();
        if (artist != string.Empty)
        {
            tag.Artist = artist;
        }
        if (!string.IsNullOrEmpty(tag.Artist))
        {
            arguments.Add("-metadata");
            arguments.Add($"artist={tag.Artist}");
        }
        Console.WriteLine("专辑：" + tag.Album);
        var album = Console.ReadLine();
        if (album != string.Empty)
        {
            tag.Album = album;
        }
        if (!string.IsNullOrEmpty(tag.Album))
        {
            arguments.Add("-metadata");
            arguments.Add($"album={tag.Album}");
        }
        Console.WriteLine("专辑艺术家：" + tag.AlbumArtist);
        var albumArtist = Console.ReadLine();
        if (albumArtist != string.Empty)
        {
            tag.AlbumArtist = albumArtist;
        }
        if (!string.IsNullOrEmpty(tag.AlbumArtist))
        {
            arguments.Add("-metadata");
            arguments.Add($"albumArtist={tag.AlbumArtist}");
        }
        Console.WriteLine("年份：" + tag.Year);
        var year = Console.ReadLine();
        if (year != string.Empty)
        {
            tag.Year = year;
        }
        if (!string.IsNullOrEmpty(tag.Year))
        {
            arguments.Add("-metadata");
            arguments.Add($"year={tag.Year}");
        }
        Console.WriteLine("音轨号：" + tag.Track);
        var track = Console.ReadLine();
        if (track != string.Empty)
        {
            tag.Track = track;
        }
        if (!string.IsNullOrEmpty(tag.Track))
        {
            arguments.Add("-metadata");
            arguments.Add($"track={tag.Track}");
        }

        var trachNum = !string.IsNullOrEmpty(tag.Track) ? int.Parse(tag.Track) : 0;
        var dstFile = (trachNum < 10 ? $"0{tag.Track}" : $"{tag.Track}") + ". " + tag.Title + Path.GetExtension(srcFile);
        arguments.Add(dstFile);
        await Cli.Wrap("ffmpeg").WithArguments(arguments, true).ExecuteAsync();
    }
}