namespace Halal_Station_Remastered.Utils.Enums
{
    public enum ClientCodes
    {
        // Basic Network Errors
        Success = 0,
        UnhandledError = 1,
        RichPresence_ServiceUnavailable = 101,

        // Network Errors
        CouldNotSendHttpRequest = 103,
        NetworkUnavailableCheckConnection = 104,
        CannotParseServersResponse = 105,
        NetworkUnavailableCheckConnectionDuplicate = 107,

        // Under Maintenance Errors
        OnlineServicesUnderMaintenance = 200,
        OnlineServicesUnderMaintenanceDuplicate = 201,

        // Party Errors
        OnlyPartyLeaderCanStartGame = 1312,
        OnlyPartyLeaderCanKickMembers = 1352,
        PartySizeShouldBeReducedToStartPlaylist = 1341,

        // Dedicated Server Errors
        CouldNotConnectToGameServerGameSessionFinished = -1,
        CantConnectToDedicatedServer = 1360,
        CantConnectToDedicatedServerDuplicate = 1361,

        // Service Errors
        UserServiceUnavalible = 100,
        TitleResourceServiceFailed = 102,

        // Clan Errors
        ClanNotFound = 331,
        ClanNameTooShort = 332,
        ClanNameTooLong = 333,
        ClanNameAlreadyUsed = 334,
        UserAlreadyInClan = 335,
        UserNotInCland = 336,
        CannotLeaveClanOnlyMember = 337,
        ClanNameCantBeEmpty = 338,
        SpecialSymbolsNotAllowedInClanName = 339,
        ClanCreationNotAvailable = 341,

        // Ban Errors
        AccountBanned = 325,
        BannedForCheating = 326,
        BannedForInappropriateBehavior = 327,
        AccountMarkedAsStolen = 328,
        BannedForViolatingUserAgreement = 329,

        // User Errors
        NewNicknameMustBeUnique = 302,
        NicknameContainsInappropriateWords = 330,
        ItemAlreadyPurchased = 316,
        NotEnoughtGold = 317,
        NotEnoughtCredits = 318,
        PlayerSessionExpired = 308,
        NicknameTooShort = 309,
        NicknameTooLong = 310,
        NicknameShouldStartWithLetter = 311,
        SpecialSymbolsNotAllowedInNickname = 312,
        NicknameChangeNotAvailable = 321,
        NicknameCantBeEmpty = 322,
        InvalidLoginOrPassword = 401,
        ErrorMutingUser = 1101,
        NotEnoughResearchPoints = 343,

        // Offer Errors
        RequirementsToExecuteOfferNotMet = 340,

        // Unknown Backend Errors
        InternalError106 = 106,
        InternalError108 = 108,
        InternalError109 = 109,
        InternalError110 = 110,
        InternalError315 = 315,
        InternalError342 = 342,
        InternalError344 = 344,
        InternalError345 = 345,
        InternalError400 = 400,
        InternalError1311 = 1311,
        InternalError1351 = 1351,

        // Unknown Party Errors
        PartyNotAvailableForJoin1302 = 1302,
        PartyNotAvailableForJoin1303 = 1303,
        PartyNotAvailableForJoin1304 = 1304,
        PartyNotAvailableForJoin1305 = 1305,
        PartyNotAvailableForJoin1306 = 1306
    }
}