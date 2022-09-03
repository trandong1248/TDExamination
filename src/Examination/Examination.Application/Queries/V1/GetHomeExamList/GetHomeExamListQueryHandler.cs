using AutoMapper;
using Examination.Domain.AggregateModels.ExamAggregate;
using Examination.Dto;
using MediatR;
using MongoDB.Driver;

namespace Examination.Application.Queries.V1.GetHomeExamList
{
    public class GetHomeExamListQueryHandler : IRequestHandler<GetHomeExamListQuery, IEnumerable<ExamDto>>
    {
        private readonly IExamRepository _examRepository;
        private readonly IClientSessionHandle _clientSessionHandle;
        private readonly IMapper _mapper;
        public GetHomeExamListQueryHandler(IExamRepository examRepository, IClientSessionHandle clientSessionHandle, IMapper mapper)
        {
            _examRepository = examRepository ?? throw new ArgumentNullException(nameof(examRepository));
            _clientSessionHandle = clientSessionHandle ?? throw new ArgumentNullException(nameof(clientSessionHandle));
            _mapper = mapper;   
        }

        public async Task<IEnumerable<ExamDto>> Handle(GetHomeExamListQuery request, CancellationToken cancellationToken)
        {
            var exams = await _examRepository.GetExamListAsync();
            var examDto = _mapper.Map<IEnumerable<ExamDto>>(exams);
            return examDto;
        }
    }
}