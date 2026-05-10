using School_Management.Core.Enums;
using School_Management.Core.Helpers;
using School_Management.Core.Interfaces.Application;
using School_Management.Core.Interfaces.Infrastructure;
using School_Management.Core.Models;

namespace School_Management.Application.Services
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _candidateRepository;

        public CandidateService(ICandidateRepository candidateRepository)
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
                        Message = $"សិស្សឈ្មោះ៖ \"{candidate.FullName}\" ត្រូវបានលុបជាការស្រេច!",
                        Status = Status.Success
                    };
                }
                else
                {
                    return new()
                    {
                        Message = $"មិនអាចរកឃើញសិស្សដែលមាន ID: \"{candidateId}\" នោះទេ!",
                        Status = Status.Rejected
                    };
                }
            }
            catch (Exception ex)
            {
                return new()
                {
                    Message = $"ប្រព័ន្ធបានចាប់កំហុសបច្ចេកទេសមួយនៅពេលដែលកំពុងព្យាយាមលុប\nError: {ex}",
                    Status = Status.Failed
                };
            }
        }

        public async Task<ReturnResponse> UpdateCandidateAsync(Candidate candidate)
        {
            ValidationResponse response = candidate.HasAllRequiredData();
            if (response.IsValid)
            {
                candidate.FirstName = candidate.FirstName.RemoveHiddenSpaces();
                candidate.LastName = candidate.LastName.RemoveHiddenSpaces();
                candidate.LatinFirstName = candidate.LatinFirstName.RemoveHiddenSpaces();
                candidate.LatinLastName = candidate.LatinLastName.RemoveHiddenSpaces();

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
                string message = "ព័ត៌មានសិស្សានុសិស្សមានការខ្វះខាត, សូមធ្វើការត្រួតពិនិត្យទៅលើព័ត៌មានខាងក្រោម៖";
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

        public async Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize)
        {
            try
            {
                IEnumerable<Candidate> candidates = await _candidateRepository.GetCandidatesOnlyAsync(page, pageSize);
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

        public async Task<ReturnResponse<IEnumerable<Candidate>>> GetAllAsync(int page, int pageSize, StudentFilterOptions options)
        {
            try
            {
                options ??= new StudentFilterOptions();
                IEnumerable<Candidate> candidates = await _candidateRepository.GetCandidatesOnlyPagedAsync(page, pageSize, options);
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

        public async Task<ReturnResponse<int>> GetAllCountAsync(StudentFilterOptions options)
        {
            try
            {
                options ??= new StudentFilterOptions();
                int count = await _candidateRepository.GetCandidatesOnlyCountAsync(options);
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

        public async Task<ReturnResponse> InsertCandidateAsync(Candidate candidate)
        {
            ValidationResponse response = candidate.HasAllRequiredData();

            if (response.IsValid)
            {
                candidate.FirstName = candidate.FirstName.RemoveHiddenSpaces();
                candidate.LastName = candidate.LastName.RemoveHiddenSpaces();
                candidate.LatinFirstName = candidate.LatinFirstName.RemoveHiddenSpaces();
                candidate.LatinLastName = candidate.LatinLastName.RemoveHiddenSpaces();

                try
                {
                    await _candidateRepository.AddAsync(candidate);
                }
                catch (Exception ex)
                {
                    return new()
                    {
                        Status = Status.Failed,
                        Message = $"There's an error when trying to insert the student:\n{ex.Message}"
                    };
                }

                return new()
                {
                    Status = Status.Success,
                    Message = $"Successfully updated the candidate named \"{candidate.FullName}\""
                };
            }
            else
            {
                string message = "ព័ត៌មានសិស្សានុសិស្សមានការខ្វះខាត, សូមធ្វើការត្រួតពិនិត្យទៅលើព័ត៌មានខាងក្រោម៖";
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
    }
}
