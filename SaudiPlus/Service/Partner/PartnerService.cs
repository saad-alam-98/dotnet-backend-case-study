using AutoMapper;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Request.Partner;
using SaudiPlus.Response.Partner;
using Microsoft.Extensions.Options;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Partner
{
    public class PartnerService : BaseService<PartnerService>, IPartnerService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IPartnerRepository _partnerRepository;
        private readonly IFileService _fileService;
        private readonly BucketSetting _bucket;

        public PartnerService(
            IMapper mapper,
            IFileService fileService,
            IOptions<BucketSetting> bucket,
            ILogger<PartnerService> logger,
            ILanguageService languageService,
            IPartnerRepository partnerRepository,
            IBusinessRepository businessRepository
        ) : base(mapper, logger, languageService)
        {
            _bucket = bucket.Value;
            _fileService = fileService;
            _partnerRepository = partnerRepository;
            _businessRepository = businessRepository;
        }

        public async Task<SuccessResponse> CreateBusinessPartner(Guid businessId)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                var existingBusiness = await _businessRepository.GetByIdAsync(businessId);
                if (existingBusiness == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Business with id {businessId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var isAlreadyPartner = await _partnerRepository.GetBusinessPartner(businessId);
                if (isAlreadyPartner != null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"This business already exist as a partner",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var partner = _mapper.Map<Entities.Partner>(existingBusiness);
                partner.BusinessUserId = businessId;
                partner.LogoPath = existingBusiness!.LogoPath;

                await _partnerRepository.InsertAsync(partner, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Partner Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating business partner");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> CreatePartner(CreatePartnerDTO model)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }


            var uploadResponse = await _fileService.UploadFileAsync(
                model.Logo,
                _bucket.Partner + "/" + model.NameEn +"/"+ DateTime.UtcNow.TimeOfDay
            );

            if (uploadResponse.error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: uploadResponse.error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                var partner = _mapper.Map<Entities.Partner>(model);
                partner.IsActive = true;
                partner.LogoPath = uploadResponse.URL;

                await _partnerRepository.InsertAsync(partner, true);


                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Partner Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating partner");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> DeletePartner(int id)
        {
            try
            {
                var existingPartner = await _partnerRepository.GetByIdAsync(id);
                if (existingPartner == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Parner does not exist",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!string.IsNullOrEmpty(existingPartner.LogoPath))
                {
                    await _fileService.DeleteFileAsync(existingPartner.LogoPath);
                }

                await _partnerRepository.DeleteAsync(id, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Successfully deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting partner");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> EditPartner(EditPartnerDTO model)
        {
            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            var existingPartner = await _partnerRepository.GetPartnerWithRelation(model.Id);
            if (existingPartner == null)
            {
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: $"Partner with id {model.Id} not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                (string URL, Exception? error) uploadResponse = ("", null);
                if (model.Logo != null)
                {
                    if (!string.IsNullOrEmpty(existingPartner.LogoPath))
                    {
                        var deleteResponse = await _fileService.DeleteFileAsync(existingPartner.LogoPath);

                        if (deleteResponse.error != null)
                        {
                            return new SuccessResponse
                            (
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: deleteResponse.error.Message,
                                payload: new SuccessResponsePayload { success = false }
                            );
                        }
                    }

                    uploadResponse = await _fileService.UploadFileAsync(
                        model.Logo,
                        _bucket.Partner + "/" + model.NameEn + "/" + DateTime.UtcNow.TimeOfDay
                    );


                    if (uploadResponse.error != null)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status400BadRequest,
                            message: uploadResponse.error.Message,
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }

                    existingPartner.LogoPath = uploadResponse.URL;
                }
                
                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingPartner.IsActive;
                model.CategoryId = model.CategoryId.HasValue ? model.CategoryId.Value : existingPartner.CategoryId;
                _mapper.Map(model, existingPartner);

                await _partnerRepository.UpdateAsync(existingPartner, true);

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Partner Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing partners");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                   message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetPartnersResponse> GetActivePartner()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetPartnersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var partners = await _partnerRepository.GetPartnersWithRelation();
                var payload = new GetPartnersPayload
                {
                    Items = [],
                    TotalItems = partners.Count,
                    HasNextPage = false,
                };

                foreach (var partner in partners)
                {
                    var p = _mapper.Map<PartnerInfoLocalizedPayload>(partner);
                    p.Name = partner.GetNameByLanguageCode(languageCode);
                    p.SetCategoryNameLocalized(partner.GetCategoryNameLocalized(languageCode));
                    payload.Items.Add(p);
                }

                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Active Partners",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting partner");
                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBusinessForPartner> GetBusinessForPartner()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBusinessForPartner
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var business = (await _businessRepository.GetBusinessWithPartner()).Where(b => b.Partner == null);
                var payload = new GetBusinessForPartnerPayload()
                {
                    items = []
                };
                foreach (var item in business)
                {
                    var name = item.GetNameByLanguageCode(languageCode);
                    var b = new GetBusinessForPartnerItem
                    {
                        Id = item.Id,
                        Name = name
                    };
                    payload.items.Add(b);
                }

                return new GetBusinessForPartner
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Business For Partners",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business");
                return new GetBusinessForPartner
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPartnersResponse> GetPartnersLocalized(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetPartnersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _partnerRepository.GetPartnersForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(partner => partner.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(name =>
                        name.NameEn.ToLower().Contains(searchKeyword) ||
                        name.NameAr.ToLower().Contains(searchKeyword) ||
                        name.CompanyName.ToLower().Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var partners = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;
                var payload = new GetPartnersPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var partner in partners)
                {
                    var item = _mapper.Map<PartnerInfoLocalizedPayload>(partner);
                    item.Name = partner.GetNameByLanguageCode(languageCode);
                    item.SetCategoryNameLocalized(partner.GetCategoryNameLocalized(languageCode));
                    payload.Items.Add(item);
                }

                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Partners Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching partners");
                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPartnersResponse> GetPartnersDetailed(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetPartnersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _partnerRepository.GetPartnersForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(partner => partner.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(name =>
                        name.NameEn.ToLower().Contains(searchKeyword) ||
                        name.NameAr.ToLower().Contains(searchKeyword) ||
                        name.CompanyName.ToLower().Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var partners = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;
                var payload = new GetPartnersPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                foreach (var partner in partners)
                {
                    var item = _mapper.Map<PartnerInfoDetailPayload>(partner);
                    item.SetCategoryNameLocalized(partner.GetCategoryNameLocalized(languageCode));
                    payload.Items.Add(item);
                }

                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Partners Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching partners");
                return new GetPartnersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetPartnerResponse> GetPartnerById(int id)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetPartnerResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var existingPartner = await _partnerRepository.GetPartnerWithRelation(id);
                if (existingPartner == null)
                {
                    return new GetPartnerResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Partner with id {id} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<PartnerInfoDetailPayload>(existingPartner);
                payload.SetCategoryNameLocalized(existingPartner.GetCategoryNameLocalized(languageCode));

                return new GetPartnerResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: $"Partner",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting business");
                return new GetPartnerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> TogglePartnerStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _partnerRepository.GetPartnerWithRelation(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Partner with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _partnerRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Partner Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating partner status");
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
