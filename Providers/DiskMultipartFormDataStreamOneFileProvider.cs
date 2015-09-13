using System.Net.Http;
using System.Net.Http.Headers;
using Common.Exceptions;

namespace Common.MVC.Providers
{
    public class DiskMultipartFormDataStreamOneFileProvider : MultipartFormDataStreamProvider
    {
        private bool _load;
        public readonly string Key;

        public DiskMultipartFormDataStreamOneFileProvider(string path)
            : base(path)
        {
        }

        public DiskMultipartFormDataStreamOneFileProvider(string path, string key)
            : base(path)
        {
            Key = key;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            if (_load) throw new ValidationServiceException("Нельзя передать за раз больше одного файла");
            _load = true;
            return Key;
        }
    }
}