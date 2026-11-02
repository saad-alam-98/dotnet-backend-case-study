using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Banner;
using SaudiPlus.Response.Banner;
using Microsoft.Extensions.Options;
using SaudiPlus.Interfaces.Service;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Banner
{
    public class BannerService : BaseService<BannerService>, IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IFileService _fileService;
        private readonly BucketSetting _bucket;

        public BannerService(
            IMapper mapper,
            ILogger<BannerService> logger,
            ILanguageService languageService,
            IBannerRepository bannerRepository,
            IFileService fileService,
            IOptions<BucketSetting> bucket
        ) : base(mapper, logger, languageService)
        {
            _bannerRepository = bannerRepository;
            _fileService = fileService;
            _bucket = bucket.Value;
        }

        public async Task<GetBannerResponse> CreateBanner(CreateBannerDTO model)
        {
            var validationResponse = ValidateBannerAssociation(model.BusinessId, model.CategoryId);
            if (validationResponse != null)
                return validationResponse;

            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            var uploadResponse = await _fileService.UploadFileAsync(
                model.banner, 
                _bucket.Banner + "/" + model.Title
            );

            if (uploadResponse.error != null)
            {
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: uploadResponse.error.Message,
                    payload: null
                );
            }

            try
            {
                var banner = _mapper.Map<Entities.Banner>(model);
                banner.ImagePath = uploadResponse.URL;
                await _bannerRepository.InsertAsync(banner, true);
                var entry = await _bannerRepository.GetBannerWithRelations(banner.Id);
                var payload = _mapper.Map<BannerPayloadItem>(entry);

                payload.SetCityNameLocalized(entry!.GetCityNameLocalized(languageCode));
                payload.SetBusinessNameLocalized(entry!.GetBusinessNameLocalized(languageCode));
                payload.SetCategoryNameLocalized(entry!.GetCategoryNameLocalized(languageCode));

                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Banner Created",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating banners");
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBannerResponse> EditBanner(EditBannerDTO model)
        {
            var validationResponse = ValidateBannerAssociation(model.BusinessId, model.CategoryId);
            if (validationResponse != null)
                return validationResponse;

            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            var existingBanner = await _bannerRepository.GetBannerWithRelations(model.Id);
            if (existingBanner == null)
            {
                return new GetBannerResponse(
                    statusCode: StatusCodes.Status404NotFound,
                    message: $"Banner with id {model.Id} not found",
                    payload: null
                );
            }

            (string URL, Exception? error) uploadResponse = ("", null);
            if (model.image != null)
            {
                uploadResponse = await _fileService.UploadFileAsync(model.image, _bucket.Banner + "/" + model.Title);

                if (uploadResponse.error != null)
                {
                    return new GetBannerResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: uploadResponse.error.Message,
                        payload: null
                    );
                }
            }
            try
            {
                if (model.BusinessId.HasValue)
                {
                    existingBanner.CategoryId = null;
                    existingBanner.BusinessId = model.BusinessId;
                }
                else if (model.CategoryId.HasValue)
                {
                    existingBanner.BusinessId = null;
                    existingBanner.CategoryId = model.CategoryId;
                }

                if (!string.IsNullOrEmpty(uploadResponse.URL))
                {
                    existingBanner.ImagePath = uploadResponse.URL;
                }

                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingBanner.IsActive;
                _mapper.Map(model, existingBanner);

                await _bannerRepository.UpdateAsync(existingBanner, true);

                var banner = await _bannerRepository.GetBannerWithRelations(existingBanner.Id);
                var payload = _mapper.Map<BannerPayloadItem>(banner);

                payload.SetCityNameLocalized(banner!.GetCityNameLocalized(languageCode));
                payload.SetBusinessNameLocalized(banner!.GetBusinessNameLocalized(languageCode));
                payload.SetCategoryNameLocalized(banner!.GetCategoryNameLocalized(languageCode));

                return new GetBannerResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Banner Updated",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing business offers");
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: ex.Message,
                    payload: null
                );
            }
        }

        public async Task<GetBannersResponse> GetAllBanner()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBannersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var payload = await GetBannersAsync(languageCode);
                return new GetBannersResponse
                (
                    StatusCodes.Status200OK,
                    "Banners",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting banner");
                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBannersResponse> GetActiveBanner()
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBannersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var payload = await GetBannersAsync(languageCode, r => r.IsActive);
                return new GetBannersResponse
                (
                    StatusCodes.Status200OK,
                    "Active Banners",
                    payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting active banner");
                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBannerResponse> GetBannerById(int bannerId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBannerResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var response = await _bannerRepository.GetBannerWithRelations(bannerId);

                if (response == null)
                {
                    return new GetBannerResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Banner with Id {bannerId} not found",
                        payload: null
                    );
                }

                var payload = _mapper.Map<BannerPayloadItem>(response);
                payload.SetCityNameLocalized(response!.GetCityNameLocalized(languageCode));
                payload.SetBusinessNameLocalized(response!.GetBusinessNameLocalized(languageCode));
                payload.SetCategoryNameLocalized(response!.GetCategoryNameLocalized(languageCode));

                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Banners",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting banner with id {bannerId}");
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleBannerStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _bannerRepository.GetBannerWithRelations(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Banner with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _bannerRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Banners",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating banner status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> DeleteBanner(int bannerId)
        {
            try
            {
                var banner = await _bannerRepository.GetByIdAsync(bannerId);
                if (banner == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status404NotFound,
                       message: $"Banner with Id {bannerId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!string.IsNullOrEmpty(banner.ImagePath))
                {
                    await _fileService.DeleteFileAsync(banner.ImagePath);
                }

                await _bannerRepository.DeleteAsync(bannerId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Banner Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting banner");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        private GetBannerResponse? ValidateBannerAssociation(Guid? businessId, int? categoryId)
        {
            if (businessId.HasValue && categoryId.HasValue)
            {
                return new GetBannerResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: "A banner can only be linked to a single business or category",
                    payload: null
                );
            }
            return null;
        }

        private async Task<GetBannerPayload> GetBannersAsync(
            Language languageCode,
            Func<Entities.Banner, bool>? filter = null
        )
        {
            var banners = await _bannerRepository.GetBannersWithRelations();
            var filteredBanners = filter != null ? banners.Where(filter) : banners;

            return new GetBannerPayload
            {
                Items = filteredBanners.Select(r =>
                {
                    var banner = _mapper.Map<BannerPayloadItem>(r);
                    banner.SetCityNameLocalized(r!.GetCityNameLocalized(languageCode));
                    banner.SetBusinessNameLocalized(r!.GetBusinessNameLocalized(languageCode));
                    banner.SetCategoryNameLocalized(r!.GetCategoryNameLocalized(languageCode));
                    return banner;
                
                }).ToList(),
                TotalItems = filteredBanners.Count(),
                HasNextPage = false
            };
        }

        public async Task<GetBannersResponse> GetBannerByFilter(EntityFilter filter)
        {
            try
            {
                var (languageCode, languageError) = _languageService.GetLanguageCode();
                if (languageError != null)
                {
                    return new GetBannersResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: languageError.Message,
                        payload: null
                    );
                }

                var query = _bannerRepository.GetBannersForFilter();
                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(banner => banner.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(banner =>
                        banner.Title.ToLower().Contains(searchKeyword));
                }

                var totalItems = await query.CountAsync();
                var businesses = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();

                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetBannerPayload
                {
                    Items = businesses.Select(b => {
                        var item = _mapper.Map<BannerPayloadItem>(b);
                        item.SetCityNameLocalized(b.GetCityNameLocalized(languageCode));
                        item.SetCategoryNameLocalized(b.GetCategoryNameLocalized(languageCode));
                        item.SetBusinessNameLocalized(b.GetBusinessNameLocalized(languageCode));
                        return item;
                    }).ToList(),
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };

                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "banners Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching banners");
                return new GetBannersResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }
    }
}
