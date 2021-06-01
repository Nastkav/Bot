using Google.Apis.Books.v1;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkBootsBot
{
    public class BookApi
    {
        private readonly BooksService _booksService;
        public BookApi(string apiKey)
        {
            _booksService = new BooksService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = this.GetType().ToString()
            });
        }
        public List<Book> Search(string query, int offset, int count)
        {
            var listquery = _booksService.Volumes.List(query);
            listquery.PrintType = VolumesResource.ListRequest.PrintTypeEnum.BOOKS;
            listquery.MaxAllowedMaturityRating = VolumesResource.ListRequest.MaxAllowedMaturityRatingEnum.MATURE;
            listquery.MaxResults = count;
            listquery.StartIndex = offset;
            listquery.Filter = VolumesResource.ListRequest.FilterEnum.Ebooks;
            var res = listquery.Execute();

            var books = res.Items.Select(b => new Book
            {
                Id = b.Id,
                Title = b.VolumeInfo.Title,
                // Subtitle = b.VolumeInfo.Subtitle,
                // Description = b.VolumeInfo.Description,
                // PageCount = b.VolumeInfo.PageCount,
                Link = b.VolumeInfo.CanonicalVolumeLink,
            }).ToList();
            return new List<Book>(books);
        }
        

        public List<Book> Best(int offset, int count)
        {
            var listquery = _booksService.Volumes.List("бестселлер");
            listquery.PrintType = VolumesResource.ListRequest.PrintTypeEnum.BOOKS;
            listquery.MaxAllowedMaturityRating = VolumesResource.ListRequest.MaxAllowedMaturityRatingEnum.MATURE;
            listquery.MaxResults = count;
            listquery.StartIndex = offset;
            listquery.Filter = VolumesResource.ListRequest.FilterEnum.Ebooks;
            var res = listquery.Execute();
            var books = res.Items.Select(b => new Book
            {
                Id = b.Id,
                Title = b.VolumeInfo.Title,
                // Subtitle = b.VolumeInfo.Subtitle,
                // Description = b.VolumeInfo.Description,
                // PageCount = b.VolumeInfo.PageCount,
                Link = b.VolumeInfo.CanonicalVolumeLink,
            }).ToList();
            return new List<Book>(books);
        }

        public List<Book> RandomBook(int offset, int count)
        {
            Random r = new Random();
            int i = r.Next(65, 90);
            char a = Convert.ToChar(i);
            var listquery = _booksService.Volumes.List(a.ToString()); ;
            listquery.PrintType = VolumesResource.ListRequest.PrintTypeEnum.BOOKS;
            listquery.MaxAllowedMaturityRating = VolumesResource.ListRequest.MaxAllowedMaturityRatingEnum.MATURE;
            listquery.MaxResults = count;
            listquery.StartIndex = offset;
            listquery.Filter = VolumesResource.ListRequest.FilterEnum.Ebooks;
            var res = listquery.Execute();
            var books = res.Items.Select(b => new Book
            {
                Id = b.Id,
                Title = b.VolumeInfo.Title,
                // Subtitle = b.VolumeInfo.Subtitle,
                // Description = b.VolumeInfo.Description,
                // PageCount = b.VolumeInfo.PageCount,
                Link = b.VolumeInfo.CanonicalVolumeLink,
            }).ToList();
            return new List<Book>(books);
        }
    }
}