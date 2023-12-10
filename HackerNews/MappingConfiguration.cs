using AutoMapper;
using HackerNews.Models;

namespace HackerNews
{
    public class MappingConfiguration : Profile
    {
        public MappingConfiguration()
        {
            CreateMap<Story, StoryResponse>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Uri, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.PostedBy, opt => opt.MapFrom(src => src.By))
                .ForMember(dest => dest.Time, opt => opt.ConvertUsing(new TimeConverter(), src => src.Time))
                .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.Score))
                .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Kids.Count()));
        }
    }

    class TimeConverter : IValueConverter<int, DateTime>
    {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
        public DateTime Convert(int source, ResolutionContext resolutionContext) 
        {
            return origin.AddSeconds(source);
        }
    }

}
