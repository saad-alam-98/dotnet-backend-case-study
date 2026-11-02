using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Entities;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Request.Policy;
using SaudiPlus.Response.Policy;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Request.BusinessUser;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Policy
{
    public class PolicyService : BaseService<PolicyService>, IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;
        private readonly IContactUsRepository _contactUsRepository;
        private readonly IBusinessPolicyRepository _businessPolicyRepository;

        public PolicyService(
            ILogger<PolicyService> logger,
            ILanguageService languageService,
            IPolicyRepository policyRepository,
            IContactUsRepository contactUsRepository,
            IBusinessPolicyRepository businessPolicyRepository,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _policyRepository = policyRepository;
            _contactUsRepository = contactUsRepository;
            _businessPolicyRepository = businessPolicyRepository;
        }

        public async Task<GetPolicyResponse> SetPolicy(SetPolicyDTO model)
        {
            try
            {
                var normalizedType = model.Type.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse<PolicyType>(normalizedType, true, out var policyType))
                {

                }
                else
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Policy Type",
                        payload: null
                    );
                }

                var currentPolicy = await _policyRepository.GetPolicyByType(policyType);
                if (currentPolicy == null)
                {
                    currentPolicy = _mapper.Map<Entities.Policy>(model);
                    currentPolicy.Type = policyType;
                    currentPolicy.IsActive = true;
                    await _policyRepository.InsertAsync(currentPolicy, true);
                }
                else
                {
                    currentPolicy = _mapper.Map(model, currentPolicy);
                    await _policyRepository.UpdateAsync(currentPolicy, true);
                }

                var payload = _mapper.Map<PolicyDetailed>(currentPolicy);

                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Policy Set Success",
                    payload: payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting policy");
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPolicyResponse> GetPolicy(string type)
        {
            try
            {
                var normalizedType = type.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse<PolicyType>(normalizedType, true, out var policyType))
                {

                }
                else
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Policy Type",
                        payload: null
                    );
                }

                var currentPolicy = await _policyRepository.GetPolicyByType(policyType);
                if (currentPolicy == null)
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Policy with type {type} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<PolicyDetailed>(currentPolicy);

                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Policy Get Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting policy");
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPolicyResponse> GetPolicyLocalized(string type)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var normalizedType = type.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse<PolicyType>(normalizedType, true, out var policyType))
                {

                }
                else
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Policy Type",
                        payload: null
                    );
                }

                var currentPolicy = await _policyRepository.GetPolicyByType(policyType);
                if (currentPolicy == null)
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Policy with type {type} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<PolicyLocalized>(currentPolicy);
                payload.SetPolicyContentLocalized(currentPolicy.GetContentByLanguageCode(languageCode));

                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Policy Get Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting policy");
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        
        public async Task<GetPolicyResponse> SetBusinessPolicy(Guid businessId, SetPolicyDTO model)
        {
            try
            {
                var normalizedType = model.Type.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse<PolicyType>(normalizedType, true, out var policyType))
                {
                    if (policyType == PolicyType.AboutApp)
                    {
                        return new GetPolicyResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Invalid Policy Type",
                            payload: null
                        );
                    }
                }
                else
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Policy Type",
                        payload: null
                    );
                }

                var currentPolicy = await _businessPolicyRepository.GetBusinessPolicyByType(businessId, policyType);
                if (currentPolicy == null)
                {
                    currentPolicy = _mapper.Map<BusinessPolicy>(model);
                    currentPolicy.Type = policyType;
                    currentPolicy.IsActive = true;
                    currentPolicy.BusinessId = businessId;
                    await _businessPolicyRepository.InsertAsync(currentPolicy, true);
                }
                else
                {
                    currentPolicy = _mapper.Map(model, currentPolicy);
                    await _businessPolicyRepository.UpdateAsync(currentPolicy, true);
                }

                var payload = _mapper.Map<PolicyDetailed>(currentPolicy);

                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Business Policy Set Success",
                    payload: payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting policy");
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPolicyResponse> GetBusinessPolicy(Guid businessId, string type)
        {
            try
            {
                var normalizedType = type.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse<PolicyType>(normalizedType, true, out var policyType))
                {
                    if (policyType == PolicyType.AboutApp)
                    {
                        return new GetPolicyResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Invalid Policy Type",
                            payload: null
                        );
                    }
                }
                else
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Policy Type",
                        payload: null
                    );
                }

                var currentPolicy = await _businessPolicyRepository.GetBusinessPolicyByType(businessId, policyType);
                if (currentPolicy == null)
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Policy with type {type} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<PolicyDetailed>(currentPolicy);

                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Policy Get Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting policy");
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPolicyResponse> GetBusinessPolicyLocalized(Guid businessId, string type)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var normalizedType = type.Replace(" ", "", StringComparison.OrdinalIgnoreCase);
                if (Enum.TryParse<PolicyType>(normalizedType, true, out var policyType))
                {
                    if (policyType == PolicyType.AboutApp)
                    {
                        return new GetPolicyResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: "Invalid Policy Type",
                            payload: null
                        );
                    }
                }
                else
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: "Invalid Policy Type",
                        payload: null
                    );
                }

                var currentPolicy = await _businessPolicyRepository.GetBusinessPolicyByType(businessId, policyType);
                if (currentPolicy == null)
                {
                    return new GetPolicyResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Policy with type {type} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<PolicyLocalized>(currentPolicy);
                payload.SetPolicyContentLocalized(currentPolicy.GetContentByLanguageCode(languageCode));

                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Policy Get Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting policy");
                return new GetPolicyResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        
        public async Task<SuccessResponse> SetSocialMedia(SetSocialMediaDto model)
        {
            try
            {
                if (model.Type == null)
                {
                    return new SuccessResponse(
                        StatusCodes.Status404NotFound,
                        "Type cannot be null",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var contact = await _contactUsRepository.GetByIdAsync(1);

                if (model.Type.ToLower() == "Facebook".ToLower())
                {
                    contact.FacebookUrl = model.Url;
                    contact.FacebookPage = model.Page;
                }
                else if (model.Type.ToLower() == "Instagram".ToLower())
                {
                    contact.InstagramUrl = model.Url;
                    contact.InstagramPage = model.Page;
                }
                else if (model.Type.ToLower() == "Snapchat".ToLower())
                {
                    contact.SnapchatUrl = model.Url;
                    contact.SnapchatPage = model.Page;
                }
                else if (model.Type.ToLower() == "Twitter".ToLower())
                {
                    contact.TwitterUrl = model.Url;
                    contact.TwitterPage = model.Page;
                }
                else
                {
                    return new SuccessResponse(
                       StatusCodes.Status404NotFound,
                       "Invalid Type",
                       payload: new SuccessResponsePayload { success = false }
                   );
                }

                await _contactUsRepository.UpdateAsync(contact, true);

                return new SuccessResponse(
                    StatusCodes.Status200OK,
                    $"{model.Type} Update Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating social media");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> SetContactUs(SetContactDTO model)
        {
            try
            {
                var currentPolicy = await _contactUsRepository.GetAsync();
                if (currentPolicy == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Error fetching contact us details",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var selectedItem = currentPolicy.LastOrDefault();
                if (selectedItem == null)
                {
                    selectedItem = _mapper.Map<ContactUs>(model);
                    await _contactUsRepository.InsertAsync(selectedItem, true);
                }
                else
                {
                    selectedItem = _mapper.Map(model, selectedItem);
                    await _contactUsRepository.UpdateAsync(selectedItem, true);
                }

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Contact Us Success",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting contact us details");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetContactUsResponse> GetContactUs()
        {
            try
            {
                var currentPolicy = await _contactUsRepository.GetAsync();
                if (currentPolicy == null)
                {
                    return new GetContactUsResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Error fetching contact us details",
                        payload: null
                    );
                }

                var selectedItem = currentPolicy.LastOrDefault();
                var payload = _mapper.Map<ContactUsPayload>(selectedItem);

                return new GetContactUsResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Policy Get Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting contact us details");
                return new GetContactUsResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}
