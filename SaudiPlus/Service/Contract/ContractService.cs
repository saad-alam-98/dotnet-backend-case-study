using AutoMapper;
using SaudiPlus.Enums;
using SaudiPlus.Response;
using SaudiPlus.Interfaces;
using SaudiPlus.Request.Contract;
using SaudiPlus.Response.Contract;
using SaudiPlus.Interfaces.Service;
using SaudiPlus.Interfaces.Repository;

namespace SaudiPlus.Service.Contract
{
    public class ContractService : BaseService<ContractService>, IContractService
    {
        private readonly IContractRepository _ContractRepository;
        private readonly IBusinessRepository _businessRepository;
        public ContractService(
            IContractRepository contractRepository,
            IBusinessRepository businessRepository,
            ILanguageService languageService,
            ILogger<ContractService> logger,
            IMapper mapper
        ) : base(mapper, logger, languageService)
        {
            _ContractRepository = contractRepository;
            _businessRepository = businessRepository;
        }

        public async Task<SuccessResponse> AddContract(CreateContractDTO model)
        {
            try
            {
                var business = await _businessRepository.GetByIdAsync(model.BusinessId);
                if (business == null)
                {
                    return new SuccessResponse
                    (
                        statusCode: StatusCodes.Status500InternalServerError,
                        message: "Business not found",
                        payload: new SuccessResponsePayload { success = false }
                    );
                }

                var contractType = (ContractType)model.ContractType;

                if (contractType == ContractType.MonthlySubscription)
                {
                    if (model.SubscriptionAmount == 0)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "Please enter valid subscription amount",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }
                }
                else if (contractType == ContractType.FixedAmountPerOffer)
                {
                    if (model.FixedAmountPerOffer == 0)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "Please enter valid offer amount",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }
                }
                else if (contractType == ContractType.PercentageOfInvoice)
                {
                    if (model.PercentageOfInvoice == 0)
                    {
                        return new SuccessResponse
                        (
                            statusCode: StatusCodes.Status500InternalServerError,
                            message: "Please enter valid invoice percentage",
                            payload: new SuccessResponsePayload { success = false }
                        );
                    }
                }

                var Contract = _mapper.Map<Entities.Contract>(model);
                Contract.ContractType = contractType;
                Contract.IsActive = true;
                await _ContractRepository.InsertAsync(Contract, true);

                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Contract Created",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<SuccessResponse> EditContract(EditContractDTO model)
        {
            var existingContract = await _ContractRepository.GetBusinessContract(model.BusinessId);
            if (existingContract == null)
            {
                return new SuccessResponse(
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "Contract not found",
                    payload: new SuccessResponsePayload { success = false }
                );
            }

            try
            {
                _mapper.Map(model, existingContract);

                if (model.ContractType.HasValue && model.ContractType != 0)
                {
                    var contractType = (ContractType)model.ContractType;
                    if (contractType == ContractType.MonthlySubscription)
                    {
                        if (model.SubscriptionAmount == 0)
                        {
                            return new SuccessResponse
                            (
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: "Please enter valid subscription amount",
                                payload: new SuccessResponsePayload { success = false }
                            );
                        }
                        existingContract.ContractType = contractType;
                        existingContract.SubscriptionAmount = model.SubscriptionAmount;
                        existingContract.FixedAmountPerOffer = 0;
                        existingContract.PercentageOfInvoice = 0;
                    }
                    else if (contractType == ContractType.FixedAmountPerOffer)
                    {
                        if (model.FixedAmountPerOffer == 0)
                        {
                            return new SuccessResponse
                            (
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: "Please enter valid offer amount",
                                payload: new SuccessResponsePayload { success = false }
                            );
                        }
                        existingContract.ContractType = contractType;
                        existingContract.SubscriptionAmount = 0;
                        existingContract.FixedAmountPerOffer = model.FixedAmountPerOffer;
                        existingContract.PercentageOfInvoice = 0;
                    }
                    else if (contractType == ContractType.PercentageOfInvoice)
                    {
                        if (model.PercentageOfInvoice == 0)
                        {
                            return new SuccessResponse
                            (
                                statusCode: StatusCodes.Status500InternalServerError,
                                message: "Please enter valid invoice percentage",
                                payload: new SuccessResponsePayload { success = false }
                            );
                        }
                        existingContract.ContractType = contractType;
                        existingContract.SubscriptionAmount = 0;
                        existingContract.FixedAmountPerOffer = 0;
                        existingContract.PercentageOfInvoice = model.PercentageOfInvoice;
                    }
                }

                await _ContractRepository.UpdateAsync(existingContract, true);

                return new SuccessResponse(
                    statusCode: StatusCodes.Status200OK,
                    message: "Contract Updated",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: new SuccessResponsePayload { success = false }
                );
            }
        }

        public async Task<GetContractResponse> GetContract(Guid businessId)
        {
            try
            {
                var response = await _ContractRepository.GetBusinessContract(businessId);
                if (response == null)
                {
                    return new GetContractResponse
                    (
                        statusCode: StatusCodes.Status400BadRequest,
                        message: $"Contract with business Id {businessId} not found",
                        payload: null
                    );
                }

                var item = _mapper.Map<GetContractPayload>(response);

                // Ensure contractType is returned as an integer
                item.ContractType = (int)response.ContractType;

                // Reset values to 0 before assignment
                item.SubscriptionAmount = 0;
                item.OfferAmount = 0;
                item.InvoicePercentage = 0;

                // Assign correct values based on contract type
                switch (response.ContractType)
                {
                    case ContractType.MonthlySubscription:
                        item.SubscriptionAmount = response.SubscriptionAmount ?? 0;
                        break;
                    case ContractType.FixedAmountPerOffer:
                        item.OfferAmount = response.FixedAmountPerOffer ?? 0;
                        break;
                    case ContractType.PercentageOfInvoice:
                        item.InvoicePercentage = response.PercentageOfInvoice ?? 0;
                        break;
                }

                return new GetContractResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Contract retrieved successfully",
                    payload: item
                );
            }
            catch (Exception ex)
            {
                return new GetContractResponse
                (
                    statusCode: StatusCodes.Status500InternalServerError,
                    message: "An unexpected error occurred. Please try again later.",
                    payload: null
                );
            }
        }

        public async Task<SuccessResponse> DeleteContract(int ContractId)
        {
            try
            {
                await _ContractRepository.DeleteAsync(ContractId, true);
                return new SuccessResponse
                (
                    statusCode: StatusCodes.Status200OK,
                    message: "Contract Deleted",
                    payload: new SuccessResponsePayload { success = true }
                );
            }
            catch (Exception ex)
            {
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
