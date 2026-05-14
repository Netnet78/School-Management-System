namespace SchoolManagement.Application.Features.Candidates.Services
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
                string message = "?????????????????????????????????, ??????????????????????????????????????????";
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
                candidate.FirstName = Core.Shared.Extensions.StringExtensions.RemoveHiddenSpaces(candidate.FirstName);
                candidate.LastName = Core.Shared.Extensions.StringExtensions.RemoveHiddenSpaces(candidate.LastName);
                candidate.LatinFirstName = Core.Shared.Extensions.StringExtensions.RemoveHiddenSpaces(candidate.LatinFirstName);
                candidate.LatinLastName = Core.Shared.Extensions.StringExtensions.RemoveHiddenSpaces(candidate.LatinLastName);

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
                string message = "?????????????????????????????????, ??????????????????????????????????????????";
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


