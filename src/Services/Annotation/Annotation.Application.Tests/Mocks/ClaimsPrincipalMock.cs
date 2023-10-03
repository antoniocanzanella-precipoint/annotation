using PreciPoint.Ims.Core.Authorization.Domain;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Mocks;

public class ClaimsPrincipalMock : AClaimsPrincipal
{
    public ClaimsPrincipalMock(
        IPrincipal principal,
        string clientId,
        Guid userId,
        IReadOnlyDictionary<string, ClientRoles> rolesPerClient,
        IReadOnlyList<string> groups,
        string accessToken,
        IReadOnlyList<string> tenants,
        string activeTenant,
        bool isAnonymous)
        : base(principal,
            clientId,
            userId,
            rolesPerClient,
            groups,
            accessToken,
            tenants,
            activeTenant,
            isAnonymous) { }
}