using SchoolManagement.Core.Features.Candidates.DTOs;

namespace SchoolManagement.Application.Features.Candidates.Services
{
    public class CandidateService : CrudServiceBase<Candidate>, ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;

        public CandidateService(ICandidateRepository candidateRepository)
            : base(candidateRepository)
        {
            _candidateRepository = candidateRepository;
        }

        public async Task<ReturnResponse> DeleteCandidateAsync(int candidateId)
        {
            Candidate? candidate = await _candidateRepository.GetByIdAsync(candidateId);

            try
            {
                if (candidate != null)
                {
                    await _candidateRepository.DeleteAsync(candidate);
                    return new()
                    {
                        Message = $"ព័ត៌មានសិស្សឈ្មោះ \"{candidate.FullName}\" ត្រូវបានលុបរួចរាល់ជាការស្រេច!",
                        Status = Status.Success
                    };
                }
                else
                {
                    return new()
                    {
                        Message = $"មិនអាចរកឃើញសិស្សដែលមានលេខ ID: \"{candidateId}\" នោះទេ!",
                        Status = Status.Rejected
                    };
                }
            }
            catch (Exception ex)
            {
                return new()
                {
                    Message = $"មានកំហុសបច្ចេកទេសក្នុងការលុបទិន្នន័យ\nError: {ex}",
                    Status = Status.Failed
                };
            }
        }

        public async Task<ReturnResponse> UpdateCandidateAsync(Candidate candidate)
        {
            ValidationResponse response = candidate.HasAllRequiredData();
            if (response.IsValid)
            {
                candidate.FirstName = StringExtensions.RemoveHiddenSpaces(candidate.FirstName, true);
                candidate.LastName = StringExtensions.RemoveHiddenSpaces(candidate.LastName, true);
                candidate.LatinFirstName = StringExtensions.RemoveHiddenSpaces(candidate.LatinFirstName, true);
                candidate.LatinLastName = StringExtensions.RemoveHiddenSpaces(candidate.LatinLastName, true);

                try
                {
                    await _candidateRepository.UpdateAsync(candidate);
                }
                catch (Exception ex)
                {
                    return new() { Status = Status.Failed, Message = $"There's an error when trying to update the student:\n{ex.Message}" };
                }

                return new()
                {
                    Status = Status.Success,
                    Message = $"Successfully updated the candidate named \"{candidate.FullName}\""
                };
            }
            else
            {
                string message = "ទិន្នន័យសិស្សមាននៅមានភាព​ខ្វះខាត, សូមធ្វើការត្រួតពិនិត្យលើព័ត៌មានបញ្ចូលថែមទៀត!";
                for (int i = 0; i < response.MissingProperties.Length; i++)
                {
                    ValidationError missing = response.MissingProperties[i];
                    message += $"\n{i + 1}. {missing.PropertyName}: {missing.ErrorMessage}";
                }
                return new()
                {
                    Status = Status.Rejected,
                    Message = message,
                };
            }
        }

        public async Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize,
            IEnumerable<FilterCondition<Candidate>>? filters,
            StudentDataStateFilterOptions dataState,
            string? sortBy,
            OrderDirection orderBy)
        {
            try
            {
                IEnumerable<Candidate> candidates = await _candidateRepository.GetPagedAsync(
                    page, pageSize, filters, dataState, sortBy, orderBy);
                return new()
                {
                    Status = Status.Success,
                    Value = candidates,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"There's an error when trying to retrieve the candidates data\n{ex.Message}"
                };
            }
        }

        public async Task<ReturnResponse<int>> GetAllCountAsync(
            IEnumerable<FilterCondition<Candidate>>? filters,
            StudentDataStateFilterOptions dataState)
        {
            try
            {
                int count = await _candidateRepository.GetCountAsync(filters, dataState);
                return new()
                {
                    Status = Status.Success,
                    Value = count,
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"There's something wrong when trying to get the candidates count\n{ex.Message}",
                };
            }
        }

        public async Task<ReturnResponse<CandidateDashboardMetrics>> GetDashboardMetricsAsync(int? daysFilter)
        {
            try
            {
                var result = await _candidateRepository.GetDashboardMetricsAsync(daysFilter, DateTime.UtcNow);
                return new()
                {
                    Status = Status.Success,
                    Value = result
                };
            }
            catch (Exception ex)
            {
                return new()
                {
                    Status = Status.Failed,
                    Message = $"ទិន្នន័យសង្ខេបមិនត្រូវបានទាញយកបានដោយជោគជ័យនោះទេ! មូលហេតុ៖\n{ex.Message}"
                };
            }
        }
    }
}
