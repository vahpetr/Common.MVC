using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Common.Utilites;

namespace Common.MVC.HttpActionResults
{
    public class ResponseFile : IHttpActionResult
    {
        private Stream Stream { get; set; }
        private string Name { get; set; }
        private string Ext { get; set; }

        private ResponseFile(string name, string ext)
        {
            Name = name;
            Ext = ext;
        }

        public ResponseFile(string path)
            : this(Path.GetFileName(path), Path.GetExtension(path))
        {
            Stream = File.OpenRead(path);
        }

        public ResponseFile(string path, string name, string ext)
            : this(name, ext)
        {
            Stream = File.OpenRead(path);
        }

        public ResponseFile(byte[] content, string name, string ext)
            : this(name, ext)
        {
            Stream = new MemoryStream(content);
        }

        public ResponseFile(Stream stream, string name, string ext)
            : this(name, ext)
        {
            Stream = stream;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(WebApiUtilites.CreateResponseFile(Stream, Name, Ext));
        }
    }
}