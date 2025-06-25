using Microsoft.Extensions.Logging;
using UCI.Middleware.Entities.Entities.Ivass;
using UCI.Middleware.Entities.Enums.Ivass;
using UCI.Middleware.Integration.Database.Interfaces;
using UCI.Middleware.Repositories.Interfaces;
using UCI.Middleware.Services.Interfaces;

namespace UCI.Middleware.Integration.Database.Implementation
{
    /// <summary>
    /// Service implementation for ClaimsSubmission business operations.
    /// </summary>
    public class ClaimsSubmissionService : BaseService<ClaimsSubmission, Guid>, IClaimsSubmissionService
    {
        private readonly ILogger<ClaimsSubmissionService> _specificLogger;

        public ClaimsSubmissionService(
            IUnitOfWork unitOfWork,
            ILogger<ClaimsSubmissionService> logger)
            : base(unitOfWork, logger as ILogger<BaseService<ClaimsSubmission, Guid>>)
        {
            _specificLogger = logger;
        }

        protected override IRepository<ClaimsSubmission> Repository => _unitOfWork.ClaimsSubmissions;

        public async Task<ClaimsSubmission?> GetSubmission(Guid id)
        {

            try
            {
                _specificLogger.LogDebug("Getting submission by ID {Id}", id);
                return await _unitOfWork.ClaimsSubmissions.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting submission by ID {Id}", id);
                throw;
            }
        }

        public async Task<ClaimsSubmission> CreateSubmissionFromFileAsync(string inputFileName,
            string inputFileFullPath, Guid correspondentId)
        {
            try
            {
                _specificLogger.LogInformation("Creating submission from file {FileName} for correspondent {CorrespondentId}",
                    inputFileName, correspondentId);


                var submission = new ClaimsSubmission
                {
                    Id = Guid.NewGuid(),
                    InputFileName = inputFileName,
                    InputFileFullPath = inputFileFullPath,
                    UploadDate = DateTime.UtcNow,
                    SubmissionStatusId = SubmissionStatusType.Uploaded.GetId(),
                    CorrespondentId = correspondentId
                };

                var result = await CreateAsync(submission);
                _specificLogger.LogInformation("Successfully created submission {Id} from file {FileName}", result.Id, inputFileName);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error recording response for file {file} for {CorrespondentId}", inputFileName, correspondentId);
                throw;
            }
        }

        public async Task<PaginatedResult<ClaimsSubmission>> GetByStatusPagedAsync(SubmissionStatusType status,
            int pageNumber = 1, int pageSize = 50)
        {
            try
            {
                _specificLogger.LogDebug("Getting paged submissions by status {StatusId}, page {PageNumber}, size {PageSize}",
                    status, pageNumber, pageSize);

                var allSubmissions = await _unitOfWork.ClaimsSubmissions.GetByStatusAsync(status);
                var totalCount = allSubmissions.Count();

                var pagedSubmissions = allSubmissions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new PaginatedResult<ClaimsSubmission>
                {
                    Items = pagedSubmissions,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                _specificLogger.LogInformation("Retrieved {Count} submissions for status {StatusId} (page {PageNumber})",
                    pagedSubmissions.Count, status, pageNumber);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting paged submissions by status {StatusId}", status);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetByCorrespondentAsync(Guid correspondentId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                _specificLogger.LogDebug("Getting submissions by correspondent {CorrespondentId} from {FromDate} to {ToDate}",
                    correspondentId, fromDate, toDate);

                var submissions = await _unitOfWork.ClaimsSubmissions.GetByCorrespondentAsync(correspondentId);

                if (fromDate.HasValue || toDate.HasValue)
                {
                    submissions = submissions.Where(s =>
                        (!fromDate.HasValue || s.UploadDate >= fromDate.Value) &&
                        (!toDate.HasValue || s.UploadDate <= toDate.Value));
                }

                var result = submissions.ToList();
                _specificLogger.LogInformation("Found {Count} submissions for correspondent {CorrespondentId}",
                    result.Count, correspondentId);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting submissions by correspondent {CorrespondentId}", correspondentId);
                throw;
            }
        }

        
        public async Task<ClaimsSubmission?> GetSubmissionWithErrorsAsync(Guid id)
        {
            try
            {
                _specificLogger.LogDebug("Getting complete submission data for {Id}", id);
                return await _unitOfWork.ClaimsSubmissions.GetCompleteAsync(id);
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting complete submission {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<ClaimsSubmission>> GetPendingResponseAsync(int olderThanHours)
        {
            try
            {
                _specificLogger.LogDebug("Getting pending response submissions older than {Hours} hours", olderThanHours);

                var submissions = await _unitOfWork.ClaimsSubmissions.GetPendingResponseAsync(olderThanHours);

                var result = submissions.ToList();
                _specificLogger.LogInformation("Found {Count} submissions pending response", result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting pending response submissions");
                throw;
            }
        }

        public async Task<ClaimsSubmission> GetByProtocolAsync(string protocol)
        {
            try
            {
                _specificLogger.LogDebug("Getting submissions by protocol {Protocol}", protocol);
                return await _unitOfWork.ClaimsSubmissions.GetByProtocolAsync(protocol);
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error getting submissions by protocol {Protocol}", protocol);
                throw;
            }
        }



        public override async Task<ValidationResult> ValidateAsync(ClaimsSubmission entity)
        {
            var errors = new List<string>();

            // Base validation
            var baseValidation = await base.ValidateAsync(entity);
            if (!baseValidation.IsValid)
            {
                errors.AddRange(baseValidation.Errors);
            }

            // Business-specific validation
            if (string.IsNullOrWhiteSpace(entity.InputFileName))
            {
                errors.Add("Input file name is required");
            }

            if (string.IsNullOrWhiteSpace(entity.InputFileFullPath))
            {
                errors.Add("Input file path is required");
            }

            if (entity.UploadDate == default)
            {
                errors.Add("Upload date is required");
            }

            if (entity.UploadDate > DateTime.UtcNow.AddMinutes(5)) // Allow 5 minutes tolerance for clock skew
            {
                errors.Add("Upload date cannot be in the future");
            }

            if (entity.SubmissionStatusId <= 0)
            {
                errors.Add("Valid submission status is required");
            }

            // Validate protocol if provided
            if (!string.IsNullOrEmpty(entity.Protocol) && entity.Protocol.Length > 20)
            {
                errors.Add("Protocol cannot exceed 20 characters");
            }

            return errors.Any() ? ValidationResult.Failure(errors.ToArray()) : ValidationResult.Success();
        }


        public async Task<ClaimsSubmission> UpdateStatusAsync(Guid submissionId, SubmissionStatusType status)
        {
            try
            {
                _specificLogger.LogInformation("Updating status for submission {Id} to {StatusId}", submissionId, status.GetId());

                var submission = await _unitOfWork.ClaimsSubmissions.GetByIdAsync(submissionId);
                if (submission == null)
                {
                    _specificLogger.LogWarning("Submission {Id} not found for status update", submissionId);
                    throw new EntityNotFoundException($"Submission {submissionId} not found");
                }

                submission.SubmissionStatusId = status.GetId();
                var result = await UpdateAsync(submission);

                _specificLogger.LogInformation("Successfully updated status for submission {Id} to {StatusId}", submissionId, status.GetId());
                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error updating status for submission {Id}", submissionId);
                throw;
            }
        }

        public async Task<ClaimsSubmission> MarkAsSentAsync(Guid submissionId, string protocol)
        {
            try
            {
                _specificLogger.LogInformation("Marking submission {Id} as sent with protocol {Protocol}", submissionId, protocol);

                var submission = await _unitOfWork.ClaimsSubmissions.GetByIdAsync(submissionId);
                if (submission == null)
                {
                    throw new EntityNotFoundException($"Submission {submissionId} not found");
                }

                // Business validation: can only mark as sent if in appropriate status
                if (submission.SendDate.HasValue)
                {
                    _specificLogger.LogWarning("Submission {Id} already marked as sent", submissionId);
                    throw new ValidationException("Submission already marked as sent");
                }

                submission.Protocol = protocol;
                submission.SendDate = DateTime.UtcNow;
                submission.SubmissionStatusId = SubmissionStatusType.Sent.GetId();

                var result = await UpdateAsync(submission);
                _specificLogger.LogInformation("Successfully marked submission {Id} as sent with protocol {Protocol}", submissionId, protocol);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error marking submission {Id} as sent", submissionId);
                throw;
            }
        }

        public async Task<ClaimsSubmission> CreateResponseAsync(Guid submissionId, string? outputFileName = null, string? outputFileFullPath = null)
        {
            try
            {
                _specificLogger.LogInformation("Recording response for submission {Id}", submissionId);

                var submission = await _unitOfWork.ClaimsSubmissions.GetByIdAsync(submissionId);
                if (submission == null)
                {
                    throw new EntityNotFoundException($"Submission {submissionId} not found");
                }

                // Business validation: can only record response if sent
                if (!submission.SendDate.HasValue)
                {
                    _specificLogger.LogWarning("Cannot record response for submission {Id} that hasn't been sent", submissionId);
                    throw new ValidationException("Cannot record response for submission that hasn't been sent");
                }

                submission.ResponseDate = DateTime.UtcNow;
                submission.LastResponseAttemptDate = DateTime.UtcNow;
                submission.OutputFileName = outputFileName;
                submission.OutputFileFullPath = outputFileFullPath;
                submission.SubmissionStatusId = SubmissionStatusType.Completed.GetId();

                var result = await UpdateAsync(submission);
                _specificLogger.LogInformation("Successfully recorded response for submission {Id}", submissionId);

                return result;
            }
            catch (Exception ex)
            {
                _specificLogger.LogError(ex, "Error recording response for submission {Id}", submissionId);
                throw;
            }
        }
    }
}