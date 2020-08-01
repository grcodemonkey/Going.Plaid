﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using VerifyTests;
using VerifyXunit;
using Xunit;
using static VerifyXunit.Verifier;

namespace Going.Plaid.Tests
{
	[UsesVerify]
	public class PlaidClientTest
	{
		public PlaidClient PlaidClient { get; }

		public PlaidClientTest()
		{
			VerifierSettings.DisableClipboard();
			VerifierSettings.ScrubLinesContaining(StringComparison.OrdinalIgnoreCase, "request_id");

			var configuration = new ConfigurationBuilder()
				.AddEnvironmentVariables("PLAID_CONFIG_")
				.AddJsonFile("secrets.json", optional: true)
				.Build();

			if (string.IsNullOrWhiteSpace(configuration["Environment"]))
				throw new InvalidOperationException("Please provide Environment configuration via PLAID_CONFIG_ENVIRONMENT or secrets.json.");
			if (string.IsNullOrWhiteSpace(configuration["Client_Id"]))
				throw new InvalidOperationException("Please provide Client_Id configuration via PLAID_CONFIG_CLIENT_ID or secrets.json.");
			if (string.IsNullOrWhiteSpace(configuration["Secret"]))
				throw new InvalidOperationException("Please provide Secret configuration via PLAID_CONFIG_SECRET or secrets.json.");
			if (string.IsNullOrWhiteSpace(configuration["Access_Token"]))
				throw new InvalidOperationException("Please provide Access_Token configuration via PLAID_CONFIG_ACCESS_TOKEN or secrets.json.");

			PlaidClient = new PlaidClient(
				Enum.Parse<Environment>(configuration["Environment"]),
				configuration["Client_Id"],
				configuration["Secret"],
				configuration["Access_Token"]);
		}

		[Fact]
		public async Task FetchItemAsync()
		{
			var result = await PlaidClient.FetchItemAsync(
				new Management.GetItemRequest());
			await Verify(result);
		}

		/*[TestMethod]
		public void ExchangePublicTokenAsync_should_retrieve_a_response_from_the_api_server()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);

			// Act
			var request = new Management.ExchangeTokenRequest()
			{
				PublicToken = "public-sandbox-5c224a01-8314-4491-a06f-39e193d5cddc"
			}.UseDefaults();
			var result = sut.ExchangeTokenAsync(request).Result;

			// Assert
			result.Exception.ShouldNotBeNull();
			result.IsSuccessStatusCode.ShouldBeFalse();
		}

		[TestMethod]
		public void FetchCategoriesAsync_should_retrieve_the_api_category_list()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);
			var request = new Category.GetCategoriesRequest()
			{
			}.UseDefaults();

			// Act
			var result = sut.FetchCategoriesAsync(request).Result;

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Categories.Length.ShouldBeGreaterThan(0);
		}

		[TestMethod]
		public void FetchInstitutionsAsync_should_retrieve_a_list_of_banks_that_matches_a_specified_query()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);

			// Act
			var request = new Institution.SearchRequest()
			{
				Query = "citi",
			}.UseDefaults();

			var result = sut.FetchInstitutionsAsync(request).Result;

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Institutions.Length.ShouldBeGreaterThanOrEqualTo(1);
			result.Institutions.ShouldAllBe(i => i.Name.ToLower().Contains(request.Query.ToLower()));
		}

		[TestMethod]
		public void FetchInstitutionByIdAsync_should_retrieve_a_bank_that_matches_a_specified_id()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);

			// Act
			var request = new Institution.SearchByIdRequest()
			{
				InstitutionId = "ins_109511"
			}.UseDefaults();
			var response = sut.FetchInstitutionByIdAsync(request).Result;

			// Assert
			response.IsSuccessStatusCode.ShouldBeTrue();
			response.RequestId.ShouldNotBeNullOrEmpty();
			response.Institution.Id.ShouldBe(request.InstitutionId);
		}

		[TestMethod]
		public void FetchTransactionsAsync_should_retrieve_a_list_of_transactions()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);

			// Act
			var request = new Transactions.GetTransactionsRequest().UseDefaults();
			var result = sut.FetchTransactionsAsync(request).Result;

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.TransactionsReturned.ShouldBeGreaterThan(0);
			result.Transactions.Length.ShouldBeGreaterThan(0);
			result.Transactions[0].Amount.ShouldBeGreaterThan(0);
		}

		[TestMethod]
		public void FetchAccountBalanceAsync_should_retrieve_the_account_balances_of_an_user()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);

			// Act
			var request = new Balance.GetBalanceRequest().UseDefaults();
			var result = sut.FetchAccountBalanceAsync(request).Result;

			// Assert
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Accounts.Length.ShouldBeGreaterThanOrEqualTo(1);
			result.Accounts[0].Balance.Current.ShouldBeGreaterThanOrEqualTo(1);
		}

		[TestMethod]
		public void FetchAccountInfoAsync_should_retrieve_the_routing_numbers_of_an_user_accounts()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);
			var request = new Auth.GetAccountInfoRequest()
			{
			}.UseDefaults();

			// Act
			var result = sut.FetchAccountInfoAsync(request).Result;

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Accounts.Length.ShouldBeGreaterThan(0);
			(result.Numbers.ACH.Length + result.Numbers.EFT.Length + result.Numbers.International.Length + result.Numbers.BACS.Length).ShouldBeGreaterThan(0);
			result.Item.ShouldNotBeNull();
		}

		[TestMethod]
		public void FetchAccountInfoAsync_International_Bacs_should_retrieve_the_routing_numbers_of_an_user_accounts()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);
			var request = new Auth.GetAccountInfoRequest()
			{
			}.UseIntlDefaults();

			// Act
			var result = sut.FetchAccountInfoAsync(request).Result;

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Accounts.Length.ShouldBeGreaterThan(0);
			result.Numbers.International.Length.ShouldBeGreaterThan(0);
			result.Numbers.BACS.Length.ShouldBeGreaterThan(0);
			result.Item.ShouldNotBeNull();
		}

		[TestMethod]
		public void FetchUserIdentityAsync_should_retrieve_the_personal_info_of_an_user()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);
			var request = new Identity.GetUserIdentityRequest()
			{
			}.UseDefaults();

			// Act
			var result = sut.FetchUserIdentityAsync(request).Result;
			bool publicKeyDontHaveAccess = result.Exception?.ErrorCode == "INVALID_PRODUCT";
			if (publicKeyDontHaveAccess) Assert.Inconclusive(Helper.your_public_key_do_not_have_access_contact_plaid);

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Accounts.Length.ShouldBeGreaterThan(0);
			result.Item.ShouldNotBeNull();
		}

		[TestMethod]
		public void FetchIncomeAsync_should_retrieve_the_monthly_earnings_of_an_user()
		{
			// Arrange
			var sut = new PlaidClient(Environment.Sandbox);
			var request = new Income.GetIncomeRequest()
			{
			}.UseDefaults();

			// Act
			var result = sut.FetchUserIncomeAsync(request).Result;
			bool publicKeyDontHaveAccess = result.Exception?.ErrorCode == "INVALID_PRODUCT";
			if (publicKeyDontHaveAccess) Assert.Inconclusive(Helper.your_public_key_do_not_have_access_contact_plaid);

			// Assert
			result.IsSuccessStatusCode.ShouldBeTrue();
			result.RequestId.ShouldNotBeNullOrEmpty();
			result.Income.Streams.Length.ShouldBeGreaterThan(0);
			result.Income.LastYearIncome.ShouldBeGreaterThan(0);
		}*/
	}
}