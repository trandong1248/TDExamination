using AutoMapper;
using Examination.Domain.AggregateModels.ExamAggregate;
using Examination.Dto;

namespace Examination.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Exam, ExamDto>().ReverseMap();
        }
    }
}