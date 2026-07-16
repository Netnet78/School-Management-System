using SchoolManagement.Application.Common;

namespace SchoolManagement.Application.Features.Subjects.Services
{
    public class SubjectService : CrudServiceBase<Subject>, ISubjectService
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectService(ISubjectRepository repository) : base(repository)
        {
            _subjectRepository = repository;
        }

        public async Task<IEnumerable<SubjectMapper>> GetMappersForSubjectAsync(int subjectId)
        {
            return await _subjectRepository.GetMappersForSubjectAsync(subjectId);
        }

        public override async Task<ReturnResponse> InsertAsync(Subject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                await ResolveComponentsAsync(entity);
                return await base.InsertAsync(entity);
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"មិនអាចបន្ថែមមុខវិជ្ជាបានទេ។\n{ex.Message}"
                };
            }
        }

        public override async Task<ReturnResponse> UpdateAsync(Subject entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            try
            {
                Subject? existing = await _subjectRepository.GetByIdWithMappersAsync(entity.Id);
                if (existing is null)
                {
                    return new() { Status = Status.Failed, Message = "មិនអាចរកឃើញមុខវិជ្ជា" };
                }

                existing.Name = entity.Name.Trim();
                existing.KhmerName = entity.KhmerName.Trim();
                existing.MaxScore = entity.MaxScore;
                existing.IsActive = entity.IsActive;

                await ResolveComponentsAsync(entity);

                CollectionSynchronizer.Sync(
                    existing.Mappers,
                    entity.Mappers,
                    ex => ex.Component.Name,
                    en => en.Component.Name,
                    (ex, en) => { ex.Component = en.Component; },
                    en => new SubjectMapper()
                    {
                        Component = en.Component
                    });

                await _subjectRepository.UpdateAsync(existing);
                return new() { Status = Status.Success };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"មិនអាចកែប្រែមុខវិជ្ជាបានទេ។\n{ex.Message}"
                };
            }
        }

        private async Task ResolveComponentsAsync(Subject entity)
        {
            // Build incoming list with both name and khmer name (trimmed), de-duplicated by name
            var incoming = entity.Mappers
                .Where(m => m.Component != null && !string.IsNullOrWhiteSpace(m.Component.Name))
                .Select(m => new
                {
                    Name = m.Component!.Name.Trim(),
                    KhmerName = (m.Component.KhmerName ?? string.Empty).Trim()
                })
                .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            // If no component entries provided, fall back to subject's Name + KhmerName
            if (incoming.Count == 0)
            {
                incoming.Add(new { Name = entity.Name.Trim(), KhmerName = (entity.KhmerName ?? string.Empty).Trim() });
            }

            var incomingNames = incoming.Select(i => i.Name).ToList();

            Dictionary<string, SubjectComponent> existing = await _subjectRepository
                .FindComponentsByNamesAsync(incomingNames);

            List<SubjectMapper> newMappers = incomingNames
                .Select(name =>
                {
                    var incomingKhmer = incoming.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.KhmerName ?? string.Empty;

                    if (existing.TryGetValue(name, out SubjectComponent? comp))
                    {
                        // Update existing component's KhmerName when provided
                        if (!string.IsNullOrWhiteSpace(incomingKhmer) && comp.KhmerName != incomingKhmer)
                        {
                            comp.KhmerName = incomingKhmer;
                        }

                        return new SubjectMapper { Component = comp };
                    }

                    // New component: set both Name and KhmerName
                    return new SubjectMapper { Component = new SubjectComponent { Name = name, KhmerName = incomingKhmer } };
                })
                .ToList();

            entity.Mappers = newMappers;
        }
    }
}