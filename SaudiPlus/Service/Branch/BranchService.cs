using QRCoder;
using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Request;
using SaudiPlus.Entities;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Helper.Bucket;
using SaudiPlus.Helper.Filter;
using SaudiPlus.Request.Branch;
using SaudiPlus.Response.Branch;
using SaudiPlus.Entities.Identity;
using SaudiPlus.Interfaces.Service;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Branch
{
    public class BranchService : BaseService<BranchService>, IBranchService
    {
        private readonly BucketSetting _bucket;
        private readonly IMapService _mapService;
        private readonly IFileService _fileService;
        private readonly SaudiPlusDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly IBranchRepository _BranchRepository;
        private readonly IBusinessRepository _businessUserRepository;

        public BranchService(
            IMapper mapper,
            IMapService mapService,
            IFileService fileService,
            SaudiPlusDbContext dbContext,
            UserManager<User> userManager,
            ILogger<BranchService> logger,
            IOptions<BucketSetting> bucket,
            ILanguageService languageService,
            IBranchRepository BranchRepository,
            IBusinessRepository businessUserRepository
        ) : base(mapper, logger, languageService)
        {
            _bucket = bucket.Value;
            _dbContext = dbContext;
            _mapService = mapService;
            _userManager = userManager;
            _fileService = fileService;
            _BranchRepository = BranchRepository;
            _businessUserRepository = businessUserRepository;
        }

        public async Task<GetBranchResponse> CreateBranch(Guid BusinessId, ICreateBranchDTO model)
        {
            var existingBusiness = await _businessUserRepository.GetBusinessWithRelation(BusinessId);
            if (existingBusiness == null)
            {
                return new GetBranchResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: $"Business with Id {BusinessId} not found",
                    payload: null
                );
            }

            var existingBranches = await _BranchRepository.GetBusinessBranches(BusinessId);
            if (existingBranches.Any(b => b.BranchNumber == model.BranchNumber))
            {
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: $"Branch number {model.BranchNumber} already exist",
                    payload: null
                );
            }

            if (existingBranches.Any(b => b.Name == model.Name))
            {
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: $"Branch name {model.Name} already exist",
                    payload: null
                );
            }

            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            var executionStrategy = _dbContext.Database.CreateExecutionStrategy();
            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var Branch = _mapper.Map<Entities.Branch>(model);
                    Branch.BusinessId = existingBusiness.Id;
                    Branch.IsActive = true;

                    await _BranchRepository.InsertAsync(Branch, true);

                    var qrText = $"branchId:{Branch.Id}";
                    using var qrGenerator = new QRCodeGenerator();
                    using var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                    using var qrCode = new PngByteQRCode(qrCodeData);
                    var qrCodeImage = qrCode.GetGraphic(20);
                    var qrFormFile = _fileService.CreateFormFileFromBytes(
                        qrCodeImage,
                        "QRCode.png",
                        "image/png"
                    );
                    var qRUploadResponse = await _fileService.UploadFileAsync(
                        qrFormFile,
                        $"{_bucket.QR}/{existingBusiness.NameEn}/{Branch.Name}"
                    );
                    if (qRUploadResponse.error != null)
                    {
                        await transaction.RollbackAsync();
                        return new GetBranchResponse(
                            statusCode: StatusCodes.Status400BadRequest,
                            message: qRUploadResponse.error.Message,
                            payload: null
                        );
                    }
                    var (Rating, Review) = await _mapService.GetPlaceRating(Branch.PlaceId);
                    Branch.Rating = Rating;
                    Branch.ReviewCount = Review;
                    Branch.QrCodePath = qRUploadResponse.URL;
                    await _BranchRepository.UpdateAsync(Branch, true);

                    var entry = await _BranchRepository.GetBranchWithRelations(Branch.Id);
                    var payload = _mapper.Map<BranchPayloadItem>(entry);

                    payload.SetCityNameLocalized(entry!.GetCityNameLocalized(languageCode));
                    payload.SetBusinessNameLocalized(entry!.GetBusinessNameLocalized(languageCode));

                    await transaction.CommitAsync();
                    return new GetBranchResponse
                    (
                        statusCode: StatusCodes.Status200OK,
                        message: "Branch Created",
                        payload: payload
                    );
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "An error occurred while creating business branch");
                    return new GetBranchResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "An unexpected error occurred. Please try again later.",
                        payload: null
                    );
                }
            });
        }

        public async Task<GetBranchResponse> EditBranch(EditBranchDTO model)
        {
            var existingBranch = await _BranchRepository.GetBranchWithRelations(model.Id);
            if (existingBranch == null)
            {
                return new GetBranchResponse(
                    statusCode: StatusCodes.Status400BadRequest,
                    message: $"Branch with Id {model.Id} not found",
                    payload: null
                );
            }

            var (languageCode, error) = _languageService.GetLanguageCode();
            if (error != null)
            {
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status400BadRequest,
                    message: error.Message,
                    payload: null
                );
            }

            try
            {
                model.CityId = model.CityId ?? existingBranch.CityId;

                model.IsActive = model.IsActive.HasValue ? model.IsActive.Value : existingBranch.IsActive;
                _mapper.Map(model, existingBranch);

                await _BranchRepository.UpdateAsync(existingBranch, true);
                var payload = _mapper.Map<BranchPayloadItem>(existingBranch);

                payload.SetCityNameLocalized(existingBranch!.GetCityNameLocalized(languageCode));
                payload.SetBusinessNameLocalized(existingBranch!.GetBusinessNameLocalized(languageCode));

                return new GetBranchResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Branch Updated",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while editing business Branchs");
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBranchResponse> GetBranchById(int branchId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBranchResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var Branch = await _BranchRepository.GetBranchWithRelations(branchId);
                if (Branch == null)
                {
                    return new GetBranchResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Branch with Id {branchId} not found",
                        payload: null
                    );
                }
                var payload = _mapper.Map<BranchPayloadItem>(Branch);

                payload.SetCityNameLocalized(Branch!.GetCityNameLocalized(languageCode));
                payload.SetBusinessNameLocalized(Branch!.GetBusinessNameLocalized(languageCode));

                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while getting Branch with id {branchId}");
                return new GetBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> ToggleBranchStatus(ToggleStatusDTO<int> model)
        {
            try
            {
                var response = await _BranchRepository.GetBranchWithRelations(model.Id);
                if (response == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Branch with Id {model.Id} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                response.IsActive = model.IsActive;
                await _BranchRepository.UpdateAsync(response, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Branch Status Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating Branch status");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> DeleteBranch(int branchId)
        {
            try
            {
                var Branch = await _BranchRepository.GetByIdAsync(branchId);
                if (Branch == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Branch with Id {branchId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                await _BranchRepository.DeleteAsync(branchId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Branch Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting Branch");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetBranchesResponse> GetBusinessBranches(Guid businessId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBranchesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var payload = await GetBranchsAsync(businessId, languageCode);
                return new GetBranchesResponse
                (
                    StatusCodes.Status200OK,
                    "Business Branchs",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an business Branchs");
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetAllBranchResponse> GetAllBranches(EntityFilter filter)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetAllBranchResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }


                var query = _BranchRepository.GetAllBranches();

                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(business => business.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(branch =>
                        branch.Name.ToLower().Contains(searchKeyword) ||
                        branch.Address.ToLower().Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var branches = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();
                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetAllBranchesPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };
                foreach (var branch in branches)
                {
                    var item = _mapper.Map<BranchPayloadItem>(branch);
                    item.SetBusinessNameLocalized(branch.Business.GetNameByLanguageCode(languageCode));
                    item.Business.SetCategoryNameLocalized(branch.Business.Category.GetNameByLanguageCode(languageCode));
                    item.SetCityNameLocalized(branch.City.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }


                return new GetAllBranchResponse
                (
                    StatusCodes.Status200OK,
                    "Business Branchs",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an business Branchs");
                return new GetAllBranchResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBranchesResponse> GetBranch(Guid businessId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBranchesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var identityUser = await _userManager.FindByIdAsync(businessId.ToString());
                if (identityUser == null)
                {
                    return new GetBranchesResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Business user with Id {businessId} not found",
                        payload: null
                    );
                }

                var existingBusiness = await _businessUserRepository.GetBusinessWithRelation(identityUser.Email);
                if (existingBusiness == null)
                {
                    return new GetBranchesResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Business with Id {businessId} not found",
                        payload: null
                    );
                }

                var payload = await GetBranchsAsync(existingBusiness.Id, languageCode);
                return new GetBranchesResponse
                (
                    StatusCodes.Status200OK,
                    "Business Branchs",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an business Branchs");
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<GetBranchesResponse> GetBusinessActiveBranches(Guid businessId)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBranchesResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var payload = await GetBranchsAsync(businessId, languageCode, b => b.IsActive);
                return new GetBranchesResponse
                (
                    StatusCodes.Status200OK,
                    "Business Active Branchs",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating an business active Branchs");
                return new GetBranchesResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        private async Task<GetBranchesPayload> GetBranchsAsync(
            Guid businessId,
            Language languageCode,
            Func<Entities.Branch, bool>? filter = null
        )
        {
            var banners = await _BranchRepository.GetBusinessBranches(businessId);
            var filteredBanners = filter != null ? banners.Where(filter) : banners;

            return new GetBranchesPayload
            {
                items = filteredBanners.Select(r =>
                {
                    var banner = _mapper.Map<BranchPayloadItem>(r);
                    banner.SetCityNameLocalized(r!.GetCityNameLocalized(languageCode));
                    banner.SetBusinessNameLocalized(r!.GetBusinessNameLocalized(languageCode));
                    return banner;

                }).ToList()
            };
        }

        public async Task<GetBranchQRCodeResponse> GetBranchQR(int branchId)
        {
            try
            {
                var Branch = await _BranchRepository.GetBranchWithRelations(branchId);
                if (Branch == null)
                {
                    return new GetBranchQRCodeResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Branch with Id {branchId} not found",
                        payload: null
                    );
                }
                var payload = _mapper.Map<GetBranchQRCodePayload>(Branch);

                return new GetBranchQRCodeResponse
                (
                    StatusCodes.Status200OK,
                    "Business Active Branchs",
                    payload
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while getting branch QR");
                return new GetBranchQRCodeResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> SetBranchQR(Guid BusinessId, int branchId)
        {
            try
            {
                var existingBusiness = await _businessUserRepository.GetBusinessWithRelation(BusinessId);
                if (existingBusiness == null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: $"Business with Id {BusinessId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var existingBranch = await _BranchRepository.GetByIdAsync(branchId);
                if (existingBranch == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Branch with id {branchId} not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                if (!string.IsNullOrEmpty(existingBranch.QrCodePath))
                {
                    await _fileService.DeleteFileAsync(existingBranch.QrCodePath);
                }

                var qrText = $"branchId:{existingBranch.Id}";
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeImage = qrCode.GetGraphic(20);
                var qrFormFile = _fileService.CreateFormFileFromBytes(
                    qrCodeImage,
                    "QRCode.png",
                    "image/png"
                );
                var qRUploadResponse = await _fileService.UploadFileAsync(
                    qrFormFile,
                    $"{_bucket.QR}/{existingBusiness.NameEn}/{existingBranch.Name}"
                );
                if (qRUploadResponse.error != null)
                {
                    return new SuccessResponse(
                        statusCode: StatusCodes.Status400BadRequest,
                        message: qRUploadResponse.error.Message,
                        payload: new SuccessResponsePayload { success = false }
                    );
                }
                existingBranch.QrCodePath = qRUploadResponse.URL;
                await _BranchRepository.UpdateAsync(existingBranch, true);

                return new SuccessResponse
                (
                    StatusCodes.Status200OK,
                    "Business Active Branchs",
                    payload: new SuccessResponsePayload { success = true }
                );

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting branch QR");
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetBranchesPaginatedResponse> GetBranchByFilter(Guid businessId, EntityFilter filter)
        {
            try
            {
                var (languageCode, error) = _languageService.GetLanguageCode();
                if (error != null)
                {
                    return new GetBranchesPaginatedResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: error.Message,
                        payload: null
                    );
                }

                var query = _BranchRepository.GetBranchForFilter(businessId);

                if (filter.isActive.HasValue)
                {
                    query = query
                        .Where(business => business.IsActive == filter.isActive.Value);
                }

                if (!string.IsNullOrEmpty(filter.searchKeyword))
                {
                    var searchKeyword = filter.searchKeyword.ToLower();
                    query = query.Where(branch =>
                        branch.Name.ToLower().Contains(searchKeyword) ||
                        branch.Address.ToLower().Contains(searchKeyword)
                    );
                }

                var totalItems = await query.CountAsync();
                var branches = await query
                    .Skip((filter.pageNumber - 1) * filter.pageSize)
                    .Take(filter.pageSize)
                    .ToListAsync();
                bool hasNextPage = (filter.pageNumber * filter.pageSize) < totalItems;

                var payload = new GetBranchesPaginatedPayload
                {
                    Items = [],
                    TotalItems = totalItems,
                    HasNextPage = hasNextPage
                };
                foreach (var branch in branches)
                {
                    var item = _mapper.Map<BranchPayloadItem>(branch);
                    item.SetBusinessNameLocalized(branch.Business.GetNameByLanguageCode(languageCode));
                    item.Business.SetCategoryNameLocalized(branch.Business.Category.GetNameByLanguageCode(languageCode));
                    item.SetCityNameLocalized(branch.City.GetNameByLanguageCode(languageCode));
                    payload.Items.Add(item);
                }

                return new GetBranchesPaginatedResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Branch Fetch Success",
                    payload: payload
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching branches");
                return new GetBranchesPaginatedResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> SyncBranchRating()
        {
            try
            {
                var branches = await _BranchRepository.GetAsync();
                foreach(var branch in branches)
                {
                    if (string.IsNullOrEmpty(branch.PlaceId))
                        continue;

                    var (rating, review) = await _mapService.GetPlaceRating(branch.PlaceId);
                    branch.Rating = rating;
                    branch.ReviewCount = review;

                    await _BranchRepository.UpdateAsync(branch, true);
                }

                return new SuccessResponse
                (
                    StatusCodes.Status200OK,
                    "Branches Rating Synced",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while syncing branch rating");
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
