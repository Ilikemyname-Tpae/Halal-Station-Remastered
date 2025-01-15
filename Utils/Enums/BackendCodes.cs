namespace Halal_Station_Remastered.Utils.Enums
{
    public enum BackendCodes
    {
        // Basic Api Errors.
        Success = 0,
        TimeOut = 1,
        ParseError = 2,

        // TS??
        TS_ParentBranchNotFound = 100,
        TS_BranchIsLocked = 101,
        TS_BranchAlreadyExists = 102,
        TS_SetTileConfigError = 103,
        TS_SetTileConfigError_InheritedNotFound = 1032,
        TS_SetTileConfigError_UnknownType = 104,
        TS_UnlockBranch_InvalidUser = 105,
        TS_VersionNotFound = 106,
        TS_TR_ItemNotFound = 107,
        TS_TR_ItemTypeInvalid = 108,

        // Account creation Errors.
        FE_CreateUserError = 200,
        FE_DuplicateSlice = 201,
        FE_LockedSlice = 202,

        // User Errors.
        UserTokenCheckFailed = 3,
        US_UserIdNotFound = 300,
        US_UserGamertagNotFound = 301,
        US_UserGamertagAlreadyExists = 302,
        US_UserBranchNotFound = 303,
        US_UserBattleTagIsInvalid = 304,
        US_Loadouts_SlotIndexOutOfRange = 305,
        US_Loadouts_LoadoutNotFound = 306,

        // Backend Menu Errors.
        AS_UserBlocked = 400,
        AS_InvalidCredentials = 401,
        AS_EnvironmentNotFound = 402,
        AS_DefaultSliceDeploymentNotFound = 403,
        AS_UserServiceEndpointWasNotFoundInDefaultSlice = 404,
        AS_UserEnsuringFailed = 405,
        AS_TitleServersForUserGroupNotFound = 406,
        AS_SessionWasNotCreated = 407,
        AS_UserSetupDefaultsFailed = 408,

        // Client Server Errors.
        CS_US_Error = 500,
        CS_PS_Error = 601,
        CS_TS_Error = 602,

        // Auth Errors.
        DSAS_UserGroupNotFound = 650,
        DSAS_TitleSupportServersForUserGroupNotFound = 651,

        // Title Services Errors.
        TSS_ContextHeaderWasNotFound = 700,
        TSS_UserGroupContextDeserializationFailed = 701,
        TSS_ServiceNameWasNotResolved = 702,
        TSS_ServiceEndpointNotFound = 703,
        TSS_ContextVariableWasNotFound = 704,

        // Title Server Errors.
        TitleServer_ContextHeaderNotFound = 750,
        TitleServer_UserContextDeserializationFailed = 751,
        TitleServer_UserGroupContextRetrievingFailed = 752,

        // Client Data Protocol Errors.
        CDP_DataLengthHeaderWasNotFound = 800,
        CDP_DataLengthHeaderCanNotBeParsedAsInt32 = 801,
        CDP_ProcessingForVeryLongContentWasNotImplemented = 802,
        CDP_DumpDataIsAbsent = 803,

        // Dedicated Servers Errors.
        DS_OnlyDiagnosticsServiceFirstKey = 850,
        DS_AddressAndOperationShouldBeSpecified = 851,
        DS_OperationDoesNotSupported = 852
    }
}