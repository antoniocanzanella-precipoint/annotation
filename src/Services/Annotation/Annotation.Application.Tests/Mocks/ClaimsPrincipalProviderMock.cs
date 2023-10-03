using Microsoft.AspNetCore.Http;
using PreciPoint.Ims.Core.Authorization.Domain;
using PreciPoint.Ims.Core.Authorization.Providers;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace PreciPoint.Ims.Services.Annotation.Application.Tests.Mocks;

public class ClaimsPrincipalProviderMock : IClaimsPrincipalProvider
{
    private readonly IReadOnlyList<string> _tenants;

    public ClaimsPrincipalProviderMock()
    {
        _tenants = new List<string> { "/Tenants/Test" };
    }

    public AClaimsPrincipal ExtractClaimsPrincipal(ClaimsPrincipal claimsPrincipal,
        IHeaderDictionary headerDictionary,
        string accessToken)
    {
        throw new NotImplementedException();
    }

    public AClaimsPrincipal Current => GetCurrent();

    private ClaimsPrincipalMock GetCurrent()
    {
        var user = new ClaimsPrincipalMock(GetPrinciple(), "clientId",
            new Guid("c64aba85-b94f-4460-8ce6-2c18724f49d4"),
            new Dictionary<string, ClientRoles>(), new List<string>(), "accessToken", _tenants, _tenants[0], true);

        return user;
    }

    private ClaimsPrincipal GetPrinciple()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new(ClaimTypes.Name, "example name"), new Claim(ClaimTypes.NameIdentifier, "1"), new Claim("custom-claim", "example claim value") },
            "mock"));
    }
}