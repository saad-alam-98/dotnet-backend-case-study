using AutoMapper;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using Microsoft.AspNetCore.Mvc;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;
using SaudiPlus.Request.BusinessRequest;
using SaudiPlus.Response.BusinessRequest;


namespace SaudiPlus.Service.BusinessRequest
{
    public class BusinessRequestService : BaseService<BusinessRequestService>, IBusinessRequestService
    {
        private readonly IBusinessRequestRepository _businessRequestRepository;
        public BusinessRequestService(
            IMapper mapper,
            ILogger<BusinessRequestService> logger,
            ILanguageService languageService,
            IBusinessRequestRepository businessRequestRepository
        ) : base(mapper, logger, languageService)
        {
            _businessRequestRepository = businessRequestRepository;
        }

        public async Task<ActionResult<SuccessResponse>> CreateBusinessRequest(CreateBusinessRequestDTO model)
        {
            try
            {
                var existingRequest = await _businessRequestRepository.GetByBusinessName(model.BusinessName);
                if (existingRequest != null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "A request for this business name already exists.",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var request = _mapper.Map<Entities.BusinessRequest>(model);
                request.IsActive = true;

                await _businessRequestRepository.InsertAsync(request, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Request Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating business request");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<ActionResult<SuccessResponse>> DeleteBusinessRequest(int id)
        {
            try
            {
                var existingRequest = await _businessRequestRepository.GetByIdAsync(id);
                if (existingRequest == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Request does not exist",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                await _businessRequestRepository.DeleteAsync(id, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting request");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<ActionResult<BusinessRequestResponse>> GetBusinessRequestById(int id)
        {
            try
            {
                var existingRequest = await _businessRequestRepository.GetRequestWithRelation(id);
                if (existingRequest == null)
                {
                    return new BusinessRequestResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Request with id {id} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<BusinessRequestPayload>(existingRequest);

                return new BusinessRequestResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting request");
                return new BusinessRequestResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<ActionResult<BusinessListRequestResponse>> GetBusinessRequestList(EntityFilter filter)
        {
            try
            {
                var query = _businessRequestRepository.GetRequestForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(request => request.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(name =>
                        name.FirstName.ToLower().Contains(searchKeyword) ||
                        name.LastName.ToLower().Contains(searchKeyword) ||
                        name.BusinessName.ToLower().Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var request = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;
                var payload = new BusinessListPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var r in request)
                {
                    var item = _mapper.Map<BusinessRequestPayload>(r);
                    payload.Items.Add(item);
                }

                return new BusinessListRequestResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching request");
                return new BusinessListRequestResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<ActionResult<SuccessResponse>> ToggleBusinessRequestStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _businessRequestRepository.GetByIdAsync(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Request with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _businessRequestRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating request status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }
    }
}
