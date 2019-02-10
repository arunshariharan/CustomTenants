using Moq;
using MultiTenant.Models;
using MultiTenant.Services;
using System.IdentityModel.Tokens.Jwt;
using System;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MultiTenant.Repositories;
using System.Security.Claims;
using System.Collections.Generic;

namespace MultiTenant.Api.Test.Services
{
    public class TenantSeviceShould
    {
        [Theory]
        [InlineData("1111", 1)]
        [InlineData("2222", 2)]
        [InlineData("3333", 3)]
        [InlineData("localhost:2222", 2)]
        [InlineData("192.168.99.100:1111", 1)]
        public void GetTheCorrectTenantIdForEachHost(string host, int expectedTenantId)
        {
            var tenantId = TenantService.GetCurrentTenantId(host);

            Assert.Equal(tenantId, expectedTenantId);
        }

        [Fact]
        public void ThrowExceptionWhenUnauthorizedTenantAccesses()
        {
            Action tenantId = () => TenantService.GetCurrentTenantId("unauthorized.host");

            Assert.Throws<Exception>(tenantId);
        }

        [Fact]
        public void SetCorrectTenantDetails()
        {
            const string VALID_HOST_PORT = "1111";
            const int EXPECTED_TENANT_ID = 1;
            const string EXPECTED_TENANT_NAME = "Tenant A";
            List<int> expectedIdsList =  new List<int>() { 1, 2, 3 };
            List<string> expectedHostsList = new List<string>() { "1111", "2222", "3333" };
            List<string> expectedNamesList = new List<string>() { "Tenant A", "Tenant B", "Tenant C" };

            TenantService.SetTenantDetails(VALID_HOST_PORT);

            Assert.Equal(TenantService.TenantId, EXPECTED_TENANT_ID);
            Assert.Equal(TenantService.TenantName, EXPECTED_TENANT_NAME);
            Assert.Equal(TenantService.TenantHost, VALID_HOST_PORT);
            Assert.Equal(TenantService.AllTenantHosts, expectedHostsList);
            Assert.Equal(TenantService.AllTenantIds, expectedIdsList);
            Assert.Equal(TenantService.AllTenantNames, expectedNamesList);
        }
    }
}
